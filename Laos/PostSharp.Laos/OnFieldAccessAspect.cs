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
    /// Custom attribute that, when applied to a field defined in the current assembly, intercepts
    /// all <i>get</i> and <i>set</i> operations on this field. 
    /// This custom attribute can be multicasted (see <see cref="MulticastAttribute"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This aspect can be applied only to fields defined inside the current assembly. 
    /// It is recommended that you apply this attribute only to fields with <b>private</b> 
    /// or <b>internal</b> visibility.
    /// </para>
    /// </remarks>
#if !SMALL
    [Serializable]
#endif
    [AttributeUsage(
        AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field,
        AllowMultiple = true, Inherited = false )]
    [MulticastAttributeUsage( MulticastTargets.Field, TargetTypeAttributes = MulticastAttributes.UserGenerated,
        TargetMemberAttributes = MulticastAttributes.NonLiteral, AllowMultiple = true )]
    [SuppressMessage( "Microsoft.Naming", "CA1710" /* IdentifiersShouldHaveCorrectSuffix */ )]
    public abstract class OnFieldAccessAspect : LaosFieldLevelAspect, IOnFieldAccessAspect
#if !SMALL
, IOnFieldAccessAspectConfiguration
#endif
    {
        /// <inheritdoc />
        public virtual void OnGetValue( FieldAccessEventArgs eventArgs )
        {
        }

        /// <inheritdoc />
        public virtual void OnSetValue( FieldAccessEventArgs eventArgs )
        {
            eventArgs.StoredFieldValue = eventArgs.ExposedFieldValue;
        }

#if !SMALL
        /// <inheritdoc />
        [CompileTimeSemantic]
        public virtual InstanceTagRequest GetInstanceTagRequest()
        {
            return null;
        }

        /// <inheritdoc />
        [CompileTimeSemantic]
        public virtual OnFieldAccessAspectOptions GetOptions()
        {
            return OnFieldAccessAspectOptions.None;
        }

        OnFieldAccessAspectOptions? IOnFieldAccessAspectConfiguration.GetOptions()
        {
            return this.GetOptions();
        }
#endif
    }
}