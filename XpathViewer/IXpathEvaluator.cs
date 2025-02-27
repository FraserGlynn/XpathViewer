using System.Xml.XPath;

namespace XpathViewer
{
    internal interface IXpathEvaluator
    {
        XPathNodeIterator Select(string xpath);
        string Evaluate(string xpath);

    }
}
