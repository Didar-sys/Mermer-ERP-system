// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Commerce.BillsListViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Commerce.Models;
using Payhas.Binyat.CRM.Models;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Transactions;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Commerce;

public class BillsListViewModel : FundsTransactionsListViewModel<Bill, BillLine>
{
  public BillsListViewModel(
    IMvxMessenger messenger,
    IRepository<Bill> repository,
    IListAuthorizer<Bill> authorizer,
    Reference<Partner> partners,
    Reference<Depository> depositories,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, repository, authorizer, depositories, navigationService, userInteractionService)
  {
    this.Partners = partners;
  }

  public Reference<Partner> Partners { get; }

  protected override Task PreLoad() => Task.WhenAll(base.PreLoad(), this.Partners.Initialize());
}
