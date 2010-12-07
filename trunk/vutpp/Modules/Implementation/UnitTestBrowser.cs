using System.Runtime.InteropServices; // DllImport
using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
//using Microsoft.VisualStudio.CommandBars;
using EnvDTE;
using System.ComponentModel.Design;

#if !VS2003
using EnvDTE80;
using DTE = EnvDTE80.DTE2;
#endif

namespace VUTPP
{
	/// <summary>
	/// UnitTestBrowser에 대한 요약 설명입니다.
	/// </summary>
	public class UnitTestBrowser : System.Windows.Forms.UserControl
	{
		private System.ComponentModel.IContainer components;
		private Label LabelContributor;
		private LinkLabel linkContributor;

        private bool bUseThread = true;

		public UnitTestBrowser()
		{
			// 이 호출은 Windows.Forms Form 디자이너에 필요합니다.
			InitializeComponent();

			// TODO: InitializeComponent를 호출한 다음 초기화 작업을 추가합니다.
			tabControl1.BackColor = Color.Transparent;

			Browser = this;

			progressBar.StartColor = progressBar.EndColor = Color.LimeGreen;
			progressBar.ForeColor = Color.Black;
			progressBar.BackColor = Color.White;
			progressBar.Value = 0;
			progressBar.MaxValue = 0;

			DisplayFullpath.Checked = ConfigManager.Instance.DisplayFullpath;
			GotoLineSelect.Checked = ConfigManager.Instance.GotoLineSelect;
			WatchCurrentFile.Checked = ConfigManager.Instance.WatchCurrentFile;
			AutoBuild.Checked = ConfigManager.Instance.AutoBuild;
			TestTimeOut.Value = ConfigManager.Instance.TestTimeOut;
			ConnectWait.Value = ConfigManager.Instance.ConnectWait;

			nodeTooltip.Initial = 0;
		}

		/// <summary> 
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
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
			public TreeNodeTag(VUTPP.UnitTestBrowser.TREENODE_TYPE vType, string vFramework, string vKey)
			{
				key = vKey;
				type = vType;
				framework = vFramework;
				file = null;
				code = null;
				line = -1;
				line_end = -1;
			}
			public TreeNodeTag(VUTPP.UnitTestBrowser.TREENODE_TYPE vType, string vFramework, string vKey, string vCode, string vFile, int vLine, int vLineEnd)
			{
				key = vKey;
				type = vType;
				framework = vFramework;
				code = vCode;
				file = vFile;
				line = vLine;
				line_end = vLineEnd;
			}
			public string key, file, code, framework;
			public int line, line_end;
			public TREENODE_TYPE type;

			public string GetToolTipText(string projectNameCompare)
			{
				string strTooltip = string.Format("Framework : {0}\n", framework);
				switch( type )
				{
					case TREENODE_TYPE.TEST:
					case TREENODE_TYPE.FAILURE:
					{
						string filename = file;
						if( projectNameCompare != null )
						{
							bool bSet = false;
							for( int i = 0; i < projectNameCompare.Length; i++ )
							{
								if( filename.Substring(i, 1).ToUpper() != projectNameCompare.Substring(i,1).ToUpper() )
								{
									filename = filename.Substring( filename.LastIndexOf( '\\', i ) );
									int findIndex = i;
									do 
									{
										if( filename[0] != '\\' )
											filename = "\\" + filename;
										filename = ".." + filename;
										findIndex = projectNameCompare.IndexOf( '\\', findIndex );
									} while (findIndex != -1);
									
									bSet = true;
									break;
								}
							}
							if( bSet == false )
								filename = "." + filename.Substring(projectNameCompare.Length);
						}
						strTooltip += string.Format("Code : {0}\nFile : {1}\nLine : {2}\nLineCount : {3}", code, filename, line, line_end-line+1);
						break;
					}
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
		private SolutionConfiguration ActiveConfiguration;
		static UnitTestBrowser Browser = null;
		System.Threading.Thread RunningThread = null, WatchThread = null, ParseThread = null;
		static public bool bRunning = false;

		private EnvDTE.Window m_toolWin;
		private DTE m_dte;			// Reference to the Visual Studio DTE object
		private SolutionEvents solutionEvents;
		private WindowEvents windowEvents;
		private TextEditorEvents textEditorEvents;
		public System.Windows.Forms.TabControl tabControl1;
		public System.Windows.Forms.TabPage Tests;
		public System.Windows.Forms.TabPage Config;
		public System.Windows.Forms.TreeView TestList;
		private System.Windows.Forms.CheckBox DisplayFullpath;
		private System.Windows.Forms.CheckBox GotoLineSelect;
		private System.Windows.Forms.CheckBox WatchCurrentFile;
		private System.Windows.Forms.Button RunAll;
		private System.Windows.Forms.Button RunSelected;
		private System.Windows.Forms.Button Stop;
		private System.Windows.Forms.Button RefreshTestList;
		private System.Windows.Forms.ImageList imageList1;
		private CustomTooltip.BalloonToolTip nodeTooltip = new CustomTooltip.BalloonToolTip();
		private TreeNode tooltipNode = null;
		public static int s_Line = -1;
		public VistaStyleProgressBar.ProgressBar progressBar;
		private SortedList m_CurrentFileInfo = new SortedList();
		private System.Windows.Forms.NumericUpDown TestTimeOut;
		private System.Windows.Forms.Label labelTestTimeOut;
		private System.Windows.Forms.CheckBox AutoBuild;
		private System.Windows.Forms.Label labelConnectWait;
		private System.Windows.Forms.NumericUpDown ConnectWait;
		private TreeNode ThreadRunSelectedNode;
		private System.Windows.Forms.GroupBox About;
		private System.Windows.Forms.Label labelProjectHome;
		private System.Windows.Forms.LinkLabel linkProjectHome;
		private System.Windows.Forms.LinkLabel linkAuthorHome;
		private System.Windows.Forms.Label labelAuthorHome;
		private System.Windows.Forms.LinkLabel linkProgressBar;
		private System.Windows.Forms.Label labelProgressBar;
		private System.Windows.Forms.Label labelIcon;
		private System.Windows.Forms.LinkLabel linkIcon;
		private Window ActiveWindow;
		private bool mbParse = false;

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
				if( sr != null )
					sr.Close();
			}

			override public string GetLine()
			{
				return sr.ReadLine();
			}
		}

