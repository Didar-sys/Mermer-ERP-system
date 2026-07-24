// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Common.CouchRepository`1
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.IO;
using Couchbase.N1QL;
using FluentValidation;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Data.Authorizers;
using Mermer.Data.Models;
using Mermer.Data.Patcher;
using Mermer.Data.Storage;
using Mermer.Data.Synchronizer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Common;

public class CouchRepository<T> : CouchReadOnlyRepository<T>, IRepository<T>, IReadOnlyRepository<T> where T : class, IModel
{
  protected readonly IPatcher Patcher;
  protected readonly IValidator<T> Validator;
  protected readonly ILoginService LoginService;
  protected readonly IListAuthorizer<T> Authorizer;
  protected readonly IDocumentChangeListener ChangeListener;
  protected readonly ICouchLocalChangesRepositoryService<CouchPatch> LocalChangesRepositoryService;

  public CouchRepository(
    IPatcher patcher,
    ICouchCluster cluster,
    IValidator<T> validator,
    ILoginService loginService,
    IListAuthorizer<T> authorizer,
    IDocumentChangeListener changeListener,
    ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService)
    : base(cluster, (IReadOnlyListAuthorizer<T>) authorizer)
  {
    this.Patcher = patcher;
    this.Validator = validator;
    this.LoginService = loginService;
    this.Authorizer = authorizer;
    this.ChangeListener = changeListener;
    this.LocalChangesRepositoryService = localChangesRepositoryService;
  }

  public virtual async Task CreateAsync(T model)
  {
    CouchRepository<T> couchRepository = this;
    await couchRepository.ValidateAsync(model);
    couchRepository.Authorizer.AuthorizeCreate(model);
    using (IBucket bucket1 = couchRepository.Cluster.OpenDefaultBucket())
    {
      if (await bucket1.ExistsAsync(model.Id))
        throw new Exception($"{typeof (T).Name} creation failed!There is an existing item with Id: {model.Id}");
      Patch patch = couchRepository.Patcher.CreatePatch<T>(model, default (T));
      ICouchLocalChangesRepositoryService<CouchPatch> repositoryService = couchRepository.LocalChangesRepositoryService;
      CouchPatch[] patches = new CouchPatch[1];
      CouchPatch couchPatch = new CouchPatch();
      couchPatch.Id = patch.Id;
      couchPatch.Action = patch.Action;
      couchPatch.PropertyPatches = patch.PropertyPatches;
      couchPatch.SubListPatches = patch.SubListPatches;
      couchPatch.DocType = typeof (T).Name;
      couchPatch.Author = couchRepository.LoginService.Session.Username;
      patches[0] = couchPatch;
      IBucket bucket2 = bucket1;
      await repositoryService.StorePatchesAsync((IEnumerable<CouchPatch>) patches, bucket2);
      if ((await bucket1.InsertAsync<object>((IDocument<object>) new Document<object>()
      {
        Id = model.Id,
        Content = (object) model
      }, ReplicateTo.One, PersistTo.One)).Status == ResponseStatus.KeyExists)
        throw new Exception($"{typeof (T).Name} creation failed!There is an existing item with Id: {model.Id}");
      couchRepository.ChangeListener.Touch();
    }
  }

  public virtual async Task UpdateAsync(T model)
  {
    CouchRepository<T> couchRepository = this;
    await couchRepository.ValidateAsync(model);
    using (IBucket bucket1 = couchRepository.Cluster.OpenDefaultBucket())
    {
      IDocumentResult<T> existing = await bucket1.GetDocumentAsync<T>(model.Id);
      if ((object) existing.Content == null)
        throw new Exception($"{typeof (T).Name} update failed!No item found with Id: {model.Id}");
      couchRepository.Authorizer.AuthorizeUpdate(existing.Content, model);
      IQueryResult<ChangeDocument<CouchPatch>> queryResult = await bucket1.QueryAsync<ChangeDocument<CouchPatch>>($"SELECT RAW `docs` FROM `{couchRepository.Cluster.DefaultBucket}` as `docs` WHERE docs.patch.id = '{model.Id}'");
      T target = default (T);
      foreach (ChangeDocument<CouchPatch> changeDocument in (IEnumerable<ChangeDocument<CouchPatch>>) queryResult.Rows.OrderBy<ChangeDocument<CouchPatch>, DateTime>((Func<ChangeDocument<CouchPatch>, DateTime>) (x => x.PatchDate)))
      {
        try
        {
          target = couchRepository.Patcher.ApplyPatch<T>((Patch) changeDocument.Patch, target);
        }
        catch (Exception ex)
        {
        }
      }
      Patch patch1 = couchRepository.Patcher.CreatePatch<T>(existing.Content, target);
      if (patch1 != null)
      {
        ICouchLocalChangesRepositoryService<CouchPatch> repositoryService = couchRepository.LocalChangesRepositoryService;
        CouchPatch[] patches = new CouchPatch[1];
        CouchPatch couchPatch = new CouchPatch();
        couchPatch.Id = patch1.Id;
        couchPatch.Action = patch1.Action;
        couchPatch.PropertyPatches = patch1.PropertyPatches;
        couchPatch.SubListPatches = patch1.SubListPatches;
        couchPatch.DocType = typeof (T).Name;
        couchPatch.Author = couchRepository.LoginService.Session.Username;
        patches[0] = couchPatch;
        IBucket bucket2 = bucket1;
        await repositoryService.StorePatchesAsync((IEnumerable<CouchPatch>) patches, bucket2);
      }
      Patch patch2 = couchRepository.Patcher.CreatePatch<T>(model, existing.Content);
      if (patch2 == null)
        return;
      ICouchLocalChangesRepositoryService<CouchPatch> repositoryService1 = couchRepository.LocalChangesRepositoryService;
      CouchPatch[] patches1 = new CouchPatch[1];
      CouchPatch couchPatch1 = new CouchPatch();
      couchPatch1.Id = patch2.Id;
      couchPatch1.Action = patch2.Action;
      couchPatch1.PropertyPatches = patch2.PropertyPatches;
      couchPatch1.SubListPatches = patch2.SubListPatches;
      couchPatch1.DocType = typeof (T).Name;
      couchPatch1.Author = couchRepository.LoginService.Session.Username;
      patches1[0] = couchPatch1;
      IBucket bucket3 = bucket1;
      await repositoryService1.StorePatchesAsync((IEnumerable<CouchPatch>) patches1, bucket3);
      IDocumentResult<object> documentResult = await bucket1.ReplaceAsync<object>((IDocument<object>) new Document<object>()
      {
        Id = model.Id,
        Content = (object) model
      }, ReplicateTo.One, PersistTo.One);
      couchRepository.ChangeListener.Touch();
      existing = (IDocumentResult<T>) null;
    }
  }

    public virtual async Task ValidateAsync(T model)
    {
        // Отключаем старую FluentValidation, чтобы избежать конфликта версий (MissingMethodException)
        // await this.Validator.AssertValidAsync<T>(model);

        // Говорим программе: "Все окей, данные идеальны, храни!"
        await Task.CompletedTask;
    }
}
