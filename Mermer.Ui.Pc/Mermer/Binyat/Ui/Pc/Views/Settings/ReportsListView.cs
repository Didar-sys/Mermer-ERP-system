using MvvmCross.Wpf.Views;
using Mermer.Ui.Pc.ViewModels;
using System.Windows;

namespace Mermer.Ui.Pc.Views.Settings;

public partial class ReportsListView : MvxWpfView
{
    public ReportsListView() => InitializeComponent();

    private ReportsListViewModel Vm => ViewModel as ReportsListViewModel;

    private void OnEditReportClick(object sender, RoutedEventArgs e)
    {
        if (Vm?.SelectedItem == null) return;
        new ReportDesigner(Vm.SelectedItem.Value).Show();
    }
}