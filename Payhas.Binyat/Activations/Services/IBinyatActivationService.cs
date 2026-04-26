// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Activations.Services.IBinyatActivationService
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Activations.Models;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Activations.Services;

public interface IBinyatActivationService
{
  Task ActivateClientAsync(string licenseId, string note);

  Task ActivateServerAsync(string licenseId, string note);

  Task ActivateSynchronizerAsync(string licenseId, string note);

  Task ReactivateClientAsync();

  Task ReactivateServerAsync();

  Task ReactivateSynchronizerAsync();

  Task DeactivateClientAsync();

  Task DeactivateServerAsync();

  Task DeactivateSynchronizerAsync();

  Task<ActivationStatus> GetClientActiveDatesAsync();

  Task<ActivationStatus> GetServerActiveDatesAsync();

  Task<ActivationStatus> GetSynchronizerActiveDatesAsync();

  Task ValidateClientActivationAsync();

  Task ValidateServerActivationAsync();

  Task ValidateSynchronizerActivationAsync();
}
