// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Tools.ObservableCollectionWatcher`1
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

#nullable disable
namespace Mermer.Data.Tools;

public class ObservableCollectionWatcher<T> : IObservableCollectionWatcher where T : INotifyPropertyChanged
{
  private readonly ObservableCollection<T> _collection;

  public ObservableCollectionWatcher(ObservableCollection<T> collection)
  {
    this._collection = collection;
    this.Init();
  }

  private void Init()
  {
    if (this._collection == null)
      return;
    this._collection.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnCollectionChanged);
    foreach (T obj in (Collection<T>) this._collection)
      obj.PropertyChanged += new PropertyChangedEventHandler(this.Item_PropertyChanged);
  }

  private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    if (e.OldItems != null)
    {
      foreach (object oldItem in (IEnumerable) e.OldItems)
      {
        if (oldItem is INotifyPropertyChanged notifyPropertyChanged)
          notifyPropertyChanged.PropertyChanged -= new PropertyChangedEventHandler(this.Item_PropertyChanged);
      }
    }
    if (e.NewItems != null)
    {
      foreach (object newItem in (IEnumerable) e.NewItems)
      {
        if (newItem is INotifyPropertyChanged notifyPropertyChanged)
          notifyPropertyChanged.PropertyChanged += new PropertyChangedEventHandler(this.Item_PropertyChanged);
      }
    }
    this.OnItemsChanged();
  }

  private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    this.OnItemPropertyChanged(sender, e);
    this.OnItemsChanged();
  }

  public event ItemPropertyChangedEventHandler ItemPropertyChanged;

  protected virtual void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    ItemPropertyChangedEventHandler itemPropertyChanged = this.ItemPropertyChanged;
    if (itemPropertyChanged == null)
      return;
    itemPropertyChanged(sender, e);
  }

  public event ItemsChangedEventHandler ItemsChanged;

  protected virtual void OnItemsChanged()
  {
    ItemsChangedEventHandler itemsChanged = this.ItemsChanged;
    if (itemsChanged == null)
      return;
    itemsChanged();
  }
}
