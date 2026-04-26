// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.App
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.ViewModels;
using MvvmCross.Localization;
using MvvmCross.Platform;
using MvvmCross.Platform.IoC;
using Payhas.Binyat.Ui.Core.ViewModels;
using Payhas.Mvvm.Tools;

#nullable disable
namespace Payhas.Binyat.Ui.Core;

public class App : MvxApplication
{
  public override void Initialize()
  {
    this.CreatableTypes().EndingWith("Service").AsInterfaces().RegisterAsLazySingleton();
    Mvx.RegisterType<IMvxTextProvider, JsonTextProvider>();
    this.RegisterAppStart<LoginViewModel>();
  }
}
