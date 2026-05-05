// Decompiled with JetBrains decompiler
// Type: Mermer.Commerce.Models.Authorizers.BillAuthorizer
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Transactions.Models.Authorizers;

#nullable disable
namespace Mermer.Commerce.Models.Authorizers;

public class BillAuthorizer(
  ILoginService loginService,
  ILocalizationService localizationService,
  IAuthorizationService authService) : TransactionActionsAuthorizer<Bill, BillType>(loginService, localizationService, authService)
{
  protected override string[] GetAccessedAccounts(Bill item)
  {
    return new string[1]{ item.DepositoryId };
  }
}
