// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Helpers.HtmlCssParser
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using System.Collections;
using System.Collections.Generic;
using System.Xml;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Helpers;

internal static class HtmlCssParser
{
  private static readonly string[] _colors = new string[140]
  {
    "aliceblue",
    "antiquewhite",
    "aqua",
    "aquamarine",
    "azure",
    "beige",
    "bisque",
    "black",
    "blanchedalmond",
    "blue",
    "blueviolet",
    "brown",
    "burlywood",
    "cadetblue",
    "chartreuse",
    "chocolate",
    "coral",
    "cornflowerblue",
    "cornsilk",
    "crimson",
    "cyan",
    "darkblue",
    "darkcyan",
    "darkgoldenrod",
    "darkgray",
    "darkgreen",
    "darkkhaki",
    "darkmagenta",
    "darkolivegreen",
    "darkorange",
    "darkorchid",
    "darkred",
    "darksalmon",
    "darkseagreen",
    "darkslateblue",
    "darkslategray",
    "darkturquoise",
    "darkviolet",
    "deeppink",
    "deepskyblue",
    "dimgray",
    "dodgerblue",
    "firebrick",
    "floralwhite",
    "forestgreen",
    "fuchsia",
    "gainsboro",
    "ghostwhite",
    "gold",
    "goldenrod",
    "gray",
    "green",
    "greenyellow",
    "honeydew",
    "hotpink",
    "indianred",
    "indigo",
    "ivory",
    "khaki",
    "lavender",
    "lavenderblush",
    "lawngreen",
    "lemonchiffon",
    "lightblue",
    "lightcoral",
    "lightcyan",
    "lightgoldenrodyellow",
    "lightgreen",
    "lightgrey",
    "lightpink",
    "lightsalmon",
    "lightseagreen",
    "lightskyblue",
    "lightslategray",
    "lightsteelblue",
    "lightyellow",
    "lime",
    "limegreen",
    "linen",
    "magenta",
    "maroon",
    "mediumaquamarine",
    "mediumblue",
    "mediumorchid",
    "mediumpurple",
    "mediumseagreen",
    "mediumslateblue",
    "mediumspringgreen",
    "mediumturquoise",
    "mediumvioletred",
    "midnightblue",
    "mintcream",
    "mistyrose",
    "moccasin",
    "navajowhite",
    "navy",
    "oldlace",
    "olive",
    "olivedrab",
    "orange",
    "orangered",
    "orchid",
    "palegoldenrod",
    "palegreen",
    "paleturquoise",
    "palevioletred",
    "papayawhip",
    "peachpuff",
    "peru",
    "pink",
    "plum",
    "powderblue",
    "purple",
    "red",
    "rosybrown",
    "royalblue",
    "saddlebrown",
    "salmon",
    "sandybrown",
    "seagreen",
    "seashell",
    "sienna",
    "silver",
    "skyblue",
    "slateblue",
    "slategray",
    "snow",
    "springgreen",
    "steelblue",
    "tan",
    "teal",
    "thistle",
    "tomato",
    "turquoise",
    "violet",
    "wheat",
    "white",
    "whitesmoke",
    "yellow",
    "yellowgreen"
  };
  private static readonly string[] _systemColors = new string[28]
  {
    "activeborder",
    "activecaption",
    "appworkspace",
    "background",
    "buttonface",
    "buttonhighlight",
    "buttonshadow",
    "buttontext",
    "captiontext",
    "graytext",
    "highlight",
    "highlighttext",
    "inactiveborder",
    "inactivecaption",
    "inactivecaptiontext",
    "infobackground",
    "infotext",
    "menu",
    "menutext",
    "scrollbar",
    "threeddarkshadow",
    "threedface",
    "threedhighlight",
    "threedlightshadow",
    "threedshadow",
    "window",
    "windowframe",
    "windowtext"
  };
  private static readonly string[] _fontGenericFamilies = new string[5]
  {
    "serif",
    "sans-serif",
    "monospace",
    "cursive",
    "fantasy"
  };
  private static readonly string[] _fontStyles = new string[3]
  {
    "normal",
    "italic",
    "oblique"
  };
  private static readonly string[] _fontVariants = new string[2]
  {
    "normal",
    "small-caps"
  };
  private static readonly string[] _fontWeights = new string[13]
  {
    "normal",
    "bold",
    "bolder",
    "lighter",
    "100",
    "200",
    "300",
    "400",
    "500",
    "600",
    "700",
    "800",
    "900"
  };
  private static readonly string[] _fontAbsoluteSizes = new string[7]
  {
    "xx-small",
    "x-small",
    "small",
    "medium",
    "large",
    "x-large",
    "xx-large"
  };
  private static readonly string[] _fontRelativeSizes = new string[2]
  {
    "larger",
    "smaller"
  };
  private static readonly string[] _fontSizeUnits = new string[9]
  {
    "px",
    "mm",
    "cm",
    "in",
    "pt",
    "pc",
    "em",
    "ex",
    "%"
  };
  private static readonly string[] _listStyleTypes = new string[9]
  {
    "disc",
    "circle",
    "square",
    "decimal",
    "lower-roman",
    "upper-roman",
    "lower-alpha",
    "upper-alpha",
    "none"
  };
  private static readonly string[] _listStylePositions = new string[2]
  {
    "inside",
    "outside"
  };
  private static readonly string[] _textDecorations = new string[5]
  {
    "none",
    "underline",
    "overline",
    "line-through",
    "blink"
  };
  private static readonly string[] _textTransforms = new string[4]
  {
    "none",
    "capitalize",
    "uppercase",
    "lowercase"
  };
  private static readonly string[] _textAligns = new string[4]
  {
    "left",
    "right",
    "center",
    "justify"
  };
  private static readonly string[] _verticalAligns = new string[8]
  {
    "baseline",
    "sub",
    "super",
    "top",
    "text-top",
    "middle",
    "bottom",
    "text-bottom"
  };
  private static readonly string[] _floats = new string[3]
  {
    "left",
    "right",
    "none"
  };
  private static readonly string[] _clears = new string[4]
  {
    "none",
    "left",
    "right",
    "both"
  };
  private static readonly string[] _borderStyles = new string[9]
  {
    "none",
    "dotted",
    "dashed",
    "solid",
    "double",
    "groove",
    "ridge",
    "inset",
    "outset"
  };
  private static string[] _blocks = new string[4]
  {
    "block",
    "inline",
    "list-item",
    "none"
  };

