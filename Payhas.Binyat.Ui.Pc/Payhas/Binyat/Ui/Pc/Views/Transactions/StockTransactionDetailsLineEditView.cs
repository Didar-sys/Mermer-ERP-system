// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Views.Transactions.StockTransactionDetailsLineEditView
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Xpf.Editors;
using MvvmCross.Wpf.Views;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Views.Transactions;

public class StockTransactionDetailsLineEditView : MvxWpfView, IComponentConnector
{
  internal SpinEdit FirstFocus;
  internal SpinEdit PriceEdit;
  private bool _contentLoaded;

  public StockTransactionDetailsLineEditView() => this.InitializeComponent();

  private void FirstFocus_KeyDown(object sender, KeyEventArgs e)
  {
    if (e.Key != Key.Return)
      return;
    this.PriceEdit.Focus();
    e.Handled = true;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/views/transactions/stocktransactiondetailslineeditview.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 1)
    {
      if (connectionId == 2)
        this.PriceEdit = (SpinEdit) target;
      else
        this._contentLoaded = true;
    }
    else
    {
      this.FirstFocus = (SpinEdit) target;
      this.FirstFocus.KeyDown += new KeyEventHandler(this.FirstFocus_KeyDown);
    }
  }
}
