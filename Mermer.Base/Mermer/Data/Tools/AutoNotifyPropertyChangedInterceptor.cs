// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Tools.AutoNotifyPropertyChangedInterceptor
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Castle.DynamicProxy;
using System;
using System.ComponentModel;

#nullable disable
namespace Mermer.Data.Tools;

public class AutoNotifyPropertyChangedInterceptor : IInterceptor
{
  private PropertyChangedEventHandler _handler;

  public void Intercept(IInvocation invocation)
  {
    if (invocation.Method.Name == "add_PropertyChanged")
      this._handler = (PropertyChangedEventHandler) Delegate.Combine((Delegate) this._handler, (Delegate) invocation.Arguments[0]);
    else if (invocation.Method.Name == "remove_PropertyChanged")
      this._handler = (PropertyChangedEventHandler) Delegate.Remove((Delegate) this._handler, (Delegate) invocation.Arguments[0]);
    else if (invocation.Method.Name.StartsWith("set_"))
    {
      invocation.Proceed();
      PropertyChangedEventHandler handler = this._handler;
      if (handler == null)
        return;
      handler(invocation.Proxy, new PropertyChangedEventArgs(invocation.Method.Name.Substring("set_".Length)));
    }
    else
      invocation.Proceed();
  }
}
