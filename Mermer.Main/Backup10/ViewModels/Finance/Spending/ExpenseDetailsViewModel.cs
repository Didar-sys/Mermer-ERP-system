// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Finance.Spending.ExpenseDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using Mermer.Finance.Spending.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Finance.Spending;

public class ExpenseDetailsViewModel(
  IRepository<Expense> repository,
  IListAuthorizer<Expense> authorizer,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : DetailsViewModel<Expense>(repository, authorizer, navigationService, userInteractionService)
{
  private string[] _typeNames;
  private string[] _groupNames;
  private string[] _tagNames;

  public virtual string[] TypeNames
  {
    get => this._typeNames;
    set => this.SetProperty<string[]>(ref this._typeNames, value, nameof (TypeNames));
  }

  public virtual string[] GroupNames
  {
    get => this._groupNames;
    set => this.SetProperty<string[]>(ref this._groupNames, value, nameof (GroupNames));
  }

  public virtual string[] TagNames
  {
    get => this._tagNames;
    set => this.SetProperty<string[]>(ref this._tagNames, value, nameof (TagNames));
  }

  protected override Task PreLoad() => Task.WhenAll(base.PreLoad(), this.LoadFacetsAsync());

  protected virtual async Task LoadFacetsAsync()
  {
    ExpenseDetailsViewModel detailsViewModel = this;
    Dictionary<string, Dictionary<string, int>> facets = await ((IRepositoryWithFacets<Expense>) detailsViewModel.Repository).GetFacets("TypeNames", "GroupNames", "TagNames");
    detailsViewModel.TypeNames = facets["TypeNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
    detailsViewModel.GroupNames = facets["GroupNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
    detailsViewModel.TagNames = facets["TagNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
  }
}
