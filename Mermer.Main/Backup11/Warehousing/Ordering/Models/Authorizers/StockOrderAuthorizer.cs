// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Ordering.Models.Authorizers.StockOrderAuthorizer
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Transactions.Models.Authorizers;
using Mermer.Data.Tools.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Warehousing.Ordering.Models.Authorizers;

public class StockOrderAuthorizer(
  ILoginService loginService,
  ILocalizationService localizationService,
  IAuthorizationService authService) : TransactionAuthorizer<StockOrder>(loginService, localizationService, authService, (Enum) TransactionActions.StockOrders)
{
  protected override string[] GetAccessedAccounts(StockOrder item)
  {
    return new string[1]{ item.WarehouseId };
  }

  protected override Expression<Func<StockOrder, bool>> GetFilter(IEnumerable<string> accounts)
  {
    return Predicate.Create<StockOrder>((Expression<Func<StockOrder, bool>>) (x => accounts.Contains<string>(x.WarehouseId)));
  }
}
