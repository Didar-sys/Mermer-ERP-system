// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Synchronizer.Core.Tools.IChangeHelper
// Assembly: Payhas.Data.Synchronizer.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 51A01EA9-84E6-49FB-B6E1-8048825E2DB0
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Data.Synchronizer.Core.dll

using Payhas.Data.Synchronizer.Core.Models;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Data.Synchronizer.Core.Tools;

public interface IChangeHelper
{
  string GenerateChangeId(IChangeDocument changeDocument);

  string GenerateChangeId(string userId, string serverId, int patchId);

  IEnumerable<string> GenerateChangeId(
    string userId,
    Dictionary<string, IEnumerable<ChangeIdsRange>> changesIndex);

  IEnumerable<(string start, string end)> GenerateChangeIdRanges(
    string userId,
    Dictionary<string, IEnumerable<ChangeIdsRange>> changesIndex);

  int GetItemsCount(
    Dictionary<string, IEnumerable<ChangeIdsRange>> changesIndex);
}
