// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Helpers.MvxWpfCommandHelper
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using MvvmCross.Core.ViewModels;
using System;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Helpers;

public class MvxWpfCommandHelper : IMvxCommandHelper
{
  public event EventHandler CanExecuteChanged
  {
    add => CommandManager.RequerySuggested += value;
    remove => CommandManager.RequerySuggested -= value;
  }

  public void RaiseCanExecuteChanged(object sender) => CommandManager.InvalidateRequerySuggested();
}
