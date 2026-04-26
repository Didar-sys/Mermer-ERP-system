// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Settings.BarcodeSettingsViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Common.Settings;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using Payhas.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Settings;

public class BarcodeSettingsViewModel : DialogViewModel
{
  private readonly IConfigurator _configurator;
  private BarcodeConfig _config;

  public BarcodeSettingsViewModel(
    IMvxMessenger messenger,
    IConfigurator configurator,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._configurator = configurator;
  }

  protected override async Task OnLoad()
  {
    this.Config = await this._configurator.GetConfigAsync<BarcodeConfig>();
  }

  public virtual BarcodeConfig Config
  {
    get => this._config;
    set => this.SetProperty<BarcodeConfig>(ref this._config, value, nameof (Config));
  }

  public ICommand SaveCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSaveAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task OnSaveAsync()
  {
    BarcodeSettingsViewModel settingsViewModel = this;
    settingsViewModel.IsBusy = true;
    try
    {
      await settingsViewModel._configurator.SetConfigAsync<BarcodeConfig>(settingsViewModel.Config);
      int num = await settingsViewModel.OnCloseAsync() ? 1 : 0;
    }
    catch (Exception ex)
    {
      settingsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    settingsViewModel.IsBusy = false;
  }
}
