// Decompiled with JetBrains decompiler
// Type: Mermer.Licensing.Client.Models.ActivationResult
// Assembly: Mermer.Licensing.Client, Version=0.0.6.0, Culture=neutral, PublicKeyToken=null
// MVID: D27E04BC-87FA-488D-A2D4-54F1C56BAB05
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Licensing.Client.dll

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

#nullable disable
namespace Mermer.Licensing.Client.Models;

public class ActivationResult
{
  public string MachineId { get; set; }

  public string ApplicationId { get; set; }

  public string[] ApplicationModuleIds { get; set; }

  public DateTime DateValidFrom { get; set; }

  public DateTime? DateValidTill { get; set; }

  public string Signature { get; set; }

  public override string ToString()
  {
    return JsonConvert.SerializeObject((object) new Dictionary<string, object>()
    {
      {
        "MachineId",
        (object) this.MachineId
      },
      {
        "ApplicationId",
        (object) this.ApplicationId
      },
      {
        "ApplicationModuleIds",
        (object) this.ApplicationModuleIds
      },
      {
        "DateValidFrom",
        (object) this.DateValidFrom
      },
      {
        "DateValidTill",
        (object) this.DateValidTill
      }
    });
  }
}
