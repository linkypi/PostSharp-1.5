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
using System.Reflection;

namespace PostSharp.Laos
{
    /// <summary>
    /// Base class for aspects that implement an exception handler
    /// (<see cref="OnMethodBoundaryAspect"/>, <see cref="OnExceptionAspect"/>).
    /// </summary>
    /// <seealso cref="IExceptionHandlerAspect"/>
#if !SMALL
    [Serializable]
#endif
    [SuppressMessage( "Microsoft.Naming", "CA1710" /* IdentifiersShouldHaveCorrectSuffix */ )]
    public abstract class ExceptionHandlerAspect : LaosMethodLevelAspect, IExceptionHandlerAspect
#if !SMALL
        , IExceptionHandlerAspectConfiguration
#endif
    {
        /// <inheritdoc />
        public virtual void OnException( MethodExecutionEventArgs eventArgs )
        {
        }


#if !SMALL
        /// <inheritdoc />
        public virtual InstanceTagRequest GetInstanceTagRequest()
        {
            return null;
        }

        /// <inheritdoc />
        public virtual Type GetExceptionType( MethodBase method )
        {
            return null;
        }

        TypeIdentity IExceptionHandlerAspectConfiguration.GetExceptionType( MethodBase method )
        {
            return TypeIdentity.Wrap( this.GetExceptionType( method ) );
        }
#endif
    }
}