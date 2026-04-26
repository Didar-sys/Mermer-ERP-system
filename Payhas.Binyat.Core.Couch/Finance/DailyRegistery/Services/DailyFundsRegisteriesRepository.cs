// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Finance.DailyRegistery.Services.DailyFundsRegisteriesRepository
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using AutoMapper;
using FluentValidation;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Common.Services;
using Payhas.Binyat.Core.Couch.Changes;
using Payhas.Binyat.Core.Couch.Changes.Services;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Binyat.Finance.DailyRegistery.Models;
using Payhas.Binyat.Finance.DailyRegistery.Services;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.FundsManagement.Services;
using Payhas.Data.Authorizers;
using Payhas.Data.Patcher;
using Payhas.Data.Storage;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Finance.DailyRegistery.Services;

public class DailyFundsRegisteriesRepository : 
  CouchRepositoryWithFacet<DailyFundsRegistery>,
  IDailyFundsRegisteriesRepository,
  IRepository<DailyFundsRegistery>,
  IReadOnlyRepository<DailyFundsRegistery>
{
  private readonly IMapper _mapper;
  private readonly IFundsBalancesRepository _balancesRepository;
  private readonly IReadOnlyListAuthorizer<FundsBalance> _balanceAuthorizer;

  public DailyFundsRegisteriesRepository(
    IMapper mapper,
    ICouchCluster cluster,
    IValidator<DailyFundsRegistery> validator,
    IListAuthorizer<DailyFundsRegistery> authorizer,
    IFundsBalancesRepository balancesRepository,
    IReadOnlyListAuthorizer<FundsBalance> balanceAuthorizer,
    IDocumentChangeListener changeListener,
    ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService,
    IPatcher patcher,
    ILoginService loginService)
    : base(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
  {
    this._mapper = mapper;
    this._balancesRepository = balancesRepository;
    this._balanceAuthorizer = balanceAuthorizer;
  }

  public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
  {
    return this.GetFacetsFromView("transaction", "facets", fields);
  }

  async Task<IEnumerable<DailyFundsRegisteryInfo>> IDailyFundsRegisteriesRepository.GetAsync(
    params Expression<Func<DailyFundsRegistery, bool>>[] predicates)
  {
    DailyFundsRegisteriesRepository registeriesRepository = this;
    IEnumerable<DailyFundsRegistery> async1 = await registeriesRepository.GetAsync(predicates);
    List<DailyFundsRegisteryInfo> infos = registeriesRepository._mapper.Map<List<DailyFundsRegisteryInfo>>((object) async1);
    try
    {
      registeriesRepository._balanceAuthorizer.Authorize();
      foreach (DailyFundsRegisteryInfo info in infos)
        info.Computed = new Decimal?((await registeriesRepository._balancesRepository.GetBalanceToDateAsync(info.DepositoryId, info.Date)).Balance);
    }
    catch (Exception ex)
    {
    }
    IEnumerable<DailyFundsRegisteryInfo> async2 = (IEnumerable<DailyFundsRegisteryInfo>) infos;
    infos = (List<DailyFundsRegisteryInfo>) null;
    return async2;
  }
}
