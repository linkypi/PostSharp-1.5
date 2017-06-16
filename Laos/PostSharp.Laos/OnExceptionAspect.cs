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
    /// Custom attribute that, when applied to a method, defines an exception
    /// handler around the whole method and calls a custom method in this exception
    /// handler.
    /// </summary>
    /// <see cref="OnMethodBoundaryAspect"/>
#if !SMALL
    [Serializable]
#endif
    [AttributeUsage( AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor |
                     AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property |
                     AttributeTargets.Struct,
        AllowMultiple = true, Inherited = false )]
    [MulticastAttributeUsage(
        MulticastTargets.Method | MulticastTargets.StaticConstructor | MulticastTargets.InstanceConstructor,
        TargetTypeAttributes = MulticastAttributes.UserGenerated,
        TargetMemberAttributes =
            MulticastAttributes.NonAbstract | MulticastAttributes.AnyScope | MulticastAttributes.AnyVisibility |
            MulticastAttributes.Managed,
        AllowMultiple = true )]
    [SuppressMessage( "Microsoft.Naming", "CA1710" /* IdentifiersShouldHaveCorrectSuffix */ )]
    public abstract class OnExceptionAspect : ExceptionHandlerAspect, IOnExceptionAspect
#if !SMALL
, IOnExceptionAspectConfiguration
#endif
    {
#if !SMALL
        /// <inheritdoc />
        [CompileTimeSemantic]
        public virtual OnExceptionAspectOptions GetOptions()
        {
            return OnExceptionAspectOptions.None;
        }

        OnExceptionAspectOptions? IOnExceptionAspectConfiguration.GetOptions()
        {
            return this.GetOptions();
        }
#endif
    }
}