// Decompiled with JetBrains decompiler
// Type: Mermer.StockManagement.Models.StockBalanceWithCodeAndName
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

#nullable disable
namespace Mermer.StockManagement.Models;

public class StockBalanceWithCodeAndName : StockBalance
{
  public string StockCode { get; set; }

  public string StockName { get; set; }
}
