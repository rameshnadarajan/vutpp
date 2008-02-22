using System.Runtime.InteropServices; // DllImport
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.VCProject;
using Microsoft.VisualStudio.VCProjectEngine;

namespace Tnrsoft.VUTPP
{
    class Runner
    {
        [DllImport("Kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hInstance);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibraryExA(string libraryFilename, IntPtr hFile, UInt32 flag);

        [DllImport("Kernel32.dll")]
        private static extern RunTestProc GetProcAddress(IntPtr hInstance, string procName);

        private UnitTestBrowser Browser;
        private string projectname, framework, filename, dllpath = null;
        private string suitename, testname;
        private IntPtr m_DllInstance = IntPtr.Zero;
        private RunTestProc m_Proc = null;
        private TestRule m_Rule;
        private bool m_bEnableBind = false, m_bRun;

        public bool EnableBind
        {
            get { return m_bEnableBind; }
        }

        public string ProjectName
        {
            get { return projectname; }
        }

        public Runner(UnitTestBrowser vBrowser, EnvDTE.Project project, string ActiveConfigurationName)
        {
            Browser = vBrowser;
            projectname = project.UniqueName;
            m_Rule = TestRule.CheckProject(project);
            framework = m_Rule.Name;
            VCProject vcProject = (VCProject)project.Object;
            if (vcProject != null)
            {
                IVCCollection configs = (IVCCollection)vcProject.Configurations;
                VCConfiguration config = (VCConfiguration)configs.Item(ActiveConfigurationName);
                if (config.ConfigurationType == Microsoft.VisualStudio.VCProjectEngine.ConfigurationTypes.typeDynamicLibrary)
                {
                    dllpath = config.PrimaryOutput;
                    m_bEnableBind = true;
                }
            }
        }

        ~Runner()
        {
           Unbind();
        }

        public bool Bind()
        {
            if (dllpath == null)
                return false;

            Unbind();

            try
            {
                m_DllInstance = LoadLibraryExA(dllpath, IntPtr.Zero, 8);
                if (m_DllInstance != IntPtr.Zero)
                {
            		m_Proc = (RunTestProc)GetProcAddress( m_DllInstance, "RunTest" );
                    if (m_Proc != null)
                        return true;
                    else
                        MessageBox.Show("GetProcAddress(RunTest) Failed", dllpath);
                }
                else
                    MessageBox.Show("LoadLibrary Failed", dllpath);
            }
            catch (System.Exception)
            {
            	
            }
            Unbind();

            return false;
        }

        public void Unbind()
        {
            if (m_DllInstance != IntPtr.Zero)
            {
                try
                {
                    FreeLibrary(m_DllInstance);
                }
                catch (System.Exception)
                {
                }
            }
            m_Proc = null;
        }

        public bool Process(string vSuitename, string vTestname, string vFilename)
        {
            suitename = vSuitename;
            testname = vTestname;
            filename = vFilename;

            m_bTestFail = false;
            m_bRun = true;

            try
            {
                Browser.progressBar.Value++;
                m_Proc(m_DllInstance, suitename, testname, this.OnTestFailure);
            }
            catch (System.Exception)
            {
                m_bRun = false;
            }

            TreeNode[] projectNodes = Browser.TestList.Nodes.Find(projectname, false);
            if (projectNodes.Length == 0)
                return false;

            TreeNode parentNode = projectNodes[0];

            if (m_Rule.SuiteType != SUITE_TYPE.NOT_USE)
            {
                TreeNode[] suiteNodes = projectNodes[0].Nodes.Find(suitename, false);
                if (suiteNodes.Length == 0)
                    return false;
                parentNode = suiteNodes[0];
            }

            TreeNode[] testNodes = parentNode.Nodes.Find(testname, false);
            if (testNodes.Length == 0)
                return false;

            if (m_bTestFail == true && testNodes[0].Nodes.Count == 0)
                return false;

            return m_bRun;
        }

        private bool m_bTestFail;

        public delegate void RunTestProc(IntPtr hModule, string suitename, string testname, TestFailureCB CB);
        public delegate void TestFailureCB(string failure, int line);

        [DllImport("VUTPPRunner.dll")]
        private static extern bool RunTest(string suitename, string testname, string filename);

        [DllImport("VUTPPRunner.dll")]
        private static extern bool BindDll(string dllpath, string projectname, string framework, TestFailureCB cb);

        [DllImport("VUTPPRunner.dll")]
        private static extern void UnbindDll();

        private void OnTestFailure(string failure, int line)
        {
            if (line == -1)
            {
                m_bRun = false;
                return;
            }
            m_bTestFail = true;
            if (Browser == null)
                return;

            TreeNode[] projectNodes = Browser.TestList.Nodes.Find(projectname, false);
            if (projectNodes.Length == 0)
                return;

            TreeNode parentNode = projectNodes[0];

            if (m_Rule.SuiteType != SUITE_TYPE.NOT_USE)
            {
                TreeNode[] suiteNodes = projectNodes[0].Nodes.Find(suitename, false);
                if (suiteNodes.Length == 0)
                    return;
                parentNode = suiteNodes[0];
            }

            TreeNode[] testNodes = parentNode.Nodes.Find(testname, false);
            if (testNodes.Length == 0)
                return;

            UnitTestBrowser.ParseInputFile pi = new UnitTestBrowser.ParseInputFile(filename);
            string code ="";
            for( int lineNumber = 1; lineNumber <= line; lineNumber++ )
            {
                code = pi.GetLine();
            }
            pi.Release();

            TreeNode failureNode = new TreeNode(failure);
            failureNode.Name = failure;
            Browser.SetIcon(failureNode, UnitTestBrowser.ICON_TYPE.FAIL);
            UnitTestBrowser.TreeNodeTag tag = new UnitTestBrowser.TreeNodeTag(UnitTestBrowser.TREENODE_TYPE.FAILURE, framework, code.Trim(), filename, line);
            failureNode.Tag = tag;

            Browser.Add(testNodes[0].Nodes, failureNode);

            testNodes[0].Expand();
        }

    }
}
