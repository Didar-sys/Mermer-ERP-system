// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Tools.AutoNotifyPropertyChanged
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using Castle.DynamicProxy;
using System;
using System.ComponentModel;

#nullable disable
namespace Payhas.Data.Tools;

public class AutoNotifyPropertyChanged
{
  public static T Wrap<T>(T target)
  {
    return (T) new ProxyGenerator().CreateClassProxyWithTarget(typeof (T), new Type[1]
    {
      typeof (INotifyPropertyChanged)
    }, (object) target, new IInterceptor[1]
    {
      (IInterceptor) new AutoNotifyPropertyChangedInterceptor()
    });
  }

  public static T Unwrap<T>(T proxy)
  {
    if (!ProxyUtil.IsProxy((object) proxy))
      return proxy;
    try
    {
      return (T) ((IProxyTargetAccessor) (object) proxy).DynProxyGetTarget();
    }
    catch
    {
      return proxy;
    }
  }
}
