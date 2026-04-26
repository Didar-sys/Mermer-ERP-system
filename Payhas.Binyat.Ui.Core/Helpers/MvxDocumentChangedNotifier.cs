// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.Helpers.MvxDocumentChangedNotifier
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Common.Services;
using Payhas.Mvvm.Messages;
using System;

#nullable disable
namespace Payhas.Binyat.Ui.Core.Helpers;

public class MvxDocumentChangedNotifier : AbstractDocumentChangedNotifier
{
  public override void DocumentChanged<T>(string id)
  {
    try
    {
      Mvx.Resolve<IMvxMessenger>()?.Publish<DocumentModified<T>>(new DocumentModified<T>((object) this, id));
    }
    catch (Exception ex)
    {
    }
  }
}
