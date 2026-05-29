using Mermer.Ui.Core.Services;
using Payhas.Binyat.Ui.Core.Services;
using System;
using System.IO;
using System.Management;
using System.Threading.Tasks;

namespace Mermer.Ui.Pc.Services;

public class MachineIdProviderService : IMachineIdProviderService
{
    private string _machineId;

    public Task<string> GetUniqueIdAsync()
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrEmpty(_machineId))
                _machineId = GetUniqueId();
            return _machineId;
        });
    }

    private string GetUniqueId(string drive = "C")
    {
        if (string.IsNullOrEmpty(drive))
        {
            foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
            {
                if (driveInfo.IsReady)
                {
                    drive = driveInfo.RootDirectory.ToString();
                    break;
                }
            }
            if (string.IsNullOrEmpty(drive))
                throw new Exception("Could not find a drive");
        }

        if (drive.EndsWith(":\\"))
            drive = drive.Substring(0, drive.Length - 2);

        string volumeSerial = GetVolumeSerial(drive);
        string cpuid = GetCpuid();

        return cpuid.Substring(13) + cpuid.Substring(1, 4) + volumeSerial + cpuid.Substring(4, 4);
    }

    private static string GetVolumeSerial(string drive)
    {
        using (ManagementObject managementObject = new ManagementObject($"win32_logicaldisk.deviceid=\"{drive}:\""))
        {
            managementObject.Get();
            return managementObject["VolumeSerialNumber"].ToString();
        }
    }

    private static string GetCpuid()
    {
        string cpuid = "";
        using (ManagementClass mc = new ManagementClass("win32_processor"))
        {
            foreach (ManagementObject instance in mc.GetInstances())
            {
                if (cpuid == "")
                {
                    cpuid = instance.Properties["processorID"].Value.ToString();
                    instance.Dispose();
                    break;
                }
                instance.Dispose();
            }
        }
        return cpuid;
    }
}