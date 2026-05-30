// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Finance.DailyRegistery.Mappers.DailyFundsRegisteryToInfoMapper
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using AutoMapper;
using Mermer.Finance.DailyRegistery.Models;

#nullable disable
namespace Mermer.Core.Couch.Finance.DailyRegistery.Mappers;

public class DailyFundsRegisteryToInfoMapper : Profile
{
  public DailyFundsRegisteryToInfoMapper()
  {
    this.CreateMap<DailyFundsRegistery, DailyFundsRegisteryInfo>();
  }
}
