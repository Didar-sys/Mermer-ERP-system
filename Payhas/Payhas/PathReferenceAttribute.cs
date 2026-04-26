// Decompiled with JetBrains decompiler
// Type: Payhas.PathReferenceAttribute
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using System;

#nullable disable
namespace Payhas;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class PathReferenceAttribute : Attribute
{
  public PathReferenceAttribute()
  {
  }

  public PathReferenceAttribute([NotNull, PathReference] string basePath)
  {
    this.BasePath = basePath;
  }

  [CanBeNull]
  public string BasePath { get; private set; }
}
