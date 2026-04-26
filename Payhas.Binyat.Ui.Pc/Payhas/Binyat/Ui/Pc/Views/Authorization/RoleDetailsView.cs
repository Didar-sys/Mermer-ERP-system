// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Views.Authorization.RoleDetailsView
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Xpf.Editors;
using DevExpress.Xpf.LayoutControl;
using MvvmCross.Wpf.Views;
using Payhas.Binyat.Ui.Core.ViewModels.Authorization;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Views.Authorization;

public class RoleDetailsView : MvxWpfView, IComponentConnector
{
  internal TextEdit FirstFocus;
  internal LayoutGroup Actions;
  internal LayoutGroup ListActions;
  internal LayoutGroup TransactionActions;
  private bool _contentLoaded;

  public RoleDetailsView() => this.InitializeComponent();

  private async void RoleDetailsView_OnLoaded(object sender, RoutedEventArgs e)
  {
    RoleDetailsView roleDetailsView = this;
    RoleDetailsViewModel vm = (RoleDetailsViewModel) roleDetailsView.DataContext;
    while (vm.IsBusy)
      await Task.Delay(TimeSpan.FromSeconds(1.0));
    RoleDetailsView.PopulateActionOptions(roleDetailsView.Actions, vm.Actions, "Actions");
    RoleDetailsView.PopulateActionOptions(roleDetailsView.ListActions, vm.ListActions, "ListActions");
    RoleDetailsView.PopulateActionOptions(roleDetailsView.TransactionActions, vm.TransactionActions, "TransactionActions");
    vm = (RoleDetailsViewModel) null;
  }

  private static void PopulateActionOptions(
    LayoutGroup listGroup,
    IEnumerable<RoleAction> actions,
    string propertyName)
  {
    if (!(actions is RoleAction[] roleActionArray))
      roleActionArray = actions.ToArray<RoleAction>();
    RoleAction[] source1 = roleActionArray;
    for (int index1 = 0; index1 < ((IEnumerable<RoleAction>) source1).Count<RoleAction>(); ++index1)
    {
      LayoutGroup layoutGroup = new LayoutGroup();
      if (!(source1[index1].Options is RoleOption[] roleOptionArray))
        roleOptionArray = source1[index1].Options.ToArray<RoleOption>();
      RoleOption[] source2 = roleOptionArray;
      for (int index2 = 0; index2 < ((IEnumerable<RoleOption>) source2).Count<RoleOption>(); ++index2)
      {
        CheckEdit checkEdit = new CheckEdit()
        {
          Content = (object) source2[index2].Name
        };
        Binding binding = new Binding()
        {
          Path = new PropertyPath($"{propertyName}[{index1}].Options[{index2}].IsSelected", Array.Empty<object>()),
          Mode = BindingMode.TwoWay,
          UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        BindingOperations.SetBinding((DependencyObject) checkEdit, CheckEdit.IsCheckedProperty, (BindingBase) binding);
        layoutGroup.Children.Add((UIElement) checkEdit);
      }
      LayoutItem element = new LayoutItem()
      {
        Label = (object) source1[index1].Name,
        Content = (UIElement) layoutGroup
      };
      listGroup.Children.Add((UIElement) element);
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/views/authorization/roledetailsview.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.RoleDetailsView_OnLoaded);
        break;
      case 2:
        this.FirstFocus = (TextEdit) target;
        break;
      case 3:
        this.Actions = (LayoutGroup) target;
        break;
      case 4:
        this.ListActions = (LayoutGroup) target;
        break;
      case 5:
        this.TransactionActions = (LayoutGroup) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
