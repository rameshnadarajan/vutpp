using EnvDTE;

#if !VS2003
using EnvDTE80;
using DTE = EnvDTE80.DTE2;
#endif

using Extensibility;

#if VS2003
using Microsoft.Office.Core;
using System.Reflection;
#else
using Microsoft.VisualStudio.CommandBars;
#endif

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace VUTPP
{
	/// <summary>
	/// Main에 대한 요약 설명입니다.
	/// </summary>
	public class VUTPPMain : IAddin, Extensibility.IDTExtensibility2, IDTCommandTarget 
	{
		#region Constructor
		public VUTPPMain()
		{
			//
			// TODO: 여기에 생성자 논리를 추가합니다.
			//
		}
		#endregion // Constructor

		#region Public methods

		public void OnConnection(object application, Extensibility.ext_ConnectMode connectMode, object addInInst, ref System.Array custom) 
		{
//            Trace.Write("OnConnection");
            Debug.Assert(application != null);
			m_devEnvApplicationObject = (DTE)application;
			m_addInInst = (AddIn)addInInst;
			try 
			{
				SetCurrentThreadUICulture();
			}
			catch (Exception e) 
			{
				MessageBox.Show(e.ToString(), Constants.ProgId, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
//			Trace.Write("before connectMode");
			if (connectMode == Extensibility.ext_ConnectMode.ext_cm_UISetup) 
			{
//				Trace.Write("ext_cm_UISetup");

				try 
				{
					CreateVUTPPMenu();
				}
				catch (Exception e) 
				{
					MessageBox.Show(e.ToString(), Constants.ProgramTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
//				Trace.Write("ext_cm_UISetup finish");
			}
			// Only execute the startup code if the connection mode is a startup mode
			if (connectMode == ext_ConnectMode.ext_cm_AfterStartup || connectMode == ext_ConnectMode.ext_cm_Startup)
			{
				try
				{
					// Declare variables
					string guidStr = "{F5475F21-5884-40bf-834E-1155558D5749}";

					// Get the executing assembly...
					System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();

					// Get Visual Studio's global collection of tool windows...
					// Create a new tool window, embedding the UnitTestBrowser control inside it...

#if VS2003
					browser = new UnitTestBrowser();
                    m_toolWin = CreateToolWindow(browser, Constants.ToolWindowTitle, guidStr);
#else
					// The Control ProgID for the user control
					string ctrlProgID = "VUTPP.UnitTestBrowser";

					object objTemp = null;
					EnvDTE80.Windows2 toolWins = (Windows2)m_devEnvApplicationObject.Windows;
					m_toolWin = toolWins.CreateToolWindow2(m_addInInst, asm.Location, ctrlProgID, Constants.ToolWindowTitle, guidStr, ref objTemp);
					browser = (UnitTestBrowser)objTemp;
#endif
					object icon = ToolWindowUtil.LoadTabIcon( "icon.bmp", m_devEnvApplicationObject.Version );
					m_toolWin.SetTabPicture( icon );

					// Pass the DTE object to the user control...
					browser.DTE = m_devEnvApplicationObject;
					browser.ToolWindow = m_toolWin;

					// and set the tool windows default size...
					m_toolWin.Visible = true;		// MUST make tool window visible before using any methods or properties,

					browser.DoRefreshTestList(false);
					// otherwise exceptions will occurr.
					//toolWin.Height = 400;
					//toolWin.Width = 600;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString(), Constants.ProgramTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}

//			Trace.Write("OnConnection finish");
		}

		/// <summary>
		///   Implements the OnDisconnection method of the IDTExtensibility2 interface.
		///   Receives notification that the Add-in is being unloaded.
		/// </summary>
		/// <param term='disconnectMode'>
		///   Describes how the Add-in is being unloaded.
		/// </param>
		/// <param term='custom'>
		///   Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(Extensibility.ext_DisconnectMode disconnectMode, ref System.Array custom) 
		{
		}

		/// <summary>
		///   Implements the OnAddInsUpdate method of the IDTExtensibility2 interface.
		///   Receives notification that the collection of Add-ins has changed.
		/// </summary>
		/// <param term='custom'>
		///   Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnAddInsUpdate(ref System.Array custom) 
		{
		}

		/// <summary>
		///   Implements the OnStartupComplete method of the IDTExtensibility2 interface.
		///   Receives notification that the host application has completed loading.
		/// </summary>
		/// <param term='custom'>
		///   Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref System.Array custom) 
		{
			UpdateAddinControlsStates();
		}

		/// <summary>
		///   Implements the OnBeginShutdown method of the IDTExtensibility2 interface.
		///   Receives notification that the host application is being unloaded.
		/// </summary>
		/// <param term='custom'>
		///   Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref System.Array custom) 
		{
		}
		
		/// <summary>
		///   Implements the QueryStatus method of the IDTCommandTarget interface.
		///   This is called when the command's availability is updated
		/// </summary>
		/// <param term='commandName'>
		///	  The name of the command to determine state for.
		/// </param>
		/// <param term='neededText'>
		///	  Text that is needed for the command.
		/// </param>
		/// <param term='status'>
		///	  The state of the command in the user interface.
		/// </param>
		/// <param term='commandText'>
		///	  Text requested by the neededText parameter.
		/// </param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText) 
		{
			status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
		}

		/// <summary>
		///   Implements the Exec method of the IDTCommandTarget interface.
		///   This is called when the command is invoked.
		/// </summary>
		/// <param term='commandName'>
		///	  The name of the command to execute.
		/// </param>
		/// <param term='executeOption'>
		///	  Describes how the command should be run.
		/// </param>
		/// <param term='varIn'>
		///	  Parameters passed from the caller to the command handler.
		/// </param>
		/// <param term='varOut'>
		///	  Parameters passed from the command handler to the caller.
		/// </param>
		/// <param term='handled'>
		///	  Informs the caller if the command was handled or not.
		/// </param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled) 
		{
			handled = false;
			if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault) 
			{
				try 
				{
					switch( commandName.Substring( m_addInInst.ProgID.Length+1 ) )
					{
						case Constants.Commands.UnitTestBrowser:
							m_toolWin.Visible = true;
							browser.Visible = true;
							browser.tabControl1.SelectedTab = browser.Tests;
							handled = true;
							break;
					}
				}
				catch (Exception exception) 
				{
					MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace.ToString(), "Error starting VisualUnitTest++ add-in", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		#endregion // Public methods

		#region Private methods
		private void SetCurrentThreadUICulture() 
		{
			Debug.Assert(m_devEnvApplicationObject != null);
			int lcid = 0;
			// first try to obtain language from "Environment"-"International", "Language";
			// this works with VS2005 beta, but will throw exception with VS2002
			try 
			{
				lcid = (int)m_devEnvApplicationObject.get_Properties(ENVIRONMENT, INTERNATIONAL).Item(LANGUAGE).Value;
			}
			catch (Exception) 
			{
				// if the above fails, try "Environment"-"Help", "Preferred Language"
				try 
				{
					lcid = (int)m_devEnvApplicationObject.get_Properties(ENVIRONMENT, HELP).Item(PREFERREDLANGUAGE).Value;
				}
				catch (Exception) 
				{
					// if both failed, leave it as is
					return;
				}
			}
			System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(lcid);
		}

		private bool DeleteVUTPPMenu()
		{
			foreach (CommandBarControl cbc in ((CommandBars)m_devEnvApplicationObject.CommandBars).ActiveMenuBar.Controls) 
			{
				if (cbc.Caption == Constants.MenuName) 
				{
					try
					{
						cbc.Delete(false);
					}
					catch (System.Exception)
					{
						
					}
					return true;
				}
			}
			return false;
		}

		private bool CreateVUTPPMenu() 
		{
//            Trace.Write("CreateVUTPPMenu");
			Debug.Assert(m_devEnvApplicationObject != null);

			while( DeleteVUTPPMenu() == true );

			Commands commands = m_devEnvApplicationObject.Commands;
			foreach(Command command in commands)
			{
				if(command.Name != null && command.Name.IndexOf("VUTPP") != -1)
				{
					try
					{
						command.Delete();
					}
					catch(Exception)
					{
					}
				}
			}

//            if (m_vutppMenu == null)
            {
//                Trace.Write("create m_vutppMenu");
                int index = ((CommandBars)m_devEnvApplicationObject.CommandBars).ActiveMenuBar.Controls.Count + 1;
                CommandBar vutppMenu = (CommandBar)m_devEnvApplicationObject.Commands.AddCommandBar(Constants.MenuName, vsCommandBarType.vsCommandBarTypeMenu, ((CommandBars)m_devEnvApplicationObject.CommandBars).ActiveMenuBar, index);
                m_vutppMenu = (CommandBarPopup)((CommandBars)m_devEnvApplicationObject.CommandBars).ActiveMenuBar.Controls[index].Control;
                Debug.Assert(m_vutppMenu != null);

				AddCommand((AddIn)m_addInInst, Constants.Commands.UnitTestBrowser, Constants.CommandCaptions.UnitTestBrowser, Constants.CommandCaptions.UnitTestBrowser, true, 0, false);
			}
            m_vutppMenu.Enabled = true;
            m_vutppMenu.Visible = true;
			UpdateAddinControlsStates();

//			Trace.Write("CreateVUTPPMenu Finish");
            return true;
		}

		private void AddCommand(AddIn addInInst, string name, string buttonText, string toolTip, bool msoButton, int bitmapId, bool beginGroup) 
		{
			object[] contextGUIDS = new object[] { };
			// search if command already exists in commands collection
			Commands commands = m_devEnvApplicationObject.Commands;
			Command command = null;
			string fullName = addInInst.ProgID + "." + name;
			try 
			{
				command = commands.Item(fullName, 0);
			}
			catch 
			{
			}
			// if command has not been found, add it
			if (command == null) 
			{
				command = commands.AddNamedCommand(addInInst, name, buttonText, toolTip, msoButton, bitmapId, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported+(int)vsCommandStatus.vsCommandStatusEnabled);
			}
			AddCommandToMenubar(command, buttonText, toolTip, beginGroup);
		}

		private void AddCommandToMenubar(Command command, string buttonText, string toolTip, bool beginGroup) 
		{
			Debug.Assert(command != null);
			Debug.Assert(buttonText != null && buttonText.Length > 0);
			Debug.Assert(toolTip != null && toolTip.Length > 0);
			Debug.Assert(m_vutppMenu != null);

			_CommandBars commandBars = (_CommandBars)m_devEnvApplicationObject.CommandBars;
			CommandBar menuBar = commandBars.ActiveMenuBar;
			// search if command is already available
			CommandBarControl menuItem = menuBar.FindControl(MsoControlType.msoControlButton, MissingValue, command.Name, MissingValue, true);
			if (menuItem == null) 
			{
				menuItem = AppendControl(command, buttonText, toolTip, m_vutppMenu.CommandBar);
			}
			else
				menuItem.Visible = true;
			if (beginGroup)
				menuItem.BeginGroup = true;
			Debug.Assert(menuItem != null);
		}

		private CommandBarButton AppendControl(Command command, string buttonText, string toolTip, CommandBar commandBar) 
		{
			Debug.Assert(command != null);
			Debug.Assert(buttonText != null && buttonText.Length > 0);
			Debug.Assert(toolTip != null && toolTip.Length > 0);
			Debug.Assert(commandBar != null);
			CommandBarButton commandBarButton = (CommandBarButton)command.AddControl(commandBar, commandBar.Controls.Count + 1);
			Debug.Assert(commandBarButton != null);
			commandBarButton.Tag = command.Name;
			commandBarButton.Caption = buttonText;
			commandBarButton.TooltipText = toolTip;
			return commandBarButton;
		}

		/// <summary>
		///   Updates states of the addin control(s).
		/// </summary>
		private void UpdateAddinControlsStates() 
		{
			vsCommandStatus commandStatus = vsCommandStatus.vsCommandStatusUnsupported;
			object commandText = null;
			QueryStatus(Constants.Commands.UnitTestBrowser, vsCommandStatusTextWanted.vsCommandStatusTextWantedNone, ref commandStatus, ref commandText);
		}

#if VS2003
		private Window CreateToolWindow(UserControl control, string caption, string guid)
		{
			int defaultHeight = control.Height;
			int defaultWidth = control.Width;

			object obj = null;
			Window window = m_devEnvApplicationObject.Windows.CreateToolWindow(m_addInInst, "VUTPPUserControlHost.VUTPPUserControlHostCtl", caption, guid, ref obj);
			window.Visible = true;
			obj.GetType().InvokeMember("HostUserControl", BindingFlags.InvokeMethod, null, obj, new object[] {control});

			try
			{
				window.Width = defaultWidth;
				window.Height = defaultHeight;
			} 
			catch {} // if the toolwindow has a remembered position, it will throw an exception setting the width

			return window;
		}
#endif
		#endregion // Private methods

		#region Fields

		// Development environment object.
		private DTE m_devEnvApplicationObject;
		private AddIn m_addInInst;
		private CommandBarPopup	m_vutppMenu = null;
		private static readonly object MissingValue = System.Reflection.Missing.Value;

		private const string ENVIRONMENT        = "Environment";
		private const string HELP               = "Help";
		private const string INTERNATIONAL      = "International";
		private const string LANGUAGE           = "Language";
		private const string PREFERREDLANGUAGE  = "PreferredLanguage";

		private EnvDTE.Window m_toolWin;
		private VUTPP.UnitTestBrowser browser;

		#endregion // Private Fields
	}
}
