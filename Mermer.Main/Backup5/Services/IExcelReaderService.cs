// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.Services.IExcelReaderService
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

#nullable disable
namespace Mermer.Ui.Core.Services;

public interface IExcelReaderService
{
  void OpenExcelFile(string filename);

  object GetValue(int row, int column);

  void SetValue(object value, int row, int column);

  void CloseExcelFile();
}
