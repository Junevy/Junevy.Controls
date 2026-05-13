using System.Globalization;
using System.Windows.Data;

namespace Junevy.Controls.Converters
{
    public class IncreaseFontsizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var fontSize = (int)value;

                fontSize += 5;
                return fontSize;
            }
            catch
            {
                return 20;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
