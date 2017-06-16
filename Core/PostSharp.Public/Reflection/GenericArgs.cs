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
using System.Globalization;

namespace PostSharp.Reflection
{
    /// <summary>
    /// Classes derived from <see cref="GenericArg"/> represent unbound generic arguments.
    /// It is a 'trick' to create unbound generic instances, because C# and <b>System.Reflection</b>
    /// does not make it possible. Use the <see cref="Map"/> method to bind the unbound
    /// generic arguments to concrete types.
    /// </summary>
    public abstract class GenericArg
    {
#if !SMALL
        /// <summary>
        /// Binds unbound generic arguments to concrete types.
        /// </summary>
        /// <param name="type">A <see cref="Type"/> containing unbound generic arguments
        /// (derived from <see cref="GenericArg"/>).</param>
        /// <param name="genericTypeParameters">Array of types to which
        /// the unbound generic type arguments should be bound.</param>
        /// <param name="genericMethodParameters">Array of types to which
        /// the unbound generic method parameters should be bound.</param>
        /// <returns>A <see cref="Type"/> where unbound generic arguments have been replaced
        /// by <paramref name="genericTypeParameters"/> and <paramref name="genericMethodParameters"/>.</returns>
        public static Type Map( Type type, Type[] genericTypeParameters, Type[] genericMethodParameters )
        {
            if ( type == null )
            {
                throw new ArgumentNullException( "type" );
            }

            if ( type.BaseType != null &&
                 type.BaseType.BaseType != null &&
                 ReflectionTypeComparer.GetInstance().Equals( type.BaseType.BaseType, typeof(GenericArg) ) )
            {
                int ordinal = int.Parse( new string( type.Name[type.Name.Length - 1], 1 ),
                                         CultureInfo.InvariantCulture );

                // This is a special type defined in PostSharp.Laos, used
                // to 'fake' unbound type parameters.
                if ( type.FullName.StartsWith( "PostSharp.Reflection.GenericType" ) )
                {
                    if ( genericTypeParameters == null || ordinal >= genericTypeParameters.Length )
                    {
                        return null;
                    }
                    else
                    {
                        return genericTypeParameters[ordinal];
                    }
                }
                else if ( type.FullName.StartsWith( "PostSharp.Reflection.GenericMethod" ) )
                {
                    if ( genericMethodParameters == null || ordinal >= genericMethodParameters.Length )
                    {
                        return null;
                    }
                    else
                    {
                        return genericMethodParameters[ordinal];
                    }
                }
                else
                {
                    return type;
                }
            }
            else if ( type.IsGenericType )
            {
                Type[] genericArguments = type.GetGenericArguments();
                Type[] mappedGenericArguments = new Type[genericArguments.Length];
                for ( int i = 0 ; i < genericArguments.Length ; i++ )
                {
                    mappedGenericArguments[i] =
                        Map( genericArguments[i], genericTypeParameters, genericMethodParameters );
                    if ( mappedGenericArguments[i] == null )
                    {
                        return null;
                    }
                }
                return type.GetGenericTypeDefinition().MakeGenericType( mappedGenericArguments );
            }
            else if ( type.HasElementType )
            {
                throw new NotSupportedException( "Types with modifiers (like pointers) are not supported." );
            }
            else
            {
                return type;
            }
        }
#endif
    }


    /// <summary>
    /// Classes derived from <see cref="GenericTypeArg"/> represent unbound generic type parameters.
    /// </summary>
    /// <seealso cref="GenericArg"/>
    public abstract class GenericTypeArg : GenericArg
    {
        internal GenericTypeArg()
        {
        }
    }

    /// <summary>
    /// Classes derived from <see cref="GenericMethodArg"/> represent unbound generic method parameters.
    /// </summary>
    /// <seealso cref="GenericArg"/>
    public abstract class GenericMethodArg : GenericArg
    {
        internal GenericMethodArg()
        {
        }
    }

    /// <summary>
    /// Unbound generic type argument. Reference to the 0-th generic type argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericTypeArg0 : GenericTypeArg
    {
        private GenericTypeArg0()
        {
        }
    }

