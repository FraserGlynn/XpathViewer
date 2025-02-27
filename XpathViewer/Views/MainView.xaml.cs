using ICSharpCode.AvalonEdit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.XPath;
using XpathViewer.ViewModels;

namespace XpathViewer.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {

        private MainViewModel viewModel => DataContext as MainViewModel;

        private readonly XmlHighlighter _xmlDataHighlighter;
        private readonly XmlHighlighter _xmlDataHighlighterHover;
        private readonly XmlHighlighter _outputHighlighterHover;

        public MainView()
        {
            InitializeComponent();

            txteditor_output.MouseMove += Txteditor_output_MouseMove;
            txteditor_output.MouseLeave += Txteditor_output_MouseLeave;
            txteditor_output.TextArea.TextView.MouseDown += Txteditor_output_MouseDown;

            _xmlDataHighlighter = new XmlHighlighter(txteditor_xmlData, Color.FromArgb(80, 255, 255, 0));
            txteditor_xmlData.TextArea.TextView.BackgroundRenderers.Add(_xmlDataHighlighter);

            _xmlDataHighlighterHover = new XmlHighlighter(txteditor_xmlData, Color.FromArgb(80, 255, 0, 0));
            txteditor_xmlData.TextArea.TextView.BackgroundRenderers.Add(_xmlDataHighlighterHover);

            _outputHighlighterHover = new XmlHighlighter(txteditor_output, Color.FromArgb(80, 255, 0, 0));
            txteditor_output.TextArea.TextView.BackgroundRenderers.Add(_outputHighlighterHover);

            DataContextChanged += MainView_DataContextChanged;
        }

        private void MainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            viewModel.SelectionChanged += ViewModel_SelectionChanged;
        }
        private void ViewModel_SelectionChanged(XPathNodeIterator iterator)
        {
            _xmlDataHighlighter.Create(iterator);
        }

        private void Txteditor_output_MouseMove(object sender, MouseEventArgs e)
        {
            _xmlDataHighlighterHover.ClearHighlights();
            _outputHighlighterHover.ClearHighlights();

            Point mousePoint = e.GetPosition(txteditor_output.TextArea);
            TextViewPosition? postition = txteditor_output.TextArea.TextView.GetPosition(mousePoint + txteditor_output.TextArea.TextView.ScrollOffset);

            if (postition != null)
            {
                int lineNumber = postition.Value.Line;
                if (viewModel.OutputNavigators.TryGetValue(lineNumber, out XPathNavigator navigator))
                {
                    Mouse.OverrideCursor = Cursors.Hand;
                    _xmlDataHighlighterHover.Create(navigator);
                    _outputHighlighterHover.Create(lineNumber);
                }
            }
            else
            {
                // If TextViewPosition is null, reset the cursor
                Mouse.OverrideCursor = null;
            }

        }

        private void Txteditor_output_MouseLeave(object sender, MouseEventArgs e)
        {
            // When the mouse leaves the editor clear all the hover highlights & reset the cursor
            Mouse.OverrideCursor = null;
            _xmlDataHighlighterHover.ClearHighlights();
            _outputHighlighterHover.ClearHighlights();
        }

        private void Txteditor_output_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePoint = e.GetPosition(txteditor_output.TextArea);
            TextViewPosition? postition = txteditor_output.TextArea.TextView.GetPosition(mousePoint + txteditor_output.TextArea.TextView.ScrollOffset);

            if (postition != null)
            {
                int lineNumber = postition.Value.Line;
                if (viewModel.OutputNavigators.TryGetValue(lineNumber, out XPathNavigator navigator))
                {
                    if (navigator is IXmlLineInfo lineInfo && lineInfo.HasLineInfo())
                        txteditor_xmlData.ScrollToLine(lineInfo.LineNumber);
                }
            }

        }


    }
}
