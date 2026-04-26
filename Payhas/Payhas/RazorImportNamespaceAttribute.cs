// Decompiled with JetBrains decompiler
// Type: Payhas.RazorImportNamespaceAttribute
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using System;

#nullable disable
namespace Payhas;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class RazorImportNamespaceAttribute : Attribute
{
  public RazorImportNamespaceAttribute([NotNull] string name) => this.Name = name;

  [NotNull]
  public string Name { get; private set; }
}
