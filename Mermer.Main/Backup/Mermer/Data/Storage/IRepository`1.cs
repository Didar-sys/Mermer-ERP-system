// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Storage.IRepository`1
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Data.Models;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Data.Storage;

public interface IRepository<T> : IReadOnlyRepository<T> where T : IModel
{
  Task CreateAsync(T model);

  Task UpdateAsync(T model);
}
