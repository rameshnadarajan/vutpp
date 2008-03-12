﻿// VsPkg.cs : Implementation of VUTPP
//

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace larosel.VUTPP
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the registration utility (regpkg.exe) that this class needs
    // to be registered as package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // A Visual Studio component can be registered under different regitry roots; for instance
    // when you debug your package you want to register it in the experimental hive. This
    // attribute specifies the registry root to use if no one is provided to regpkg.exe with
    // the /root switch.
    [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\8.0")]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration(true, null, null, null)]
//    [InstalledProductRegistration(false, "#110", "#112", "1.0", IconResourceID = 400)]
    // In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
    // package needs to have a valid load key (it can be requested at 
    // http://msdn.microsoft.com/vstudio/extend/). This attributes tells the shell that this 
    // package has a load key embedded in its resources.
    [ProvideLoadKey("Standard", "1.0", "VisualUnitTest++", "larosel", 104)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource(1000, 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(MyToolWindow))]
    [Guid(GuidList.guidVUTPPPkgString)]
    public sealed class VUTPP : Package, IVsInstalledProduct
    {
        const int bitmapResourceID = 300;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public VUTPP()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new COMException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            UnitTestBrowser browser = (UnitTestBrowser)window.Window;
            browser.tabControl1.SelectedTab = browser.Tests;
        }

        private void ShowConfigWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new COMException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            UnitTestBrowser browser = (UnitTestBrowser)window.Window;
            browser.tabControl1.SelectedTab = browser.Config;
        }

        private void evRunAll(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new COMException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            UnitTestBrowser browser = (UnitTestBrowser)window.Window;
            browser.ExecRunAll();
        }

        private void evRunSelected(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new COMException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            UnitTestBrowser browser = (UnitTestBrowser)window.Window;
            browser.ExecRunSelected();
        }

        private void evDebugProject(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new COMException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            UnitTestBrowser browser = (UnitTestBrowser)window.Window;
        }

        private void evDebugSelected(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new COMException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            UnitTestBrowser browser = (UnitTestBrowser)window.Window;
        }

        private void evStop(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new COMException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            UnitTestBrowser browser = (UnitTestBrowser)window.Window;
            browser.StopRunning();
        }

        private void evRefreshTestList(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new COMException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            UnitTestBrowser browser = (UnitTestBrowser)window.Window;
            browser.ExecRefreshTestList();
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .ctc file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the tool window
                MenuCommand mc = null;

                mcs.AddCommand(new MenuCommand(new EventHandler(ShowToolWindow), new CommandID(GuidList.guidVUTPPCmdSet, (int)PkgCmdIDList.cmdidUnitTestBrowser)));
                mcs.AddCommand(new MenuCommand(new EventHandler(ShowConfigWindow), new CommandID(GuidList.guidVUTPPCmdSet, (int)PkgCmdIDList.cmdidConfig)));
                mcs.AddCommand(new MenuCommand(new EventHandler(evRefreshTestList), new CommandID(GuidList.guidVUTPPCmdSet, (int)PkgCmdIDList.cmdidRefreshTestList)));
                mcs.AddCommand(new MenuCommand(new EventHandler(evRunAll), new CommandID(GuidList.guidVUTPPCmdSet, (int)PkgCmdIDList.cmdidRunAll)));
                mcs.AddCommand(new MenuCommand(new EventHandler(evRunSelected), new CommandID(GuidList.guidVUTPPCmdSet, (int)PkgCmdIDList.cmdidRunSelected)));
                mcs.AddCommand(mc = new MenuCommand(new EventHandler(evDebugProject), new CommandID(GuidList.guidVUTPPCmdSet, (int)PkgCmdIDList.cmdidDebugProject)));
                mc.Enabled = false;
                mcs.AddCommand(mc = new MenuCommand(new EventHandler(evDebugSelected), new CommandID(GuidList.guidVUTPPCmdSet, (int)PkgCmdIDList.cmdidDebugSelected)));
                mc.Enabled = false;
                mcs.AddCommand(mc = new MenuCommand(new EventHandler(evStop), new CommandID(GuidList.guidVUTPPCmdSet, (int)PkgCmdIDList.cmdidStopRun)));
                mc.Enabled = false;
            }
        }
        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            // Show a Message Box to prove we were here
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "VUTPP",
                       string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.ToString()),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result);
        }


        #region IVsInstalledProduct 멤버

        public int IdBmpSplash(out uint pIdBmp)
        {
            pIdBmp = 500;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }
        public int IdIcoLogoForAboutbox(out uint pIdIco)
        {
            pIdIco = 600;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }
        public int OfficialName(out string pbstrName)
        {
            pbstrName = "VisualUnitTest++";
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }
        public int ProductDetails(out string pbstrProductDetails)
        {
            pbstrProductDetails = Resources.ToolWindowTitle + "\r\n" + Resources.ProductDetail;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }
        public int ProductID(out string pbstrPID)
        {
            pbstrPID = "VUTPP";
            return Microsoft.VisualStudio.VSConstants.S_OK;
        } 

        #endregion
    }
}