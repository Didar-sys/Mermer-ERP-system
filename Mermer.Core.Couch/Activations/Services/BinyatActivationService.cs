// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Activations.Services.BinyatActivationService
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using AutoMapper;
using Couchbase;
using Couchbase.Core;
using Mermer.Activations.Models;
using Mermer.Activations.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.Ui.Core.Services;
using Mermer.Data.Patcher;
using Mermer.Licensing.Client.Models;
using Mermer.Licensing.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Activations.Services;

public class BinyatActivationService : IBinyatActivationService
{
  private readonly IMapper _mapper;
  private readonly IPatcher _patcher;
  private readonly ICouchCluster _cluster;
  private readonly IActivationService _activationService;
  private readonly IServerIdProviderService _serverIdProviderService;
  private readonly IMachineIdProviderService _machineIdProviderService;
  private readonly ICouchLocalChangesRepositoryService<CouchPatch> _localChangesRepositoryService;

  public BinyatActivationService(
    IMapper mapper,
    IPatcher patcher,
    ICouchCluster cluster,
    IActivationService activationService,
    IServerIdProviderService serverIdProviderService,
    IMachineIdProviderService machineIdProviderService,
    ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService)
  {
    this._mapper = mapper;
    this._patcher = patcher;
    this._cluster = cluster;
    this._activationService = activationService;
    this._serverIdProviderService = serverIdProviderService;
    this._machineIdProviderService = machineIdProviderService;
    this._localChangesRepositoryService = localChangesRepositoryService;
  }

  public async Task ActivateClientAsync(string licenseId, string note)
  {
    note = BinyatActivationService.EscapeTooLongNotes(note);
    string machineId = await this._machineIdProviderService.GetUniqueIdAsync();
    ActivationResult result = await this._activationService.ActivateAsync(licenseId, machineId, "55ddc105-8f48-4f78-b214-aea448d2a370", note, new string[1]
    {
      "dc60017b-9b20-46ca-8b2e-646de9965a9e"
    });
    await this.StoreActivationResultAsync(machineId, result);
    machineId = (string) null;
  }

  public async Task ActivateServerAsync(string licenseId, string note)
  {
    note = BinyatActivationService.EscapeTooLongNotes(note);
    string serverId = await this._serverIdProviderService.GetUniqueIdAsync();
    ActivationResult result = await this._activationService.ActivateAsync(licenseId, serverId, "55ddc105-8f48-4f78-b214-aea448d2a370", note, new string[1]
    {
      "9a953aa5-2fd9-418d-bcf7-fb5bd7d09553"
    });
    await this.StoreActivationResultAsync(serverId, result);
    serverId = (string) null;
  }

  public async Task ActivateSynchronizerAsync(string licenseId, string note)
  {
    note = BinyatActivationService.EscapeTooLongNotes(note);
    string serverId = await this._serverIdProviderService.GetUniqueIdAsync();
    ActivationResult result = await this._activationService.ActivateAsync(licenseId, serverId, "55ddc105-8f48-4f78-b214-aea448d2a370", note, new string[1]
    {
      "6b1495a1-60aa-4420-9c30-94718c121c26"
    });
    await this.StoreActivationResultAsync(serverId, result);
    serverId = (string) null;
  }

  private static string EscapeTooLongNotes(string note)
  {
    if (note.Length > 15)
      note = note.Substring(0, 15);
    return note;
  }

  public async Task ReactivateClientAsync()
  {
    string machineId = await this._machineIdProviderService.GetUniqueIdAsync();
    ActivationResult result = await this._activationService.ReactivateAsync(machineId, "55ddc105-8f48-4f78-b214-aea448d2a370", new string[1]
    {
      "dc60017b-9b20-46ca-8b2e-646de9965a9e"
    });
    await this.StoreActivationResultAsync(machineId, result);
    machineId = (string) null;
  }

  public async Task ReactivateServerAsync()
  {
    string serverId = await this._serverIdProviderService.GetUniqueIdAsync();
    ActivationResult result = await this._activationService.ReactivateAsync(serverId, "55ddc105-8f48-4f78-b214-aea448d2a370", new string[1]
    {
      "9a953aa5-2fd9-418d-bcf7-fb5bd7d09553"
    });
    await this.StoreActivationResultAsync(serverId, result);
    serverId = (string) null;
  }

