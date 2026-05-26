// Decompiled with JetBrains decompiler
// Type: Mermer.Authorization.Services.IUsersService
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Models;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Authorization.Services;

public interface IUsersService
{
  Task<User> LoginUser(string username, string password);
}
