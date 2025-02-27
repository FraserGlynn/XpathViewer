using System.IO;
using System.Net;
using System.Xml;
using System.Xml.XPath;

namespace XpathViewer
{
    internal class XpathEvaluator : IXpathEvaluator
    {

        private readonly XPathNavigator _navigator;

        public static IXpathEvaluator Create(string xml)
        {
            using (StringReader stringReader = new StringReader(xml))
            {
                using (XmlTextReader xmlReader = new XmlTextReader(stringReader))
                {
                    xmlReader.WhitespaceHandling = WhitespaceHandling.All;
                    XPathDocument document = new XPathDocument(xmlReader);
                    return Create(document.CreateNavigator());
                }
            }
        }

        public static IXpathEvaluator Create(XPathNavigator navigator)
        {
            return new XpathEvaluator(navigator);
        }

        private XpathEvaluator(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

        public XPathNodeIterator Select(string xpath)
        {
            return _navigator.Select(CreateXpathExpression(xpath));
        }

        public string Evaluate(string xpath)
        {
            object result = _navigator.Evaluate(CreateXpathExpression(xpath));

            if (result is XPathNodeIterator)
                return ((XPathNodeIterator)result).Current.SelectSingleNode(xpath)?.ToString() ?? string.Empty;

            return result.ToString();
        }

        private XPathExpression CreateXpathExpression(string xpath)
        {
            //Using HtmlDecode to allow escaped characters to be used in the expression
            XPathExpression expression = XPathExpression.Compile(WebUtility.HtmlDecode(xpath));
            return expression;
        }


    }
}
