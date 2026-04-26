// Decompiled with JetBrains decompiler
// Type: Payhas.AspChildControlTypeAttribute
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using System;

#nullable disable
namespace Payhas;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AspChildControlTypeAttribute : Attribute
{
  public AspChildControlTypeAttribute([NotNull] string tagName, [NotNull] Type controlType)
  {
    this.TagName = tagName;
    this.ControlType = controlType;
  }

  [NotNull]
  public string TagName { get; private set; }

  [NotNull]
  public Type ControlType { get; private set; }
}
