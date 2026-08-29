using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Junevy.Controls.ToolboxDemo;

public partial class MainWindow : Window
{
    private const string ToolDataFormat = "Junevy.Controls.Tool";

    public static RoutedUICommand PlaceToolCommand { get; } = new(
        "Place tool",
        nameof(PlaceToolCommand),
        typeof(MainWindow));

    public ObservableCollection<ToolGroup> ToolGroups { get; } =
    [
        new(
            "Vision tools",
            "\uE66B",
            [
                new("Camera", "\uE66B", "camera"),
                new("Image source", "\uE63E", "image-source"),
                new("Pattern match", "\uE65D", "pattern-match"),
                new("Edge detect", "\uE62E", "edge-detect"),
                new("Blob analysis", "\uE63A", "blob-analysis"),
                new("Color inspect", "\uE61D", "color-inspect"),
                new("Barcode", "\uE611", "barcode"),
                new("OCR", "\uE64A", "ocr"),
                new("Calibration", "\uE60F", "calibration"),
                new("Very long perspective correction tool title", "\uE641", "perspective"),
                new("Threshold", "\uE61A", "threshold"),
                new("Morphology", "\uE65A", "morphology"),
                new("Histogram", "\uE635", "histogram"),
                new("Result output", "\uE60B", "result-output")
            ]),
        new(
            "Measurement tools",
            "\uE60F",
            [
                new("Distance", "\uE62E", "distance"),
                new("Angle", "\uE641", "angle"),
                new("Circle gauge", "\uE65D", "circle-gauge"),
                new("Line gauge", "\uE61A", "line-gauge"),
                new("Count", "\uE635", "count"),
                new("Coordinate", "\uE60B", "coordinate")
            ])
    ];

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    internal static bool TryGetToolDefinition(IDataObject data, out ToolDefinition? tool)
    {
        tool = data.GetDataPresent(ToolDataFormat)
            ? data.GetData(ToolDataFormat) as ToolDefinition
            : null;
        return tool is not null;
    }

    private void OnPlaceToolCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = e.Parameter is ToolDefinition;
        e.Handled = true;
    }

    private void OnPlaceToolExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is ToolDefinition tool)
        {
            StatusText.Text = $"Executed {tool.Title} ({tool.Kind})";
        }
    }

    private void OnCanvasDragOver(object sender, DragEventArgs e)
    {
        e.Effects = TryGetToolDefinition(e.Data, out _)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnCanvasDrop(object sender, DragEventArgs e)
    {
        if (sender is not Canvas canvas || !TryGetToolDefinition(e.Data, out ToolDefinition? tool))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        Point position = e.GetPosition(canvas);
        Border node = CreateToolNode(tool!);
        canvas.Children.Add(node);
        Canvas.SetLeft(node, Math.Max(0, position.X - 12));
        Canvas.SetTop(node, Math.Max(0, position.Y - 12));

        StatusText.Text = $"Dropped {tool!.Title} ({tool.Kind})";
        e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }

    private static Border CreateToolNode(ToolDefinition tool)
    {
        var label = new TextBlock
        {
            Text = tool.Title,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 180
        };
        label.SetResourceReference(TextBlock.ForegroundProperty, "Theme.Brush.Text.Primary");

        var node = new Border
        {
            Padding = new Thickness(12, 8, 12, 8),
            BorderThickness = new Thickness(1),
            Child = label,
            ToolTip = $"{tool.Title} ({tool.Kind})"
        };
        node.SetResourceReference(Border.BackgroundProperty, "Theme.Brush.Surface.Raised");
        node.SetResourceReference(Border.BorderBrushProperty, "Theme.Brush.Accent.Primary");
        node.SetResourceReference(Border.CornerRadiusProperty, "Theme.SmallCornerRadius");
        return node;
    }
}

public sealed record ToolGroup(
    string Title,
    string Icon,
    ObservableCollection<ToolDefinition> Tools);
