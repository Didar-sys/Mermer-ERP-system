// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Changes.Services.ICouchLocalChangesRepositoryService`1
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Core;
using Mermer.Data.Synchronizer.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Changes.Services;

public interface ICouchLocalChangesRepositoryService<T> : 
  ILocalChangesRepositoryService<T>,
  IChangesRepositoryService<T>
  where T : class
{
  Task StorePatchesAsync(IEnumerable<T> patches, IBucket bucket);
}
