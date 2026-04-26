// Decompiled with JetBrains decompiler
// Type: Payhas.Ui.Core.Pc.Tools.LocalizeExtension
// Assembly: Payhas.Ui.Core.Pc, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99463FBB-953B-46DD-9DD6-5278306A8C84
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Ui.Core.Pc.dll

using MvvmCross.Localization;
using MvvmCross.Platform;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Ui.Core.Pc.Tools;

[ContentProperty("Text")]
[MarkupExtensionReturnType(typeof (string))]
public class LocalizeExtension : UpdatableMarkupExtension
{
  private readonly IMvxTextProvider _textProvicer;
  private string _fallBackText;

  public LocalizeExtension()
  {
    try
    {
      this._textProvicer = Mvx.Resolve<IMvxTextProvider>();
    }
    catch
    {
      this._textProvicer = (IMvxTextProvider) null;
    }
  }

  public LocalizeExtension(string text)
    : this()
  {
    this.Text = text;
  }

  public LocalizeExtension(string text, string fall)
    : this(text)
  {
    this.FallBackText = fall;
  }

  public LocalizeExtension(string text, string fall, string params1)
    : this(text, fall)
  {
    this.Params1 = params1;
  }

  public LocalizeExtension(string text, string fall, string params1, string params2)
    : this(text, fall, params1)
  {
    this.Params2 = params2;
  }

  public LocalizeExtension(
    string text,
    string fall,
    string params1,
    string params2,
    string params3)
    : this(text, fall, params1, params2)
  {
    this.Params3 = params3;
  }

  [ConstructorArgument("text")]
  public string Text { get; set; }

  [ConstructorArgument("fall")]
  public string FallBackText
  {
    get
    {
      if (!string.IsNullOrEmpty(this._fallBackText))
        return string.Format(this._fallBackText, this.GetParams());
      return !string.IsNullOrEmpty(this.Text) ? "#" + this.Text : string.Empty;
    }
    set => this._fallBackText = value;
  }

  [ConstructorArgument("params1")]
  public string Params1 { get; set; }

  [ConstructorArgument("params2")]
  public string Params2 { get; set; }

  [ConstructorArgument("params3")]
  public string Params3 { get; set; }

  private object[] GetParams()
  {
    return ((IEnumerable<string>) new string[3]
    {
      this.Params1,
      this.Params2,
      this.Params3
    }).Where<string>((Func<string, bool>) (x => !string.IsNullOrEmpty(x))).Cast<object>().ToArray<object>();
  }

  private string GetDefaultText()
  {
    string text = this._textProvicer?.GetText((string) null, (string) null, this.Text, this.GetParams());
    return !string.IsNullOrEmpty(text) ? text : this.FallBackText;
  }

  private string GetText()
  {
    return !(this.TargetObject is FrameworkElement targetObject) || !(targetObject.DataContext is BaseViewModel dataContext) ? this.GetDefaultText() : dataContext.TextSource.GetText(this.Text);
  }

  protected override object ProvideValueInternal(IServiceProvider serviceProvider)
  {
    return (object) this.GetText();
  }

  protected override void TargetObjectDataContextChanged(
    object sender,
    DependencyPropertyChangedEventArgs e)
  {
    this.UpdateValue((object) this.GetText());
  }
}
