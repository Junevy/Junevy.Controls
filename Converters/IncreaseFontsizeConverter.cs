using System.Globalization;
using System.Windows.Data;

namespace Junevy.Controls.Converters
{
    public class IncreaseFontsizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return 20d;
            }

            try
            {
                double increase = 5d;
                if (parameter is string text && double.TryParse(text, NumberStyles.Float, culture, out double parsed))
                {
                    increase = parsed;
                }

                return System.Convert.ToDouble(value, culture) + increase;
            }
            catch
            {
                return 20d;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
