#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part; of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion

#if !SMALL

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace PostSharp.Laos.Serializers
{
    /// <summary>
    /// Implementation of <see cref="SerializationBinder"/> used at runtime when aspect instances
    /// are deserialized. By overriding the default binder, you can resolve assembly names differently.
    /// This can be useful if assemblies have been renamed or merged between PostSharp run and execution
    /// </summary>
    /// <example>
    /// LaosSerializationBinder binder = new LaosSerializationBinder();
    /// binder.Retarget("MyAssembly", "MyAssemblyMerged");
    /// BinaryLaosSerializer.Binder = binder;
    /// </example>
    public class BinaryLaosSerializationBinder : SerializationBinder
    {
        private readonly Dictionary<string, Assembly> assemblyCache = new Dictionary<string, Assembly>();
        private readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

        private readonly Dictionary<string, string> policies =
            new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );

       

        /// <summary>
        /// Retarget an assembly name to a new one.
        /// </summary>
        /// <param name="oldAssemblyName">Old assembly name (in short form only).</param>
        /// <param name="newAssemblyName">New assembly name (in short or full form).</param>
        public void Retarget( string oldAssemblyName, string newAssemblyName )
        {
            if ( string.IsNullOrEmpty( oldAssemblyName ) ) throw new ArgumentNullException( "oldAssemblyName" );
            if ( string.IsNullOrEmpty( newAssemblyName ) ) throw new ArgumentNullException( "newAssemblyName" );
            policies.Add( oldAssemblyName, newAssemblyName );
        }

        /// <inheritdoc />
        public override Type BindToType( string assemblyName, string typeName )
        {
            string fullTypeName = typeName + ", " + assemblyName;
            Type type;
            if ( !this.typeCache.TryGetValue( fullTypeName, out type ) )
            {
                Assembly assembly;
                if ( !this.assemblyCache.TryGetValue( assemblyName, out assembly ) )
                {
                    try
                    {
                        assembly = Assembly.Load( assemblyName );
                    }
                    catch ( FileNotFoundException )
                    {
                        string newAssemblyName;
                        if ( policies.TryGetValue( new AssemblyName( assemblyName ).Name, out newAssemblyName ) )
                        {
                            assembly = Assembly.Load( newAssemblyName );
                        }
                        else throw;
                    }
                    this.assemblyCache.Add( assemblyName, assembly );
                }

                type = assembly.GetType( typeName );
                this.typeCache.Add( fullTypeName, type );
            }

            return type;
        }
    }
}

#endif