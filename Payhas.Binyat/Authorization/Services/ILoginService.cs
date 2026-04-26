// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Authorization.Services.ILoginService
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Authorization.Models;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Authorization.Services;

public interface ILoginService
{
  UserSession Session { get; set; }

  bool IsLoggedIn { get; }

  Task LoginAsync(string username, string password);

  Task UpdatePassword(string currentPassword, string newPassword);

  Task LogoutAsync();
}
