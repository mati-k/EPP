using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Services;
using System;
using System.Collections.Generic;

namespace EPP.Views
{
    public class RichTextBlock : TextBlock
    {
        public string RichText
        {
            get => GetValue(RichTextProperty);
            set => SetValue(RichTextProperty, value);
        }

        public static readonly StyledProperty<string> RichTextProperty = AvaloniaProperty.Register<RichTextBlock, string>(nameof(RichText), defaultBindingMode: BindingMode.OneWay);

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (string.IsNullOrEmpty(RichText) || Inlines == null)
            {
                return;
            }

            Inlines.Clear();

            var root = new Span();
            FormatText(root);
            Inlines.Add(root);
        }

        protected void FormatText(Span root)
        {
            var fontService = Ioc.Default.GetService<IFontService>()!;

            string text = RichText.Replace("\\n", Environment.NewLine);
            string[] formats = text.Split('§');

            Stack<char> formatCharacters = new Stack<char>();

            for (int i = 0; i < formats.Length; i++)
            {
                if (formats[i].Length == 0)
                    continue;

                Span span = new Span();
                root.Inlines.Add(span);

                if (i == 0)
                {
                    span.Inlines.Add(formats[i]);
                    continue;
                }

                if (formats[i][0] == '!' && formatCharacters.Count > 0)
                {
                    formatCharacters.Pop();
                }

                else
                {
                    formatCharacters.Push(formats[i][0]);
                }

                span.Inlines.Add(formats[i].Substring(1));

                //if (formatCharacters.Count == 0 || fontService.GetColorForKey(formatCharacters.Peek()) == null)
                //{
                //    span.Foreground = Brushes.White;
                //}

                if (formatCharacters.Count > 0 && fontService.GetColorForKey(formatCharacters.Peek()) != null)
                {
                    span.Foreground = fontService.GetColorForKey(formatCharacters.Peek());
                }
            }
        }
    }
}
