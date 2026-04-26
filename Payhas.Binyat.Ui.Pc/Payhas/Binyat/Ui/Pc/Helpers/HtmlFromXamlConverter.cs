// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Helpers.HtmlFromXamlConverter
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using System;
using System.IO;
using System.Text;
using System.Xml;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Helpers;

public static class HtmlFromXamlConverter
{
  internal static string ConvertXamlToHtml(string xamlString)
  {
    XmlTextReader xamlReader = new XmlTextReader((TextReader) new StringReader(xamlString));
    StringBuilder sb = new StringBuilder(100);
    XmlTextWriter htmlWriter = new XmlTextWriter((TextWriter) new StringWriter(sb));
    return !HtmlFromXamlConverter.WriteFlowDocument(xamlReader, htmlWriter) ? "" : sb.ToString();
  }

  private static bool WriteFlowDocument(XmlTextReader xamlReader, XmlTextWriter htmlWriter)
  {
    if (!HtmlFromXamlConverter.ReadNextToken((XmlReader) xamlReader) || xamlReader.NodeType != XmlNodeType.Element || xamlReader.Name != "FlowDocument")
      return false;
    StringBuilder inlineStyle = new StringBuilder();
    htmlWriter.WriteStartElement("HTML");
    htmlWriter.WriteStartElement("BODY");
    HtmlFromXamlConverter.WriteFormattingProperties(xamlReader, htmlWriter, inlineStyle);
    HtmlFromXamlConverter.WriteElementContent(xamlReader, htmlWriter, inlineStyle);
    htmlWriter.WriteEndElement();
    htmlWriter.WriteEndElement();
    return true;
  }

  private static void WriteFormattingProperties(
    XmlTextReader xamlReader,
    XmlTextWriter htmlWriter,
    StringBuilder inlineStyle)
  {
    inlineStyle.Remove(0, inlineStyle.Length);
    if (!xamlReader.HasAttributes)
      return;
    bool flag = false;
    while (xamlReader.MoveToNextAttribute())
    {
      string str = (string) null;
      switch (xamlReader.Name)
      {
        case "Background":
          str = $"background-color:{HtmlFromXamlConverter.ParseXamlColor(xamlReader.Value)};";
          break;
        case "BorderBrush":
          str = $"border-color:{HtmlFromXamlConverter.ParseXamlColor(xamlReader.Value)};";
          flag = true;
          break;
        case "BorderThickness":
          str = $"border-width:{HtmlFromXamlConverter.ParseXamlThickness(xamlReader.Value)};";
          flag = true;
          break;
        case "ColumnSpan":
          htmlWriter.WriteAttributeString("COLSPAN", xamlReader.Value);
          break;
        case "FontFamily":
          str = $"font-family:{xamlReader.Value};";
          break;
        case "FontSize":
          str = $"font-size:{xamlReader.Value};";
          break;
        case "FontStyle":
          str = $"font-style:{xamlReader.Value.ToLower()};";
          break;
        case "FontWeight":
          str = $"font-weight:{xamlReader.Value.ToLower()};";
          break;
        case "Foreground":
          str = $"color:{HtmlFromXamlConverter.ParseXamlColor(xamlReader.Value)};";
          break;
        case "Margin":
          str = $"margin:{HtmlFromXamlConverter.ParseXamlThickness(xamlReader.Value)};";
          break;
        case "Padding":
          str = $"padding:{HtmlFromXamlConverter.ParseXamlThickness(xamlReader.Value)};";
          break;
        case "RowSpan":
          htmlWriter.WriteAttributeString("ROWSPAN", xamlReader.Value);
          break;
        case "TextAlignment":
          str = $"text-align:{xamlReader.Value};";
          break;
        case "TextDecorations":
          str = "text-decoration:underline;";
          break;
        case "TextIndent":
          str = $"text-indent:{xamlReader.Value};";
          break;
        case "Width":
          str = $"width:{xamlReader.Value};";
          break;
      }
      if (str != null)
        inlineStyle.Append(str);
    }
    if (flag)
      inlineStyle.Append("border-style:solid;mso-element:para-border-div;");
    xamlReader.MoveToElement();
  }

  private static string ParseXamlColor(string color)
  {
    if (color.StartsWith("#"))
      color = "#" + color.Substring(3);
    return color;
  }

  private static string ParseXamlThickness(string thickness)
  {
    string[] strArray = thickness.Split(',');
    for (int index = 0; index < strArray.Length; ++index)
    {
      double result;
      strArray[index] = !double.TryParse(strArray[index], out result) ? "1" : Math.Ceiling(result).ToString();
    }
    string xamlThickness;
    switch (strArray.Length)
    {
      case 1:
        xamlThickness = thickness;
        break;
      case 2:
        xamlThickness = $"{strArray[1]} {strArray[0]}";
        break;
      case 4:
        xamlThickness = $"{strArray[1]} {strArray[2]} {strArray[3]} {strArray[0]}";
        break;
      default:
        xamlThickness = strArray[0];
        break;
    }
    return xamlThickness;
  }

