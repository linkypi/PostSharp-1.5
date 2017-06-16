#region Using directives

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Permissions;

#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly : AssemblyTitle("PostSharp")]
[assembly : AssemblyDescription("PostSharp Command Line Utility")]
#if DEBUG
[assembly : AssemblyConfiguration("Debug")]
#endif

[assembly: AssemblyCompany( "SharpCrafters s.r.o." )]
[assembly : AssemblyProduct("PostSharp")]
[assembly: AssemblyCopyright("Copyright (c) 2004-2010 by SharpCrafters s.r.o.")]
[assembly: AssemblyTrademark("")]
[assembly : AssemblyCulture("")]
[assembly : PermissionSet(SecurityAction.RequestMinimum, Name = "FullTrust")]

// Hint for NGen
[assembly : DefaultDependency(LoadHint.Always)]