		private TreeNode Parse(ProjectItem item, USE_DOCUMENT useDocument, TestRule rule)
		{
			string fileName = item.get_FileNames(1);

			if (fileName.ToLower().EndsWith(".cpp") == false)
				return null;

			TreeNode projectNode = new TreeNode();

			IParseInput pi = null;
			if (useDocument != USE_DOCUMENT.NOT)
			{
				IEnumerator itr = m_dte.Documents.GetEnumerator();
				while( itr.MoveNext() == true )
				{
					Document document = (Document)itr.Current;
					if( document != null && document.ProjectItem == item )
					{
						TextDocument td = (TextDocument)document.Object("TextDocument");
						if (td != null)
						{
							pi = new ParseInputTextDocument(td);
						}
						break;
					}
				}
			}

			if(pi == null)
				pi = new ParseInputFile(fileName);

			TreeNode suiteNode = null, testNode = null;
			int lineIndex = 1;
			string strLine;

			int suiteBraceCount = 0, testBraceCount = 0;
			char[] trimFilter = { ' ', '\r', '\n', '\t' };
			char[] commands = { '{', '}', '\"', '\'', '/', '(' };
			char[] commandsSeperator = { ',', ')' };

			char command = '\0';
			bool bProcessString = false, bProcessComment = false;

			while ((strLine = pi.GetLine()) != null)
			{
				int index = 0;
				while (index != -1 && ((command != '\0' && (index = strLine.IndexOf(command, index)) != -1) || (command == '\0' && (index = strLine.IndexOfAny(commands, index)) != -1)))
				{
					command = strLine[index];

					if( bProcessComment == false || command == '/' )
					{
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
								if (rule.SuiteType == SUITE_TYPE.BRACE && suiteNode != null)
									suiteBraceCount++;
								if (testNode != null)
									testBraceCount++;
								command = '\0';
								break;

							case '}':
								if( rule.SuiteType == SUITE_TYPE.BRACE && suiteNode != null )
								{
									if (--suiteBraceCount == 0)
										suiteNode = null;
								}
								if( testNode != null )
								{
									if (--testBraceCount == 0)
									{
										TreeNodeTag tag = (TreeNodeTag)testNode.Tag;
										tag.line_end = lineIndex;
										testNode.Tag = tag;
										testNode = null;
									}
								}
								command = '\0';
								break;

							case '(':
							{
								string strCommand = strLine.Substring(0, index).Trim(trimFilter);
								int keyIndex = rule.Keywords.IndexOfKey(strCommand);
								if( keyIndex != -1 )
								{
									TestKeyword keyword = (TestKeyword)rule.Keywords.GetByIndex(keyIndex);
									ArrayList commandNames = new ArrayList();

									while( strLine[index] != ')' )
									{
										int newIndex = strLine.IndexOfAny(commandsSeperator, index + 1);
										if (newIndex == -1)
										{
											pi.Release();
											return null;
										}
										commandNames.Add( strLine.Substring(index + 1, newIndex - index - 1).Trim(trimFilter) );
										index = newIndex;
									}
									switch(keyword.Type)
									{
										case TESTKEYWORD_TYPE.SUITE:
											if (rule.SuiteType == SUITE_TYPE.BRACE)
												suiteNode = AddSuite(projectNode, (string)commandNames[keyword.NameIndex], rule);
											break;

										case TESTKEYWORD_TYPE.SUITE_BEGIN:
											if (rule.SuiteType == SUITE_TYPE.BEGIN_END)
												suiteNode = AddSuite(projectNode, (string)commandNames[keyword.NameIndex], rule);
											break;

										case TESTKEYWORD_TYPE.SUITE_END:
											if (rule.SuiteType == SUITE_TYPE.BEGIN_END)
												suiteNode = null;
											break;

										case TESTKEYWORD_TYPE.TEST:
											switch( rule.SuiteType )
											{
												case SUITE_TYPE.NOT_USE:
													testNode = AddTest(projectNode, null, (string)commandNames[keyword.NameIndex], strLine.Trim(), fileName, lineIndex, -1, rule);
													break;

												case SUITE_TYPE.WITH_TEST:
													suiteNode = AddSuite(projectNode, (string)commandNames[keyword.SuiteIndex], rule);
													testNode = AddTest(projectNode, suiteNode, (string)commandNames[keyword.NameIndex], strLine.Trim(), fileName, lineIndex, -1, rule);
													suiteNode = null;
													break;

												default:
													if( suiteNode == null )
														suiteNode = AddSuite(projectNode, "DefaultSuite", rule);
													testNode = AddTest(projectNode, suiteNode, (string)commandNames[keyword.NameIndex], strLine.Trim(), fileName, lineIndex, -1, rule);
													break;
											}
											break;
									}
								}
								command = '\0';
							}
								break;
						}
					}
					if (index != -1)
						index++;
				}
				lineIndex++;
			}
			pi.Release();
			return projectNode;
		}

		#region Properties
		/// <summary>
		/// Recieves the VS DTE object
		/// </summary>
		public DTE DTE
		{
			set
			{
				m_dte = value;
				InitEvents();
			}
		}

		public EnvDTE.Window ToolWindow
		{
			set
			{
				m_toolWin = value;
			}
		}
		#endregion