  internal static void GetElementPropertiesFromCssAttributes(
    XmlElement htmlElement,
    string elementName,
    CssStylesheet stylesheet,
    Hashtable localProperties,
    List<XmlElement> sourceContext)
  {
    string style = stylesheet.GetStyle(elementName, sourceContext);
    string attribute = HtmlToXamlConverter.GetAttribute(htmlElement, "style");
    string str1 = style ?? (string) null;
    if (attribute != null)
      str1 = str1 == null ? attribute : $"{str1};{attribute}";
    if (str1 == null)
      return;
    string str2 = str1;
    char[] chArray1 = new char[1]{ ';' };
    foreach (string str3 in str2.Split(chArray1))
    {
      char[] chArray2 = new char[1]{ ':' };
      string[] strArray = str3.Split(chArray2);
      if (strArray.Length == 2)
      {
        string lower1 = strArray[0].Trim().ToLower();
        string lower2 = HtmlToXamlConverter.UnQuote(strArray[1].Trim()).ToLower();
        int nextIndex = 0;
        switch (lower1)
        {
          case "background":
            HtmlCssParser.ParseCssBackground(lower2, ref nextIndex, localProperties);
            continue;
          case "background-color":
            HtmlCssParser.ParseCssColor(lower2, ref nextIndex, localProperties, "background-color");
            continue;
          case "border":
            HtmlCssParser.ParseCssBorder(lower2, ref nextIndex, localProperties);
            continue;
          case "border-color":
          case "border-style":
          case "border-width":
            HtmlCssParser.ParseCssRectangleProperty(lower2, ref nextIndex, localProperties, lower1);
            continue;
          case "clear":
            HtmlCssParser.ParseCssClear(lower2, ref nextIndex, localProperties);
            continue;
          case "color":
            HtmlCssParser.ParseCssColor(lower2, ref nextIndex, localProperties, "color");
            continue;
          case "float":
            HtmlCssParser.ParseCssFloat(lower2, ref nextIndex, localProperties);
            continue;
          case "font":
            HtmlCssParser.ParseCssFont(lower2, localProperties);
            continue;
          case "font-family":
            HtmlCssParser.ParseCssFontFamily(lower2, ref nextIndex, localProperties);
            continue;
          case "font-size":
            HtmlCssParser.ParseCssSize(lower2, ref nextIndex, localProperties, "font-size", true);
            continue;
          case "font-style":
            HtmlCssParser.ParseCssFontStyle(lower2, ref nextIndex, localProperties);
            continue;
          case "font-variant":
            HtmlCssParser.ParseCssFontVariant(lower2, ref nextIndex, localProperties);
            continue;
          case "font-weight":
            HtmlCssParser.ParseCssFontWeight(lower2, ref nextIndex, localProperties);
            continue;
          case "height":
          case "width":
            HtmlCssParser.ParseCssSize(lower2, ref nextIndex, localProperties, lower1, true);
            continue;
          case "line-height":
            HtmlCssParser.ParseCssSize(lower2, ref nextIndex, localProperties, "line-height", true);
            continue;
          case "margin":
            HtmlCssParser.ParseCssRectangleProperty(lower2, ref nextIndex, localProperties, lower1);
            continue;
          case "margin-bottom":
          case "margin-left":
          case "margin-right":
          case "margin-top":
            HtmlCssParser.ParseCssSize(lower2, ref nextIndex, localProperties, lower1, true);
            continue;
          case "padding":
            HtmlCssParser.ParseCssRectangleProperty(lower2, ref nextIndex, localProperties, lower1);
            continue;
          case "padding-bottom":
          case "padding-left":
          case "padding-right":
          case "padding-top":
            HtmlCssParser.ParseCssSize(lower2, ref nextIndex, localProperties, lower1, true);
            continue;
          case "text-align":
            HtmlCssParser.ParseCssTextAlign(lower2, ref nextIndex, localProperties);
            continue;
          case "text-decoration":
            HtmlCssParser.ParseCssTextDecoration(lower2, ref nextIndex, localProperties);
            continue;
          case "text-indent":
            HtmlCssParser.ParseCssSize(lower2, ref nextIndex, localProperties, "text-indent", false);
            continue;
          case "text-transform":
            HtmlCssParser.ParseCssTextTransform(lower2, ref nextIndex, localProperties);
            continue;
          case "vertical-align":
            HtmlCssParser.ParseCssVerticalAlign(lower2, ref nextIndex, localProperties);
            continue;
          default:
            continue;
        }
      }
    }
  }

