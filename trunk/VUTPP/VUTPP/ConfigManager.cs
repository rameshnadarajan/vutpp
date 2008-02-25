using System.Reflection;
using System.Configuration;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using Microsoft.Win32;

namespace larosel.VUTPP
{
    class ConfigManager
    {
        #region Singleton Pattern
        private static volatile ConfigManager m_instance;
        private static object m_syncRoot = new object();

        // config
        public bool ReduceFilename
        {
            get
            {
                return bool.Parse(ReadValue("ReduceFilename", true));
            }
            set
            {
                WriteValue("ReduceFilename", value);
            }
        }
        public bool GotoLineSelect
        {
            get
            {
                return bool.Parse(ReadValue("GotoLineSelect", true));
            }
            set
            {
                WriteValue("GotoLineSelect", value);
            }
        }
        public bool WatchCurrentFile
        {
            get
            {
                return bool.Parse(ReadValue("WatchCurrentFile", true));
            }
            set
            {
                WriteValue("WatchCurrentFile", value);
            }
        }
        public int ParseTime
        {
            get
            {
                return int.Parse(ReadValue("ParseTime", 500));
            }
            set
            {
                WriteValue("ParseTime", value);
            }
        }

        private string ReadValue(string key, object defaultValue)
        {
            RegistryKey reg = Registry.CurrentUser;
            reg = reg.OpenSubKey(@"Software\VisualUnitTest++", true);
            if (reg == null)
                return defaultValue.ToString();
            return Convert.ToString(reg.GetValue(key, defaultValue));
        }

        private void WriteValue(string key, object value)
        {
            RegistryKey reg = Registry.CurrentUser;
            reg = reg.CreateSubKey(@"Software\VisualUnitTest++", RegistryKeyPermissionCheck.ReadWriteSubTree);
            reg.SetValue(key, value.ToString());
            reg.Close();
        }
        ConfigManager()
        {
        }

        public static ConfigManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_instance == null)
                        {
                            m_instance = new ConfigManager();
                            m_instance.Initialize();
                        }
                    }
                }

                return m_instance;
            }
        }

        private SortedList<string, TestRule> m_TestRules = new SortedList<string, TestRule>();
        public SortedList<string, TestRule> TestRules
        {
            get { return m_TestRules; }
        }

        private void Initialize()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string exePath = asm.Location;
            string exeRoot = exePath.Substring(0, exePath.LastIndexOf('\\')+1);
            string rulesPath = exeRoot + "Rules.xml";

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load(rulesPath);
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show(rulesPath, "Load fail");
            }

            foreach (XmlNode ruleNode in doc.DocumentElement.ChildNodes)
            {
                if (ruleNode.Attributes == null)
                    continue;

                TestRule rule = new TestRule(ruleNode);
                m_TestRules.Add(rule.Name.ToUpper(), rule);
            }
        }

        #endregion
    }
}
