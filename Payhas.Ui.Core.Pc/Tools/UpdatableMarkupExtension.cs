// Decompiled with JetBrains decompiler
// Type: Payhas.Ui.Core.Pc.Tools.UpdatableMarkupExtension
// Assembly: Payhas.Ui.Core.Pc, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99463FBB-953B-46DD-9DD6-5278306A8C84
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Ui.Core.Pc.dll

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Ui.Core.Pc.Tools;

public abstract class UpdatableMarkupExtension : MarkupExtension
{
  protected object TargetObject { get; private set; }

  protected object TargetProperty { get; private set; }

  public sealed override object ProvideValue(IServiceProvider serviceProvider)
  {
    if (!(serviceProvider.GetService(typeof (IProvideValueTarget)) is IProvideValueTarget service))
      return this.ProvideValueInternal(serviceProvider);
    this.TargetObject = service.TargetObject;
    this.TargetProperty = service.TargetProperty;
    if (this.TargetObject is FrameworkElement targetObject)
      targetObject.DataContextChanged += new DependencyPropertyChangedEventHandler(this.TargetObjectDataContextChanged);
    return this.ProvideValueInternal(serviceProvider);
  }

  protected void UpdateValue(object value)
  {
    if (this.TargetObject == null)
      return;
    DependencyProperty prop1 = this.TargetProperty as DependencyProperty;
    if (prop1 != null)
    {
      DependencyObject obj = (DependencyObject) this.TargetObject;
      if (obj.CheckAccess())
        UpdateAction();
      else
        obj.Dispatcher.Invoke(new Action(UpdateAction));

      void UpdateAction() => obj.SetValue(prop1, value);
    }
    else
      ((PropertyInfo) this.TargetProperty).SetValue(this.TargetObject, value, (object[]) null);
  }

  protected abstract object ProvideValueInternal(IServiceProvider serviceProvider);

  protected virtual void TargetObjectDataContextChanged(
    object sender,
    DependencyPropertyChangedEventArgs e)
  {
  }
}
