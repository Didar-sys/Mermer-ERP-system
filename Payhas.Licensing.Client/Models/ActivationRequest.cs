// Decompiled with JetBrains decompiler
// Type: Payhas.Licensing.Client.Models.ActivationRequest
// Assembly: Payhas.Licensing.Client, Version=0.0.6.0, Culture=neutral, PublicKeyToken=null
// MVID: D27E04BC-87FA-488D-A2D4-54F1C56BAB05
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Licensing.Client.dll

#nullable disable
namespace Payhas.Licensing.Client.Models;

public class ActivationRequest
{
  public string LicenseId { get; set; }

  public string MachineId { get; set; }

  public string Note { get; set; }

  public string ApplicationId { get; set; }

  public string[] ApplicationModuleIds { get; set; }
}
