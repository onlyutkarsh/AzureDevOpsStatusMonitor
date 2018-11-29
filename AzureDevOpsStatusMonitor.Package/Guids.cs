// Guids.cs
// MUST match guids.h

using System;

namespace AzureDevOpsStatusMonitor
{
    static class GuidList
    {
        public const string guidVSOStatusPkgString = "beb913a8-333c-41b1-a736-878cebad3bc6";
        public const string guidVSOStatusCmdSetString = "cde75c62-f5ad-4178-ac17-9ef5116792f6";

        public static readonly Guid guidVSOStatusCmdSet = new Guid(guidVSOStatusCmdSetString);
        public static Guid guidVSOStatusPkg = new Guid(guidVSOStatusPkgString);
    };
}