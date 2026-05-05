// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Patcher.IPatcher
// Assembly: Mermer.Data.Patcher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2AD11298-697F-4B7E-AC43-C662A1FFE782
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Data.Patcher.dll

using System.Collections.Generic;

#nullable disable
namespace Mermer.Data.Patcher;

public interface IPatcher
{
  Patch CreatePatch<T>(T source, T target, string id = null) where T : class;

  T ApplyPatch<T>(Patch patch, T target) where T : class;

  Patch CreatePatchForLeftPatch(Patch patch, List<Patch> laterPatches);
}
