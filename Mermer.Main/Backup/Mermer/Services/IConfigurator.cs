// Decompiled with JetBrains decompiler
// Type: Mermer.Services.IConfigurator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Services;

public interface IConfigurator
{
  bool HasConfig<T>(string path = null) where T : class;

  bool HasConfig(Type forType, string path = null);

  T GetConfig<T>(string path = null) where T : class;

  Task<T> GetConfigAsync<T>(string path = null) where T : class;

  object GetConfig(Type forType, string path = null);

  Task<object> GetConfigAsync(Type forType, string path = null);

  void SetConfig<T>(T config, string path = null) where T : class;

  Task SetConfigAsync<T>(T config, string path = null) where T : class;
}
