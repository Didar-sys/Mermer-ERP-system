// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Extenders.ObservableCollectionExtender
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System.Collections.ObjectModel;
using System.Linq;

#nullable disable
namespace Mermer.Data.Extenders;

public static class ObservableCollectionExtender
{
  public static T RemoveWithSelection<T>(this ObservableCollection<T> collection, T item) where T : class
  {
    int num = collection.IndexOf(item);
    collection.Remove(item);
    int index = num - 1;
    if (index < 0)
      index = 0;
    return index < collection.Count ? collection.ElementAt<T>(index) : default (T);
  }
}
