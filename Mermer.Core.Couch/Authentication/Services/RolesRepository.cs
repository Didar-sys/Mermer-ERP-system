// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Authentication.Services.RolesRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

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
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Authentication.Services;

public class RolesRepository(
  IPatcher patcher,
  ICouchCluster cluster,
  IValidator<Role> validator,
  ILoginService loginService,
  IListAuthorizer<Role> authorizer,
  IDocumentChangeListener changeListener,
  ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService) : 
  CouchRepository<Role>(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
{
  public override async Task<Role> GetAsync(string id)
  {
    Role async = await base.GetAsync(id);
    if (async.Authorizations != null && async.Authorizations.Any<KeyValuePair<string, int>>())
      async.Authorizations = async.Authorizations.ToDictionary<KeyValuePair<string, int>, string, int>((Func<KeyValuePair<string, int>, string>) (x => x.Key.First<char>().ToString().ToUpper() + x.Key.Substring(1)), (Func<KeyValuePair<string, int>, int>) (x => x.Value));
    return async;
  }
}