		#region 구성 요소 디자이너에서 생성한 코드
		/// <summary> 
		/// 디자이너 지원에 필요한 메서드입니다. 
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnitTestBrowser));
			this.progressBar = new VistaStyleProgressBar.ProgressBar();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.Tests = new System.Windows.Forms.TabPage();
			this.RefreshTestList = new System.Windows.Forms.Button();
			this.Stop = new System.Windows.Forms.Button();
			this.RunSelected = new System.Windows.Forms.Button();
			this.RunAll = new System.Windows.Forms.Button();
			this.TestList = new System.Windows.Forms.TreeView();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.Config = new System.Windows.Forms.TabPage();
			this.About = new System.Windows.Forms.GroupBox();
			this.LabelContributor = new System.Windows.Forms.Label();
			this.linkContributor = new System.Windows.Forms.LinkLabel();
			this.linkProjectHome = new System.Windows.Forms.LinkLabel();
			this.labelProjectHome = new System.Windows.Forms.Label();
			this.linkAuthorHome = new System.Windows.Forms.LinkLabel();
			this.labelAuthorHome = new System.Windows.Forms.Label();
			this.linkProgressBar = new System.Windows.Forms.LinkLabel();
			this.labelProgressBar = new System.Windows.Forms.Label();
			this.labelIcon = new System.Windows.Forms.Label();
			this.linkIcon = new System.Windows.Forms.LinkLabel();
			this.AutoBuild = new System.Windows.Forms.CheckBox();
			this.labelTestTimeOut = new System.Windows.Forms.Label();
			this.TestTimeOut = new System.Windows.Forms.NumericUpDown();
			this.WatchCurrentFile = new System.Windows.Forms.CheckBox();
			this.GotoLineSelect = new System.Windows.Forms.CheckBox();
			this.DisplayFullpath = new System.Windows.Forms.CheckBox();
			this.labelConnectWait = new System.Windows.Forms.Label();
			this.ConnectWait = new System.Windows.Forms.NumericUpDown();
			this.tabControl1.SuspendLayout();
			this.Tests.SuspendLayout();
			this.Config.SuspendLayout();
			this.About.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TestTimeOut)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ConnectWait)).BeginInit();
			this.SuspendLayout();
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.BackColor = System.Drawing.Color.Transparent;
			this.progressBar.Location = new System.Drawing.Point(8, 32);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(312, 24);
			this.progressBar.TabIndex = 7;
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.Tests);
			this.tabControl1.Controls.Add(this.Config);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(336, 512);
			this.tabControl1.TabIndex = 0;
			// 
			// Tests
			// 
			this.Tests.Controls.Add(this.RefreshTestList);
			this.Tests.Controls.Add(this.Stop);
			this.Tests.Controls.Add(this.RunSelected);
			this.Tests.Controls.Add(this.RunAll);
			this.Tests.Controls.Add(this.TestList);
			this.Tests.Controls.Add(this.progressBar);
			this.Tests.Location = new System.Drawing.Point(4, 22);
			this.Tests.Name = "Tests";
			this.Tests.Size = new System.Drawing.Size(328, 486);
			this.Tests.TabIndex = 1;
			this.Tests.Text = "Tests";
			// 
			// RefreshTestList
			// 
			this.RefreshTestList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RefreshTestList.Location = new System.Drawing.Point(8, 456);
			this.RefreshTestList.Name = "RefreshTestList";
			this.RefreshTestList.Size = new System.Drawing.Size(96, 23);
			this.RefreshTestList.TabIndex = 6;
			this.RefreshTestList.Text = "Refresh Tests";
			this.RefreshTestList.Click += new System.EventHandler(this.RefreshTestList_Click);
			// 
			// Stop
			// 
			this.Stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.Stop.Location = new System.Drawing.Point(280, 0);
			this.Stop.Name = "Stop";
			this.Stop.Size = new System.Drawing.Size(40, 23);
			this.Stop.TabIndex = 3;
			this.Stop.Text = "Stop";
			this.Stop.Click += new System.EventHandler(this.Stop_Click);
			// 
			// RunSelected
			// 
			this.RunSelected.Location = new System.Drawing.Point(72, 0);
			this.RunSelected.Name = "RunSelected";
			this.RunSelected.Size = new System.Drawing.Size(96, 23);
			this.RunSelected.TabIndex = 2;
			this.RunSelected.Text = "Run Selected";
			this.RunSelected.Click += new System.EventHandler(this.RunSelected_Click);
			// 
			// RunAll
			// 
			this.RunAll.Location = new System.Drawing.Point(8, 0);
			this.RunAll.Name = "RunAll";
			this.RunAll.Size = new System.Drawing.Size(56, 23);
			this.RunAll.TabIndex = 1;
			this.RunAll.Text = "Run All";
			this.RunAll.Click += new System.EventHandler(this.RunAll_Click);
			// 
			// TestList
			// 
			this.TestList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TestList.Cursor = System.Windows.Forms.Cursors.Default;
			this.TestList.HotTracking = true;
			this.TestList.ImageIndex = 0;
			this.TestList.ImageList = this.imageList1;
			this.TestList.Location = new System.Drawing.Point(8, 64);
			this.TestList.Name = "TestList";
			this.TestList.SelectedImageIndex = 0;
			this.TestList.Size = new System.Drawing.Size(312, 384);
			this.TestList.TabIndex = 0;
			this.TestList.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.TestList_BeforeSelect);
			this.TestList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TestList_AfterSelect);
			this.TestList.DoubleClick += new System.EventHandler(this.TestList_DoubleClick);
			this.TestList.MouseLeave += new System.EventHandler(this.TestList_MouseLeave);
			this.TestList.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TestList_MouseMove);
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "");
			this.imageList1.Images.SetKeyName(1, "");
			this.imageList1.Images.SetKeyName(2, "");
			this.imageList1.Images.SetKeyName(3, "");
			this.imageList1.Images.SetKeyName(4, "");
			this.imageList1.Images.SetKeyName(5, "");
			this.imageList1.Images.SetKeyName(6, "");
			// 
			// Config
			// 
			this.Config.Controls.Add(this.About);
			this.Config.Controls.Add(this.AutoBuild);
			this.Config.Controls.Add(this.labelTestTimeOut);
			this.Config.Controls.Add(this.TestTimeOut);
			this.Config.Controls.Add(this.WatchCurrentFile);
			this.Config.Controls.Add(this.GotoLineSelect);
			this.Config.Controls.Add(this.DisplayFullpath);
			this.Config.Controls.Add(this.labelConnectWait);
			this.Config.Controls.Add(this.ConnectWait);
			this.Config.Location = new System.Drawing.Point(4, 22);
			this.Config.Name = "Config";
			this.Config.Size = new System.Drawing.Size(328, 486);
			this.Config.TabIndex = 1;
			this.Config.Text = "Config";
			// 
			// About
			// 
			this.About.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.About.Controls.Add(this.LabelContributor);
			this.About.Controls.Add(this.linkContributor);
			this.About.Controls.Add(this.linkProjectHome);
			this.About.Controls.Add(this.labelProjectHome);
			this.About.Controls.Add(this.linkAuthorHome);
			this.About.Controls.Add(this.labelAuthorHome);
			this.About.Controls.Add(this.linkProgressBar);
			this.About.Controls.Add(this.labelProgressBar);
			this.About.Controls.Add(this.labelIcon);
			this.About.Controls.Add(this.linkIcon);
			this.About.Location = new System.Drawing.Point(8, 326);
			this.About.Name = "About";
			this.About.Size = new System.Drawing.Size(312, 154);
			this.About.TabIndex = 6;
			this.About.TabStop = false;
			this.About.Text = "About";
			// 
			// LabelContributor
			// 
			this.LabelContributor.Location = new System.Drawing.Point(8, 121);
			this.LabelContributor.Name = "LabelContributor";
			this.LabelContributor.Size = new System.Drawing.Size(88, 16);
			this.LabelContributor.TabIndex = 2;
			this.LabelContributor.Text = "Contributor :";
			// 
			// linkContributor
			// 
			this.linkContributor.BackColor = System.Drawing.Color.Transparent;
			this.linkContributor.Location = new System.Drawing.Point(96, 121);
			this.linkContributor.Name = "linkContributor";
			this.linkContributor.Size = new System.Drawing.Size(136, 16);
			this.linkContributor.TabIndex = 3;
			this.linkContributor.TabStop = true;
			this.linkContributor.Text = "blog.powerumc.kr";
			this.linkContributor.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkContributor_LinkClicked);
			// 
			// linkProjectHome
			// 
			this.linkProjectHome.BackColor = System.Drawing.Color.Transparent;
			this.linkProjectHome.Location = new System.Drawing.Point(96, 24);
			this.linkProjectHome.Name = "linkProjectHome";
			this.linkProjectHome.Size = new System.Drawing.Size(144, 16);
			this.linkProjectHome.TabIndex = 1;
			this.linkProjectHome.TabStop = true;
			this.linkProjectHome.Text = "vutpp.googlecode.com";
			this.linkProjectHome.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkProjectHome_LinkClicked);
			// 
			// labelProjectHome
			// 
			this.labelProjectHome.Location = new System.Drawing.Point(8, 24);
			this.labelProjectHome.Name = "labelProjectHome";
			this.labelProjectHome.Size = new System.Drawing.Size(88, 16);
			this.labelProjectHome.TabIndex = 0;
			this.labelProjectHome.Text = "ProjectHome :";
			// 
			// linkAuthorHome
			// 
			this.linkAuthorHome.BackColor = System.Drawing.Color.Transparent;
			this.linkAuthorHome.Location = new System.Drawing.Point(96, 48);
			this.linkAuthorHome.Name = "linkAuthorHome";
			this.linkAuthorHome.Size = new System.Drawing.Size(112, 16);
			this.linkAuthorHome.TabIndex = 1;
			this.linkAuthorHome.TabStop = true;
			this.linkAuthorHome.Text = "www.larosel.com";
			this.linkAuthorHome.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAuthorHome_LinkClicked);
			// 
			// labelAuthorHome
			// 
			this.labelAuthorHome.Location = new System.Drawing.Point(8, 48);
			this.labelAuthorHome.Name = "labelAuthorHome";
			this.labelAuthorHome.Size = new System.Drawing.Size(88, 16);
			this.labelAuthorHome.TabIndex = 0;
			this.labelAuthorHome.Text = "AuthorHome :";
			// 
			// linkProgressBar
			// 
			this.linkProgressBar.BackColor = System.Drawing.Color.Transparent;
			this.linkProgressBar.Location = new System.Drawing.Point(96, 72);
			this.linkProgressBar.Name = "linkProgressBar";
			this.linkProgressBar.Size = new System.Drawing.Size(184, 16);
			this.linkProgressBar.TabIndex = 1;
			this.linkProgressBar.TabStop = true;
			this.linkProgressBar.Text = "Vista Style Progress Bar in C#";
			this.linkProgressBar.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkProgressBar_LinkClicked);
			// 
			// labelProgressBar
			// 
			this.labelProgressBar.Location = new System.Drawing.Point(8, 72);
			this.labelProgressBar.Name = "labelProgressBar";
			this.labelProgressBar.Size = new System.Drawing.Size(88, 16);
			this.labelProgressBar.TabIndex = 0;
			this.labelProgressBar.Text = "ProgressBar :";
			// 
			// labelIcon
			// 
			this.labelIcon.Location = new System.Drawing.Point(8, 96);
			this.labelIcon.Name = "labelIcon";
			this.labelIcon.Size = new System.Drawing.Size(88, 16);
			this.labelIcon.TabIndex = 0;
			this.labelIcon.Text = "IconFile :";
			// 
			// linkIcon
			// 
			this.linkIcon.BackColor = System.Drawing.Color.Transparent;
			this.linkIcon.Location = new System.Drawing.Point(96, 96);
			this.linkIcon.Name = "linkIcon";
			this.linkIcon.Size = new System.Drawing.Size(136, 16);
			this.linkIcon.TabIndex = 1;
			this.linkIcon.TabStop = true;
			this.linkIcon.Text = "www.famfamfam.com";
			this.linkIcon.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkIcon_LinkClicked);
			// 
			// AutoBuild
			// 
			this.AutoBuild.Location = new System.Drawing.Point(8, 80);
			this.AutoBuild.Name = "AutoBuild";
			this.AutoBuild.Size = new System.Drawing.Size(104, 24);
			this.AutoBuild.TabIndex = 5;
			this.AutoBuild.Text = "AutoBuild";
			this.AutoBuild.CheckedChanged += new System.EventHandler(this.AutoBuild_CheckedChanged);
			// 
			// labelTestTimeOut
			// 
			this.labelTestTimeOut.Location = new System.Drawing.Point(8, 112);
			this.labelTestTimeOut.Name = "labelTestTimeOut";
			this.labelTestTimeOut.Size = new System.Drawing.Size(80, 16);
			this.labelTestTimeOut.TabIndex = 4;
			this.labelTestTimeOut.Text = "Test Timeout";
			// 
			// TestTimeOut
			// 
			this.TestTimeOut.Location = new System.Drawing.Point(96, 112);
			this.TestTimeOut.Name = "TestTimeOut";
			this.TestTimeOut.Size = new System.Drawing.Size(64, 21);
			this.TestTimeOut.TabIndex = 3;
			this.TestTimeOut.ValueChanged += new System.EventHandler(this.TestTimeOut_ValueChanged);
			// 
			// WatchCurrentFile
			// 
			this.WatchCurrentFile.Location = new System.Drawing.Point(8, 56);
			this.WatchCurrentFile.Name = "WatchCurrentFile";
			this.WatchCurrentFile.Size = new System.Drawing.Size(128, 24);
			this.WatchCurrentFile.TabIndex = 2;
			this.WatchCurrentFile.Text = "Watch current file";
			this.WatchCurrentFile.CheckedChanged += new System.EventHandler(this.WatchCurrentFile_CheckedChanged);
			// 
			// GotoLineSelect
			// 
			this.GotoLineSelect.Location = new System.Drawing.Point(8, 32);
			this.GotoLineSelect.Name = "GotoLineSelect";
			this.GotoLineSelect.Size = new System.Drawing.Size(280, 24);
			this.GotoLineSelect.TabIndex = 1;
			this.GotoLineSelect.Text = "Goto line select";
			this.GotoLineSelect.CheckedChanged += new System.EventHandler(this.GotoLineSelect_CheckedChanged);
			// 
			// DisplayFullpath
			// 
			this.DisplayFullpath.Location = new System.Drawing.Point(8, 8);
			this.DisplayFullpath.Name = "DisplayFullpath";
			this.DisplayFullpath.Size = new System.Drawing.Size(280, 24);
			this.DisplayFullpath.TabIndex = 0;
			this.DisplayFullpath.Text = "Display full filepath";
			this.DisplayFullpath.CheckedChanged += new System.EventHandler(this.DisplayFullpath_CheckedChanged);
			// 
			// labelConnectWait
			// 
			this.labelConnectWait.Location = new System.Drawing.Point(8, 137);
			this.labelConnectWait.Name = "labelConnectWait";
			this.labelConnectWait.Size = new System.Drawing.Size(80, 16);
			this.labelConnectWait.TabIndex = 4;
			this.labelConnectWait.Text = "ConnectWait";
			// 
			// ConnectWait
			// 
			this.ConnectWait.Location = new System.Drawing.Point(96, 136);
			this.ConnectWait.Name = "ConnectWait";
			this.ConnectWait.Size = new System.Drawing.Size(64, 21);
			this.ConnectWait.TabIndex = 3;
			this.ConnectWait.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.ConnectWait.ValueChanged += new System.EventHandler(this.ConnectWait_ValueChanged);
			// 
			// UnitTestBrowser
			// 
			this.Controls.Add(this.tabControl1);
			this.Name = "UnitTestBrowser";
			this.Size = new System.Drawing.Size(336, 512);
			this.tabControl1.ResumeLayout(false);
			this.Tests.ResumeLayout(false);
			this.Config.ResumeLayout(false);
			this.About.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TestTimeOut)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ConnectWait)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void DisplayFullpath_CheckedChanged(object sender, System.EventArgs e)
		{
			ConfigManager.Instance.DisplayFullpath = DisplayFullpath.Checked;
		}

		private void GotoLineSelect_CheckedChanged(object sender, System.EventArgs e)
		{
			ConfigManager.Instance.GotoLineSelect = GotoLineSelect.Checked;
		}

		private void WatchCurrentFile_CheckedChanged(object sender, System.EventArgs e)
		{
			ConfigManager.Instance.WatchCurrentFile = WatchCurrentFile.Checked;
		}

		private void RunAll_Click(object sender, System.EventArgs e)
		{
			ExecRunAll();
		}

		public void ExecRunAll()
		{
            try
            {
			    SetRunning(true);

			    BuildList.Clear();
			    RunList.Clear();

			    if (m_dte.Solution.IsOpen)
			    {
				    LevelCount levelCount = new LevelCount();
				    foreach (TreeNode projectNode in TestList.Nodes)
				    {
					    TreeNodeTag tag = (TreeNodeTag)projectNode.Tag;
					    ArrayList projectList = GetProjectList(m_dte);
					    foreach (Project project in projectList)
					    {
						    if (project.UniqueName == tag.key)
						    {
							    TestRule rule = TestRule.CheckProject(project);
							    if (rule == null)
								    continue;

							    if (RefreshProject(project, true) == false)
								    continue;

							    ClearNode(projectNode, ICON_TYPE.READY, false);
							    GetLevelCount(projectNode, ref levelCount);

							    Runner newRunner = new Runner(this, project, GetConfigurationName( project.UniqueName ));
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

			    CheckSave();
			    ThreadRunSelectedNode = null;
                if (bUseThread)
                {
                    RunningThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.ThreadFuncRun));
                    RunningThread.Start();
                }
                else
                {
                    ThreadFuncRun();
                }
            }
            catch (System.Exception ex)
            {
                int a = 0;
            }
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
			TreeNodeTag projectTag = (TreeNodeTag)projectNode.Tag;

			BuildList.Clear();
			RunList.Clear();

			if (m_dte.Solution.IsOpen)
			{
				ArrayList projectList = GetProjectList(m_dte);
				foreach (Project project in projectList)
				{
					if (project.UniqueName == projectTag.key)
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
						Runner newRunner = new Runner(this, project, GetConfigurationName( project.UniqueName ));
						if (newRunner.EnableBind == true)
							BuildList.Add(newRunner);
						else
							ClearNode(projectNode, ICON_TYPE.ERROR, false);
						break;
					}
				}
			}
			CheckSave();
			ThreadRunSelectedNode = TestList.SelectedNode;
            if (bUseThread)
            {
                RunningThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.ThreadFuncRun));
                RunningThread.Start();
            }
            else
            {
                ThreadFuncRun();
            }
        }

		private void CheckSave()
		{
			if( ConfigManager.Instance.AutoBuild == true )
			{
				IEnumerator itr = m_dte.Documents.GetEnumerator();
				while( itr.MoveNext() == true )
				{
					Document document = (Document)itr.Current;
					if( document.Saved == false )
						document.Save(document.FullName);
				}
			}
		}

		private void Stop_Click(object sender, System.EventArgs e)
		{
			StopRunning();
		}

		private void RefreshTestList_Click(object sender, System.EventArgs e)
		{
			ExecRefreshTestList();
		}

		public void ExecRefreshTestList()
		{
			if (IsRunning() == true)
				return;
			DoRefreshTestList(false);
		}

		public void DoRefreshTestList( bool bReparse )
		{
			TestList.Nodes.Clear();
			if (m_dte.Solution.IsOpen)
			{
				ArrayList projectList = GetProjectList(m_dte);
				foreach (Project project in projectList)
				{
					RefreshProject(project, false);
				}
				//                TestList.Sort();
			}
		}

        delegate bool IsTreeNodeExpandedCB(TreeNode node);
        public bool IsTreeNodeExpanded(TreeNode node)
        {
            if (TestList.InvokeRequired == true)
            {
                return (bool)this.Invoke(new IsTreeNodeExpandedCB(IsTreeNodeExpanded), new object[] { node });
            }
            return node.IsExpanded;
        }

        delegate void TreeNodeExpandCB(TreeNode node);
        public void TreeNodeExpand(TreeNode node)
        {
            if (TestList.InvokeRequired == true)
            {
                this.Invoke(new TreeNodeExpandCB(TreeNodeExpand), new object[] { node });
                return;
            }
            node.Expand();
        }

		delegate void SetIconCB(TreeNode node, ICON_TYPE iconType);
		public void SetIcon(TreeNode node, ICON_TYPE iconType)
		{
			if (TestList.InvokeRequired == true || progressBar.InvokeRequired == true)
			{
				this.Invoke(new SetIconCB(SetIcon), new object[] { node, iconType});
				return;
			}
			node.SelectedImageIndex = node.ImageIndex = (int)iconType;
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

			TreeNode projectNode = FindByKey( TestList.Nodes, projectName );

			if (projectNode == null)
			{
				projectNode = new TreeNode(projectText);
				SetIcon(projectNode, ICON_TYPE.READY);
				TreeNodeTag tag = new TreeNodeTag(TREENODE_TYPE.PROJECT, rule.Name, projectName);
				projectNode.Tag = tag;

				Add( TestList.Nodes, projectNode );

			}

			return projectNode;
		}

		private TreeNode AddSuite(TreeNode node, string suiteName, TestRule rule)
		{
			TreeNode suiteNode = FindByKey(node.Nodes, suiteName);

			if (suiteNode == null)
			{
				suiteNode = new TreeNode(suiteName);
				SetIcon(suiteNode, ICON_TYPE.READY);
				TreeNodeTag tag = new TreeNodeTag(TREENODE_TYPE.SUITE, rule.Name, suiteName);
				suiteNode.Tag = tag;

				Add(node.Nodes, suiteNode);
			}

			return suiteNode;
		}

		private TreeNode AddTest(TreeNode projectNode, TreeNode suiteNode, string testName, string strCode, string fileName, int lineIndex, int lineEndIndex, TestRule rule)
		{
			TreeNode parentNode = suiteNode;
			if (parentNode == null)
				parentNode = projectNode;

			TreeNode testNode = FindByKey(parentNode.Nodes, testName);

			if (testNode == null)
			{
				testNode = new TreeNode(testName);
				SetIcon(testNode, ICON_TYPE.READY);
				TreeNodeTag tag = new TreeNodeTag( TREENODE_TYPE.TEST, rule.Name, testName, strCode, fileName, lineIndex, lineEndIndex );
				testNode.Tag = tag;

				Add(parentNode.Nodes, testNode);
			}

			return testNode;
		}

		private void RescanProjectItem(ProjectItem projectItem, TreeNode projectNode, TestRule rule)
		{
			Reparse(projectItem, rule, USE_DOCUMENT.AUTO);

			foreach (ProjectItem child in projectItem.ProjectItems)
			{
				RescanProjectItem(child, projectNode, rule);
			}
		}

		private void ProcessScan(TreeNode parseParentNode, TreeNode parentNode, TestRule rule)
		{
			foreach (TreeNode parseNode in parseParentNode.Nodes)
			{
				TreeNodeTag tag = (TreeNodeTag)parseNode.Tag;
				TreeNode node = FindByKey(parentNode.Nodes, tag.key);

				switch (tag.type)
				{
					case TREENODE_TYPE.SUITE:
						{
							TreeNode suiteNode = null;
							if (node == null)
								suiteNode = AddSuite(parentNode, tag.key, rule);
							else
								suiteNode = node;

							ProcessScan(parseNode, suiteNode, rule);
						}
						break;

					case TREENODE_TYPE.TEST:
						if (node == null)
						{
							TreeNode projectNode = parentNode;
							while( projectNode.Parent != null ) projectNode = projectNode.Parent;
							TreeNode testNode = AddTest(projectNode, parentNode, tag.key, tag.code, tag.file, tag.line, tag.line_end, rule);
							if (parseNode.Checked == true)
								SelectNode(testNode);
						}
						break;
				}
			}
		}

		private void ScanProjectItem(ProjectItem item, TreeNode projectNode, USE_DOCUMENT useDocument, TestRule rule)
		{
			TreeNode parseNode = Parse(item, useDocument, rule);
			if (parseNode != null && parseNode.Nodes.Count != 0)
			{
				ProcessScan(parseNode, projectNode, rule);
			}

			foreach (ProjectItem child in item.ProjectItems)
			{
				ScanProjectItem(child, projectNode, useDocument, rule);
			}
		}

		static public ArrayList GetProjectList(DTE dte)
		{
			ArrayList projectList = new ArrayList();

			try
			{
			if (dte.Solution.IsOpen)
			{
				foreach (Project project in dte.Solution.Projects)
				{
					GetProjectListRecursive(ref projectList, project);
				}
			}
			}
			catch (System.Runtime.InteropServices.COMException ex)
			{
				
			}
			return projectList;
		}

		static private void GetProjectListRecursive(ref ArrayList projectList, Project project)
		{
			if (project.Kind == Constants.Guids.guidVCProject)
				projectList.Add(project);
			else if (project.Kind == Constants.Guids.vsSolutionFolder)
			{
				foreach (ProjectItem projectItem in project.ProjectItems)
				{
					if (projectItem.SubProject != null)
						GetProjectListRecursive(ref projectList, projectItem.SubProject);
				}
			}
		}

		public void StopRunning()
		{
			if( bRunning == true )
				bRunning = false;
			else
				SetRunning( false );
		}

		private void SetRunning(bool bRun)
		{
			if (bRun == true)
			{
				ActiveConfiguration = m_dte.Solution.SolutionBuild.ActiveConfiguration;
				if( WatchThread != null )
					WatchThread.Interrupt();
				if( ParseThread != null )
					ParseThread.Interrupt();
				progressBar.StartColor = progressBar.EndColor = Color.LimeGreen;
				progressBar.Value = 0;
				progressBar.Animate = true;
			}
			else
			{
				BuildList.Clear();
				RunList.Clear();

				RunningThread = null;

				CheckActivate();
				progressBar.Animate = false;
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
				windowEvents.WindowClosing += new _dispWindowEvents_WindowClosingEventHandler(WindowClosing);
			}

			if( textEditorEvents == null )
			{
				textEditorEvents = m_dte.Events.get_TextEditorEvents(null);
				textEditorEvents.LineChanged += new _dispTextEditorEvents_LineChangedEventHandler(LineChanged);
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
				windowEvents.WindowClosing -= new _dispWindowEvents_WindowClosingEventHandler(WindowClosing);
			}

			if( textEditorEvents != null )
			{
				textEditorEvents.LineChanged -= new _dispTextEditorEvents_LineChangedEventHandler(LineChanged);
			}
		}

		public void LineChanged(EnvDTE.TextPoint startPoint, EnvDTE.TextPoint endPoint, int hint)
		{
			if (IsRunning() == true)
				return;
			mbParse = true;
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
			if (WatchThread != null)
			{
				WatchThread.Abort();
				WatchThread = null;
			}
			if (ParseThread != null)
			{
				ParseThread.Abort();
				ParseThread = null;
			}
			TestList.Nodes.Clear();
			SetRunning(false);
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
				TreeNode projectNode = FindByKey(TestList.Nodes, projectItem.ContainingProject.UniqueName);
				if (projectNode != null)
				{
					ScanProjectItem(projectItem, projectNode, useDocument, rule);
					//                    TestList.Sort();
				}
			}
		}

		private void RemoveItem(ProjectItem projectItem)
		{
			Project project = projectItem.ContainingProject;

			if (projectItem != null && TestRule.CheckProject(project) != null)
			{
				TreeNode projectNode = FindByKey(TestList.Nodes, projectItem.ContainingProject.UniqueName);
				if (projectNode != null)
				{
					string filename = projectItem.get_FileNames(1);

					Queue nodes = new Queue();
					nodes.Enqueue(projectNode);

					while( nodes.Count > 0 )
					{
						TreeNode node = (TreeNode)nodes.Dequeue();

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
					ClearEmptySuite(projectNode);
				}
			}
		}

		private void ProcessReparse(TreeNode parseParentNode, TreeNode parentNode, string filename)
		{
			for( int nodeIndex = 0; nodeIndex < parentNode.Nodes.Count;)
			{
				TreeNode node = parentNode.Nodes[nodeIndex];
				TreeNodeTag tag = (TreeNodeTag)node.Tag;

				switch (tag.type)
				{
					case TREENODE_TYPE.SUITE:
					{
						TreeNode parseNode = FindByKey(parseParentNode.Nodes, tag.key);
						ProcessReparse(parseNode, node, filename);
						nodeIndex++;
					}
					break;

					case TREENODE_TYPE.TEST:
					{
						if( String.Compare( tag.file, filename, true ) == 0 )
						{
							if (parseParentNode == null)
								Remove(parentNode.Nodes, node);
							else
							{
								TreeNode parseNode = FindByKey(parseParentNode.Nodes, tag.key);
								if (parseNode == null )
								{
									Remove(parentNode.Nodes, node);
								}
								else if (parseNode.Checked)
								{
									SelectNode(node);
									nodeIndex++;
								}
								else
									nodeIndex++;
							}
						}
						else
							nodeIndex++;
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

            if (TestList.SelectedNode == node)
                return;

            if (node == null)
                ClearSelectNode();

			TestList.SelectedNode = node;
		}

		delegate void ClearSelectNodeCB();
		private void ClearSelectNode()
		{
			if (TestList.InvokeRequired == true)
			{
				this.Invoke(new ClearSelectNodeCB(ClearSelectNode), null);
				return;
			}
			if( TestList.SelectedNode != null )
				TestList.SelectedNode.ForeColor = Color.Black;
			TestList.SelectedNode = null;
		}

		private void Reparse(ProjectItem projectItem, TestRule rule, USE_DOCUMENT useDocument)
		{
			if (projectItem == null)
				return;

			Project project = projectItem.ContainingProject;
			if (project == null)
				return;

			TreeNode parseNode = Parse(projectItem, useDocument, rule);
			if (parseNode == null || parseNode.Nodes.Count == 0)
			{
				RemoveItem(projectItem);
				return;
			}

			string filename = projectItem.get_FileNames(1);

			TreeNode projectNode = FindByKey(TestList.Nodes, projectItem.ContainingProject.UniqueName);
			if (projectNode != null)
			{
				ProcessReparse(parseNode, projectNode, filename);
				ProcessScan(parseNode, projectNode, rule);
				ClearEmptySuite(projectNode);
			}
		}

		public void ReparseCurrentFile()
		{
			if (IsRunning() == true)
				return;

			Window window = ActiveWindow;
			if (window == null || window.Document == null || window.Document.ProjectItem == null)
				return;

			ProjectItem projectItem = window.Document.ProjectItem;
			Project project = projectItem.ContainingProject;
			TextDocument document = (TextDocument)window.Document.Object("TextDocument");
			if (document == null)
				return;

			TestRule rule;
			if ((rule = TestRule.CheckProject(project)) != null)
			{
				Reparse(projectItem, rule, USE_DOCUMENT.USE);
			}
		}

		private void ThreadFuncWatch()
		{
			try
			{
				while(ConfigManager.Instance.WatchCurrentFile == true )
				{
					Window window = ActiveWindow;
					if( window != null && window.Document != null )
					{
						TextDocument document = (TextDocument)window.Document.Object("TextDocument");
						if( document != null && s_Line != document.Selection.ActivePoint.Line )
						{
							s_Line = document.Selection.ActivePoint.Line;
							SelectLine( s_Line );
						}
					}
					System.Threading.Thread.Sleep(100);
				}
			}
			catch (System.Runtime.InteropServices.SEHException ex)
			{
			}
			catch (System.Runtime.InteropServices.COMException ex)
			{
			}
			catch (System.NullReferenceException ex)
			{
			}
			catch( System.Threading.ThreadInterruptedException ex )
			{
			}
            catch (System.Exception ex)
            {
                int a = 0;
            }
        }

		private void ThreadFuncParse()
		{
			try
			{
				while( true )
				{
					if( mbParse == true )
					{
						while( mbParse == true )
						{
							mbParse = false;
							System.Threading.Thread.Sleep(500);
						}
						ReparseCurrentFile();
						CheckCurrentFile();
					}
					else
						System.Threading.Thread.Sleep(500);
				}
			}
			catch( System.Threading.ThreadInterruptedException ex )
			{
			}
            catch (System.Exception ex)
            {
                int a = 0;
            }
        }

		public void WindowActivated(Window getFocus, Window lostFocus)
		{
            try
            {
                ActiveWindow = getFocus;
                CheckActivate();
            }
            catch (System.Exception ex)
            {
                int a = 0;            	
            }
		}

		public void WindowClosing(Window closingWindow)
		{
			if( WatchThread != null )
				WatchThread.Interrupt();
			if( ParseThread != null )
				ParseThread.Interrupt();
		}

		private void CheckActivate()
		{
			if (IsRunning() == true)
				return;

			Window window = ActiveWindow;
			if (window == null || window.ProjectItem == null || TestRule.CheckProject(window.Project) == null
				|| m_toolWin.Visible == false || this.Visible == false || this.tabControl1.SelectedTab != this.Tests || ConfigManager.Instance.WatchCurrentFile == false)
			{
				if( WatchThread != null )
					WatchThread.Interrupt();
				if( ParseThread != null )
					ParseThread.Interrupt();
				m_CurrentFileInfo.Clear();
			}
			else
			{
				if( WatchThread != null )
				{
					WatchThread.Interrupt();
					m_CurrentFileInfo.Clear();
				}
				if( ParseThread != null )
					ParseThread.Interrupt();
				CheckCurrentFile();
				WatchThread = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadFuncWatch));
				WatchThread.Priority = System.Threading.ThreadPriority.Lowest;
				WatchThread.Start();
				ParseThread = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadFuncParse));
				ParseThread.Priority = System.Threading.ThreadPriority.Lowest;
				ParseThread.Start();
			}
		}
		// Create a node sorter that implements the IComparer interface.
		public class NodeSorter
		{
			// Compare the length of the strings, or the strings
			// themselves, if they are the same length.
			public static int Compare(object x, object y)
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
					case TREENODE_TYPE.FAILURE:
						if (xtag.file == ytag.file)
							return xtag.line - ytag.line;

						return string.Compare(xtag.file, ytag.file);
				}
				return 0;
			}
		}

		private void CheckCurrentFile()
		{
			m_CurrentFileInfo.Clear();
			s_Line = -1;

			Window window = ActiveWindow;
			if( window == null || window.Document == null )
				return;

			ProjectItem projectItem = window.Document.ProjectItem;
			if( projectItem == null || projectItem.FileCount == 0 )
				return;

			Project project = projectItem.ContainingProject;
			if( project == null )
				return;

			string filename = projectItem.get_FileNames(1);

			TreeNode projectNode = FindByKey(TestList.Nodes, projectItem.ContainingProject.UniqueName);
			if( projectNode == null )
				return;

			CheckCurrentFileRecursive( projectNode, filename );
		}

		private void CheckCurrentFileRecursive( TreeNode node, string filename )
		{
			foreach( TreeNode childNode in node.Nodes )
			{
				TreeNodeTag tag = (TreeNodeTag)childNode.Tag;
				switch( tag.type )
				{
					case TREENODE_TYPE.SUITE:
						CheckCurrentFileRecursive( childNode, filename );
					break;

					case TREENODE_TYPE.TEST:
						if( tag.file == filename )
						{
							m_CurrentFileInfo.Add( tag.line, childNode );
						}
					break;
				}
			}
		}

		private void SelectLine( int line )
		{
			if( m_CurrentFileInfo.Count == 0 )
				return;

			foreach( TreeNode node in m_CurrentFileInfo.GetValueList() )
			{
				TreeNodeTag tag = (TreeNodeTag)node.Tag;
				if( line >= tag.line && line <= tag.line_end)
				{
					SelectNode( node );
					return;
				}
				if( line < tag.line )
					break;
			}
			SelectNode( null );
		}

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
//						Trace.WriteLine(string.Format("{0} to RunList", projectName));
					}

					BuildList.RemoveAt(0);

					if (BuildList.Count == 0)
						return;

					projectName = ((Runner)BuildList[0]).ProjectName;
				}
				else
					bFirstBuild = false;

				if( ConfigManager.Instance.AutoBuild == true )
				{
					ArrayList projectList = GetProjectList(m_dte);
					foreach (Project project in projectList)
					{
						if (project.UniqueName == projectName)
						{
							//						Trace.WriteLine(string.Format("{0} Build", projectName));
							m_dte.Solution.SolutionBuild.BuildProject(GetConfigurationName(project.UniqueName), project.UniqueName, false);
							break;
						}
					}
				}
			}
		}

		private string GetConfigurationName( string projectName )
		{
			if( ActiveConfiguration == null )
				return "";

			for( int i = 1; i <= ActiveConfiguration.SolutionContexts.Count; i++ )
			{
				SolutionContext context = ActiveConfiguration.SolutionContexts.Item(i);
				if( context.ProjectName == projectName )
					return context.ConfigurationName;
			}

			return "";
		}

		private void ThreadFuncRun()
		{
            try
            {
                TreeNode selectedNode = ThreadRunSelectedNode;

                bFirstBuild = true;

                while (m_dte.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateInProgress)
                    System.Threading.Thread.Sleep(100);

                Solution solution = m_dte.Solution;

                while (BuildList.Count > 0 || RunList.Count > 0)
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

                    if (RunList.Count > 0)
                    {
                        Runner runner = (Runner)RunList[0];
                        RunList.RemoveAt(0);

                        foreach (TreeNode projectNode in TestList.Nodes)
                        {
                            TreeNodeTag tag = (TreeNodeTag)projectNode.Tag;
                            if (tag.key == runner.ProjectName)
                            {
                                ArrayList projectList = GetProjectList(m_dte);
                                foreach (Project project in projectList)
                                {
                                    if (project.UniqueName == runner.ProjectName)
                                    {
                                        if (runner.Bind())
                                        {
                                            if (selectedNode == null)
                                                selectedNode = projectNode;

                                            if (ProcessTest(selectedNode, runner, false) == true)
                                                runner.Finish();
                                            else if (bRunning == true)
                                            {
                                                TreeNode parentNode = selectedNode;
                                                while (parentNode != null)
                                                {
                                                    SetIcon(parentNode, ICON_TYPE.ERROR);
                                                    parentNode = parentNode.Parent;
                                                }
                                            }
                                            runner.Unbind();
                                        }
                                        else
                                            ClearNode(projectNode, ICON_TYPE.ERROR, false);
                                        selectedNode = null;
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
            catch (System.Exception ex)
            {
                int a = 0;
            }
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
						if( runner.Process(node) == false && bRunning == true )
						{
							SetIcon(node, ICON_TYPE.ERROR);
							return false;
						}
						else if( bRunning == true )
							SetIcon(node, node.Nodes.Count == 0 ? ICON_TYPE.SUCCESS : ICON_TYPE.CHECK);
					}
					catch (System.Exception)
					{
						SetIcon(node, ICON_TYPE.ERROR);
						return false;
					}
					break;

				case TREENODE_TYPE.SUITE:
				case TREENODE_TYPE.PROJECT:
					{
                        bool bExpand = IsTreeNodeExpanded( node );
						if( bExpand == false )
							TreeNodeExpand( node );

						foreach (TreeNode child in node.Nodes)
						{
							if (ProcessTest(child, runner, true) == false && bRunning == true)
							{
								SetIcon(node, ICON_TYPE.ERROR);
								return false;
							}
						}
						CheckIcon(node);
						if( bExpand == false && node.ImageIndex == (int)ICON_TYPE.SUCCESS )
							node.Collapse();
					}
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
				switch ((ICON_TYPE)childNode.ImageIndex)
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

		delegate int FindIndexCB(TreeNodeCollection nodes, TreeNode node);
		public int FindIndex(TreeNodeCollection nodes, TreeNode node)
		{
			if (TestList.InvokeRequired == true)
				return (int)this.Invoke(new FindIndexCB(FindIndex), new object[] { nodes, node });

			int nodeIndexMin = 0, nodeIndexMax = nodes.Count-1;

			while( nodeIndexMin <= nodeIndexMax )
			{
				int nodeIndex = (nodeIndexMin+nodeIndexMax)/2;
				if( NodeSorter.Compare( nodes[nodeIndex], node ) < 0 )
					nodeIndexMin = nodeIndex+1;
				else
					nodeIndexMax = nodeIndex-1;
			}
			return nodeIndexMin;
		}

		delegate void AddNodeCB(TreeNodeCollection nodes, TreeNode node);
		public void Add(TreeNodeCollection nodes, TreeNode node)
		{
			if (TestList.InvokeRequired == true)
			{
				this.Invoke(new AddNodeCB(Add), new object[] { nodes, node });
				return;
			}
			nodes.Insert( FindIndex( nodes, node ), node );
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
			TreeNode node = FindByKey( nodes, key );
			if( node != null )
			nodes.Remove( node );
		}

		delegate TreeNode FindNodeByKeyCB(TreeNodeCollection nodes, string key);
		public TreeNode FindByKey(TreeNodeCollection nodes, string key)
		{
			if( nodes.Count == 0 )
				return null;

			if (TestList.InvokeRequired == true)
			{
				return (TreeNode)this.Invoke(new FindNodeByKeyCB(FindByKey), new object[] { nodes, key });
			}
			IEnumerator itr = nodes.GetEnumerator();
			while( itr.MoveNext() == true )
			{
				TreeNodeTag tag = (TreeNodeTag)((TreeNode)itr.Current).Tag;
				if( tag.key == key )
					return (TreeNode)itr.Current;
			}
			return null;
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

			return true;
		}

		private void TestList_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			if( e.Node != null )
				e.Node.ForeColor = Color.Red;
		}

		private void TestList_BeforeSelect(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			if( TestList.SelectedNode != null )
				TestList.SelectedNode.ForeColor = Color.Black;
		}

		private void TestList_MouseLeave(object sender, System.EventArgs e)
		{
 			nodeTooltip.Enabled = false;
			tooltipNode = null;
		}

		private void TestList_DoubleClick(object sender, System.EventArgs e)
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

		private void TestList_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			TreeNode node = TestList.GetNodeAt( e.X, e.Y );
			if( node == null || node.Bounds.Contains( e.X, e.Y ) == false )
			{
				nodeTooltip.Enabled = false;
				tooltipNode = null;
				return;
			}

			if( node == tooltipNode )
				return;

			nodeTooltip.Enabled = false;

			TestRule rule = null;

			string projectNameCompare = "";
			TreeNode projectNode = node;
			while (projectNode.Parent != null) projectNode = projectNode.Parent;

			if (m_dte.Solution.IsOpen)
			{
				ArrayList projectList = GetProjectList(m_dte);
				foreach (Project project in projectList)
				{
					TreeNodeTag projectTag = (TreeNodeTag)projectNode.Tag;
					if (project.UniqueName == projectTag.key)
					{
						if( ConfigManager.Instance.DisplayFullpath == false )
						{
							projectNameCompare = project.FullName.Substring( 0, project.FullName.LastIndexOf('\\') );
						}
						rule = TestRule.CheckProject(project);
						if (rule == null)
							return;
						break;
					}
				}
			}

			TreeNodeTag tag = (TreeNodeTag)node.Tag;
			string strTooltip = tag.GetToolTipText(projectNameCompare);

			LevelCount levelCount = new LevelCount();
			GetLevelCount(node, ref levelCount);

			strTooltip += levelCount.GetTooltip(node, rule.SuiteType != SUITE_TYPE.NOT_USE);

			nodeTooltip.Title = node.Text;
			nodeTooltip.SetBalloonText(TestList, strTooltip);
			tooltipNode = node;
			nodeTooltip.SetPosition( TestList, node.Bounds.Left, node.Bounds.Bottom);
			nodeTooltip.Enabled = true;
		}

		private void TestTimeOut_ValueChanged(object sender, System.EventArgs e)
		{
			ConfigManager.Instance.TestTimeOut = (uint)TestTimeOut.Value;		
		}

		private void AutoBuild_CheckedChanged(object sender, System.EventArgs e)
		{
			ConfigManager.Instance.AutoBuild = (bool)AutoBuild.Checked;
		}

		private void ConnectWait_ValueChanged(object sender, System.EventArgs e)
		{
			ConfigManager.Instance.ConnectWait = (uint)ConnectWait.Value;		
		}

		private void linkProjectHome_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			e.Link.Visited = true;
			System.Diagnostics.Process.Start("http://vutpp.googlecode.com");
		}

		private void linkAuthorHome_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			e.Link.Visited = true;
			System.Diagnostics.Process.Start("http://www.larosel.com");
		}

		private void linkProgressBar_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			e.Link.Visited = true;
			System.Diagnostics.Process.Start("http://www.codeproject.com/KB/cpp/VistaProgressBar.aspx");
		}

		private void linkIcon_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			e.Link.Visited = true;
			System.Diagnostics.Process.Start("http://www.famfamfam.com/lab/icons/");
		}

		private void linkContributor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			e.Link.Visited = true;
			System.Diagnostics.Process.Start("http://blog.powerumc.kr/");
		}

	}
}


// this.progressBar = new VistaStyleProgressBar.ProgressBar();
// this.Tests.Controls.Add(this.progressBar);
// 
// progressBar
// 
// this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
// | System.Windows.Forms.AnchorStyles.Right)));
// this.progressBar.BackColor = System.Drawing.Color.Transparent;
// this.progressBar.Location = new System.Drawing.Point(8, 32);
// this.progressBar.Name = "progressBar";
// this.progressBar.Size = new System.Drawing.Size(312, 24);
// this.progressBar.TabIndex = 7;
