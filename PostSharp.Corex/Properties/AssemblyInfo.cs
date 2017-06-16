#region Using directives

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;

#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle( "PostSharp.Core" )]
[assembly: AssemblyDescription( "PostSharp Core Library" )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "SharpCrafters s.r.o." )]
[assembly: AssemblyProduct( "PostSharp" )]
[assembly: AssemblyCopyright( "Copyright (c) 2004-2010 by SharpCrafters s.r.o." )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]
[assembly: ComVisible( false )]
[assembly: CLSCompliant( false )]

// Security: require unsafe code, i.e. full trust

[assembly: PermissionSet( SecurityAction.RequestMinimum, Name = "FullTrust" )]
[assembly: ReliabilityContract( Consistency.MayCorruptInstance,
    Cer.None )]


// Disable error: AvoidNamespacesWithFewTypes

[assembly:
    SuppressMessage( "Microsoft.Design", "CA1020", Scope = "namespace", Target = "PostSharp.Collections.Specialized" )
]


// Hint for NGen

[assembly: DefaultDependency( LoadHint.Always )]

#if DEBUG

[assembly:
    Debuggable( DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.Default )]
#endif

[assembly: NeutralResourcesLanguage( "en-US" )]

// Make PostSharp.PlatformAbstraction.* a friend assembly.

[assembly:
    InternalsVisibleTo(
        "PostSharp.PlatformAbstraction.DotNet, PublicKey=002400000480000094000000060200000024000052534131000400000100010029a210ad0342b29ebf859a2b9bacfda9bc786bc6a8de8ad14c0fe24b5645188b3595cad82b3d9a3c1319e2abe49ab0ffeed8358d16fef234af601315f39258539c9006391a699ca15542a0595df441f039a5f411b0b3138a2a472f63043a49ebb45118b1649da88bc59f295ad58801a5f9100fcf3091eaea883d17811edc49a8"
        )]
