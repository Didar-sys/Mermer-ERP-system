// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Synchronizer.Core.Tools.IChangesIndexHelper
// Assembly: Mermer.Data.Synchronizer.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 51A01EA9-84E6-49FB-B6E1-8048825E2DB0
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Data.Synchronizer.Core.dll

using Mermer.Data.Synchronizer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Data.Synchronizer.Core.Tools;

public interface IChangesIndexHelper
{
  Task<(Dictionary<string, IEnumerable<ChangeIdsRange>> sourceDiff, Dictionary<string, IEnumerable<ChangeIdsRange>> targetDiff)> GetDiffAsync(
    Dictionary<string, IEnumerable<ChangeIdsRange>> source,
    Dictionary<string, IEnumerable<ChangeIdsRange>> target);
}
