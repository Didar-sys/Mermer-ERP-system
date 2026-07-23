using DevExpress.Xpf.WindowsUI;
using System.Windows;
using System.Windows.Media;

namespace Mermer.Ui.Pc.Controls.MenuItems;

public partial class CommonClose : AppBarButton
{
    public CommonClose() => InitializeComponent();

    private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // Отримуємо поточну ViewModel нашої форми
        if (this.DataContext is Mermer.Mvvm.ViewModels.BaseViewModel viewModel)
        {
            // Викликаємо правильну команду закриття з ViewModel (яка містить перевірку на IsDirty та біле вікно)
            if (viewModel.CloseCommand != null && viewModel.CloseCommand.CanExecute(null))
            {
                viewModel.CloseCommand.Execute(null);
            }
        }
    }

    // Радар: піднімається по дереву елементів вгору, поки не знайде головну форму
    private FrameworkElement FindParentView(DependencyObject child)
    {
        if (child == null) return null;

        DependencyObject parent = VisualTreeHelper.GetParent(child) ?? LogicalTreeHelper.GetParent(child);
        if (parent == null) return null;

        // Зупиняємося, коли знаходимо головну форму (вони успадковуються від MvxWpfView або мають "View" у назві)
        if (parent is MvvmCross.Wpf.Views.MvxWpfView || parent.GetType().Name.EndsWith("View"))
        {
            return parent as FrameworkElement;
        }

        return FindParentView(parent);
    }
}