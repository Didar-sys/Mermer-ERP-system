// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Helpers.HtmlToXamlConverter
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Xml;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Helpers;

public static class HtmlToXamlConverter
{
  private static XmlElement InlineFragmentParentElement;
  public const string Xaml_FlowDocument = "FlowDocument";
  public const string Xaml_Run = "Run";
  public const string Xaml_Span = "Span";
  public const string Xaml_Hyperlink = "Hyperlink";
  public const string Xaml_Hyperlink_NavigateUri = "NavigateUri";
  public const string Xaml_Hyperlink_TargetName = "TargetName";
  public const string Xaml_Section = "Section";
  public const string Xaml_List = "List";
  public const string Xaml_List_MarkerStyle = "MarkerStyle";
  public const string Xaml_List_MarkerStyle_None = "None";
  public const string Xaml_List_MarkerStyle_Decimal = "Decimal";
  public const string Xaml_List_MarkerStyle_Disc = "Disc";
  public const string Xaml_List_MarkerStyle_Circle = "Circle";
  public const string Xaml_List_MarkerStyle_Square = "Square";
  public const string Xaml_List_MarkerStyle_Box = "Box";
  public const string Xaml_List_MarkerStyle_LowerLatin = "LowerLatin";
  public const string Xaml_List_MarkerStyle_UpperLatin = "UpperLatin";
  public const string Xaml_List_MarkerStyle_LowerRoman = "LowerRoman";
  public const string Xaml_List_MarkerStyle_UpperRoman = "UpperRoman";
  public const string Xaml_ListItem = "ListItem";
  public const string Xaml_LineBreak = "LineBreak";
  public const string Xaml_Paragraph = "Paragraph";
  public const string Xaml_Margin = "Margin";
  public const string Xaml_Padding = "Padding";
  public const string Xaml_BorderBrush = "BorderBrush";
  public const string Xaml_BorderThickness = "BorderThickness";
  public const string Xaml_Table = "Table";
  public const string Xaml_TableColumn = "TableColumn";
  public const string Xaml_TableRowGroup = "TableRowGroup";
  public const string Xaml_TableRow = "TableRow";
  public const string Xaml_TableCell = "TableCell";
  public const string Xaml_TableCell_BorderThickness = "BorderThickness";
  public const string Xaml_TableCell_BorderBrush = "BorderBrush";
  public const string Xaml_TableCell_ColumnSpan = "ColumnSpan";
  public const string Xaml_TableCell_RowSpan = "RowSpan";
  public const string Xaml_Width = "Width";
  public const string Xaml_Brushes_Black = "Black";
  public const string Xaml_FontFamily = "FontFamily";
  public const string Xaml_FontSize = "FontSize";
  public const string Xaml_FontSize_XXLarge = "22pt";
  public const string Xaml_FontSize_XLarge = "20pt";
  public const string Xaml_FontSize_Large = "18pt";
  public const string Xaml_FontSize_Medium = "16pt";
  public const string Xaml_FontSize_Small = "12pt";
  public const string Xaml_FontSize_XSmall = "10pt";
  public const string Xaml_FontSize_XXSmall = "8pt";
  public const string Xaml_FontWeight = "FontWeight";
  public const string Xaml_FontWeight_Bold = "Bold";
  public const string Xaml_FontStyle = "FontStyle";
  public const string Xaml_Foreground = "Foreground";
  public const string Xaml_Background = "Background";
  public const string Xaml_TextDecorations = "TextDecorations";
  public const string Xaml_TextDecorations_Underline = "Underline";
  public const string Xaml_TextIndent = "TextIndent";
  public const string Xaml_TextAlignment = "TextAlignment";
  private static string _xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

  public static string ConvertHtmlToXaml(string htmlString, bool asFlowDocument)
  {
    XmlElement html = HtmlParser.ParseHtml(htmlString);
    XmlElement xmlElement = new XmlDocument().CreateElement((string) null, asFlowDocument ? "FlowDocument" : "Section", HtmlToXamlConverter._xamlNamespace);
    CssStylesheet stylesheet = new CssStylesheet(html);
    List<XmlElement> sourceContext = new List<XmlElement>(10);
    HtmlToXamlConverter.InlineFragmentParentElement = (XmlElement) null;
    HtmlToXamlConverter.AddBlock(xmlElement, (XmlNode) html, new Hashtable(), stylesheet, sourceContext);
    if (!asFlowDocument)
      xmlElement = HtmlToXamlConverter.ExtractInlineFragment(xmlElement);
    xmlElement.SetAttribute("xml:space", "preserve");
    return xmlElement.OuterXml;
  }

  public static string GetAttribute(XmlElement element, string attributeName)
  {
    attributeName = attributeName.ToLower();
    for (int i = 0; i < element.Attributes.Count; ++i)
    {
      if (element.Attributes[i].Name.ToLower() == attributeName)
        return element.Attributes[i].Value;
    }
    return (string) null;
  }

  internal static string UnQuote(string value)
  {
    if (value.StartsWith("\"") && value.EndsWith("\"") || value.StartsWith("'") && value.EndsWith("'"))
      value = value.Substring(1, value.Length - 2).Trim();
    return value;
  }

  private static XmlNode AddBlock(
    XmlElement xamlParentElement,
    XmlNode htmlNode,
    Hashtable inheritedProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    switch (htmlNode)
    {
      case XmlComment _:
        HtmlToXamlConverter.DefineInlineFragmentParent((XmlComment) htmlNode, (XmlElement) null);
        break;
      case XmlText _:
        htmlNode = HtmlToXamlConverter.AddImplicitParagraph(xamlParentElement, htmlNode, inheritedProperties, stylesheet, sourceContext);
        break;
      case XmlElement _:
        XmlElement xmlElement = (XmlElement) htmlNode;
        string localName = xmlElement.LocalName;
        if (xmlElement.NamespaceURI != "http://www.w3.org/1999/xhtml")
          return (XmlNode) xmlElement;
        sourceContext.Add(xmlElement);
        switch (localName.ToLower())
        {
          case "blockquote":
          case "body":
          case "caption":
          case "center":
          case "cite":
          case "div":
          case "form":
          case "html":
          case "pre":
            HtmlToXamlConverter.AddSection(xamlParentElement, xmlElement, inheritedProperties, stylesheet, sourceContext);
            goto case "head";
          case "dd":
          case "dl":
          case "dt":
          case "h1":
          case "h2":
          case "h3":
          case "h4":
          case "h5":
          case "h6":
          case "nsrtitle":
          case "p":
          case "textarea":
          case "tt":
            HtmlToXamlConverter.AddParagraph(xamlParentElement, xmlElement, inheritedProperties, stylesheet, sourceContext);
            goto case "head";
          case "dir":
          case "menu":
          case "ol":
          case "ul":
            HtmlToXamlConverter.AddList(xamlParentElement, xmlElement, inheritedProperties, stylesheet, sourceContext);
            goto case "head";
          case "head":
          case "meta":
          case "script":
          case "style":
          case "title":
            sourceContext.RemoveAt(sourceContext.Count - 1);
            break;
          case "img":
            HtmlToXamlConverter.AddImage(xamlParentElement, xmlElement, inheritedProperties, stylesheet, sourceContext);
            goto case "head";
          case "li":
            htmlNode = (XmlNode) HtmlToXamlConverter.AddOrphanListItems(xamlParentElement, xmlElement, inheritedProperties, stylesheet, sourceContext);
            goto case "head";
          case "table":
            HtmlToXamlConverter.AddTable(xamlParentElement, xmlElement, inheritedProperties, stylesheet, sourceContext);
            goto case "head";
          default:
            htmlNode = HtmlToXamlConverter.AddImplicitParagraph(xamlParentElement, (XmlNode) xmlElement, inheritedProperties, stylesheet, sourceContext);
            goto case "head";
        }
        break;
    }
    return htmlNode;
  }