  private static void ParseWhiteSpace(string styleValue, ref int nextIndex)
  {
    while (nextIndex < styleValue.Length && char.IsWhiteSpace(styleValue[nextIndex]))
      ++nextIndex;
  }

  private static bool ParseWord(string word, string styleValue, ref int nextIndex)
  {
    HtmlCssParser.ParseWhiteSpace(styleValue, ref nextIndex);
    for (int index = 0; index < word.Length; ++index)
    {
      if (nextIndex + index >= styleValue.Length || (int) word[index] != (int) styleValue[nextIndex + index])
        return false;
    }
    if (nextIndex + word.Length < styleValue.Length && char.IsLetterOrDigit(styleValue[nextIndex + word.Length]))
      return false;
    nextIndex += word.Length;
    return true;
  }

  private static string ParseWordEnumeration(string[] words, string styleValue, ref int nextIndex)
  {
    for (int index = 0; index < words.Length; ++index)
    {
      if (HtmlCssParser.ParseWord(words[index], styleValue, ref nextIndex))
        return words[index];
    }
    return (string) null;
  }

  private static void ParseWordEnumeration(
    string[] words,
    string styleValue,
    ref int nextIndex,
    Hashtable localProperties,
    string attributeName)
  {
    string wordEnumeration = HtmlCssParser.ParseWordEnumeration(words, styleValue, ref nextIndex);
    if (wordEnumeration == null)
      return;
    localProperties[(object) attributeName] = (object) wordEnumeration;
  }

  private static string ParseCssSize(string styleValue, ref int nextIndex, bool mustBeNonNegative)
  {
    HtmlCssParser.ParseWhiteSpace(styleValue, ref nextIndex);
    int num = nextIndex;
    if (nextIndex < styleValue.Length && styleValue[nextIndex] == '-')
      ++nextIndex;
    if (nextIndex >= styleValue.Length || !char.IsDigit(styleValue[nextIndex]))
      return (string) null;
    while (nextIndex < styleValue.Length && (char.IsDigit(styleValue[nextIndex]) || styleValue[nextIndex] == '.'))
      ++nextIndex;
    string str1 = styleValue.Substring(num, nextIndex - num);
    string str2 = HtmlCssParser.ParseWordEnumeration(HtmlCssParser._fontSizeUnits, styleValue, ref nextIndex) ?? "px";
    return mustBeNonNegative && styleValue[num] == '-' ? "0" : str1 + str2;
  }

