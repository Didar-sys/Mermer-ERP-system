// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.Services.IMachineIdProviderService
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.Services;

public interface IMachineIdProviderService
{
  Task<string> GetUniqueIdAsync();
}
