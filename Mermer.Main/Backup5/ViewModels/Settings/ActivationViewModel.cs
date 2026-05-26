// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Settings.ActivationViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Activations.Models;
using Mermer.Activations.Services;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Settings;

public class ActivationViewModel : DialogViewModel
{
  private readonly IBinyatActivationService _activationService;
  private string _note;
  private string _clientLicenseId;
  private ActivationStatus _clientActivationStatus;
  private string _serverLicenseId;
  private ActivationStatus _serverActivationStatus;

  public ActivationViewModel(
    IMvxMessenger messenger,
    IMvxNavigationService navigationService,
    IBinyatActivationService activationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._activationService = activationService;
  }

  protected override async Task OnLoad()
  {
    await base.OnLoad();
    await Task.WhenAll(this.OnUpdateClientStatusAsync(), this.OnUpdateServerStatusAsync());
  }

  public virtual string Note
  {
    get => this._note;
    set => this.SetProperty<string>(ref this._note, value, nameof (Note));
  }

  public virtual string ClientLicenseId
  {
    get => this._clientLicenseId;
    set => this.SetProperty<string>(ref this._clientLicenseId, value, nameof (ClientLicenseId));
  }

  public ActivationStatus ClientActivationStatus
  {
    get => this._clientActivationStatus;
    set
    {
      this.SetProperty<ActivationStatus>(ref this._clientActivationStatus, value, nameof (ClientActivationStatus));
    }
  }

  public ICommand UpdateClientStatusCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnUpdateClientStatusAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task OnUpdateClientStatusAsync()
  {
    ActivationViewModel activationViewModel = this;
    activationViewModel.IsBusy = true;
    try
    {
      ActivationStatus activeDatesAsync = await activationViewModel._activationService.GetClientActiveDatesAsync();
      activationViewModel.ClientActivationStatus = activeDatesAsync;
    }
    catch (Exception ex)
    {
      activationViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    activationViewModel.IsBusy = false;
  }

  public ICommand ActivateClientCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnActivateClientAsync), (Func<bool>) (() => !this.IsBusy && !string.IsNullOrEmpty(this.ClientLicenseId)));
    }
  }

  public virtual async Task OnActivateClientAsync()
  {
    ActivationViewModel activationViewModel = this;
    activationViewModel.IsBusy = true;
    try
    {
      await activationViewModel._activationService.ActivateClientAsync(activationViewModel.ClientLicenseId, activationViewModel.Note);
    }
    catch (Exception ex)
    {
      activationViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    activationViewModel.IsBusy = false;
    activationViewModel.UpdateClientStatusCommand.Execute((object) null);
  }

  public ICommand ReactivateClientCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnReactivateClientAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task OnReactivateClientAsync()
  {
    ActivationViewModel activationViewModel = this;
    activationViewModel.IsBusy = true;
    try
    {
      await activationViewModel._activationService.ReactivateClientAsync();
    }
    catch (Exception ex)
    {
      activationViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    activationViewModel.IsBusy = false;
    activationViewModel.UpdateClientStatusCommand.Execute((object) null);
  }

  public ICommand DeactivateClientCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnDeactivateClientAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task OnDeactivateClientAsync()
  {
    ActivationViewModel activationViewModel = this;
    activationViewModel.IsBusy = true;
    try
    {
      await activationViewModel._activationService.DeactivateClientAsync();
    }
    catch (Exception ex)
    {
      activationViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    activationViewModel.IsBusy = false;
    activationViewModel.UpdateClientStatusCommand.Execute((object) null);
  }

  public string ServerLicenseId
  {
    get => this._serverLicenseId;
    set => this.SetProperty<string>(ref this._serverLicenseId, value, nameof (ServerLicenseId));
  }

  public ActivationStatus ServerActivationStatus
  {
    get => this._serverActivationStatus;
    set
    {
      this.SetProperty<ActivationStatus>(ref this._serverActivationStatus, value, nameof (ServerActivationStatus));
    }
  }

  public ICommand UpdateServerStatusCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnUpdateServerStatusAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task OnUpdateServerStatusAsync()
  {
    ActivationViewModel activationViewModel = this;
    activationViewModel.IsBusy = true;
    try
    {
      ActivationStatus activeDatesAsync = await activationViewModel._activationService.GetServerActiveDatesAsync();
      activationViewModel.ServerActivationStatus = activeDatesAsync;
    }
    catch (Exception ex)
    {
      activationViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    activationViewModel.IsBusy = false;
  }

  public ICommand ActivateServerCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnActivateServerAsync), (Func<bool>) (() => !this.IsBusy && !string.IsNullOrEmpty(this.ServerLicenseId)));
    }
  }

  public virtual async Task OnActivateServerAsync()
  {
    ActivationViewModel activationViewModel = this;
    activationViewModel.IsBusy = true;
    try
    {
      await activationViewModel._activationService.ActivateServerAsync(activationViewModel.ServerLicenseId, activationViewModel.Note);
    }
    catch (Exception ex)
    {
      activationViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    activationViewModel.IsBusy = false;
    activationViewModel.UpdateServerStatusCommand.Execute((object) null);
  }

  public ICommand ReactivateServerCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnReactivateServerAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task OnReactivateServerAsync()
  {
    ActivationViewModel activationViewModel = this;
    activationViewModel.IsBusy = true;
    try
    {
      await activationViewModel._activationService.ReactivateServerAsync();
    }
    catch (Exception ex)
    {
      activationViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    activationViewModel.IsBusy = false;
    activationViewModel.UpdateServerStatusCommand.Execute((object) null);
  }

  public ICommand DeactivateServerCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnDeactivateServerAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task OnDeactivateServerAsync()
  {
    ActivationViewModel activationViewModel = this;
    activationViewModel.IsBusy = true;
    try
    {
      await activationViewModel._activationService.DeactivateServerAsync();
    }
    catch (Exception ex)
    {
      activationViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    activationViewModel.IsBusy = false;
    activationViewModel.UpdateServerStatusCommand.Execute((object) null);
  }
}
