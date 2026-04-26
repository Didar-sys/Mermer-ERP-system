// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Common.DataImportViewModel`1
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Ui.Core.Services;
using Payhas.Mvvm.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Common;

public class DataImportViewModel<T> : 
  DataImportViewModel,
  IMvxViewModelResult<IEnumerable<T>>,
  IMvxViewModel
{
  protected DataImportViewModel(
    IMvxMessenger messenger,
    IExcelReaderService excelReaderService,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, excelReaderService, navigationService, userInteractionService)
  {
  }

  public override Task<bool> Close(IEnumerable<object> list = null)
  {
    return this.NavigationService.Close<IEnumerable<T>>((IMvxViewModelResult<IEnumerable<T>>) this, list != null ? list.Cast<T>() : (IEnumerable<T>) null);
  }
}
