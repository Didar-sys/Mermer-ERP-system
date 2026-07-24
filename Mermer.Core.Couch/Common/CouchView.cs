// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Common.CouchView
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Core;
using Couchbase.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Common;

public class CouchView
{
  protected readonly ICouchCluster Cluster;

  public CouchView(ICouchCluster cluster) => this.Cluster = cluster;

  protected virtual Task<IEnumerable<T>> GetRecordsAsync<T>(
    string designDoc,
    string view,
    object[] keys,
    bool reduce = false,
    int groupLevel = 0,
    Func<ViewRow<T>, T> projector = null)
  {
    return this.GetRecordsAsync<T, T>(designDoc, view, keys, reduce, groupLevel, projector);
  }

  protected virtual Task<IEnumerable<TResult>> GetRecordsAsync<TRow, TResult>(
    string designDoc,
    string view,
    [NotNull] object[] keys,
    bool reduce = false,
    int groupLevel = 0,
    Func<ViewRow<TRow>, TResult> projector = null)
  {
    return this.GetRecordsAsync<TRow, TResult>(new List<IViewQuery>()
    {
      this.StartViewQuery(designDoc, view, keys)
    }, reduce, groupLevel, projector);
  }

  protected virtual Task<IEnumerable<T>> GetRecordsAsync<T>(
    string designDoc,
    string view,
    Tuple<object, object>[] startEndKeys,
    bool reduce = false,
    int groupLevel = 0,
    Func<ViewRow<T>, T> projector = null,
    bool inclusiveEnd = false)
  {
    return this.GetRecordsAsync<T, T>(designDoc, view, startEndKeys, reduce, groupLevel, projector, inclusiveEnd);
  }

  protected virtual Task<IEnumerable<TResult>> GetRecordsAsync<TRow, TResult>(
    string designDoc,
    string view,
    Tuple<object, object>[] startEndKeys,
    bool reduce = false,
    int groupLevel = 0,
    Func<ViewRow<TRow>, TResult> projector = null,
    bool inclusiveEnd = false)
  {
    List<IViewQuery> queries = new List<IViewQuery>();
    if (startEndKeys != null)
      queries.AddRange(((IEnumerable<Tuple<object, object>>) startEndKeys).Select<Tuple<object, object>, IViewQuery>((Func<Tuple<object, object>, IViewQuery>) (keys => this.StartViewQuery(designDoc, view, keys.Item1, keys.Item2, inclusiveEnd))));
    else
      queries.Add(this.StartViewQuery(designDoc, view));
    return this.GetRecordsAsync<TRow, TResult>(queries, reduce, groupLevel, projector);
  }

  private async Task<IEnumerable<TResult>> GetRecordsAsync<TRow, TResult>(
    List<IViewQuery> queries,
    bool reduce = false,
    int groupLevel = 0,
    Func<ViewRow<TRow>, TResult> projector = null,
    int retrials = 0)
  {
    List<IEnumerable<TResult>> list = new List<IEnumerable<TResult>>(queries.Count);
    using (IBucket bucket = this.Cluster.OpenDefaultBucket())
    {
      try
      {
        if (queries.Count == 1)
          return await this.GetRecordsByOneAsync<TRow, TResult>(queries, reduce, groupLevel, projector);
        List<Task> taskList = new List<Task>();
        foreach (IViewQuery query in queries)
        {
          query.Reduce(reduce);
          if (groupLevel > 0)
            query.GroupLevel(groupLevel);
          if (projector == null)
          {
            Task task = bucket.QueryAsync<TResult>((IViewQueryable) query).ContinueWith((Action<Task<IViewResult<TResult>>>) (t =>
            {
              if (!t.IsCompleted || !t.Result.Success)
                return;
              list.Add(t.Result.Values);
            }));
            taskList.Add(task);
          }
          else
          {
            Task task = bucket.QueryAsync<TRow>((IViewQueryable) query).ContinueWith((Action<Task<IViewResult<TRow>>>) (t =>
            {
              if (!t.IsCompleted || !t.Result.Success)
                return;
              list.Add(t.Result.Rows.Select<ViewRow<TRow>, TResult>(projector));
            }));
            taskList.Add(task);
          }
        }
        await Task.WhenAll((IEnumerable<Task>) taskList);
        if (queries.Count != list.Count)
          return retrials < 3 ? await this.GetRecordsAsync<TRow, TResult>(queries, reduce, groupLevel, projector, retrials + 1) : await this.GetRecordsByOneAsync<TRow, TResult>(queries, reduce, groupLevel, projector);
      }
            catch (Exception ex)
            {
                throw new Exception("Couchbase View Query Failed: " + ex.Message, ex);
            }
        }
    IEnumerable<TResult> first = (IEnumerable<TResult>) new List<TResult>();
    foreach (IEnumerable<TResult> second in list)
      first = first.Concat<TResult>(second);
    return first;
  }

    protected virtual async Task<long> GetCountAsync(string designDoc, string view, IViewQuery query)
    {
        using (IBucket bucket = this.Cluster.OpenDefaultBucket())
        {
            query.Reduce(true); // Обязательно
            var result = await bucket.QueryAsync<dynamic>(query);
            if (!result.Success) throw new Exception(result.Message);

            // Результат _count всегда находится в первом элементе Values
            return (long)(result.Rows.FirstOrDefault()?.Value ?? 0);
        }
    }

    private async Task<IEnumerable<TResult>> GetRecordsByOneAsync<TRow, TResult>(
    List<IViewQuery> queries,
    bool reduce = false,
    int groupLevel = 0,
    Func<ViewRow<TRow>, TResult> projector = null)
  {
    List<TResult> list = new List<TResult>();
    using (IBucket bucket = this.Cluster.OpenDefaultBucket())
    {
      foreach (IViewQuery query in queries)
      {
        query.Reduce(reduce);
        if (groupLevel > 0)
          query.GroupLevel(groupLevel);
        if (projector == null)
          list.AddRange((await bucket.QueryAsync<TResult>((IViewQueryable) query)).Values);
        else
          list.AddRange((await bucket.QueryAsync<TRow>((IViewQueryable) query)).Rows.Select<ViewRow<TRow>, TResult>(projector));
      }
    }
    IEnumerable<TResult> recordsByOneAsync = (IEnumerable<TResult>) list;
    list = (List<TResult>) null;
    return recordsByOneAsync;
  }

  protected virtual IViewQuery StartViewQuery(string designDoc, string view)
  {
    return new ViewQuery().From(designDoc, view);
  }

  protected virtual IViewQuery StartViewQuery(string designDoc, string view, object[] keys)
  {
    return new ViewQuery().From(designDoc, view).Keys((IEnumerable) keys);
  }

  protected virtual IViewQuery StartViewQuery(
    string designDoc,
    string view,
    object startKey,
    object endKey,
    bool inclusiveEnd)
  {
    return new ViewQuery().From(designDoc, view).InclusiveEnd(inclusiveEnd).StartKey(startKey).EndKey(endKey);
  }
}
