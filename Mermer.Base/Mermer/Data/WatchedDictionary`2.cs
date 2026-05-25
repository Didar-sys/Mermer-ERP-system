// Decompiled with JetBrains decompiler
// Type: Mermer.Data.WatchedDictionary`2
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace Mermer.Data;

public class WatchedDictionary<TKey, TValue> : 
  IDictionary<TKey, TValue>,
  ICollection<KeyValuePair<TKey, TValue>>,
  IEnumerable<KeyValuePair<TKey, TValue>>,
  IEnumerable,
  INotifyDictionaryChanged<TKey, TValue>
{
  private IDictionary<TKey, TValue> _dictionary;

  public WatchedDictionary()
    : this(new Dictionary<TKey, TValue>())
  {
  }

  public WatchedDictionary(Dictionary<TKey, TValue> dictionary)
  {
    this._dictionary = (IDictionary<TKey, TValue>) dictionary;
  }

  public event ValueChangedEventHandler<TKey, TValue> ValueChanged;

  public void OnValueChanged(TKey key, TValue newValue = default, TValue oldValue = default)
  {
    if (this.ValueChanged == null)
      return;
    this.ValueChanged((object) this, new ValueChangedEventArgs<TKey, TValue>(key, newValue, oldValue));
  }

  public event EventHandler CollectionChanged;

  public void OnCollectionChanged()
  {
    if (this.CollectionChanged == null)
      return;
    this.CollectionChanged((object) this, new EventArgs());
  }

  public TValue this[TKey key]
  {
    get => !this._dictionary.ContainsKey(key) ? default (TValue) : this._dictionary[key];
    set
    {
      TValue oldValue = this[key];
      if (oldValue.Equals((object) value))
        return;
      if (this._dictionary.ContainsKey(key))
        this._dictionary[key] = value;
      else
        this._dictionary.Add(key, value);
      this.OnValueChanged(key, value, oldValue);
    }
  }

  public ICollection<TKey> Keys => this._dictionary.Keys;

  public ICollection<TValue> Values => this._dictionary.Values;

  public int Count => this._dictionary.Count;

  public bool IsReadOnly => this._dictionary.IsReadOnly;

  public void Add(TKey key, TValue value)
  {
    this._dictionary.Add(key, value);
    this.OnValueChanged(key, value);
  }

  public void Add(KeyValuePair<TKey, TValue> item) => this.Add(item.Key, item.Value);

  public void Clear()
  {
    this._dictionary.Clear();
    this.OnCollectionChanged();
  }

  public bool Contains(KeyValuePair<TKey, TValue> item) => this._dictionary.Contains(item);

  public bool ContainsKey(TKey key) => this._dictionary.ContainsKey(key);

  public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
  {
    this._dictionary.CopyTo(array, arrayIndex);
  }

  public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
  {
    return this._dictionary.GetEnumerator();
  }

  public bool Remove(TKey key)
  {
    int num = this._dictionary.Remove(key) ? 1 : 0;
    this.OnValueChanged(key);
    return num != 0;
  }

  public bool Remove(KeyValuePair<TKey, TValue> item)
  {
    int num = ((ICollection<KeyValuePair<TKey, TValue>>) this._dictionary).Remove(item) ? 1 : 0;
    this.OnValueChanged(item.Key);
    return num != 0;
  }

  public bool TryGetValue(TKey key, out TValue value)
  {
    return this._dictionary.TryGetValue(key, out value);
  }

  IEnumerator IEnumerable.GetEnumerator() => this._dictionary.GetEnumerator();
}
