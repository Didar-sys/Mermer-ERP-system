// Decompiled with JetBrains decompiler
// Type: Mermer.Common.Settings.UpdateSettings
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Data.Models;

#nullable disable
namespace Mermer.Common.Settings;

public class UpdateSettings : BindableObject
{
  private bool _checkForUpdates = true;

  public virtual bool CheckForUpdates
  {
    get => this._checkForUpdates;
    set => this.SetProperty<bool>(ref this._checkForUpdates, value, nameof (CheckForUpdates));
  }
}
