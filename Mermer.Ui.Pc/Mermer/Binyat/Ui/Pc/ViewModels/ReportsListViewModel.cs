// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.ViewModels.ReportsListViewModel
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.Mvvm;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using Mermer.Ui.Core.Helpers;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Pc.ViewModels;

public class ReportsListViewModel(
  IMvxMessenger messenger,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : ListViewModelBase<ListHelper<string>>(messenger, navigationService, userInteractionService)
{
  public override string Caption => this["Printing Layouts List", Array.Empty<object>()];

  protected override Task OnLoad()
  {
    this.List = (IEnumerable<ListHelper<string>>) new ListHelper<string>[10]
    {
      new ListHelper<string>("InvoiceStandard", this["Invoice", Array.Empty<object>()]),
      new ListHelper<string>("InvoiceCheque", this["Invoice Cheque", Array.Empty<object>()]),
      new ListHelper<string>("StockSlipStandard", this["Stock Slip", Array.Empty<object>()]),
      new ListHelper<string>("StockOrderStandard", this["Stock Order", Array.Empty<object>()]),
      new ListHelper<string>("StockTransferStandard", this["Stock Transfer", Array.Empty<object>()]),
      new ListHelper<string>("StockTransferStandardSent", this["Stock Transfer Sent", Array.Empty<object>()]),
      new ListHelper<string>("StockTransferStandardReceived", this["Stock Transfer Received", Array.Empty<object>()]),
      new ListHelper<string>("BillStandard", this["Bill", Array.Empty<object>()]),
      new ListHelper<string>("BillCheque", this["Bill Cheque", Array.Empty<object>()]),
      new ListHelper<string>("ExpenseSlipStandard", this["Expense Slip", Array.Empty<object>()])
    };
    return base.OnLoad();
  }
    // Команда для двойного клика (Выбрать или Просмотр)
    public ICommand SelectOrViewDetailsCommand => new MvxAsyncCommand(OnSelectOrViewDetailsCommandAsync, () => !IsBusy);

    protected virtual Task OnSelectOrViewDetailsCommandAsync()
    {
        try
        {
            var type = this.GetType();
            var editCmd = type.GetProperty("EditCommand")?.GetValue(this) as ICommand;
            var selectCmd = type.GetProperty("SelectCommand")?.GetValue(this) as ICommand;

            if (selectCmd != null && selectCmd.CanExecute(null))
            {
                selectCmd.Execute(null);
            }
            else if (editCmd != null && editCmd.CanExecute(null))
            {
                editCmd.Execute(null);
            }
        }
        catch { }

        return Task.CompletedTask;
    }

    // Команда для кнопки "Просмотр деталей"
    public ICommand ViewDetailsCommand => new MvxAsyncCommand(OnViewDetailsCommandAsync, () => !IsBusy);

    protected virtual Task OnViewDetailsCommandAsync()
    {
        try
        {
            var editCmd = this.GetType().GetProperty("EditCommand")?.GetValue(this) as ICommand;

            if (editCmd != null && editCmd.CanExecute(null))
            {
                editCmd.Execute(null);
            }
        }
        catch { }

        return Task.CompletedTask;
    }
}
