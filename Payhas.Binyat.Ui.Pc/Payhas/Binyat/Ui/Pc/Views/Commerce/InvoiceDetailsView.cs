// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Views.Commerce.InvoiceDetailsView
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.LookUp;
using MvvmCross.Wpf.Views;
using Payhas.Binyat.Ui.Core.ViewModels.Commerce;
using Payhas.Binyat.Ui.Pc.Controls;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Views.Commerce;

public class InvoiceDetailsView : MvxWpfView, IComponentConnector
{
  internal StockLookupEdit FirstFocus;
  internal GridControl GridControl;
  internal LookUpEdit PartnerEditor;
  private bool _contentLoaded;

  public InvoiceDetailsView() => this.InitializeComponent();

  private void DetectShortCut(object sender, KeyEventArgs e)
  {
    if (!(this.DataContext is InvoiceDetailsViewModel dataContext))
      return;
    switch (e.Key)
    {
      case Key.End:
        dataContext.UpdatePaymentCommand.Execute((object) null);
        e.Handled = true;
        break;
      case Key.Home:
        this.PartnerEditor.Focus();
        this.PartnerEditor.OpenPopupCommand.Execute((object) null);
        e.Handled = true;
        break;
      case Key.Insert:
        dataContext.SelectedLinePlusOneCommand.Execute((object) null);
        e.Handled = true;
        break;
      case Key.Delete:
        dataContext.SelectedLineMinusOneCommand.Execute((object) null);
        e.Handled = true;
        break;
      case Key.F3:
        this.FirstFocus.Focus();
        e.Handled = true;
        break;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/views/commerce/invoicedetailsview.xaml", UriKind.Relative));
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
    switch (connectionId)
    {
      case 1:
        ((UIElement) target).PreviewKeyDown += new KeyEventHandler(this.DetectShortCut);
        break;
      case 2:
        this.FirstFocus = (StockLookupEdit) target;
        break;
      case 3:
        this.GridControl = (GridControl) target;
        break;
      case 4:
        this.PartnerEditor = (LookUpEdit) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
