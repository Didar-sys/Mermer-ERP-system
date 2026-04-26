// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Finance.Spending.Services.ExpensesRepository
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using FluentValidation;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Common.Services;
using Payhas.Binyat.Core.Couch.Changes;
using Payhas.Binyat.Core.Couch.Changes.Services;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Binyat.Finance.Spending.Models;
using Payhas.Data.Authorizers;
using Payhas.Data.Patcher;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Finance.Spending.Services;

public class ExpensesRepository(
  ICouchCluster cluster,
  IValidator<Expense> validator,
  IListAuthorizer<Expense> authorizer,
  IDocumentChangeListener changeListener,
  ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService,
  IPatcher patcher,
  ILoginService loginService) : CouchRepositoryWithFacet<Expense>(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
{
  public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
  {
    return this.GetFacetsFromView("finance-spending", "expense-facets", fields);
  }
}
