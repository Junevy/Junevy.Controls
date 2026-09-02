using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NUnit.Framework;
using JunevyComboBox = Junevy.Controls.Controls.Box.ComboBox;
using JunevyTextBox = Junevy.Controls.Controls.Text.TextBox;
using TestHost = Junevy.Controls.Tests.Toolbox.WpfTestHost;

namespace Junevy.Controls.Tests.Controls;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class ComboBoxAndTextBoxTests
{
    [Test]
    public void DisplayMemberPath_IsUsedByDropDownAndSelectionTemplates()
    {
        var comboBox = new JunevyComboBox
        {
            Width = 180,
            ItemsSource = new[]
            {
                new DisplayItem("First"),
                new DisplayItem("Second")
            },
            DisplayMemberPath = nameof(DisplayItem.Name),
            SelectedIndex = 0
        };
        ApplyTheme(comboBox);

        Window window = TestHost.Show(comboBox);
        try
        {
            comboBox.ApplyTemplate();
            comboBox.UpdateLayout();
            comboBox.IsDropDownOpen = true;
            TestHost.Drain(comboBox.Dispatcher);

            var item = (System.Windows.Controls.ComboBoxItem)comboBox.ItemContainerGenerator.ContainerFromIndex(0);
            string[] textValues = FindVisualChildren<TextBlock>(comboBox)
                .Select(textBlock => textBlock.Text)
                .Where(text => !string.IsNullOrEmpty(text))
                .ToArray();
            string[] itemTextValues = FindVisualChildren<TextBlock>(item)
                .Select(textBlock => textBlock.Text)
                .Where(text => !string.IsNullOrEmpty(text))
                .ToArray();

            var placeholder = (TextBlock)comboBox.Template.FindName("Placeholder", comboBox);
            Assert.Multiple(() =>
            {
                Assert.That(item.ContentTemplateSelector, Is.Not.Null);
                Assert.That(itemTextValues, Does.Contain("First"));
                Assert.That(textValues, Does.Contain("First"));
                Assert.That(textValues, Does.Not.Contain(typeof(DisplayItem).FullName));
                Assert.That(placeholder.Visibility, Is.EqualTo(Visibility.Collapsed));
                Assert.That(
                    FindVisualChildren<ContentPresenter>(comboBox)
                        .Any(presenter => ReferenceEquals(presenter.Content, comboBox.SelectedItem)
                            && presenter.ContentTemplateSelector is not null),
                    Is.True);
            });
        }
        finally
        {
            TestHost.CloseAndDrain(window);
        }
    }

    [Test]
    public void TextBoxAndComboBox_OverlayInGridHaveSameArrangedHeight()
    {
        var grid = new Grid { Width = 240 };
        var textBox = new JunevyTextBox { Text = "Value" };
        var comboBox = new JunevyComboBox
        {
            ItemsSource = new[] { "Value" },
            SelectedIndex = 0,
            Visibility = Visibility.Collapsed
        };
        ApplyTheme(grid);
        grid.Children.Add(textBox);
        grid.Children.Add(comboBox);

        Window window = TestHost.Show(grid);
        try
        {
            grid.UpdateLayout();
            double textHeight = textBox.ActualHeight;
            double textTop = textBox.TranslatePoint(new Point(), grid).Y;

            textBox.Visibility = Visibility.Collapsed;
            comboBox.Visibility = Visibility.Visible;
            grid.UpdateLayout();

            double comboTop = comboBox.TranslatePoint(new Point(), grid).Y;
            Assert.Multiple(() =>
            {
                Assert.That(comboBox.ActualHeight, Is.EqualTo(textHeight).Within(0.01));
                Assert.That(grid.ActualHeight, Is.EqualTo(textHeight).Within(0.01));
                Assert.That(comboTop, Is.EqualTo(textTop).Within(0.01));
            });
        }
        finally
        {
            TestHost.CloseAndDrain(window);
        }
    }

    private static void ApplyTheme(FrameworkElement element)
    {
        element.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri(
                "pack://application:,,,/Junevy.Controls;component/Themes/Generic.xaml",
                UriKind.Absolute)
        });
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        if (root is null)
        {
            yield break;
        }

        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                yield return match;
            }

            foreach (T descendant in FindVisualChildren<T>(child))
            {
                yield return descendant;
            }
        }
    }

    private sealed class DisplayItem
    {
        internal DisplayItem(string name) => Name = name;

        public string Name { get; }
    }
}
