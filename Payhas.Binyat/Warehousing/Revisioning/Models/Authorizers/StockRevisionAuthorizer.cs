// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Revisioning.Models.Authorizers.StockRevisionAuthorizer
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Common.Services;
using Payhas.Binyat.Transactions.Models.Authorizers;
using Payhas.Data.Tools.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Warehousing.Revisioning.Models.Authorizers;

public class StockRevisionAuthorizer(
  ILoginService loginService,
  ILocalizationService localizationService,
  IAuthorizationService authService) : TransactionAuthorizer<StockRevision>(loginService, localizationService, authService, (Enum) TransactionActions.StockRevisions)
{
  protected override string[] GetAccessedAccounts(StockRevision item)
  {
    return new string[1]{ item.WarehouseId };
  }

  protected override Expression<Func<StockRevision, bool>> GetFilter(IEnumerable<string> accounts)
  {
    return Predicate.Create<StockRevision>((Expression<Func<StockRevision, bool>>) (x => accounts.Contains<string>(x.WarehouseId)));
  }
}
