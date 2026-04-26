// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Controls.StyledGridControl
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Controls;

public class StyledGridControl : GridControl, IComponentConnector
{
  public static readonly DependencyProperty AutoWidthProperty = DependencyProperty.Register(nameof (AutoWidth), typeof (bool), typeof (StyledGridControl), new PropertyMetadata((object) true));
  public static readonly DependencyProperty ShowGroupedColumnsProperty = DependencyProperty.Register(nameof (ShowGroupedColumns), typeof (bool), typeof (StyledGridControl), new PropertyMetadata((object) true));
  public static readonly DependencyProperty ShowTotalSummaryProperty = DependencyProperty.Register(nameof (ShowTotalSummary), typeof (bool), typeof (StyledGridControl), new PropertyMetadata((object) true));
  public static readonly DependencyProperty SearchControlProperty = DependencyProperty.Register(nameof (SearchControl), typeof (SearchControl), typeof (StyledGridControl), new PropertyMetadata((object) null));
  public static readonly DependencyProperty PrintButtonProperty = DependencyProperty.Register(nameof (PrintButton), typeof (ButtonBase), typeof (StyledGridControl), new PropertyMetadata((object) null));
  public static readonly DependencyProperty FilterButtonProperty = DependencyProperty.Register(nameof (FilterButton), typeof (ButtonBase), typeof (StyledGridControl), new PropertyMetadata((object) null));
  internal StyledGridControl This;
  internal TableView TableView;
  private bool _contentLoaded;

  public StyledGridControl() => this.InitializeComponent();

  public bool AutoWidth
  {
    get => (bool) this.GetValue(StyledGridControl.AutoWidthProperty);
    set => this.SetValue(StyledGridControl.AutoWidthProperty, (object) value);
  }

  public bool ShowGroupedColumns
  {
    get => (bool) this.GetValue(StyledGridControl.ShowGroupedColumnsProperty);
    set => this.SetValue(StyledGridControl.ShowGroupedColumnsProperty, (object) value);
  }

  public bool ShowTotalSummary
  {
    get => (bool) this.GetValue(StyledGridControl.ShowTotalSummaryProperty);
    set => this.SetValue(StyledGridControl.ShowTotalSummaryProperty, (object) value);
  }

  public SearchControl SearchControl
  {
    get => (SearchControl) this.GetValue(StyledGridControl.SearchControlProperty);
    set => this.SetValue(StyledGridControl.SearchControlProperty, (object) value);
  }

  public ButtonBase PrintButton
  {
    get => (ButtonBase) this.GetValue(StyledGridControl.PrintButtonProperty);
    set => this.SetValue(StyledGridControl.PrintButtonProperty, (object) value);
  }

  public ButtonBase FilterButton
  {
    get => (ButtonBase) this.GetValue(StyledGridControl.FilterButtonProperty);
    set => this.SetValue(StyledGridControl.FilterButtonProperty, (object) value);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/controls/styledgridcontrol.xaml", UriKind.Relative));
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
        this.TableView = (TableView) target;
      else
        this._contentLoaded = true;
    }
    else
      this.This = (StyledGridControl) target;
  }
}
