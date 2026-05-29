using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid.LookUp;
using DevExpress.Xpf.LayoutControl;
using Mermer.Ui.Core.ViewModels.Common;
using Microsoft.Win32;
using MvvmCross.Wpf.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Binding = System.Windows.Data.Binding;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Mermer.Ui.Pc.Views.Common;

public partial class DataImportView : MvxWpfView
{
    private DataImportViewModel _vm;

    public DataImportView() => InitializeComponent();

    private void MvxWpfView_Loaded(object sender, RoutedEventArgs e)
    {
        _vm = DataContext as DataImportViewModel;
        if (_vm == null)
            throw new Exception("Wrong VM binded, expected: DataImportViewModel");

        _vm.PropertyChanged += Vm_PropertyChanged;
        if (_vm.Properties != null)
            UpdateLayout(_vm.Properties);
    }

    private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Properties")
            UpdateLayout(_vm.Properties);
    }

    private void UpdateLayout(IEnumerable<DataImportViewModel.Property> properties)
    {
        int num = 0;
        foreach (DataImportViewModel.Property property in properties)
        {
            LayoutItem element = new LayoutItem { Label = property.DisplayName };
            LookUpEdit target = new LookUpEdit();

            System.Windows.Data.Binding binding = new Binding
            {
                Path = new PropertyPath($"Properties[{num}].ColumnIndex"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            BindingOperations.SetBinding(target, BaseEdit.EditValueProperty, binding);
            BindingOperations.SetBinding(target, LookUpEditBase.ItemsSourceProperty, new Binding("Columns"));

            target.DisplayMember = "Text";
            target.ValueMember = "Value";
            target.NullValueButtonPlacement = EditorPlacement.EditBox;

            element.Content = target;
            Properties.Root.Children.Add(element);
            ++num;
        }
    }

    private void ButtonInfo_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog().GetValueOrDefault())
        {
            FileName.Text = openFileDialog.FileName;
        }
    }
}