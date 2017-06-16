﻿#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.
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

namespace PostSharp.Laos
{
    /// <summary>
    /// Utility methods for PostSharp Laos.
    /// </summary>
    public sealed class LaosUtils
    {
        /// <summary>
        /// Gets the <see cref="InstanceCredentials"/> of the calling instance. This method must be
        /// invoked from an instance method (not a static method) of a type that has been enhanced
        /// by an aspect.
        /// </summary>
        /// <returns>The <see cref="InstanceCredentials"/> of the calling instance.</returns>
        /// <remarks>
        /// Calls to this method are transformed, at build time, to calls to
        /// <b>this.GetInstanceCredentials</b>, a method that is typically generated by PostSharp. 
        /// This is why the current method has actually no
        /// implementation.
        /// </remarks>
        public static InstanceCredentials GetCurrentInstanceCredentials()
        {
            throw new NotSupportedException("The caller of this method should be transformed by PostSharp.");
        }

        
        /// <summary>
        /// Initializes the all the aspects of the calling instance. This method must be
        /// invoked from an instance method (not a static method) of a type that has been enhanced
        /// by an aspect.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Calls to this method are transformed, at build time, to calls to
        /// <b>this.InitializeAspects</b>, a method that is typically generated by PostSharp. 
        /// This is why the current method has actually no implementation.
        /// </para>
        /// <para>
        /// The constructors of enhanced classes always initialize aspects. The only scenario
        /// where this method needs to be invoked manually is when instances are not built
        /// using the constructor, but for instance with the method <see cref="System.Runtime.Serialization.FormatterServices.GetUninitializedObject"/>.
        /// </para>
        /// </remarks>
        public static void InitializeCurrentAspects()
        {
            throw new NotSupportedException("The caller of this method should be transformed by PostSharp.");
        }
    }
}
