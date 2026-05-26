// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Settings.PrinterSettingsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Common.Settings;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.Services;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using Mermer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Settings;

public class PrinterSettingsViewModel : DialogViewModel
{
  private readonly IConfigurator _configurator;
  private readonly IPrintingService _printingService;
  private PrinterConfig _config;

  public PrinterSettingsViewModel(
    IMvxMessenger messenger,
    IConfigurator configurator,
    IPrintingService printingService,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._configurator = configurator;
    this._printingService = printingService;
  }

  public IEnumerable<ListHelper<string>> Printers { get; set; }

  public PrinterConfig Config
  {
    get => this._config;
    set => this.SetProperty<PrinterConfig>(ref this._config, value, nameof (Config));
  }

  protected override Task PreLoad()
  {
    this.Printers = this._printingService.GetPrinterNames().Select<string, ListHelper<string>>((Func<string, ListHelper<string>>) (x => new ListHelper<string>(x)));
    return base.PreLoad();
  }

  protected override async Task OnLoad()
  {
    this.Config = await this._configurator.GetConfigAsync<PrinterConfig>();
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
    PrinterSettingsViewModel settingsViewModel = this;
    settingsViewModel.IsBusy = true;
    try
    {
      await settingsViewModel._configurator.SetConfigAsync<PrinterConfig>(settingsViewModel.Config);
      int num = await settingsViewModel.OnCloseAsync() ? 1 : 0;
    }
    catch (Exception ex)
    {
      settingsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    settingsViewModel.IsBusy = false;
  }
}
