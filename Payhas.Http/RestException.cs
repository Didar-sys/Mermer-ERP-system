// Decompiled with JetBrains decompiler
// Type: Payhas.Http.RestException
// Assembly: Payhas.Http, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7DF49D0A-4DE2-4BBD-B7D0-7E5326D360BD
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Http.dll

using System;

#nullable disable
namespace Payhas.Http;

public class RestException
{
  public RestException()
  {
  }

  public RestException(string message, RestException innerException = null)
  {
    this.Message = message;
    this.InnerException = innerException;
  }

  public string Message { get; set; }

  public RestException InnerException { get; set; }

  public static RestException Map(Exception src) => RestException.Map(src, new RestException());

  public static RestException Map(Exception src, RestException dest)
  {
    dest.Message = src.Message;
    if (src.InnerException != null)
      dest.InnerException = RestException.Map(src.InnerException);
    return dest;
  }

  public Exception ToExecption() => new Exception(this.Message, this.InnerException?.ToExecption());
}
