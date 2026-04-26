// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Services.RegistryConfigurator
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using Microsoft.Win32;
using Payhas.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Services;

public class RegistryConfigurator : IConfigurator, IDisposable
{
  private RegistryKey _regKey;
  private readonly string _appKey;

  public RegistryConfigurator(string appKey) => this._appKey = appKey;

  public void Dispose()
  {
    if (this._regKey == null)
      return;
    this._regKey.Dispose();
    this._regKey = (RegistryKey) null;
  }

  private static string GetConfigName(Type forType, string path)
  {
    return (path == null ? "" : path + "\\") + forType.Name;
  }

  public bool HasConfig<T>(string path = null) where T : class => this.HasConfig(typeof (T), path);

  public bool HasConfig(Type forType, string path = null)
  {
    string configName = RegistryConfigurator.GetConfigName(forType, path);
    RegistryKey subKey = Registry.CurrentUser.CreateSubKey(this._appKey);
    return subKey != null && ((IEnumerable<string>) subKey.GetSubKeyNames()).Contains<string>(configName);
  }

  public T GetConfig<T>(string path = null) where T : class
  {
    return this.GetConfig(typeof (T), path) as T;
  }

  public async Task<T> GetConfigAsync<T>(string path = null) where T : class
  {
    return await this.GetConfigAsync(typeof (T), path) as T;
  }

  public object GetConfig(Type forType, string path = null)
  {
    object instance = Activator.CreateInstance(forType);
    string configName = RegistryConfigurator.GetConfigName(forType, path);
    this._regKey = Registry.CurrentUser.CreateSubKey(this._appKey)?.CreateSubKey(configName);
    if (this._regKey != null)
    {
      foreach (PropertyInfo property in forType.GetProperties())
      {
        if (!(property.SetMethod == (MethodInfo) null))
        {
          object obj = property.GetValue(instance);
          string str = this._regKey.GetValue(property.Name, (object) obj?.ToString())?.ToString();
          property.SetValue(instance, RegistryConfigurator.ConvertStringToValue(str, property.PropertyType));
          this._regKey.SetValue(property.Name, (object) (str ?? ""), RegistryValueKind.String);
        }
      }
      this._regKey.Close();
    }
    return instance;
  }

  public Task<object> GetConfigAsync(Type forType, string path = null)
  {
    return Task.Run<object>((Func<object>) (() => this.GetConfig(forType, path)));
  }

  public void SetConfig<T>(T config, string path = null) where T : class
  {
    string configName = RegistryConfigurator.GetConfigName(typeof (T), path);
    this._regKey = Registry.CurrentUser.CreateSubKey(this._appKey)?.CreateSubKey(configName);
    if (this._regKey == null)
      return;
    foreach (PropertyInfo property in typeof (T).GetProperties())
    {
      object obj = property.GetValue((object) config);
      if (obj != null)
        this._regKey.SetValue(property.Name, obj, RegistryValueKind.String);
      else
        this._regKey.SetValue(property.Name, (object) "");
    }
    this._regKey.Close();
  }

  public Task SetConfigAsync<T>(T config, string path = null) where T : class
  {
    return Task.Run((Action) (() => this.SetConfig<T>(config, path)));
  }

  private static object ConvertStringToValue(string value, Type type)
  {
    switch (type.Name)
    {
      case "Boolean":
        return (object) bool.Parse(value);
      case "Char":
        return (object) char.Parse(value);
      case "DateTime":
        return (object) DateTime.Parse(value);
      case "Decimal":
        return (object) Decimal.Parse(value);
      case "Double":
        return (object) double.Parse(value);
      case "Float":
        return (object) double.Parse(value);
      case "Int16":
        return (object) short.Parse(value);
      case "Int32":
        return (object) int.Parse(value);
      case "Int64":
        return (object) long.Parse(value);
      case "String":
        return (object) value;
      default:
        if (type.Name.StartsWith("Nullable"))
          return RegistryConfigurator.ConvertStringToValue(value, type.GenericTypeArguments[0]);
        return type.BaseType == typeof (Enum) && !string.IsNullOrEmpty(value) ? Enum.Parse(type, value) : (object) null;
    }
  }
}
