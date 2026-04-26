// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Services.ReportLayoutStorageService
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using Couchbase;
using Couchbase.Core;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Core.Couch.Changes;
using Payhas.Binyat.Core.Couch.Changes.Services;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Data.Patcher;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Services;

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
    this._patcher = patcher;
    this._cluster = cluster;
    this._loginService = loginService;
    this._localChangesRepositoryService = localChangesRepositoryService;
  }

  public async Task<string> GetAsync(string reportName)
  {
    string layout;
    using (IBucket bucket = this._cluster.OpenDefaultBucket())
      layout = (await bucket.GetDocumentAsync<ReportLayout>(ReportLayoutStorageService.GetReportName(reportName))).Content?.Layout;
    return layout;
  }

  public async Task StoreAsync(string reportName, string reportLayout)
  {
    using (IBucket bucket1 = this._cluster.OpenDefaultBucket())
    {
      string id = ReportLayoutStorageService.GetReportName(reportName);
      ReportLayout model = new ReportLayout()
      {
        Name = reportName,
        Layout = reportLayout
      };
      IDocumentResult<ReportLayout> documentAsync = await bucket1.GetDocumentAsync<ReportLayout>(id);
      Patch patch = this._patcher.CreatePatch<ReportLayout>(model, documentAsync.Content, id);
      ICouchLocalChangesRepositoryService<CouchPatch> repositoryService = this._localChangesRepositoryService;
      CouchPatch[] patches = new CouchPatch[1];
      CouchPatch couchPatch = new CouchPatch();
      couchPatch.Id = patch.Id;
      couchPatch.Action = patch.Action;
      couchPatch.PropertyPatches = patch.PropertyPatches;
      couchPatch.SubListPatches = patch.SubListPatches;
      couchPatch.DocType = typeof (ReportLayout).Name;
      couchPatch.Author = this._loginService.Session.Username;
      patches[0] = couchPatch;
      IBucket bucket2 = bucket1;
      await repositoryService.StorePatchesAsync((IEnumerable<CouchPatch>) patches, bucket2);
      IDocumentResult<ReportLayout> documentResult = await bucket1.UpsertAsync<ReportLayout>((IDocument<ReportLayout>) new Document<ReportLayout>()
      {
        Id = id,
        Content = model
      });
      id = (string) null;
      model = (ReportLayout) null;
    }
  }

  private static string GetReportName(string reportName) => "Report-" + reportName;
}
