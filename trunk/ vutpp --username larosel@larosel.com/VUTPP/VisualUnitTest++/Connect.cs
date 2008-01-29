using System;
using System.Diagnostics;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Windows.Forms;

namespace VUTPP
{
	/// <summary>추가 기능을 구현하는 개체입니다.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
		/// <summary>Add-in 개체에 대한 생성자를 구현합니다. 이 메서드 안에 초기화 코드를 만드십시오.</summary>
		public Connect()
		{
		}

		/// <summary>IDTExtensibility2 인터페이스의 OnConnection 메서드를 구현합니다. 추가 기능이 로드되고 있다는 알림 메시지를 받습니다.</summary>
		/// <param term='application'>호스트 응용 프로그램의 루트 개체입니다.</param>
		/// <param term='connectMode'>추가 기능이 로드되는 방법을 설명합니다.</param>
		/// <param term='addInInst'>이 추가 기능을 나타내는 개체입니다.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;

            // Only execute the startup code if the connection mode is a startup mode
            if (connectMode == ext_ConnectMode.ext_cm_AfterStartup || connectMode == ext_ConnectMode.ext_cm_Startup)
            {
                try
                {
                    // Declare variables
                    string ctrlProgID, guidStr;
                    EnvDTE80.Windows2 toolWins;
                    object objTemp = null;

                    // The Control ProgID for the user control
                    ctrlProgID = "VUTPP.UnitTestBrowser";

                    // This guid must be unique for each different tool window,
                    // but you may use the same guid for the same tool window.
                    // This guid can be used for indexing the windows collection,
                    // for example: applicationObject.Windows.Item(guidstr)
                    guidStr = "{0E594C62-AC81-446e-B192-C4AE5CC5E2DD}";

                    // Get the executing assembly...
                    System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();

                    // Get Visual Studio's global collection of tool windows...
                    toolWins = (Windows2)_applicationObject.Windows;

                    // Create a new tool window, embedding the UnitTestBrowser control inside it...
                    m_toolWin = toolWins.CreateToolWindow2(_addInInstance, asm.Location, ctrlProgID, "VisualUnitTest++", guidStr, ref objTemp);

                    // Pass the DTE object to the user control...
                    browser = (UnitTestBrowser)objTemp;
                    browser.DTE = _applicationObject;

                    // and set the tool windows default size...
                    m_toolWin.Visible = true;		// MUST make tool window visible before using any methods or properties,

                    browser.DoRefreshTestList(false);
                    // otherwise exceptions will occurr.
                    //toolWin.Height = 400;
                    //toolWin.Width = 600;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to create the tool window: " + ex.Message, "VisualUnitTest++", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            // Create the menu item and toolbar for starting the VisualUnitTest++
            if (connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
                DTE dte = (DTE)application;
//                 string toolsMenuName;
// 
//                 try
//                 {
//                     // If you would like to move the command to a different menu, change the word  
//                     // "Tools" to the English version of the menu. This code will take the culture, 
//                     // append on the name of the menu then add the command to that menu. You can 
// //                    // find a list of all the top-level menus in the file CommandBar.resx.
// //                    ResourceManager resourceManager = new ResourceManager("VUTPP.CommandBar", Assembly.GetExecutingAssembly());
//                     CultureInfo cultureInfo = new System.Globalization.CultureInfo(dte.LocaleID);
//                     string resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools");
//                     toolsMenuName = VUTPP.CommandBar.ResourceManager.GetString(resourceName);
//                 }
//                 catch (Exception ex)
//                 {
//                     Debug.WriteLine(ex.Message);
//                     Debug.WriteLine(ex.StackTrace);
//                     // We tried to find a localized version of the word Tools, but one was not found.
//                     // Default to the en-US word, which may work for the current culture.
//                     toolsMenuName = "Tools";
//                 }
//                 // Get the command bars collection, and find the MenuBar command bar
//                 CommandBars cmdBars = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars);
//                 Microsoft.VisualStudio.CommandBars.CommandBar menuBar = cmdBars["MenuBar"];
// 
//                 // Add command to 'Tools' menu
//                 CommandBarPopup toolsPopup = (CommandBarPopup)menuBar.Controls[toolsMenuName];
//                 AddPopupCommand(toolsPopup, "VUTPP", "VisualUnitTest++", "Display the VisualUnitTest++ window.", 44);
// 
                // Add new command bar with button
                Microsoft.VisualStudio.CommandBars.CommandBar buttonBar = AddCommandBar("VisualUnitTest++ Toolbar", MsoBarPosition.msoBarFloating);
                AddToolbarCommand(buttonBar, "DisplayWindow", "Display the VisualUnitTest++ window", "Display the VisualUnitTest++ window.", 44);

                AddCommand("Stop", "Stop Running", "Stop Running", 44);
                AddCommand("RunSelected", "Run Selected Tests", "Run Selected Tests", 44);
                AddCommand("RunAll", "Run All Tests", "Run All Tests", 44);
                AddCommand("Refresh", "Refresh Tests", "Refresh Tests", 44);
            }

        }

        /// <summary>
        /// Add a command bar to the VS2005 interface.
        /// </summary>
        /// <param name="name">The name of the command bar</param>
        /// <param name="position">Initial command bar positioning</param>
        /// <returns></returns>
        private Microsoft.VisualStudio.CommandBars.CommandBar AddCommandBar(string name, MsoBarPosition position)
        {
            // Get the command bars collection
            CommandBars cmdBars = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars);
            Microsoft.VisualStudio.CommandBars.CommandBar bar = null;

            try
            {
                try
                {
                    // Create the new CommandBar
                    bar = cmdBars.Add(name, position, false, false);
                }
                catch (ArgumentException)
                {
                    // Try to find an existing CommandBar
                    bar = cmdBars[name];
                }
            }
            catch
            {
            }

            return bar;
        }

