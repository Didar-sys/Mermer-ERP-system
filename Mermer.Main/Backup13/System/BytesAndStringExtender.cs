// Decompiled with JetBrains decompiler
// Type: System.BytesAndStringExtender
// Assembly: Mermer.Licensing.Client, Version=0.0.6.0, Culture=neutral, PublicKeyToken=null
// MVID: D27E04BC-87FA-488D-A2D4-54F1C56BAB05
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Licensing.Client.dll

using System.Globalization;
using System.Text;

#nullable disable
namespace System;

internal static class BytesAndStringExtender
{
  internal static string ToHexString(this byte[] bytes)
  {
    StringBuilder stringBuilder = new StringBuilder();
    for (int index = 0; index < bytes.Length; ++index)
      stringBuilder.Append(bytes[index].ToString("X2"));
    return stringBuilder.ToString();
  }

  internal static byte[] ToBytes(this string hex)
  {
    if (hex.Length == 0)
      return new byte[1];
    if (hex.Length % 2 == 1)
      hex = "0" + hex;
    byte[] bytes = new byte[hex.Length / 2];
    for (int index = 0; index < hex.Length / 2; ++index)
      bytes[index] = byte.Parse(hex.Substring(2 * index, 2), NumberStyles.AllowHexSpecifier);
    return bytes;
  }
}
