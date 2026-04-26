// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.StockManagement.Mappers.StockMergeMapper
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using AutoMapper;
using Payhas.Binyat.StockManagement.Services;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.StockManagement.Mappers;

public class StockMergeMapper : Profile
{
  public StockMergeMapper() => this.CreateMap<StockSearchResult, StockMerge>();
}
