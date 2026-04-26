// Decompiled with JetBrains decompiler
// Type: Payhas.Mvvm.Services.IUserInteractionService
// Assembly: Payhas.Mvvm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3EAA5570-F618-4E39-B929-F7374F99B43D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Mvvm.dll

using System;

#nullable disable
namespace Payhas.Mvvm.Services;

public interface IUserInteractionService
{
  void ShowExceptionMessage(Exception exception, string caption = null);

  bool? ShowMessage(string caption, string message, UserInteractionType type = UserInteractionType.Ok);
}
