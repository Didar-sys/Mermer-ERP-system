// Decompiled with JetBrains decompiler
// Type: Mermer.Common.Settings.AppSettings
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Data.Models;

#nullable disable
namespace Mermer.Common.Settings;

public class AppSettings : BindableObject
{
  private string _culture;
  private bool _openPosOnLoad;
  private bool _autoHideMenu;
  private string _defaultCurrencyId;
  private string _defaultOfficeId;
  private string _defaultWarehouseId;
  private string _defaultDepositoryId;
  private string _defaultStockPriceGroup;
  private int _defaultDueDateInDays;
  private int _localCodePrefix;
  private int _lastStockCodeValue;
  private int _lastTransactionCodeValue;
  private int _lastPartnerCodeValue;
  private bool _freezeStockBalanceOnRevision;
  private bool _allowStockPriceChangeOnRevision;
  private bool _showLastPurchasePriceOnSearch;
  private bool _openEditorWhenAdding;

  public AppSettings()
  {
    this.ShowLastPurchasePriceOnSearch = true;
    this.OpenEditorWhenAdding = true;
  }

  public virtual string Culture
  {
    get => this._culture;
    set => this.SetProperty<string>(ref this._culture, value, nameof (Culture));
  }

  public bool OpenPosOnLoad
  {
    get => this._openPosOnLoad;
    set => this.SetProperty<bool>(ref this._openPosOnLoad, value, nameof (OpenPosOnLoad));
  }

  public virtual bool AutoHideMenu
  {
    get => this._autoHideMenu;
    set => this.SetProperty<bool>(ref this._autoHideMenu, value, nameof (AutoHideMenu));
  }

  public virtual string DefaultCurrencyId
  {
    get => this._defaultCurrencyId;
    set => this.SetProperty<string>(ref this._defaultCurrencyId, value, nameof (DefaultCurrencyId));
  }

  public virtual string DefaultOfficeId
  {
    get => this._defaultOfficeId;
    set => this.SetProperty<string>(ref this._defaultOfficeId, value, nameof (DefaultOfficeId));
  }

  public virtual string DefaultWarehouseId
  {
    get => this._defaultWarehouseId;
    set
    {
      this.SetProperty<string>(ref this._defaultWarehouseId, value, nameof (DefaultWarehouseId));
    }
  }

  public virtual string DefaultDepositoryId
  {
    get => this._defaultDepositoryId;
    set
    {
      this.SetProperty<string>(ref this._defaultDepositoryId, value, nameof (DefaultDepositoryId));
    }
  }

  public string DefaultStockPriceGroup
  {
    get => this._defaultStockPriceGroup;
    set
    {
      this.SetProperty<string>(ref this._defaultStockPriceGroup, value, nameof (DefaultStockPriceGroup));
    }
  }

  public virtual int DefaultDueDateInDays
  {
    get => this._defaultDueDateInDays;
    set
    {
      this.SetProperty<int>(ref this._defaultDueDateInDays, value, nameof (DefaultDueDateInDays));
    }
  }

  public virtual int LocalCodePrefix
  {
    get => this._localCodePrefix;
    set => this.SetProperty<int>(ref this._localCodePrefix, value, nameof (LocalCodePrefix));
  }

  public virtual int LastStockCodeValue
  {
    get => this._lastStockCodeValue;
    set => this.SetProperty<int>(ref this._lastStockCodeValue, value, nameof (LastStockCodeValue));
  }

  public virtual int LastTransactionCodeValue
  {
    get => this._lastTransactionCodeValue;
    set
    {
      this.SetProperty<int>(ref this._lastTransactionCodeValue, value, nameof (LastTransactionCodeValue));
    }
  }

  public virtual int LastPartnerCodeValue
  {
    get => this._lastPartnerCodeValue;
    set
    {
      this.SetProperty<int>(ref this._lastPartnerCodeValue, value, nameof (LastPartnerCodeValue));
    }
  }

  public bool FreezeStockBlanaceOnRevision
  {
    get => this._freezeStockBalanceOnRevision;
    set
    {
      this.SetProperty<bool>(ref this._freezeStockBalanceOnRevision, value, nameof (FreezeStockBlanaceOnRevision));
    }
  }

  public bool AllowStockPriceChangeOnRevision
  {
    get => this._allowStockPriceChangeOnRevision;
    set
    {
      this.SetProperty<bool>(ref this._allowStockPriceChangeOnRevision, value, nameof (AllowStockPriceChangeOnRevision));
    }
  }

  public bool ShowLastPurchasePriceOnSearch
  {
    get => this._showLastPurchasePriceOnSearch;
    set
    {
      this.SetProperty<bool>(ref this._showLastPurchasePriceOnSearch, value, nameof (ShowLastPurchasePriceOnSearch));
    }
  }

  public bool OpenEditorWhenAdding
  {
    get => this._openEditorWhenAdding;
    set
    {
      this.SetProperty<bool>(ref this._openEditorWhenAdding, value, nameof (OpenEditorWhenAdding));
    }
  }
}
