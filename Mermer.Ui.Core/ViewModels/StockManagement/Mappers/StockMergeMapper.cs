// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.StockManagement.Mappers.StockMergeMapper
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using AutoMapper;
using Mermer.StockManagement.Services;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.StockManagement.Mappers;

public class StockMergeMapper : Profile
{
  public StockMergeMapper() => this.CreateMap<StockSearchResult, StockMerge>();
}
