// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.EnumerableExtender
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using System.Linq;

#nullable disable
namespace System.Collections.Generic;

public static class EnumerableExtender
{
  private static readonly Random Rnd = new Random();

  public static T GetRandom<T>(this IEnumerable<T> list)
  {
    if (!(list is IList<T> objList))
      objList = (IList<T>) list.ToList<T>();
    IList<T> source = objList;
    return source.Count > 0 ? source.ElementAt<T>(EnumerableExtender.Rnd.Next(0, source.Count - 1)) : default (T);
  }
}
