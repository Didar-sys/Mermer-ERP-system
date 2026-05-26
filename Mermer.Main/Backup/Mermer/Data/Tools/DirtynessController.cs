// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Tools.DirtynessController
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

#nullable disable
namespace Mermer.Data.Tools;

public static class DirtynessController
{
  private static readonly Dictionary<string, bool> DirtyEffectiveList = new Dictionary<string, bool>();

  public static Func<IEnumerable<T>> ControlList<T>(IEnumerable<T> list, Action<T> setDirtyAction) where T : INotifyPropertyChanged
  {
    List<T> dirtyList = new List<T>();
    foreach (T document in list)
      DirtynessController.ControlDocument<T>(document, new Action<T>(SetDirtyActionExtended));
    if (list is ObservableCollection<T> observableCollection)
      observableCollection.CollectionChanged += (NotifyCollectionChangedEventHandler) ((sender, e) =>
      {
        if (e.Action != NotifyCollectionChangedAction.Add)
          return;
        foreach (T document in e.NewItems.OfType<T>())
          DirtynessController.ControlDocument<T>(document, new Action<T>(SetDirtyActionExtended));
      });
    return (Func<IEnumerable<T>>) (() => (IEnumerable<T>) dirtyList);

    void SetDirtyActionExtended(T doc)
    {
      if (!dirtyList.Contains(doc))
        dirtyList.Add(doc);
      setDirtyAction(doc);
    }
  }

  public static void ControlDocument<T>(T document, Action<T> setDirtyAction) where T : INotifyPropertyChanged
  {
    document.PropertyChanged += (PropertyChangedEventHandler) ((sender, e) =>
    {
      string key = document.GetType()?.ToString() + e.PropertyName;
      if (!DirtynessController.DirtyEffectiveList.ContainsKey(key))
      {
        bool flag = TypeExtensions.GetProperty(document.GetType(), e.PropertyName).GetCustomAttribute(typeof (NotDirtyEffectiveAttribute)) == null;
        DirtynessController.DirtyEffectiveList.Add(key, flag);
      }
      if (!DirtynessController.DirtyEffectiveList[key])
        return;
      setDirtyAction(document);
    });
    foreach (PropertyInfo runtimeProperty in typeof (T).GetRuntimeProperties())
    {
      if (runtimeProperty.PropertyType.IsConstructedGenericType && runtimeProperty.PropertyType.IsOfGenericType(typeof (ObservableCollection<>)))
      {
        MethodInfo methodInfo = typeof (DirtynessController).GetRuntimeMethods().First<MethodInfo>((Func<MethodInfo, bool>) (m => m.Name == "ControlSubList"));
        Type genericTypeArgument = runtimeProperty.PropertyType.GenericTypeArguments[0];
        if (((IEnumerable<Type>) TypeExtensions.GetInterfaces(genericTypeArgument)).Contains<Type>(typeof (INotifyPropertyChanged)))
          methodInfo.MakeGenericMethod(genericTypeArgument, typeof (T)).Invoke((object) null, new object[3]
          {
            runtimeProperty.GetValue((object) document),
            (object) document,
            (object) setDirtyAction
          });
      }
    }
  }

  public static void ControlSubList<TDoc, TRoot>(
    ObservableCollection<TDoc> list,
    TRoot rootDocument,
    Action<TRoot> setDirtyAction)
    where TDoc : INotifyPropertyChanged
  {
    if (list == null)
      return;
    foreach (TDoc document in (Collection<TDoc>) list)
      DirtynessController.ControlSubDocument<TDoc, TRoot>(document, rootDocument, setDirtyAction);
    list.CollectionChanged += (NotifyCollectionChangedEventHandler) ((sender, e) =>
    {
      if (e.Action != NotifyCollectionChangedAction.Add)
        return;
      setDirtyAction(rootDocument);
      foreach (TDoc document in e.NewItems.OfType<TDoc>())
        DirtynessController.ControlSubDocument<TDoc, TRoot>(document, rootDocument, setDirtyAction);
    });
  }

  public static void ControlSubDocument<TDoc, TRoot>(
    TDoc document,
    TRoot rootDocument,
    Action<TRoot> setDirtyAction)
    where TDoc : INotifyPropertyChanged
  {
    document.PropertyChanged += (PropertyChangedEventHandler) ((sender, e) =>
    {
      string key = document.GetType()?.ToString() + e.PropertyName;
      if (!DirtynessController.DirtyEffectiveList.ContainsKey(key))
      {
        bool flag = TypeExtensions.GetProperty(document.GetType(), e.PropertyName).GetCustomAttribute(typeof (NotDirtyEffectiveAttribute)) == null;
        DirtynessController.DirtyEffectiveList.Add(key, flag);
      }
      if (!DirtynessController.DirtyEffectiveList[key])
        return;
      setDirtyAction(rootDocument);
    });
    foreach (PropertyInfo runtimeProperty in typeof (TDoc).GetRuntimeProperties())
    {
      if (runtimeProperty.PropertyType.IsConstructedGenericType && runtimeProperty.PropertyType.IsOfGenericType(typeof (ObservableCollection<>)))
        typeof (DirtynessController).GetRuntimeMethods().First<MethodInfo>((Func<MethodInfo, bool>) (m => m.Name == "ControlSubList")).MakeGenericMethod(runtimeProperty.PropertyType.GenericTypeArguments[0], typeof (TDoc)).Invoke((object) null, new object[3]
        {
          runtimeProperty.GetValue((object) document),
          (object) document,
          (object) setDirtyAction
        });
    }
  }
}
