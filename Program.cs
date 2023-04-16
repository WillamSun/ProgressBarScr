﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProgressBarSrc
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ScreenSaverForm());
        }


        public class FilesINI
        {
            string path;
            public FilesINI(string path)
            { this.path = path; }
            [DllImport("kernel32")]
            private static extern long WritePrivateProfileString(string section, string key,
                string val, string filePath);

            [DllImport("kernel32")]
            private static extern int GetPrivateProfileString(string section, string key, string def,
                StringBuilder retVal, int size, string filePath);

            public void Write(string key, string section, string value)
            {
                WritePrivateProfileString(section, key, value, path);
            }

            public string Read(string key, string section)
            {
                StringBuilder temp = new StringBuilder(255);
                GetPrivateProfileString(section, key, "", temp, 255, path);
                return temp.ToString();

            }

            public int ReadInteger(string key, string section, int Default)
            {
                string str = Read(key, section);
                try
                { return int.Parse(str); }
                catch
                {
                    if (!string.IsNullOrEmpty(str) && str.ToLower() != "default") MessageBox.Show("\"" + str + "\" is not a valid integer for " + key);
                    return Default;
                }
            }
            public double ReadDouble(string key, string section, double Default)
            {
                string str = Read(key, section);
                try
                { return double.Parse(str); }
                catch
                {
                    if (!string.IsNullOrEmpty(str) && str.ToLower() != "default") MessageBox.Show("\"" + str + "\" is not a valid value for " + key);
                    return Default;
                }
            }
        }

    }
}
