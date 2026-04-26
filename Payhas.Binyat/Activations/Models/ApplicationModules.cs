// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Activations.Models.ApplicationModules
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;

#nullable disable
namespace Payhas.Binyat.Activations.Models;

public static class ApplicationModules
{
  public const string Client = "dc60017b-9b20-46ca-8b2e-646de9965a9e";
  public const string Server = "9a953aa5-2fd9-418d-bcf7-fb5bd7d09553";
  public const string Synchronizer = "6b1495a1-60aa-4420-9c30-94718c121c26";

  public static string ToModuleName(string moduleId)
  {
    switch (moduleId)
    {
      case "dc60017b-9b20-46ca-8b2e-646de9965a9e":
        return "Client";
      case "9a953aa5-2fd9-418d-bcf7-fb5bd7d09553":
        return "Server";
      case "6b1495a1-60aa-4420-9c30-94718c121c26":
        return "Synchronizer";
      default:
        throw new Exception("Wrong module Id");
    }
  }
}
