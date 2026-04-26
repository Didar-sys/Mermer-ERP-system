// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Patcher.Patch
// Assembly: Payhas.Data.Patcher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2AD11298-697F-4B7E-AC43-C662A1FFE782
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Data.Patcher.dll

using System.Collections.Generic;

#nullable disable
namespace Payhas.Data.Patcher;

public class Patch
{
  public string Id { get; set; }

  public PatchAction Action { get; set; }

  public Dictionary<string, object> PropertyPatches { get; set; }

  public Dictionary<string, List<Patch>> SubListPatches { get; set; }
}
