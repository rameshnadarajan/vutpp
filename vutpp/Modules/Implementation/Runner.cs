using System.Runtime.InteropServices; // DllImport
using System;
using System.Text;
using System.Windows.Forms;

namespace VUTPP
{
    class Runner
    {
		private struct SECURITY_ATTRIBUTES
		{
			public int length;
			public IntPtr lpSecurityDescriptor;
			public bool bInheritHandle;
		}

		private struct PROCESS_INFORMATION
		{
			public uint hProcess;
			public uint hThread;
			public uint dwProcessId;
			public uint dwThreadId;
		}
 
// 		private struct PROCESS_BASIC_INFORMATION
// 		{
// 			public int ExitStatus;
// 			public int PebBaseAddress;
// 			public int AffinityMask;
// 			public int BasePriority;
// 			public uint UniqueProcessId;
// 			public uint InheritedFromUniqueProcessId;
// 		}
 
		private struct STARTUPINFO
		{
			public uint cb;
			public string lpReserved;
			public string lpDesktop;
			public string lpTitle;
			public uint dwX;
			public uint dwY;
			public uint dwXSize;
			public uint dwYSize;
			public uint dwXCountChars;
			public uint dwYCountChars;
			public uint dwFillAttribute;
			public uint dwFlags;
			public short wShowWindow;
			public short cbReserved2;
			public IntPtr lpReserved2;
			public uint hStdInput;
			public uint hStdOutput;
			public uint hStdError;
		}

		[DllImport("kernel32.dll")]
		static extern bool CreateProcess(
			string lpApplicationName,
			string lpCommandLine,
			/* ref SECURITY_ATTRIBUTES */ IntPtr lpProcessAttributes,
			/* ref SECURITY_ATTRIBUTES */ IntPtr lpThreadAttributes,
			bool bInheritHandles,
			uint dwCreationFlags,
			IntPtr lpEnvironment,
			string lpCurrentDirectory,
			ref STARTUPINFO lpStartupInfo,
			out PROCESS_INFORMATION lpProcessInformation
			);

		[DllImport("kernel32.dll")]
		static extern bool TerminateProcess(uint hProcess, int exitCode);

		[DllImport("kernel32.dll")]
		static extern int WaitForSingleObject(uint hHandle, int timeOut);

		[DllImport("kernel32.dll")]
		private static extern bool CreatePipe(out uint hReadPipe, out uint hWritePipe, ref SECURITY_ATTRIBUTES sattr, uint size);

		[DllImport("kernel32.dll")]
		static extern bool CloseHandle(uint hHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool PeekNamedPipe(uint handle, byte[] buffer, uint nBufferSize, out uint bytesRead, out uint bytesAvail, out uint BytesLeftThisMessage);

		[DllImport("kernel32.dll")]
		static extern bool ReadFile(uint handle, byte[] buffer, uint bufsize, out uint dwRead, IntPtr OverlappedRead);
 
		[DllImport("kernel32.dll")]
		static extern bool WriteFile(uint handle, byte[] buffer, uint bufsize, out uint dwWritten, IntPtr OverlappedRead);

		[DllImport("Kernel32.dll")]
		private static extern bool FreeLibrary(uint hInstance);

		[DllImport("Kernel32.dll")]
		private static extern uint LoadLibraryExA(string libraryFilename, IntPtr hFile, UInt32 flag);

		public delegate void BindProc(uint hModule, uint readPipe, uint writePipe);

		[DllImport("Kernel32.dll")]
		private static extern BindProc GetProcAddress(uint hInstance, string procName);

		private uint m_DllInstance = 0;
		private BindProc m_Proc = null;

		System.Threading.Thread DllBindThread = null;

        private UnitTestBrowser Browser;
        private string projectname, framework, outputpath = null, defaultpath;
        private uint readPipe = 0, writePipe = 0, readPipeChild = 0, writePipeChild = 0;
		private uint processChild = 0;
        private TestRule m_Rule;
        private bool m_bEnableBind = false;
		private byte[] readBuffer = new byte[1024], writeBuffer = new byte[1024];

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
			defaultpath = project.FullName.Substring( 0, project.FullName.LastIndexOf( '\\' ) );
            outputpath = VCBind.GetPrimaryOutput(project, ActiveConfigurationName, m_Rule.BindType);
            m_bEnableBind = outputpath != null;
		}

