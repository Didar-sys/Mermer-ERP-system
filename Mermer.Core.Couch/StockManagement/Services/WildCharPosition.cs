// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.StockManagement.Services.WildCharPosition
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using System;

#nullable disable
namespace Mermer.Core.Couch.StockManagement.Services;

[Flags]
internal enum WildCharPosition
{
  None = 0,
  Start = 1,
  End = 2,
  Both = End | Start, // 0x00000003
}
