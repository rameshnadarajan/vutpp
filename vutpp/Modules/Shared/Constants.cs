using System;

namespace VUTPP
{
	/// <summary>
	/// Constants에 대한 요약 설명입니다.
	/// </summary>
	public struct Constants
	{
		public const string ProgramTitle        = "VisualUnitTest++";
		public const string ToolWindowTitle     = "VisualUnitTest++ - 0.4 by www.larosel.com";
		public const string ProgId              = "VUTPP.Connect";
		public const string CommandBarName      = "VUTPP";
		public const string MenuName            = "V&UTPP";

		public const string FrameworkNotSupported                  = "VS version {0} not supported.";

		public struct Guids
		{
			public const string guidSolutionFolder	= "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
			public const string guidVCProject		= "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}";
			public const string vsSolutionFolder	= "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
		};

		public struct Commands 
		{
			public const string UnitTestBrowser = "UnitTestBrowser";
			// 			public const string Build           = "Build";
			// 			public const string Rebuild         = "Rebuild";
			// 			public const string Save            = "Save";
			// 			public const string PrintVersions   = "PrintVersions";
			// 			public const string ExportVersions  = "ExportVersions";
			// 			public const string Configure       = "Configure";
			// 			public const string About           = "About";

		};
		public struct CommandCaptions
		{
			public const string UnitTestBrowser = "UnitTest&Browser";
		};
	}
}