        ~Runner()
        {
           Unbind();
        }

		private bool CheckAlive()
		{
			if( processChild != 0 )
				return WaitForSingleObject( processChild, 0 ) == 258;	// timeout:258
			if( DllBindThread != null )
				return DllBindThread.IsAlive;

			return false;
		}

		private bool ReadString( uint timeout, out string strRead )
		{
			DateTime dt = DateTime.Now;
			uint bytesRead = 0, bytesAvail, bytesLeft;
			
			while( timeout == 0 || DateTime.Now.Subtract( dt ) < TimeSpan.FromMilliseconds( timeout ) )
			{
				if( UnitTestBrowser.bRunning == false )
					break;

				if( CheckAlive() == false )	// timeout:258
					break;

				if( PeekNamedPipe( readPipe, readBuffer, 1024, out bytesRead, out bytesAvail, out bytesLeft ) == false )
					break;

				if( bytesRead == 1024 )
				{
					if( ReadFile( readPipe, readBuffer, 1024, out bytesRead, IntPtr.Zero ) == true && bytesRead == 1024 )
					{
						strRead = Encoding.Default.GetString(readBuffer, 0, 1024);
						strRead = strRead.Substring(0, strRead.IndexOf('\0'));
						return true;
					}
					break;
				}
			}

			strRead = "";
			return false;
		}

		private bool WriteString( string strWrite )
		{
			if( Encoding.Default.GetBytes(strWrite, 0, strWrite.Length, writeBuffer, 0 ) == 0 )
				return false;
			writeBuffer[strWrite.Length] = 0;
			uint bytesWrite;
			return WriteFile( writePipe, writeBuffer, 1024, out bytesWrite, IntPtr.Zero ) == true && bytesWrite == 1024;
		}

		private void DllBind()
		{
			try
			{
				m_Proc( m_DllInstance, readPipeChild, writePipeChild );
			}
			catch (System.Exception ex)
			{
				
			}
		}

        public bool Bind()
        {
            if (outputpath == null)
                return false;

            try
            {
				SECURITY_ATTRIBUTES attr = new SECURITY_ATTRIBUTES();
				attr.length = Marshal.SizeOf(attr);
				attr.lpSecurityDescriptor = IntPtr.Zero;
				attr.bInheritHandle = true;

				if( CreatePipe( out readPipe, out writePipeChild, ref attr, 1024 ) == false )
				{
					MessageBox.Show("CreatePipe Failed", outputpath);
					return false;
				}

				if( CreatePipe( out readPipeChild, out writePipe, ref attr, 1024 ) == false )
				{
					Unbind();
					MessageBox.Show("CreatePipe Failed", outputpath);
					return false;
				}

				switch( m_Rule.BindType )
				{
					case BIND_TYPE.Application:
					{
						STARTUPINFO si = new STARTUPINFO();
						si.cb = (uint)Marshal.SizeOf(si);
						// 				si.wShowWindow = 1;
						// 				si.dwFlags = 1;
						PROCESS_INFORMATION pi;

						if( CreateProcess( outputpath, string.Format( "{0} --vutpp:{1},{2}", outputpath, readPipeChild, writePipeChild ), IntPtr.Zero, IntPtr.Zero, true, 0x08000000, IntPtr.Zero, null, ref si, out pi ) == false )
						{
							Unbind();
							MessageBox.Show("CreateProcess Failed", outputpath);
							return false;
						}
						processChild = pi.hProcess;
					}
					break;

					case BIND_TYPE.DynamicLibrary:
					{
						try
						{
							m_DllInstance = LoadLibraryExA(outputpath, IntPtr.Zero, 8);
							if (m_DllInstance != 0)
							{
								m_Proc = (BindProc)GetProcAddress( m_DllInstance, "VUTPPBind" );
								if (m_Proc != null)
								{
									if( DllBindThread == null )
									{
										DllBindThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.DllBind));
										DllBindThread.Start();
									}
								}
								else
								{
									Unbind();
									MessageBox.Show("GetProcAddress(VUTPPBind) Failed", outputpath);
								}
							}
							else
								MessageBox.Show("LoadLibrary Failed", outputpath);
						}
						catch (System.Exception)
						{
            	
						}
					}
					break;
				}

