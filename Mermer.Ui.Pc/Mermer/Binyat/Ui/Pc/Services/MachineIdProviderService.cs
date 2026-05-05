// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Services.MachineIdProviderService
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using Mermer.Ui.Core.Services;
using System;
using System.IO;
using System.Management;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Pc.Services;

public class MachineIdProviderService : IMachineIdProviderService
{
  private string _machineId;

  public Task<string> GetUniqueIdAsync()
  {
    return Task.Run<string>((Func<string>) (() =>
    {
      if (string.IsNullOrEmpty(this._machineId))
        this._machineId = this.GetUniqueId();
      return this._machineId;
    }));
  }

  private string GetUniqueId(string drive = "C")
  {
    if (string.IsNullOrEmpty(drive))
    {
      foreach (DriveInfo drive1 in DriveInfo.GetDrives())
      {
        if (drive1.IsReady)
        {
          drive = drive1.RootDirectory.ToString();
          break;
        }
      }
      if (string.IsNullOrEmpty(drive))
        throw new Exception("Could not find a drive");
    }
    if (drive.EndsWith(":\\"))
      drive = drive.Substring(0, drive.Length - 2);
    string volumeSerial = MachineIdProviderService.GetVolumeSerial(drive);
    string cpuid = MachineIdProviderService.GetCpuid();
    return cpuid.Substring(13) + cpuid.Substring(1, 4) + volumeSerial + cpuid.Substring(4, 4);
  }

  private static string GetVolumeSerial(string drive)
  {
    ManagementObject managementObject = new ManagementObject($"win32_logicaldisk.deviceid=\"{drive}:\"");
    managementObject.Get();
    string volumeSerial = managementObject["VolumeSerialNumber"].ToString();
    managementObject.Dispose();
    return volumeSerial;
  }

  private static string GetCpuid()
  {
    string cpuid = "";
    foreach (ManagementObject instance in new ManagementClass("win32_processor").GetInstances())
    {
      if (cpuid == "")
      {
        cpuid = instance.Properties["processorID"].Value.ToString();
        break;
      }
    }
    return cpuid;
  }
}
