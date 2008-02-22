// Guids.cs
// MUST match guids.h
using System;

namespace Tnrsoft.VUTPP
{
    static class GuidList
    {
        public const string guidVUTPPPkgString = "dab173a7-1c0b-42cd-9201-8fac7ac03ec6";
        public const string guidVUTPPCmdSetString = "5a6b0d64-2ee6-4e89-a96a-c88479cd820f";
        public const string guidToolWindowPersistanceString = "68c8acd0-029b-4d1d-ba79-bb84edcca834";

        public static readonly Guid guidVUTPPCmdSet = new Guid(guidVUTPPCmdSetString);
    };
}