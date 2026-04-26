// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Models.BindableObject
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using Payhas.Data.Extenders;
using Payhas.Data.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

#nullable disable
namespace Payhas.Data.Models;

public class BindableObject : INotifyPropertyChanged
{
  private readonly Dictionary<string, List<string>> _autoRaiseProperties;

  public BindableObject()
  {
    this._autoRaiseProperties = new Dictionary<string, List<string>>();
    this.PropertyChanged += new PropertyChangedEventHandler(this.This_PropertyChanged);
  }

  private void This_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (!this._autoRaiseProperties.ContainsKey(e.PropertyName))
      return;
    foreach (string propertyName in this._autoRaiseProperties[e.PropertyName])
      this.RaisePropertyChanged(propertyName);
  }

  public event PropertyChangedEventHandler PropertyChanged;

  [NotifyPropertyChangedInvocator]
  public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    if (!this._autoRaiseProperties.ContainsKey(propertyName))
      return;
    foreach (string propertyName1 in this._autoRaiseProperties[propertyName].Distinct<string>())
      this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName1));
  }

  public void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
  {
    this.RaisePropertyChanged(this.GetPropertyNameFromExpression<T>(propertyExpression));
  }

  protected virtual bool SetProperty<T>(
    ref T storage,
    T value,
    [CallerMemberName] string propertyName = null,
    params string[] propertiesToRaiseChange)
  {
    if (EqualityComparer<T>.Default.Equals(storage, value))
      return false;
    storage = value;
    this.RaisePropertyChanged(propertyName);
    if (((IEnumerable<string>) propertiesToRaiseChange).Any<string>())
    {
      foreach (string propertyName1 in propertiesToRaiseChange)
        this.RaisePropertyChanged(propertyName1);
      if ((object) value != null)
      {
        if (value is IWatchedObservableCollection observableCollection)
          observableCollection.Watcher.ItemsChanged += (ItemsChangedEventHandler) (() =>
          {
            foreach (string propertyName2 in propertiesToRaiseChange)
              this.RaisePropertyChanged(propertyName2);
          });
        else if (((object) value).IsInstanceOfGenericType(typeof (ObservableCollection<>)) && value.GetType().GenericTypeArguments.Length == 1 && TypeExtensions.IsAssignableFrom(typeof (INotifyPropertyChanged), value.GetType().GenericTypeArguments[0]))
          ((IObservableCollectionWatcher) Activator.CreateInstance(typeof (ObservableCollectionWatcher<>).MakeGenericType(value.GetType().GenericTypeArguments), (object) value)).ItemsChanged += (ItemsChangedEventHandler) (() =>
          {
            foreach (string propertyName3 in propertiesToRaiseChange)
              this.RaisePropertyChanged(propertyName3);
          });
      }
    }
    return true;
  }

  protected void AutoRaisePropertyChanged(Dictionary<string, string[]> properties)
  {
    foreach (KeyValuePair<string, string[]> property in properties)
      this.AutoRaisePropertyChanged(property.Key, property.Value);
  }

  protected void AutoRaisePropertyChanged(string propertyName, params string[] reletedPropertyNames)
  {
    if (!this._autoRaiseProperties.ContainsKey(propertyName))
      this._autoRaiseProperties.Add(propertyName, new List<string>());
    this._autoRaiseProperties[propertyName].AddRange((IEnumerable<string>) reletedPropertyNames);
  }
}
