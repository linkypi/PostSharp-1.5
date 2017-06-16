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
using PostSharp.Extensibility;

namespace PostSharp.Laos
{
    /// <summary>
    /// Base class for all aspects applied on types.
    /// </summary>
    /// <seealso cref="ILaosTypeLevelAspect"/>
#if !SMALL
    [Serializable]
#endif
    [DebuggerNonUserCode]
    [SuppressMessage( "Microsoft.Naming", "CA1710" /* IdentifiersShouldHaveCorrectSuffix */ )]
    public abstract class LaosTypeLevelAspect : LaosWeavableAspect, ILaosTypeLevelAspect
#if !SMALL
        , ILaosTypeLevelAspectBuildSemantics
#endif
    {
#if !SMALL
        /// <inheritdoc />
        [CompileTimeSemantic]
        public virtual bool CompileTimeValidate( Type type )
        {
            return true;
        }

        /// <inheritdoc />
        [CompileTimeSemantic]
        public override sealed bool CompileTimeValidate( object target )
        {
            Type type = target as Type;
            if ( type == null )
                throw new ArgumentException( string.Format( "Aspects of type {0} can be applied to types only.",
                                                            this.GetType().FullName ) );

            return this.CompileTimeValidate( type );
        }

        /// <inheritdoc />
        [CompileTimeSemantic]
        public virtual void CompileTimeInitialize( Type type )
        {
        }
#endif

        /// <inheritdoc />
        public virtual void RuntimeInitialize( Type type )
        {
        }
    }
}