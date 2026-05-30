using Couchbase.Core;
using Couchbase.Views;
using FluentValidation;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Data.Authorizers;
using Mermer.Data.Models;
using Mermer.Data.Patcher;
using Mermer.Data.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        return Task.FromResult(fields.ToDictionary(x => x, x => new Dictionary<string, int>()));
    }

    protected virtual async Task<Dictionary<string, Dictionary<string, int>>> GetFacetsFromView(
      string designDoc,
      string view,
      params string[] fields)
    {
        using (IBucket bucket = this.Cluster.OpenDefaultBucket())
        {
            // Явно приводимо fields до масиву об'єктів для Couchbase ViewQuery
            object[] queryKeys = fields.Cast<object>().ToArray();
            var query = new ViewQuery().From(designDoc, view).Group(true).Keys(queryKeys);

            var viewResult = await bucket.QueryAsync<Dictionary<string, int>>(query);

            if (!viewResult.Success)
            {
                throw viewResult.Exception ?? new Exception(viewResult.Message);
            }

            // Створюємо словник під назвою facets
            var facets = viewResult.Rows.ToDictionary<ViewRow<Dictionary<string, int>>, string, Dictionary<string, int>>(
                x => x.Key.ToString(),
                x => x.Value ?? new Dictionary<string, int>()
            );

            // Використовуємо ту саму назву - facets
            foreach (string key in fields.Where(x => !facets.ContainsKey(x)))
            {
                facets.Add(key, new Dictionary<string, int>());
            }

            return facets;
        }
    }
}