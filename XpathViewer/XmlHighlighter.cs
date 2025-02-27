using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Linq;
using System.Windows.Media;
using System.Xml;
using System.Xml.XPath;
using XpathViewer.Controls;

namespace XpathViewer
{
    internal class XmlHighlighter : IBackgroundRenderer
    {
        private readonly TextSegmentCollection<TextMarker> _markers = new TextSegmentCollection<TextMarker>();
        private readonly TextEditor _textEditor;
        private readonly Color _highlightColor;

        public XmlHighlighter(TextEditor textEditor, Color highlightColor)
        {
            _textEditor = textEditor;
            _textEditor.TextArea.TextView.VisualLinesChanged += (sender, e) => _textEditor.TextArea.TextView.InvalidateVisual();
            _highlightColor = highlightColor;
        }
        public KnownLayer Layer => KnownLayer.Selection;

        public void Create(XPathNodeIterator iterator)
        {
            ClearHighlights();

            while (iterator.MoveNext())
            {
                Create(iterator.Current);
            }
        }

        public void Create(XPathNavigator navigator)
        {
            if (navigator is IXmlLineInfo lineInfo && lineInfo.HasLineInfo())
            {
                int startOffset = _textEditor.Document.GetOffset(lineInfo.LineNumber, lineInfo.LinePosition) - 1;
                int length = navigator.OuterXml.Length + 1;

                // If navigator is an Element and the inner XML contains <> (ie, contains child nodes), calculate the end based on the closing xml tag
                if (navigator.NodeType == XPathNodeType.Element && (navigator.InnerXml.Contains('<') || navigator.InnerXml.Contains('>')))
                    length = GetLengthFromClosingXmlTag(_textEditor.Text, navigator.Name, startOffset);

                Create(startOffset, length);
            }
        }

        public void Create(int lineNumber)
        {
            DocumentLine line = _textEditor.Document.GetLineByNumber(lineNumber);
            Create(line.Offset, line.Length);
        }

        public void Create(int startOffset, int length)
        {
            TextMarker marker = new TextMarker(startOffset, length);

            // Don't add a TextMarker if one already exists in the same range
            if (_markers.Where(m => marker.StartOffset >= m.StartOffset && marker.EndOffset <= m.EndOffset).Count() != 0)
                return;

            _markers.Add(marker);
            _textEditor.TextArea.TextView.Redraw();
        }

        public void ClearHighlights()
        {
            _markers.Clear();
            _textEditor.TextArea.TextView.Redraw();
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            foreach (var marker in _markers)
            {
                foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    // First adding a rectangle with the same bursh as the textEditor background to prevent strange colors with overlapping highlights, then add the highlight color
                    drawingContext.DrawRectangle(_textEditor.Background, null, rect);
                    drawingContext.DrawRectangle(new SolidColorBrush(_highlightColor), null, rect);
                }
            }
        }


        private int GetLengthFromClosingXmlTag(string xml, string nodeName, int startIndex)
        {
            int depth = 0;
            int currentIndex = startIndex;

            while (currentIndex < xml.Length)
            {
                int openingTagIndex = xml.IndexOf($"<{nodeName}", currentIndex);
                int closingTagIndex = xml.IndexOf($"</{nodeName}", currentIndex);

                if (openingTagIndex != -1 && openingTagIndex < closingTagIndex)
                {
                    depth++;
                    currentIndex = openingTagIndex + 1;
                }
                else
                {
                    depth--;

                    if (depth <= 0)
                        return xml.IndexOf(">", closingTagIndex) + 1 - startIndex;

                    currentIndex = closingTagIndex + 1;
                }
            }

            return -1;
        }


        private class TextMarker : TextSegment
        {
            public TextMarker(int startOffset, int length)
            {
                StartOffset = startOffset;
                Length = length;
            }
        }

    }
}
