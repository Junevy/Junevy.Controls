using System.Globalization;
using System.Windows.Data;

namespace Junevy.Controls.Converters;

/// <summary>
///     Formats ImageViewer info-bar values: NaN/empty shows "--",
///     otherwise formats the number with the ConverterParameter format
///     (e.g. "0", "0.#"); strings pass through.
/// </summary>
public sealed class ImageInfoValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        switch (value)
        {
            case null:
            case string text when text.Length == 0:
                return "--";

            case double number when double.IsNaN(number) || double.IsInfinity(number):
                return "--";

            case double number:
                return number.ToString(parameter as string ?? "0.##", culture);

            default:
                return value.ToString() ?? "--";
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
