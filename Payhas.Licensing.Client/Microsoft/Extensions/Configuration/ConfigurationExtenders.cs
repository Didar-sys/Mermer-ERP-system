// Decompiled with JetBrains decompiler
// Type: Microsoft.Extensions.Configuration.ConfigurationExtenders
// Assembly: Payhas.Licensing.Client, Version=0.0.6.0, Culture=neutral, PublicKeyToken=null
// MVID: D27E04BC-87FA-488D-A2D4-54F1C56BAB05
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Licensing.Client.dll

using System.Linq;
using System.Text;

#nullable disable
namespace Microsoft.Extensions.Configuration;

public static class ConfigurationExtenders
{
  public static string AsString(this IConfigurationSection section)
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.Append("{");
    bool flag = true;
    foreach (IConfigurationSection child in section.GetChildren())
    {
      if (child.GetChildren().Any<IConfigurationSection>())
      {
        stringBuilder.Append(section.AsString());
      }
      else
      {
        if (!flag)
          stringBuilder.Append(",");
        stringBuilder.Append($"\"{child.Key}\":\"{child.Value}\"");
        flag = false;
      }
    }
    stringBuilder.Append("}");
    return stringBuilder.ToString().Replace("\"null\"", "null").Replace("\"\"", "null");
  }
}
