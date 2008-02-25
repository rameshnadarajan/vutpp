// Guids.cs
// MUST match guids.h
using System;

namespace larosel.VUTPP
{
    static class GuidList
    {
        public const string guidVUTPPPkgString = "a7211b0c-3cc3-4859-aaa5-40cee542daf1";
        public const string guidVUTPPCmdSetString = "1c8bda63-da87-41bc-9f09-5d65f72f3614";
        public const string guidToolWindowPersistanceString = "2e904cef-a2d5-40a0-b5e0-32377cfff2e9";

        public static readonly Guid guidVUTPPPkg = new Guid(guidVUTPPPkgString);
        public static readonly Guid guidVUTPPCmdSet = new Guid(guidVUTPPCmdSetString);
        public static readonly Guid guidToolWindowPersistance = new Guid(guidToolWindowPersistanceString);

        public const string guidSolutionFolder = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
        public const string guidVCProject = "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}";
    };
}