using DevExpress.Xpf.WindowsUI;
using System.Windows;
using System.Windows.Media;

namespace Mermer.Ui.Pc.Controls.MenuItems;

public partial class CommonClose : AppBarButton
{
    public CommonClose() => InitializeComponent();

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // 1. Знаходимо велике вікно (наприклад, InvoiceDetailsView), в якому лежить ця кнопка
        var parentView = FindParentView(this);

        if (parentView != null)
        {
            // 2. Передаємо це вікно нашому кілеру вкладок!
            Mermer.Ui.Pc.TabNavigationHelper.ForceCloseTab(parentView);
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