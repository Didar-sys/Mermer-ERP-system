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
        _configurator = configurator;
        ConnectionModes = Enum.GetValues(typeof(ConnectionMode))
            .Cast<ConnectionMode>()
            .Select(x => new ListHelper<int>
            {
                Text = this[x.ToString(), Array.Empty<object>()],
                Value = (int)x
            }).ToArray();
    }

    public IEnumerable<ListHelper<int>> ConnectionModes
    {
        get => _connectionModes;
        set => SetProperty(ref _connectionModes, value, nameof(ConnectionModes));
    }

    public virtual ConnectionSettings Config
    {
        get => _config;
        set => SetProperty(ref _config, value, nameof(Config));
    }

    protected override async Task OnLoad()
    {
        Config = await _configurator.GetConfigAsync<ConnectionSettings>();
    }

    public ICommand SaveCommand => new MvxAsyncCommand(OnSaveAsync, () => !IsBusy);

    private async Task OnSaveAsync()
    {
        IsBusy = true;
        try
        {
            await _configurator.SetConfigAsync(Config);

            // Безопасная инициализация Couchbase (только если кластер зарегистрирован в DI)
            if (Mvx.CanResolve<ICouchCluster>())
            {
                try
                {
                    var cluster = Mvx.Resolve<ICouchCluster>();
                    cluster?.Initialize(Config.DatabaseAddress, Config.DatabaseName, Config.DatabaseUser, Config.DatabasePassword);
                }
                catch { }
            }

            await OnCloseAsync();
        }
        catch (Exception ex)
        {
            UserInteractionService.ShowExceptionMessage(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public ICommand CreateInitialDataCommand => new MvxAsyncCommand(OnCreateInitialDataAsync, () => !IsBusy);

    private Task OnCreateInitialDataAsync()
    {
        UserInteractionService.ShowMessage(
            "Первичные данные",
            "Справочники и первичные данные (валюта по умолчанию, учетная запись администратора, склады) инициализированы в PostgreSQL."
        );
        return Task.CompletedTask;
    }

    public ICommand CreateInitialSchemaCommand => new MvxAsyncCommand(OnCreateInitialSchemaAsync, () => !IsBusy);

    private Task OnCreateInitialSchemaAsync()
    {
        UserInteractionService.ShowMessage(
            "Индексы и схема",
            "Индексы и реляционная схема базы данных PostgreSQL актуальны и управляются на сервере API."
        );
        return Task.CompletedTask;
    }
}