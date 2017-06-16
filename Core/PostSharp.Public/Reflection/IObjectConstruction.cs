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

#if !SMALL
using System.Reflection;

namespace PostSharp.Reflection
{
    /// <summary>
    /// Specifies a simple way to build an object by specifying its type,
    /// the parameters of its constructor, and values of fields or properties.
    /// </summary>
    /// <remarks>
    /// This interface has similar semantics as the <see cref="CustomAttributeData"/>
    /// class.
    /// </remarks>
    /// <seealso cref="ObjectConstruction"/>
    public interface IObjectConstruction
    {
        /// <summary>
        /// Gets the assembly-qualified type name of the object.
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Gets the number of constructor arguments.
        /// </summary>
        int ConstructorArgumentCount { get; }

        /// <summary>
        /// Gets a constructor argument.
        /// </summary>
        /// <param name="index">Ordinal of the constructor argument.</param>
        /// <returns>The constructor argument at position <paramref name="index"/>.</returns>
        object GetConstructorArgument( int index );

        /// <summary>
        /// Gets an array containing the name of initialized properties of fields.
        /// </summary>
        /// <returns>An array containing the name of initialized properties of fields</returns>
        string[] GetPropertyNames();

        /// <summary>
        /// Gets the value to which a given property of field is initialized.
        /// </summary>
        /// <param name="name">Name of the property or field.</param>
        /// <returns>The value to which the property or field named <paramref name="name"/>
        /// is initialized.</returns>
        object GetPropertyValue( string name );
    }
}

#endif