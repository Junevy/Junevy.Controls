using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Junevy.Controls.Controls.Image;

[TemplatePart(Name = PART_Image, Type = typeof(System.Windows.Controls.Image))]
public class ImageViewer : Control
{
    private const string PART_Image = "PART_Image";
    private const double MinZoom = 0.05;
    private const double MaxZoom = 64;

    private readonly MatrixTransform transform = new();
    private System.Windows.Controls.Image? imageElement;
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
            typeof(ImageViewer));

    public ImageSource? BackgroundImage
    {
        get => (ImageSource?)GetValue(BackgroundImageProperty);
        set => SetValue(BackgroundImageProperty, value);
    }

    public static readonly DependencyProperty BackgroundImageProperty =
        DependencyProperty.Register(nameof(BackgroundImage), typeof(ImageSource), typeof(ImageViewer));

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        imageElement = GetTemplateChild(PART_Image) as System.Windows.Controls.Image;
        if (imageElement != null)
        {
            imageElement.RenderTransform = transform;
            imageElement.RenderTransformOrigin = new Point(0, 0);
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (imageElement?.Source == null)
        {
            return;
        }

        double scale = e.Delta > 0 ? 1.2 : 0.8;
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

    public void FitToWindow()
    {
        if (Source is not BitmapSource bitmap || imageElement == null)
        {
            return;
        }

        if (ActualWidth <= 0 || ActualHeight <= 0 || bitmap.PixelWidth <= 0 || bitmap.PixelHeight <= 0)
        {
            return;
        }

        double scale = Math.Min(ActualWidth / bitmap.PixelWidth, ActualHeight / bitmap.PixelHeight);
        double imageWidth = bitmap.PixelWidth * scale;
        double imageHeight = bitmap.PixelHeight * scale;
        double offsetX = (ActualWidth - imageWidth) / 2;
        double offsetY = (ActualHeight - imageHeight) / 2;

        Matrix matrix = Matrix.Identity;
        matrix.Scale(scale, scale);
        matrix.Translate(offsetX, offsetY);
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
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using FileStream stream = File.Create(dialog.FileName);
        encoder.Save(stream);
    }
}
