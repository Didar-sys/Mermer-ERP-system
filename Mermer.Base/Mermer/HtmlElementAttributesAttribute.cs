// Decompiled with JetBrains decompiler
// Type: Mermer.HtmlElementAttributesAttribute
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class HtmlElementAttributesAttribute : Attribute
{
  public HtmlElementAttributesAttribute()
  {
  }

  public HtmlElementAttributesAttribute([NotNull] string name) => this.Name = name;

  [CanBeNull]
  public string Name { get; private set; }
}
