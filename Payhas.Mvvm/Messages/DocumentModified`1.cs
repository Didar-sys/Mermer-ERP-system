// Decompiled with JetBrains decompiler
// Type: Payhas.Mvvm.Messages.DocumentModified`1
// Assembly: Payhas.Mvvm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3EAA5570-F618-4E39-B929-F7374F99B43D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Mvvm.dll

using MvvmCross.Plugins.Messenger;

#nullable disable
namespace Payhas.Mvvm.Messages;

public class DocumentModified<T> : MvxMessage
{
  public DocumentModified(object sender, string id)
    : base(sender)
  {
    this.Id = id;
  }

  public string Id { get; set; }
}
