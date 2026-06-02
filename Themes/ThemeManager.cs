using System.Windows;

namespace Junevy.Controls.Themes;

public static class ThemeManager
{
    private static readonly Uri LightThemeUri = CreateUri("AppColors.Light.xaml");
    private static readonly Uri DarkThemeUri = CreateUri("AppColors.Dark.xaml");
    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public static void ApplyTheme(AppTheme theme)
    {
        if (Application.Current == null)
        {
            return;
        }

        ApplyTheme(Application.Current.Resources, theme);
    }

    public static void ApplyTheme(ResourceDictionary resources, AppTheme theme)
    {
        //ArgumentNullException.ThrowIfNull(resources);
        if (resources == null)
            throw new ArgumentNullException(nameof(resources));

        Uri targetUri = GetThemeUri(theme);
        ResourceDictionary targetDictionary = new() { Source = targetUri };
        var dictionaries = resources.MergedDictionaries;

        for (int i = 0; i < dictionaries.Count; i++)
        {
            ResourceDictionary dictionary = dictionaries[i];
            Uri? source = dictionary.Source;
            if (source == null)
            {
                continue;
            }

            if (!IsThemeDictionary(source))
            {
                continue;
            }

            dictionaries[i] = targetDictionary;
            CurrentTheme = theme;
            return;
        }

        dictionaries.Insert(0, targetDictionary);
        CurrentTheme = theme;
    }

    public static void ToggleTheme()
    {
        ApplyTheme(CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
    }

    public static ResourceDictionary CreateThemeDictionary(AppTheme theme)
    {
        return new ResourceDictionary
        {
            Source = GetThemeUri(theme)
        };
    }

    private static bool IsThemeDictionary(Uri source)
    {
        string path = source.OriginalString;
        return path.EndsWith("Themes/AppColors.xaml", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith("Themes/AppColors.Light.xaml", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith("Themes/AppColors.Dark.xaml", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith("Themes\\AppColors.xaml", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith("Themes\\AppColors.Light.xaml", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith("Themes\\AppColors.Dark.xaml", StringComparison.OrdinalIgnoreCase);
    }

    private static Uri GetThemeUri(AppTheme theme)
    {
        return theme switch
        {
            AppTheme.Dark => DarkThemeUri,
            _ => LightThemeUri
        };
    }

    private static Uri CreateUri(string fileName)
    {
        return new Uri($"/Junevy.Controls;component/Themes/{fileName}", UriKind.Relative);
    }
}
