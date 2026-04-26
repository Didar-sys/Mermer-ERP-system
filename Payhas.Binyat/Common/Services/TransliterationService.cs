// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Common.Services.TransliterationService
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Data.Extenders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Common.Services;

public class TransliterationService : ITransliterationService
{
  public Task<IEnumerable<string>> Parse(string text)
  {
    return Task.Run<IEnumerable<string>>((Func<IEnumerable<string>>) (() =>
    {
      List<string> source = new List<string>();
      string str = text;
      char[] separator = new char[1]{ ' ' };
      foreach (string text1 in str.Split(separator, StringSplitOptions.RemoveEmptyEntries))
      {
        source.Add(text1.ConvertToEn());
        source.Add(text1.ConvertToRu());
        source.Add(text1.LayoutEn());
        source.Add(text1.LayoutRu());
      }
      return source.Distinct<string>();
    }));
  }

  public IEnumerable<string> Parse(string text, TransliterationType type)
  {
    string[] source = text.Split(new char[1]{ ' ' }, StringSplitOptions.RemoveEmptyEntries);
    switch (type)
    {
      case TransliterationType.ConvertEn:
        return ((IEnumerable<string>) source).Select<string, string>((Func<string, string>) (x => x.ConvertToEn()));
      case TransliterationType.LayoutEn:
        return ((IEnumerable<string>) source).Select<string, string>((Func<string, string>) (x => x.LayoutEn()));
      case TransliterationType.ConvertRu:
        return ((IEnumerable<string>) source).Select<string, string>((Func<string, string>) (x => x.ConvertToRu()));
      case TransliterationType.LayoutRu:
        return ((IEnumerable<string>) source).Select<string, string>((Func<string, string>) (x => x.LayoutRu()));
      default:
        throw new ArgumentOutOfRangeException(nameof (type), (object) type, (string) null);
    }
  }
}
