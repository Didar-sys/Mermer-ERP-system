// Decompiled with JetBrains decompiler
// Type: Mermer.ContractAnnotationAttribute
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ContractAnnotationAttribute : Attribute
{
  public ContractAnnotationAttribute([NotNull] string contract)
    : this(contract, false)
  {
  }

  public ContractAnnotationAttribute([NotNull] string contract, bool forceFullStates)
  {
    this.Contract = contract;
    this.ForceFullStates = forceFullStates;
  }

  [NotNull]
  public string Contract { get; private set; }

  public bool ForceFullStates { get; private set; }
}
