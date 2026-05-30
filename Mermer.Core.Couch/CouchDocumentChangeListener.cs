using Couchbase.Core;
using Couchbase.Views;
using Mermer.Common.Services;
using Mermer.Core.Couch.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mermer.Core.Couch;

public class CouchDocumentChangeListener : IDocumentChangeListener
{
    private readonly ICouchCluster _cluster;
    private readonly IDocumentChangedNotifier _notifier;
    private CancellationTokenSource _cancellationTokenSource;
    private int _updateIntervalRotator;

    public CouchDocumentChangeListener(ICouchCluster cluster, IDocumentChangedNotifier notifier)
    {
        _cluster = cluster;
        _notifier = notifier;
    }

    public bool Started { get; private set; }
    public int UpdateInterval { get; private set; }
    public string LastRevision { get; private set; }

    public async void Start()
    {
        try
        {
            Started = true;
            UpdateInterval = 1;
            _updateIntervalRotator = 1;
            LastRevision = await GetLastRevisionAsync();

            while (Started)
            {
                string str;
                try
                {
                    str = await NotifyNewRevisionAsync(LastRevision);
                }
                catch
                {
                    str = LastRevision;
                }

                if (str != LastRevision)
                {
                    UpdateInterval = 1;
                    _updateIntervalRotator = 1;
                    LastRevision = str;
                }
                else
                {
                    if (_updateIntervalRotator > 10)
                    {
                        _updateIntervalRotator = 1;
                        UpdateInterval = UpdateInterval < 4 ? UpdateInterval + 1 : 5;
                    }
                    _updateIntervalRotator++;
                }

                try
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    await Task.Delay(TimeSpan.FromSeconds(UpdateInterval), _cancellationTokenSource.Token);
                }
                catch
                {
                    UpdateInterval = 1;
                }
            }
        }
        catch
        {
            Started = false;
        }
    }

    public void Touch() => _cancellationTokenSource?.Cancel();
    public void Stop() => Started = false;

    private async Task<string> GetLastRevisionAsync()
    {
        using (IBucket bucket = _cluster.OpenDefaultBucket())
        {
            var viewResult1 = await bucket.QueryAsync<int>(StartQuery().Reduce(true));
            int num = viewResult1.Success ? viewResult1.Values.FirstOrDefault() : throw viewResult1.Exception ?? new Exception(viewResult1.Message);

            if (num == 0) return string.Empty;

            var viewResult2 = await bucket.QueryAsync<string>(StartQuery().Reduce(false).Skip(num - 1).Limit(1));
            if (!viewResult2.Success)
                throw viewResult2.Exception ?? new Exception(viewResult2.Message);

            return viewResult2.Rows.Single().Key.ToString();
        }
    }

    private async Task<string> NotifyNewRevisionAsync(string since)
    {
        string lastRevision = since;
        using (IBucket bucket = _cluster.OpenDefaultBucket())
        {
            var viewResult = await bucket.QueryAsync<string>(StartQuery().Reduce(false).StartKey(since + "0"));
            if (!viewResult.Success)
                throw viewResult.Exception ?? new Exception(viewResult.Message);

            foreach (var row in viewResult.Rows)
            {
                lastRevision = row.Key.ToString();
                _notifier.DocumentChanged(row.Value, row.Id);
            }
        }
        return lastRevision;
    }

    private static IViewQuery StartQuery() => new ViewQuery().From("common", "common-revisions");
}