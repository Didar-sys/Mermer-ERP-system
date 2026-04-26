// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Models.Authorizers.StockSlipAuthorizer
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Common.Services;
using Payhas.Binyat.Transactions.Models.Authorizers;

#nullable disable
namespace Payhas.Binyat.Warehousing.Models.Authorizers;

public class StockSlipAuthorizer(
  ILoginService loginService,
  ILocalizationService localizationService,
  IAuthorizationService authService) : TransactionActionsAuthorizer<StockSlip, StockSlipType>(loginService, localizationService, authService)
{
  protected override string[] GetAccessedAccounts(StockSlip item)
  {
    return new string[1]{ item.WarehouseId };
  }
}
