// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Services.AppUpdaterService
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using NuGet;
using Squirrel;
using System;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Services;

public static class AppUpdaterService
{
  private static readonly string UpdateUrl = "https://binyat.payhas.com/downloads/binyat/1.4";

  public static bool UpdateAvailable { get; private set; }

  public static SemanticVersion UpdateVersion { get; private set; }

  public static async Task<bool?> CheckForUpdatesAsync()
  {
    try
    {
      using (UpdateManager mgr = new UpdateManager(AppUpdaterService.UpdateUrl))
      {
        UpdateInfo updateInfo = await mgr.CheckForUpdate(false, (Action<int>) null);
        if (updateInfo.CurrentlyInstalledVersion.Version != updateInfo.FutureReleaseEntry.Version)
        {
          AppUpdaterService.UpdateAvailable = true;
          AppUpdaterService.UpdateVersion = updateInfo.FutureReleaseEntry.Version;
          return new bool?(true);
        }
      }
      return new bool?(false);
    }
    catch (Exception ex)
    {
      return new bool?();
    }
  }

  public static async Task<ReleaseEntry> UpdateAsync()
  {
    try
    {
      using (UpdateManager mgr = new UpdateManager(AppUpdaterService.UpdateUrl))
        return await mgr.UpdateApp();
    }
    catch (Exception ex)
    {
      return (ReleaseEntry) null;
    }
  }
}
