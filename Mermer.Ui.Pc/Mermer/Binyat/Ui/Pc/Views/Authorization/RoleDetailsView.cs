using DevExpress.Xpf.Editors;
using DevExpress.Xpf.LayoutControl;
using MvvmCross.Wpf.Views;
using Mermer.Ui.Core.ViewModels.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Binding = System.Windows.Data.Binding;

namespace Mermer.Ui.Pc.Views.Authorization;

public partial class RoleDetailsView : MvxWpfView
{
    public RoleDetailsView() => InitializeComponent();

    private async void RoleDetailsView_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is RoleDetailsViewModel vm)
        {
            while (vm.IsBusy)
                await Task.Delay(TimeSpan.FromSeconds(1.0));

            PopulateActionOptions(Actions, vm.Actions, "Actions");
            PopulateActionOptions(ListActions, vm.ListActions, "ListActions");
            PopulateActionOptions(TransactionActions, vm.TransactionActions, "TransactionActions");
        }
    }

    private static void PopulateActionOptions(LayoutGroup listGroup, IEnumerable<RoleAction> actions, string propertyName)
    {
        var roleActionArray = actions as RoleAction[] ?? actions.ToArray();

        for (int index1 = 0; index1 < roleActionArray.Length; ++index1)
        {
            LayoutGroup layoutGroup = new LayoutGroup();
            var roleOptionArray = roleActionArray[index1].Options as RoleOption[] ?? roleActionArray[index1].Options.ToArray();

            for (int index2 = 0; index2 < roleOptionArray.Length; ++index2)
            {
                CheckEdit checkEdit = new CheckEdit
                {
                    Content = roleOptionArray[index2].Name
                };

                Binding binding = new Binding
                {
                    Path = new PropertyPath($"{propertyName}[{index1}].Options[{index2}].IsSelected"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding(checkEdit, CheckEdit.IsCheckedProperty, binding);
                layoutGroup.Children.Add(checkEdit);
            }

            LayoutItem element = new LayoutItem
            {
                Label = roleActionArray[index1].Name,
                Content = layoutGroup
            };
            listGroup.Children.Add(element);
        }
    }
}