        /// <summary>
        /// Add a menu to the VS2005 interface.
        /// </summary>
        /// <param name="name">The name of the menu</param>
        /// <returns></returns>
        private Microsoft.VisualStudio.CommandBars.CommandBar AddCommandMenu(string name)
        {
            // Get the command bars collection
            CommandBars cmdBars = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars);
            Microsoft.VisualStudio.CommandBars.CommandBar menu = null;

            try
            {
                try
                {
                    // Create the new CommandBar
                    menu = cmdBars.Add(name, MsoBarPosition.msoBarPopup, false, false);
                }
                catch (ArgumentException)
                {
                    // Try to find an existing CommandBar
                    menu = cmdBars[name];
                }
            }
            catch
            {
            }

            return menu;
        }

        /// <summary>
        /// Add a command to a popup menu in VS2005.
        /// </summary>
        /// <param name="popup">The popup menu to add the command to.</param>
        /// <param name="name">The name of the new command.</param>
        /// <param name="label">The text label of the command.</param>
        /// <param name="ttip">The tooltip for the command.</param>
        /// <param name="iconIdx">The icon index, which should match the resource ID in the add-ins resource assembly.</param>
        private void AddPopupCommand(CommandBarPopup popup, string name, string label, string ttip, int iconIdx)
        {
            // Do not try to add commands to a null menu
            if (popup == null)
                return;

            // Get commands collection
            Commands2 commands = (Commands2)_applicationObject.Commands;
            object[] contextGUIDS = new object[] { };

            try
            {
                // Add command
                Command command = commands.AddNamedCommand2(_addInInstance, name, label, ttip, true, iconIdx, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);
                if ((command != null) && (popup != null))
                {
                    command.AddControl(popup.CommandBar, 1);
                }
            }
            catch (ArgumentException)
            {
                // Command already exists, so ignore
            }
        }

        /// <summary>
        /// Add a command to a toolbar in VS2005.
        /// </summary>
        /// <param name="bar">The bar to add the command to.</param>
        /// <param name="name">The name of the new command.</param>
        /// <param name="label">The text label of the command.</param>
        /// <param name="ttip">The tooltip for the command.</param>
        /// <param name="iconIdx">The icon index, which should match the resource ID in the add-ins resource assembly.</param>
        private void AddToolbarCommand(Microsoft.VisualStudio.CommandBars.CommandBar bar, string name, string label, string ttip, int iconIdx)
        {
            // Do not try to add commands to a null bar
            if (bar == null)
                return;

            // Get commands collection
            Commands2 commands = (Commands2)_applicationObject.Commands;
            object[] contextGUIDS = new object[] { };

            try
            {
                // Add command
                Command command = commands.AddNamedCommand2(_addInInstance, name, label, ttip, true, iconIdx, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePict, vsCommandControlType.vsCommandControlTypeButton);
                if (command != null && bar != null)
                {
                    command.AddControl(bar, 1);
                }
            }
            catch (ArgumentException)
            {
                // Command already exists, so ignore
            }
        }

        private void AddCommand(string name, string label, string ttip, int iconIdx)
        {
            // Get commands collection
            Commands2 commands = (Commands2)_applicationObject.Commands;
            object[] contextGUIDS = new object[] { };

            try
            {
                // Add command
                Command command = commands.AddNamedCommand2(_addInInstance, name, label, ttip, true, iconIdx, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePict, vsCommandControlType.vsCommandControlTypeButton);
            }
            catch (ArgumentException)
            {
                // Command already exists, so ignore
            }
        }
        
        /// <summary>IDTExtensibility2 인터페이스의 OnDisconnection 메서드를 구현합니다. 추가 기능이 언로드되고 있다는 알림 메시지를 받습니다.</summary>
		/// <param term='disconnectMode'>추가 기능이 언로드되는 방법을 설명합니다.</param>
		/// <param term='custom'>호스트 응용 프로그램과 관련된 매개 변수의 배열입니다.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
            if (browser != null)
            {
                browser.Release();
                browser = null;
            }
		}

		/// <summary>IDTExtensibility2 인터페이스의 OnAddInsUpdate 메서드를 구현합니다. 추가 기능의 컬렉션이 변경되면 알림 메시지를 받습니다.</summary>
		/// <param term='custom'>호스트 응용 프로그램과 관련된 매개 변수의 배열입니다.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>IDTExtensibility2 인터페이스의 OnStartupComplete 메서드를 구현합니다. 호스트 응용 프로그램에서 로드가 완료되었다는 알림 메시지를 받습니다.</summary>
		/// <param term='custom'>호스트 응용 프로그램과 관련된 매개 변수의 배열입니다.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
		}

		/// <summary>IDTExtensibility2 인터페이스의 OnBeginShutdown 메서드를 구현합니다. 호스트 응용 프로그램이 언로드되고 있다는 알림 메시지를 받습니다.</summary>
		/// <param term='custom'>호스트 응용 프로그램과 관련된 매개 변수의 배열입니다.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}

        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
        {
//            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedStatus)
            {
                // Respond only if the command name is for our menu item or toolbar button
                switch (commandName)
                {
                    case "VUTPP.Connect.DisplayWindow":
                        status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                        break;

                    default:
                        status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
//                        MessageBox.Show(commandName);
                        break;
                }
            }
        }

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                // Respond only if the command name is for our menu item or toolbar button
//                MessageBox.Show(commandName);
                switch (commandName)
                {
                    case "VUTPP.Connect.DisplayWindow":
                        if (m_toolWin != null)
                        {
                            m_toolWin.Visible = true;
                            m_toolWin.SetFocus();
                        }

                        handled = true;
                        break;

                    case "VUTPP.Connect.Stop":
                        if( browser != null )
                            browser.StopRunning();
                        handled = true;
                        break;

                    case "VUTPP.Connect.RunSelected":
                        if (browser != null)
                            browser.ExecRunSelected();
                        handled = true;
                        break;

                    case "VUTPP.Connect.RunAll":
                        if (browser != null)
                            browser.ExecRunAll();
                        handled = true;
                        break;

                    case "VUTPP.Connect.Refresh":
                        if (browser != null)
                            browser.DoRefreshTestList(false);
                        handled = true;
                        break;
               }
            }
        }

       // The tool window object
        private EnvDTE.Window m_toolWin;

        // Default cache objects (Created by wizard)
        private DTE2 _applicationObject;
		private AddIn _addInInstance;
        private UnitTestBrowser browser;
}
}