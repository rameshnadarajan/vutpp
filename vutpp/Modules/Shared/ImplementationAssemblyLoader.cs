using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace VUTPP
{
	public struct ImplementationAssemblyLoader 
	{

		private const string AddinImplementationAssemblyBasename    = "AddinImplementation.";

		public static Assembly LoadMainAssembly(string runtimeVersion) 
		{
			string addinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			StringBuilder mainAssemblyFile = new StringBuilder(Path.Combine(addinPath, AddinImplementationAssemblyBasename));
			switch( runtimeVersion )
			{
				case "7.10":
					mainAssemblyFile.Append("VS2003");
					break;

				case "8.0":
					mainAssemblyFile.Append("VS2005");
					break;

				case "9.0":
					mainAssemblyFile.Append("VS2008");
					break;

				case "10.0":
					mainAssemblyFile.Append("VS2010");
					break;

				default:
					throw new ArgumentOutOfRangeException("runtimeVersion", string.Format(Constants.FrameworkNotSupported, runtimeVersion));
			}mainAssemblyFile.Append(".dll");
			return Assembly.LoadFrom(mainAssemblyFile.ToString());
		}
	}
}
