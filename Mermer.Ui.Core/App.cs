// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.App
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.ViewModels;
using MvvmCross.Localization;
using MvvmCross.Platform;
using MvvmCross.Platform.IoC;
using Mermer.Ui.Core.ViewModels;
using Mermer.Mvvm.Tools;

#nullable disable
namespace Mermer.Ui.Core;

public class App : MvxApplication
{
  public override void Initialize()
  {
    this.CreatableTypes().EndingWith("Service").AsInterfaces().RegisterAsLazySingleton();
    Mvx.RegisterType<IMvxTextProvider, JsonTextProvider>();
    this.RegisterAppStart<LoginViewModel>();
  }
}
