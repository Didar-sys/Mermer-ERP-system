// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Views.Common.DataImportView
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid.LookUp;
using DevExpress.Xpf.LayoutControl;
using Microsoft.Win32;
using MvvmCross.Wpf.Views;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Views.Common;

public class DataImportView : MvxWpfView, IComponentConnector
{
  private DataImportViewModel _vm;
  internal ButtonEdit FileName;
  internal LayoutGroup Properties;
  private bool _contentLoaded;

  public DataImportView() => this.InitializeComponent();

  private void MvxWpfView_Loaded(object sender, RoutedEventArgs e)
  {
    this._vm = this.DataContext as DataImportViewModel;
    if (this._vm == null)
      throw new Exception("Wrong VM binded, expected: DataImportViewModel");
    this._vm.PropertyChanged += new PropertyChangedEventHandler(this.Vm_PropertyChanged);
    if (this._vm.Properties == null)
      return;
    this.UpdateLayout((IEnumerable<DataImportViewModel.Property>) this._vm.Properties);
  }

  private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (!(e.PropertyName == "Properties"))
      return;
    this.UpdateLayout((IEnumerable<DataImportViewModel.Property>) this._vm.Properties);
  }

  private void UpdateLayout(
    IEnumerable<DataImportViewModel.Property> properties)
  {
    int num = 0;
    foreach (DataImportViewModel.Property property in properties)
    {
      LayoutItem element = new LayoutItem();
      element.Label = (object) property.DisplayName;
      LookUpEdit target = new LookUpEdit();
      Binding binding = new Binding()
      {
        Path = new PropertyPath($"{"Properties"}[{num}].{"ColumnIndex"}", Array.Empty<object>()),
        Mode = BindingMode.TwoWay,
        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
      };
      BindingOperations.SetBinding((DependencyObject) target, BaseEdit.EditValueProperty, (BindingBase) binding);
      BindingOperations.SetBinding((DependencyObject) target, LookUpEditBase.ItemsSourceProperty, (BindingBase) new Binding("Columns"));
      target.DisplayMember = "Text";
      target.ValueMember = "Value";
      target.NullValueButtonPlacement = new EditorPlacement?(EditorPlacement.EditBox);
      element.Content = (UIElement) target;
      this.Properties.Root.Children.Add((UIElement) element);
      ++num;
    }
  }

  private void ButtonInfo_Click(object sender, RoutedEventArgs e)
  {
    OpenFileDialog openFileDialog = new OpenFileDialog();
    if (!openFileDialog.ShowDialog().GetValueOrDefault())
      return;
    this.FileName.Text = openFileDialog.FileName;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/views/common/dataimportview.xaml", UriKind.Relative));
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
        this.FileName = (ButtonEdit) target;
        break;
      case 3:
        ((CommandButtonInfo) target).Click += new RoutedEventHandler(this.ButtonInfo_Click);
        break;
      case 4:
        this.Properties = (LayoutGroup) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
