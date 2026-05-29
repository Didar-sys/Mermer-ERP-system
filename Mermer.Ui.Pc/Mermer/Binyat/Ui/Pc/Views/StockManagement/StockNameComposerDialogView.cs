using DevExpress.Xpf.Grid.LookUp;
using DevExpress.Xpf.LayoutControl;
using MvvmCross.Wpf.Views;
using Mermer.StockManagement.Models;
using Mermer.Ui.Core.ViewModels.StockManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Mermer.Ui.Pc.Views.StockManagement;

public partial class StockNameComposerDialogView : MvxWpfView
{
    private List<LookUpEdit> _composerEdits;
    private StockNameComposerDialogViewModel _vm;

    public StockNameComposerDialogView() => InitializeComponent();

    private void MvxWpfView_Loaded(object sender, RoutedEventArgs e)
    {
        _vm = DataContext as StockNameComposerDialogViewModel;
        if (_vm == null)
            throw new Exception("Wrong VM binded, expected: StockNameComposerDialogViewModel");

        _vm.PropertyChanged += Vm_PropertyChanged;
        if (_vm.Composers != null)
            UpdateLayout(_vm.Composers);
    }

    private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Composers")
            UpdateLayout(_vm.Composers);
    }

    private void UpdateLayout(IEnumerable<StockNameComposer> composers)
    {
        _composerEdits = new List<LookUpEdit>();
        foreach (StockNameComposer composer in composers)
        {
            LayoutItem element = new LayoutItem { Label = composer.Name };
            LookUpEdit lookUpEdit = new LookUpEdit
            {
                ItemsSource = composer.Values,
                DisplayMember = "Fullname",
                ValueMember = "Fullname"
            };

            element.Content = lookUpEdit;
            Composers.Root.Children.Add(element);
            _composerEdits.Add(lookUpEdit);
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        _vm.Values = _composerEdits
            .Where(x => x.SelectedItem != null)
            .Select(x => x.SelectedItem)
            .Cast<StockNameComposerValue>();

        _vm.Compose.Execute(null);
    }
}