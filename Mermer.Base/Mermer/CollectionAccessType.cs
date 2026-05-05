// Decompiled with JetBrains decompiler
// Type: Mermer.CollectionAccessType
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer;

[Flags]
public enum CollectionAccessType
{
  None = 0,
  Read = 1,
  ModifyExistingContent = 2,
  UpdatedContent = 6,
}
