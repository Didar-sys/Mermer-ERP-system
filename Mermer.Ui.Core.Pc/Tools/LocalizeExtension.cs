// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.Pc.Tools.LocalizeExtension
// Assembly: Mermer.Ui.Core.Pc, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99463FBB-953B-46DD-9DD6-5278306A8C84
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.Pc.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using MvvmCross.Localization;
using MvvmCross.Platform;
using Mermer.Mvvm.ViewModels;

// Атрибут має бути ТУТ, одразу після всіх using
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Mermer.Ui.Core.Pc.Tools")]

namespace Mermer.Ui.Core.Pc.Tools
{
    [ContentProperty("Text")]
    [MarkupExtensionReturnType(typeof(string))]
    public class LocalizeExtension : UpdatableMarkupExtension
    {
        private readonly IMvxTextProvider _textProvicer;
        private string _fallBackText;

        public LocalizeExtension()
        {
            try
            {
                _textProvicer = Mvx.Resolve<IMvxTextProvider>();
            }
            catch
            {
                _textProvicer = null;
            }
        }

        public LocalizeExtension(string text) : this()
        {
            Text = text;
        }

        public LocalizeExtension(string text, string fall) : this(text)
        {
            FallBackText = fall;
        }

        public LocalizeExtension(string text, string fall, string params1) : this(text, fall)
        {
            Params1 = params1;
        }

        public LocalizeExtension(string text, string fall, string params1, string params2) : this(text, fall, params1)
        {
            Params2 = params2;
        }

        public LocalizeExtension(string text, string fall, string params1, string params2, string params3) : this(text, fall, params1, params2)
        {
            Params3 = params3;
        }

        [ConstructorArgument("text")]
        public string Text { get; set; }

        [ConstructorArgument("fall")]
        public string FallBackText
        {
            get
            {
                if (!string.IsNullOrEmpty(_fallBackText))
                    return string.Format(_fallBackText, GetParams());
                return !string.IsNullOrEmpty(Text) ? "#" + Text : string.Empty;
            }
            set => _fallBackText = value;
        }

        [ConstructorArgument("params1")]
        public string Params1 { get; set; }

        [ConstructorArgument("params2")]
        public string Params2 { get; set; }

        [ConstructorArgument("params3")]
        public string Params3 { get; set; }

        private object[] GetParams()
        {
            return new[] { Params1, Params2, Params3 }
                .Where(x => !string.IsNullOrEmpty(x))
                .Cast<object>()
                .ToArray();
        }

        private string GetDefaultText()
        {
            string text = _textProvicer?.GetText(null, null, Text, GetParams());
            return !string.IsNullOrEmpty(text) ? text : FallBackText;
        }

        private string GetText()
        {
            if (!(TargetObject is FrameworkElement targetObject) || !(targetObject.DataContext is BaseViewModel dataContext))
                return GetDefaultText();

            return dataContext.TextSource.GetText(Text);
        }

        protected override object ProvideValueInternal(IServiceProvider serviceProvider)
        {
            return GetText();
        }

        protected override void TargetObjectDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateValue(GetText());
        }
    }
}