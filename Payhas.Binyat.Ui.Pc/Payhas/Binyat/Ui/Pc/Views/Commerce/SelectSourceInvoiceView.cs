// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Views.Commerce.SelectSourceInvoiceView
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using MvvmCross.Wpf.Views;
using Payhas.Binyat.Ui.Core.ViewModels.Commerce;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Views.Commerce;

public class SelectSourceInvoiceView : MvxWpfView, IComponentConnector
{
  private bool _contentLoaded;

  public SelectSourceInvoiceView() => this.InitializeComponent();

  private void ButtonEdit_KeyDown(object sender, KeyEventArgs e)
  {
    if (e.Key != Key.Return || !(this.DataContext is SelectSourceInvoiceViewModel dataContext))
      return;
    dataContext.SearchInvoices.Execute((object) null);
    e.Handled = true;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/views/commerce/selectsourceinvoiceview.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  internal Delegate _CreateDelegate(Type delegateType, string handler)
  {
    return Delegate.CreateDelegate(delegateType, (object) this, handler);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    if (connectionId == 1)
      ((UIElement) target).KeyDown += new KeyEventHandler(this.ButtonEdit_KeyDown);
    else
      this._contentLoaded = true;
  }
}
