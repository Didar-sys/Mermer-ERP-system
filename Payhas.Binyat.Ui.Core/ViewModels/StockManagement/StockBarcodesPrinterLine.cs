// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.StockManagement.StockBarcodesPrinterLine
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using Payhas.Data.Models;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.StockManagement;

public class StockBarcodesPrinterLine : BindableObject
{
  private string _barcode;
  private int _copiesCount;

  public virtual string Barcode
  {
    get => this._barcode;
    set => this.SetProperty<string>(ref this._barcode, value, nameof (Barcode));
  }

  public virtual int CopiesCount
  {
    get => this._copiesCount;
    set => this.SetProperty<int>(ref this._copiesCount, value, nameof (CopiesCount));
  }
}
