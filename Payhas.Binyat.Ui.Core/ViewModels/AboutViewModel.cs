// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.AboutViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Common.Settings;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using Payhas.Services;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels;

public class AboutViewModel : DialogViewModel
{
  private UpdateSettings _updateSettings;
  private readonly IConfigurator _configurator;
  private bool _checkForUpdates;

  public AboutViewModel(
    IMvxMessenger messenger,
    IConfigurator configurator,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._configurator = configurator;
  }

  public virtual bool CheckForUpdates
  {
    get => this._checkForUpdates;
    set
    {
      if (!this.SetProperty<bool>(ref this._checkForUpdates, value, nameof (CheckForUpdates)) || this.IsBusy)
        return;
      this._updateSettings.CheckForUpdates = this._checkForUpdates;
      this._configurator.SetConfig<UpdateSettings>(this._updateSettings);
    }
  }

  protected override Task OnLoad()
  {
    this._updateSettings = this._configurator.HasConfig<UpdateSettings>() ? this._configurator.GetConfig<UpdateSettings>() : new UpdateSettings();
    this.CheckForUpdates = this._updateSettings.CheckForUpdates;
    return base.OnLoad();
  }
}
