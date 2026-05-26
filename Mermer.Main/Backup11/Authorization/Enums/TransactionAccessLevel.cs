// Decompiled with JetBrains decompiler
// Type: Mermer.Authorization.Enums.TransactionAccessLevel
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer.Authorization.Enums;

[Flags]
public enum TransactionAccessLevel
{
  None = 0,
  Create = 1,
  ReadOwn = 2,
  UpdateOwn = 6,
  DeleteOwn = 14, // 0x0000000E
  CompleteOwn = 16, // 0x00000010
  ReadAll = 34, // 0x00000022
  UpdateAll = 102, // 0x00000066
  DeleteAll = 238, // 0x000000EE
  CompleteAll = 272, // 0x00000110
}
