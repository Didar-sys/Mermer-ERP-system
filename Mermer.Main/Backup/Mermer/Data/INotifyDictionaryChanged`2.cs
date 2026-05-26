// Decompiled with JetBrains decompiler
// Type: Mermer.Data.INotifyDictionaryChanged`2
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer.Data;

public interface INotifyDictionaryChanged<TKey, TValue>
{
  event ValueChangedEventHandler<TKey, TValue> ValueChanged;

  event EventHandler CollectionChanged;
}