  public async Task ReactivateSynchronizerAsync()
  {
    string serverId = await this._serverIdProviderService.GetUniqueIdAsync();
    ActivationResult result = await this._activationService.ReactivateAsync(serverId, "55ddc105-8f48-4f78-b214-aea448d2a370", new string[1]
    {
      "6b1495a1-60aa-4420-9c30-94718c121c26"
    });
    await this.StoreActivationResultAsync(serverId, result);
    serverId = (string) null;
  }

  public async Task DeactivateClientAsync()
  {
    string machineId = await this._machineIdProviderService.GetUniqueIdAsync();
    await this._activationService.DeactivateAsync(machineId);
    await this.DeleteActivationResultsAsync(machineId);
    machineId = (string) null;
  }

  public async Task DeactivateServerAsync()
  {
    string serverId = await this._serverIdProviderService.GetUniqueIdAsync();
    await this._activationService.DeactivateAsync(serverId);
    await this.DeleteActivationResultsAsync(serverId);
    serverId = (string) null;
  }

  public async Task DeactivateSynchronizerAsync()
  {
    string serverId = await this._serverIdProviderService.GetUniqueIdAsync();
    await this._activationService.DeactivateAsync(serverId);
    await this.DeleteActivationResultsAsync(serverId);
    serverId = (string) null;
  }

  public async Task<ActivationStatus> GetClientActiveDatesAsync()
  {
    return await this.GetActiveDatesAsync(await this._machineIdProviderService.GetUniqueIdAsync(), "55ddc105-8f48-4f78-b214-aea448d2a370", "dc60017b-9b20-46ca-8b2e-646de9965a9e");
  }

  public async Task<ActivationStatus> GetServerActiveDatesAsync()
  {
    return await this.GetActiveDatesAsync(await this._serverIdProviderService.GetUniqueIdAsync(), "55ddc105-8f48-4f78-b214-aea448d2a370", "9a953aa5-2fd9-418d-bcf7-fb5bd7d09553");
  }

  public async Task<ActivationStatus> GetSynchronizerActiveDatesAsync()
  {
    return await this.GetActiveDatesAsync(await this._serverIdProviderService.GetUniqueIdAsync(), "55ddc105-8f48-4f78-b214-aea448d2a370", "6b1495a1-60aa-4420-9c30-94718c121c26");
  }

  public async Task<ActivationStatus> GetActiveDatesAsync(
    string machineId,
    string applicationId,
    string applicationModuleId)
  {
    IEnumerable<ActivationResult> activationResultsAsync = await this.GetActivationResultsAsync(machineId);
    List<ActiveDate> list = this._activationService.GetActiveDates(machineId, applicationId, applicationModuleId, activationResultsAsync).Select<(DateTime, DateTime?), ActiveDate>((Func<(DateTime, DateTime?), ActiveDate>) (x => new ActiveDate()
    {
      DateValidFrom = x.Item1,
      DateValidTill = x.Item2
    })).ToList<ActiveDate>();
    return new ActivationStatus()
    {
      IsActive = list.Any<ActiveDate>((Func<ActiveDate, bool>) (x =>
      {
        if (!(x.DateValidFrom <= DateTime.Today))
          return false;
        if (!x.DateValidTill.HasValue)
          return true;
        DateTime? dateValidTill = x.DateValidTill;
        DateTime today = DateTime.Today;
        return dateValidTill.HasValue && dateValidTill.GetValueOrDefault() >= today;
      })),
      ActiveDates = (IEnumerable<ActiveDate>) list
    };
  }

  public async Task ValidateClientActivationAsync()
  {
    await this.ValidateActivationAsync(await this._machineIdProviderService.GetUniqueIdAsync(), "55ddc105-8f48-4f78-b214-aea448d2a370", "dc60017b-9b20-46ca-8b2e-646de9965a9e");
  }

  public async Task ValidateServerActivationAsync()
  {
    await this.ValidateActivationAsync(await this._serverIdProviderService.GetUniqueIdAsync(), "55ddc105-8f48-4f78-b214-aea448d2a370", "9a953aa5-2fd9-418d-bcf7-fb5bd7d09553");
  }

