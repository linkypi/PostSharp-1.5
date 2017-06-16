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
    /// Base class for all aspects applied on fields.
    /// </summary>
    /// <seealso cref="ILaosFieldLevelAspect"/>
#if !SMALL
    [Serializable]
#endif
    [DebuggerNonUserCode]
    [SuppressMessage( "Microsoft.Naming", "CA1710" /* IdentifiersShouldHaveCorrectSuffix */ )]
    public abstract class LaosFieldLevelAspect : LaosWeavableAspect, ILaosFieldLevelAspect
#if !SMALL
        , ILaosFieldLevelAspectBuildSemantics
#endif
    {
#if !SMALL
        /// <inheritdoc />
        [CompileTimeSemantic]
        public virtual bool CompileTimeValidate( FieldInfo field )
        {
            return true;
        }

        /// <inheritdoc />
        public override sealed bool CompileTimeValidate( object target )
        {
            FieldInfo field = target as FieldInfo;
            if ( field == null )
                throw new ArgumentException( string.Format( "Aspects of type {0} can be applied to fields only.",
                                                            this.GetType().FullName ) );

            return this.CompileTimeValidate( field );
        }

        /// <inheritdoc />
        [CompileTimeSemantic]
        public virtual void CompileTimeInitialize( FieldInfo field )
        {
        }
#endif

        /// <inheritdoc />
        public virtual void RuntimeInitialize( FieldInfo field )
        {
        }
    }
}