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

#if !SMALL

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace PostSharp.Reflection
{
    /// <summary>
    /// Custom implementation of a reflection <see cref="Binder"/> that select
    /// methods based on exact matches using the <see cref="ReflectionTypeComparer"/>.
    /// </summary>
    public sealed class CustomReflectionBinder : Binder
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly CustomReflectionBinder Instance = new CustomReflectionBinder();

        private CustomReflectionBinder()
        {
        }
     
        /// <inheritdoc />
        public override FieldInfo BindToField( BindingFlags bindingAttr, FieldInfo[] match, object value,
                                               CultureInfo culture )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override MethodBase BindToMethod( BindingFlags bindingAttr, MethodBase[] match, ref object[] args,
                                                 ParameterModifier[] modifiers, CultureInfo culture, string[] names,
                                                 out object state )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override object ChangeType( object value, Type type, CultureInfo culture )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void ReorderArgumentArray( ref object[] args, object state )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override MethodBase SelectMethod( BindingFlags bindingAttr, MethodBase[] match, Type[] types,
                                                 ParameterModifier[] modifiers )
        {
            MethodBase selectedMethod = null;

            foreach ( MethodBase candidate in match )
            {
                ParameterInfo[] candidateParameters = candidate.GetParameters();

                if ( candidateParameters.Length != types.Length )
                {
                    continue;
                }

                bool isMatch = true;
                for ( int i = 0 ; i < candidateParameters.Length ; i++ )
                {
                    if ( !ReflectionTypeComparer.GetInstance().Equals( candidateParameters[i].ParameterType, types[i] ) )
                    {
                        isMatch = false;
                        break;
                    }
                }

                if ( !isMatch )
                {
                    continue;
                }

                if ( selectedMethod != null )
                {
                    throw new AmbiguousMatchException();
                }

                selectedMethod = candidate;
            }

            return selectedMethod;
        }

        /// <inheritdoc />
        public override PropertyInfo SelectProperty( BindingFlags bindingAttr, PropertyInfo[] match, Type returnType,
                                                     Type[] indexes, ParameterModifier[] modifiers )
        {
            throw new NotImplementedException();
        }
    }
}

#endif
