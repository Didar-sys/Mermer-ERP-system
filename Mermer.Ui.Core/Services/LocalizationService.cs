// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.Services.LocalizationService
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Localization;
using Mermer.Common.Services;

#nullable disable
namespace Mermer.Ui.Core.Services;

public class LocalizationService : ILocalizationService
{
  private readonly IMvxLanguageBinder _language;

  public LocalizationService()
    : this(nameof (LocalizationService))
  {
  }

  public LocalizationService(string owningObjectType)
  {
    this._language = (IMvxLanguageBinder) new MvxLanguageBinder(owningObjectType);
  }

  public string GetText(string entryKey) => this._language.GetText(entryKey);

  public string GetText(string entryKey, params object[] args)
  {
    return this._language.GetText(entryKey, args);
  }
}
