// Decompiled with JetBrains decompiler
// Type: System.DateTimeExtender
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

#nullable disable
namespace System;

public static class DateTimeExtender
{
  public static DateTime StartOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
  {
    int num = (7 + (date.DayOfWeek - startOfWeek)) % 7;
    return date.AddDays((double) (-1 * num)).Date;
  }

  public static DateTime EndOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
  {
    int num = (6 - (date.DayOfWeek - startOfWeek)) % 7;
    return date.AddDays((double) num).Date;
  }
}
