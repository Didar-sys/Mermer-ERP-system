// Decompiled with JetBrains decompiler
// Type: Payhas.Data.WatchedObservableCollection`1
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using Payhas.Data.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

#nullable disable
namespace Payhas.Data;

public class WatchedObservableCollection<T> : ObservableCollection<T>, IWatchedObservableCollection where T : INotifyPropertyChanged
{
  public WatchedObservableCollection()
  {
    this.Watcher = (IObservableCollectionWatcher) new ObservableCollectionWatcher<T>((ObservableCollection<T>) this);
  }

  public WatchedObservableCollection(IEnumerable<T> collection)
    : base(collection)
  {
    this.Watcher = (IObservableCollectionWatcher) new ObservableCollectionWatcher<T>((ObservableCollection<T>) this);
  }

  public IObservableCollectionWatcher Watcher { get; }

  public void ForEach(Action<T> action)
  {
    foreach (T obj in (Collection<T>) this)
      action(obj);
  }
}
