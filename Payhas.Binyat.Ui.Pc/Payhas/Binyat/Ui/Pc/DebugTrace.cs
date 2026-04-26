// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.DebugTrace
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using MvvmCross.Platform.Platform;
using System;

#nullable disable
namespace Payhas.Binyat.Ui.Pc;

public class DebugTrace : IMvxTrace
{
  public void Trace(MvxTraceLevel level, string tag, Func<string> message)
  {
  }

  public void Trace(MvxTraceLevel level, string tag, string message)
  {
  }

  public void Trace(MvxTraceLevel level, string tag, string message, params object[] args)
  {
    try
    {
    }
    catch (FormatException ex)
    {
      this.Trace(MvxTraceLevel.Error, tag, "Exception during trace of {0} {1}", new object[2]
      {
        (object) level,
        (object) message
      });
    }
  }
}
