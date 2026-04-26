// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.LoginViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Payhas.Binyat.Activations.Services;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Ui.Core.ViewModels.Settings;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels;

public class LoginViewModel : BaseViewModel
{
  private readonly ILoginService _loginService;
  private readonly IBinyatActivationService _activationService;
  private string _username;
  private string _password;

  public LoginViewModel(
    ILoginService loginService,
    IMvxNavigationService navigationService,
    IBinyatActivationService activationService,
    IUserInteractionService userInteractionService)
    : base(navigationService, userInteractionService)
  {
    this._loginService = loginService;
    this._activationService = activationService;
  }

  public string Username
  {
    get => this._username;
    set => this.SetProperty<string>(ref this._username, value, nameof (Username));
  }

  public string Password
  {
    get => this._password;
    set => this.SetProperty<string>(ref this._password, value, nameof (Password));
  }

  public ICommand LoginCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.LoginAsync), (Func<bool>) (() => !this.IsBusy && !string.IsNullOrEmpty(this.Username) && !string.IsNullOrEmpty(this.Password)));
    }
  }

  private async Task LoginAsync()
  {
    LoginViewModel loginViewModel = this;
    loginViewModel.IsBusy = true;
    try
    {
      await Task.WhenAll(loginViewModel._activationService.ValidateClientActivationAsync(), loginViewModel._activationService.ValidateServerActivationAsync());
      await loginViewModel._loginService.LoginAsync(loginViewModel.Username, loginViewModel.Password);
      await loginViewModel.NavigationService.Navigate<MainViewModel>();
    }
    catch (ApplicationException ex)
    {
      loginViewModel.UserInteractionService.ShowMessage(loginViewModel["Application is not activated", Array.Empty<object>()], loginViewModel["Please activate this copy of your application!", Array.Empty<object>()]);
    }
    catch (InvalidOperationException ex)
    {
      loginViewModel.UserInteractionService.ShowMessage(loginViewModel["Error Logging In", Array.Empty<object>()], loginViewModel["User not exists, or wrong password!", Array.Empty<object>()]);
    }
    catch (Exception ex)
    {
      loginViewModel.UserInteractionService.ShowExceptionMessage(ex, $"{loginViewModel["Error Logging In!", Array.Empty<object>()]} ({ex.GetType()})");
    }
    loginViewModel.IsBusy = false;
  }

  public ICommand ShowSettingsCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.ShowSettingsAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task ShowSettingsAsync()
  {
    await this.NavigationService.Navigate<ConnectionSettingsViewModel>();
  }

  public ICommand ShowActivationCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnShowActivationAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  public Task OnShowActivationAsync() => this.NavigationService.Navigate<ActivationViewModel>();
}
