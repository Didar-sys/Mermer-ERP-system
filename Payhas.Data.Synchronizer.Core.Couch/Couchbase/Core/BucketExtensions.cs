// Decompiled with JetBrains decompiler
// Type: Couchbase.Core.BucketExtensions
// Assembly: Payhas.Data.Synchronizer.Core.Couch, Version=0.0.9.0, Culture=neutral, PublicKeyToken=null
// MVID: 7309E162-8E25-4800-97C2-B3CD230F4B8B
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Data.Synchronizer.Core.Couch.dll

using Payhas.Data.Synchronizer.Core.Models;
using Payhas.Data.Synchronizer.Core.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Couchbase.Core;

public static class BucketExtensions
{
  public static async Task StorePatchesAsync<T>(
    this IBucket bucket,
    IChangeHelper changeHelper,
    string userId,
    string serverId,
    IEnumerable<T> patches)
  {
    string counterDocId = $"sync:{serverId}:counters:patch-id";
    List<IDocument<ChangeDocument<T>>> documents = new List<IDocument<ChangeDocument<T>>>();
    foreach (T patch1 in patches)
    {
      T patch = patch1;
      int patchId;
      string changeId;
      do
      {
        patchId = (int) (await bucket.IncrementAsync(counterDocId, TimeSpan.FromSeconds(3.0))).Value;
        changeId = changeHelper.GenerateChangeId(userId, serverId, patchId);
      }
      while (await bucket.ExistsAsync(changeId));
      ChangeDocument<T> changeDocument1 = new ChangeDocument<T>();
      changeDocument1.Id = changeId;
      changeDocument1.UserId = userId;
      changeDocument1.ServerId = serverId;
      changeDocument1.PatchDate = DateTime.Now;
      changeDocument1.PatchId = patchId;
      changeDocument1.Patch = patch;
      ChangeDocument<T> changeDocument2 = changeDocument1;
      documents.Add((IDocument<ChangeDocument<T>>) new Document<ChangeDocument<T>>()
      {
        Id = changeDocument2.Id,
        Content = changeDocument2
      });
      changeId = (string) null;
      patch = default (T);
    }
    IDocumentResult<ChangeDocument<T>>[] documentResultArray = await bucket.InsertAsync<ChangeDocument<T>>(documents, ReplicateTo.One, PersistTo.One);
  }
}
