// Decompiled with JetBrains decompiler
// Type: Mermer.Licensing.Client.Services.IActivationService
// Assembly: Mermer.Licensing.Client, Version=0.0.6.0, Culture=neutral, PublicKeyToken=null
// MVID: D27E04BC-87FA-488D-A2D4-54F1C56BAB05
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Licensing.Client.dll

using Mermer.Licensing.Client.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Licensing.Client.Services;

public interface IActivationService
{
  void ValidateActivation(
    string machineId,
    string applicationId,
    string applicationModuleId,
    IEnumerable<ActivationResult> activations);

  bool IsActivated(
    string machineId,
    string applicationId,
    string applicationModuleId,
    IEnumerable<ActivationResult> activations);

  IEnumerable<(DateTime DateValidFrom, DateTime? DateValidTill)> GetActiveDates(
    string machineId,
    string applicationId,
    string applicationModuleId,
    IEnumerable<ActivationResult> activations);

  Task<ActivationResult> ActivateAsync(
    string licenseId,
    string machineId,
    string applicationId,
    string note,
    string[] applicationModuleIds);

  Task<ActivationResult> ActivateTrialAsync(
    string machineId,
    string applicationId,
    string[] applicationModuleIds);

  Task<ActivationResult> ReactivateAsync(
    string machineId,
    string applicationId,
    string[] applicationModuleIds);

  Task DeactivateAsync(string machineId);
}
