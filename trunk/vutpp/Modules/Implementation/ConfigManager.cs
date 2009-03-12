using System.Reflection;
using System.Configuration;
using System;
using System.Collections;
using System.Text;
using System.Xml;
using Microsoft.Win32;

namespace VUTPP
{
    class ConfigManager
    {
        #region Singleton Pattern
        private static volatile ConfigManager m_instance;
        private static object m_syncRoot = new object();
		#endregion

        // config
        public bool DisplayFullpath
        {
            get
            {
                return bool.Parse(ReadValue("DisplayFullpath", false));
            }
            set
            {
                WriteValue("DisplayFullpath", value);
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
		public uint TestTimeOut
		{
			get
			{
				return uint.Parse(ReadValue("TestTimeOut", 0));
			}
			set
			{
				WriteValue("TestTimeOut", value);
			}
		}
		public uint ConnectWait
		{
			get
			{
				return uint.Parse(ReadValue("ConnectWait", 5));
			}
			set
			{
				WriteValue("ConnectWait", value);
			}
		}
		public bool AutoBuild
		{
			get
			{
				return bool.Parse(ReadValue("AutoBuild", false));
			}
			set
			{
				WriteValue("AutoBuild", value);
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
            reg = reg.CreateSubKey(@"Software\VisualUnitTest++");
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

        private SortedList m_TestRules = new SortedList();
        public SortedList TestRules
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

    }
}
