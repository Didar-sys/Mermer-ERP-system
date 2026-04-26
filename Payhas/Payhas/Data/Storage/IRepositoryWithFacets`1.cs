// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Storage.IRepositoryWithFacets`1
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using Payhas.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Data.Storage;

public interface IRepositoryWithFacets<T> : IRepository<T>, IReadOnlyRepository<T> where T : IModel
{
  Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields);
}
