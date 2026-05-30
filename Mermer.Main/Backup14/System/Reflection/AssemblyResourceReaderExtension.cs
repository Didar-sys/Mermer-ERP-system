// Decompiled with JetBrains decompiler
// Type: System.Reflection.AssemblyResourceReaderExtension
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace System.Reflection;

public static class AssemblyResourceReaderExtension
{
  public static async Task<string> ReadResourceAsync(this Assembly assembly, string resourceName)
  {
    string[] resources = assembly.GetManifestResourceNames();
    string str1;
    using (Stream stream = assembly.GetManifestResourceStream(((IEnumerable<string>) resources).Single<string>((Func<string, bool>) (x => x.EndsWith(resourceName)))))
    {
      using (StreamReader reader = new StreamReader(stream))
        str1 = await reader.ReadToEndAsync();
    }
    string[] strArray = resources;
    for (int index = 0; index < strArray.Length; ++index)
    {
      string resourceName1 = strArray[index];
      while (resourceName1.Length > 0)
      {
        string oldValue1;
        string oldValue2;
        if (str1.Contains("//import:" + resourceName1))
        {
          oldValue1 = str1;
          oldValue2 = "//import:" + resourceName1;
          str1 = oldValue1.Replace(oldValue2, await assembly.ReadResourceAsync(resourceName1));
          oldValue1 = (string) null;
          oldValue2 = (string) null;
          break;
        }
        if (str1.Contains(resourceName1))
        {
          oldValue2 = str1;
          oldValue1 = resourceName1;
          str1 = oldValue2.Replace(oldValue1, await assembly.ReadResourceAsync(resourceName1));
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
    string str2 = str1.Replace(Environment.NewLine, resourceName.EndsWith(".json") ? "" : "\\n");
    resources = (string[]) null;
    return str2;
  }
}
