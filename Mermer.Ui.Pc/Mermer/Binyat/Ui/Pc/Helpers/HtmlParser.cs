// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Helpers.HtmlParser
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

#nullable disable
namespace Mermer.Ui.Pc.Helpers;

internal class HtmlParser
{
  internal const string HtmlHeader = "Version:1.0\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\nStartSelection:{4:D10}\r\nEndSelection:{5:D10}\r\n";
  internal const string HtmlStartFragmentComment = "<!--StartFragment-->";
  internal const string HtmlEndFragmentComment = "<!--EndFragment-->";
  internal const string XhtmlNamespace = "http://www.w3.org/1999/xhtml";
  private HtmlLexicalAnalyzer _htmlLexicalAnalyzer;
  private XmlDocument _document;
  private Stack<XmlElement> _openedElements;
  private Stack<XmlElement> _pendingInlineElements;

  private HtmlParser(string inputString)
  {
    this._document = new XmlDocument();
    this._openedElements = new Stack<XmlElement>();
    this._pendingInlineElements = new Stack<XmlElement>();
    this._htmlLexicalAnalyzer = new HtmlLexicalAnalyzer(inputString);
    this._htmlLexicalAnalyzer.GetNextContentToken();
  }

  internal static XmlElement ParseHtml(string htmlString)
  {
    return new HtmlParser(htmlString).ParseHtmlContent();
  }

  internal static string ExtractHtmlFromClipboardData(string htmlDataString)
  {
    int num1 = htmlDataString.IndexOf("StartHTML:");
    if (num1 < 0)
      return "ERROR: Urecognized html header";
    int startIndex = int.Parse(htmlDataString.Substring(num1 + "StartHTML:".Length, "0123456789".Length));
    if (startIndex < 0 || startIndex > htmlDataString.Length)
      return "ERROR: Urecognized html header";
    int num2 = htmlDataString.IndexOf("EndHTML:");
    if (num2 < 0)
      return "ERROR: Urecognized html header";
    int length = int.Parse(htmlDataString.Substring(num2 + "EndHTML:".Length, "0123456789".Length));
    if (length > htmlDataString.Length)
      length = htmlDataString.Length;
    return htmlDataString.Substring(startIndex, length - startIndex);
  }

  internal static string AddHtmlClipboardHeader(string htmlString)
  {
    StringBuilder stringBuilder = new StringBuilder();
    int num1 = "Version:1.0\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\nStartSelection:{4:D10}\r\nEndSelection:{5:D10}\r\n".Length + 6 * ("0123456789".Length - "{0:D10}".Length);
    int num2 = num1 + htmlString.Length;
    int num3 = htmlString.IndexOf("<!--StartFragment-->", 0);
    int num4 = num3 < 0 ? num1 : num1 + num3 + "<!--StartFragment-->".Length;
    int num5 = htmlString.IndexOf("<!--EndFragment-->", 0);
    int num6 = num5 < 0 ? num2 : num1 + num5;
    stringBuilder.AppendFormat("Version:1.0\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\nStartSelection:{4:D10}\r\nEndSelection:{5:D10}\r\n", (object) num1, (object) num2, (object) num4, (object) num6, (object) num4, (object) num6);
    stringBuilder.Append(htmlString);
    return stringBuilder.ToString();
  }

  private void InvariantAssert(bool condition, string message)
  {
    if (!condition)
      throw new Exception("Assertion error: " + message);
  }

