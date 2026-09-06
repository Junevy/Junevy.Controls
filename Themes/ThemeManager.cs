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
        if (resources == null)
            throw new ArgumentNullException(nameof(resources));

        Uri targetUri = GetThemeUri(theme);
        RemoveThemeDictionaries(resources);

        // Append at the TOP level of the merged dictionary chain: top-level changes
        // invalidate live DynamicResource references, while replacing dictionaries
        // nested inside Generic.xaml would silently leave existing elements on the
        // old theme. Appending also gives the theme dictionary the highest lookup
        // priority, shadowing any theme dictionary still nested elsewhere.
        resources.MergedDictionaries.Add(CreateDictionary(targetUri));
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

    private static void RemoveThemeDictionaries(ResourceDictionary resources)
    {
        var dictionaries = resources.MergedDictionaries;

        for (int i = dictionaries.Count - 1; i >= 0; i--)
        {
            ResourceDictionary dictionary = dictionaries[i];
            Uri? source = dictionary.Source;

            if (source != null && IsThemeDictionary(source))
            {
                dictionaries.RemoveAt(i);
                continue;
            }

            RemoveThemeDictionaries(dictionary);
        }
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

    private static ResourceDictionary CreateDictionary(Uri source)
    {
        return new ResourceDictionary { Source = source };
    }
}