  private static void ParseCssSize(
    string styleValue,
    ref int nextIndex,
    Hashtable localValues,
    string propertyName,
    bool mustBeNonNegative)
  {
    string cssSize = HtmlCssParser.ParseCssSize(styleValue, ref nextIndex, mustBeNonNegative);
    if (cssSize == null)
      return;
    localValues[(object) propertyName] = (object) cssSize;
  }

  private static string ParseCssColor(string styleValue, ref int nextIndex)
  {
    HtmlCssParser.ParseWhiteSpace(styleValue, ref nextIndex);
    string cssColor = (string) null;
    if (nextIndex < styleValue.Length)
    {
      int startIndex = nextIndex;
      char c = styleValue[nextIndex];
      if (c == '#')
      {
        ++nextIndex;
        while (nextIndex < styleValue.Length)
        {
          char upper = char.ToUpper(styleValue[nextIndex]);
          if ('0' <= upper && upper <= '9' || 'A' <= upper && upper <= 'F')
            ++nextIndex;
          else
            break;
        }
        if (nextIndex > startIndex + 1)
          cssColor = styleValue.Substring(startIndex, nextIndex - startIndex);
      }
      else if (styleValue.Substring(nextIndex, 3).ToLower() == "rbg")
      {
        while (nextIndex < styleValue.Length && styleValue[nextIndex] != ')')
          ++nextIndex;
        if (nextIndex < styleValue.Length)
          ++nextIndex;
        cssColor = "gray";
      }
      else if (char.IsLetter(c))
      {
        cssColor = HtmlCssParser.ParseWordEnumeration(HtmlCssParser._colors, styleValue, ref nextIndex);
        if (cssColor == null)
        {
          cssColor = HtmlCssParser.ParseWordEnumeration(HtmlCssParser._systemColors, styleValue, ref nextIndex);
          if (cssColor != null)
            cssColor = "black";
        }
      }
    }
    return cssColor;
  }

  private static void ParseCssColor(
    string styleValue,
    ref int nextIndex,
    Hashtable localValues,
    string propertyName)
  {
    string cssColor = HtmlCssParser.ParseCssColor(styleValue, ref nextIndex);
    if (cssColor == null)
      return;
    localValues[(object) propertyName] = (object) cssColor;
  }

  private static void ParseCssFont(string styleValue, Hashtable localProperties)
  {
    int nextIndex = 0;
    HtmlCssParser.ParseCssFontStyle(styleValue, ref nextIndex, localProperties);
    HtmlCssParser.ParseCssFontVariant(styleValue, ref nextIndex, localProperties);
    HtmlCssParser.ParseCssFontWeight(styleValue, ref nextIndex, localProperties);
    HtmlCssParser.ParseCssSize(styleValue, ref nextIndex, localProperties, "font-size", true);
    HtmlCssParser.ParseWhiteSpace(styleValue, ref nextIndex);
    if (nextIndex < styleValue.Length && styleValue[nextIndex] == '/')
    {
      ++nextIndex;
      HtmlCssParser.ParseCssSize(styleValue, ref nextIndex, localProperties, "line-height", true);
    }
    HtmlCssParser.ParseCssFontFamily(styleValue, ref nextIndex, localProperties);
  }

  private static void ParseCssFontStyle(
    string styleValue,
    ref int nextIndex,
    Hashtable localProperties)
  {
    HtmlCssParser.ParseWordEnumeration(HtmlCssParser._fontStyles, styleValue, ref nextIndex, localProperties, "font-style");
  }

  private static void ParseCssFontVariant(
    string styleValue,
    ref int nextIndex,
    Hashtable localProperties)
  {
    HtmlCssParser.ParseWordEnumeration(HtmlCssParser._fontVariants, styleValue, ref nextIndex, localProperties, "font-variant");
  }

  private static void ParseCssFontWeight(
    string styleValue,
    ref int nextIndex,
    Hashtable localProperties)
  {
    HtmlCssParser.ParseWordEnumeration(HtmlCssParser._fontWeights, styleValue, ref nextIndex, localProperties, "font-weight");
  }

