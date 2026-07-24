using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.Services;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mermer.Ui.Core.ViewModels.Common;

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
    private Property[] _properties;

    public DataImportViewModel(
        IMvxMessenger messenger,
        IExcelReaderService excelReaderService,
        IMvxNavigationService navigationService,
        IUserInteractionService userInteractionService)
        : base(messenger, navigationService, userInteractionService)
    {
        _excelReaderService = excelReaderService;
    }

    public new string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string FileName
    {
        get => _fileName;
        set
        {
            if (SetProperty(ref _fileName, value) && !string.IsNullOrEmpty(_fileName))
            {
                LoadFileAsync();
            }
        }
    }

    public bool IsFileLoaded
    {
        get => _isFileLoaded;
        set => SetProperty(ref _isFileLoaded, value);
    }

    public IEnumerable<ListHelper<int>> Columns
    {
        get => _columns;
        set => SetProperty(ref _columns, value);
    }

    public Property[] Properties
    {
        get => _properties;
        set => SetProperty(ref _properties, value);
    }

    public void Prepare(Type parameter)
    {
        _itemType = parameter;
        Properties = _itemType.GetTypeInfo().DeclaredProperties.Select(x => new Property
        {
            Info = x,
            DisplayName = this[x.Name]
        }).ToArray();
    }

    public override Task<bool> OnCloseAsync() => Close();

    public virtual Task<bool> Close(IEnumerable<object> list = null)
    {
        return NavigationService.Close(this, list);
    }

    protected virtual async void LoadFileAsync()
    {
        IsBusy = true;
        Status = this["Loading file..."];

        try
        {
            await Task.Run(() =>
            {
                // 1. Открываем файл
                _excelReaderService.OpenExcelFile(_fileName);

                // 2. Генерируем список колонок, начиная с 0-го индекса
                var columnsList = new List<ListHelper<int>>();
                for (int i = 0; i < 50; i++) // Изменено: начинаем с 0, чтобы поймать Pair
                {
                    string headerText = $"Column {i + 1}";
                    try
                    {
                        // Ряд 0 — это наша шапка (Pair, Date и т.д.)
                        object headerObj = _excelReaderService.GetValue(0, i);

                        if (headerObj != null && !string.IsNullOrWhiteSpace(headerObj.ToString()))
                        {
                            headerText = headerObj.ToString();
                        }
                    }
                    catch { }

                    columnsList.Add(new ListHelper<int> { Value = i, Text = headerText });
                }
                Columns = columnsList;
            });

            IsFileLoaded = true;
            Status = this["File Loaded"];
        }
        catch (Exception ex)
        {
            try { _excelReaderService.CloseExcelFile(); } catch { }
            UserInteractionService.ShowExceptionMessage(ex);
            Status = this["Error loading file"];
        }
        finally
        {
            IsBusy = false;
        }
    }

    public ICommand ImportCommand => new MvxAsyncCommand(OnImportCommandAsync, () => !IsBusy);

    protected virtual async Task OnImportCommandAsync()
    {
        IsBusy = true;
        Status = this["Reading data..."];

        try
        {
            var configuredProperties = Properties.Where(x => x.ColumnIndex.HasValue).ToList();
            if (!configuredProperties.Any())
                throw new Exception(this["At least one property must be configured to import!"]);

            var list = new List<object>();
            int row = 1;
            bool hasDataInRow;

            do
            {
                hasDataInRow = false;
                object instance = Activator.CreateInstance(_itemType);

                foreach (var property in configuredProperties)
                {
                    object cellValue = _excelReaderService.GetValue(row, property.ColumnIndex.Value);

                    if (cellValue != null && !string.IsNullOrWhiteSpace(cellValue.ToString()))
                    {
                        // Вытаскиваем базовый тип, если это Nullable (например, decimal?)
                        Type targetType = Nullable.GetUnderlyingType(property.Info.PropertyType) ?? property.Info.PropertyType;

                        object convertedValue;

                        
                        if (targetType == typeof(decimal) || targetType == typeof(double) || targetType == typeof(float))
                        {
                           
                            string cleanNumStr = cellValue.ToString().Trim().Replace(",", ".");

                            // 3. Конвертируем, указывая InvariantCulture (международный стандарт, понимающий точку)
                            convertedValue = Convert.ChangeType(cleanNumStr, targetType, System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            
                            convertedValue = Convert.ChangeType(cellValue, targetType);
                        }
                        property.Info.SetValue(instance, convertedValue);
                        hasDataInRow = true;
                    }
                }

                if (hasDataInRow)
                {
                    list.Add(instance);
                    row++;
                }

                Status = this["{0} items read", row - 1];
            }
            while (hasDataInRow);

            await Close(list);
        }
        catch (Exception ex)
        {
            UserInteractionService.ShowExceptionMessage(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public class Property
    {
        public PropertyInfo Info { get; set; }
        public string DisplayName { get; set; }
        public int? ColumnIndex { get; set; }
    }
}