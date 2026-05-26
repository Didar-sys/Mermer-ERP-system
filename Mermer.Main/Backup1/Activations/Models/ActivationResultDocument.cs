// Decompiled with JetBrains decompiler
// Type: Mermer.Activations.Models.ActivationResultDocument
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System.Collections.Generic;

#nullable disable
namespace Mermer.Activations.Models;

public class ActivationResultDocument
{
  public string Id { get; set; }

  public string DocType => this.GetType().Name;

  public List<ActivationResultItem> ActivationResults { get; set; }
}
