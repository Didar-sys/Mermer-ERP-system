// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.StockManagement.StockBarcodesPrinterViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Ui.Core.Services;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.StockManagement;

public class StockBarcodesPrinterViewModel : 
  DialogViewModel,
  IMvxViewModel<StockBarcodesPrinterParams>,
  IMvxViewModel
{
  private readonly IPrintingService _printingService;
  private StockBarcodesPrinterParams _parameter;
  private string _name;
  private string _price;
  private ObservableCollection<StockBarcodesPrinterLine> _lines;

  public StockBarcodesPrinterViewModel(
    IMvxMessenger messenger,
    IPrintingService printingService,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._printingService = printingService;
  }

  public virtual string Name
  {
    get => this._name;
    set => this.SetProperty<string>(ref this._name, value, nameof (Name));
  }

  public virtual string Price
  {
    get => this._price;
    set => this.SetProperty<string>(ref this._price, value, nameof (Price));
  }

  public virtual ObservableCollection<StockBarcodesPrinterLine> Lines
  {
    get => this._lines;
    set
    {
      this.SetProperty<ObservableCollection<StockBarcodesPrinterLine>>(ref this._lines, value, nameof (Lines));
    }
  }

  public void Prepare(StockBarcodesPrinterParams parameter) => this._parameter = parameter;

  protected override Task OnLoad()
  {
    this.Name = this._parameter.Name;
    this.Price = this._parameter.Price;
    this.Lines = new ObservableCollection<StockBarcodesPrinterLine>(this._parameter.Barcodes.Select<string, StockBarcodesPrinterLine>((Func<string, StockBarcodesPrinterLine>) (x => new StockBarcodesPrinterLine()
    {
      Barcode = x,
      CopiesCount = 1
    })));
    return base.OnLoad();
  }

  public ICommand PrintCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnPrintAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  public virtual async Task OnPrintAsync()
  {
    StockBarcodesPrinterViewModel printerViewModel = this;
    try
    {
      foreach (StockBarcodesPrinterLine barcodesPrinterLine in printerViewModel.Lines.Where<StockBarcodesPrinterLine>((Func<StockBarcodesPrinterLine, bool>) (x => x.CopiesCount > 0)))
        await printerViewModel._printingService.PrintBarcodes(printerViewModel.Name, barcodesPrinterLine.Barcode, printerViewModel.Price, barcodesPrinterLine.CopiesCount);
    }
    catch (Exception ex)
    {
      printerViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
  }
}
