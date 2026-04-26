// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Helpers.TableViewKeyDownBehavior
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Grid;
using System.Windows;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Helpers;

public class TableViewKeyDownBehavior : Behavior<TableView>
{
  public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof (Key), typeof (Key), typeof (TableViewKeyDownBehavior), new PropertyMetadata((object) Key.None));
  public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof (Command), typeof (ICommand), typeof (TableViewKeyDownBehavior), new PropertyMetadata((PropertyChangedCallback) null));

  private TableView AssociatedView => this.AssociatedObject;

  public Key Key
  {
    get => (Key) this.GetValue(TableViewKeyDownBehavior.KeyProperty);
    set => this.SetValue(TableViewKeyDownBehavior.KeyProperty, (object) value);
  }

  public ICommand Command
  {
    get => (ICommand) this.GetValue(TableViewKeyDownBehavior.CommandProperty);
    set => this.SetValue(TableViewKeyDownBehavior.CommandProperty, (object) value);
  }

  protected override void OnAttached()
  {
    base.OnAttached();
    this.AssociatedView.PreviewKeyDown += new KeyEventHandler(this.AssociatedView_PreviewKeyDown);
  }

  protected override void OnDetaching()
  {
    this.AssociatedView.PreviewKeyDown -= new KeyEventHandler(this.AssociatedView_PreviewKeyDown);
    base.OnDetaching();
  }

  private void AssociatedView_PreviewKeyDown(object sender, KeyEventArgs e)
  {
    if (e.Key != this.Key)
      return;
    this.Command?.Execute(this.AssociatedView.DataControl.SelectedItem);
  }
}
