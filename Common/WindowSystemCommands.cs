using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;

namespace Junevy.Controls.Common;

/// <summary>
///     WPF declares <see cref="SystemCommands" /> window commands but never registers handlers
///     for them, so a caption button bound to one is auto-disabled until the host window supplies
///     the binding. Controls that ship caption buttons install the missing handlers here.
/// </summary>
internal static class WindowSystemCommands
{
    public static void EnsureRegistered(Window window)
    {
        var installed = Add(window, SystemCommands.MinimizeWindowCommand,
            static w => SystemCommands.MinimizeWindow(w),
            static w => w.ResizeMode != ResizeMode.NoResize);

        installed |= Add(window, SystemCommands.MaximizeWindowCommand,
            static w => SystemCommands.MaximizeWindow(w),
            static w => w.WindowState == WindowState.Normal && IsResizable(w));

        installed |= Add(window, SystemCommands.RestoreWindowCommand,
            static w => SystemCommands.RestoreWindow(w),
            static w => w.WindowState != WindowState.Normal && IsResizable(w));

        installed |= Add(window, SystemCommands.CloseWindowCommand,
            static w => w.Close(), null);

        if (installed)
        {
            // Buttons realized before the bindings appeared still need one requery pass.
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private static bool Add(Window window, RoutedCommand command, Action<Window> execute, Func<Window, bool>? canExecute)
    {
        if (IsRegistered(window, command))
        {
            return false;
        }

        window.CommandBindings.Add(canExecute == null
            ? new CommandBinding(command, (_, _) => execute(window))
            : new CommandBinding(command, (_, _) => execute(window), (_, args) =>
            {
                args.CanExecute = canExecute(window);
                args.Handled = true;
            }));

        return true;
    }

    private static bool IsRegistered(Window window, ICommand command)
        => Contains(window.CommandBindings, command);

    private static bool Contains(CommandBindingCollection bindings, ICommand command)
    {
        for (var i = 0; i < bindings.Count; i++)
        {
            if (Equals(bindings[i].Command, command))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsResizable(Window window)
        => window.ResizeMode == ResizeMode.CanResize || window.ResizeMode == ResizeMode.CanResizeWithGrip;
}