    /// <summary>
    /// Unbound generic type argument. Reference to the 1-th generic type argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericTypeArg1 : GenericTypeArg
    {
        private GenericTypeArg1()
        {
        }
    }

    /// <summary>
    /// Unbound generic type argument. Reference to the 2-th generic type argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericTypeArg2 : GenericTypeArg
    {
        private GenericTypeArg2()
        {
        }
    }

    /// <summary>
    /// Unbound generic type argument. Reference to the 3-th generic type argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericTypeArg3 : GenericTypeArg
    {
        private GenericTypeArg3()
        {
        }
    }

    /// <summary>
    /// Unbound generic type argument. Reference to the 4-th generic type argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericTypeArg4 : GenericTypeArg
    {
        private GenericTypeArg4()
        {
        }
    }

    /// <summary>
    /// Unbound generic type argument. Reference to the 5-th generic type argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericTypeArg5 : GenericTypeArg
    {
        private GenericTypeArg5()
        {
        }
    }

    /// <summary>
    /// Unbound generic type argument. Reference to the 6-th generic type argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericTypeArg6 : GenericTypeArg
    {
        private GenericTypeArg6()
        {
        }
    }

    /// <summary>
    /// Unbound generic type argument. Reference to the 7-th generic type argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericTypeArg7 : GenericTypeArg
    {
        private GenericTypeArg7()
        {
        }
    }

    /// <summary>
    /// Unbound generic type argument. Reference to the 8-th generic type argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericTypeArg8 : GenericTypeArg
    {
        private GenericTypeArg8()
        {
        }
    }

    /// <summary>
    /// Unbound generic type argument. Reference to the 9-th generic type argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericTypeArg9 : GenericTypeArg
    {
        private GenericTypeArg9()
        {
        }
    }

    /// <summary>
    /// Unbound generic method argument. Reference to the 0-th generic method argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericMethodArg0 : GenericMethodArg
    {
        private GenericMethodArg0()
        {
        }
    }

    /// <summary>
    /// Unbound generic method argument. Reference to the 1-th generic method argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericMethodArg1 : GenericMethodArg
    {
        private GenericMethodArg1()
        {
        }
    }

    /// <summary>
    /// Unbound generic method argument. Reference to the 2-th generic method argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericMethodArg2 : GenericMethodArg
    {
        private GenericMethodArg2()
        {
        }
    }

    /// <summary>
    /// Unbound generic method argument. Reference to the 3-th generic method argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericMethodArg3 : GenericMethodArg
    {
        private GenericMethodArg3()
        {
        }
    }

    /// <summary>
    /// Unbound generic method argument. Reference to the 4-th generic method argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericMethodArg4 : GenericMethodArg
    {
        private GenericMethodArg4()
        {
        }
    }

    /// <summary>
    /// Unbound generic method argument. Reference to the 5-th generic method argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericMethodArg5 : GenericMethodArg
    {
        private GenericMethodArg5()
        {
        }
    }

    /// <summary>
    /// Unbound generic method argument. Reference to the 6-th generic method argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericMethodArg6 : GenericMethodArg
    {
        private GenericMethodArg6()
        {
        }
    }

    /// <summary>
    /// Unbound generic method argument. Reference to the 7-th generic method argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericMethodArg7 : GenericMethodArg
    {
        private GenericMethodArg7()
        {
        }
    }

    /// <summary>
    /// Unbound generic method argument. Reference to the 8-th generic method argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericMethodArg8 : GenericMethodArg
    {
        private GenericMethodArg8()
        {
        }
    }

    /// <summary>
    /// Unbound generic method argument. Reference to the 9-th generic method argument.
    /// </summary>
    /// <seealso cref="GenericArg.Map"/>
    /// <seealso cref="GenericArg"/>
    public sealed class GenericMethodArg9 : GenericMethodArg
    {
        private GenericMethodArg9()
        {
        }
    }
}
