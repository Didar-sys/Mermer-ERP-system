// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Common.CouchRepositoryWithFacet`1
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Core;
using Couchbase.Views;
using FluentValidation;
using Microsoft.CSharp.RuntimeBinder;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Data.Authorizers;
using Mermer.Data.Models;
using Mermer.Data.Patcher;
using Mermer.Data.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Common;

public class CouchRepositoryWithFacet<T>(
  IPatcher patcher,
  ICouchCluster cluster,
  IValidator<T> validator,
  ILoginService loginService,
  IListAuthorizer<T> authorizer,
  IDocumentChangeListener changeListener,
  ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService) : 
  CouchRepository<T>(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService),
  IRepositoryWithFacets<T>,
  IRepository<T>,
  IReadOnlyRepository<T>
  where T : class, IModel
{
  public virtual Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
  {
    return Task.FromResult<Dictionary<string, Dictionary<string, int>>>(((IEnumerable<string>) fields).ToDictionary<string, string, Dictionary<string, int>>((Func<string, string>) (x => x), (Func<string, Dictionary<string, int>>) (x => new Dictionary<string, int>())));
  }

  protected virtual async Task<Dictionary<string, Dictionary<string, int>>> GetFacetsFromView(
    string designDoc,
    string view,
    params string[] fields)
  {
    Dictionary<string, Dictionary<string, int>> facetsFromView;
    using (IBucket bucket = this.Cluster.OpenDefaultBucket())
    {
      Dictionary<string, Dictionary<string, int>> facets = (await bucket.QueryAsync<Dictionary<string, int>>((IViewQueryable) new ViewQuery().From(designDoc, view).Group(true).Keys((IEnumerable) fields))).Rows.ToDictionary<ViewRow<Dictionary<string, int>>, string, Dictionary<string, int>>((Func<ViewRow<Dictionary<string, int>>, string>) (x =>
      {
        // ISSUE: reference to a compiler-generated field
        if (CouchRepositoryWithFacet<T>.\u003C\u003Eo__2.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          CouchRepositoryWithFacet<T>.\u003C\u003Eo__2.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof (string), typeof (CouchRepositoryWithFacet<T>)));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        return CouchRepositoryWithFacet<T>.\u003C\u003Eo__2.\u003C\u003Ep__0.Target((CallSite) CouchRepositoryWithFacet<T>.\u003C\u003Eo__2.\u003C\u003Ep__0, x.Key);
      }), (Func<ViewRow<Dictionary<string, int>>, Dictionary<string, int>>) (x => x.Value ?? new Dictionary<string, int>()));
      foreach (string key in ((IEnumerable<string>) fields).Where<string>((Func<string, bool>) (x => !facets.ContainsKey(x))))
        facets.Add(key, new Dictionary<string, int>());
      facetsFromView = facets;
    }
    return facetsFromView;
  }
}
