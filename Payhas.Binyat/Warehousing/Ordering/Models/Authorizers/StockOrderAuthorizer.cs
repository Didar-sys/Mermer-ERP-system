// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Ordering.Models.Authorizers.StockOrderAuthorizer
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
namespace Payhas.Binyat.Warehousing.Ordering.Models.Authorizers;

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
