// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Enterprise.DepositoriesListViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Mermer.Enterprise.Models;
using Mermer.Ui.Core.Helpers;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Enterprise;

public class DepositoriesListViewModel : ListViewModel<Depository>
{
  public DepositoriesListViewModel(
    IRepository<Depository> depositoriesRepository,
    IListAuthorizer<Depository> authorizer,
    Reference<Office> officesReference,
    IMvxMessenger messenger,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(depositoriesRepository, authorizer, messenger, navigationService, userInteractionService)
  {
    this.Offices = officesReference;
  }

  protected override Task PreLoad() => Task.WhenAll(base.PreLoad(), this.Offices.Initialize());

  public Reference<Office> Offices { get; set; }
}
