// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Views.StockManagement.StockNameComposerDialogView
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.Xpf.Grid.LookUp;
using DevExpress.Xpf.LayoutControl;
using MvvmCross.Wpf.Views;
using Mermer.StockManagement.Models;
using Mermer.Ui.Core.ViewModels.StockManagement;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

#nullable disable
namespace Mermer.Ui.Pc.Views.StockManagement;

public class StockNameComposerDialogView : MvxWpfView, IComponentConnector
{
  private List<LookUpEdit> _composerEdits;
  private StockNameComposerDialogViewModel _vm;
  internal LayoutGroup Composers;
  private bool _contentLoaded;

  public StockNameComposerDialogView() => this.InitializeComponent();

  private void MvxWpfView_Loaded(object sender, RoutedEventArgs e)
  {
    this._vm = this.DataContext as StockNameComposerDialogViewModel;
    if (this._vm == null)
      throw new Exception("Wrong VM binded, expected: StockNameComposerDialogViewModel");
    this._vm.PropertyChanged += new PropertyChangedEventHandler(this.Vm_PropertyChanged);
    if (this._vm.Composers == null)
      return;
    this.UpdateLayout(this._vm.Composers);
  }

  private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (!(e.PropertyName == "Composers"))
      return;
    this.UpdateLayout(this._vm.Composers);
  }

  private void UpdateLayout(IEnumerable<StockNameComposer> composers)
  {
    this._composerEdits = new List<LookUpEdit>();
    foreach (StockNameComposer composer in composers)
    {
      LayoutItem element = new LayoutItem();
      element.Label = (object) composer.Name;
      LookUpEdit lookUpEdit = new LookUpEdit();
      lookUpEdit.ItemsSource = (object) composer.Values;
      lookUpEdit.DisplayMember = "Fullname";
      lookUpEdit.ValueMember = "Fullname";
      element.Content = (UIElement) lookUpEdit;
      this.Composers.Root.Children.Add((UIElement) element);
      this._composerEdits.Add(lookUpEdit);
    }
  }

  private void Button_Click(object sender, RoutedEventArgs e)
  {
    this._vm.Values = this._composerEdits.Where<LookUpEdit>((Func<LookUpEdit, bool>) (x => x.SelectedItem != null)).Select<LookUpEdit, object>((Func<LookUpEdit, object>) (x => x.SelectedItem)).Cast<StockNameComposerValue>();
    this._vm.Compose.Execute((object) null);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Mermer.Ui.Pc;component/views/stockmanagement/stocknamecomposerdialogview.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.MvxWpfView_Loaded);
        break;
      case 2:
        this.Composers = (LayoutGroup) target;
        break;
      case 3:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.Button_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
