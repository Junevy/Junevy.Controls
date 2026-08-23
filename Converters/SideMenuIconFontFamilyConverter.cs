using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Junevy.Controls.Controls.Menu;

namespace Junevy.Controls.Converters
{
    /// <summary>
    /// Uses a SideMenu's explicit FontFamily for glyph icons while preserving
    /// its attached icon-font fallback for existing menus.
    /// </summary>
    public sealed class SideMenuIconFontFamilyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3 || values[0] is not SideMenu sideMenu)
            {
                return DependencyProperty.UnsetValue;
            }

            if (values[1] is FontFamily fontFamily && HasExplicitFontFamily(sideMenu))
            {
                return fontFamily;
            }

            return values[2] as FontFamily ?? DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static bool HasExplicitFontFamily(Control control)
        {
            BaseValueSource source = DependencyPropertyHelper
                .GetValueSource(control, Control.FontFamilyProperty)
                .BaseValueSource;

            return source == BaseValueSource.Local ||
                   source == BaseValueSource.Style ||
                   source == BaseValueSource.DefaultStyle;
        }
    }
}
