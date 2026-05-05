// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Synchronizer.Core.Couch.Helpers.AssemblyResourceReaderExtension
// Assembly: Mermer.Data.Synchronizer.Core.Couch, Version=0.0.9.0, Culture=neutral, PublicKeyToken=null
// MVID: 7309E162-8E25-4800-97C2-B3CD230F4B8B
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Data.Synchronizer.Core.Couch.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Data.Synchronizer.Core.Couch.Helpers;

public static class AssemblyResourceReaderExtension
{
  public static async Task<string> ReadResourceAsync(this Assembly assembly, string resourceName)
  {
    string[] resources = assembly.GetManifestResourceNames();
    string str;
    using (Stream stream = assembly.GetManifestResourceStream(((IEnumerable<string>) resources).Single<string>((Func<string, bool>) (x => x.EndsWith(resourceName)))))
    {
      using (StreamReader reader = new StreamReader(stream))
        str = await reader.ReadToEndAsync();
    }
    string[] strArray = resources;
    for (int index = 0; index < strArray.Length; ++index)
    {
      string resourceName1 = strArray[index];
      while (resourceName1.Length > 0)
      {
        string oldValue1;
        string oldValue2;
        if (str.Contains("//import:" + resourceName1))
        {
          oldValue1 = str;
          oldValue2 = "//import:" + resourceName1;
          str = oldValue1.Replace(oldValue2, await assembly.ReadResourceAsync(resourceName1));
          oldValue1 = (string) null;
          oldValue2 = (string) null;
          break;
        }
        if (str.Contains(resourceName1))
        {
          oldValue2 = str;
          oldValue1 = resourceName1;
          str = oldValue2.Replace(oldValue1, await assembly.ReadResourceAsync(resourceName1));
          oldValue2 = (string) null;
          oldValue1 = (string) null;
          break;
        }
        int startIndex = resourceName1.IndexOf(".", StringComparison.Ordinal) + 1;
        if (startIndex != 0 && startIndex != resourceName1.Length)
        {
          resourceName1 = resourceName1.Substring(startIndex);
          if (!resourceName1.Contains("."))
            break;
        }
        else
          break;
      }
    }
    strArray = (string[]) null;
    return str.Replace(Environment.NewLine, resourceName.EndsWith(".json") ? "" : "\\n");
  }
}
