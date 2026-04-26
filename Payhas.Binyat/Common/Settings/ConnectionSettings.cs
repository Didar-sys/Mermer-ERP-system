// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Common.Settings.ConnectionSettings
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Data.Models;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Common.Settings;

public class ConnectionSettings : BindableObject
{
  private int _mode;
  private string _serviceAddress;
  private string _databaseAddress;
  private string _databaseName;
  private string _databaseUser;
  private string _databasePassword;
  private bool _allowReporting = true;

  public virtual int Mode
  {
    get => this._mode;
    set
    {
      if (!this.SetProperty<int>(ref this._mode, value, nameof (Mode)))
        return;
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsDirectModeSelected));
    }
  }

  public bool IsDirectModeSelected => this.Mode == 0;

  public virtual string ServiceAddress
  {
    get => this._serviceAddress ?? "http://localhost:8088";
    set => this.SetProperty<string>(ref this._serviceAddress, value, nameof (ServiceAddress));
  }

  public virtual string DatabaseAddress
  {
    get => this._databaseAddress ?? "http://localhost:8091";
    set => this.SetProperty<string>(ref this._databaseAddress, value, nameof (DatabaseAddress));
  }

  public virtual string DatabaseName
  {
    get => this._databaseName ?? "binyat";
    set => this.SetProperty<string>(ref this._databaseName, value, nameof (DatabaseName));
  }

  public virtual string DatabaseUser
  {
    get => this._databaseUser;
    set => this.SetProperty<string>(ref this._databaseUser, value, nameof (DatabaseUser));
  }

  public virtual string DatabasePassword
  {
    get => this._databasePassword;
    set => this.SetProperty<string>(ref this._databasePassword, value, nameof (DatabasePassword));
  }

  public virtual bool AllowReporting
  {
    get => this._allowReporting;
    set => this.SetProperty<bool>(ref this._allowReporting, value, nameof (AllowReporting));
  }
}
