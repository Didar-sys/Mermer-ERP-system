// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Activations.Models.ActivationResultItem
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Licensing.Client.Models;
using System;

#nullable disable
namespace Payhas.Binyat.Activations.Models;

public class ActivationResultItem : ActivationResult
{
  public ActivationResultItem() => this.Id = Guid.NewGuid().ToString();

  public ActivationResultItem(ActivationResult result)
    : this()
  {
    this.MachineId = result.MachineId;
    this.ApplicationId = result.ApplicationId;
    this.ApplicationModuleIds = result.ApplicationModuleIds;
    this.DateValidFrom = result.DateValidFrom;
    this.DateValidTill = result.DateValidTill;
    this.Signature = result.Signature;
  }

  public ActivationResultItem(ActivationResultItem resultItem)
    : this((ActivationResult) resultItem)
  {
    this.Id = resultItem.Id;
  }

  public string Id { get; set; }

  public string DocType => this.GetType().Name;
}
