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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace PostSharp.Reflection
{
    /// <summary>
    /// Provides helper methods for work with <see cref="System.Reflection"/>.
    /// </summary>
    public static class ReflectionHelper
    {
        static readonly Type[] emptyTypes = new Type[0];
        
        /// <summary>
        /// Identifies and gets a method in a type given its complete string-serialized signature.
        /// </summary>
        /// <param name="type">Type to which the method belong.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="methodSignature">Method signature (from <c>Method.ToString()</c>).</param>
        /// <returns>The method (or constructor) with signature <paramref name="methodSignature"/>
        /// in <paramref name="type"/>, or <b>null</b> if such method was not found.</returns>
        public static MethodBase GetMethod( Type type, string methodName, string methodSignature )
        {
            if ( methodName == ".cctor" )
            {
                ConstructorInfo constructor = type.GetConstructor(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                    null, emptyTypes, null);
                if ( constructor != null )
                {
                    if ( string.Compare( constructor.ToString(), methodSignature ) == 0 )
                    {
                        return constructor;
                    }
                }
            }
            else
            {
                List<MethodBase> methods;

                if ( methodName == ".ctor" )
                {
                    methods = new List<MethodBase>( type.GetConstructors(
                                                        BindingFlags.Public | BindingFlags.NonPublic |
                                                        BindingFlags.Instance ) );
                }
                else
                {
                    methods =
                        new List<MethodBase>(
                            type.GetMethods( BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public |
                                             BindingFlags.NonPublic ) );
                }

                foreach ( MethodBase method in methods )
                {
                    if ( string.Compare( methodName, method.Name, StringComparison.InvariantCulture ) == 0 )
                    {
                        if ( string.Compare( method.ToString(), methodSignature ) == 0 )
                        {
                            return method;
                        }
                    }
                }
            }

#if !CF
            if ( Debugger.IsLogging() )
            {
                Debugger.Log( 1, "PostSharp",
                              string.Format(
                                  "PostSharp.Reflection.ReflectionHelper::GetMethod: Cannot find the method {{{0}}} on type {{{1}}}.{2}",
                                  methodName, type.FullName, Environment.NewLine ) );
            }
#endif

            return null;
        }
    }
}

