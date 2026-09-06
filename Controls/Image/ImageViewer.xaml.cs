using Microsoft.Win32;
using Junevy.Controls.Common;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Junevy.Controls.Controls.Image;

[TemplatePart(Name = PART_Image, Type = typeof(System.Windows.Controls.Image))]
public class ImageViewer : Control
{
    private const string PART_Image = "PART_Image";
    private const double MinZoom = 0.05;
    private const double MaxZoom = 64;
    private const double ZoomStep = 1.2;

    private readonly MatrixTransform transform = new();
    private System.Windows.Controls.Image? imageElement;
    private Matrix fitMatrix = Matrix.Identity;
    private Point lastPoint;
    private bool isPanning;

    static ImageViewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ImageViewer),
            new FrameworkPropertyMetadata(typeof(ImageViewer)));
    }

    public ImageViewer()
    {
        SetCurrentValue(ZoomInCommandProperty, new RelayCommand(ZoomIn, () => Source is BitmapSource));
        SetCurrentValue(ZoomOutCommandProperty, new RelayCommand(ZoomOut, () => Source is BitmapSource));
        SetCurrentValue(SaveCommandProperty, new RelayCommand(SaveImage, () => Source is BitmapSource));

        transform.Changed += (_, _) => SetCurrentValue(ZoomProperty, transform.Matrix.M11);
        SizeChanged += OnSizeChanged;

        ContextMenu = CreateContextMenu();
    }

    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(
            nameof(Source),
            typeof(ImageSource),
            typeof(ImageViewer),
            new PropertyMetadata(OnSourceChanged));

    /// <summary>Toolbar command that zooms in. Bind to replace with a custom command.</summary>
    public ICommand? ZoomInCommand
    {
        get => (ICommand?)GetValue(ZoomInCommandProperty);
        set => SetValue(ZoomInCommandProperty, value);
    }

    public static readonly DependencyProperty ZoomInCommandProperty =
        DependencyProperty.Register(nameof(ZoomInCommand), typeof(ICommand), typeof(ImageViewer));

    /// <summary>Toolbar command that zooms out. Bind to replace with a custom command.</summary>
    public ICommand? ZoomOutCommand
    {
        get => (ICommand?)GetValue(ZoomOutCommandProperty);
        set => SetValue(ZoomOutCommandProperty, value);
    }

    public static readonly DependencyProperty ZoomOutCommandProperty =
        DependencyProperty.Register(nameof(ZoomOutCommand), typeof(ICommand), typeof(ImageViewer));

    /// <summary>Toolbar command that saves the image. Bind to replace with a custom command.</summary>
    public ICommand? SaveCommand
    {
        get => (ICommand?)GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    public static readonly DependencyProperty SaveCommandProperty =
        DependencyProperty.Register(nameof(SaveCommand), typeof(ICommand), typeof(ImageViewer));

    /// <summary>Current zoom factor relative to the image's pixel size (1 = 100%).</summary>
    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(ImageViewer), new PropertyMetadata(1.0));

    /// <summary>Frame rate shown in the info bar; bind to the camera's live value (NaN shows "--").</summary>
    public double FrameRate
    {
        get => (double)GetValue(FrameRateProperty);
        set => SetValue(FrameRateProperty, value);
    }

    public static readonly DependencyProperty FrameRateProperty =
        DependencyProperty.Register(nameof(FrameRate), typeof(double), typeof(ImageViewer), new PropertyMetadata(double.NaN));

    /// <summary>Image pixel width; filled automatically from <see cref="Source"/> unless bound.</summary>
    public double ImageWidth
    {
        get => (double)GetValue(ImageWidthProperty);
        set => SetValue(ImageWidthProperty, value);
    }

    public static readonly DependencyProperty ImageWidthProperty =
        DependencyProperty.Register(nameof(ImageWidth), typeof(double), typeof(ImageViewer), new PropertyMetadata(double.NaN));

    /// <summary>Image pixel height; filled automatically from <see cref="Source"/> unless bound.</summary>
    public double ImageHeight
    {
        get => (double)GetValue(ImageHeightProperty);
        set => SetValue(ImageHeightProperty, value);
    }

    public static readonly DependencyProperty ImageHeightProperty =
        DependencyProperty.Register(nameof(ImageHeight), typeof(double), typeof(ImageViewer), new PropertyMetadata(double.NaN));

    /// <summary>Horizontal resolution (DPI); filled automatically from <see cref="Source"/> unless bound.</summary>
    public double ResolutionX
    {
        get => (double)GetValue(ResolutionXProperty);
        set => SetValue(ResolutionXProperty, value);
    }

    public static readonly DependencyProperty ResolutionXProperty =
        DependencyProperty.Register(nameof(ResolutionX), typeof(double), typeof(ImageViewer), new PropertyMetadata(double.NaN));

    /// <summary>Vertical resolution (DPI); filled automatically from <see cref="Source"/> unless bound.</summary>
    public double ResolutionY
    {
        get => (double)GetValue(ResolutionYProperty);
        set => SetValue(ResolutionYProperty, value);
    }

    public static readonly DependencyProperty ResolutionYProperty =
        DependencyProperty.Register(nameof(ResolutionY), typeof(double), typeof(ImageViewer), new PropertyMetadata(double.NaN));

    /// <summary>Pixel format name; filled automatically from <see cref="Source"/> unless bound.</summary>
    public string? ImageFormat
    {
        get => (string?)GetValue(ImageFormatProperty);
        set => SetValue(ImageFormatProperty, value);
    }

    public static readonly DependencyProperty ImageFormatProperty =
        DependencyProperty.Register(nameof(ImageFormat), typeof(string), typeof(ImageViewer), new PropertyMetadata(null));

    /// <summary>Geometry-drawn checkerboard brush; theme-aware by default.</summary>
    public Brush? CheckerboardBrush
    {
        get => (Brush?)GetValue(CheckerboardBrushProperty);
        set => SetValue(CheckerboardBrushProperty, value);
    }

    public static readonly DependencyProperty CheckerboardBrushProperty =
        DependencyProperty.Register(nameof(CheckerboardBrush), typeof(Brush), typeof(ImageViewer));

    public bool ShowToolBar
    {
        get => (bool)GetValue(ShowToolBarProperty);
        set => SetValue(ShowToolBarProperty, value);
    }

    public static readonly DependencyProperty ShowToolBarProperty =
        DependencyProperty.Register(nameof(ShowToolBar), typeof(bool), typeof(ImageViewer), new PropertyMetadata(true));

    public bool ShowInfoBar
    {
        get => (bool)GetValue(ShowInfoBarProperty);
        set => SetValue(ShowInfoBarProperty, value);
    }

    public static readonly DependencyProperty ShowInfoBarProperty =
        DependencyProperty.Register(nameof(ShowInfoBar), typeof(bool), typeof(ImageViewer), new PropertyMetadata(true));

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        imageElement = GetTemplateChild(PART_Image) as System.Windows.Controls.Image;
        if (imageElement != null)
        {
            imageElement.RenderTransform = transform;
            imageElement.RenderTransformOrigin = new Point(0, 0);

            // The source may have been set before the template existed; fit now if
            // the view is still in its default (unadjusted) state.
            if (Source is BitmapSource && transform.Matrix == fitMatrix)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, FitToWindow);
            }
        }
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ImageViewer viewer = (ImageViewer)d;

        // SetCurrentValue keeps user bindings alive: a bound value wins over the auto-filled one.
        if (e.NewValue is BitmapSource bitmap)
        {
            viewer.SetCurrentValue(ImageWidthProperty, (double)bitmap.PixelWidth);
            viewer.SetCurrentValue(ImageHeightProperty, (double)bitmap.PixelHeight);
            viewer.SetCurrentValue(ResolutionXProperty, bitmap.DpiX);
            viewer.SetCurrentValue(ResolutionYProperty, bitmap.DpiY);
            viewer.SetCurrentValue(ImageFormatProperty, bitmap.Format.ToString());

            // Refit only while the view is at its previous fit, so a camera stream
            // never resets the user's zoom, but a resolution change does.
            if (viewer.transform.Matrix == viewer.fitMatrix)
            {
                viewer.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, viewer.FitToWindow);
            }
        }
        else
        {
            viewer.SetCurrentValue(ImageWidthProperty, double.NaN);
            viewer.SetCurrentValue(ImageHeightProperty, double.NaN);
            viewer.SetCurrentValue(ResolutionXProperty, double.NaN);
            viewer.SetCurrentValue(ResolutionYProperty, double.NaN);
            viewer.SetCurrentValue(ImageFormatProperty, null);

            viewer.transform.Matrix = Matrix.Identity;
            viewer.fitMatrix = Matrix.Identity;
        }

        CommandManager.InvalidateRequerySuggested();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize != e.PreviousSize && Source is BitmapSource && transform.Matrix == fitMatrix)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, FitToWindow);
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (imageElement?.Source == null)
        {
            return;
        }

        double scale = e.Delta > 0 ? ZoomStep : 1 / ZoomStep;
        Point point = e.GetPosition(imageElement);
        Matrix matrix = transform.Matrix;
        double nextScale = matrix.M11 * scale;

        if (nextScale < MinZoom || nextScale > MaxZoom)
        {
            e.Handled = true;
            return;
        }

        matrix.ScaleAt(scale, scale, point.X, point.Y);
        transform.Matrix = matrix;
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (imageElement?.Source == null)
        {
            return;
        }

        Focus();
        isPanning = true;
        lastPoint = e.GetPosition(this);
        CaptureMouse();
        Cursor = Cursors.Hand;

        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        EndPan();
        base.OnMouseLeftButtonUp(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!isPanning)
        {
            return;
        }

        Point current = e.GetPosition(this);
        Vector delta = current - lastPoint;
        Matrix matrix = transform.Matrix;
        matrix.Translate(delta.X, delta.Y);
        transform.Matrix = matrix;
        lastPoint = current;

        base.OnMouseMove(e);
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        EndPan();
        base.OnLostMouseCapture(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        if (isPanning && e.LeftButton != MouseButtonState.Pressed)
        {
            EndPan();
        }

        base.OnMouseLeave(e);
    }

    public void ActualSize()
    {
        transform.Matrix = Matrix.Identity;
    }

    /// <summary>
    ///     Scales and centers the image so it fills the viewport. Zoom becomes
    ///     relative to the image's pixel size (identity matrix = 100%).
    /// </summary>
    public void FitToWindow()
    {
        if (Source is not BitmapSource bitmap || imageElement == null)
        {
            return;
        }

        double naturalWidth = bitmap.Width;
        double naturalHeight = bitmap.Height;
        if (ActualWidth <= 0 || ActualHeight <= 0 || naturalWidth <= 0 || naturalHeight <= 0)
        {
            return;
        }

        double scale = Math.Min(ActualWidth / naturalWidth, ActualHeight / naturalHeight);
        double elementWidth = imageElement.ActualWidth > 0 ? imageElement.ActualWidth : naturalWidth;
        double elementHeight = imageElement.ActualHeight > 0 ? imageElement.ActualHeight : naturalHeight;

        Matrix matrix = new(scale, 0, 0, scale, (elementWidth - naturalWidth * scale) / 2, (elementHeight - naturalHeight * scale) / 2);
        transform.Matrix = matrix;
        fitMatrix = matrix;
    }

    public void ZoomIn()
    {
        ZoomAtCenter(ZoomStep);
    }

    public void ZoomOut()
    {
        ZoomAtCenter(1 / ZoomStep);
    }

    private void ZoomAtCenter(double scale)
    {
        if (imageElement?.Source == null)
        {
            return;
        }

        double nextScale = transform.Matrix.M11 * scale;
        if (nextScale < MinZoom || nextScale > MaxZoom)
        {
            return;
        }

        Point center = imageElement.ActualWidth > 0 && imageElement.ActualHeight > 0
            ? new Point(imageElement.ActualWidth / 2, imageElement.ActualHeight / 2)
            : new Point(ActualWidth / 2, ActualHeight / 2);

        Matrix matrix = transform.Matrix;
        matrix.ScaleAt(scale, scale, center.X, center.Y);
        transform.Matrix = matrix;
    }

    private void EndPan()
    {
        if (!isPanning)
        {
            return;
        }

        isPanning = false;

        if (IsMouseCaptured)
        {
            ReleaseMouseCapture();
        }

        Cursor = Cursors.Arrow;
    }

    private ContextMenu CreateContextMenu()
    {
        ContextMenu menu = new();

        MenuItem fit = new() { Header = "Fit to Window" };
        fit.Click += (_, _) => FitToWindow();

        MenuItem actual = new() { Header = "Actual Size" };
        actual.Click += (_, _) => ActualSize();

        MenuItem savePng = new() { Header = "Save as PNG" };
        savePng.Click += (_, _) => SaveImage(false);

        MenuItem saveBmp = new() { Header = "Save as BMP" };
        saveBmp.Click += (_, _) => SaveImage(true);

        menu.Items.Add(fit);
        menu.Items.Add(actual);
        menu.Items.Add(new Separator());
        menu.Items.Add(savePng);
        menu.Items.Add(saveBmp);

        return menu;
    }

    /// <summary>Toolbar save: lets the user pick PNG or BMP in one dialog.</summary>
    private void SaveImage()
    {
        if (Source is not BitmapSource bitmap)
        {
            return;
        }

        SaveFileDialog dialog = new()
        {
            Filter = "PNG (*.png)|*.png|BMP (*.bmp)|*.bmp",
            FilterIndex = 1
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        BitmapEncoder encoder = dialog.FilterIndex == 2 ? new BmpBitmapEncoder() : new PngBitmapEncoder();
        WriteBitmap(encoder, bitmap, dialog.FileName);
    }

    private void SaveImage(bool saveBmp)
    {
        if (Source is not BitmapSource bitmap)
        {
            return;
        }

        SaveFileDialog dialog = new()
        {
            Filter = saveBmp ? "BMP (*.bmp)|*.bmp" : "PNG (*.png)|*.png"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        BitmapEncoder encoder = saveBmp ? new BmpBitmapEncoder() : new PngBitmapEncoder();
        WriteBitmap(encoder, bitmap, dialog.FileName);
    }

    private static void WriteBitmap(BitmapEncoder encoder, BitmapSource bitmap, string fileName)
    {
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using FileStream stream = File.Create(fileName);
        encoder.Save(stream);
    }
}
