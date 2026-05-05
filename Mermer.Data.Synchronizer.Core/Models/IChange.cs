// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Synchronizer.Core.Models.IChange
// Assembly: Mermer.Data.Synchronizer.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 51A01EA9-84E6-49FB-B6E1-8048825E2DB0
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Data.Synchronizer.Core.dll

using System;

#nullable disable
namespace Mermer.Data.Synchronizer.Core.Models;

public interface IChange
{
  string ServerId { get; set; }

  DateTime PatchDate { get; set; }

  int PatchId { get; set; }
}
