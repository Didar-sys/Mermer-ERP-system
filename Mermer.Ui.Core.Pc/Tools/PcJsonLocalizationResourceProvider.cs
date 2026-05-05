// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.Pc.Tools.PcJsonLocalizationResourceProvider
// Assembly: Mermer.Ui.Core.Pc, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99463FBB-953B-46DD-9DD6-5278306A8C84
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.Pc.dll

using Newtonsoft.Json;
using Mermer.Mvvm.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable disable
namespace Mermer.Ui.Core.Pc.Tools;

public class PcJsonLocalizationResourceProvider : IJsonLocalizationResourceProvider, IDisposable
{
  private readonly string _rootPath;
  private readonly Dictionary<string, Dictionary<string, string>> _resources;

  public PcJsonLocalizationResourceProvider(string rootPath)
  {
    this._rootPath = rootPath;
    this._resources = new Dictionary<string, Dictionary<string, string>>();
    foreach (FileInfo fileInfo in ((IEnumerable<FileInfo>) new DirectoryInfo(this._rootPath).GetFiles()).ToList<FileInfo>())
    {
      Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(fileInfo.FullName));
      this._resources.Add(Path.GetFileNameWithoutExtension(fileInfo.Name), dictionary);
    }
  }

  public void UpdateResources()
  {
    foreach (KeyValuePair<string, Dictionary<string, string>> resource in this._resources)
    {
      string contents = JsonConvert.SerializeObject((object) resource.Value);
      File.WriteAllText(Path.Combine(this._rootPath, resource.Key) + ".json", contents);
    }
  }

  public bool TryGetResource(string context, out Dictionary<string, string> resource)
  {
    resource = this._resources.ContainsKey(context) ? this._resources[context] : (Dictionary<string, string>) null;
    return resource != null;
  }

  public void Dispose() => this.UpdateResources();
}