  private XmlElement ParseHtmlContent()
  {
    XmlElement htmlElement = this._document.CreateElement("html", "http://www.w3.org/1999/xhtml");
    this.OpenStructuringElement(htmlElement);
    while (this._htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.EOF)
    {
      if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.OpeningTagStart)
      {
        this._htmlLexicalAnalyzer.GetNextTagToken();
        if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
        {
          string lower = this._htmlLexicalAnalyzer.NextToken.ToLower();
          this._htmlLexicalAnalyzer.GetNextTagToken();
          XmlElement element = this._document.CreateElement(lower, "http://www.w3.org/1999/xhtml");
          this.ParseAttributes(element);
          if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.EmptyTagEnd || HtmlSchema.IsEmptyElement(lower))
            this.AddEmptyElement(element);
          else if (HtmlSchema.IsInlineElement(lower))
            this.OpenInlineElement(element);
          else if (HtmlSchema.IsBlockElement(lower) || HtmlSchema.IsKnownOpenableElement(lower))
            this.OpenStructuringElement(element);
        }
      }
      else if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.ClosingTagStart)
      {
        this._htmlLexicalAnalyzer.GetNextTagToken();
        if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
        {
          string lower = this._htmlLexicalAnalyzer.NextToken.ToLower();
          this._htmlLexicalAnalyzer.GetNextTagToken();
          this.CloseElement(lower);
        }
      }
      else if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Text)
        this.AddTextContent(this._htmlLexicalAnalyzer.NextToken);
      else if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Comment)
        this.AddComment(this._htmlLexicalAnalyzer.NextToken);
      this._htmlLexicalAnalyzer.GetNextContentToken();
    }
    if (htmlElement.FirstChild is XmlElement && htmlElement.FirstChild == htmlElement.LastChild && htmlElement.FirstChild.LocalName.ToLower() == "html")
      htmlElement = (XmlElement) htmlElement.FirstChild;
    return htmlElement;
  }

  private XmlElement CreateElementCopy(XmlElement htmlElement)
  {
    XmlElement element = this._document.CreateElement(htmlElement.LocalName, "http://www.w3.org/1999/xhtml");
    for (int i = 0; i < htmlElement.Attributes.Count; ++i)
    {
      XmlAttribute attribute = htmlElement.Attributes[i];
      element.SetAttribute(attribute.Name, attribute.Value);
    }
    return element;
  }

  private void AddEmptyElement(XmlElement htmlEmptyElement)
  {
    this.InvariantAssert(this._openedElements.Count > 0, "AddEmptyElement: Stack of opened elements cannot be empty, as we have at least one artificial root element");
    this._openedElements.Peek().AppendChild((XmlNode) htmlEmptyElement);
  }

  private void OpenInlineElement(XmlElement htmlInlineElement)
  {
    this._pendingInlineElements.Push(htmlInlineElement);
  }

  private void OpenStructuringElement(XmlElement htmlElement)
  {
    if (HtmlSchema.IsBlockElement(htmlElement.LocalName))
    {
      while (this._openedElements.Count > 0 && HtmlSchema.IsInlineElement(this._openedElements.Peek().LocalName))
      {
        XmlElement htmlElement1 = this._openedElements.Pop();
        this.InvariantAssert(this._openedElements.Count > 0, "OpenStructuringElement: stack of opened elements cannot become empty here");
        this._pendingInlineElements.Push(this.CreateElementCopy(htmlElement1));
      }
    }
    if (this._openedElements.Count > 0)
    {
      XmlElement xmlElement = this._openedElements.Peek();
      if (HtmlSchema.ClosesOnNextElementStart(xmlElement.LocalName, htmlElement.LocalName))
      {
        this._openedElements.Pop();
        xmlElement = this._openedElements.Count > 0 ? this._openedElements.Peek() : (XmlElement) null;
      }
      xmlElement?.AppendChild((XmlNode) htmlElement);
    }
    this._openedElements.Push(htmlElement);
  }

  private bool IsElementOpened(string htmlElementName)
  {
    foreach (XmlNode openedElement in this._openedElements)
    {
      if (openedElement.LocalName == htmlElementName)
        return true;
    }
    return false;
  }

  private void CloseElement(string htmlElementName)
  {
    this.InvariantAssert(this._openedElements.Count > 0, "CloseElement: Stack of opened elements cannot be empty, as we have at least one artificial root element");
    if (this._pendingInlineElements.Count > 0 && this._pendingInlineElements.Peek().LocalName == htmlElementName)
    {
      XmlElement newChild = this._pendingInlineElements.Pop();
      this.InvariantAssert(this._openedElements.Count > 0, "CloseElement: Stack of opened elements cannot be empty, as we have at least one artificial root element");
      this._openedElements.Peek().AppendChild((XmlNode) newChild);
    }
    else
    {
      if (!this.IsElementOpened(htmlElementName))
        return;
      while (this._openedElements.Count > 1)
      {
        XmlElement htmlElement = this._openedElements.Pop();
        if (htmlElement.LocalName == htmlElementName)
          break;
        if (HtmlSchema.IsInlineElement(htmlElement.LocalName))
          this._pendingInlineElements.Push(this.CreateElementCopy(htmlElement));
      }
    }
  }

  private void AddTextContent(string textContent)
  {
    this.OpenPendingInlineElements();
    this.InvariantAssert(this._openedElements.Count > 0, "AddTextContent: Stack of opened elements cannot be empty, as we have at least one artificial root element");
    this._openedElements.Peek().AppendChild((XmlNode) this._document.CreateTextNode(textContent));
  }

  private void AddComment(string comment)
  {
    this.OpenPendingInlineElements();
    this.InvariantAssert(this._openedElements.Count > 0, "AddComment: Stack of opened elements cannot be empty, as we have at least one artificial root element");
    this._openedElements.Peek().AppendChild((XmlNode) this._document.CreateComment(comment));
  }

  private void OpenPendingInlineElements()
  {
    if (this._pendingInlineElements.Count <= 0)
      return;
    XmlElement newChild = this._pendingInlineElements.Pop();
    this.OpenPendingInlineElements();
    this.InvariantAssert(this._openedElements.Count > 0, "OpenPendingInlineElements: Stack of opened elements cannot be empty, as we have at least one artificial root element");
    this._openedElements.Peek().AppendChild((XmlNode) newChild);
    this._openedElements.Push(newChild);
  }

  private void ParseAttributes(XmlElement xmlElement)
  {
    while (this._htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.EOF && this._htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.TagEnd && this._htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.EmptyTagEnd)
    {
      if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
      {
        string nextToken1 = this._htmlLexicalAnalyzer.NextToken;
        this._htmlLexicalAnalyzer.GetNextEqualSignToken();
        this._htmlLexicalAnalyzer.GetNextAtomToken();
        string nextToken2 = this._htmlLexicalAnalyzer.NextToken;
        xmlElement.SetAttribute(nextToken1, nextToken2);
      }
      this._htmlLexicalAnalyzer.GetNextTagToken();
    }
  }
}
