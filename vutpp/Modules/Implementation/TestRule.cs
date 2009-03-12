using System;
using System.Collections;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.VCProject;

namespace VUTPP
{
    enum TESTKEYWORD_TYPE
    {
        SUITE,
        SUITE_BEGIN,
        SUITE_END,
        TEST
    }

    enum SUITE_TYPE
    {
        NOT_USE,
        BEGIN_END,
        BRACE,
		WITH_TEST
    }

	public enum BIND_TYPE
	{
		Application,
		DynamicLibrary,
	}

    class TestKeyword
    {
        private TESTKEYWORD_TYPE m_Type;
        private string m_Name;
		private int m_NameIndex;
		private int m_SuiteIndex;

        public TESTKEYWORD_TYPE Type
        {
            get{ return m_Type; }
        }
        public string Name
        {
            get { return m_Name; }
        }
        public int NameIndex
        {
            get { return m_NameIndex; }
        }
		public int SuiteIndex
		{
			get { return m_SuiteIndex; }
		}

        public TestKeyword(XmlNode node, SUITE_TYPE suiteType)
        {
            m_Type = (TESTKEYWORD_TYPE)Enum.Parse(typeof(TESTKEYWORD_TYPE), node.Attributes.GetNamedItem("type").Value);
            m_Name = node.Attributes.GetNamedItem("name").Value;
			m_NameIndex = int.Parse(node.Attributes.GetNamedItem("nameindex").Value);
			if( suiteType == SUITE_TYPE.WITH_TEST )
				m_SuiteIndex = int.Parse(node.Attributes.GetNamedItem("suiteindex").Value);
			else
				m_SuiteIndex = -1;
        }
    }

    class TestRule
    {
        private string m_Name;
		private SUITE_TYPE m_SuiteType;
		private BIND_TYPE m_BindType;
        private SortedList m_Keywords = new SortedList();

        public string Name
        {
            get{ return m_Name; }
        }
        public SUITE_TYPE SuiteType
        {
            get{ return m_SuiteType; }
        }
		public BIND_TYPE BindType
		{
			get{ return m_BindType; }
		}
		public SortedList Keywords
        {
            get{ return m_Keywords; }
        }


        public TestRule(XmlNode node)
        {
            m_Name = node.Attributes.GetNamedItem("name").Value;
			m_SuiteType = (SUITE_TYPE)Enum.Parse( typeof(SUITE_TYPE), node.Attributes.GetNamedItem("suitetype").Value );
			m_BindType = (BIND_TYPE)Enum.Parse( typeof(BIND_TYPE), node.Attributes.GetNamedItem("bindtype").Value );

            XmlNodeList keywordList = node.SelectNodes("KeywordList/Keyword");
            foreach (XmlNode keywordNode in keywordList)
            {
                TestKeyword keyword = new TestKeyword(keywordNode, m_SuiteType);
                m_Keywords.Add(keyword.Name, keyword);
            }
        }

        public static TestRule CheckProject(EnvDTE.Project project)
        {
            if (project == null)
                return null;

            if (project.Kind == Constants.Guids.guidVCProject)
            {
                string PreprocessorDefinitions = VCBind.GetPreprocessorDefinitions(project);
                if (PreprocessorDefinitions == null)
                    return null;

                PreprocessorDefinitions = PreprocessorDefinitions.ToUpper();

                int index = PreprocessorDefinitions.IndexOf("VUTPP_");
                if (index != -1)
                {
                    string projectDefine = PreprocessorDefinitions.Substring(index + 6);
                    char[] endDefine = { ' ', ';' };
                    int index2 = projectDefine.IndexOfAny(endDefine);
                    if (index2 != -1)
                        projectDefine = projectDefine.Substring(0, index2);
                    projectDefine = projectDefine.Trim();

                    int testRuleIndex = ConfigManager.Instance.TestRules.IndexOfKey(projectDefine);
					if( testRuleIndex == -1 )
						return null;
                    return (VUTPP.TestRule)ConfigManager.Instance.TestRules.GetByIndex( testRuleIndex );
                }
            }
            return null;
        }
    }
}
