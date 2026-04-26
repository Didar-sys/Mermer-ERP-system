// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Transactions.Models.ITransactionModel
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Data.Models;
using System;

#nullable disable
namespace Payhas.Binyat.Transactions.Models;

public interface ITransactionModel : IModel
{
  DateTime Date { get; set; }

  string Code { get; set; }

  string Type { get; }

  string UserId { get; set; }

  string UserName { get; set; }

  bool IsCompleted { get; set; }

  bool IsDisabled { get; set; }
}
