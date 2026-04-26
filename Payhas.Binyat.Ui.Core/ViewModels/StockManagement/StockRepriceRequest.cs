// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.StockManagement.StockRepriceRequest
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using System;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.StockManagement;

public class StockRepriceRequest
{
  public string StockId { get; set; }

  public Decimal ReferencePrice { get; set; }

  public string ReferencePriceCurrencyId { get; set; }
}
