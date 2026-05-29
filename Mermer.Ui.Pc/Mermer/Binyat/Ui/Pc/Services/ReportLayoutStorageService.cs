using Couchbase;
using Couchbase.Core;
using Mermer.Authorization.Services;
using Mermer.Data.Patcher;
using Newtonsoft.Json;
using Payhas.Binyat.Core.Couch.Changes;
using Payhas.Binyat.Core.Couch.Changes.Services;
using Payhas.Binyat.Core.Couch.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mermer.Ui.Pc.Services;

public class ReportLayoutStorageService : IReportLayoutStorageService
{
    private readonly IPatcher _patcher;
    private readonly ICouchCluster _cluster;
    private readonly ILoginService _loginService;
    private readonly ICouchLocalChangesRepositoryService<CouchPatch> _localChangesRepositoryService;

    public ReportLayoutStorageService(
        IPatcher patcher,
        ICouchCluster cluster,
        ILoginService loginService,
        ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService)
    {
        _patcher = patcher;
        _cluster = cluster;
        _loginService = loginService;
        _localChangesRepositoryService = localChangesRepositoryService;
    }

    public async Task<string> GetAsync(string reportName)
    {
        using (IBucket bucket = _cluster.OpenDefaultBucket())
        {
            var doc = await bucket.GetDocumentAsync<ReportLayout>(GetReportName(reportName));
            return doc.Content?.Layout;
        }
    }

    public async Task StoreAsync(string reportName, string reportLayout)
    {
        using (IBucket bucket = _cluster.OpenDefaultBucket())
        {
            string id = GetReportName(reportName);
            ReportLayout model = new ReportLayout
            {
                Name = reportName,
                Layout = reportLayout
            };

            var documentAsync = await bucket.GetDocumentAsync<ReportLayout>(id);

            // Створюємо новий об'єкт Mermer.Data.Patcher
            Patch mermerPatch = _patcher.CreatePatch(model, documentAsync.Content, id);

            // Конвертуємо його в старий Payhas.CouchPatch через JSON
            string patchJson = JsonConvert.SerializeObject(mermerPatch);
            CouchPatch couchPatch = JsonConvert.DeserializeObject<CouchPatch>(patchJson);

            // Дозаповнюємо необхідні поля
            couchPatch.DocType = typeof(ReportLayout).Name;
            couchPatch.Author = _loginService.Session.Username;

            await _localChangesRepositoryService.StorePatchesAsync(new[] { couchPatch }, bucket);

            await bucket.UpsertAsync(new Document<ReportLayout>
            {
                Id = id,
                Content = model
            });
        }
    }

    private static string GetReportName(string reportName) => "Report-" + reportName;
}