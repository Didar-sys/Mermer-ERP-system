// Decompiled with JetBrains decompiler
// Type: Payhas.Mvvm.Tools.IJsonLocalizationResourceProvider
// Assembly: Payhas.Mvvm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3EAA5570-F618-4E39-B929-F7374F99B43D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Mvvm.dll

using System.Collections.Generic;

#nullable disable
namespace Payhas.Mvvm.Tools;

public interface IJsonLocalizationResourceProvider
{
  void UpdateResources();

  bool TryGetResource(string context, out Dictionary<string, string> resource);
}
