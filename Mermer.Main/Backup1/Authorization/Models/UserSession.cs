// Decompiled with JetBrains decompiler
// Type: Mermer.Authorization.Models.UserSession
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Newtonsoft.Json;
using Mermer.Authorization.Enums;
using System.Collections.Generic;

#nullable disable
namespace Mermer.Authorization.Models;

public class UserSession
{
  [JsonProperty("sub")]
  public virtual string UserId { get; set; }

  [JsonProperty("name")]
  public virtual string Username { get; set; }

  [JsonProperty("admin")]
  public virtual bool IsAdmin { get; set; }

  public virtual Dictionary<string, AccountAccessLevel> Accounts { get; set; }

  public virtual Dictionary<string, int> Roles { get; set; }
}