  private static void WriteElementContent(
    XmlTextReader xamlReader,
    XmlTextWriter htmlWriter,
    StringBuilder inlineStyle)
  {
    bool flag = false;
    if (xamlReader.IsEmptyElement)
    {
      if (htmlWriter != null && !flag && inlineStyle.Length > 0)
      {
        htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
        inlineStyle.Remove(0, inlineStyle.Length);
      }
    }
    else
    {
      while (HtmlFromXamlConverter.ReadNextToken((XmlReader) xamlReader) && xamlReader.NodeType != XmlNodeType.EndElement)
      {
        switch (xamlReader.NodeType)
        {
          case XmlNodeType.Element:
            if (xamlReader.Name.Contains("."))
            {
              HtmlFromXamlConverter.AddComplexProperty(xamlReader, inlineStyle);
              continue;
            }
            if (htmlWriter != null && !flag && inlineStyle.Length > 0)
            {
              htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
              inlineStyle.Remove(0, inlineStyle.Length);
            }
            flag = true;
            HtmlFromXamlConverter.WriteElement(xamlReader, htmlWriter, inlineStyle);
            continue;
          case XmlNodeType.Text:
          case XmlNodeType.CDATA:
          case XmlNodeType.SignificantWhitespace:
            if (htmlWriter != null)
            {
              if (!flag && inlineStyle.Length > 0)
                htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
              htmlWriter.WriteString(xamlReader.Value);
            }
            flag = true;
            continue;
          case XmlNodeType.Comment:
            if (htmlWriter != null)
            {
              if (!flag && inlineStyle.Length > 0)
                htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
              htmlWriter.WriteComment(xamlReader.Value);
            }
            flag = true;
            continue;
          default:
            continue;
        }
      }
    }
  }

  private static void AddComplexProperty(XmlTextReader xamlReader, StringBuilder inlineStyle)
  {
    if (inlineStyle != null && xamlReader.Name.EndsWith(".TextDecorations"))
      inlineStyle.Append("text-decoration:underline;");
    HtmlFromXamlConverter.WriteElementContent(xamlReader, (XmlTextWriter) null, (StringBuilder) null);
  }

  private static void WriteElement(
    XmlTextReader xamlReader,
    XmlTextWriter htmlWriter,
    StringBuilder inlineStyle)
  {
    if (htmlWriter == null)
    {
      HtmlFromXamlConverter.WriteElementContent(xamlReader, (XmlTextWriter) null, (StringBuilder) null);
    }
    else
    {
      string localName;
      switch (xamlReader.Name)
      {
        case "BlockUIContainer":
          localName = "DIV";
          break;
        case "Bold":
          localName = "B";
          break;
        case "InlineUIContainer":
          localName = "SPAN";
          break;
        case "Italic":
          localName = "I";
          break;
        case "List":
          string attribute = xamlReader.GetAttribute("MarkerStyle");
          localName = attribute == null || attribute == "None" || attribute == "Disc" || attribute == "Circle" || attribute == "Square" || attribute == "Box" ? "UL" : "OL";
          break;
        case "ListItem":
          localName = "LI";
          break;
        case "Paragraph":
          localName = "P";
          break;
        case "Run":
        case "Span":
          localName = "SPAN";
          break;
        case "Section":
          localName = "DIV";
          break;
        case "Table":
          localName = "TABLE";
          break;
        case "TableCell":
          localName = "TD";
          break;
        case "TableColumn":
          localName = "COL";
          break;
        case "TableRow":
          localName = "TR";
          break;
        case "TableRowGroup":
          localName = "TBODY";
          break;
        default:
          localName = (string) null;
          break;
      }
      if (htmlWriter != null && localName != null)
      {
        htmlWriter.WriteStartElement(localName);
        HtmlFromXamlConverter.WriteFormattingProperties(xamlReader, htmlWriter, inlineStyle);
        HtmlFromXamlConverter.WriteElementContent(xamlReader, htmlWriter, inlineStyle);
        htmlWriter.WriteEndElement();
      }
      else
        HtmlFromXamlConverter.WriteElementContent(xamlReader, (XmlTextWriter) null, (StringBuilder) null);
    }
  }

  private static bool ReadNextToken(XmlReader xamlReader)
  {
    while (xamlReader.Read())
    {
      switch (xamlReader.NodeType)
      {
        case XmlNodeType.None:
        case XmlNodeType.Element:
        case XmlNodeType.Text:
        case XmlNodeType.CDATA:
        case XmlNodeType.SignificantWhitespace:
        case XmlNodeType.EndElement:
          return true;
        case XmlNodeType.Comment:
          return true;
        case XmlNodeType.Whitespace:
          if (xamlReader.XmlSpace == XmlSpace.Preserve)
            return true;
          continue;
        default:
          continue;
      }
    }
    return false;
  }
}
