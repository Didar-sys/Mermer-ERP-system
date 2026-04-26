// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Views.Commerce.InvoicePaymentDialogView
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Xpf.Editors;
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

public class InvoicePaymentDialogView : MvxWpfView, IComponentConnector
{
  internal SpinEdit FirstFocus;
  private bool _contentLoaded;

  public InvoicePaymentDialogView() => this.InitializeComponent();

  private void Payments_OnKeyDown(object sender, KeyEventArgs e)
  {
    if (e.Key != Key.End || !(this.DataContext is InvoicePaymentDialogViewModel dataContext))
      return;
    dataContext.FillPaymentCommand.Execute((object) null);
    e.Handled = true;
  }

  private void Changes_OnKeyDown(object sender, KeyEventArgs e)
  {
    if (e.Key != Key.End || !(this.DataContext is InvoicePaymentDialogViewModel dataContext))
      return;
    dataContext.FillChangesCommand.Execute((object) null);
    e.Handled = true;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/views/commerce/invoicepaymentdialogview.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((UIElement) target).PreviewKeyDown += new KeyEventHandler(this.Payments_OnKeyDown);
        break;
      case 2:
        this.FirstFocus = (SpinEdit) target;
        break;
      case 3:
        ((UIElement) target).PreviewKeyDown += new KeyEventHandler(this.Changes_OnKeyDown);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
