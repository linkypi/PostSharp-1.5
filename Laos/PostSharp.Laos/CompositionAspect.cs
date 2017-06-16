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
    /// Custom attribute that, when applied on a type, compose another type into that
    /// type and implements one of its interfaces.
    /// </summary>
    /// <remarks>
    /// Use the <see cref="Post"/>.<see cref="Post.Cast{SourceType,TargetType}"/>
    /// method to cast the enhanced type to the newly implemented interface. This
    /// cast is verified during post-compilation.
    /// </remarks>
#if !SMALL
    [Serializable]
#endif
    [SuppressMessage( "Microsoft.Naming", "CA1710" /* IdentifiersShouldHaveCorrectSuffix */ )]
    [AttributeUsage( AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct,
        AllowMultiple = true, Inherited = false )]
    [MulticastAttributeUsage( MulticastTargets.Class | MulticastTargets.Struct, AllowMultiple = true,
        TargetTypeAttributes = MulticastAttributes.UserGenerated )]
    public abstract class CompositionAspect : LaosTypeLevelAspect, ICompositionAspect
#if !SMALL
        , ICompositionAspectConfiguration
#endif
    {
        /// <inheritdoc />
        public virtual InstanceTagRequest GetInstanceTagRequest()
        {
            return null;
        }

        /// <inheritdoc />
        public abstract object CreateImplementationObject( InstanceBoundLaosEventArgs eventArgs );

#if !SMALL
        /// <inheritdoc />
        public abstract Type GetPublicInterface( Type containerType );

        /// <inheritdoc />
        public virtual Type[] GetProtectedInterfaces( Type containerType )
        {
            return null;
        }

        /// <inheritdoc />
        public virtual CompositionAspectOptions GetOptions()
        {
            return CompositionAspectOptions.None;
        }

        CompositionAspectOptions? ICompositionAspectConfiguration.GetOptions()
        {
            return this.GetOptions();
        }


        TypeIdentity ICompositionAspectConfiguration.GetPublicInterface( Type containerType )
        {
            return TypeIdentity.Wrap( this.GetPublicInterface( containerType ) );
        }

        TypeIdentity[] ICompositionAspectConfiguration.GetProtectedInterfaces( Type containerType )
        {
            return TypeIdentity.Wrap( this.GetProtectedInterfaces( containerType ) );
        }
#endif
    }
}