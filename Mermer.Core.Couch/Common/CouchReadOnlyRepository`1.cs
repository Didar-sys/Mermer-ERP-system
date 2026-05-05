// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Common.CouchReadOnlyRepository`1
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using Mermer.Data.Authorizers;
using Mermer.Data.Models;
using Mermer.Data.Storage;
using Mermer.Data.Tools.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Common;

public class CouchReadOnlyRepository<T> : IReadOnlyRepository<T> where T : IModel
{
  protected readonly ICouchCluster Cluster;
  protected readonly IReadOnlyListAuthorizer<T> Authorizer;

  public CouchReadOnlyRepository(ICouchCluster cluster, IReadOnlyListAuthorizer<T> authorizer)
  {
    this.Cluster = cluster;
    this.Authorizer = authorizer;
  }

  public virtual async Task<T> GetAsync(string id)
  {
    this.Authorizer.Authorize();
    T content;
    using (IBucket bucket = this.Cluster.OpenDefaultBucket())
    {
      IDocumentResult<T> documentAsync = await bucket.GetDocumentAsync<T>(id);
      this.Authorizer.AuthorizeRead(documentAsync.Content);
      content = documentAsync.Content;
    }
    return content;
  }

  public virtual async Task<IEnumerable<T>> GetAsync(string[] ids)
  {
    this.Authorizer.Authorize();
    IEnumerable<T> async;
    using (IBucket bucket = this.Cluster.OpenDefaultBucket())
      async = ((IEnumerable<IDocumentResult<T>>) await bucket.GetDocumentsAsync<T>((IEnumerable<string>) ids)).Select<IDocumentResult<T>, T>((Func<IDocumentResult<T>, T>) (x => x.Content));
    return async;
  }

  public virtual async Task<int> CountAsync(params Expression<Func<T, bool>>[] predicates)
  {
    this.Authorizer.Authorize();
    int num;
    using (IBucket bucket = this.Cluster.OpenDefaultBucket())
      num = await this.StartListQuery(bucket, predicates).ExecuteAsync<T, int>((Expression<Func<IQueryable<T>, int>>) (q => q.Count<T>()));
    return num;
  }

  public virtual async Task<IEnumerable<T>> GetAsync(params Expression<Func<T, bool>>[] predicates)
  {
    this.Authorizer.Authorize();
    IEnumerable<T> async;
    using (IBucket bucket = this.Cluster.OpenDefaultBucket())
      async = await this.StartListQuery(bucket, predicates).ExecuteAsync<T>();
    return async;
  }

  protected virtual IQueryable<T> StartListQuery(
    IBucket bucket,
    params Expression<Func<T, bool>>[] predicates)
  {
    BucketContext bucketContext = new BucketContext(bucket);
    bucketContext.EndChangeTracking();
    IQueryable<T> seed = bucketContext.Query<T>().Where<T>((Expression<Func<T, bool>>) (x => x.DocType == typeof (T).Name && x.Id == N1QlFunctions.Key((object) x)));
    List<Expression<Func<T, bool>>> source = (predicates != null ? ((IEnumerable<Expression<Func<T, bool>>>) predicates).ToList<Expression<Func<T, bool>>>() : (List<Expression<Func<T, bool>>>) null) ?? new List<Expression<Func<T, bool>>>();
    source.Add(this.Authorizer.AuthorizedListFilter());
    return source.Where<Expression<Func<T, bool>>>((Func<Expression<Func<T, bool>>, bool>) (predicateQuery => predicateQuery != null)).Select<Expression<Func<T, bool>>, Expression<Func<T, bool>>>((Func<Expression<Func<T, bool>>, Expression<Func<T, bool>>>) (predicateQuery => predicateQuery.Safe<T>())).Aggregate<Expression<Func<T, bool>>, IQueryable<T>>(seed, (Func<IQueryable<T>, Expression<Func<T, bool>>, IQueryable<T>>) ((current, predicateQuery) => current.Where<T>(predicateQuery)));
  }
}
