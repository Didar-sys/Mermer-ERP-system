// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Tools.Barcode.Symbology
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

#nullable disable
namespace Mermer.Data.Tools.Barcode;

public static class Symbology
{
  public static bool IsEan(string barcode, out string mainCode)
  {
    mainCode = barcode.Substring(0, barcode.Length - 1);
    return barcode.Substring(barcode.Length - 1, 1) == Symbology.CalculateChecksumDigit(mainCode);
  }

  public static string CalculateChecksumDigit(string barcode)
  {
    int num1 = 0;
    int num2 = barcode.Length % 2;
    for (int length = barcode.Length; length >= 1; --length)
    {
      int result;
      if (!int.TryParse(barcode.Substring(length - 1, 1), out result))
        return (string) null;
      if (length % 2 == num2)
        num1 += result * 3;
      else
        num1 += result;
    }
    return ((10 - num1 % 10) % 10).ToString();
  }
}
