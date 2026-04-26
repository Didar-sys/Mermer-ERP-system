// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Views.Warehousing.Revisioning.StockRevisionDetailsReportView
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Xpf.Controls.Primitives;
using MvvmCross.Wpf.Views;
using Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Revisioning;
using Payhas.Binyat.Ui.Pc.Controls;
using Payhas.Binyat.Ui.Pc.Controls.MenuItems;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Views.Warehousing.Revisioning;

public class StockRevisionDetailsReportView : MvxWpfView, IComponentConnector
{
  internal StyledGridSearchControl SearchControl;
  internal ListPrint PrintButton;
  internal ListFilter FilterButton;
  internal ToggleButton ExpandButton;
  internal StyledGridControl GridControl;
  private bool _contentLoaded;

  public StockRevisionDetailsReportView() => this.InitializeComponent();

  private void ExceedsClick(object sender, EventArgs e)
  {
    this.GridControl.FilterString = "TotalDifference > 0";
    if (!(this.DataContext is StockRevisionDetailsReportViewModel dataContext))
      return;
    dataContext.SubCaption = dataContext["Exceeds", Array.Empty<object>()];
  }

  private void EqualsClick(object sender, EventArgs e)
  {
    this.GridControl.FilterString = "TotalDifference = 0";
    if (!(this.DataContext is StockRevisionDetailsReportViewModel dataContext))
      return;
    dataContext.SubCaption = dataContext["Equals", Array.Empty<object>()];
  }

  private void DeficitsClick(object sender, EventArgs e)
  {
    this.GridControl.FilterString = "TotalDifference < 0";
    if (!(this.DataContext is StockRevisionDetailsReportViewModel dataContext))
      return;
    dataContext.SubCaption = dataContext["Deficits", Array.Empty<object>()];
  }

  private void AllRecordsClick(object sender, EventArgs e)
  {
    this.GridControl.FilterString = string.Empty;
    if (!(this.DataContext is StockRevisionDetailsReportViewModel dataContext))
      return;
    dataContext.SubCaption = dataContext["All Records", Array.Empty<object>()];
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/views/warehousing.revisioning/stockrevisiondetailsreportview.xaml", UriKind.Relative));
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
        this.SearchControl = (StyledGridSearchControl) target;
        break;
      case 2:
        this.PrintButton = (ListPrint) target;
        break;
      case 3:
        this.FilterButton = (ListFilter) target;
        break;
      case 4:
        ((ClickableBase) target).Click += new EventHandler(this.ExceedsClick);
        break;
      case 5:
        ((ClickableBase) target).Click += new EventHandler(this.EqualsClick);
        break;
      case 6:
        ((ClickableBase) target).Click += new EventHandler(this.DeficitsClick);
        break;
      case 7:
        ((ClickableBase) target).Click += new EventHandler(this.AllRecordsClick);
        break;
      case 8:
        this.ExpandButton = (ToggleButton) target;
        break;
      case 9:
        this.GridControl = (StyledGridControl) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
