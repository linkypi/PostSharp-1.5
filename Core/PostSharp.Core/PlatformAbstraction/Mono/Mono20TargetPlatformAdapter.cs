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

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using PostSharp.CodeModel;
using PostSharp.Extensibility;
using PostSharp.ModuleWriter;
using PostSharp.PlatformAbstraction.DotNet;

namespace PostSharp.PlatformAbstraction.Mono
{
    /// <summary>
    /// Implementation of <see cref="TargetPlatformAdapter"/> for Mono 2.0.
    /// </summary>
    public class Mono20TargetPlatformAdapter : TargetPlatformAdapter
    {
        private readonly string ilasmPath;

        /// <summary>
        /// Initializes a new instance of <see cref="Mono20TargetPlatformAdapter"/>.
        /// </summary>
        /// <param name="parameters">Collection of parameters.</param>
        public Mono20TargetPlatformAdapter( NameValueCollection parameters )
        {
            this.ilasmPath = parameters["IlasmPath"];
        }

        /// <inheritdoc />
        public override ILWriterCompatibility GetILWriterCompatibility()
        {
            return ILWriterCompatibility.Mono;
        }

        /// <inheritdoc />
        private static string BuildIlasmCommandLine( ModuleDeclaration module, AssembleOptions options )
        {
            StringBuilder commandLineBuilder = new StringBuilder( 1024 );
            commandLineBuilder.Append( "\"" );
            commandLineBuilder.Append( Path.GetFullPath( options.SourceFile ) );
            commandLineBuilder.Append( "\" " );
            //commandLineBuilder.Append("/QUIET ");
            if ( module.EntryPoint == null )
            {
                commandLineBuilder.Append( "/DLL " );
            }
            else
            {
                commandLineBuilder.Append( "/EXE " );
            }

            /*
            switch (options.DebugOptions)
            {
                case DebugOption.Auto:
                    if (module.HasDebugInfo)
                    {
                        commandLineBuilder.Append("/DEBUG ");
                    }
                    break;

                case DebugOption.Debug:
                    commandLineBuilder.Append("/DEBUG ");
                    break;

                case DebugOption.DebugExplicit:
                    commandLineBuilder.Append("/DEBUG=IMPL ");
                    break;

                case DebugOption.DebugImplicit:
                    commandLineBuilder.Append("/DEBUG=OPT ");
                    break;

                case DebugOption.None:
                    break;

                case DebugOption.Pdb:
                    commandLineBuilder.Append("/PDB ");
                    break;
            }
             */

            if ( options.UnmanagedResourceFile != null )
            {
                commandLineBuilder.AppendFormat( CultureInfo.InvariantCulture,
                                                 "\"/RESOURCE={0}\" ", options.UnmanagedResourceFile );
            }

            commandLineBuilder.AppendFormat(
                CultureInfo.InvariantCulture, "\"/OUTPUT={0}\" ", Path.GetFullPath( options.TargetFile ) );
            /*
            commandLineBuilder.AppendFormat(
                CultureInfo.InvariantCulture, "/SUBSYSTEM={1} " +
                                              "/FLAGS={2} /BASE={4} /STACK={5} /ALIGNMENT={3} /MDV={6} ",
                Path.GetFullPath(options.TargetFile),
                module.Subsystem,
                (int)module.ImageAttributes,
                module.FileAlignment,
                module.ImageBase,
                module.StackReserve,
                module.MetadataVersionString,
                module.MetadataMajorVersion,
                module.MetadataMinorVersion);
            */

            if ( options.SignAssembly )
            {
                commandLineBuilder.AppendFormat( "\"/KEY:{0}\"", options.PrivateKeyLocation );
            }

            return commandLineBuilder.ToString();
        }

        /// <inheritdoc />
        public override void Assemble( ModuleDeclaration module, AssembleOptions options )
        {
            // The bootstrapper should be the main module of the current platform.

            StringWriter ilOutput = new StringWriter();
            ilOutput.WriteLine();
            int exitCode = ToolInvocationHelper.InvokeTool(
                this.ilasmPath,
                BuildIlasmCommandLine( module, options ),
                Path.GetDirectoryName( options.SourceFile ),
                ilOutput);

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

        /// <inheritdoc />
        public override void Verify( string file )
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc />
        public override Encoding GetDefaultMsilEncoding()
        {
            return Encoding.ASCII;
        }
    }
}