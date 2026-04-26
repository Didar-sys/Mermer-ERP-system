// Decompiled with JetBrains decompiler
// Type: Payhas.Licensing.Client.PayhasLicensingClientModule
// Assembly: Payhas.Licensing.Client, Version=0.0.6.0, Culture=neutral, PublicKeyToken=null
// MVID: D27E04BC-87FA-488D-A2D4-54F1C56BAB05
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Licensing.Client.dll

using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;
using Microsoft.Extensions.Options;
using Payhas.Licensing.Client.Models;
using System;
using System.Reflection;

#nullable disable
namespace Payhas.Licensing.Client;

public class PayhasLicensingClientModule : Autofac.Module
{
  private readonly ActivationConfiguration _config;

  public PayhasLicensingClientModule(ActivationConfiguration config) => this._config = config;

  protected override void Load(ContainerBuilder builder)
  {
    base.Load(builder);
    builder.RegisterInstance<IOptions<ActivationConfiguration>>(Microsoft.Extensions.Options.Options.Create<ActivationConfiguration>(this._config));
    Assembly assembly = typeof (PayhasLicensingClientModule).GetTypeInfo().Assembly;
    builder.RegisterAssemblyTypes(assembly).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Func<Type, bool>) (t => t.Name.EndsWith("Service"))).AsImplementedInterfaces<object>().AsSelf<object>().SingleInstance();
  }
}
