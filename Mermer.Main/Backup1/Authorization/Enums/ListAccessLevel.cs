// Decompiled with JetBrains decompiler
// Type: Mermer.Authorization.Enums.ListAccessLevel
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer.Authorization.Enums;

[Flags]
public enum ListAccessLevel
{
  None = 0,
  Create = 1,
  Read = 2,
  Update = 6,
  Delete = 14, // 0x0000000E
}
