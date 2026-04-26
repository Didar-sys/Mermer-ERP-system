// Decompiled with JetBrains decompiler
// Type: Payhas.Mvvm.ViewModels.DialogViewModel
// Assembly: Payhas.Mvvm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3EAA5570-F618-4E39-B929-F7374F99B43D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Mvvm.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Payhas.Mvvm.Services;

#nullable disable
namespace Payhas.Mvvm.ViewModels;

public class DialogViewModel : BaseViewModel, IDialogViewModel
{
  protected readonly IMvxMessenger Messenger;

  protected DialogViewModel(
    IMvxMessenger messenger,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(navigationService, userInteractionService)
  {
    this.Messenger = messenger;
  }
}
