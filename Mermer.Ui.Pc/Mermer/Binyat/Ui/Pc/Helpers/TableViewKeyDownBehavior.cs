using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Grid;
using System.Windows;
using System.Windows.Input;
// Вирішуємо конфлікт KeyEventArgs
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Mermer.Ui.Pc.Helpers;

public class TableViewKeyDownBehavior : Behavior<TableView>
{
    public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof(Key), typeof(Key), typeof(TableViewKeyDownBehavior), new PropertyMetadata(Key.None));
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(TableViewKeyDownBehavior), new PropertyMetadata(null));

    private TableView AssociatedView => AssociatedObject;

    public Key Key
    {
        get => (Key)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedView.PreviewKeyDown += AssociatedView_PreviewKeyDown;
    }

    protected override void OnDetaching()
    {
        AssociatedView.PreviewKeyDown -= AssociatedView_PreviewKeyDown;
        base.OnDetaching();
    }

    private void AssociatedView_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key) return;
        Command?.Execute(AssociatedView.DataControl.SelectedItem);
    }
}