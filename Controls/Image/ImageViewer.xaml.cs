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

    private System.Windows.Controls.Image? _image;

    private MatrixTransform _transform = new();

    private Point _lastPoint;

    private bool _isPanning;

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

    #region Source

    public BitmapSource? Source
    {
        get => (BitmapSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(
            nameof(Source),
            typeof(BitmapSource),
            typeof(ImageViewer));

    #endregion




    public ImageSource BackgroundImage
    {
        get { return (ImageSource)GetValue(BackgroundImageProperty); }
        set { SetValue(BackgroundImageProperty, value); }
    }
    public static readonly DependencyProperty BackgroundImageProperty =
        DependencyProperty.Register("BackgroundImage", typeof(ImageSource), typeof(ImageViewer));




    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _image = GetTemplateChild(PART_Image)
            as System.Windows.Controls.Image;

        if (_image != null)
        {
            _image.RenderTransform = _transform;
            _image.RenderTransformOrigin = new Point(0, 0);
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (_image == null)
            return;

        double scale = e.Delta > 0 ? 1.2 : 0.8;

        Point p = e.GetPosition(_image);

        Matrix matrix = _transform.Matrix;

        matrix.ScaleAt(scale, scale, p.X, p.Y);

        _transform.Matrix = matrix;

        e.Handled = true;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        _isPanning = true;

        _lastPoint = e.GetPosition(this);

        CaptureMouse();

        Cursor = Cursors.Hand;

        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        _isPanning = false;

        ReleaseMouseCapture();

        Cursor = Cursors.Arrow;

        base.OnMouseLeftButtonUp(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!_isPanning)
            return;

        Point current = e.GetPosition(this);

        Vector delta = current - _lastPoint;

        Matrix matrix = _transform.Matrix;

        matrix.Translate(delta.X, delta.Y);

        _transform.Matrix = matrix;

        _lastPoint = current;

        base.OnMouseMove(e);
    }

    public void ActualSize()
    {
        _transform.Matrix = Matrix.Identity;
    }

    public void FitToWindow()
    {
        if (Source == null)
            return;

        if (_image == null)
            return;

        double scaleX =
            ActualWidth / Source.PixelWidth;

        double scaleY =
            ActualHeight / Source.PixelHeight;

        double scale =
            Math.Min(scaleX, scaleY);

        double imageWidth =
            Source.PixelWidth * scale;

        double imageHeight =
            Source.PixelHeight * scale;

        double offsetX =
            (ActualWidth - imageWidth) / 2;

        double offsetY =
            (ActualHeight - imageHeight) / 2;

        Matrix matrix = Matrix.Identity;

        matrix.Scale(scale, scale);

        matrix.Translate(
            offsetX,
            offsetY);

        _transform.Matrix = matrix;
    }

    private ContextMenu CreateContextMenu()
    {
        ContextMenu menu = new();

        MenuItem fit = new()
        {
            Header = "适应窗口"
        };

        fit.Click += (_, _) => FitToWindow();

        MenuItem actual = new()
        {
            Header = "100%"
        };

        actual.Click += (_, _) => ActualSize();

        MenuItem savePng = new()
        {
            Header = "保存PNG"
        };

        savePng.Click += (_, _) => SaveImage(false);

        MenuItem saveBmp = new()
        {
            Header = "保存BMP"
        };

        saveBmp.Click += (_, _) => SaveImage(true);

        menu.Items.Add(fit);
        menu.Items.Add(actual);
        menu.Items.Add(new Separator());
        menu.Items.Add(savePng);
        menu.Items.Add(saveBmp);

        return menu;
    }

    private void SaveImage(bool jpg)
    {
        if (Source == null)
            return;

        SaveFileDialog dialog = new();

        dialog.Filter =
            jpg
                ? "BMP (*.bmp)|*.bmp"
                : "PNG (*.png)|*.png";

        if (dialog.ShowDialog() != true)
            return;

        BitmapEncoder encoder =
            jpg
                ? new BmpBitmapEncoder()
                : new PngBitmapEncoder();

        encoder.Frames.Add(
            BitmapFrame.Create(Source));

        using FileStream fs =
            File.Create(dialog.FileName);

        encoder.Save(fs);
    }
}