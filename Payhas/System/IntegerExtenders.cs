// Decompiled with JetBrains decompiler
// Type: System.IntegerExtenders
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

#nullable disable
namespace System;

public static class IntegerExtenders
{
  public static bool HasBit(this int source, int value)
  {
    try
    {
      return (source & value) == value;
    }
    catch
    {
      return false;
    }
  }

  public static bool IsBit(this int source, int value)
  {
    try
    {
      return source == value;
    }
    catch
    {
      return false;
    }
  }

  public static int AddBit(this int source, int value)
  {
    try
    {
      return source | value;
    }
    catch
    {
      return source;
    }
  }

  public static int RemoveBit(this int source, int value)
  {
    try
    {
      return source & ~value;
    }
    catch
    {
      return source;
    }
  }
}
