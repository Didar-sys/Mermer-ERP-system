// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Helpers.ShortcutExtender
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Pc.Helpers;

public static class ShortcutExtender
{
  public static readonly DependencyProperty ShortcutProperty = DependencyProperty.RegisterAttached("Shortcut", typeof (string), typeof (GridExtender), (PropertyMetadata) new UIPropertyMetadata(new PropertyChangedCallback(ShortcutExtender.OnShortcutPropertyChanged)));
  public static readonly DependencyProperty FocusShortcutProperty = DependencyProperty.RegisterAttached("FocusShortcut", typeof (string), typeof (GridExtender), (PropertyMetadata) new UIPropertyMetadata(new PropertyChangedCallback(ShortcutExtender.OnFocusShortcutPropertyChanged)));

  public static Window MainForm { get; set; }

  public static string GetShortcut(DependencyObject obj)
  {
    return (string) obj.GetValue(ShortcutExtender.ShortcutProperty);
  }

  public static void SetShortcut(DependencyObject obj, string value)
  {
    obj.SetValue(ShortcutExtender.ShortcutProperty, (object) value);
  }

  private static void OnShortcutPropertyChanged(
    DependencyObject d,
    DependencyPropertyChangedEventArgs e)
  {
    if (ShortcutExtender.MainForm == null)
      return;
    Button button = d as Button;
    if (button == null)
      return;
    string shortcut = e.NewValue as string;
    if (string.IsNullOrEmpty(shortcut))
      return;
    IInvokeProvider buttonClicker = (IInvokeProvider) new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke);
    ShortcutExtender.MainForm.KeyDown += (KeyEventHandler) ((sender, arg) =>
    {
      if (!button.IsVisible || !(shortcut == ShortcutExtender.GetShortcutString(arg)))
        return;
      if (button.Command != null)
        button.Command.Execute(button.CommandParameter);
      else
        buttonClicker?.Invoke();
    });
  }

  private static string GetShortcutString(KeyEventArgs arg)
  {
    string str = "";
    if (Keyboard.Modifiers == ModifierKeys.Control)
      str += "Ctrl+";
    if (Keyboard.Modifiers == ModifierKeys.Shift)
      str += "Shift+";
    if (Keyboard.Modifiers == ModifierKeys.Alt)
      str += "Alt+";
    return str + arg.Key.ToString();
  }

  public static string GetFocusShortcut(DependencyObject obj)
  {
    return (string) obj.GetValue(ShortcutExtender.FocusShortcutProperty);
  }

  public static void SetFocusShortcut(DependencyObject obj, string value)
  {
    obj.SetValue(ShortcutExtender.FocusShortcutProperty, (object) value);
  }

  private static void OnFocusShortcutPropertyChanged(
    DependencyObject d,
    DependencyPropertyChangedEventArgs e)
  {
    if (ShortcutExtender.MainForm == null)
      return;
    UIElement uielement = d as UIElement;
    if (uielement == null)
      return;
    string shortcut = e.NewValue as string;
    if (string.IsNullOrEmpty(shortcut))
      return;
    ShortcutExtender.MainForm.KeyDown += (KeyEventHandler) ((sender, arg) =>
    {
      if (!uielement.IsVisible || !(shortcut == ShortcutExtender.GetShortcutString(arg)))
        return;
      uielement.Focus();
    });
  }
}
