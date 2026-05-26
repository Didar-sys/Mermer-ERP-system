// Decompiled with JetBrains decompiler
// Type: Mermer.Mvvm.Tools.JsonTextProvider
// Assembly: Mermer.Mvvm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3EAA5570-F618-4E39-B929-F7374F99B43D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Mvvm.dll

using MvvmCross.Localization;
using System.Collections.Generic;
using System.Globalization;

#nullable disable
namespace Mermer.Mvvm.Tools;

public class JsonTextProvider : IMvxTextProvider
{
  public const string UpdateTrigger = "SourceUpdateTrigger-jnfdh762bkjsd864bhsd56s52";
  private readonly IJsonLocalizationResourceProvider _resourceProvider;

  public JsonTextProvider(IJsonLocalizationResourceProvider resourceProvider)
  {
    this._resourceProvider = resourceProvider;
  }

  public bool TryGetText(out string textValue, string namespaceKey, string typeKey, string name)
  {
    textValue = this.GetText(namespaceKey, typeKey, name);
    return true;
  }

  public bool TryGetText(
    out string textValue,
    string namespaceKey,
    string typeKey,
    string name,
    params object[] formatArgs)
  {
    textValue = this.GetText(namespaceKey, typeKey, name, formatArgs);
    return true;
  }

  public string GetText(string namespaceKey, string typeKey, string name)
  {
    if (name == "SourceUpdateTrigger-jnfdh762bkjsd864bhsd56s52")
    {
      this._resourceProvider.UpdateResources();
      return "#" + name;
    }
    string text;
    return !string.IsNullOrEmpty(namespaceKey) && !string.IsNullOrEmpty(typeKey) && this.TryGetText($"{namespaceKey}.{typeKey}", CultureInfo.CurrentCulture, name, out text) || !string.IsNullOrEmpty(typeKey) && this.TryGetText(typeKey, CultureInfo.CurrentCulture, name, out text) || !string.IsNullOrEmpty(namespaceKey) && this.TryGetText(namespaceKey, CultureInfo.CurrentCulture, name, out text) || this.TryGetText("default", CultureInfo.CurrentCulture, name, out text, true) ? text : "#" + name;
  }

  public string GetText(
    string namespaceKey,
    string typeKey,
    string name,
    params object[] formatArgs)
  {
    string text = this.GetText(namespaceKey, typeKey, name);
    return string.IsNullOrEmpty(text) ? text : string.Format(text, formatArgs);
  }

  private bool TryGetText(string resourceName, string name, out string text, bool saveIfNotExist = false)
  {
    text = string.Empty;
    Dictionary<string, string> resource;
    if (this._resourceProvider.TryGetResource(resourceName, out resource))
    {
      if (resource.ContainsKey(name))
      {
        text = resource[name];
        return true;
      }
      if (saveIfNotExist)
        resource.Add(name, "#" + name);
    }
    return false;
  }

  private bool TryGetText(
    string context,
    CultureInfo culture,
    string name,
    out string text,
    bool saveIfNotExist = false)
  {
    return this.TryGetText($"{context}.{culture.Name}", name, out text) || this.TryGetText($"{context}.{culture.TwoLetterISOLanguageName}", name, out text) || this.TryGetText(context, name, out text, saveIfNotExist);
  }
}
