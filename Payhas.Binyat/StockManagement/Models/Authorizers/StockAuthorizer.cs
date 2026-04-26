// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.Authorizers.StockAuthorizer
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Common.Authorizers;
using System;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models.Authorizers;

public class StockAuthorizer(IAuthorizationService authService) : ListAuthorizer<Stock>(authService, (Enum) ListActions.StocksList)
{
}
