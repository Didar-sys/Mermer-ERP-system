// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Synchronizer.Core.Tools.ChangesIndexHelper
// Assembly: Mermer.Data.Synchronizer.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 51A01EA9-84E6-49FB-B6E1-8048825E2DB0
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Data.Synchronizer.Core.dll

using Mermer.Data.Synchronizer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Data.Synchronizer.Core.Tools;

public class ChangesIndexHelper : IChangesIndexHelper
{
  public async Task<(Dictionary<string, IEnumerable<ChangeIdsRange>> sourceDiff, Dictionary<string, IEnumerable<ChangeIdsRange>> targetDiff)> GetDiffAsync(
    Dictionary<string, IEnumerable<ChangeIdsRange>> source,
    Dictionary<string, IEnumerable<ChangeIdsRange>> target)
  {
    Dictionary<string, IEnumerable<ChangeIdsRange>> sourceDiff = (Dictionary<string, IEnumerable<ChangeIdsRange>>) null;
    Dictionary<string, IEnumerable<ChangeIdsRange>> targetDiff = (Dictionary<string, IEnumerable<ChangeIdsRange>>) null;
    await Task.WhenAll(Task.Run((Func<Task>) (async () => sourceDiff = await this.GetUniqueItemsInDictionaryAsync(source, target))), Task.Run((Func<Task>) (async () => targetDiff = await this.GetUniqueItemsInDictionaryAsync(target, source))));
    return (sourceDiff, targetDiff);
  }

  private async Task<Dictionary<string, IEnumerable<ChangeIdsRange>>> GetUniqueItemsInDictionaryAsync(
    Dictionary<string, IEnumerable<ChangeIdsRange>> source,
    Dictionary<string, IEnumerable<ChangeIdsRange>> comparedTo)
  {
    if (source == null || !source.Any<KeyValuePair<string, IEnumerable<ChangeIdsRange>>>())
      return new Dictionary<string, IEnumerable<ChangeIdsRange>>();
    if (comparedTo == null || !comparedTo.Any<KeyValuePair<string, IEnumerable<ChangeIdsRange>>>())
      return source;
    Dictionary<string, IEnumerable<ChangeIdsRange>> diff = new Dictionary<string, IEnumerable<ChangeIdsRange>>();
    SemaphoreSlim diffLock = new SemaphoreSlim(1, 1);
    await Task.WhenAll(source.Select<KeyValuePair<string, IEnumerable<ChangeIdsRange>>, Task>((Func<KeyValuePair<string, IEnumerable<ChangeIdsRange>>, Task>) (x => Task.Run((Func<Task>) (async () =>
    {
      IEnumerable<ChangeIdsRange> uniqueItemsAsync = await this.GetUniqueItemsAsync(x.Value, comparedTo.ContainsKey(x.Key) ? comparedTo[x.Key] : (IEnumerable<ChangeIdsRange>) null);
      List<ChangeIdsRange> uniqueItems = uniqueItemsAsync != null ? uniqueItemsAsync.ToList<ChangeIdsRange>() : (List<ChangeIdsRange>) null;
      if (uniqueItems == null || !uniqueItems.Any<ChangeIdsRange>())
        return;
      await diffLock.WaitAsync();
      diff.Add(x.Key, (IEnumerable<ChangeIdsRange>) uniqueItems);
      diffLock.Release();
    })))));
    return diff;
  }

  private async Task<IEnumerable<ChangeIdsRange>> GetUniqueItemsAsync(
    IEnumerable<ChangeIdsRange> source,
    IEnumerable<ChangeIdsRange> comparedTo)
  {
    IEnumerable<ChangeIdsRange> source1 = source;
    List<ChangeIdsRange> list = source1 != null ? source1.ToList<ChangeIdsRange>() : (List<ChangeIdsRange>) null;
    IEnumerable<ChangeIdsRange> source2 = comparedTo;
    List<ChangeIdsRange> comparedList = source2 != null ? source2.ToList<ChangeIdsRange>() : (List<ChangeIdsRange>) null;
    if (list == null || !list.Any<ChangeIdsRange>())
      return (IEnumerable<ChangeIdsRange>) new List<ChangeIdsRange>();
    if (comparedList == null || !comparedList.Any<ChangeIdsRange>())
      return (IEnumerable<ChangeIdsRange>) list;
    List<ChangeIdsRange> diff = new List<ChangeIdsRange>();
    SemaphoreSlim diffLock = new SemaphoreSlim(1, 1);
    await Task.WhenAll(list.Select<ChangeIdsRange, Task>((Func<ChangeIdsRange, Task>) (sourceIndex => Task.Run((Func<Task>) (async () =>
    {
      IOrderedEnumerable<ChangeIdsRange> orderedEnumerable = comparedList.Where<ChangeIdsRange>((Func<ChangeIdsRange, bool>) (comparedIndex => comparedIndex.Start <= sourceIndex.End && comparedIndex.End >= sourceIndex.Start)).OrderBy<ChangeIdsRange, int>((Func<ChangeIdsRange, int>) (comparedIndex => comparedIndex.Start));
      int lastIndex = sourceIndex.Start;
      foreach (ChangeIdsRange changeIdsRange in (IEnumerable<ChangeIdsRange>) orderedEnumerable)
      {
        ChangeIdsRange comparedIndex = changeIdsRange;
        if (comparedIndex.Start > lastIndex)
        {
          await diffLock.WaitAsync();
          diff.Add(new ChangeIdsRange(lastIndex, comparedIndex.Start - 1));
          diffLock.Release();
        }
        lastIndex = comparedIndex.End + 1;
        if (lastIndex < sourceIndex.End)
          comparedIndex = (ChangeIdsRange) null;
        else
          break;
      }
      if (lastIndex > sourceIndex.End)
        return;
      await diffLock.WaitAsync();
      diff.Add(new ChangeIdsRange(lastIndex, sourceIndex.End));
      diffLock.Release();
    })))));
    return (IEnumerable<ChangeIdsRange>) diff.OrderBy<ChangeIdsRange, int>((Func<ChangeIdsRange, int>) (x => x.Start));
  }
}