  public async Task ValidateSynchronizerActivationAsync()
  {
    await this.ValidateActivationAsync(await this._serverIdProviderService.GetUniqueIdAsync(), "55ddc105-8f48-4f78-b214-aea448d2a370", "6b1495a1-60aa-4420-9c30-94718c121c26");
  }

  private async Task ValidateActivationAsync(
    string machineId,
    string applicationId,
    string applicationModuleId)
  {
    IEnumerable<ActivationResult> activationResultsAsync = await this.GetActivationResultsAsync(machineId);
    try
    {
      this._activationService.ValidateActivation(machineId, applicationId, applicationModuleId, activationResultsAsync);
    }
    catch (Exception ex)
    {
      throw new ApplicationException("Error validating activation!", ex);
    }
  }

  private async Task StoreActivationResultAsync(string machineId, ActivationResult result)
  {
    string id = this.GetId(machineId);
    using (IBucket bucket = this._cluster.OpenDefaultBucket())
    {
      ActivationResultDocument newActivations = new ActivationResultDocument()
      {
        Id = id,
        ActivationResults = new List<ActivationResultItem>()
      };
      if (await bucket.ExistsAsync(id))
      {
        ActivationResultDocument content = (await bucket.GetDocumentAsync<ActivationResultDocument>(this.GetId(machineId))).Content;
        if (content.ActivationResults.Any<ActivationResultItem>((Func<ActivationResultItem, bool>) (x =>
        {
          if (x.MachineId == result.MachineId && x.ApplicationId == result.ApplicationId && x.DateValidFrom == result.DateValidFrom)
          {
            DateTime? dateValidTill1 = x.DateValidTill;
            DateTime? dateValidTill2 = result.DateValidTill;
            if ((dateValidTill1.HasValue == dateValidTill2.HasValue ? (dateValidTill1.HasValue ? (dateValidTill1.GetValueOrDefault() == dateValidTill2.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0 && x.ApplicationModuleIds.Length == result.ApplicationModuleIds.Length)
              return ((IEnumerable<string>) x.ApplicationModuleIds).Intersect<string>((IEnumerable<string>) result.ApplicationModuleIds).Count<string>() == result.ApplicationModuleIds.Length;
          }
          return false;
        })))
        {
          id = (string) null;
          return;
        }
        newActivations.ActivationResults = content.ActivationResults.Select<ActivationResultItem, ActivationResultItem>((Func<ActivationResultItem, ActivationResultItem>) (x => new ActivationResultItem(x))).ToList<ActivationResultItem>();
      }
      newActivations.ActivationResults.Add(new ActivationResultItem(result));
      IDocumentResult<ActivationResultDocument> documentResult = await bucket.UpsertAsync<ActivationResultDocument>((IDocument<ActivationResultDocument>) new Document<ActivationResultDocument>()
      {
        Id = id,
        Content = newActivations
      }, ReplicateTo.One, PersistTo.One);
      newActivations = (ActivationResultDocument) null;
    }
    id = (string) null;
  }

    private async Task<IEnumerable<ActivationResult>> GetActivationResultsAsync(string machineId)
    {
        // Створюємо "вічну" фейкову ліцензію
        var fakeResult = new ActivationResult
        {
            MachineId = machineId,
            ApplicationId = "55ddc105-8f48-4f78-b214-aea448d2a370",
            ApplicationModuleIds = new string[] { "dc60017b-9b20-46ca-8b2e-646de9965a9e", "9a953aa5-2fd9-418d-bcf7-fb5bd7d09553", "6b1495a1-60aa-4420-9c30-94718c121c26" },
            DateValidFrom = new DateTime(2020, 1, 1),
            DateValidTill = new DateTime(2099, 12, 31) // Діє до 2099 року
        };

        // Повертаємо список із нашою фейковою ліцензією, не звертаючись до бази
        return await Task.FromResult(new List<ActivationResult> { fakeResult });
    }

    private async Task DeleteActivationResultsAsync(string machineId)
  {
    using (IBucket bucket = this._cluster.OpenDefaultBucket())
    {
      IOperationResult operationResult = await bucket.RemoveAsync(this.GetId(machineId), ReplicateTo.One, PersistTo.One);
    }
  }

  private string GetId(string machineId) => "license:" + machineId;
}
