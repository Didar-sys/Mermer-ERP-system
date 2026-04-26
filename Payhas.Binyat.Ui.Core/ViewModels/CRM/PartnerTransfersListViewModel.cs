// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.CRM.PartnerTransfersListViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.CRM.Models;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.CRM;

public class PartnerTransfersListViewModel : TransactionsListViewModel<PartnerTransfer>
{
  public PartnerTransfersListViewModel(
    IMvxMessenger messenger,
    Reference<Office> offices,
    Reference<Partner> partners,
    IRepository<PartnerTransfer> repository,
    IListAuthorizer<PartnerTransfer> authorizer,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(repository, authorizer, messenger, navigationService, userInteractionService)
  {
    this.Offices = offices;
    this.Partners = partners;
  }

  public Reference<Partner> Partners { get; set; }

  public Reference<Office> Offices { get; }

  protected override Task PreLoad()
  {
    return Task.WhenAll(base.PreLoad(), this.Partners.Initialize(), this.Offices.Initialize());
  }
}
