// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Common.CopyCreate
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Payhas.Binyat.Ui.Core.ViewModels.Commerce;
using Payhas.Binyat.Ui.Core.ViewModels.Warehousing;
using Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Ordering;
using Payhas.Data.Models;
using Payhas.Mvvm.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Common;

public class CopyCreate : BindableObject
{
  private readonly IMvxNavigationService _navigationService;
  private readonly IUserInteractionService _userInteractionService;
  private Func<IEnumerable<CopyCreateLine>> _getLines;

  public CopyCreate(
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
  {
    this._navigationService = navigationService;
    this._userInteractionService = userInteractionService;
  }

  public virtual Func<IEnumerable<CopyCreateLine>> GetLines
  {
    get => this._getLines;
    set
    {
      this.SetProperty<Func<IEnumerable<CopyCreateLine>>>(ref this._getLines, value, nameof (GetLines));
    }
  }

  public ICommand ToNew
  {
    get
    {
      return (ICommand) new MvxAsyncCommand<CopyCreateType>(new Func<CopyCreateType, Task>(this.OnToNewAsync), (Func<CopyCreateType, bool>) (x => true));
    }
  }

  private Task OnToNewAsync(CopyCreateType type)
  {
    try
    {
      IEnumerable<CopyCreateLine> copyCreateLines = this.GetLines();
      switch (type)
      {
        case CopyCreateType.Invoice:
          return this._navigationService.Navigate<InvoiceDetailsViewModel, IEnumerable<CopyCreateLine>>(copyCreateLines);
        case CopyCreateType.StockSlip:
          return this._navigationService.Navigate<StockSlipDetailsViewModel, IEnumerable<CopyCreateLine>>(copyCreateLines);
        case CopyCreateType.StockTransfer:
          return this._navigationService.Navigate<StockTransferDetailsViewModel, IEnumerable<CopyCreateLine>>(copyCreateLines);
        case CopyCreateType.StockOrder:
          return this._navigationService.Navigate<StockOrderDetailsViewModel, IEnumerable<CopyCreateLine>>(copyCreateLines);
        case CopyCreateType.StockOrderTemplate:
          return this._navigationService.Navigate<StockOrderTemplateDetailsViewModel, IEnumerable<CopyCreateLine>>(copyCreateLines);
        default:
          throw new ArgumentOutOfRangeException(nameof (type), (object) type, (string) null);
      }
    }
    catch (Exception ex)
    {
      this._userInteractionService.ShowExceptionMessage(ex);
      return Task.CompletedTask;
    }
  }
}