  private static void ParseCssFontFamily(
    string styleValue,
    ref int nextIndex,
    Hashtable localProperties)
  {
    string str1 = (string) null;
    while (nextIndex < styleValue.Length)
    {
      string str2 = HtmlCssParser.ParseWordEnumeration(HtmlCssParser._fontGenericFamilies, styleValue, ref nextIndex);
      if (str2 == null)
      {
        if (nextIndex < styleValue.Length && (styleValue[nextIndex] == '"' || styleValue[nextIndex] == '\''))
        {
          char ch = styleValue[nextIndex];
          ++nextIndex;
          int startIndex = nextIndex;
          while (nextIndex < styleValue.Length && (int) styleValue[nextIndex] != (int) ch)
            ++nextIndex;
          str2 = $"\"{styleValue.Substring(startIndex, nextIndex - startIndex)}\"";
        }
        if (str2 == null)
        {
          int startIndex = nextIndex;
          while (nextIndex < styleValue.Length && styleValue[nextIndex] != ',' && styleValue[nextIndex] != ';')
            ++nextIndex;
          if (nextIndex > startIndex)
          {
            str2 = styleValue.Substring(startIndex, nextIndex - startIndex).Trim();
            if (str2.Length == 0)
              str2 = (string) null;
          }
        }
      }
      HtmlCssParser.ParseWhiteSpace(styleValue, ref nextIndex);
      if (nextIndex < styleValue.Length && styleValue[nextIndex] == ',')
        ++nextIndex;
      if (str2 != null)
      {
        if (str1 == null && str2.Length > 0)
        {
          if (str2[0] == '"' || str2[0] == '\'')
            str2 = str2.Substring(1, str2.Length - 2);
          str1 = str2;
        }
      }
      else
        break;
    }
    if (str1 == null)
      return;
    localProperties[(object) "font-family"] = (object) str1;
  }

  private static void ParseCssListStyle(string styleValue, Hashtable localProperties)
  {
    int nextIndex = 0;
    while (nextIndex < styleValue.Length)
    {
      string cssListStyleType = HtmlCssParser.ParseCssListStyleType(styleValue, ref nextIndex);
      if (cssListStyleType != null)
      {
        localProperties[(object) "list-style-type"] = (object) cssListStyleType;
      }
      else
      {
        string listStylePosition = HtmlCssParser.ParseCssListStylePosition(styleValue, ref nextIndex);
        if (listStylePosition != null)
        {
          localProperties[(object) "list-style-position"] = (object) listStylePosition;
        }
        else
        {
          string cssListStyleImage = HtmlCssParser.ParseCssListStyleImage(styleValue, ref nextIndex);
          if (cssListStyleImage == null)
            break;
          localProperties[(object) "list-style-image"] = (object) cssListStyleImage;
        }
      }
    }
  }

  private static string ParseCssListStyleType(string styleValue, ref int nextIndex)
  {
    return HtmlCssParser.ParseWordEnumeration(HtmlCssParser._listStyleTypes, styleValue, ref nextIndex);
  }

  private static string ParseCssListStylePosition(string styleValue, ref int nextIndex)
  {
    return HtmlCssParser.ParseWordEnumeration(HtmlCssParser._listStylePositions, styleValue, ref nextIndex);
  }

  private static string ParseCssListStyleImage(string styleValue, ref int nextIndex)
  {
    return (string) null;
  }

  private static void ParseCssTextDecoration(
    string styleValue,
    ref int nextIndex,
    Hashtable localProperties)
  {
    for (int index = 1; index < HtmlCssParser._textDecorations.Length; ++index)
      localProperties[(object) ("text-decoration-" + HtmlCssParser._textDecorations[index])] = (object) "false";
    while (nextIndex < styleValue.Length)
    {
      string wordEnumeration = HtmlCssParser.ParseWordEnumeration(HtmlCssParser._textDecorations, styleValue, ref nextIndex);
      switch (wordEnumeration)
      {
        case null:
          return;
        case "none":
          return;
        default:
          localProperties[(object) ("text-decoration-" + wordEnumeration)] = (object) "true";
          continue;
      }
    }
  }

  private static void ParseCssTextTransform(
    string styleValue,
    ref int nextIndex,
    Hashtable localProperties)
  {
    HtmlCssParser.ParseWordEnumeration(HtmlCssParser._textTransforms, styleValue, ref nextIndex, localProperties, "text-transform");
  }

  private static void ParseCssTextAlign(
    string styleValue,
    ref int nextIndex,
    Hashtable localProperties)
  {
    HtmlCssParser.ParseWordEnumeration(HtmlCssParser._textAligns, styleValue, ref nextIndex, localProperties, "text-align");
  }

