using System.Runtime.InteropServices; // DllImport
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
//using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.VCProject;
using Microsoft.VisualStudio.VCProjectEngine;
using EnvDTE;


namespace larosel.VUTPP
{
    public partial class UnitTestBrowser : UserControl
    {
        public string vsSolutionFolder = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";

        public UnitTestBrowser()
        {
            m_dte = (EnvDTE.DTE)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE));
            InitEvents();

            InitializeComponent();
            this.DoubleBuffered = true;

            tabControl1.BackColor = Color.Transparent;
            TestList.TreeViewNodeSorter = new NodeSorter();

            parseTimer.Tick += new EventHandler(TimerEventProcessor);
            parseTimer.Interval = ConfigManager.Instance.ParseTime;

            Browser = this;

            progressBar.StartColor = progressBar.EndColor = Color.LimeGreen;
            progressBar.Value = 0;
            progressBar.MaxValue = 0;

            ReduceFilename.Checked = ConfigManager.Instance.ReduceFilename;
            GotoLineSelect.Checked = ConfigManager.Instance.GotoLineSelect;
            WatchCurrentFile.Checked = ConfigManager.Instance.WatchCurrentFile;
            ReparseTick.Value = ConfigManager.Instance.ParseTime;
        }

        ~UnitTestBrowser()
        {
            Release();
        }

        public void Release()
        {
            Browser = null;
            if (RunningThread != null)
            {
                RunningThread.Abort();
                RunningThread = null;
            }
            if (ParseThread != null)
            {
                ParseThread.Abort();
                ParseThread = null;
            }
            ReleaseProjectEvents();
            ReleaseEvents();
        }

        public enum TREENODE_TYPE
        {
            PROJECT,
            SUITE,
            TEST,
            FAILURE,
        };

        public struct TreeNodeTag
        {
            public TreeNodeTag(larosel.VUTPP.UnitTestBrowser.TREENODE_TYPE vType, string vFramework)
            {
                type = vType;
                framework = vFramework;
                file = null;
                code = null;
                line = -1;
            }
            public TreeNodeTag(larosel.VUTPP.UnitTestBrowser.TREENODE_TYPE vType, string vFramework, string vCode, string vFile, int vLine)
            {
                type = vType;
                framework = vFramework;
                code = vCode;
                file = vFile;
                line = vLine;
            }
            public string file, code, framework;
            public int line;
            public TREENODE_TYPE type;

            public string GetToolTipText(int projectNameCutOffset)
            {
                string strTooltip = string.Format("Framework : {0}\n", framework);
                switch( type )
                {
                    case TREENODE_TYPE.TEST:
                    case TREENODE_TYPE.FAILURE:
                        strTooltip += string.Format("Code : {0}\nFile : {1}\nLine : {2}", code, file.Substring(projectNameCutOffset), line);
                        break;
                }
                return strTooltip.Trim();
            }
        }

        private class LevelCount
        {
            public LevelCount()
            {
                SuiteCount = 0;
                TestCount = 0;
                FailureCount = 0;
            }

            public string GetTooltip(TreeNode node, bool bUseSuite)
            {
                string strTooltip = "";

                TreeNodeTag tag = (TreeNodeTag)node.Tag;
                if (tag.type <= TREENODE_TYPE.PROJECT && bUseSuite == true)
                    strTooltip += string.Format("\nSuites : {0}", SuiteCount);

                if(tag.type <= TREENODE_TYPE.SUITE)
                    strTooltip += string.Format("\nTests : {0}", TestCount);

                if(tag.type <= TREENODE_TYPE.TEST)
                    strTooltip += string.Format("\nFailures : {0}", FailureCount);

                return strTooltip;
            }

            public int SuiteCount;
            public int TestCount;
            public int FailureCount;
        }

        #region Variables
        private string ActiveConfigurationName;
        static UnitTestBrowser Browser = null;
        System.Threading.Thread RunningThread = null, ParseThread = null;
        static private bool bRunning = false;

        private DTE m_dte;			// Reference to the Visual Studio DTE object
        private VCProjectEngineEvents vcprojectEvents = null;
        private SolutionEvents solutionEvents;
        private WindowEvents windowEvents;
        static private System.Windows.Forms.Timer parseTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.ToolTip nodeTooltip = new System.Windows.Forms.ToolTip();
        public enum ICON_TYPE
        {
            READY,
            START,
            FAIL,
            SUCCESS,
            CHECK,
            ERROR,
        };
        enum USE_DOCUMENT
        {
            NOT,
            AUTO,
            USE
        };
        #endregion

        public abstract class IParseInput
        {
            abstract public string GetLine();
            public virtual void Release() { }
        }

        public class ParseInputTextDocument : IParseInput
        {
            private TextDocument m_Document = null;
            private EditPoint ep = null;
            private int line;

            public ParseInputTextDocument( TextDocument document )
            {
                m_Document = document;
                ep = m_Document.StartPoint.CreateEditPoint();
                line = 1;
            }

            override public string GetLine()
            {
                if ( ep == null || line+1 == m_Document.EndPoint.Line )
                    return null;

                line++;
                return ep.GetLines(line-1, line);
            }
        }

        public class ParseInputFile : IParseInput
        {
            private string m_Filename;
            private StreamReader sr;
            public ParseInputFile(string filename)
            {
                m_Filename = filename;
                sr = new StreamReader(filename);
            }

            ~ParseInputFile()
            {
                Release();
            }

            override public void Release()
            {
                sr.Close();
            }

            override public string GetLine()
            {
                return sr.ReadLine();
            }
        }

        private TreeNode Parse(ProjectItem item, USE_DOCUMENT useDocument, TestRule rule, int selectLine)
        {
            string fileName = item.get_FileNames(0);

            if (fileName.ToLower().EndsWith(".cpp") == false)
                return null;

            TreeNode projectNode = new TreeNode();

            IParseInput pi = null;
            if (useDocument != USE_DOCUMENT.NOT && item.Document != null )
            {
                TextDocument td = (TextDocument)item.Document.Object("TextDocument");
                if (td != null)
                {
                    pi = new ParseInputTextDocument(td);
                }
            }

            if(pi == null)
                pi = new ParseInputFile(fileName);

            TreeNode suiteNode = null, testNode = null;
            int lineIndex = 1;
            string strLine;

            int braceCount = 0;
            char[] trimFilter = { ' ', '\r', '\n', '\t' };
            char[] commands = { '{', '}', '\"', '\'', '/', '(' };
            char[] commandsSeperator = { ',', ')' };

            if (rule.SuiteType != SUITE_TYPE.BRACE)
            {
                commands.SetValue('\0', 0);
                commands.SetValue('\0', 1);
            }
            char command = '\0';
            bool bProcessString = false, bProcessComment = false;

            while ((strLine = pi.GetLine()) != null)
            {
                int index = 0;
                while (index != -1 && ((command != '\0' && (index = strLine.IndexOf(command, index)) != -1) || (command == '\0' && (index = strLine.IndexOfAny(commands, index)) != -1)))
                {
                    command = strLine[index];

                    switch (command)
                    {
                        case '\"':
                        case '\'':
                            if (bProcessString == true)
                            {
                                command = '\0';
                                bProcessString = false;
                            }
                            else
                                bProcessString = true;
                            break;

                        case '/':
                            if (index + 1 < strLine.Length && strLine[index + 1] == '/')
                            {
                                index = -1;
                                command = '\0';
                            }
                            else if (index > 0 && strLine[index - 1] == '*')
                            {
                                if (bProcessComment == false)
                                {
                                    pi.Release();
                                    return null;
                                }
                                bProcessComment = false;
                                command = '\0';
                            }
                            else if (index + 1 < strLine.Length && strLine[index + 1] == '*')
                                bProcessComment = true;
                            else
                                command = '\0';
                            break;

                        case '{':
                            if (suiteNode != null)
                                braceCount++;
                            command = '\0';
                            break;

                        case '}':
                            braceCount--;
                            if (braceCount == 0)
                                suiteNode = null;
                            command = '\0';
                            break;

                        case '(':
                            {
                                string strCommand = strLine.Substring(0, index).Trim(trimFilter);
                                TestKeyword keyword;
                                if (rule.Keywords.TryGetValue(strCommand, out keyword) == true)
                                {
                                    string commandName = null;
                                    int commandCount = keyword.NameIndex+1;

                                    for (int c = 0; c < commandCount; c++)
                                    {
                                        int newIndex = strLine.IndexOfAny(commandsSeperator, index + 1);
                                        if (newIndex == -1)
                                        {
                                            pi.Release();
                                            return null;
                                        }
                                        commandName = strLine.Substring(index + 1, newIndex - index - 1).Trim(trimFilter);
                                        index = newIndex;
                                    }
                                    switch(keyword.Type)
                                    {
                                        case TESTKEYWORD_TYPE.SUITE:
                                            if (rule.SuiteType == SUITE_TYPE.BRACE)
                                                suiteNode = AddSuite(projectNode, commandName, rule);
                                            break;

                                        case TESTKEYWORD_TYPE.SUITE_BEGIN:
                                            if (rule.SuiteType == SUITE_TYPE.BEGIN_END)
                                                suiteNode = AddSuite(projectNode, commandName, rule);
                                            break;

                                        case TESTKEYWORD_TYPE.SUITE_END:
                                            if (rule.SuiteType == SUITE_TYPE.BEGIN_END)
                                                suiteNode = null;
                                            break;

                                        case TESTKEYWORD_TYPE.TEST:
                                            if (suiteNode == null && rule.SuiteType != SUITE_TYPE.NOT_USE)
                                                suiteNode = AddSuite(projectNode, "DefaultSuite", rule);
                                            testNode = AddTest(projectNode, suiteNode, commandName, strLine.Trim(), fileName, lineIndex, rule);
                                            break;
                                    }
                                }
                                command = '\0';
                            }
                            break;
                    }
                    if (index != -1)
                        index++;
                }
                if (lineIndex == selectLine && testNode != null)
                    testNode.Checked = true;
                lineIndex++;
            }
            pi.Release();
            return projectNode;
        }

        delegate void SetIconCB(TreeNode node, ICON_TYPE iconType);
        public void SetIcon(TreeNode node, ICON_TYPE iconType)
        {
            if (TestList.InvokeRequired == true || progressBar.InvokeRequired == true)
            {
                this.Invoke(new SetIconCB(SetIcon), new object[] { node, iconType});
                return;
            }
            node.SelectedImageKey = node.ImageKey = iconType.ToString();
            switch (iconType)
            {
                case ICON_TYPE.ERROR:
                case ICON_TYPE.FAIL:
                    progressBar.StartColor = progressBar.EndColor = System.Drawing.Color.Red;
                    break;
            }
        }

        delegate TreeNode AddProjectCB(string projectName, string projectText, TestRule rule);
        private TreeNode AddProject(string projectName, string projectText, TestRule rule)
        {
            if (TestList.InvokeRequired == true)
            {
                return (TreeNode)this.Invoke(new AddProjectCB(AddProject), new object[] { projectName, projectText, rule });
            }

            TreeNode[] nodes = TestList.Nodes.Find(projectName, false);

            TreeNode projectNode = null;

            if (nodes.Length == 0)
            {
                projectNode = new TreeNode(projectText);
                projectNode.Name = projectName;
                SetIcon(projectNode, ICON_TYPE.READY);
                TreeNodeTag tag = new TreeNodeTag(TREENODE_TYPE.PROJECT, rule.Name);
                projectNode.Tag = tag;

                TestList.Nodes.Add(projectNode);

            }
            else
                projectNode = nodes[0];

            return projectNode;
        }

        private TreeNode AddSuite(TreeNode node, string suiteName, TestRule rule)
        {
            TreeNode[] nodes = node.Nodes.Find(suiteName, false);

            TreeNode suiteNode = null;

            if (nodes.Length == 0)
            {
                suiteNode = new TreeNode(suiteName);
                suiteNode.Name = suiteName;
                SetIcon(suiteNode, ICON_TYPE.READY);
                TreeNodeTag tag = new TreeNodeTag(TREENODE_TYPE.SUITE, rule.Name);
                suiteNode.Tag = tag;

                Add(node.Nodes, suiteNode);
            }
            else
                suiteNode = nodes[0];

            return suiteNode;
        }

        private TreeNode AddTest(TreeNode projectNode, TreeNode suiteNode, string testName, string strCode, string fileName, int lineIndex, TestRule rule)
        {
            TreeNode parentNode = suiteNode;
            if (parentNode == null)
                parentNode = projectNode;

            TreeNode[] nodes = parentNode.Nodes.Find(testName, false);

            TreeNode testNode = null;

            if (nodes.Length == 0)
            {
                testNode = new TreeNode(testName);
                testNode.Name = testName;
                SetIcon(testNode, ICON_TYPE.READY);
                TreeNodeTag tag = new TreeNodeTag( TREENODE_TYPE.TEST, rule.Name, strCode, fileName, lineIndex);
                testNode.Tag = tag;

                Add(parentNode.Nodes, testNode);
            }
            else
                testNode = nodes[0];

            if (parentNode.IsExpanded == false)
                parentNode.Expand();

            return testNode;
        }

        private void RescanProjectItem(ProjectItem projectItem, TreeNode projectNode, TestRule rule)
        {
            Reparse(projectItem, -1, rule, USE_DOCUMENT.AUTO);

            foreach (ProjectItem child in projectItem.ProjectItems)
            {
                RescanProjectItem(child, projectNode, rule);
            }
        }

        private void ProcessScan(TreeNode parseParentNode, TreeNode parentNode, TestRule rule)
        {
            foreach (TreeNode parseNode in parseParentNode.Nodes)
            {
                TreeNode[] nodes = parentNode.Nodes.Find(parseNode.Name, false);
                TreeNodeTag tag = (TreeNodeTag)parseNode.Tag;

                switch (tag.type)
                {
                    case TREENODE_TYPE.SUITE:
                        {
                            TreeNode suiteNode = null;
                            if (nodes.Length == 0)
                                suiteNode = AddSuite(parentNode, parseNode.Name, rule);
                            else
                                suiteNode = nodes[0];

                            ProcessScan(parseNode, suiteNode, rule);
                        }
                        break;

                    case TREENODE_TYPE.TEST:
                        if (parentNode.Nodes.Find(parseNode.Name, false).Length == 0)
                        {
                            TreeNode projectNode = parentNode;
                            while( projectNode.Parent != null ) projectNode = projectNode.Parent;
                            TreeNode testNode = AddTest(projectNode, parentNode, parseNode.Name, tag.code, tag.file, tag.line, rule);
                            if (parseNode.Checked == true)
                                SelectNode(testNode);
                        }
                        break;
                }
            }
        }

        private void ScanProjectItem(ProjectItem item, TreeNode projectNode, USE_DOCUMENT useDocument, TestRule rule)
        {
            TreeNode parseNode = Parse(item, useDocument, rule, -1);
            if (parseNode != null && parseNode.Nodes.Count != 0)
            {
                ProcessScan(parseNode, projectNode, rule);
            }

            foreach (ProjectItem child in item.ProjectItems)
            {
                ScanProjectItem(child, projectNode, useDocument, rule);
            }
        }

        private ArrayList GetProjectList()
        {
            ArrayList projectList = new ArrayList();

            if (m_dte.Solution.IsOpen)
            {
                foreach (Project project in m_dte.Solution.Projects)
                {
                    GetProjectListRecursive(ref projectList, project);
                }
            }
            return projectList;
        }

        private void GetProjectListRecursive(ref ArrayList projectList, Project project)
        {
            if (project.Kind == vcContextGuids.vcContextGuidVCProject)
                projectList.Add(project);
            else if (project.Kind == vsSolutionFolder)
            {
                foreach (ProjectItem projectItem in project.ProjectItems)
                {
                    if (projectItem.SubProject != null)
                        GetProjectListRecursive(ref projectList, projectItem.SubProject);
                }
            }
        }

        public void DoRefreshTestList( bool bReparse )
        {
            TestList.Nodes.Clear();
            if (m_dte.Solution.IsOpen)
            {
                InitProjectEvents();
                ArrayList projectList = GetProjectList();
                foreach (Project project in projectList)
                {
                    RefreshProject(project, false);
                }
                TestList.ExpandAll();
//                TestList.Sort();
            }
        }

        delegate void AddNodeCB(TreeNodeCollection nodes, TreeNode node);
        public void Add(TreeNodeCollection nodes, TreeNode node)
        {
            if (TestList.InvokeRequired == true)
            {
                this.Invoke(new AddNodeCB(Add), new object[] { nodes, node });
                return;
            }
            nodes.Add(node);
        }

        delegate void RemoveNodeCB(TreeNodeCollection nodes, TreeNode node);
        private void Remove(TreeNodeCollection nodes, TreeNode node)
        {
            if (TestList.InvokeRequired == true)
            {
                this.Invoke(new RemoveNodeCB(Remove), new object[] { nodes, node });
                return;
            }
            nodes.Remove(node);
        }

        delegate void RemoveNodeAtCB(TreeNodeCollection nodes, int index);
        private void RemoveAt(TreeNodeCollection nodes, int index)
        {
            if (TestList.InvokeRequired == true)
            {
                this.Invoke(new RemoveNodeAtCB(RemoveAt), new object[] { nodes, index });
                return;
            }
            nodes.RemoveAt(index);
        }

        delegate void RemoveNodeByKeyCB(TreeNodeCollection nodes, string key);
        private void RemoveByKey(TreeNodeCollection nodes, string key)
        {
            if (TestList.InvokeRequired == true)
            {
                this.Invoke(new RemoveNodeByKeyCB(RemoveByKey), new object[] { nodes, key });
                return;
            }
            nodes.RemoveByKey(key);
        }

        private void ClearEmptySuite(TreeNode projectNode)
        {
            for (int nodeIndex = 0; nodeIndex < projectNode.Nodes.Count; )
            {
                TreeNodeTag tag = (TreeNodeTag)projectNode.Nodes[nodeIndex].Tag;
                if (projectNode.Nodes[nodeIndex].Nodes.Count == 0 && tag.type == TREENODE_TYPE.SUITE)
                    RemoveAt(projectNode.Nodes, nodeIndex);
                else
                    nodeIndex++;
            }
        }

        private bool RefreshProject(Project project, bool bReparse)
        {
            TestRule rule = TestRule.CheckProject(project);
            if (rule == null)
            {
                RemoveByKey( TestList.Nodes, project.UniqueName);
                return false;
            }

            string projectName = project.UniqueName;

            TreeNode projectNode = AddProject(projectName, project.Name, rule);

            if (bReparse == true)
            {
                foreach (ProjectItem item in project.ProjectItems)
                {
                    RescanProjectItem(item, projectNode, rule);
                }
            }
            else
            {
                foreach (ProjectItem item in project.ProjectItems)
                {
                    ScanProjectItem(item, projectNode, USE_DOCUMENT.AUTO, rule);
                }
            }
            ClearEmptySuite(projectNode);

            projectNode.ExpandAll();
            return true;
        }

        private void TestList_DoubleClick(object sender, EventArgs e)
        {
            if (TestList.SelectedNode == null)
                return;
            TreeNodeTag tag = (TreeNodeTag)TestList.SelectedNode.Tag;
            if( tag.file == null )
                return;

            TestList.SelectedNode.Toggle();

            Document document = m_dte.Documents.Open(tag.file, "Text", false);
            if (document != null)
            {
                TextSelection selection = (TextSelection)document.Selection;
                if (selection != null)
                {
                    selection.GotoLine(tag.line, ConfigManager.Instance.GotoLineSelect);
                }
            }
        }

        private void RefreshTestList_Click(object sender, EventArgs e)
        {
            if( IsRunning() == true )
                return;
            DoRefreshTestList( false );
        }

        private void InitEvents()
        {
            if (solutionEvents == null)
            {
                solutionEvents = m_dte.Events.SolutionEvents;
                solutionEvents.Opened += new _dispSolutionEvents_OpenedEventHandler(SolutionOpened);
                solutionEvents.AfterClosing += new _dispSolutionEvents_AfterClosingEventHandler(SolutionClosed);
                solutionEvents.ProjectAdded += new _dispSolutionEvents_ProjectAddedEventHandler(ProjectAdded);
                solutionEvents.ProjectRemoved += new _dispSolutionEvents_ProjectRemovedEventHandler(ProjectRemoved);
                solutionEvents.ProjectRenamed += new _dispSolutionEvents_ProjectRenamedEventHandler(ProjectRenamed);
            }

            if (windowEvents == null)
            {
                windowEvents = m_dte.Events.get_WindowEvents(null);
                windowEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(WindowActivated);
            }
        }

        private void InitProjectEvents()
        {
            if (vcprojectEvents == null && m_dte.Solution.IsOpen == true)
            {
                ArrayList projectList = GetProjectList();
                if (projectList.Count > 0)
                {
                    VCProjectItem item = (VCProjectItem)((Project)projectList[0]).Object;
                    VCProjectEngine projEngine = (VCProjectEngine)item.VCProjectEngine;

                    vcprojectEvents = (VCProjectEngineEvents)projEngine.Events;
                    vcprojectEvents.ItemAdded += new _dispVCProjectEngineEvents_ItemAddedEventHandler(this.ItemAdded);
                    vcprojectEvents.ItemRemoved += new _dispVCProjectEngineEvents_ItemRemovedEventHandler(this.ItemRemoved);
                    vcprojectEvents.ItemRenamed += new _dispVCProjectEngineEvents_ItemRenamedEventHandler(this.ItemRenamed);
                }
            }
        }

        private void ReleaseEvents()
        {
            if (solutionEvents != null)
            {
                solutionEvents.Opened -= new _dispSolutionEvents_OpenedEventHandler(SolutionOpened);
                solutionEvents.AfterClosing -= new _dispSolutionEvents_AfterClosingEventHandler(SolutionClosed);
                solutionEvents.ProjectAdded -= new _dispSolutionEvents_ProjectAddedEventHandler(ProjectAdded);
                solutionEvents.ProjectRemoved -= new _dispSolutionEvents_ProjectRemovedEventHandler(ProjectRemoved);
                solutionEvents.ProjectRenamed -= new _dispSolutionEvents_ProjectRenamedEventHandler(ProjectRenamed);
            }

            if (windowEvents != null)
            {
                windowEvents.WindowActivated -= new _dispWindowEvents_WindowActivatedEventHandler(WindowActivated);
            }
        }

        private void ReleaseProjectEvents()
        {
            TestList.Nodes.Clear();
            if (vcprojectEvents != null)
            {
                vcprojectEvents.ItemAdded -= new _dispVCProjectEngineEvents_ItemAddedEventHandler(this.ItemAdded);
                vcprojectEvents.ItemRemoved -= new _dispVCProjectEngineEvents_ItemRemovedEventHandler(this.ItemRemoved);
                vcprojectEvents.ItemRenamed -= new _dispVCProjectEngineEvents_ItemRenamedEventHandler(this.ItemRenamed);
            }
        }

        public void SolutionOpened()
        {
            if (IsRunning() == true)
                return;
            DoRefreshTestList(false);
        }

        public void SolutionClosed()
        {
            if (RunningThread != null)
            {
                RunningThread.Abort();
                RunningThread = null;
            }
            if (ParseThread != null)
            {
                ParseThread.Abort();
                ParseThread = null;
            }
            ReleaseProjectEvents();
            SetRunning(false);
            StopParseTimer();
        }

        public void SolutionRenamed(string oldName)
        {
        }

        public void ProjectAdded(Project project)
        {
            if (IsRunning() == true)
                return;

            RefreshProject(project, false);
        }

        public void ProjectRemoved(Project project)
        {
            if (IsRunning() == true)
                return;

            RemoveByKey(TestList.Nodes, project.UniqueName);
        }

        public void ProjectRenamed(Project project, string oldName)
        {
            // not fire VCProject
        }

        private void AddItem(ProjectItem projectItem, USE_DOCUMENT useDocument)
        {
            if (IsRunning() == true)
                return;

            TestRule rule;
            if (projectItem != null && (rule = TestRule.CheckProject(projectItem.ContainingProject)) != null)
            {
                TreeNode[] projectNode = TestList.Nodes.Find(projectItem.ContainingProject.UniqueName, false);
                if (projectNode.Length > 0)
                {
                    ScanProjectItem(projectItem, projectNode[0], useDocument, rule);
//                    TestList.Sort();
                }
            }
        }

        public void ItemAdded(object item, object parent)
        {
            if (IsRunning() == true)
                return;

            VCFile file = item as VCFile;

            if (file != null)
            {
                ProjectItem projectItem = file.Object as ProjectItem;

                AddItem(projectItem, USE_DOCUMENT.AUTO);
            }
        }

        private void RemoveItem(ProjectItem projectItem)
        {
            Project project = projectItem.ContainingProject;

            if (projectItem != null && TestRule.CheckProject(project) != null)
            {
                TreeNode[] projectNodes = TestList.Nodes.Find(projectItem.ContainingProject.UniqueName, false);
                if (projectNodes.Length > 0)
                {
                    string filename = projectItem.get_FileNames(0);

                    Queue<TreeNode> nodes = new Queue<TreeNode>();
                    nodes.Enqueue(projectNodes[0]);

                    while( nodes.Count > 0 )
                    {
                        TreeNode node = nodes.Dequeue();

                        for( int childIndex = 0; childIndex < node.Nodes.Count; )
                        {
                            TreeNode child = node.Nodes[childIndex];
                            TreeNodeTag tag = (TreeNodeTag)child.Tag;

                            if (string.Compare(filename, tag.file, true) == 0)
                                RemoveAt(node.Nodes, childIndex);
                            else
                            {
                                nodes.Enqueue(child);
                                childIndex++;
                            }
                        }
                    }
                }
            }
        }

        public void ItemRemoved(object item, object parent)
        {
            if (IsRunning() == true)
                return;
            VCFile file = item as VCFile;
            if (file != null)
            {
                ProjectItem projectItem = file.Object as ProjectItem;
                RemoveItem(projectItem);
            }
        }

        public void ItemRenamed(object item, object parent, string oldName)
        {
            if (IsRunning() == true)
                return;
            VCFile file = item as VCFile;
            if (file != null)
            {
                ProjectItem projectItem = file.Object as ProjectItem;
                Project project = projectItem.ContainingProject;

                if (projectItem != null && TestRule.CheckProject(project) != null)
                {
                    TreeNode[] projectNodes = TestList.Nodes.Find(projectItem.ContainingProject.UniqueName, false);
                    if (projectNodes.Length > 0)
                    {
                        string fileName = projectItem.get_FileNames(0);
                        oldName = fileName.Substring(0, fileName.Length - projectItem.Name.Length) + oldName;

                        Queue<TreeNode> nodes = new Queue<TreeNode>();
                        nodes.Enqueue(projectNodes[0]);

                        while (nodes.Count > 0)
                        {
                            TreeNode node = nodes.Dequeue();

                            for (int childIndex = 0; childIndex < node.Nodes.Count; childIndex++)
                            {
                                TreeNode child = node.Nodes[childIndex];
                                TreeNodeTag tag = (TreeNodeTag)child.Tag;

                                if (string.Compare(oldName, tag.file, true) == 0)
                                {
                                    tag.file = fileName;
                                    child.Tag = tag;
                                }
                                nodes.Enqueue(child);
                            }
                        }

                        ClearEmptySuite(projectNodes[0]);
                    }
                }
            }
        }

        private void ProcessReparse(TreeNode parseParentNode, TreeNode parentNode, string filename)
        {
            foreach (TreeNode node in parentNode.Nodes)
            {
                TreeNodeTag tag = (TreeNodeTag)node.Tag;

                switch (tag.type)
                {
                    case TREENODE_TYPE.SUITE:
                        {
                            TreeNode[] parseNodes = parseParentNode.Nodes.Find(node.Name, false);
                            TreeNode parseNode = null;
                            if (parseNodes.Length > 0)
                                parseNode = parseNodes[0];
                            ProcessReparse(parseNode, node, filename);
                        }
                        break;

                    case TREENODE_TYPE.TEST:
                        if( String.Compare( tag.file, filename, true ) == 0 )
                        {
                            if (parseParentNode == null)
                                Remove(parentNode.Nodes, node);
                            else
                            {
                                TreeNode[] parseNodes = parseParentNode.Nodes.Find(node.Name, false);
                                if (parseNodes.Length == 0)
                                    Remove(parentNode.Nodes, node);
                                else if (parseNodes[0].Checked)
                                    SelectNode(node);
                            }
                        }
                        break;
                }
            }
        }

        delegate void SelectNodeCB(TreeNode node);

        private void SelectNode(TreeNode node)
        {
            if (TestList.InvokeRequired == true)
            {
                this.Invoke(new SelectNodeCB(SelectNode), new object[] { node });
                return;
            }
            TestList.SelectedNode = node;
        }

        private void Reparse(ProjectItem projectItem, int selectLine, TestRule rule, USE_DOCUMENT useDocument)
        {
            if (projectItem == null)
                return;

            Project project = projectItem.ContainingProject;
            if (project == null)
                return;

            TreeNode parseNode = Parse(projectItem, useDocument, rule, selectLine);
            if (parseNode == null || parseNode.Nodes.Count == 0)
            {
                RemoveItem(projectItem);
                return;
            }

            string filename = projectItem.get_FileNames(0);

            TreeNode[] projectNodes = TestList.Nodes.Find(projectItem.ContainingProject.UniqueName, false);
            if (projectNodes.Length > 0)
            {
                TreeNode projectNode = projectNodes[0];
                ProcessReparse(parseNode, projectNode, filename);
                ProcessScan(parseNode, projectNode, rule);
                ClearEmptySuite(projectNode);
            }
        }

        public void ReparseCurrentFile()
        {
            if (IsRunning() == true)
                return;

            if (m_dte == null || m_dte.ActiveWindow == null || m_dte.ActiveWindow.Document == null || m_dte.ActiveWindow.Document.ProjectItem == null)
                return;

            ProjectItem projectItem = m_dte.ActiveWindow.Document.ProjectItem;
            Project project = projectItem.ContainingProject;
            TextDocument document = (TextDocument)m_dte.ActiveWindow.Document.Object("TextDocument");
            if (document == null)
                return;

            TestRule rule;
            if ((rule = TestRule.CheckProject(project)) != null)
            {
                int selectLine = -1;
                if (document.Selection != null && document.Selection.ActivePoint != null)
                    selectLine = document.Selection.ActivePoint.Line;

                Reparse(projectItem, selectLine, rule, USE_DOCUMENT.USE);
            }
        }

        delegate void StartTimerCB();
        private void StartParseTimer()
        {
            if (this.InvokeRequired == true)
            {
                this.Invoke(new StartTimerCB(StartParseTimer));
                return;
            }
            parseTimer.Start();
        }

        delegate void StopTimerCB();
        private void StopParseTimer()
        {
            if (this.InvokeRequired == true)
            {
                this.Invoke(new StopTimerCB(StopParseTimer));
                return;
            }
            parseTimer.Stop();
        }

        private void ThreadFuncParse()
        {
            ReparseCurrentFile();
            StartParseTimer();
        }

        private static void TimerEventProcessor(Object myObject, EventArgs myEventArgs) 
        {
            if (ConfigManager.Instance.WatchCurrentFile == true)
            {
                Browser.StopParseTimer();
                Browser.ParseThread = new System.Threading.Thread(new System.Threading.ThreadStart(Browser.ThreadFuncParse));
                Browser.ParseThread.Start();
            }
        }

        public void WindowActivated(Window getFocus, Window lostFocus)
        {
            CheckActivate();
        }

        private void CheckActivate()
        {
            if (IsRunning() == true)
                return;

            if (m_dte.ActiveWindow == null || m_dte.ActiveWindow.ProjectItem == null || TestRule.CheckProject(m_dte.ActiveWindow.Project) == null)
                StopParseTimer();
            else
                StartParseTimer();
        }

        // Create a node sorter that implements the IComparer interface.
        public class NodeSorter : System.Collections.IComparer
        {
            // Compare the length of the strings, or the strings
            // themselves, if they are the same length.
            public int Compare(object x, object y)
            {
                TreeNode tx = x as TreeNode;
                TreeNode ty = y as TreeNode;

                TreeNodeTag xtag = (TreeNodeTag)tx.Tag;
                TreeNodeTag ytag = (TreeNodeTag)ty.Tag;

                switch( xtag.type )
                {
                    case TREENODE_TYPE.PROJECT:
                    case TREENODE_TYPE.SUITE:
                        return string.Compare(tx.Text, ty.Text, true);

                    case TREENODE_TYPE.TEST:
                        if (xtag.file == ytag.file)
                            return xtag.line - ytag.line;

                        return string.Compare(xtag.file, ytag.file);
                }
                return 0;
            }
        }

        private void TestList_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if( TestList.SelectedNode != null )
                TestList.SelectedNode.ForeColor = Color.Black;
        }

        private void TestList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if( e.Node != null )
                e.Node.ForeColor = Color.Red;
        }

        private void GetLevelCount( TreeNode node, ref LevelCount levelCount )
        {
            foreach (TreeNode childNode in node.Nodes)
            {
                TreeNodeTag tag = (TreeNodeTag)childNode.Tag;
                switch (tag.type)
                {
                    case TREENODE_TYPE.PROJECT:
                        break;

                    case TREENODE_TYPE.SUITE:
                        levelCount.SuiteCount++;
                        break;

                    case TREENODE_TYPE.TEST:
                        levelCount.TestCount++;
                        break;

                    case TREENODE_TYPE.FAILURE:
                        levelCount.FailureCount++;
                        break;
                }
                GetLevelCount(childNode, ref levelCount);
            }
        }

        private void TestList_NodeMouseHover(object sender, TreeNodeMouseHoverEventArgs e)
        {
            TreeNode node = e.Node;
            nodeTooltip.Hide(TestList);

            TestRule rule = null;

            if (node != null)
            {
                int projectNameCutOffset = 0;
                TreeNode projectNode = node;
                while (projectNode.Parent != null) projectNode = projectNode.Parent;

                if (m_dte.Solution.IsOpen)
                {
                    ArrayList projectList = GetProjectList();
                    foreach (Project project in projectList)
                    {
                        if (project.UniqueName == projectNode.Name)
                        {
                            if( ConfigManager.Instance.ReduceFilename == true )
                                projectNameCutOffset = project.FullName.LastIndexOf('\\');
                            rule = TestRule.CheckProject(project);
                            if (rule == null)
                                return;
                            break;
                        }
                    }
                }

                nodeTooltip.ToolTipTitle = node.Text;
                TreeNodeTag tag = (TreeNodeTag)node.Tag;
                string strTooltip = tag.GetToolTipText(projectNameCutOffset);
                if (strTooltip.Length > 0)
                    strTooltip += "\n";

                LevelCount levelCount = new LevelCount();
                GetLevelCount(node, ref levelCount);

                strTooltip += levelCount.GetTooltip(node, rule.SuiteType != SUITE_TYPE.NOT_USE).Trim();

                nodeTooltip.Show(strTooltip, TestList, node.Bounds.Left, node.Bounds.Bottom+2);
            }
        }

        private void TestList_MouseLeave(object sender, EventArgs e)
        {
            nodeTooltip.Hide(TestList);
        }

        private void RunAll_Click(object sender, EventArgs e)
        {
            ExecRunAll();
        }

        public void ExecRunAll()
        {
            SetRunning(true);

            BuildList.Clear();
            RunList.Clear();

            if (m_dte.Solution.IsOpen)
            {
                LevelCount levelCount = new LevelCount();
                foreach (TreeNode projectNode in TestList.Nodes)
                {
                    ArrayList projectList = GetProjectList();
                    foreach (Project project in projectList)
                    {
                        if (project.UniqueName == projectNode.Name)
                        {
                            TestRule rule = TestRule.CheckProject(project);
                            if (rule == null)
                                continue;

                            if (RefreshProject(project, true) == false)
                                continue;

                            ClearNode(projectNode, ICON_TYPE.READY, false);
                            GetLevelCount(projectNode, ref levelCount);

                            Runner newRunner = new Runner(this, project, ActiveConfigurationName);
                            if( newRunner.EnableBind == true )
                                BuildList.Add(newRunner);
                            else
                                ClearNode(projectNode, ICON_TYPE.ERROR, false);

                            break;
                        }
                    }
                }
                progressBar.MaxValue = levelCount.TestCount;
            }

            RunningThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(this.ThreadFuncRun));
            RunningThread.Start();
        }

        private void RunSelected_Click(object sender, EventArgs e)
        {
            ExecRunSelected();
        }

        public void ExecRunSelected()
        {
            if( TestList.SelectedNode == null )
                return;

            TreeNodeTag tag = (TreeNodeTag)TestList.SelectedNode.Tag;
            if (tag.type == TREENODE_TYPE.FAILURE)
                return;
            
            SetRunning(true);

            TreeNode projectNode = TestList.SelectedNode;
            while (projectNode.Parent != null) projectNode = projectNode.Parent;

            BuildList.Clear();
            RunList.Clear();

            if (m_dte.Solution.IsOpen)
            {
                ArrayList projectList = GetProjectList();
                foreach (Project project in projectList)
                {
                    if (project.UniqueName == projectNode.Name)
                    {
                        TestRule rule = TestRule.CheckProject(project);
                        if (rule == null)
                            continue;

                        if (RefreshProject(project, true) == false)
                            continue;

                        ClearNode(TestList.SelectedNode, ICON_TYPE.READY, false);

                        if (tag.type == TREENODE_TYPE.TEST)
                            progressBar.MaxValue = 1;
                        else
                        {
                            LevelCount levelCount = new LevelCount();
                            GetLevelCount(TestList.SelectedNode, ref levelCount);

                            progressBar.MaxValue = levelCount.TestCount;
                        }
                        Runner newRunner = new Runner(this, project, ActiveConfigurationName);
                        if (newRunner.EnableBind == true)
                            BuildList.Add(newRunner);
                        else
                            ClearNode(projectNode, ICON_TYPE.ERROR, false);
                        break;
                    }
                }
            }
            RunningThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(this.ThreadFuncRun));
            RunningThread.Start(TestList.SelectedNode);
        }

        public void StopRunning()
        {
            bRunning = false;
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            StopRunning();
        }

        private void SetRunning(bool bRun)
        {
            if (bRun == true)
            {
                ActiveConfigurationName = m_dte.Solution.SolutionBuild.ActiveConfiguration.Name;
                StopParseTimer();
                progressBar.StartColor = progressBar.EndColor = Color.LimeGreen;
                progressBar.Value = 0;
            }
            else
            {
                BuildList.Clear();
                RunList.Clear();

                RunningThread = null;

                CheckActivate();
            }

            EnableControl(RefreshTestList, !bRun);
            EnableControl(RunAll, !bRun);
            EnableControl(RunSelected, !bRun);
            EnableControl(Stop, bRun);
            bRunning = bRun;
        }

        private bool IsRunning()
        {
            if (RunningThread == null)
                return false;

            return true;
        }

        private ArrayList BuildList = new ArrayList();
        private ArrayList RunList = new ArrayList();
        private bool bFirstBuild = true;

        private void CheckBuild()
        {
            if (m_dte.Solution.SolutionBuild.BuildState != vsBuildState.vsBuildStateInProgress && BuildList.Count > 0)
            {
                string projectName = ((Runner)BuildList[0]).ProjectName;

                if (bFirstBuild == false)
                {
                    if (m_dte.Solution.SolutionBuild.LastBuildInfo == 0)
                    {
                        RunList.Add(BuildList[0]);
                        Trace.WriteLine(string.Format("{0} to RunList", projectName));
                    }

                    BuildList.RemoveAt(0);

                    if (BuildList.Count == 0)
                        return;

                    projectName = ((Runner)BuildList[0]).ProjectName;
                }
                else
                    bFirstBuild = false;

                ArrayList projectList = GetProjectList();
                foreach (Project project in projectList)
                {
                    if (project.UniqueName == projectName)
                    {
                        Trace.WriteLine(string.Format("{0} Build", projectName));
                        m_dte.Solution.SolutionBuild.BuildProject(ActiveConfigurationName, project.UniqueName, false);
                        break;
                    }
                }
            }
        }

        private void ThreadFuncRun(Object param)
        {
            TreeNode selectedNode = (TreeNode)param;

            bFirstBuild = true;

            while (m_dte.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateInProgress)
                System.Threading.Thread.Sleep(100);

            Solution solution = m_dte.Solution;

            while(BuildList.Count > 0 || RunList.Count > 0)
            {
                if (bRunning == false)
                {
                    SetRunning(false);
                    return;
                }

                while (RunList.Count == 0 && BuildList.Count > 0)
                {
                    CheckBuild();
                    System.Threading.Thread.Sleep(100);
                    if (bRunning == false)
                    {
                        SetRunning(false);
                        return;
                    }
                }

                if( RunList.Count > 0 )
                {
                    Runner runner = (Runner)RunList[0];
                    RunList.RemoveAt(0);

                    foreach (TreeNode projectNode in TestList.Nodes)
                    {
                        if (projectNode.Name == runner.ProjectName)
                        {
                            ArrayList projectList = GetProjectList();
                            foreach (Project project in projectList)
                            {
                                if (project.UniqueName == runner.ProjectName)
                                {
                                    if (runner.Bind())
                                    {
                                        if( selectedNode != null )
                                            ProcessTest(selectedNode, runner, false);
                                        else
                                            ProcessTest(projectNode, runner, false);

                                        runner.Unbind();
                                    }
                                    else
                                        ClearNode(projectNode, ICON_TYPE.ERROR, false);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            SetRunning(false);
        }

        private bool ProcessTest(TreeNode node, Runner runner, bool bInternal)
        {
            if (bRunning == false)
            {
                SetRunning(false);
                return false;
            }

            CheckBuild();

            SetIcon(node, ICON_TYPE.START);

            TreeNodeTag tag = (TreeNodeTag)node.Tag;

            switch (tag.type)
            {
                case TREENODE_TYPE.TEST:
                    try
                    {
                        if (runner.Process(node.Parent.Name, node.Name, tag.file) == false)
                            SetIcon(node, ICON_TYPE.ERROR);
                        else
                            SetIcon(node, node.Nodes.Count == 0 ? ICON_TYPE.SUCCESS : ICON_TYPE.CHECK);
                    }
                    catch (System.Exception)
                    {
                        SetIcon(node, ICON_TYPE.ERROR);
                    }
                    break;

                case TREENODE_TYPE.SUITE:
                case TREENODE_TYPE.PROJECT:
                    foreach (TreeNode child in node.Nodes)
                    {
                        if (ProcessTest(child, runner, true) == false)
                            return false;
                    }
                    CheckIcon(node);
                    break;

                default:
                    return true;
            }

            if (bInternal == false)
            {
                TreeNode parentNode = node.Parent;
                while (parentNode != null)
                {
                    CheckIcon(parentNode);
                    parentNode = parentNode.Parent;
                }
            }

            return true;
        }

        private void CheckIcon(TreeNode node)
        {
            bool bSetIcon = false;
            foreach(TreeNode childNode in node.Nodes)
            {
                switch ((ICON_TYPE)(Enum.Parse(typeof(ICON_TYPE), childNode.ImageKey)))
                {
                    case ICON_TYPE.READY:
                        SetIcon(node, ICON_TYPE.READY);
                        return;

                    case ICON_TYPE.FAIL:
                        SetIcon(node, ICON_TYPE.FAIL);
                        bSetIcon = true;
                        break;

                    case ICON_TYPE.CHECK:
                        if (bSetIcon == false)
                        {
                            SetIcon(node, ICON_TYPE.CHECK);
                            bSetIcon = true;
                        }
                        break;
                }
            }
            if( bSetIcon == false )
                SetIcon(node, ICON_TYPE.SUCCESS);
        }

        delegate void ClearNodeCB(TreeNode node, ICON_TYPE iconType, bool bInternal);

        private void ClearNode(TreeNode node, ICON_TYPE iconType, bool bInternal)
        {
            if (TestList.InvokeRequired == true)
            {
                this.Invoke(new ClearNodeCB(ClearNode), new object[] { node, iconType, bInternal });
                return;
            }
            if (bInternal == false)
            {
                TreeNode parentNode = node.Parent;
                while (parentNode != null)
                {
                    SetIcon(parentNode, iconType);
                    parentNode = parentNode.Parent;
                }
            }
            TreeNodeTag tag = (TreeNodeTag)node.Tag;
            if (tag.type == TREENODE_TYPE.TEST)
            {
                node.Nodes.Clear();
            }
            SetIcon(node, iconType);

            foreach (TreeNode childNode in node.Nodes)
            {
                ClearNode(childNode, iconType, true);
            }
        }

        // invoke
        delegate void EnableControlCallback(Control control, bool bEnable);

        private void EnableControl(Control control, bool bEnable)
        {
            if (control.InvokeRequired)
            {
                EnableControlCallback d = new EnableControlCallback(EnableControl);
                this.Invoke(d, new object[] { control, bEnable });
            }
            else
                control.Enabled = bEnable;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel1.Text);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel2.Text);
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.codeproject.com/KB/cpp/VistaProgressBar.aspx");
        }

        private void ReduceFilename_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.Instance.ReduceFilename = ReduceFilename.Checked;
        }

        private void GotoLineSelect_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.Instance.GotoLineSelect = GotoLineSelect.Checked;
        }

        private void WatchCurrentFile_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.Instance.WatchCurrentFile = WatchCurrentFile.Checked;
        }

        private void ReparseTick_ValueChanged(object sender, EventArgs e)
        {
            int tick = Decimal.ToInt32(ReparseTick.Value);
            ConfigManager.Instance.ParseTime = tick;
            parseTimer.Interval = tick;
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel1.Text);
        }
    }
}
