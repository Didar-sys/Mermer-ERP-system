// Decompiled with JetBrains decompiler
// Type: Mermer.StockManagement.Models.Authorizers.StockAuthorizer
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Common.Authorizers;
using System;

#nullable disable
namespace Mermer.StockManagement.Models.Authorizers;

public class StockAuthorizer(IAuthorizationService authService) : ListAuthorizer<Stock>(authService, (Enum) ListActions.StocksList)
{
}
