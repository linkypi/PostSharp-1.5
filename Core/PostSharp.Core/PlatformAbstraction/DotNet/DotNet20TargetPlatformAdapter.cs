#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part of compile-time components of PostSharp.                *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU General Public License     *
 *   as published by the Free Software Foundation.                             *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU General Public License         *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using PostSharp.CodeModel;
using PostSharp.Extensibility;
using PostSharp.ModuleWriter;

namespace PostSharp.PlatformAbstraction.DotNet
{
    /// <summary>
    /// Implements a <see cref="TargetPlatformAdapter"/> specifically for the .NET Framework 2.0.
    /// </summary>
    [SuppressMessage( "Microsoft.Usage", "CA224:ProvideCorrectArgumentsToFormattingMethod" )]
    public class DotNet20TargetPlatformAdapter : TargetPlatformAdapter
    {

        /// <summary>
        /// Initializes a new <see cref="DotNet20TargetPlatformAdapter"/>.
        /// </summary>
        /// <param name="parameters">Collection of configured parameters.</param>
        [SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters" )]
#pragma warning disable 168
        public DotNet20TargetPlatformAdapter( NameValueCollection parameters )
#pragma warning restore 168
        {
        }

        /// <summary>
        /// Gets the name of the .NET bootstrapper.
        /// </summary>
        /// <returns>The name of the .NET bootstrapper (a valid command line).</returns>
        /// <remarks>
        /// No bootstrapper is necessary on the Microsoft implementation of .NET,
        /// but Mono requires a bootstrapper.
        /// </remarks>
        protected virtual string GetBootstrapper()
        {
            return "";
        }


        private static string BuildIlasmCommandLine( ModuleDeclaration module, AssembleOptions options )
        {
            StringBuilder commandLineBuilder = new StringBuilder( 1024 );
            commandLineBuilder.Append( "\"" );
            commandLineBuilder.Append( Path.GetFullPath( options.SourceFile ) );
            commandLineBuilder.Append( "\" " );
            commandLineBuilder.Append( "/QUIET " );
            if ( module.EntryPoint == null )
            {
                commandLineBuilder.Append( "/DLL " );
            }
            else
            {
                commandLineBuilder.Append( "/EXE " );
            }

            switch ( options.DebugOptions )
            {
                case DebugOption.Auto:
                    if ( module.HasDebugInfo )
                    {
                        commandLineBuilder.Append( "/PDB " );
                    }
                    break;

                case DebugOption.None:
                    break;

                case DebugOption.Pdb:
                    commandLineBuilder.Append( "/PDB " );
                    break;
            }

            if ( options.UnmanagedResourceFile != null )
            {
                commandLineBuilder.AppendFormat( CultureInfo.InvariantCulture,
                                                 "\"/RESOURCE={0}\" ", options.UnmanagedResourceFile );
            }

            commandLineBuilder.AppendFormat(
                CultureInfo.InvariantCulture, "\"/OUTPUT={0}\" ", Path.GetFullPath( options.TargetFile ) );
            commandLineBuilder.AppendFormat(
                CultureInfo.InvariantCulture, "/SUBSYSTEM={1} " +
                                              "/FLAGS={2} /BASE={4} /STACK={5} /ALIGNMENT={3} /MDV={6} ",
                Path.GetFullPath( options.TargetFile ),
                module.Subsystem,
                (int) module.ImageAttributes,
                module.FileAlignment,
                module.ImageBase,
                module.StackReserve,
                module.MetadataVersionString,
                module.MetadataMajorVersion,
                module.MetadataMinorVersion );

            if ( options.SignAssembly )
            {
                commandLineBuilder.AppendFormat( "\"/KEY:{0}\"", options.PrivateKeyLocation );
            }

            return commandLineBuilder.ToString();
        }

        /// <inheritdoc />
        public override void Assemble( ModuleDeclaration module, AssembleOptions options )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( module, "module" );
            ExceptionHelper.AssertArgumentNotNull( options, "options" );

            #endregion

            // Determines the location of the framework
            string frameworkDirectory = Path.GetDirectoryName( typeof(int).Assembly.Location );

            StringWriter ilOutput = new StringWriter();
            ilOutput.WriteLine();

            int exitCode = ToolInvocationHelper.InvokeTool(
                Path.Combine( frameworkDirectory, "ilasm.exe" ),
                BuildIlasmCommandLine( module, options ),
                Path.GetDirectoryName( options.SourceFile ),
                ilOutput );

            if ( exitCode != 0 )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0034",
                                                  new object[] {exitCode, ilOutput.ToString()} );
                if ( File.Exists( options.TargetFile ) )
                {
                    File.Delete( options.TargetFile );
                }
            }
        }

        private static string GetFrameworkSdkLocation()
        {
            using ( RegistryKey key = Registry.LocalMachine.OpenSubKey( @"SOFTWARE\Microsoft\.NETFramework" ) )
            {
                if ( key == null )
                {
                    return null;
                }

                object location = key.GetValue( "SDKInstallRootv2.0" );

                if ( location == null )
                {
                    return null;
                }

                return location.ToString();
            }
        }

        /// <inheritdoc />
        public override void Verify( string file )
        {
            string frameworkSdkLocation = GetFrameworkSdkLocation();
            string peVerifyLocation = frameworkSdkLocation == null
                                          ? null
                                          : Path.Combine( frameworkSdkLocation, "bin\\peverify.exe" );
            if ( peVerifyLocation == null || !File.Exists( peVerifyLocation ) )
            {
                CoreMessageSource.Instance.Write( SeverityType.Warning, "PS0067", null );
                return;
            }

            StringBuilder commandLineBuilder = new StringBuilder();
            commandLineBuilder.Append( '"' );
            commandLineBuilder.Append( file );
            commandLineBuilder.Append( "\" /NOLOGO /VERBOSE" );

            int exitCode = ToolInvocationHelper.InvokeTool(
                peVerifyLocation,
                commandLineBuilder.ToString(),
                Path.GetDirectoryName( file ),
                null );

            if ( exitCode != 0 )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0068",
                                                  new object[] {exitCode} );
            }
        }

        /// <inheritdoc />
        public override Encoding GetDefaultMsilEncoding()
        {
            return Encoding.Unicode;
        }

        /// <inheritdoc />
        public override ILWriterCompatibility GetILWriterCompatibility()
        {
            return ILWriterCompatibility.Ms;
        }
    }
}