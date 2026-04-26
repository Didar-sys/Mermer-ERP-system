// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.CRM.PartnerDetailsViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using Payhas.Binyat.CRM.Models;
using Payhas.Binyat.CRM.Services;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.CRM;

public class PartnerDetailsViewModel : DetailsViewModel<Partner>
{
  private readonly IPartnerCodeGenerationService _codeGenerator;
  private string[] _groupNames;
  private string[] _tagNames;

  public PartnerDetailsViewModel(
    Reference<Currency> currencies,
    IRepository<Partner> repository,
    IListAuthorizer<Partner> authorizer,
    IMvxNavigationService navigationService,
    IPartnerCodeGenerationService codeGenerator,
    IUserInteractionService userInteractionService)
    : base(repository, authorizer, navigationService, userInteractionService)
  {
    this._codeGenerator = codeGenerator;
    this.Currencies = currencies;
  }

  public Reference<Currency> Currencies { get; set; }

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

  protected virtual async Task LoadFacetsAsync()
  {
    PartnerDetailsViewModel detailsViewModel = this;
    Dictionary<string, Dictionary<string, int>> facets = await ((IRepositoryWithFacets<Partner>) detailsViewModel.Repository).GetFacets("GroupNames", "TagNames");
    detailsViewModel.GroupNames = facets["GroupNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
    detailsViewModel.TagNames = facets["TagNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
  }

  protected override Task PreLoad()
  {
    return Task.WhenAll(base.PreLoad(), this.LoadFacetsAsync(), this.Currencies.Initialize());
  }

  protected override async Task PostLoad()
  {
    PartnerDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    if (!string.IsNullOrEmpty(detailsViewModel.Details.Code))
      return;
    Partner partner = detailsViewModel.Details;
    partner.Code = await detailsViewModel._codeGenerator.GetNextCode();
    partner = (Partner) null;
  }
}
