// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Authorization.ChangePasswordViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Authorization.Services;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Authorization;

public class ChangePasswordViewModel : DialogViewModel
{
  private readonly ILoginService _loginService;
  private string _currentPassword;
  private string _newPassword;
  private string _newPasswordRepeat;

  public ChangePasswordViewModel(
    IMvxMessenger messenger,
    ILoginService loginService,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._loginService = loginService;
  }

  public virtual string CurrentPassword
  {
    get => this._currentPassword;
    set => this.SetProperty<string>(ref this._currentPassword, value, nameof (CurrentPassword));
  }

  public virtual string NewPassword
  {
    get => this._newPassword;
    set => this.SetProperty<string>(ref this._newPassword, value, nameof (NewPassword));
  }

  public virtual string NewPasswordRepeat
  {
    get => this._newPasswordRepeat;
    set => this.SetProperty<string>(ref this._newPasswordRepeat, value, nameof (NewPasswordRepeat));
  }

  public ICommand ChangePasswordCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnChangePasswordCommandAsync), (Func<bool>) (() => !this.IsBusy && !string.IsNullOrEmpty(this.CurrentPassword) && !string.IsNullOrEmpty(this.NewPassword) && this.NewPassword == this.NewPasswordRepeat));
    }
  }

  private async Task OnChangePasswordCommandAsync()
  {
    ChangePasswordViewModel passwordViewModel = this;
    try
    {
      await passwordViewModel._loginService.UpdatePassword(passwordViewModel.CurrentPassword, passwordViewModel.NewPassword);
      int num = await passwordViewModel.OnCloseAsync() ? 1 : 0;
    }
    catch (Exception ex)
    {
      passwordViewModel.UserInteractionService.ShowExceptionMessage(ex, passwordViewModel["Password Could Not Be Updated", Array.Empty<object>()]);
    }
  }
}
