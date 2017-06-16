using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
#if SL
[assembly : AssemblyTitle("PostSharp.Laos.SL")]
#else
#if CF
[assembly : AssemblyTitle("PostSharp.Laos.CF")]
#else

[assembly: AssemblyTitle( "PostSharp.Laos" )]
#endif
#endif

[assembly: AssemblyDescription( "Public Interface of PostSharp Laos, a high-level AOP framework for .NET." )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "SharpCrafters s.r.o." )]
[assembly: AssemblyProduct( "PostSharp" )]
[assembly: AssemblyCopyright( "Copyright (c) 2004-2010 by SharpCrafters s.r.o." )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible( false )]
[assembly: CLSCompliant( true )]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid( "686cd1f3-1d7b-497a-b0e2-522694d4aafc" )]

// Hint for NGen
#if !SMALL

[assembly: DefaultDependency( LoadHint.Always )]
[assembly: AllowPartiallyTrustedCallers]
#endif

// Make PostSharp.Laos.Weaver a friend assembly.

[assembly:
    InternalsVisibleTo(
        "PostSharp.Laos.Weaver, PublicKey=002400000480000094000000060200000024000052534131000400000100010029a210ad0342b29ebf859a2b9bacfda9bc786bc6a8de8ad14c0fe24b5645188b3595cad82b3d9a3c1319e2abe49ab0ffeed8358d16fef234af601315f39258539c9006391a699ca15542a0595df441f039a5f411b0b3138a2a472f63043a49ebb45118b1649da88bc59f295ad58801a5f9100fcf3091eaea883d17811edc49a8"
        )]