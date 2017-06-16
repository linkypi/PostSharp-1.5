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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using PostSharp.Extensibility;

namespace PostSharp.Laos
{
    /// <summary>
    /// Base class for all aspects applied on methods.
    /// </summary>
    /// <seealso cref="ILaosMethodLevelAspect"/>
#if !SMALL
    [Serializable]
#endif
    [DebuggerNonUserCode]
    [SuppressMessage( "Microsoft.Naming", "CA1710" /* IdentifiersShouldHaveCorrectSuffix */ )]
    public abstract class LaosMethodLevelAspect : LaosWeavableAspect, ILaosMethodLevelAspect
#if !SMALL
        , ILaosMethodLevelAspectBuildSemantics
#endif
    {
#if !SMALL
        /// <inheritdoc />
        [CompileTimeSemantic]
        public virtual bool CompileTimeValidate( MethodBase method )
        {
            return true;
        }

        /// <inheritdoc />
        [CompileTimeSemantic]
        public override sealed bool CompileTimeValidate( object target )
        {
            MethodBase method = target as MethodBase;
            if ( method == null )
                throw new ArgumentException( string.Format( "Aspects of type {0} can be applied to methods only.",
                                                            this.GetType().FullName ) );

            return this.CompileTimeValidate( method );
        }

        /// <inheritdoc />
        [CompileTimeSemantic]
        public virtual void CompileTimeInitialize( MethodBase method )
        {
        }
#endif

        /// <inheritdoc />
        public virtual void RuntimeInitialize( MethodBase method )
        {
        }
    }
}