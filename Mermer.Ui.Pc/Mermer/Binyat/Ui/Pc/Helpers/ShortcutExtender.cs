using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using Button = System.Windows.Controls.Button;

// Явно вказуємо, що мається на увазі KeyEventArgs з WPF
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Mermer.Ui.Pc.Helpers;

public static class ShortcutExtender
{
    public static readonly DependencyProperty ShortcutProperty = DependencyProperty.RegisterAttached("Shortcut", typeof(string), typeof(ShortcutExtender), new UIPropertyMetadata(OnShortcutPropertyChanged));
    public static readonly DependencyProperty FocusShortcutProperty = DependencyProperty.RegisterAttached("FocusShortcut", typeof(string), typeof(ShortcutExtender), new UIPropertyMetadata(OnFocusShortcutPropertyChanged));

    public static Window MainForm { get; set; }

    public static string GetShortcut(DependencyObject obj) => (string)obj.GetValue(ShortcutProperty);
    public static void SetShortcut(DependencyObject obj, string value) => obj.SetValue(ShortcutProperty, value);

    private static void OnShortcutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (MainForm == null || !(d is Button button)) return;

        string shortcut = e.NewValue as string;
        if (string.IsNullOrEmpty(shortcut)) return;

        IInvokeProvider buttonClicker = (IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke);

        MainForm.KeyDown += (sender, arg) =>
        {
            if (!button.IsVisible || shortcut != GetShortcutString(arg)) return;

            if (button.Command != null)
                button.Command.Execute(button.CommandParameter);
            else
                buttonClicker?.Invoke();
        };
    }

    private static string GetShortcutString(KeyEventArgs arg)
    {
        string str = "";
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) str += "Ctrl+";
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) str += "Shift+";
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) str += "Alt+";
        return str + arg.Key.ToString();
    }

    public static string GetFocusShortcut(DependencyObject obj) => (string)obj.GetValue(FocusShortcutProperty);
    public static void SetFocusShortcut(DependencyObject obj, string value) => obj.SetValue(FocusShortcutProperty, value);

    private static void OnFocusShortcutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (MainForm == null || !(d is UIElement uielement)) return;

        string shortcut = e.NewValue as string;
        if (string.IsNullOrEmpty(shortcut)) return;

        MainForm.KeyDown += (sender, arg) =>
        {
            if (!uielement.IsVisible || shortcut != GetShortcutString(arg)) return;
            uielement.Focus();
        };
    }
}