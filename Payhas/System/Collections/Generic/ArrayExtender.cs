// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.ArrayExtender
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using System.Linq;

#nullable disable
namespace System.Collections.Generic;

public static class ArrayExtender
{
  public static T[] Add<T>(this T[] array, T item)
  {
    List<T> list = ((IEnumerable<T>) array).ToList<T>();
    list.Add(item);
    return list.ToArray();
  }
}
