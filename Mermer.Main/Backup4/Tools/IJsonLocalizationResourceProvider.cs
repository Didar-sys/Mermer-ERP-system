// Decompiled with JetBrains decompiler
// Type: Mermer.Mvvm.Tools.IJsonLocalizationResourceProvider
// Assembly: Mermer.Mvvm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3EAA5570-F618-4E39-B929-F7374F99B43D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Mvvm.dll

using System.Collections.Generic;

#nullable disable
namespace Mermer.Mvvm.Tools;

public interface IJsonLocalizationResourceProvider
{
  void UpdateResources();

  bool TryGetResource(string context, out Dictionary<string, string> resource);
}
