// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Finance.DailyRegistery.Mappers.DailyFundsRegisteryToInfoMapper
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using AutoMapper;
using Payhas.Binyat.Finance.DailyRegistery.Models;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Finance.DailyRegistery.Mappers;

public class DailyFundsRegisteryToInfoMapper : Profile
{
  public DailyFundsRegisteryToInfoMapper()
  {
    this.CreateMap<DailyFundsRegistery, DailyFundsRegisteryInfo>();
  }
}
