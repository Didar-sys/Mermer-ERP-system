// Decompiled with JetBrains decompiler
// Type: System.TypeExtender
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System.Reflection;

#nullable disable
namespace System;

public static class TypeExtender
{
  public static bool IsInstanceOfGenericType(this object instance, Type genericType)
  {
    return instance != null && instance.GetType().IsOfGenericType(genericType);
  }

  public static bool IsOfGenericType(this Type type, Type genericType)
  {
    for (; (object) type != null; type = type.GetTypeInfo().BaseType)
    {
      if (type.GetTypeInfo().IsGenericType && (object) type.GetGenericTypeDefinition() == (object) genericType)
        return true;
    }
    return false;
  }
}