  private static void AddBreak(XmlElement xamlParentElement, string htmlElementName)
  {
    XmlElement element1 = xamlParentElement.OwnerDocument.CreateElement((string) null, "LineBreak", HtmlToXamlConverter._xamlNamespace);
    xamlParentElement.AppendChild((XmlNode) element1);
    if (!(htmlElementName == "hr"))
      return;
    XmlText textNode = xamlParentElement.OwnerDocument.CreateTextNode("----------------------");
    xamlParentElement.AppendChild((XmlNode) textNode);
    XmlElement element2 = xamlParentElement.OwnerDocument.CreateElement((string) null, "LineBreak", HtmlToXamlConverter._xamlNamespace);
    xamlParentElement.AppendChild((XmlNode) element2);
  }

  private static void AddSection(
    XmlElement xamlParentElement,
    XmlElement htmlElement,
    Hashtable inheritedProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    bool flag = false;
    for (XmlNode xmlNode = htmlElement.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
    {
      if (xmlNode is XmlElement && HtmlSchema.IsBlockElement(xmlNode.LocalName.ToLower()))
      {
        flag = true;
        break;
      }
    }
    if (!flag)
    {
      HtmlToXamlConverter.AddParagraph(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
    }
    else
    {
      Hashtable localProperties;
      Hashtable elementProperties = HtmlToXamlConverter.GetElementProperties(htmlElement, inheritedProperties, out localProperties, stylesheet, sourceContext);
      XmlElement xmlElement = xamlParentElement.OwnerDocument.CreateElement((string) null, "Section", HtmlToXamlConverter._xamlNamespace);
      HtmlToXamlConverter.ApplyLocalProperties(xmlElement, localProperties, true);
      if (!xmlElement.HasAttributes)
        xmlElement = xamlParentElement;
      XmlNode htmlNode = htmlElement.FirstChild;
      while (htmlNode != null)
        htmlNode = HtmlToXamlConverter.AddBlock(xmlElement, htmlNode, elementProperties, stylesheet, sourceContext)?.NextSibling;
      if (xmlElement == xamlParentElement)
        return;
      xamlParentElement.AppendChild((XmlNode) xmlElement);
    }
  }

  private static void AddParagraph(
    XmlElement xamlParentElement,
    XmlElement htmlElement,
    Hashtable inheritedProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    Hashtable localProperties;
    Hashtable elementProperties = HtmlToXamlConverter.GetElementProperties(htmlElement, inheritedProperties, out localProperties, stylesheet, sourceContext);
    XmlElement element = xamlParentElement.OwnerDocument.CreateElement((string) null, "Paragraph", HtmlToXamlConverter._xamlNamespace);
    HtmlToXamlConverter.ApplyLocalProperties(element, localProperties, true);
    for (XmlNode htmlNode = htmlElement.FirstChild; htmlNode != null; htmlNode = htmlNode.NextSibling)
      HtmlToXamlConverter.AddInline(element, htmlNode, elementProperties, stylesheet, sourceContext);
    xamlParentElement.AppendChild((XmlNode) element);
  }

  private static XmlNode AddImplicitParagraph(
    XmlElement xamlParentElement,
    XmlNode htmlNode,
    Hashtable inheritedProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    XmlElement element = xamlParentElement.OwnerDocument.CreateElement((string) null, "Paragraph", HtmlToXamlConverter._xamlNamespace);
    XmlNode xmlNode = (XmlNode) null;
    while (true)
    {
      switch (htmlNode)
      {
        case null:
          goto label_8;
        case XmlComment _:
          HtmlToXamlConverter.DefineInlineFragmentParent((XmlComment) htmlNode, (XmlElement) null);
          break;
        case XmlText _:
          if (htmlNode.Value.Trim().Length > 0)
          {
            HtmlToXamlConverter.AddTextRun(element, htmlNode.Value);
            break;
          }
          break;
        case XmlElement _:
          if (!HtmlSchema.IsBlockElement(htmlNode.LocalName.ToLower()))
          {
            HtmlToXamlConverter.AddInline(element, htmlNode, inheritedProperties, stylesheet, sourceContext);
            break;
          }
          goto label_8;
      }
      xmlNode = htmlNode;
      htmlNode = htmlNode.NextSibling;
    }
label_8:
    if (element.FirstChild != null)
      xamlParentElement.AppendChild((XmlNode) element);
    return xmlNode;
  }

  private static void AddInline(
    XmlElement xamlParentElement,
    XmlNode htmlNode,
    Hashtable inheritedProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    switch (htmlNode)
    {
      case XmlComment _:
        HtmlToXamlConverter.DefineInlineFragmentParent((XmlComment) htmlNode, xamlParentElement);
        break;
      case XmlText _:
        HtmlToXamlConverter.AddTextRun(xamlParentElement, htmlNode.Value);
        break;
      case XmlElement _:
        XmlElement htmlElement = (XmlElement) htmlNode;
        if (htmlElement.NamespaceURI != "http://www.w3.org/1999/xhtml")
          break;
        string lower = htmlElement.LocalName.ToLower();
        sourceContext.Add(htmlElement);
        switch (lower)
        {
          case "a":
            HtmlToXamlConverter.AddHyperlink(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
            break;
          case "img":
            HtmlToXamlConverter.AddImage(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
            break;
          case "br":
          case "hr":
            HtmlToXamlConverter.AddBreak(xamlParentElement, lower);
            break;
          default:
            if (HtmlSchema.IsInlineElement(lower) || HtmlSchema.IsBlockElement(lower))
            {
              HtmlToXamlConverter.AddSpanOrRun(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
              break;
            }
            break;
        }
        sourceContext.RemoveAt(sourceContext.Count - 1);
        break;
    }
  }

  private static void AddSpanOrRun(
    XmlElement xamlParentElement,
    XmlElement htmlElement,
    Hashtable inheritedProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    bool flag = false;
    for (XmlNode xmlNode = htmlElement.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
    {
      if (xmlNode is XmlElement)
      {
        string lower = xmlNode.LocalName.ToLower();
        if (HtmlSchema.IsInlineElement(lower) || HtmlSchema.IsBlockElement(lower) || lower == "img" || lower == "br" || lower == "hr")
        {
          flag = true;
          break;
        }
      }
    }
    string localName = flag ? "Span" : "Run";
    Hashtable localProperties;
    Hashtable elementProperties = HtmlToXamlConverter.GetElementProperties(htmlElement, inheritedProperties, out localProperties, stylesheet, sourceContext);
    XmlElement element = xamlParentElement.OwnerDocument.CreateElement((string) null, localName, HtmlToXamlConverter._xamlNamespace);
    HtmlToXamlConverter.ApplyLocalProperties(element, localProperties, false);
    for (XmlNode htmlNode = htmlElement.FirstChild; htmlNode != null; htmlNode = htmlNode.NextSibling)
      HtmlToXamlConverter.AddInline(element, htmlNode, elementProperties, stylesheet, sourceContext);
    xamlParentElement.AppendChild((XmlNode) element);
  }

  private static void AddTextRun(XmlElement xamlElement, string textData)
  {
    for (int index = 0; index < textData.Length; ++index)
    {
      if (char.IsControl(textData[index]))
        textData = textData.Remove(index--, 1);
    }
    textData = textData.Replace(' ', ' ');
    if (textData.Length <= 0)
      return;
    xamlElement.AppendChild((XmlNode) xamlElement.OwnerDocument.CreateTextNode(textData));
  }

  private static void AddHyperlink(
    XmlElement xamlParentElement,
    XmlElement htmlElement,
    Hashtable inheritedProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    string attribute = HtmlToXamlConverter.GetAttribute(htmlElement, "href");
    if (attribute == null)
    {
      HtmlToXamlConverter.AddSpanOrRun(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
    }
    else
    {
      Hashtable localProperties;
      Hashtable elementProperties = HtmlToXamlConverter.GetElementProperties(htmlElement, inheritedProperties, out localProperties, stylesheet, sourceContext);
      XmlElement element = xamlParentElement.OwnerDocument.CreateElement((string) null, "Hyperlink", HtmlToXamlConverter._xamlNamespace);
      HtmlToXamlConverter.ApplyLocalProperties(element, localProperties, false);
      string[] strArray = attribute.Split('#');
      if (strArray.Length != 0 && strArray[0].Trim().Length > 0)
        element.SetAttribute("NavigateUri", strArray[0].Trim());
      if (strArray.Length == 2 && strArray[1].Trim().Length > 0)
        element.SetAttribute("TargetName", strArray[1].Trim());
      for (XmlNode htmlNode = htmlElement.FirstChild; htmlNode != null; htmlNode = htmlNode.NextSibling)
        HtmlToXamlConverter.AddInline(element, htmlNode, elementProperties, stylesheet, sourceContext);
      xamlParentElement.AppendChild((XmlNode) element);
    }
  }

  private static void DefineInlineFragmentParent(
    XmlComment htmlComment,
    XmlElement xamlParentElement)
  {
    if (htmlComment.Value == "StartFragment")
    {
      HtmlToXamlConverter.InlineFragmentParentElement = xamlParentElement;
    }
    else
    {
      if (!(htmlComment.Value == "EndFragment") || HtmlToXamlConverter.InlineFragmentParentElement != null || xamlParentElement == null)
        return;
      HtmlToXamlConverter.InlineFragmentParentElement = xamlParentElement;
    }
  }

  private static XmlElement ExtractInlineFragment(XmlElement xamlFlowDocumentElement)
  {
    if (HtmlToXamlConverter.InlineFragmentParentElement != null)
    {
      if (HtmlToXamlConverter.InlineFragmentParentElement.LocalName == "Span")
      {
        xamlFlowDocumentElement = HtmlToXamlConverter.InlineFragmentParentElement;
      }
      else
      {
        xamlFlowDocumentElement = xamlFlowDocumentElement.OwnerDocument.CreateElement((string) null, "Span", HtmlToXamlConverter._xamlNamespace);
        while (HtmlToXamlConverter.InlineFragmentParentElement.FirstChild != null)
        {
          XmlNode firstChild = HtmlToXamlConverter.InlineFragmentParentElement.FirstChild;
          HtmlToXamlConverter.InlineFragmentParentElement.RemoveChild(firstChild);
          xamlFlowDocumentElement.AppendChild(firstChild);
        }
      }
    }
    return xamlFlowDocumentElement;
  }

  private static void AddImage(
    XmlElement xamlParentElement,
    XmlElement htmlElement,
    Hashtable inheritedProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
  }

  private static void AddList(
    XmlElement xamlParentElement,
    XmlElement htmlListElement,
    Hashtable inheritedProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    string lower = htmlListElement.LocalName.ToLower();
    Hashtable localProperties;
    Hashtable elementProperties = HtmlToXamlConverter.GetElementProperties(htmlListElement, inheritedProperties, out localProperties, stylesheet, sourceContext);
    XmlElement element = xamlParentElement.OwnerDocument.CreateElement((string) null, "List", HtmlToXamlConverter._xamlNamespace);
    if (lower == "ol")
      element.SetAttribute("MarkerStyle", "Decimal");
    else
      element.SetAttribute("MarkerStyle", "Disc");
    HtmlToXamlConverter.ApplyLocalProperties(element, localProperties, true);
    for (XmlNode htmlLIElement = htmlListElement.FirstChild; htmlLIElement != null; htmlLIElement = htmlLIElement.NextSibling)
    {
      if (htmlLIElement is XmlElement && htmlLIElement.LocalName.ToLower() == "li")
      {
        sourceContext.Add((XmlElement) htmlLIElement);
        HtmlToXamlConverter.AddListItem(element, (XmlElement) htmlLIElement, elementProperties, stylesheet, sourceContext);
        sourceContext.RemoveAt(sourceContext.Count - 1);
      }
    }
    if (!element.HasChildNodes)
      return;
    xamlParentElement.AppendChild((XmlNode) element);
  }

  private static XmlElement AddOrphanListItems(
    XmlElement xamlParentElement,
    XmlElement htmlLIElement,
    Hashtable inheritedProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    XmlElement xmlElement1 = (XmlElement) null;
    XmlNode lastChild = xamlParentElement.LastChild;
    XmlElement xmlElement2;
    if (lastChild != null && lastChild.LocalName == "List")
    {
      xmlElement2 = (XmlElement) lastChild;
    }
    else
    {
      xmlElement2 = xamlParentElement.OwnerDocument.CreateElement((string) null, "List", HtmlToXamlConverter._xamlNamespace);
      xamlParentElement.AppendChild((XmlNode) xmlElement2);
    }
    XmlNode htmlLIElement1 = (XmlNode) htmlLIElement;
    for (string lower = htmlLIElement1 == null ? (string) null : htmlLIElement1.LocalName.ToLower(); htmlLIElement1 != null && lower == "li"; lower = htmlLIElement1 == null ? (string) null : htmlLIElement1.LocalName.ToLower())
    {
      HtmlToXamlConverter.AddListItem(xmlElement2, (XmlElement) htmlLIElement1, inheritedProperties, stylesheet, sourceContext);
      xmlElement1 = (XmlElement) htmlLIElement1;
      htmlLIElement1 = htmlLIElement1.NextSibling;
    }
    return xmlElement1;
  }

  private static void AddListItem(
    XmlElement xamlListElement,
    XmlElement htmlLIElement,
    Hashtable inheritedProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    Hashtable elementProperties = HtmlToXamlConverter.GetElementProperties(htmlLIElement, inheritedProperties, out Hashtable _, stylesheet, sourceContext);
    XmlElement element = xamlListElement.OwnerDocument.CreateElement((string) null, "ListItem", HtmlToXamlConverter._xamlNamespace);
    XmlNode htmlNode = htmlLIElement.FirstChild;
    while (htmlNode != null)
      htmlNode = HtmlToXamlConverter.AddBlock(element, htmlNode, elementProperties, stylesheet, sourceContext)?.NextSibling;
    xamlListElement.AppendChild((XmlNode) element);
  }

  private static void AddTable(
    XmlElement xamlParentElement,
    XmlElement htmlTableElement,
    Hashtable inheritedProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    Hashtable elementProperties1 = HtmlToXamlConverter.GetElementProperties(htmlTableElement, inheritedProperties, out Hashtable _, stylesheet, sourceContext);
    XmlElement fromSingleCellTable = HtmlToXamlConverter.GetCellFromSingleCellTable(htmlTableElement);
    if (fromSingleCellTable != null)
    {
      sourceContext.Add(fromSingleCellTable);
      XmlNode htmlNode = fromSingleCellTable.FirstChild;
      while (htmlNode != null)
        htmlNode = HtmlToXamlConverter.AddBlock(xamlParentElement, htmlNode, elementProperties1, stylesheet, sourceContext)?.NextSibling;
      sourceContext.RemoveAt(sourceContext.Count - 1);
    }
    else
    {
      XmlElement element1 = xamlParentElement.OwnerDocument.CreateElement((string) null, "Table", HtmlToXamlConverter._xamlNamespace);
      ArrayList arrayList = HtmlToXamlConverter.AnalyzeTableStructure(htmlTableElement, stylesheet);
      HtmlToXamlConverter.AddColumnInformation(htmlTableElement, element1, arrayList, elementProperties1, stylesheet, sourceContext);
      XmlNode xmlNode = htmlTableElement.FirstChild;
      while (xmlNode != null)
      {
        switch (xmlNode.LocalName.ToLower())
        {
          case "tbody":
          case "thead":
          case "tfoot":
            XmlElement element2 = element1.OwnerDocument.CreateElement((string) null, "TableRowGroup", HtmlToXamlConverter._xamlNamespace);
            element1.AppendChild((XmlNode) element2);
            sourceContext.Add((XmlElement) xmlNode);
            Hashtable elementProperties2 = HtmlToXamlConverter.GetElementProperties((XmlElement) xmlNode, elementProperties1, out Hashtable _, stylesheet, sourceContext);
            HtmlToXamlConverter.AddTableRowsToTableBody(element2, xmlNode.FirstChild, elementProperties2, arrayList, stylesheet, sourceContext);
            if (element2.HasChildNodes)
              element1.AppendChild((XmlNode) element2);
            sourceContext.RemoveAt(sourceContext.Count - 1);
            xmlNode = xmlNode.NextSibling;
            continue;
          case "tr":
            XmlElement element3 = element1.OwnerDocument.CreateElement((string) null, "TableRowGroup", HtmlToXamlConverter._xamlNamespace);
            xmlNode = HtmlToXamlConverter.AddTableRowsToTableBody(element3, xmlNode, elementProperties1, arrayList, stylesheet, sourceContext);
            if (element3.HasChildNodes)
            {
              element1.AppendChild((XmlNode) element3);
              continue;
            }
            continue;
          default:
            xmlNode = xmlNode.NextSibling;
            continue;
        }
      }
      if (!element1.HasChildNodes)
        return;
      xamlParentElement.AppendChild((XmlNode) element1);
    }
  }

  private static XmlElement GetCellFromSingleCellTable(XmlElement htmlTableElement)
  {
    XmlElement fromSingleCellTable = (XmlElement) null;
    for (XmlNode xmlNode1 = htmlTableElement.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
    {
      string lower1 = xmlNode1.LocalName.ToLower();
      if (lower1 == "tbody" || lower1 == "thead" || lower1 == "tfoot")
      {
        if (fromSingleCellTable != null)
          return (XmlElement) null;
        for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
        {
          if (xmlNode2.LocalName.ToLower() == "tr")
          {
            if (fromSingleCellTable != null)
              return (XmlElement) null;
            for (XmlNode xmlNode3 = xmlNode2.FirstChild; xmlNode3 != null; xmlNode3 = xmlNode3.NextSibling)
            {
              string lower2 = xmlNode3.LocalName.ToLower();
              if (lower2 == "td" || lower2 == "th")
              {
                if (fromSingleCellTable != null)
                  return (XmlElement) null;
                fromSingleCellTable = (XmlElement) xmlNode3;
              }
            }
          }
        }
      }
      else if (xmlNode1.LocalName.ToLower() == "tr")
      {
        if (fromSingleCellTable != null)
          return (XmlElement) null;
        for (XmlNode xmlNode4 = xmlNode1.FirstChild; xmlNode4 != null; xmlNode4 = xmlNode4.NextSibling)
        {
          string lower3 = xmlNode4.LocalName.ToLower();
          if (lower3 == "td" || lower3 == "th")
          {
            if (fromSingleCellTable != null)
              return (XmlElement) null;
            fromSingleCellTable = (XmlElement) xmlNode4;
          }
        }
      }
    }
    return fromSingleCellTable;
  }

  private static void AddColumnInformation(
    XmlElement htmlTableElement,
    XmlElement xamlTableElement,
    ArrayList columnStartsAllRows,
    Hashtable currentProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    if (columnStartsAllRows != null)
    {
      for (int index = 0; index < columnStartsAllRows.Count - 1; ++index)
      {
        XmlElement element = xamlTableElement.OwnerDocument.CreateElement((string) null, "TableColumn", HtmlToXamlConverter._xamlNamespace);
        element.SetAttribute("Width", ((double) columnStartsAllRows[index + 1] - (double) columnStartsAllRows[index]).ToString());
        xamlTableElement.AppendChild((XmlNode) element);
      }
    }
    else
    {
      for (XmlNode xmlNode = htmlTableElement.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
      {
        if (xmlNode.LocalName.ToLower() == "colgroup")
          HtmlToXamlConverter.AddTableColumnGroup(xamlTableElement, (XmlElement) xmlNode, currentProperties, stylesheet, sourceContext);
        else if (xmlNode.LocalName.ToLower() == "col")
          HtmlToXamlConverter.AddTableColumn(xamlTableElement, (XmlElement) xmlNode, currentProperties, stylesheet, sourceContext);
        else if (xmlNode is XmlElement)
          break;
      }
    }
  }

  private static void AddTableColumnGroup(
    XmlElement xamlTableElement,
    XmlElement htmlColgroupElement,
    Hashtable inheritedProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    Hashtable elementProperties = HtmlToXamlConverter.GetElementProperties(htmlColgroupElement, inheritedProperties, out Hashtable _, stylesheet, sourceContext);
    for (XmlNode htmlColElement = htmlColgroupElement.FirstChild; htmlColElement != null; htmlColElement = htmlColElement.NextSibling)
    {
      if (htmlColElement is XmlElement && htmlColElement.LocalName.ToLower() == "col")
        HtmlToXamlConverter.AddTableColumn(xamlTableElement, (XmlElement) htmlColElement, elementProperties, stylesheet, sourceContext);
    }
  }

  private static void AddTableColumn(
    XmlElement xamlTableElement,
    XmlElement htmlColElement,
    Hashtable inheritedProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    HtmlToXamlConverter.GetElementProperties(htmlColElement, inheritedProperties, out Hashtable _, stylesheet, sourceContext);
    XmlElement element = xamlTableElement.OwnerDocument.CreateElement((string) null, "TableColumn", HtmlToXamlConverter._xamlNamespace);
    xamlTableElement.AppendChild((XmlNode) element);
  }

  private static XmlNode AddTableRowsToTableBody(
    XmlElement xamlTableBodyElement,
    XmlNode htmlTRStartNode,
    Hashtable currentProperties,
    ArrayList columnStarts,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    XmlNode tableBody = htmlTRStartNode;
    ArrayList activeRowSpans = (ArrayList) null;
    if (columnStarts != null)
    {
      activeRowSpans = new ArrayList();
      HtmlToXamlConverter.InitializeActiveRowSpans(activeRowSpans, columnStarts.Count);
    }
    while (tableBody != null && tableBody.LocalName.ToLower() != "tbody")
    {
      if (tableBody.LocalName.ToLower() == "tr")
      {
        XmlElement element = xamlTableBodyElement.OwnerDocument.CreateElement((string) null, "TableRow", HtmlToXamlConverter._xamlNamespace);
        sourceContext.Add((XmlElement) tableBody);
        Hashtable elementProperties = HtmlToXamlConverter.GetElementProperties((XmlElement) tableBody, currentProperties, out Hashtable _, stylesheet, sourceContext);
        HtmlToXamlConverter.AddTableCellsToTableRow(element, tableBody.FirstChild, elementProperties, columnStarts, activeRowSpans, stylesheet, sourceContext);
        if (element.HasChildNodes)
          xamlTableBodyElement.AppendChild((XmlNode) element);
        sourceContext.RemoveAt(sourceContext.Count - 1);
        tableBody = tableBody.NextSibling;
      }
      else if (tableBody.LocalName.ToLower() == "td")
      {
        XmlElement element = xamlTableBodyElement.OwnerDocument.CreateElement((string) null, "TableRow", HtmlToXamlConverter._xamlNamespace);
        tableBody = HtmlToXamlConverter.AddTableCellsToTableRow(element, tableBody, currentProperties, columnStarts, activeRowSpans, stylesheet, sourceContext);
        if (element.HasChildNodes)
          xamlTableBodyElement.AppendChild((XmlNode) element);
      }
      else
        tableBody = tableBody.NextSibling;
    }
    return tableBody;
  }

  private static XmlNode AddTableCellsToTableRow(
    XmlElement xamlTableRowElement,
    XmlNode htmlTDStartNode,
    Hashtable currentProperties,
    ArrayList columnStarts,
    ArrayList activeRowSpans,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    XmlNode tableRow = htmlTDStartNode;
    int num = 0;
    while (tableRow != null && tableRow.LocalName.ToLower() != "tr" && tableRow.LocalName.ToLower() != "tbody" && tableRow.LocalName.ToLower() != "thead" && tableRow.LocalName.ToLower() != "tfoot")
    {
      if (tableRow.LocalName.ToLower() == "td" || tableRow.LocalName.ToLower() == "th")
      {
        XmlElement element = xamlTableRowElement.OwnerDocument.CreateElement((string) null, "TableCell", HtmlToXamlConverter._xamlNamespace);
        sourceContext.Add((XmlElement) tableRow);
        Hashtable elementProperties = HtmlToXamlConverter.GetElementProperties((XmlElement) tableRow, currentProperties, out Hashtable _, stylesheet, sourceContext);
        HtmlToXamlConverter.ApplyPropertiesToTableCellElement((XmlElement) tableRow, element);
        if (columnStarts != null)
        {
          for (; num < activeRowSpans.Count && (int) activeRowSpans[num] > 0; ++num)
            activeRowSpans[num] = (object) ((int) activeRowSpans[num] - 1);
          double columnStart = (double) columnStarts[num];
          double columnWidth = HtmlToXamlConverter.GetColumnWidth((XmlElement) tableRow);
          int columnSpan = HtmlToXamlConverter.CalculateColumnSpan(num, columnWidth, columnStarts);
          int rowSpan = HtmlToXamlConverter.GetRowSpan((XmlElement) tableRow);
          element.SetAttribute("ColumnSpan", columnSpan.ToString());
          for (int index = num; index < num + columnSpan; ++index)
            activeRowSpans[index] = (object) (rowSpan - 1);
          num += columnSpan;
        }
        HtmlToXamlConverter.AddDataToTableCell(element, tableRow.FirstChild, elementProperties, stylesheet, sourceContext);
        if (element.HasChildNodes)
          xamlTableRowElement.AppendChild((XmlNode) element);
        sourceContext.RemoveAt(sourceContext.Count - 1);
        tableRow = tableRow.NextSibling;
      }
      else
        tableRow = tableRow.NextSibling;
    }
    return tableRow;
  }

  private static void AddDataToTableCell(
    XmlElement xamlTableCellElement,
    XmlNode htmlDataStartNode,
    Hashtable currentProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    XmlNode htmlNode = htmlDataStartNode;
    while (htmlNode != null)
      htmlNode = HtmlToXamlConverter.AddBlock(xamlTableCellElement, htmlNode, currentProperties, stylesheet, sourceContext)?.NextSibling;
  }

  private static ArrayList AnalyzeTableStructure(
    XmlElement htmlTableElement,
    CssStylesheet stylesheet)
  {
    if (!htmlTableElement.HasChildNodes)
      return (ArrayList) null;
    bool flag = true;
    ArrayList columnStarts = new ArrayList();
    ArrayList activeRowSpans = new ArrayList();
    XmlNode xmlNode = htmlTableElement.FirstChild;
    double tableWidth = 0.0;
    for (; xmlNode != null & flag; xmlNode = xmlNode.NextSibling)
    {
      switch (xmlNode.LocalName.ToLower())
      {
        case "tbody":
          double num1 = HtmlToXamlConverter.AnalyzeTbodyStructure((XmlElement) xmlNode, columnStarts, activeRowSpans, tableWidth, stylesheet);
          if (num1 > tableWidth)
          {
            tableWidth = num1;
            break;
          }
          if (num1 == 0.0)
          {
            flag = false;
            break;
          }
          break;
        case "tr":
          double num2 = HtmlToXamlConverter.AnalyzeTRStructure((XmlElement) xmlNode, columnStarts, activeRowSpans, tableWidth, stylesheet);
          if (num2 > tableWidth)
          {
            tableWidth = num2;
            break;
          }
          if (num2 == 0.0)
          {
            flag = false;
            break;
          }
          break;
        case "td":
          flag = false;
          break;
      }
    }
    if (flag)
    {
      columnStarts.Add((object) tableWidth);
      HtmlToXamlConverter.VerifyColumnStartsAscendingOrder(columnStarts);
    }
    else
      columnStarts = (ArrayList) null;
    return columnStarts;
  }

  private static double AnalyzeTbodyStructure(
    XmlElement htmlTbodyElement,
    ArrayList columnStarts,
    ArrayList activeRowSpans,
    double tableWidth,
    CssStylesheet stylesheet)
  {
    double tableWidth1 = 0.0;
    bool flag = true;
    if (!htmlTbodyElement.HasChildNodes)
      return tableWidth1;
    HtmlToXamlConverter.ClearActiveRowSpans(activeRowSpans);
    for (XmlNode htmlTRElement = htmlTbodyElement.FirstChild; htmlTRElement != null & flag; htmlTRElement = htmlTRElement.NextSibling)
    {
      switch (htmlTRElement.LocalName.ToLower())
      {
        case "tr":
          double num = HtmlToXamlConverter.AnalyzeTRStructure((XmlElement) htmlTRElement, columnStarts, activeRowSpans, tableWidth1, stylesheet);
          if (num > tableWidth1)
          {
            tableWidth1 = num;
            break;
          }
          break;
        case "td":
          flag = false;
          break;
      }
    }
    HtmlToXamlConverter.ClearActiveRowSpans(activeRowSpans);
    return !flag ? 0.0 : tableWidth1;
  }

  private static double AnalyzeTRStructure(
    XmlElement htmlTRElement,
    ArrayList columnStarts,
    ArrayList activeRowSpans,
    double tableWidth,
    CssStylesheet stylesheet)
  {
    if (!htmlTRElement.HasChildNodes)
      return 0.0;
    bool flag = true;
    double num1 = 0.0;
    XmlNode htmlTDElement = htmlTRElement.FirstChild;
    int num2 = 0;
    if (num2 < activeRowSpans.Count && (double) columnStarts[num2] == num1)
    {
      while (num2 < activeRowSpans.Count && (int) activeRowSpans[num2] > 0)
      {
        activeRowSpans[num2] = (object) ((int) activeRowSpans[num2] - 1);
        ++num2;
        num1 = (double) columnStarts[num2];
      }
    }
    for (; htmlTDElement != null & flag; htmlTDElement = htmlTDElement.NextSibling)
    {
      HtmlToXamlConverter.VerifyColumnStartsAscendingOrder(columnStarts);
      if (htmlTDElement.LocalName.ToLower() == "td")
      {
        if (num2 < columnStarts.Count)
        {
          if (num1 < (double) columnStarts[num2])
          {
            columnStarts.Insert(num2, (object) num1);
            activeRowSpans.Insert(num2, (object) 0);
          }
        }
        else
        {
          columnStarts.Add((object) num1);
          activeRowSpans.Add((object) 0);
        }
        double columnWidth = HtmlToXamlConverter.GetColumnWidth((XmlElement) htmlTDElement);
        if (columnWidth != -1.0)
        {
          int rowSpan = HtmlToXamlConverter.GetRowSpan((XmlElement) htmlTDElement);
          int nextColumnIndex = HtmlToXamlConverter.GetNextColumnIndex(num2, columnWidth, columnStarts, activeRowSpans);
          if (nextColumnIndex != -1)
          {
            for (int index = num2; index < nextColumnIndex; ++index)
              activeRowSpans[index] = (object) (rowSpan - 1);
            num2 = nextColumnIndex;
            num1 += columnWidth;
            if (num2 < activeRowSpans.Count && (double) columnStarts[num2] == num1)
            {
              while (num2 < activeRowSpans.Count && (int) activeRowSpans[num2] > 0)
              {
                activeRowSpans[num2] = (object) ((int) activeRowSpans[num2] - 1);
                ++num2;
                num1 = (double) columnStarts[num2];
              }
            }
          }
          else
            flag = false;
        }
        else
          flag = false;
      }
    }
    return !flag ? 0.0 : num1;
  }

  private static int GetRowSpan(XmlElement htmlTDElement)
  {
    string attribute = HtmlToXamlConverter.GetAttribute(htmlTDElement, "rowspan");
    int result;
    if (attribute != null)
    {
      if (!int.TryParse(attribute, out result))
        result = 1;
    }
    else
      result = 1;
    return result;
  }

  private static int GetNextColumnIndex(
    int columnIndex,
    double columnWidth,
    ArrayList columnStarts,
    ArrayList activeRowSpans)
  {
    double columnStart = (double) columnStarts[columnIndex];
    int index = columnIndex + 1;
    while (index < columnStarts.Count && (double) columnStarts[index] < columnStart + columnWidth && index != -1)
    {
      if ((int) activeRowSpans[index] > 0)
        index = -1;
      else
        ++index;
    }
    return index;
  }

  private static void ClearActiveRowSpans(ArrayList activeRowSpans)
  {
    for (int index = 0; index < activeRowSpans.Count; ++index)
      activeRowSpans[index] = (object) 0;
  }

  private static void InitializeActiveRowSpans(ArrayList activeRowSpans, int count)
  {
    for (int index = 0; index < count; ++index)
      activeRowSpans.Add((object) 0);
  }

  private static double GetNextColumnStart(XmlElement htmlTDElement, double columnStart)
  {
    double columnWidth = HtmlToXamlConverter.GetColumnWidth(htmlTDElement);
    return columnWidth != -1.0 ? columnStart + columnWidth : -1.0;
  }

  private static double GetColumnWidth(XmlElement htmlTDElement)
  {
    double length = -1.0;
    if (!HtmlToXamlConverter.TryGetLengthValue(HtmlToXamlConverter.GetAttribute(htmlTDElement, "width") ?? HtmlToXamlConverter.GetCssAttribute(HtmlToXamlConverter.GetAttribute(htmlTDElement, "style"), "width"), out length) || length == 0.0)
      length = -1.0;
    return length;
  }

  private static int CalculateColumnSpan(
    int columnIndex,
    double columnWidth,
    ArrayList columnStarts)
  {
    int index = columnIndex;
    double num1 = 0.0;
    for (; num1 < columnWidth && index < columnStarts.Count - 1; ++index)
    {
      double num2 = (double) columnStarts[index + 1] - (double) columnStarts[index];
      num1 += num2;
    }
    return index - columnIndex;
  }

  private static void VerifyColumnStartsAscendingOrder(ArrayList columnStarts)
  {
    for (int index = 0; index < columnStarts.Count; ++index)
    {
      double columnStart = (double) columnStarts[index];
    }
  }

  private static void ApplyLocalProperties(
    XmlElement xamlElement,
    Hashtable localProperties,
    bool isBlock)
  {
    bool flag1 = false;
    string top1 = "0";
    string bottom1 = "0";
    string left1 = "0";
    string right1 = "0";
    bool flag2 = false;
    string top2 = "0";
    string bottom2 = "0";
    string left2 = "0";
    string right2 = "0";
    string str1 = (string) null;
    bool flag3 = false;
    string top3 = "0";
    string bottom3 = "0";
    string left3 = "0";
    string right3 = "0";
    IDictionaryEnumerator enumerator = localProperties.GetEnumerator();
    while (enumerator.MoveNext())
    {
      switch ((string) enumerator.Key)
      {
        case "background-color":
          HtmlToXamlConverter.SetPropertyValue(xamlElement, TextElement.BackgroundProperty, (string) enumerator.Value);
          continue;
        case "border-color-bottom":
          str1 = (string) enumerator.Value;
          continue;
        case "border-color-left":
          str1 = (string) enumerator.Value;
          continue;
        case "border-color-right":
          str1 = (string) enumerator.Value;
          continue;
        case "border-color-top":
          str1 = (string) enumerator.Value;
          continue;
        case "border-width-bottom":
          flag3 = true;
          bottom3 = (string) enumerator.Value;
          continue;
        case "border-width-left":
          flag3 = true;
          left3 = (string) enumerator.Value;
          continue;
        case "border-width-right":
          flag3 = true;
          right3 = (string) enumerator.Value;
          continue;
        case "border-width-top":
          flag3 = true;
          top3 = (string) enumerator.Value;
          continue;
        case "clear":
        case "float":
          int num = isBlock ? 1 : 0;
          continue;
        case "color":
          HtmlToXamlConverter.SetPropertyValue(xamlElement, TextElement.ForegroundProperty, (string) enumerator.Value);
          continue;
        case "font-family":
          xamlElement.SetAttribute("FontFamily", (string) enumerator.Value);
          continue;
        case "font-size":
          xamlElement.SetAttribute("FontSize", (string) enumerator.Value);
          continue;
        case "font-style":
          xamlElement.SetAttribute("FontStyle", (string) enumerator.Value);
          continue;
        case "font-weight":
          xamlElement.SetAttribute("FontWeight", (string) enumerator.Value);
          continue;
        case "list-style-type":
          if (xamlElement.LocalName == "List")
          {
            string str2;
            switch (((string) enumerator.Value).ToLower())
            {
              case "box":
                str2 = "Box";
                break;
              case "circle":
                str2 = "Circle";
                break;
              case "decimal":
                str2 = "Decimal";
                break;
              case "disc":
                str2 = "Disc";
                break;
              case "lower-latin":
                str2 = "LowerLatin";
                break;
              case "lower-roman":
                str2 = "LowerRoman";
                break;
              case "none":
                str2 = "None";
                break;
              case "square":
                str2 = "Square";
                break;
              case "upper-latin":
                str2 = "UpperLatin";
                break;
              case "upper-roman":
                str2 = "UpperRoman";
                break;
              default:
                str2 = "Disc";
                break;
            }
            xamlElement.SetAttribute("MarkerStyle", str2);
            continue;
          }
          continue;
        case "margin-bottom":
          flag1 = true;
          bottom1 = (string) enumerator.Value;
          continue;
        case "margin-left":
          flag1 = true;
          left1 = (string) enumerator.Value;
          continue;
        case "margin-right":
          flag1 = true;
          right1 = (string) enumerator.Value;
          continue;
        case "margin-top":
          flag1 = true;
          top1 = (string) enumerator.Value;
          continue;
        case "padding-bottom":
          flag2 = true;
          bottom2 = (string) enumerator.Value;
          continue;
        case "padding-left":
          flag2 = true;
          left2 = (string) enumerator.Value;
          continue;
        case "padding-right":
          flag2 = true;
          right2 = (string) enumerator.Value;
          continue;
        case "padding-top":
          flag2 = true;
          top2 = (string) enumerator.Value;
          continue;
        case "text-align":
          if (isBlock)
          {
            xamlElement.SetAttribute("TextAlignment", (string) enumerator.Value);
            continue;
          }
          continue;
        case "text-decoration-blink":
        case "text-decoration-line-through":
        case "text-decoration-none":
        case "text-decoration-overline":
          if (isBlock)
            continue;
          continue;
        case "text-decoration-underline":
          if (!isBlock && (string) enumerator.Value == "true")
          {
            xamlElement.SetAttribute("TextDecorations", "Underline");
            continue;
          }
          continue;
        case "text-indent":
          if (isBlock)
          {
            xamlElement.SetAttribute("TextIndent", (string) enumerator.Value);
            continue;
          }
          continue;
        default:
          continue;
      }
    }
    if (!isBlock)
      return;
    if (flag1)
      HtmlToXamlConverter.ComposeThicknessProperty(xamlElement, "Margin", left1, right1, top1, bottom1);
    if (flag2)
      HtmlToXamlConverter.ComposeThicknessProperty(xamlElement, "Padding", left2, right2, top2, bottom2);
    if (str1 != null)
      xamlElement.SetAttribute("BorderBrush", str1);
    if (!flag3)
      return;
    HtmlToXamlConverter.ComposeThicknessProperty(xamlElement, "BorderThickness", left3, right3, top3, bottom3);
  }

  private static void ComposeThicknessProperty(
    XmlElement xamlElement,
    string propertyName,
    string left,
    string right,
    string top,
    string bottom)
  {
    if (left[0] == '0' || left[0] == '-')
      left = "0";
    if (right[0] == '0' || right[0] == '-')
      right = "0";
    if (top[0] == '0' || top[0] == '-')
      top = "0";
    if (bottom[0] == '0' || bottom[0] == '-')
      bottom = "0";
    string str;
    if (left == right && top == bottom)
      str = !(left == top) ? $"{left},{top}" : left;
    else
      str = $"{left},{top},{right},{bottom}";
    xamlElement.SetAttribute(propertyName, str);
  }

  private static void SetPropertyValue(
    XmlElement xamlElement,
    DependencyProperty property,
    string stringValue)
  {
    TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
    try
    {
      if (converter.ConvertFromInvariantString(stringValue) == null)
        return;
      xamlElement.SetAttribute(property.Name, stringValue);
    }
    catch (Exception ex)
    {
    }
  }

  private static Hashtable GetElementProperties(
    XmlElement htmlElement,
    Hashtable inheritedProperties,
    out Hashtable localProperties,
    CssStylesheet stylesheet,
    List<XmlElement> sourceContext)
  {
    Hashtable elementProperties = new Hashtable();
    IDictionaryEnumerator enumerator1 = inheritedProperties.GetEnumerator();
    while (enumerator1.MoveNext())
      elementProperties[enumerator1.Key] = enumerator1.Value;
    string lower = htmlElement.LocalName.ToLower();
    string namespaceUri = htmlElement.NamespaceURI;
    localProperties = new Hashtable();
    switch (lower)
    {
      case "b":
      case "bold":
      case "dfn":
      case "strong":
        localProperties[(object) "font-weight"] = (object) "bold";
        break;
      case "blockquote":
        localProperties[(object) "margin-left"] = (object) "16";
        break;
      case "em":
      case "i":
      case "italic":
        localProperties[(object) "font-style"] = (object) "italic";
        break;
      case "font":
        string attribute1 = HtmlToXamlConverter.GetAttribute(htmlElement, "face");
        if (attribute1 != null)
          localProperties[(object) "font-family"] = (object) attribute1;
        string attribute2 = HtmlToXamlConverter.GetAttribute(htmlElement, "size");
        if (attribute2 != null)
        {
          double num = double.Parse(attribute2) * 4.0;
          if (num < 1.0)
            num = 1.0;
          else if (num > 1000.0)
            num = 1000.0;
          localProperties[(object) "font-size"] = (object) num.ToString();
        }
        string attribute3 = HtmlToXamlConverter.GetAttribute(htmlElement, "color");
        if (attribute3 != null)
        {
          localProperties[(object) "color"] = (object) attribute3;
          break;
        }
        break;
      case "h1":
        localProperties[(object) "font-size"] = (object) "22pt";
        break;
      case "h2":
        localProperties[(object) "font-size"] = (object) "20pt";
        break;
      case "h3":
        localProperties[(object) "font-size"] = (object) "18pt";
        break;
      case "h4":
        localProperties[(object) "font-size"] = (object) "16pt";
        break;
      case "h5":
        localProperties[(object) "font-size"] = (object) "12pt";
        break;
      case "h6":
        localProperties[(object) "font-size"] = (object) "10pt";
        break;
      case "ol":
        localProperties[(object) "list-style-type"] = (object) "decimal";
        break;
      case "pre":
        localProperties[(object) "font-family"] = (object) "Courier New";
        localProperties[(object) "font-size"] = (object) "8pt";
        localProperties[(object) "text-align"] = (object) "Left";
        break;
      case "samp":
        localProperties[(object) "font-family"] = (object) "Courier New";
        localProperties[(object) "font-size"] = (object) "8pt";
        localProperties[(object) "text-align"] = (object) "Left";
        break;
      case "u":
      case "underline":
        localProperties[(object) "text-decoration-underline"] = (object) "true";
        break;
      case "ul":
        localProperties[(object) "list-style-type"] = (object) "disc";
        break;
    }
    HtmlCssParser.GetElementPropertiesFromCssAttributes(htmlElement, lower, stylesheet, localProperties, sourceContext);
    IDictionaryEnumerator enumerator2 = localProperties.GetEnumerator();
    while (enumerator2.MoveNext())
      elementProperties[enumerator2.Key] = enumerator2.Value;
    return elementProperties;
  }

  private static string GetCssAttribute(string cssStyle, string attributeName)
  {
    if (cssStyle != null)
    {
      attributeName = attributeName.ToLower();
      string str1 = cssStyle;
      char[] chArray1 = new char[1]{ ';' };
      foreach (string str2 in str1.Split(chArray1))
      {
        char[] chArray2 = new char[1]{ ':' };
        string[] strArray = str2.Split(chArray2);
        if (strArray.Length == 2 && strArray[0].Trim().ToLower() == attributeName)
          return strArray[1].Trim();
      }
    }
    return (string) null;
  }

  private static bool TryGetLengthValue(string lengthAsString, out double length)
  {
    length = double.NaN;
    if (lengthAsString != null)
    {
      lengthAsString = lengthAsString.Trim().ToLower();
      if (lengthAsString.EndsWith("pt"))
      {
        lengthAsString = lengthAsString.Substring(0, lengthAsString.Length - 2);
        length = !double.TryParse(lengthAsString, out length) ? double.NaN : length * 96.0 / 72.0;
      }
      else if (lengthAsString.EndsWith("px"))
      {
        lengthAsString = lengthAsString.Substring(0, lengthAsString.Length - 2);
        if (!double.TryParse(lengthAsString, out length))
          length = double.NaN;
      }
      else if (!double.TryParse(lengthAsString, out length))
        length = double.NaN;
    }
    return !double.IsNaN(length);
  }

  private static string GetColorValue(string colorValue) => colorValue;

  private static void ApplyPropertiesToTableCellElement(
    XmlElement htmlChildNode,
    XmlElement xamlTableCellElement)
  {
    xamlTableCellElement.SetAttribute("BorderThickness", "1,1,1,1");
    xamlTableCellElement.SetAttribute("BorderBrush", "Black");
    string attribute = HtmlToXamlConverter.GetAttribute(htmlChildNode, "rowspan");
    if (attribute == null)
      return;
    xamlTableCellElement.SetAttribute("RowSpan", attribute);
  }
}
