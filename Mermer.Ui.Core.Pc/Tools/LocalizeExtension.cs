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
using Mermer.Mvvm.Tools;

// Атрибут має бути ТУТ, одразу після всіх using
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Mermer.Ui.Core.Pc.Tools")]

namespace Mermer.Ui.Core.Pc.Tools
{
    [ContentProperty("Text")]
    [MarkupExtensionReturnType(typeof(string))]
    public class LocalizeExtension : UpdatableMarkupExtension
    {
        // ВИДАЛЕНО: private readonly IMvxTextProvider _textProvicer;
        private string _fallBackText;

        public LocalizeExtension()
        {
            // ВИДАЛЕНО: весь блок try-catch з Mvx.Resolve, він нам більше не потрібен!
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

        // ЄДИНИЙ МЕТОД, ЯКИЙ НАМ ТЕПЕР ПОТРІБЕН ДЛЯ ОТРИМАННЯ ТЕКСТУ
        private string GetText()
        {
            if (string.IsNullOrEmpty(Text)) return string.Empty;

            // ЗВЕРТАЄМОСЯ НАПРЯМУ ДО ТВОГО МЕНЕДЖЕРА!
            // Якщо ключ не знайдено, твій менеджер повертає сам ключ (наприклад, "common.save").
            string text = LocalizationManager.Instance.Get(Text, GetParams());

            // Якщо повернувся сам ключ (перекладу немає) і в нас є FallBackText, використовуємо FallBack
            if (text == Text && !string.IsNullOrEmpty(_fallBackText))
            {
                return string.Format(_fallBackText, GetParams());
            }

            return text;
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