using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XpathViewer.ViewModels
{
    internal class MainViewModel : ViewModel
    {

        public event Action<XPathNodeIterator> SelectionChanged;

        private IXpathEvaluator _xpathEvaluator;
        private readonly Dictionary<int, XPathNavigator> _outputNavigators = new Dictionary<int, XPathNavigator>();

        public IReadOnlyDictionary<int, XPathNavigator> OutputNavigators => _outputNavigators;

        private readonly TypingTimer _xpathStringTypingTimer = new TypingTimer();
        private readonly TypingTimer _expressionStringTypingTimer = new TypingTimer();
        
        private string _xpathString;
        public string XpathString
        {
            get
            {
                return _xpathString;
            }
            set
            {
                _xpathString = value;
                OnPropertyChanged(nameof(XpathString));

                _xpathStringTypingTimer.Reset();
            }
        }

        private string _xmlData;
        public string XmlData
        {
            get
            {
                return _xmlData;
            }
            set
            {
                _xmlData = value;
                OnPropertyChanged(nameof(XmlData));

                CreateXpathEvaluator(XmlData);
                SelectData(XpathString);
            }
        }

        private string _output;
        public string Output
        {
            get
            {
                return _output;
            }
            set
            {
                _output = value;
                OnPropertyChanged(nameof(Output));
            }
        }

        private string _errorString;
        public string ErrorString
        {
            get
            {
                return _errorString;
            }
            set
            {
                _errorString = value;
                OnPropertyChanged(nameof(ErrorString));
            }
        }

        private string _expressionString;
        public string ExpressionString
        {
            get
            {
                return _expressionString;
            }
            set
            {
                _expressionString = value;
                OnPropertyChanged(nameof(ExpressionString));

                _expressionStringTypingTimer.Reset();
            }
        }

        public ICommand IndentCommand => new RelayCommand(() => XmlData = IndentXml(XmlData));

        public MainViewModel()
        {
            _xpathStringTypingTimer.TypingStop += (s, e) => SelectData(XpathString);
            _expressionStringTypingTimer.TypingStop += (s, e) => EvaluateExpressionOnOuputNavigators(ExpressionString);

            XpathString = "//employee";
            XmlData = "<company>\r\n  <department id=\"1\">\r\n    <name>Engineering</name>\r\n    <employee id=\"101\">\r\n      <name>John Doe</name>\r\n      <position>Software Engineer</position>\r\n      <salary>80000</salary>\r\n    </employee>\r\n    <employee id=\"102\">\r\n      <name>Jane Smith</name>\r\n      <position>Data Scientist</position>\r\n      <salary>90000</salary>\r\n    </employee>\r\n  </department>\r\n  <department id=\"2\">\r\n    <name>Marketing</name>\r\n    <employee id=\"201\">\r\n      <name>Michael Brown</name>\r\n      <position>Marketing Manager</position>\r\n      <salary>75000</salary>\r\n    </employee>\r\n    <employee id=\"202\">\r\n      <name>Emily White</name>\r\n      <position>SEO Specialist</position>\r\n      <salary>65000</salary>\r\n    </employee>\r\n  </department>\r\n  <department id=\"3\">\r\n    <name>Human Resources</name>\r\n    <employee id=\"301\">\r\n      <name>David Wilson</name>\r\n      <position>HR Manager</position>\r\n      <salary>78000</salary>\r\n    </employee>\r\n  </department>\r\n</company>";
        }


        private void CreateXpathEvaluator(string xmlData)
        {
            try
            {
                _xpathEvaluator = null;
                _xpathEvaluator = XpathEvaluator.Create(xmlData);
                ErrorString = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
            }
        }

        public void SelectData(string xpath)
        {
            List<string> values = new List<string>();
            _outputNavigators.Clear();

            XPathNodeIterator iterator = Select(xpath);

            if (iterator != null)
            {

                SelectionChanged?.Invoke(iterator.Clone());
                while (iterator.MoveNext())
                {
                    _outputNavigators.Add(iterator.CurrentPosition, iterator.Current.Clone());
                    values.Add(iterator.Current.Value);
                }
            }

            Output = string.Join(Environment.NewLine, values);
            EvaluateExpressionOnOuputNavigators(ExpressionString);
        }

        private XPathNodeIterator Select(string xpath)
        {
            if (_xpathEvaluator == null)
                return null;
            
            ErrorString = string.Empty;
            try
            {
                return _xpathEvaluator?.Select(xpath);
            }
            catch (Exception ex)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(ex.Message);
                if (ex.InnerException != null)
                    stringBuilder.AppendLine(ex.InnerException.Message);
                ErrorString = stringBuilder.ToString();
                return null;
            }
        }

        private void EvaluateExpressionOnOuputNavigators(string expression)
        {
            List<string> values = new List<string>();

            foreach (XPathNavigator navigator in _outputNavigators.Values)
            {
                if (string.IsNullOrEmpty(expression))
                {
                    values.Add(navigator.Value);
                }
                else
                {
                    ErrorString = string.Empty;
                    string value = string.Empty;
                    try
                    {
                        IXpathEvaluator evaluator = XpathEvaluator.Create(navigator);
                        value = evaluator.Evaluate(expression);
                    }
                    catch (Exception ex)
                    {
                        ErrorString = ex.Message;
                    }
                    values.Add(value);
                }
            }

            Output = string.Join(Environment.NewLine, values);
        }


        private string IndentXml(string originalXml)
        {
            try
            {
                XDocument xdocument = XDocument.Parse(originalXml);
                XDeclaration declaration = xdocument.Declaration;
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    Indent = true,
                    IndentChars = new string(' ', 2),
                    OmitXmlDeclaration = declaration == null,
                    Encoding = Encoding.UTF8
                };
                using (MemoryStream output = new MemoryStream())
                {
                    using (StreamReader streamReader = new StreamReader(output))
                    {
                        using (XmlWriter writer = XmlWriter.Create(output, settings))
                        {
                            xdocument.WriteTo(writer);
                            writer.Flush();
                            output.Position = 0L;
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                return originalXml;
            }
        }

    }
}
