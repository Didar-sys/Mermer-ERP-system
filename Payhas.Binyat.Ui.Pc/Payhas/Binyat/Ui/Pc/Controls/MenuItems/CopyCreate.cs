// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Controls.MenuItems.CopyCreate
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Xpf.WindowsUI;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Controls.MenuItems;

public class CopyCreate : AppBarButton, IComponentConnector
{
  public static readonly DependencyProperty OrderOnlyProperty = DependencyProperty.Register(nameof (OrderOnly), typeof (bool), typeof (CopyCreate), new PropertyMetadata((object) false));
  internal CopyCreate This;
  private bool _contentLoaded;

  public CopyCreate() => this.InitializeComponent();

  public bool OrderOnly
  {
    get => (bool) this.GetValue(CopyCreate.OrderOnlyProperty);
    set => this.SetValue(CopyCreate.OrderOnlyProperty, (object) value);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/controls/menuitems/copycreate.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  internal Delegate _CreateDelegate(Type delegateType, string handler)
  {
    return Delegate.CreateDelegate(delegateType, (object) this, handler);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    if (connectionId == 1)
      this.This = (CopyCreate) target;
    else
      this._contentLoaded = true;
  }
}
