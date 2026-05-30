// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Synchronizer.Core.Models.ChangeIndex
// Assembly: Mermer.Data.Synchronizer.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 51A01EA9-84E6-49FB-B6E1-8048825E2DB0
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Data.Synchronizer.Core.dll

using System.Collections.Generic;

#nullable disable
namespace Mermer.Data.Synchronizer.Core.Models;

public class ChangeIndex
{
  public string UserId { get; set; }

  public string ServerId { get; set; }

  public IEnumerable<ChangeIdsRange> Ranges { get; set; }
}