  private static void ParseCssVerticalAlign(
    string styleValue,
    ref int nextIndex,
    Hashtable localProperties)
  {
    HtmlCssParser.ParseWordEnumeration(HtmlCssParser._verticalAligns, styleValue, ref nextIndex, localProperties, "vertical-align");
  }

  private static void ParseCssFloat(
    string styleValue,
    ref int nextIndex,
    Hashtable localProperties)
  {
    HtmlCssParser.ParseWordEnumeration(HtmlCssParser._floats, styleValue, ref nextIndex, localProperties, "float");
  }

  private static void ParseCssClear(
    string styleValue,
    ref int nextIndex,
    Hashtable localProperties)
  {
    HtmlCssParser.ParseWordEnumeration(HtmlCssParser._clears, styleValue, ref nextIndex, localProperties, "clear");
  }

  private static bool ParseCssRectangleProperty(
    string styleValue,
    ref int nextIndex,
    Hashtable localProperties,
    string propertyName)
  {
    string str1;
    switch (propertyName)
    {
      case "border-color":
        str1 = HtmlCssParser.ParseCssColor(styleValue, ref nextIndex);
        break;
      case "border-style":
        str1 = HtmlCssParser.ParseCssBorderStyle(styleValue, ref nextIndex);
        break;
      default:
        str1 = HtmlCssParser.ParseCssSize(styleValue, ref nextIndex, true);
        break;
    }
    string str2 = str1;
    if (str2 == null)
      return false;
    localProperties[(object) (propertyName + "-top")] = (object) str2;
    localProperties[(object) (propertyName + "-bottom")] = (object) str2;
    localProperties[(object) (propertyName + "-right")] = (object) str2;
    localProperties[(object) (propertyName + "-left")] = (object) str2;
    string str3;
    switch (propertyName)
    {
      case "border-color":
        str3 = HtmlCssParser.ParseCssColor(styleValue, ref nextIndex);
        break;
      case "border-style":
        str3 = HtmlCssParser.ParseCssBorderStyle(styleValue, ref nextIndex);
        break;
      default:
        str3 = HtmlCssParser.ParseCssSize(styleValue, ref nextIndex, true);
        break;
    }
    string str4 = str3;
    if (str4 != null)
    {
      localProperties[(object) (propertyName + "-right")] = (object) str4;
      localProperties[(object) (propertyName + "-left")] = (object) str4;
      string str5;
      switch (propertyName)
      {
        case "border-color":
          str5 = HtmlCssParser.ParseCssColor(styleValue, ref nextIndex);
          break;
        case "border-style":
          str5 = HtmlCssParser.ParseCssBorderStyle(styleValue, ref nextIndex);
          break;
        default:
          str5 = HtmlCssParser.ParseCssSize(styleValue, ref nextIndex, true);
          break;
      }
      string str6 = str5;
      if (str6 != null)
      {
        localProperties[(object) (propertyName + "-bottom")] = (object) str6;
        string str7;
        switch (propertyName)
        {
          case "border-color":
            str7 = HtmlCssParser.ParseCssColor(styleValue, ref nextIndex);
            break;
          case "border-style":
            str7 = HtmlCssParser.ParseCssBorderStyle(styleValue, ref nextIndex);
            break;
          default:
            str7 = HtmlCssParser.ParseCssSize(styleValue, ref nextIndex, true);
            break;
        }
        string str8 = str7;
        if (str8 != null)
          localProperties[(object) (propertyName + "-left")] = (object) str8;
      }
    }
    return true;
  }

  private static void ParseCssBorder(
    string styleValue,
    ref int nextIndex,
    Hashtable localProperties)
  {
    do
      ;
    while (HtmlCssParser.ParseCssRectangleProperty(styleValue, ref nextIndex, localProperties, "border-width") || HtmlCssParser.ParseCssRectangleProperty(styleValue, ref nextIndex, localProperties, "border-style") || HtmlCssParser.ParseCssRectangleProperty(styleValue, ref nextIndex, localProperties, "border-color"));
  }

  private static string ParseCssBorderStyle(string styleValue, ref int nextIndex)
  {
    return HtmlCssParser.ParseWordEnumeration(HtmlCssParser._borderStyles, styleValue, ref nextIndex);
  }

  private static void ParseCssBackground(
    string styleValue,
    ref int nextIndex,
    Hashtable localValues)
  {
  }
}
