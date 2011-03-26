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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using System.Configuration;

namespace BrowserSwitcher
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string argument = @"http://www.google.co.jp";

            if (args.Length > 0)
                argument = args[0];

            Process externalProcess = new Process();

            try
            {
                String filename = ConfigurationManager.AppSettings[isDefaultBrowsing(argument) ? "default" : "another"];
                if (filename != null)
                {
                    externalProcess.StartInfo.FileName = filename;
                    externalProcess.StartInfo.Arguments = argument;
                    externalProcess.Start();
                }
                else
                    throw new FormatException("Entry for \"deault\" or \"another\" should exist in .config");
            }
            catch (FormatException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Invalid regular expression in .config" , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static bool isDefaultBrowsing(string argument)
        {
            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
            {
                if (Regex.IsMatch(key, "^default$") || Regex.IsMatch(key, "^another$"))
                    continue;

                bool isDefault = true;
                
                if (Regex.IsMatch(key, "^default"))
                    isDefault = true;
                else if (Regex.IsMatch(key, "^another"))
                    isDefault = false;
                else
                    throw new FormatException(@"Invalid element was found in .config");

                string pattern = ConfigurationManager.AppSettings[key];
                if (Regex.IsMatch(argument, pattern))
                    return isDefault;
                }

            return true;
        }
    }
}
