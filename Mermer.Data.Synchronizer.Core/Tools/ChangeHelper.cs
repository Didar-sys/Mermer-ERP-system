// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Synchronizer.Core.Tools.ChangeHelper
// Assembly: Mermer.Data.Synchronizer.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 51A01EA9-84E6-49FB-B6E1-8048825E2DB0
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Data.Synchronizer.Core.dll

using Mermer.Data.Synchronizer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Mermer.Data.Synchronizer.Core.Tools;

public class ChangeHelper : IChangeHelper
{
  public string GenerateChangeId(IChangeDocument changeDocument)
  {
    return this.GenerateChangeId(changeDocument.UserId, changeDocument.ServerId, changeDocument.PatchId);
  }

  public string GenerateChangeId(string userId, string serverId, int patchId)
  {
    return $"change:{userId}:{serverId}:{patchId:D10}";
  }

  public IEnumerable<string> GenerateChangeId(
    string userId,
    Dictionary<string, IEnumerable<ChangeIdsRange>> changesIndex)
  {
    return changesIndex.SelectMany((Func<KeyValuePair<string, IEnumerable<ChangeIdsRange>>, IEnumerable<ChangeIdsRange>>) (x => x.Value), (x, index) => new
    {
      serverId = x.Key,
      indexes = Enumerable.Range(index.Start, index.End - index.Start + 1)
    }).SelectMany(x => x.indexes, (x, index) => this.GenerateChangeId(userId, x.serverId, index));
  }

  public IEnumerable<(string start, string end)> GenerateChangeIdRanges(
    string userId,
    Dictionary<string, IEnumerable<ChangeIdsRange>> changesIndex)
  {
    return changesIndex.SelectMany<KeyValuePair<string, IEnumerable<ChangeIdsRange>>, ChangeIdsRange, (string, string)>((Func<KeyValuePair<string, IEnumerable<ChangeIdsRange>>, IEnumerable<ChangeIdsRange>>) (x => x.Value), (Func<KeyValuePair<string, IEnumerable<ChangeIdsRange>>, ChangeIdsRange, (string, string)>) ((x, index) => (this.GenerateChangeId(userId, x.Key, index.Start), this.GenerateChangeId(userId, x.Key, index.End))));
  }

  public int GetItemsCount(
    Dictionary<string, IEnumerable<ChangeIdsRange>> changesIndex)
  {
    return changesIndex.SelectMany<KeyValuePair<string, IEnumerable<ChangeIdsRange>>, ChangeIdsRange>((Func<KeyValuePair<string, IEnumerable<ChangeIdsRange>>, IEnumerable<ChangeIdsRange>>) (x => x.Value)).Sum<ChangeIdsRange>((Func<ChangeIdsRange, int>) (x => 1 + x.End - x.Start));
  }
}
