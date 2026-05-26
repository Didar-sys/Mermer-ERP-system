// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.Helpers.ListFilter
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.ViewModels;
using Mermer.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.Helpers;

public class ListFilter : BindableObject
{
  private string _title;
  private int? _itemsCount;
  private Func<ListFilter, Task<int>> _counter;
  private Func<ListFilter, bool> _canLoad;
  private Func<ListFilter, Task> _loader;
  private object _tag;
  private bool _isSelected;

  public string Title
  {
    get => this._title;
    set => this.SetProperty<string>(ref this._title, value, nameof (Title));
  }

  public string Initials
  {
    get
    {
      return string.Concat(((IEnumerable<string>) this.Title.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries)).Select<string, string>((Func<string, string>) (t => t.Substring(0, 1))));
    }
  }

  public int? ItemsCount
  {
    get => this._itemsCount;
    set => this.SetProperty<int?>(ref this._itemsCount, value, nameof (ItemsCount));
  }

  public Func<ListFilter, Task<int>> Counter
  {
    get => this._counter;
    set
    {
      this.SetProperty<Func<ListFilter, Task<int>>>(ref this._counter, value, nameof (Counter));
    }
  }

  public Func<ListFilter, bool> CanLoad
  {
    get => this._canLoad;
    set => this.SetProperty<Func<ListFilter, bool>>(ref this._canLoad, value, nameof (CanLoad));
  }

  public Func<ListFilter, Task> Loader
  {
    get => this._loader;
    set => this.SetProperty<Func<ListFilter, Task>>(ref this._loader, value, nameof (Loader));
  }

  public ICommand Command
  {
    get
    {
      return (ICommand) new MvxAsyncCommand<bool>((Func<bool, Task>) (x => this.Loader(this)), (Func<bool, bool>) (x => x || this.CanLoad(this)));
    }
  }

  public virtual object Tag
  {
    get => this._tag;
    set => this.SetProperty<object>(ref this._tag, value, nameof (Tag));
  }

  public virtual bool IsSelected
  {
    get => this._isSelected;
    set => this.SetProperty<bool>(ref this._isSelected, value, nameof (IsSelected));
  }

  public async Task Initialize()
  {
    ListFilter listFilter = this;
    try
    {
      int num = await listFilter.Counter(listFilter);
      listFilter.ItemsCount = new int?(num);
    }
    catch (Exception ex)
    {
      listFilter.ItemsCount = new int?();
    }
  }
}
