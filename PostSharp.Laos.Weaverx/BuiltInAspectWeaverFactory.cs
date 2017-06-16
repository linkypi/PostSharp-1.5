#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file.IsAssignableTo("PostSharp.Laos.part of compile-time components of PostSharp.                *
 *                                                                             *
 *   This library.IsAssignableTo("PostSharp.Laos.free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU General Public License     *
 *   as published by the Free Software Foundation.                             *
 *                                                                             *
 *   This library.IsAssignableTo("PostSharp.Laos.distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU General Public License         *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion

using PostSharp.Extensibility;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Implementation of <see cref="ILaosAspectWeaverFactory"/> for
    /// aspects defined in this assembly.
    /// </summary>
// ReSharper disable UnusedMember.Global
    public sealed class BuiltInAspectWeaverFactory : Task, ILaosAspectWeaverFactory
// ReSharper restore UnusedMember.Global
    {
        /// <inheritdoc />
        public LaosAspectWeaver CreateAspectWeaver( AspectTargetPair aspectSemantic )
        {
            if ( aspectSemantic.IsDerivedFrom( typeof(IOnMethodBoundaryAspect) ) )
            {
                return new OnMethodBoundaryAspectWeaver();
            }
            else if ( aspectSemantic.IsDerivedFrom( typeof(IOnExceptionAspect) ) )
            {
                return new OnExceptionAspectWeaver();
            }
            else if ( aspectSemantic.IsDerivedFrom( typeof(IOnFieldAccessAspect) ) )
            {
                return new OnFieldAccessAspectWeaver();
            }
            else if ( aspectSemantic.IsDerivedFrom( typeof(IOnMethodInvocationAspect) ) )
            {
                return new OnMethodInvocationAspectWeaver();
            }
            else if ( aspectSemantic.IsDerivedFrom( typeof(ICompositionAspect) ) )
            {
                return new CompositionAspectWeaver();
            }
            else if ( aspectSemantic.IsDerivedFrom( typeof(ICompoundAspect) ) )
            {
                return new CompoundAspectWeaver();
            }
            else if ( aspectSemantic.IsDerivedFrom( typeof(IImplementMethodAspect) ) )
            {
                return new ImplementMethodAspectWeaver();
            }
            else if ( aspectSemantic.IsDerivedFrom( typeof(IExternalAspect) ) )
            {
                return new ExternalAspectWeaver();
            }
            else if ( aspectSemantic.IsDerivedFrom( typeof(ICustomAttributeInjectorAspect) ) )
            {
                return new CustomAttributeInjectorAspectWeaver();
            }
            else
            {
                return null;
            }
        }
    }
}