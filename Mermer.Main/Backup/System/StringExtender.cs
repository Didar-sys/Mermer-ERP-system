// Decompiled with JetBrains decompiler
// Type: System.StringExtender
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Org.BouncyCastle.Crypto.Digests;
using System.Text;

#nullable disable
namespace System;

public static class StringExtender
{
  public static string Hash(this string source)
  {
    Sha1Digest sha1Digest = new Sha1Digest();
    byte[] bytes = new UTF8Encoding().GetBytes(source);
    sha1Digest.BlockUpdate(bytes, 0, bytes.Length);
    byte[] numArray = new byte[sha1Digest.GetDigestSize()];
    sha1Digest.DoFinal(numArray, 0);
    return Convert.ToBase64String(numArray);
  }
}
