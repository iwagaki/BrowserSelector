/*
  Copyright (c) 2011 iwagaki@users.sourceforge.net

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in
  all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
  THE SOFTWARE.
*/

using System;
using System.Windows.Forms;

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Threading;

namespace BrowserSelector
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Thread.GetDomain().UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);

            try
            {
                string xmlPath = Application.StartupPath + @"\config.xml";
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
                XmlDocument configXml = new XmlDocument();

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Parse;
                settings.ValidationType = ValidationType.DTD;

                XmlReader reader = XmlReader.Create(xmlPath, settings);

                string argument = @"http://www.google.co.jp";
                if (args.Length > 0)
                    argument = args[0];

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

        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs exceptionEvent)
        {
            Exception e = exceptionEvent.ExceptionObject as Exception;
            MessageBox.Show(e.Message, "Unhandled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
