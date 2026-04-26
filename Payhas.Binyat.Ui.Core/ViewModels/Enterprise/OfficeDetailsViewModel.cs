// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Enterprise.OfficeDetailsViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Enterprise;

public class OfficeDetailsViewModel(
  IRepositoryWithFacets<Office> repository,
  IListAuthorizer<Office> authorizer,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : DetailsViewModel<Office>((IRepository<Office>) repository, authorizer, navigationService, userInteractionService)
{
  private string[] _regionNames;
  private string[] _tagNames;

  public virtual string[] RegionNames
  {
    get => this._regionNames;
    set => this.SetProperty<string[]>(ref this._regionNames, value, nameof (RegionNames));
  }

  public virtual string[] TagNames
  {
    get => this._tagNames;
    set => this.SetProperty<string[]>(ref this._tagNames, value, nameof (TagNames));
  }

  protected override Task PreLoad() => Task.WhenAll(base.PreLoad(), this.LoadFacetsAsync());

  protected virtual async Task LoadFacetsAsync()
  {
    OfficeDetailsViewModel detailsViewModel = this;
    Dictionary<string, Dictionary<string, int>> facets = await ((IRepositoryWithFacets<Office>) detailsViewModel.Repository).GetFacets("RegionNames", "TagNames");
    detailsViewModel.RegionNames = facets["RegionNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
    detailsViewModel.TagNames = facets["TagNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
  }
}
