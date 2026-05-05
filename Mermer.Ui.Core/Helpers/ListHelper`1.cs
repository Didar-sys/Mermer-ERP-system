// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.Helpers.ListHelper`1
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

#nullable disable
namespace Mermer.Ui.Core.Helpers;

public class ListHelper<T>
{
  public ListHelper()
  {
  }

  public ListHelper(T value)
    : this(value, value.ToString())
  {
  }

  public ListHelper(T value, string text)
  {
    this.Value = value;
    this.Text = text;
  }

  public T Value { get; set; }

  public string Text { get; set; }
}
