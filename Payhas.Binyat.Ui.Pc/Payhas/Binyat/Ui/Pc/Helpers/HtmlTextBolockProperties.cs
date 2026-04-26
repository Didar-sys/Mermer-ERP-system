// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Helpers.HtmlTextBolockProperties
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Helpers;

public static class HtmlTextBolockProperties
{
  public static readonly DependencyProperty HtmlTextProperty = DependencyProperty.RegisterAttached("HtmlText", typeof (string), typeof (HtmlTextBolockProperties), (PropertyMetadata) new UIPropertyMetadata((object) "", new PropertyChangedCallback(HtmlTextBolockProperties.OnHtmlTextChanged)));

  public static string GetHtmlText(TextBlock wb)
  {
    return wb.GetValue(HtmlTextBolockProperties.HtmlTextProperty) as string;
  }

  public static void SetHtmlText(TextBlock wb, string html)
  {
    wb.SetValue(HtmlTextBolockProperties.HtmlTextProperty, (object) html);
  }

  private static void OnHtmlTextChanged(
    DependencyObject depObj,
    DependencyPropertyChangedEventArgs e)
  {
    if (!(depObj is TextBlock textBlock) || !(e.NewValue is string))
      return;
    string newValue = (string) e.NewValue;
    InlineCollection inlines;
    try
    {
      inlines = ((Paragraph) ((Section) XamlReader.Parse(HtmlToXamlConverter.ConvertHtmlToXaml(newValue, false))).Blocks.FirstBlock).Inlines;
    }
    catch
    {
      return;
    }
    Inline[] array = new Inline[inlines.Count];
    inlines.CopyTo(array, 0);
    textBlock.Inlines.Clear();
    foreach (Inline inline in array)
      textBlock.Inlines.Add(inline);
  }
}
