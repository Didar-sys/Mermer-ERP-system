// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.CouchDocumentChangeListener
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Core;
using Couchbase.Views;
using Microsoft.CSharp.RuntimeBinder;
using Mermer.Common.Services;
using Mermer.Core.Couch.Common;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch;

public class CouchDocumentChangeListener : IDocumentChangeListener
{
  private readonly ICouchCluster _cluster;
  private readonly IDocumentChangedNotifier _notifier;
  private CancellationTokenSource _cancellationTokenSource;
  private int _updateIntervalRotator;

  public CouchDocumentChangeListener(ICouchCluster cluster, IDocumentChangedNotifier notifier)
  {
    this._cluster = cluster;
    this._notifier = notifier;
  }

  public bool Started { get; private set; }

  public int UpdateInterval { get; private set; }

  public string LastRevision { get; private set; }

  public async void Start()
  {
    try
    {
      this.Started = true;
      this.UpdateInterval = 1;
      this._updateIntervalRotator = 1;
      this.LastRevision = await this.GetLastRevisionAsync();
      while (this.Started)
      {
        string str;
        try
        {
          str = await this.NotifyNewRevisionAsync(this.LastRevision);
        }
        catch (Exception ex)
        {
          str = this.LastRevision;
        }
        if (str != this.LastRevision)
        {
          this.UpdateInterval = 1;
          this._updateIntervalRotator = 1;
          this.LastRevision = str;
        }
        else
        {
          if (this._updateIntervalRotator > 10)
          {
            this._updateIntervalRotator = 1;
            this.UpdateInterval = this.UpdateInterval < 4 ? this.UpdateInterval + 1 : 5;
          }
          ++this._updateIntervalRotator;
        }
        try
        {
          this._cancellationTokenSource = new CancellationTokenSource();
          await Task.Delay(TimeSpan.FromSeconds((double) this.UpdateInterval), this._cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
          this.UpdateInterval = 1;
        }
      }
    }
    catch (Exception ex)
    {
      this.Started = false;
    }
  }

  public void Touch() => this._cancellationTokenSource?.Cancel();

  public void Stop() => this.Started = false;

  private async Task<string> GetLastRevisionAsync()
  {
    using (IBucket bucket = this._cluster.OpenDefaultBucket())
    {
      IViewResult<int> viewResult1 = await bucket.QueryAsync<int>((IViewQueryable) CouchDocumentChangeListener.StartQuery().Reduce(true));
      int num = viewResult1.Success ? viewResult1.Values.FirstOrDefault<int>() : throw viewResult1.Exception ?? new Exception(viewResult1.Message);
      if (num == 0)
        return string.Empty;
      IViewResult<string> viewResult2 = await bucket.QueryAsync<string>((IViewQueryable) CouchDocumentChangeListener.StartQuery().Reduce(false).Skip(num - 1).Limit(1));
      if (!viewResult2.Success)
        throw viewResult2.Exception ?? new Exception(viewResult2.Message);
      // ISSUE: reference to a compiler-generated field
      if (CouchDocumentChangeListener.\u003C\u003Eo__20.\u003C\u003Ep__0 == null)
      {
        // ISSUE: reference to a compiler-generated field
        CouchDocumentChangeListener.\u003C\u003Eo__20.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (CouchDocumentChangeListener)));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      return CouchDocumentChangeListener.\u003C\u003Eo__20.\u003C\u003Ep__0.Target((CallSite) CouchDocumentChangeListener.\u003C\u003Eo__20.\u003C\u003Ep__0, viewResult2.Rows.Single<ViewRow<string>>().Key);
    }
  }

  private async Task<string> NotifyNewRevisionAsync(string since)
  {
    string lastRevision = since;
    using (IBucket bucket = this._cluster.OpenDefaultBucket())
    {
      IViewResult<string> viewResult = await bucket.QueryAsync<string>((IViewQueryable) CouchDocumentChangeListener.StartQuery().Reduce(false).StartKey((object) (since + "0")));
      if (!viewResult.Success)
        throw viewResult.Exception ?? new Exception(viewResult.Message);
      foreach (ViewRow<string> row in viewResult.Rows)
      {
        // ISSUE: reference to a compiler-generated field
        if (CouchDocumentChangeListener.\u003C\u003Eo__21.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          CouchDocumentChangeListener.\u003C\u003Eo__21.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (CouchDocumentChangeListener)));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        lastRevision = CouchDocumentChangeListener.\u003C\u003Eo__21.\u003C\u003Ep__0.Target((CallSite) CouchDocumentChangeListener.\u003C\u003Eo__21.\u003C\u003Ep__0, row.Key);
        this._notifier.DocumentChanged(row.Value, row.Id);
      }
    }
    string str = lastRevision;
    lastRevision = (string) null;
    return str;
  }

  private static IViewQuery StartQuery() => new ViewQuery().From("common", "common-revisions");
}
