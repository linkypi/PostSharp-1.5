#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
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

using System;
using System.IO;
using System.Reflection;

namespace PostSharp.Laos
{
#if !SMALL
    /// <summary>
    /// Base class for Laos serializers, whose role is to serialize aspect instances at compile-time and
    /// deserialize them at runtime.
    /// </summary>
    public abstract class LaosSerializer
    {
        /// <summary>
        /// Serializes an array of aspects into a stream.
        /// </summary>
        /// <param name="aspects">Array of aspects to be serialized.</param>
        /// <param name="stream">Stream into which aspects have to be serialized.</param>
        public abstract void Serialize( ILaosAspect[] aspects, Stream stream );

        /// <summary>
        /// Deserializes a stream into an array if aspects.
        /// </summary>
        /// <param name="stream">Stream containing serialized aspects.</param>
        /// <returns>An array of aspects.</returns>
        /// <remarks>
        /// The implementation is not allowed to change the order or array elements.
        /// </remarks>
        public abstract ILaosAspect[] Deserialize( Stream stream );

        /// <summary>
        /// Deserializes aspects contained in a managed resource of an assembly.
        /// </summary>
        /// <param name="assembly">Assembly containing the serialized aspects.</param>
        /// <param name="resourceName">Name of the managed resources into which aspects have been serialized.</param>
        /// <returns>An array of aspects.</returns>
        public ILaosAspect[] Deserialize( Assembly assembly, string resourceName )
        {
            Stream stream = assembly.GetManifestResourceStream( resourceName );

            if ( stream == null )
            {
                throw new InvalidProgramException(
                    string.Format( "In assembly '{0}', cannot find the resource stream '{1}' required by PostSharp.",
                                   assembly.FullName, resourceName ) );
            }
            using ( stream )
            {
                return this.Deserialize( stream );
            }
        }
    }
#endif
}