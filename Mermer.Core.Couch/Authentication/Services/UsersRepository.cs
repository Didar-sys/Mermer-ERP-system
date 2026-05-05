// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Authentication.Services.UsersRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using FluentValidation;
using Mermer.Authorization.Models;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Authentication.Services;

public class UsersRepository(
  IPatcher patcher,
  ICouchCluster cluster,
  IValidator<User> validator,
  ILoginService loginService,
  IListAuthorizer<User> authorizer,
  IDocumentChangeListener changeListener,
  ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService) : 
  CouchRepository<User>(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
{
  public override Task CreateAsync(User model)
  {
    model.Password = model.Password.Hash();
    return base.CreateAsync(model);
  }

  public override async Task UpdateAsync(User model)
  {
    UsersRepository usersRepository = this;
    if (string.IsNullOrEmpty(model.Password))
    {
      using (IBucket bucket = usersRepository.Cluster.OpenDefaultBucket())
      {
        IDocumentResult<User> documentAsync = await bucket.GetDocumentAsync<User>(model.Id);
        if (documentAsync?.Content != null)
          model.Password = documentAsync.Content.Password;
      }
    }
    // ISSUE: reference to a compiler-generated method
    await usersRepository.\u003C\u003En__0(model);
    model.Password = (string) null;
  }

  public override async Task<User> GetAsync(string id)
  {
    User async = await base.GetAsync(id);
    async.Password = (string) null;
    return async;
  }

  public override async Task<IEnumerable<User>> GetAsync(
    params Expression<Func<User, bool>>[] predicates)
  {
    IEnumerable<User> async1 = await base.GetAsync(predicates);
    if (!(async1 is IList<User> userList))
      userList = (IList<User>) async1.ToList<User>();
    IList<User> async2 = userList;
    foreach (User user in (IEnumerable<User>) async2)
      user.Password = (string) null;
    return (IEnumerable<User>) async2;
  }
}
