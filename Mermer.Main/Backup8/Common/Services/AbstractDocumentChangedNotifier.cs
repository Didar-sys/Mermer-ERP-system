// Decompiled with JetBrains decompiler
// Type: Mermer.Common.Services.AbstractDocumentChangedNotifier
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable disable
namespace Mermer.Common.Services;

public abstract class AbstractDocumentChangedNotifier : IDocumentChangedNotifier
{
  public virtual void DocumentChanged(string typeName, string id)
  {
    Type type = ((IEnumerable<Type>) typeof (IDocumentChangedNotifier).GetTypeInfo().Assembly.GetTypes()).SingleOrDefault<Type>((Func<Type, bool>) (t => t.Name == typeName));
    if (!(type != (Type) null))
      return;
    this.DocumentChanged(type, id);
  }

  public virtual void DocumentChanged(Type type, string id)
  {
    this.GetType().GetMethod(nameof (DocumentChanged), new Type[1]
    {
      typeof (string)
    }).MakeGenericMethod(type).Invoke((object) this, new object[1]
    {
      (object) id
    });
  }

  public abstract void DocumentChanged<T>(string id);
}
