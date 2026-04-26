// Decompiled with JetBrains decompiler
// Type: Payhas.Licensing.Client.Crypto.CryptoService
// Assembly: Payhas.Licensing.Client, Version=0.0.6.0, Culture=neutral, PublicKeyToken=null
// MVID: D27E04BC-87FA-488D-A2D4-54F1C56BAB05
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Licensing.Client.dll

using NETCore.Encrypt;
using Payhas.Licensing.Client.Services;
using System;
using System.Security.Cryptography;
using System.Text;

#nullable disable
namespace Payhas.Licensing.Client.Crypto;

public class CryptoService : ICryptoService
{
  public string EncryptData(string message, string publicKey)
  {
    return EncryptProvider.RSAEncrypt(publicKey, message, RSAEncryptionPadding.Pkcs1);
  }

  public string DecryptData(string message, string privateKey)
  {
    return EncryptProvider.RSADecrypt(privateKey, message, RSAEncryptionPadding.Pkcs1);
  }

  public string SignData(string message, string privateKey)
  {
    using (RSA rsa = EncryptProvider.RSAFromString(privateKey))
      return rsa.SignData(Encoding.UTF8.GetBytes(message), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1).ToHexString();
  }

  public bool VerifyData(string message, string signature, string publicKey)
  {
    using (RSA rsa = EncryptProvider.RSAFromString(publicKey))
      return rsa.VerifyData(Encoding.UTF8.GetBytes(message), signature.ToBytes(), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
  }
}
