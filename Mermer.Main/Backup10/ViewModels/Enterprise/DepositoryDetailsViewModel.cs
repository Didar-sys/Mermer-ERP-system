// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Enterprise.DepositoryDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.Enterprise.Models;
using Mermer.Ui.Core.Helpers;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Enterprise;

public class DepositoryDetailsViewModel : DetailsViewModel<Depository>
{
  private string[] _tagNames;

  public DepositoryDetailsViewModel(
    IRepositoryWithFacets<Depository> depositoriesRepository,
    IListAuthorizer<Depository> authorizer,
    Reference<Office> officesReference,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base((IRepository<Depository>) depositoriesRepository, authorizer, navigationService, userInteractionService)
  {
    this.Offices = officesReference;
  }

  public Reference<Office> Offices { get; set; }

  public virtual string[] TagNames
  {
    get => this._tagNames;
    set => this.SetProperty<string[]>(ref this._tagNames, value, nameof (TagNames));
  }

  protected override Task PreLoad()
  {
    return Task.WhenAll(base.PreLoad(), this.LoadFacetsAsync(), this.Offices.Initialize());
  }

  protected virtual async Task LoadFacetsAsync()
  {
    DepositoryDetailsViewModel detailsViewModel = this;
    Dictionary<string, Dictionary<string, int>> facets = await ((IRepositoryWithFacets<Depository>) detailsViewModel.Repository).GetFacets("TagNames");
    detailsViewModel.TagNames = facets["TagNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
  }

  public ICommand SelectOfficeCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectOfficeAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  private async Task OnSelectOfficeAsync()
  {
    DepositoryDetailsViewModel detailsViewModel = this;
    Depository depository = detailsViewModel.Details;
    depository.OfficeId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Office>, string, string>(detailsViewModel.Details.OfficeId);
    depository = (Depository) null;
  }
}
