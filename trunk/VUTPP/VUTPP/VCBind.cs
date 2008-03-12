using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using System.Reflection;

namespace larosel.VUTPP
{
    class VCBind
    {
        static private Type BindType = null;

        static private bool Bind()
        {
            if (BindType != null)
                return true;

            string path = Assembly.GetExecutingAssembly().Location;
            path = path.Substring(0, path.LastIndexOf('\\')+1 );

            DTE dte = (EnvDTE.DTE)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE));
            if (dte == null)
                return false;

            Assembly assem = null;
            switch (dte.Version)
            {
                case "8.0":
                    assem = Assembly.LoadFrom(path + "VCBind2005.dll");
                    break;

                case "9.0":
                    assem = Assembly.LoadFrom(path + "VCBind2008.dll");
                    break;
            }
            if (assem == null)
                return false;
            BindType = assem.GetType("VCBind");
            return BindType != null;
        }

        static private object RunMethod(string name, object [] parameters)
        {
            if (Bind() == false)
                return null;

            MethodInfo mi = BindType.GetMethod(name);
            if (mi == null)
                return null;

            try
            {
                return mi.Invoke(null, parameters);
            }
            catch (System.Exception)
            {
            	
            }
            return null;
        }

        static public string GetPreprocessorDefinitions(Project project)
        {
            return (string)RunMethod("GetPreprocessorDefinitions", new object[] { project });
        }

        static public string GetPrimaryOutput(Project project, string ActiveConfigurationName)
        {
            return (string)RunMethod("GetPrimaryOutput", new object[] { project, ActiveConfigurationName });
        }
    }
}
