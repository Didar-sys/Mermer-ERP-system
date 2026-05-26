// Decompiled with JetBrains decompiler
// Type: Mermer.StringFormatMethodAttribute
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Delegate)]
public sealed class StringFormatMethodAttribute : Attribute
{
  public StringFormatMethodAttribute([NotNull] string formatParameterName)
  {
    this.FormatParameterName = formatParameterName;
  }

  [NotNull]
  public string FormatParameterName { get; private set; }
}
