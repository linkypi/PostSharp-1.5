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
    /// Custom attribute that, when applied to a method defined in the current assembly, inserts a piece
    /// of code before and after the body of these methods. This custom attribute can be multicasted
    /// (see <see cref="MulticastAttribute"/>).
    /// </summary>
    /// <remarks>
    /// This custom attribute is useful to implement "boundary" functionalities like tracing (writing a 
    /// line to log) or transactions (automatically start a transaction at entry and commit or rollback
    /// at exit).
    /// </remarks>
#if !SMALL
    [Serializable]
#endif
    [AttributeUsage( AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor |
                     AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property |
                     AttributeTargets.Struct,
        AllowMultiple = true, Inherited = false )]
    [MulticastAttributeUsage(
        MulticastTargets.Method | MulticastTargets.StaticConstructor | MulticastTargets.InstanceConstructor,
        AllowMultiple = true,
        TargetTypeAttributes = MulticastAttributes.UserGenerated,
        TargetMemberAttributes =
            MulticastAttributes.NonAbstract | MulticastAttributes.AnyScope | MulticastAttributes.AnyVisibility |
            MulticastAttributes.Managed )]
    [SuppressMessage( "Microsoft.Naming", "CA1710" /* IdentifiersShouldHaveCorrectSuffix */ )]
    public abstract class OnMethodBoundaryAspect : ExceptionHandlerAspect, IOnMethodBoundaryAspect
#if !SMALL
, IOnMethodBoundaryAspectConfiguration
#endif
    {
        /// <inheritdoc />
        public virtual void OnEntry( MethodExecutionEventArgs eventArgs )
        {
        }

        /// <inheritdoc />
        public virtual void OnExit( MethodExecutionEventArgs eventArgs )
        {
        }

        /// <inheritdoc />
        public virtual void OnSuccess( MethodExecutionEventArgs eventArgs )
        {
        }

#if !SMALL
        /// <inheritdoc />
        [CompileTimeSemantic]
        public virtual OnMethodBoundaryAspectOptions GetOptions()
        {
            return OnMethodBoundaryAspectOptions.None;
        }

        OnMethodBoundaryAspectOptions? IOnMethodBoundaryAspectConfiguration.GetOptions()
        {
            return this.GetOptions();
        }
#endif
    }
}