				string strRead;
				if( ReadString( ConfigManager.Instance.ConnectWait*1000, out strRead ) == false || strRead != "connect" )
				{
					MessageBox.Show("Bind Failed(Timeout)", outputpath);

					Unbind();
					return false;
				}

				return true;
            }
            catch (System.Exception ex)
            {
				Unbind();
				MessageBox.Show(ex.ToString(), outputpath, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}

            return false;
        }

        public void Unbind()
        {
			if( processChild != 0 )
			{
				TerminateProcess( processChild, 0 );
				processChild = 0;
			}
			if( m_DllInstance != 0 )
			{
				try
				{
					if( DllBindThread != null )
					{
						DllBindThread.Abort();
						DllBindThread = null;
					}
				}
				catch (System.Exception)
				{
				}
				FreeLibrary(m_DllInstance);
				m_DllInstance = 0;
				m_Proc = null;
			}
			CloseHandle( readPipe );
			CloseHandle( writePipe );
			CloseHandle( readPipeChild );
			CloseHandle( writePipeChild );
		}

		public void Finish()
		{
			WriteString( "__VUTPP_FINISH__" );

			DateTime dt = DateTime.Now;
			
			while( DateTime.Now.Subtract( dt ) < TimeSpan.FromMilliseconds( 3000 ) )
			{
				if( CheckAlive() == false )	// timeout:258
					break;
			}
		}

        public bool Process(TreeNode node)
        {
			UnitTestBrowser.TreeNodeTag tag = (UnitTestBrowser.TreeNodeTag)node.Tag;
			if( tag.type != UnitTestBrowser.TREENODE_TYPE.TEST )
				return false;

			string suitename = "", testname = tag.key;

			if (m_Rule.SuiteType != SUITE_TYPE.NOT_USE)
			{
				UnitTestBrowser.TreeNodeTag parentTag = (UnitTestBrowser.TreeNodeTag)node.Parent.Tag;
				suitename = parentTag.key;
			}

            try
            {
				Browser.progressBar.Value++;

				string strParam = suitename+","+testname;
				if( WriteString( strParam ) == false )
					return false;

				string strResult;
				while( true )
				{
					if( ReadString( ConfigManager.Instance.TestTimeOut*1000, out strResult ) == false )
						return false;

					int seperator = strResult.IndexOf(',');
					int line = int.Parse(strResult.Substring( 0, seperator ));
					if( line == -1 )
						return true;

					int startIndex = seperator+1;
					seperator = strResult.IndexOf(',', startIndex);
					string filename = strResult.Substring( startIndex, seperator-startIndex );
					filename = filename.Replace( '/', '\\' );

					if( filename == "" )
						filename = tag.file;
					else
					{
						if( filename[0] == '.' )
						{
							filename = filename.Remove( 0, 1 );
							filename = defaultpath + filename;
						}
						else if( filename[0] == '\\' )
						{
							filename = defaultpath.Substring( 0, 2 ) + filename;
						}
					}

					string strMessage = strResult.Substring( seperator+1 );

					string code ="";

					if( line != -2 )
					{
						try
						{
							UnitTestBrowser.ParseInputFile pi = new UnitTestBrowser.ParseInputFile(filename);
							for( int lineNumber = 1; lineNumber <= line; lineNumber++ )
							{
								code = pi.GetLine();
							}
							pi.Release();
						}
						catch (System.IO.FileNotFoundException ex)
						{
							System.Diagnostics.Debug.Assert( false );
						}
					}

					TreeNode failureNode = new TreeNode(strMessage);
					Browser.SetIcon(failureNode, UnitTestBrowser.ICON_TYPE.FAIL);
					UnitTestBrowser.TreeNodeTag failureTag = new UnitTestBrowser.TreeNodeTag(UnitTestBrowser.TREENODE_TYPE.FAILURE, framework, strMessage, code.Trim(), filename, line, line);
					failureNode.Tag = failureTag;

					Browser.Add(node.Nodes, failureNode);

                    Browser.TreeNodeExpand(node);
					if( line == -2 )
						return true;
				}
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
    }
}
