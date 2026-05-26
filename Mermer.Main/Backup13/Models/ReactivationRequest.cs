// Decompiled with JetBrains decompiler
// Type: Mermer.Licensing.Client.Models.ReactivationRequest
// Assembly: Mermer.Licensing.Client, Version=0.0.6.0, Culture=neutral, PublicKeyToken=null
// MVID: D27E04BC-87FA-488D-A2D4-54F1C56BAB05
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Licensing.Client.dll

#nullable disable
namespace Mermer.Licensing.Client.Models;

public class ReactivationRequest
{
  public string MachineId { get; set; }

  public string ApplicationId { get; set; }

  public string[] ApplicationModuleIds { get; set; }
}
