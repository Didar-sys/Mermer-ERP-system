// Decompiled with JetBrains decompiler
// Type: Payhas.MeansImplicitUseAttribute
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using System;

#nullable disable
namespace Payhas;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.GenericParameter)]
public sealed class MeansImplicitUseAttribute : Attribute
{
  public MeansImplicitUseAttribute()
    : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default)
  {
  }

  public MeansImplicitUseAttribute(ImplicitUseKindFlags useKindFlags)
    : this(useKindFlags, ImplicitUseTargetFlags.Default)
  {
  }

  public MeansImplicitUseAttribute(ImplicitUseTargetFlags targetFlags)
    : this(ImplicitUseKindFlags.Default, targetFlags)
  {
  }

  public MeansImplicitUseAttribute(
    ImplicitUseKindFlags useKindFlags,
    ImplicitUseTargetFlags targetFlags)
  {
    this.UseKindFlags = useKindFlags;
    this.TargetFlags = targetFlags;
  }

  [UsedImplicitly]
  public ImplicitUseKindFlags UseKindFlags { get; private set; }

  [UsedImplicitly]
  public ImplicitUseTargetFlags TargetFlags { get; private set; }
}
