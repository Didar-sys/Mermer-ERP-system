// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Settings.ConnectionSettingsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;
using Mermer.Common.Services;
using Mermer.Common.Settings;
using Mermer.Ui.Core.Helpers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using Mermer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Mermer.Core.Couch.Common;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Settings;

public class ConnectionSettingsViewModel : DialogViewModel
{
  private readonly IConfigurator _configurator;
  private IEnumerable<ListHelper<int>> _connectionModes;
  private ConnectionSettings _config;

  public ConnectionSettingsViewModel(
    IMvxMessenger messenger,
    IConfigurator configurator,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._configurator = configurator;
    this.ConnectionModes = (IEnumerable<ListHelper<int>>) Enum.GetValues(typeof (ConnectionMode)).Cast<ConnectionMode>().Select<ConnectionMode, ListHelper<int>>((Func<ConnectionMode, ListHelper<int>>) (x => new ListHelper<int>()
    {
      Text = this[x.ToString(), Array.Empty<object>()],
      Value = (int) x
    })).ToArray<ListHelper<int>>();
  }

  public IEnumerable<ListHelper<int>> ConnectionModes
  {
    get => this._connectionModes;
    set
    {
      this.SetProperty<IEnumerable<ListHelper<int>>>(ref this._connectionModes, value, nameof (ConnectionModes));
    }
  }

  public virtual ConnectionSettings Config
  {
    get => this._config;
    set => this.SetProperty<ConnectionSettings>(ref this._config, value, nameof (Config));
  }

  protected override async Task OnLoad()
  {
    this.Config = await this._configurator.GetConfigAsync<ConnectionSettings>();
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
    ConnectionSettingsViewModel settingsViewModel = this;
    settingsViewModel.IsBusy = true;
    try
    {
      await settingsViewModel._configurator.SetConfigAsync<ConnectionSettings>(settingsViewModel.Config);
      Mvx.Resolve<ICouchCluster>().Initialize(settingsViewModel.Config.DatabaseAddress, settingsViewModel.Config.DatabaseName, settingsViewModel.Config.DatabaseUser, settingsViewModel.Config.DatabasePassword);
      int num = await settingsViewModel.OnCloseAsync() ? 1 : 0;
    }
    catch (Exception ex)
    {
      settingsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    settingsViewModel.IsBusy = false;
  }

  public ICommand CreateInitialDataCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateInitialDataAsync), (Func<bool>) (() => !this.IsBusy && this.Config != null && this.Config.IsDirectModeSelected && !string.IsNullOrEmpty(this.Config.DatabaseAddress) && !string.IsNullOrEmpty(this.Config.DatabaseName)));
    }
  }

  private async Task OnCreateInitialDataAsync()
  {
    ConnectionSettingsViewModel settingsViewModel = this;
    settingsViewModel.IsBusy = true;
    try
    {
      foreach (IInitialDataCreator initialDataCreator in Mvx.Resolve<IEnumerable<IInitialDataCreator>>())
        await initialDataCreator.CreateAsync();
    }
    catch (Exception ex)
    {
      settingsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    settingsViewModel.IsBusy = false;
  }

  public ICommand CreateInitialSchemaCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateInitialSchemaAsync), (Func<bool>) (() => !this.IsBusy && this.Config != null && this.Config.IsDirectModeSelected && !string.IsNullOrEmpty(this.Config.DatabaseAddress) && !string.IsNullOrEmpty(this.Config.DatabaseName)));
    }
  }

  private async Task OnCreateInitialSchemaAsync()
  {
    ConnectionSettingsViewModel settingsViewModel = this;
    settingsViewModel.IsBusy = true;
    try
    {
      bool changeListenerWasStarted = false;
      IDocumentChangeListener changeListener;
      if (Mvx.TryResolve<IDocumentChangeListener>(out changeListener))
      {
        changeListenerWasStarted = changeListener.Started;
        changeListener.Stop();
      }
      foreach (IInitialSchemaCreator initialSchemaCreator in Mvx.Resolve<IEnumerable<IInitialSchemaCreator>>())
        await initialSchemaCreator.CreateAsync(settingsViewModel.Config.AllowReporting);
      if (changeListenerWasStarted)
        changeListener.Start();
      changeListener = (IDocumentChangeListener) null;
    }
    catch (Exception ex)
    {
      settingsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    settingsViewModel.IsBusy = false;
  }
}
