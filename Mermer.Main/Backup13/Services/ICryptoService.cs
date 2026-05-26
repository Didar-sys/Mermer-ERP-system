// Decompiled with JetBrains decompiler
// Type: Mermer.Licensing.Client.Services.ICryptoService
// Assembly: Mermer.Licensing.Client, Version=0.0.6.0, Culture=neutral, PublicKeyToken=null
// MVID: D27E04BC-87FA-488D-A2D4-54F1C56BAB05
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Licensing.Client.dll

#nullable disable
namespace Mermer.Licensing.Client.Services;

public interface ICryptoService
{
  string EncryptData(string message, string publicKey);

  string DecryptData(string message, string privateKey);

  string SignData(string message, string privateKey);

  bool VerifyData(string message, string signature, string publicKey);
}
