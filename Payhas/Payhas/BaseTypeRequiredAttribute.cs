// Decompiled with JetBrains decompiler
// Type: Payhas.BaseTypeRequiredAttribute
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using System;

#nullable disable
namespace Payhas;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[BaseTypeRequired(typeof (Attribute))]
public sealed class BaseTypeRequiredAttribute : Attribute
{
  public BaseTypeRequiredAttribute([NotNull] Type baseType) => this.BaseType = baseType;

  [NotNull]
  public Type BaseType { get; private set; }
}
