// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.CRM.Services.PartnersRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using FluentValidation;
using Mermer.Authorization.Services;
using Mermer.Commerce.Models;
using Mermer.Common.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.CRM.Models;
using Mermer.CRM.Services;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using Mermer.Data.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.CRM.Services;

public class PartnersRepository(
  ICouchCluster cluster,
  IValidator<Partner> validator,
  IListAuthorizer<Partner> authorizer,
  IDocumentChangeListener changeListener,
  ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService,
  IPatcher patcher,
  ILoginService loginService) : 
  CouchRepositoryWithFacet<Partner>(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService),
  IPartnersRepository,
  IRepositoryWithFacets<Partner>,
  IRepository<Partner>,
  IReadOnlyRepository<Partner>
{
  public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
  {
    return this.GetFacetsFromView("crm", "partner-facets", fields);
  }

  public override async Task<IEnumerable<Partner>> GetAsync(
    params Expression<Func<Partner, bool>>[] predicates)
  {
    return (IEnumerable<Partner>) (await base.GetAsync(predicates)).OrderBy<Partner, string>((Func<Partner, string>) (x => x.Name));
  }

  public async Task MergeAsync(string mainItemId, string[] mergeItemIds, bool disableMergedItems)
  {
    PartnersRepository partnersRepository = this;
    using (IBucket bucket1 = partnersRepository.Cluster.OpenDefaultBucket())
    {
      BucketContext context = new BucketContext(bucket1);
      IQueryable<Bill> source1 = context.Query<Bill>();
      Expression<Func<Bill, bool>> predicate1 = x => x.DocType == "Bill" && mergeItemIds.Contains(x.PartnerId);
      foreach (Bill item in await source1.Where<Bill>(predicate1).ExecuteAsync<Bill>())
      {
        item.PartnerId = mainItemId;
        ICouchLocalChangesRepositoryService<CouchPatch> repositoryService = partnersRepository.LocalChangesRepositoryService;
        CouchPatch[] patches = new CouchPatch[1];
        CouchPatch couchPatch = new CouchPatch();
        couchPatch.Id = item.Id;
        couchPatch.Action = PatchAction.Update;
        couchPatch.PropertyPatches = new Dictionary<string, object>()
        {
          {
            "PartnerId",
            (object) mainItemId
          }
        };
        couchPatch.DocType = item.GetType().Name;
        couchPatch.Author = partnersRepository.LoginService.Session.Username;
        patches[0] = couchPatch;
        IBucket bucket2 = bucket1;
        await repositoryService.StorePatchesAsync((IEnumerable<CouchPatch>) patches, bucket2);
        IDocumentResult<Bill> documentResult = await bucket1.UpsertAsync<Bill>((IDocument<Bill>) new Document<Bill>()
        {
          Id = item.Id,
          Content = item
        });
      }
      IQueryable<Invoice> source2 = context.Query<Invoice>();
      Expression<Func<Invoice, bool>> predicate2 = x => x.DocType == "Invoice" && mergeItemIds.Contains(x.PartnerId);
      foreach (Invoice item in await source2.Where<Invoice>(predicate2).ExecuteAsync<Invoice>())
      {
        item.PartnerId = mainItemId;
        ICouchLocalChangesRepositoryService<CouchPatch> repositoryService = partnersRepository.LocalChangesRepositoryService;
        CouchPatch[] patches = new CouchPatch[1];
        CouchPatch couchPatch = new CouchPatch();
        couchPatch.Id = item.Id;
        couchPatch.Action = PatchAction.Update;
        couchPatch.PropertyPatches = new Dictionary<string, object>()
        {
          {
            "PartnerId",
            (object) mainItemId
          }
        };
        couchPatch.DocType = item.GetType().Name;
        couchPatch.Author = partnersRepository.LoginService.Session.Username;
        patches[0] = couchPatch;
        IBucket bucket3 = bucket1;
        await repositoryService.StorePatchesAsync((IEnumerable<CouchPatch>) patches, bucket3);
        IDocumentResult<Invoice> documentResult = await bucket1.UpsertAsync<Invoice>((IDocument<Invoice>) new Document<Invoice>()
        {
          Id = item.Id,
          Content = item
        });
      }
      IQueryable<PartnerSlip> source3 = context.Query<PartnerSlip>();
      Expression<Func<PartnerSlip, bool>> predicate3 = x => x.DocType == "PartnerSlip" && x.Lines.Any(i => mergeItemIds.Contains(i.PartnerId));
      foreach (PartnerSlip item in await source3.Where<PartnerSlip>(predicate3).ExecuteAsync<PartnerSlip>())
      {
        CouchPatch couchPatch1 = new CouchPatch();
        couchPatch1.Id = item.Id;
        couchPatch1.Action = PatchAction.Update;
        couchPatch1.SubListPatches = new Dictionary<string, List<Patch>>()
        {
          {
            "Lines",
            new List<Patch>()
          }
        };
        couchPatch1.DocType = item.GetType().Name;
        couchPatch1.Author = partnersRepository.LoginService.Session.Username;
        CouchPatch couchPatch2 = couchPatch1;
        foreach (PartnerSlipLine line in (Collection<PartnerSlipLine>) item.Lines)
        {
          if (((IEnumerable<string>) mergeItemIds).Contains<string>(line.PartnerId))
            line.PartnerId = mainItemId;
          couchPatch2.SubListPatches["Lines"].Add(new Patch()
          {
            Id = line.Id,
            Action = PatchAction.Update,
            PropertyPatches = new Dictionary<string, object>()
            {
              {
                "PartnerId",
                (object) mainItemId
              }
            }
          });
        }
        await partnersRepository.LocalChangesRepositoryService.StorePatchesAsync((IEnumerable<CouchPatch>) new CouchPatch[1]
        {
          couchPatch2
        }, bucket1);
        IDocumentResult<PartnerSlip> documentResult = await bucket1.UpsertAsync<PartnerSlip>((IDocument<PartnerSlip>) new Document<PartnerSlip>()
        {
          Id = item.Id,
          Content = item
        });
      }
      IQueryable<PartnerTransfer> source4 = context.Query<PartnerTransfer>();
            Expression<Func<PartnerTransfer, bool>> predicate4 = x => x.DocType == "PartnerTransfer" && x.Lines.Any(i => mergeItemIds.Contains(i.PartnerId));
            foreach (PartnerTransfer item in await source4.Where<PartnerTransfer>(predicate4).ExecuteAsync<PartnerTransfer>())
      {
        CouchPatch couchPatch3 = new CouchPatch();
        couchPatch3.Id = item.Id;
        couchPatch3.Action = PatchAction.Update;
        couchPatch3.SubListPatches = new Dictionary<string, List<Patch>>()
        {
          {
            "Lines",
            new List<Patch>()
          }
        };
        couchPatch3.DocType = item.GetType().Name;
        couchPatch3.Author = partnersRepository.LoginService.Session.Username;
        CouchPatch couchPatch4 = couchPatch3;
        foreach (PartnerTransferLine line in (Collection<PartnerTransferLine>) item.Lines)
        {
                    if (mergeItemIds.Contains(line.PartnerId))
                        line.PartnerId = mainItemId;
          couchPatch4.SubListPatches["Lines"].Add(new Patch()
          {
            Id = line.Id,
            Action = PatchAction.Update,
            PropertyPatches = new Dictionary<string, object>()
            {
              {
                "PartnerId",
                (object) mainItemId
              }
            }
          });
        }
        await partnersRepository.LocalChangesRepositoryService.StorePatchesAsync((IEnumerable<CouchPatch>) new CouchPatch[1]
        {
          couchPatch4
        }, bucket1);
        IDocumentResult<PartnerTransfer> documentResult = await bucket1.UpsertAsync<PartnerTransfer>((IDocument<PartnerTransfer>) new Document<PartnerTransfer>()
        {
          Id = item.Id,
          Content = item
        });
      }
      if (disableMergedItems)
      {
        foreach (Partner item in await context.Query<Partner>().UseKeys<Partner>((IEnumerable<string>) mergeItemIds).ExecuteAsync<Partner>())
        {
          item.IsDisabled = true;
          ICouchLocalChangesRepositoryService<CouchPatch> repositoryService = partnersRepository.LocalChangesRepositoryService;
          CouchPatch[] patches = new CouchPatch[1];
          CouchPatch couchPatch = new CouchPatch();
          couchPatch.Id = item.Id;
          couchPatch.Action = PatchAction.Update;
          couchPatch.PropertyPatches = new Dictionary<string, object>()
          {
            {
              "IsDisabled",
              (object) true
            }
          };
          couchPatch.DocType = item.GetType().Name;
          couchPatch.Author = partnersRepository.LoginService.Session.Username;
          patches[0] = couchPatch;
          IBucket bucket4 = bucket1;
          await repositoryService.StorePatchesAsync((IEnumerable<CouchPatch>) patches, bucket4);
          IDocumentResult<Partner> documentResult = await bucket1.UpsertAsync<Partner>((IDocument<Partner>) new Document<Partner>()
          {
            Id = item.Id,
            Content = item
          });
        }
      }
      context = (BucketContext) null;
    }
  }
}
