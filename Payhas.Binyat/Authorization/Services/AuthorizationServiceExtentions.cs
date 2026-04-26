// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Authorization.Services.AuthorizationServiceExtentions
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Payhas.Binyat.Authorization.Services;

public static class AuthorizationServiceExtentions
{
  public static void AuthorizeAction(
    this IAuthorizationService service,
    Enum action,
    Enum accessLevel)
  {
    service.AuthorizeAction(action.ToString(), Convert.ToInt32((object) accessLevel));
  }

  public static IEnumerable<string> FilterAvailableActions(
    this IAuthorizationService service,
    Enum level,
    params Enum[] actions)
  {
    string[] array = ((IEnumerable<Enum>) actions).Select<Enum, string>((Func<Enum, string>) (x => x.ToString())).ToArray<string>();
    return service.FilterAvailableActions(Convert.ToInt32((object) level), array);
  }

  public static bool TryAuthorizeAction(
    this IAuthorizationService service,
    Enum action,
    Enum accessLevel)
  {
    try
    {
      service.AuthorizeAction(action, accessLevel);
      return true;
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public static bool TryAuthorizeAnyAction(
    this IAuthorizationService service,
    Type actionType,
    Enum accessLevel)
  {
    foreach (Enum action in Enum.GetValues(actionType).Cast<Enum>())
    {
      try
      {
        service.AuthorizeAction(action, accessLevel);
        return true;
      }
      catch (Exception ex)
      {
      }
    }
    return false;
  }
}
