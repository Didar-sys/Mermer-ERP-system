// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Warehousing.Services.StockTransfersRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using FluentValidation;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Common.Settings;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.Warehousing.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using Mermer.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Warehousing.Services;

public class StockTransfersRepository : CouchRepositoryWithFacet<StockTransfer>
{
  private readonly IConfigurator _configurator;
  private readonly IAuthorizationService _authService;

  public StockTransfersRepository(
    ICouchCluster cluster,
    IValidator<StockTransfer> validator,
    IConfigurator configurator,
    IAuthorizationService authService,
    IListAuthorizer<StockTransfer> authorizer,
    IDocumentChangeListener changeListener,
    ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService,
    IPatcher patcher,
    ILoginService loginService)
    : base(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
  {
    this._configurator = configurator;
    this._authService = authService;
  }

  public override async Task ValidateAsync(StockTransfer model)
  {
    StockTransfersRepository transfersRepository = this;
    AppSettings configAsync = await transfersRepository._configurator.GetConfigAsync<AppSettings>();
    // ISSUE: reference to a compiler-generated method
    await transfersRepository.Validator.AssertValidAsync<StockTransfer>(model, new Action<ValidationContext<StockTransfer>>(transfersRepository.\u003CValidateAsync\u003Eb__3_0));
  }

  public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
  {
    return this.GetFacetsFromView("transaction", "facets", fields);
  }
}
