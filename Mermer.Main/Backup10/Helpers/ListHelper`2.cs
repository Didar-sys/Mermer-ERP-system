// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.Helpers.ListHelper`2
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using Mermer.Data.Models;

#nullable disable
namespace Mermer.Ui.Core.Helpers;

public class ListHelper<TKey, TValue> : BindableObject
{
  private TKey _key;
  private TValue _value;

  public ListHelper()
  {
  }

  public ListHelper(TKey key, TValue value)
  {
    this.Key = key;
    this.Value = value;
  }

  public TKey Key
  {
    get => this._key;
    set => this.SetProperty<TKey>(ref this._key, value, nameof (Key));
  }

  public TValue Value
  {
    get => this._value;
    set => this.SetProperty<TValue>(ref this._value, value, nameof (Value));
  }
}
