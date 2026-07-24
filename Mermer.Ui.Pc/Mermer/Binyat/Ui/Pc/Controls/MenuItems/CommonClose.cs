using DevExpress.Xpf.WindowsUI;
using System.Windows;
using System.Windows.Media;

namespace Mermer.Ui.Pc.Controls.MenuItems;

public partial class CommonClose : AppBarButton
{
    public CommonClose() => InitializeComponent();

    private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // Получаем текущую ViewModel нашей формы
        if (this.DataContext is Mermer.Mvvm.ViewModels.BaseViewModel viewModel)
        {
            // Вызываем правильную команду закрытия из ViewModel (которая содержит проверку на IsDirty и белое окно)
            if (viewModel.CloseCommand != null && viewModel.CloseCommand.CanExecute(null))
            {
                viewModel.CloseCommand.Execute(null);
            }
        }
    }

    // Радар: поднимается по дереву элементов вверх, пока не найдет главную форму
    private FrameworkElement FindParentView(DependencyObject child)
    {
        if (child == null) return null;

        DependencyObject parent = VisualTreeHelper.GetParent(child) ?? LogicalTreeHelper.GetParent(child);
        if (parent == null) return null;

        // Останавливаемся, когда находим главную форму (они наследуются от MvxWpfView или имеют "View" в названии)
        if (parent is MvvmCross.Wpf.Views.MvxWpfView || parent.GetType().Name.EndsWith("View"))
        {
            return parent as FrameworkElement;
        }

        return FindParentView(parent);
    }
}