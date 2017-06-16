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
using System.IO;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Binding;
using PostSharp.CodeModel.Helpers;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Implementation of <see cref="ModuleLoadStrategy"/>
    /// loading the assembly by file location, using the <b>Assembly.LoadFile</b> method.
    /// </summary>
    [Serializable]
    public class ModuleLoadReflectionFromFileStrategy : ModuleLoadReflectionStrategy
    {
        private readonly string fileName;
        private IAssemblyName assemblyName;

        /// <summary>
        /// Initializes a new <see cref="ModuleLoadReflectionFromFileStrategy"/>.
        /// </summary>
        /// <param name="fileName">Full physical path of the assembly.</param>
        public ModuleLoadReflectionFromFileStrategy( string fileName )
        {
            ExceptionHelper.AssertArgumentNotEmptyOrNull( fileName, "fileName" );
            this.fileName = fileName;
        }

        /// <summary>
        /// Gets the full physical path of the assembly.
        /// </summary>
        public string FileName
        {
            get { return this.fileName; }
        }

        /// <inheritdoc />
        public override Assembly LoadAssembly()
        {
            try
            {
                // Look if the assembly is already loaded in memory. If yes, we won't load
                // it a second time.
                AssemblyName assemblyName = AssemblyName.GetAssemblyName( this.fileName );

                foreach ( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() )
                {
                    if ( AssemblyName.ReferenceMatchesDefinition( assembly.GetName(), assemblyName ) )
                    {
                        Trace.PostSharpObject.WriteLine( "The assembly {{{0}}} was already loaded.",
                                                         assemblyName );
                        return assembly;
                    }
                }

                return AssemblyLoadHelper.LoadAssemblyFromFile( this.fileName, this.Evidence );
            }
            catch ( IOException e )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0078",
                                                  new object[] {this.fileName, e.Message} );
                throw;
            }
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return this.fileName;
        }


        public override bool Matches(Domain domain, IAssemblyName assemblyName)
        {
            if (this.assemblyName == null)
            {
                this.assemblyName = domain.AssemblyRedirectionPolicies.GetCanonicalAssemblyName(
                    AssemblyNameWrapper.GetWrapper(AssemblyName.GetAssemblyName(this.fileName)));
            }

            return AssemblyComparer.GetInstance().Equals(this.assemblyName, assemblyName);
        }
    }
}