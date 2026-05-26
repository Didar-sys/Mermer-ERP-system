// Decompiled with JetBrains decompiler
// Type: Mermer.StockManagement.Models.Authorizers.StockBalancesAuthorizer
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Common.Authorizers;
using Mermer.Data.Tools.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.StockManagement.Models.Authorizers;

public class StockBalancesAuthorizer(ILoginService loginService, IAuthorizationService authService) : 
  ReadOnlyListAuthorizerWithAccountFilter<StockBalanceWithData>(loginService, authService, (Enum) Actions.StockBalancesList)
{
  public override Enum GetReadAccessLevel() => (Enum) AccessLevel.Grant;

  protected override Expression<Func<StockBalanceWithData, bool>> GetFilter(
    IEnumerable<string> accounts)
  {
    return new SafePredicateBuilder<StockBalanceWithData>((Expression<Func<StockBalanceWithData, bool>>) (x => accounts.Contains<string>(x.WarehouseId))).Expressions[0];
  }
}
