using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.VCProject;
using Microsoft.VisualStudio.VCProjectEngine;

namespace larosel.VUTPP
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
        BRACE
    }

    class TestKeyword
    {
        private TESTKEYWORD_TYPE m_Type;
        private string m_Name;
        private int m_NameIndex;

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

        public TestKeyword(XmlNode node)
        {
            m_Type = (TESTKEYWORD_TYPE)Enum.Parse(typeof(TESTKEYWORD_TYPE), node.Attributes.GetNamedItem("type").Value);
            m_Name = node.Attributes.GetNamedItem("name").Value;
            m_NameIndex = int.Parse(node.Attributes.GetNamedItem("nameindex").Value);
        }
    }

    class TestRule
    {
        private string m_Name;
        private SUITE_TYPE m_SuiteType;
        private SortedList<string, TestKeyword> m_Keywords = new SortedList<string, TestKeyword>();

        public string Name
        {
            get{ return m_Name; }
        }
        public SUITE_TYPE SuiteType
        {
            get{ return m_SuiteType; }
        }
        public SortedList<string, TestKeyword> Keywords
        {
            get{ return m_Keywords; }
        }


        public TestRule(XmlNode node)
        {
            m_Name = node.Attributes.GetNamedItem("name").Value;
            m_SuiteType = (SUITE_TYPE)Enum.Parse( typeof(SUITE_TYPE), node.Attributes.GetNamedItem("suitetype").Value );

            XmlNodeList keywordList = node.SelectNodes("KeywordList/Keyword");
            foreach (XmlNode keywordNode in keywordList)
            {
                TestKeyword keyword = new TestKeyword(keywordNode);
                m_Keywords.Add(keyword.Name, keyword);
            }
        }

        public static TestRule CheckProject(EnvDTE.Project project)
        {
            if (project == null)
                return null;

            if (project.Kind == vcContextGuids.vcContextGuidVCProject)
            {
                VCProject vcProject = (VCProject)project.Object;
                if (vcProject != null)
                {
                    IVCCollection configs = (IVCCollection)vcProject.Configurations;
                    VCConfiguration config = (VCConfiguration)configs.Item(project.DTE.Solution.SolutionBuild.ActiveConfiguration.Name);
                    if (config != null)
                    {
                        IVCCollection tools = (IVCCollection)config.Tools;
                        VCCLCompilerTool compiler = (VCCLCompilerTool)tools.Item("VCCLCompilerTool");
                        if (compiler != null)
                        {
                            if (compiler.PreprocessorDefinitions != null)
                            {
                                string PreprocessorDefinitions = compiler.PreprocessorDefinitions.ToUpper();
                                int index = PreprocessorDefinitions.IndexOf("VUTPP_");
                                if (index != -1)
                                {
                                    string projectDefine = PreprocessorDefinitions.Substring(index + 6);
                                    char[] endDefine = { ' ', ';' };
                                    int index2 = projectDefine.IndexOfAny(endDefine);
                                    if (index2 != -1)
                                        projectDefine = projectDefine.Substring(0, index2);
                                    projectDefine = projectDefine.Trim();

                                    TestRule rule;
                                    if (ConfigManager.Instance.TestRules.TryGetValue(projectDefine, out rule) == true)
                                        return rule;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
