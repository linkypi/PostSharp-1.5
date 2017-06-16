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

namespace PostSharp
{
    /// <summary>
    /// Provides some methods that are transformed during post-compilation.
    /// </summary>
    public static class Post
    {
#if !MF
        /// <summary>
        /// At post-compile time, casts an instance of a type into another.
        /// A post-compile time error is reported if the source type cannot be
        /// assigned to the target type.
        /// </summary>
        /// <typeparam name="SourceType">Source type.</typeparam>
        /// <typeparam name="TargetType">Target type.</typeparam>
        /// <param name="o">Instance to be casted.</param>
        /// <returns>The object <paramref name="o"/> casted as <typeparamref name="TargetType"/>.</returns>
        /// <remarks>
        /// The purpose of this method is to make a source code compilable even when
        /// an interface will be implemented at post-compile time.
        /// PostSharp ensures that <typeparamref name="TargetType"/> is assignable from
        /// <typeparamref name="SourceType"/>. If yes, the call to this method is
        /// simply suppressed. If types are not assignable, a build error is issued.
        /// </remarks>
        public static TargetType Cast<SourceType, TargetType>( SourceType o )
            where SourceType : class
            where TargetType : class
        {
            return (TargetType) (object) o;
        }
#endif

        /// <summary>
        /// Determines whether the calling program has been transformed by PostSharp.
        /// </summary>
        /// <value>
        /// <b>true</b> if the calling program has been transformed by PostSharp, otherwise
        /// <b>false</b>.
        /// </value>
        public static bool IsTransformed
        {
            get { return false; }
        }

#if !MF
        /// <summary>
        /// When used to retrieve the value of a field, forces the compiler to retrieve a copy
        /// of the field value instead of an address to this field. This allows to call
        /// instance methods of value-type fields without loading the field address.
        /// </summary>
        /// <typeparam name="T">Type of the value to retrieve (this type parameter can generally be omitted).</typeparam>
        /// <param name="value">Value.</param>
        /// <returns><paramref name="value"/>, exactly.</returns>
        public static T GetValue<T>( T value ) where T : struct
        {
            return value;
        }
#endif
    }
}