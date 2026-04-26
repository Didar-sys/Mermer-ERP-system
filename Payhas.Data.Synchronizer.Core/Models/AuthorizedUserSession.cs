// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Synchronizer.Core.Models.AuthorizedUserSession
// Assembly: Payhas.Data.Synchronizer.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 51A01EA9-84E6-49FB-B6E1-8048825E2DB0
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Data.Synchronizer.Core.dll

#nullable disable
namespace Payhas.Data.Synchronizer.Core.Models;

public class AuthorizedUserSession
{
  public AuthenticatedUser User { get; set; }

  public string Token { get; set; }
}
