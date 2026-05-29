using DevExpress.Xpf.WindowsUI;
using System.Windows;

namespace Mermer.Ui.Pc.Controls.MenuItems;

public partial class CopyCreate : AppBarButton
{
    public static readonly DependencyProperty OrderOnlyProperty = DependencyProperty.Register(nameof(OrderOnly), typeof(bool), typeof(CopyCreate), new PropertyMetadata(false));

    public CopyCreate() => InitializeComponent();

    public bool OrderOnly
    {
        get => (bool)GetValue(OrderOnlyProperty);
        set => SetValue(OrderOnlyProperty, value);
    }
}