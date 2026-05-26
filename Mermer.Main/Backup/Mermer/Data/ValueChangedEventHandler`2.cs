// Decompiled with JetBrains decompiler
// Type: Mermer.Data.ValueChangedEventHandler`2
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

#nullable disable
namespace Mermer.Data;

public delegate void ValueChangedEventHandler<TKey, TValue>(
  object sender,
  ValueChangedEventArgs<TKey, TValue> e);
