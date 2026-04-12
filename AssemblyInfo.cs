using System.Windows;
using System.Windows.Markup;

[assembly: XmlnsDefinition("github.com.junevy", "Junevy.Controls.Controls.Menu")]
[assembly: XmlnsDefinition("github.com.junevy", "Junevy.Controls.Controls.Bar")]
[assembly: XmlnsDefinition("github.com.junevy", "Junevy.Controls.Controls.Button")]
[assembly: XmlnsDefinition("github.com.junevy", "Junevy.Controls.Controls.Text")]

// 在文件末尾（其他 [assembly: ] 之后）添加：
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,           // 主题特定资源字典的位置（通常用 None）
    ResourceDictionaryLocation.SourceAssembly  // 泛型资源字典（Generic.xaml）的位置
)]