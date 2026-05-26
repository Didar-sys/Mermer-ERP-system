// Decompiled with JetBrains decompiler
// Type: Mermer.Mvvm.Services.UserInteractionType
// Assembly: Mermer.Mvvm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3EAA5570-F618-4E39-B929-F7374F99B43D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Mvvm.dll

using System;

#nullable disable
namespace Mermer.Mvvm.Services;

[Flags]
public enum UserInteractionType
{
  Ok = 0,
  YesNo = 1,
  YesNoCancel = 3,
}
