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
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace PostSharp.Laos
{
    /// <summary>
    /// Base class for aspects that can be woven.
    /// </summary>
    /// <remarks>
    /// Combined aspects, for instance, are not woven in themselves, and
    /// are not derived from this class.
    /// </remarks>
    /// <seealso cref="ILaosWeavableAspect"/>
#if !SMALL
    [Serializable]
#endif
    [SuppressMessage( "Microsoft.Naming", "CA1710" /* IdentifiersShouldHaveCorrectSuffix */ )]
    public abstract class LaosWeavableAspect : LaosAspect, ILaosWeavableAspect
#if !SMALL
                                               , ILaosWeavableAspectConfiguration
#endif
    {
#if !SMALL
        [NonSerialized] private int aspectPriority;

        /// <summary>
        /// Gets the weaving priority of the aspect.
        /// </summary>
        [XmlIgnore]
        public int AspectPriority
        {
            get { return aspectPriority; }
            set { aspectPriority = value; }
        }

        /// <inheritdoc />
        public virtual Type SerializerType
        {
            get { return null; }
        }

        /// <inheritdoc />
        int? ILaosWeavableAspectConfiguration.AspectPriority
        {
            get { return this.AspectPriority; }
        }
#endif
    }
}