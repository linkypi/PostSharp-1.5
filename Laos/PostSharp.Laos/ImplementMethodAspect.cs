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
using PostSharp.Extensibility;

namespace PostSharp.Laos
{
    /// <summary>
    /// Custom attribute that, when applied to a method defined in the current assembly,
    /// replaces the implementation of this method by a call to the current custom
    /// attribute.
    /// </summary>
    [MulticastAttributeUsage( MulticastTargets.Method, TargetTypeAttributes = MulticastAttributes.UserGenerated )]
    [AttributeUsage( AttributeTargets.Assembly | AttributeTargets.Class |
                     AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property |
                     AttributeTargets.Struct,
        AllowMultiple = true, Inherited = false )]
#if !SMALL
    [Serializable]
#endif
    [SuppressMessage( "Microsoft.Naming", "CA1710" /* IdentifiersShouldHaveCorrectSuffix */ )]
    public abstract class ImplementMethodAspect : LaosMethodLevelAspect, IImplementMethodAspect
#if !SMALL
, IImplementMethodAspectConfiguration
#endif
    {
        /// <inheritdoc />
        public virtual InstanceTagRequest GetInstanceTagRequest()
        {
            return null;
        }

        /// <inheritdoc />
        public abstract void OnExecution( MethodExecutionEventArgs eventArgs );

        /// <inheritdoc />
        public virtual ImplementMethodAspectOptions GetOptions()
        {
            return ImplementMethodAspectOptions.None;
        }
    }
}