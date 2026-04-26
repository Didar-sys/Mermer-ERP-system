// Decompiled with JetBrains decompiler
// Type: Payhas.MustUseReturnValueAttribute
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using System;

#nullable disable
namespace Payhas;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MustUseReturnValueAttribute : Attribute
{
  public MustUseReturnValueAttribute()
  {
  }

  public MustUseReturnValueAttribute([NotNull] string justification)
  {
    this.Justification = justification;
  }

  [CanBeNull]
  public string Justification { get; private set; }
}
