using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Windows;
using System.Windows.Data;

namespace XpathViewer.Controls
{
    internal class TextEditor : ICSharpCode.AvalonEdit.TextEditor
    {

        private bool _isTextChanging;

        private FoldingManager foldingManager;
        private XmlFoldingStrategy foldingStrategy;

        public static readonly DependencyProperty TextBindingProperty = DependencyProperty.Register("TextBinding", typeof(string), typeof(TextEditor), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TextBindingPropertyChanged, null, false, UpdateSourceTrigger.PropertyChanged));

        public TextEditor()
        {
            SearchPanel.Install(this);

            foldingManager = FoldingManager.Install(TextArea);
            foldingStrategy = new XmlFoldingStrategy();
        }

        public string TextBinding
        {
            get { return (string)GetValue(TextBindingProperty); }
            set { SetValue(TextBindingProperty, value); }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (_isTextChanging)
                return;

            if (SyntaxHighlighting == HighlightingManager.Instance.GetDefinition("XML"))
                foldingStrategy.UpdateFoldings(foldingManager, Document);

            _isTextChanging = true;
            SetCurrentValue(TextBindingProperty, Text);
            _isTextChanging = false;
        }

        private static void TextBindingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextEditor editor)
                editor.UpdateTextBinding();
        }

        private void UpdateTextBinding()
        {
            if (!_isTextChanging)
                Document.Text = TextBinding ?? string.Empty;
        }

    }
}
