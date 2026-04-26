// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Services.ExcelReaderService
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Spreadsheet;
using Payhas.Binyat.Ui.Core.Services;
using System;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Services;

public class ExcelReaderService : IExcelReaderService
{
  protected string FileTitle;
  private Workbook _workbook;
  private Worksheet _worksheet;

  public ExcelReaderService() => this._workbook = new Workbook();

  public void CloseExcelFile() => this._workbook.Dispose();

  public object GetValue(int row, int column)
  {
    if (this._worksheet != null)
    {
      try
      {
        Cell cell = this._worksheet.Cells[row, column];
        return cell != null ? (cell.Value.IsEmpty ? (object) null : (cell.Value.IsNumeric ? (object) cell.Value.NumericValue : (cell.Value.IsText ? (object) cell.Value.TextValue.Trim() : (cell.Value.IsDateTime ? (object) cell.Value.DateTimeValue : (cell.Value.IsBoolean ? (object) cell.Value.BooleanValue : (object) null))))) : (object) null;
      }
      catch (Exception ex)
      {
      }
    }
    return (object) null;
  }

  public void OpenExcelFile(string filename)
  {
    if (this._workbook.IsDisposed)
      this._workbook = new Workbook();
    if (!this._workbook.LoadDocument(filename))
      return;
    this.SetWorksheet(0);
  }

  public void SetValue(object value, int row, int column)
  {
    if (this._worksheet != null)
      this._worksheet.Cells[row, column].SetValue(value);
    throw new Exception("FileNotFound");
  }

  private void SetWorksheet(int index) => this._worksheet = this._workbook.Worksheets[0];
}
