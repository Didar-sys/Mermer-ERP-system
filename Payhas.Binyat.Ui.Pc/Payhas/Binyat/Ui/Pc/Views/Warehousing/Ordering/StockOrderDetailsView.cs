// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Views.Warehousing.Ordering.StockOrderDetailsView
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using MvvmCross.Wpf.Views;
using Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Ordering;
using Payhas.Binyat.Ui.Pc.Controls;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Views.Warehousing.Ordering;

public class StockOrderDetailsView : MvxWpfView, IComponentConnector
{
  internal StockLookupEdit FirstFocus;
  private bool _contentLoaded;

  public StockOrderDetailsView() => this.InitializeComponent();

  private void DetectShortCut(object sender, KeyEventArgs e)
  {
    if (!(this.DataContext is StockOrderDetailsViewModel dataContext))
      return;
    switch (e.Key)
    {
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
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/views/warehousing.ordering/stockorderdetailsview.xaml", UriKind.Relative));
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
    if (connectionId != 1)
    {
      if (connectionId == 2)
        this.FirstFocus = (StockLookupEdit) target;
      else
        this._contentLoaded = true;
    }
    else
      ((UIElement) target).PreviewKeyDown += new KeyEventHandler(this.DetectShortCut);
  }
}
