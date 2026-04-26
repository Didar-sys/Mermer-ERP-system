// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.ViewModels.ReportsListViewModel
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.ViewModels;

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
}
