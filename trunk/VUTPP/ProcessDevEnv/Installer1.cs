using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using Microsoft.Win32;
using System.Diagnostics;

namespace ProcessDevEnv
{
    [RunInstaller(true)]
    public partial class Installer1 : Installer
    {
        public Installer1()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

//             {
//                 using (RegistryKey setupKey = Registry.LocalMachine.OpenSubKey(
//                       @"SOFTWARE\Microsoft\VisualStudio\7.1\Setup\VS"))
//                 {
//                     if (setupKey != null)
//                     {
//                         string devenv = setupKey.GetValue("EnvironmentPath").ToString();
//                         if (!string.IsNullOrEmpty(devenv))
//                         {
//                             Process.Start(devenv, "/setup").WaitForExit();
//                         }
//                     }
//                 }
//             }
            {
                using (RegistryKey setupKey = Registry.LocalMachine.OpenSubKey(
                      @"SOFTWARE\Microsoft\VisualStudio\8.0\Setup\VS"))
                {
                    if (setupKey != null)
                    {
                        string devenv = setupKey.GetValue("EnvironmentPath").ToString();
                        if (!string.IsNullOrEmpty(devenv))
                        {
                            Process.Start(devenv, "/setup").WaitForExit();
                        }
                    }
                }
            }
            {
                using (RegistryKey setupKey = Registry.LocalMachine.OpenSubKey(
                      @"SOFTWARE\Microsoft\VisualStudio\9.0\Setup\VS"))
                {
                    if (setupKey != null)
                    {
                        string devenv = setupKey.GetValue("EnvironmentPath").ToString();
                        if (!string.IsNullOrEmpty(devenv))
                        {
                            Process.Start(devenv, "/setup /nosetupvstemplates").WaitForExit();
                        }
                    }
                }
            }
        }
    }
}