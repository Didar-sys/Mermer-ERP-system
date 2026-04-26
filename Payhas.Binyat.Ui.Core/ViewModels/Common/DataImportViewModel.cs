// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Common.DataImportViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.Services;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Common;

public class DataImportViewModel : 
  DialogViewModel,
  IMvxViewModel<Type, IEnumerable<object>>,
  IMvxViewModel<Type>,
  IMvxViewModel,
  IMvxViewModelResult<IEnumerable<object>>
{
  private Type _itemType;
  private readonly IExcelReaderService _excelReaderService;
  private string _status;
  private string _fileName;
  private bool _isFileLoaded;
  private IEnumerable<ListHelper<int>> _columns;
  private DataImportViewModel.Property[] _properties;

  public DataImportViewModel(
    IMvxMessenger messenger,
    IExcelReaderService excelReaderService,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._excelReaderService = excelReaderService;
  }

  public new string Status
  {
    get => this._status;
    set => this.SetProperty<string>(ref this._status, value, nameof (Status));
  }

  public string FileName
  {
    get => this._fileName;
    set
    {
      this.SetProperty<string>(ref this._fileName, value, nameof (FileName));
      if (string.IsNullOrEmpty(this._fileName))
        return;
      this.LoadFileAsync();
    }
  }

  public bool IsFileLoaded
  {
    get => this._isFileLoaded;
    set => this.SetProperty<bool>(ref this._isFileLoaded, value, nameof (IsFileLoaded));
  }

  public IEnumerable<ListHelper<int>> Columns
  {
    get => this._columns;
    set
    {
      this.SetProperty<IEnumerable<ListHelper<int>>>(ref this._columns, value, nameof (Columns));
    }
  }

  public DataImportViewModel.Property[] Properties
  {
    get => this._properties;
    set
    {
      this.SetProperty<DataImportViewModel.Property[]>(ref this._properties, value, nameof (Properties));
    }
  }

  public void Prepare(Type parameter)
  {
    this._itemType = parameter;
    this.Properties = this._itemType.GetTypeInfo().DeclaredProperties.Select<PropertyInfo, DataImportViewModel.Property>((Func<PropertyInfo, DataImportViewModel.Property>) (x => new DataImportViewModel.Property()
    {
      Info = x,
      DisplayName = this[x.Name, Array.Empty<object>()]
    })).ToArray<DataImportViewModel.Property>();
  }

  public override Task<bool> OnCloseAsync() => this.Close();

  public virtual Task<bool> Close(IEnumerable<object> list = null)
  {
    return this.NavigationService.Close<IEnumerable<object>>((IMvxViewModelResult<IEnumerable<object>>) this, list);
  }

  protected virtual async void LoadFileAsync()
  {
    DataImportViewModel dataImportViewModel = this;
    dataImportViewModel.IsBusy = true;
    dataImportViewModel.Status = dataImportViewModel["Loading file...", Array.Empty<object>()];
    // ISSUE: reference to a compiler-generated method
    await Task.Run(new Action(dataImportViewModel.\u003CLoadFileAsync\u003Eb__27_0));
    dataImportViewModel.Status = dataImportViewModel["File Loaded", Array.Empty<object>()];
    dataImportViewModel.IsBusy = false;
  }

  public ICommand ImportCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnImportCommandAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual async Task OnImportCommandAsync()
  {
    DataImportViewModel dataImportViewModel = this;
    dataImportViewModel.IsBusy = true;
    dataImportViewModel.Status = dataImportViewModel["Reading data...", Array.Empty<object>()];
    try
    {
      IEnumerable<DataImportViewModel.Property> source = ((IEnumerable<DataImportViewModel.Property>) dataImportViewModel.Properties).Where<DataImportViewModel.Property>((Func<DataImportViewModel.Property, bool>) (x => x.ColumnIndex.HasValue));
      if (!source.Any<DataImportViewModel.Property>())
        throw new Exception(dataImportViewModel["At least one property must be configured to import!", Array.Empty<object>()]);
      List<object> list = new List<object>();
      int row = 1;
      bool flag;
      do
      {
        flag = false;
        object instance = Activator.CreateInstance(dataImportViewModel._itemType);
        foreach (DataImportViewModel.Property property in source)
        {
          object obj1 = dataImportViewModel._excelReaderService.GetValue(row, property.ColumnIndex.Value);
          if (obj1 != null)
          {
            object obj2 = Convert.ChangeType(obj1, property.Info.PropertyType);
            property.Info.SetValue(instance, obj2);
            flag = true;
          }
        }
        if (flag)
        {
          list.Add(instance);
          ++row;
        }
        dataImportViewModel.Status = dataImportViewModel["{0} items read", new object[1]
        {
          (object) row
        }];
      }
      while (flag);
      int num = await dataImportViewModel.Close((IEnumerable<object>) list) ? 1 : 0;
    }
    catch (Exception ex)
    {
      dataImportViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    dataImportViewModel.IsBusy = false;
  }

  public class Property
  {
    public PropertyInfo Info { get; set; }

    public string DisplayName { get; set; }

    public int? ColumnIndex { get; set; }
  }
}
