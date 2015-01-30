using System;
using System.Windows.Forms;

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;

namespace BrowserSelector
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string xmlPath = "config.xml";

            XmlDocument configXml = new XmlDocument();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.ValidationType = ValidationType.DTD;
            
            XmlReader reader = XmlReader.Create(xmlPath, settings);

            string argument = @"http://www.google.co.jp";
            if (args.Length > 0)
                argument = args[0];

            try
            {
                configXml.Load(reader);

                Process externalProcess = new Process();

                string path = getApplicationPath(configXml, getBrowserByUrl(configXml, argument));

                externalProcess.StartInfo.FileName = path;
                externalProcess.StartInfo.Arguments = argument;
                externalProcess.Start();
            }
            catch (XmlSchemaValidationException e)
            {
                MessageBox.Show(e.Message, "Illigal setting in config.xml", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static string getDefualtBrowser(XmlDocument configXml)
        {
            return configXml.SelectSingleNode("/configuration/default").InnerText;
        }

        static string getBrowserByUrl(XmlDocument configXml, string argument)
        {
            XmlNodeList nodes = configXml.SelectNodes("/configuration/patterns/pattern");

            foreach (XmlNode node in nodes)
            {
                string regex = node.SelectSingleNode("regex").InnerText;
                string exec = node.SelectSingleNode("exec").InnerText;

                if (Regex.IsMatch(argument, regex))
                    return exec;
            }

            // No pattern matches
            return getDefualtBrowser(configXml);
        }

        static string getApplicationPath(XmlDocument configXml, string applicationName)
        {
            XmlNodeList nodes = configXml.SelectNodes("/configuration/applications/application");

            foreach (XmlNode node in nodes)
            {
                string name = node.SelectSingleNode("name").InnerText;
                string path = node.SelectSingleNode("path").InnerText;

                if (name == applicationName)
                    return path;
            }

            throw new FormatException(@"Cannot find an application entry for " + applicationName);
        }
    }
}
