// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Services.UserInteractionService
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.Xpf.WindowsUI;
using Mermer.Mvvm.Services;
using System;
using System.Text;
using System.Windows;

#nullable disable
namespace Mermer.Ui.Pc.Services;

public class UserInteractionService : IUserInteractionService
{
  public void ShowExceptionMessage(Exception exception, string caption = null)
  {
    StringBuilder sb = new StringBuilder();
    if (string.IsNullOrEmpty(caption))
    {
      caption = exception.Message;
      this.GetMessage(sb, exception.InnerException);
    }
    else
      this.GetMessage(sb, exception);
    this.ShowMessage(caption, sb.ToString(), UserInteractionType.Ok);
  }

  private void GetMessage(StringBuilder sb, Exception exception)
  {
    if (exception == null)
      return;
    if (exception.Message != "Exception of type 'Mermer.Common.Exceptions.AuthorizationFailedException' was thrown.")
      sb.AppendLine(exception.Message);
    this.GetMessage(sb, exception.InnerException);
  }

  public bool? ShowMessage(string caption, string message, UserInteractionType type = UserInteractionType.Ok)
  {
    MessageBoxButton button;
    switch (type)
    {
      case UserInteractionType.Ok:
        button = MessageBoxButton.OK;
        break;
      case UserInteractionType.YesNo:
        button = MessageBoxButton.YesNo;
        break;
      case UserInteractionType.YesNoCancel:
        button = MessageBoxButton.YesNoCancel;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof (type), (object) type, (string) null);
    }
    switch (WinUIMessageBox.Show((FrameworkElement) MainWindow.Instance.Root, message, caption, button))
    {
      case MessageBoxResult.Yes:
        return new bool?(true);
      case MessageBoxResult.No:
        return new bool?(false);
      default:
        return new bool?();
    }
  }
}
