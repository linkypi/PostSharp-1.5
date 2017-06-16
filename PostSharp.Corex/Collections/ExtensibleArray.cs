#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of compile-time components of PostSharp.                *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU General Public License     *
 *   as published by the Free Software Foundation.                             *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU General Public License         *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Collections
{
    /// <summary>
    /// Auto-extensible array composed of extends,
    /// with full thread safety and low lock retention.
    /// </summary>
    /// <typeparam name="T">Type of elements.</typeparam>
    [SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    public class ExtensibleArray<T> : IEnumerable<KeyValuePair<int, T>>
    {
        /// <summary>
        /// Size of extends.
        /// </summary>
        private readonly int extendSize;

        /// <summary>
        /// Collection of extends.
        /// </summary>
        private readonly List<T[]> extends;

        /// <summary>
        /// Initializes a new <see cref="ExtensibleArray{T}"/>
        /// with default extend size.
        /// </summary>
        /// <param name="initialCapacity">Initial array capacity.</param>
        public ExtensibleArray( int initialCapacity )
            : this( initialCapacity/2 + 1, initialCapacity )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ExtensibleArray{T}"/>
        /// and specifies the size of extends and the initial capacity.
        /// </summary>
        /// <param name="extendSize">Size of extends.</param>
        /// <param name="initialCapacity">Initial capacity (number of items, not extends).</param>
        public ExtensibleArray( int extendSize, int initialCapacity )
        {
            int extendCount;

            this.extendSize = extendSize;

            if ( initialCapacity%extendSize == 0 )
            {
                extendCount = initialCapacity/extendSize;
            }
            else
            {
                extendCount = 1 + initialCapacity/extendSize;
            }

            this.extends = new List<T[]>( extendCount );
            for ( int i = 0 ; i < extendCount ; i++ )
            {
                this.extends.Add( new T[extendSize] );
            }
        }

        /// <summary>
        /// Gets the index of the extend to which a given item index belongs.
        /// </summary>
        /// <param name="index">Item intex.</param>
        /// <returns>The index of the extend.</returns>
        private int GetExtendIndex( int index )
        {
            return index/extendSize;
        }

        /// <summary>
        /// Gets the index of an item inside its extend.
        /// </summary>
        /// <param name="index">Item index.</param>
        /// <returns>Index of the item in its extend.</returns>
        private int GetIndexInExtend( int index )
        {
            return index%extendSize;
        }


        /// <summary>
        /// Gets or sets an array item.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <returns>The item at position <paramref name="index"/>.</returns>
        public T this[ int index ]
        {
            get
            {
                int extendIndex = this.GetExtendIndex( index );
                if ( extendIndex >= this.extends.Count )
                {
                    return default( T );
                }
                else
                {
                    return this.extends[extendIndex][this.GetIndexInExtend( index )];
                }
            }

            set
            {
                int extendIndex = this.GetExtendIndex( index );

                if ( extendIndex >= this.extends.Count )
                {
                    lock ( this.extends )
                    {
                        if ( extendIndex >= this.extends.Count )
                        {
                            int additionalExtendCount = extendIndex - this.extends.Count + 1;
                            T[][] newExtends = new T[additionalExtendCount][];
                            for ( int i = 0 ; i < additionalExtendCount ; i++ )
                            {
                                newExtends[i] = new T[this.extendSize];
                            }
                            this.extends.AddRange( newExtends );
                        }
                    }
                }

                this.extends[extendIndex][this.GetIndexInExtend( index )] = value;
            }
        }

        /// <summary>
        /// Gives the array upper bound. 
        /// </summary>
        /// <remarks>
        /// The array is guaranteed to contain less than
        /// <see cref="UpperBound"/> items.
        /// </remarks>
        public int UpperBound { get { return this.extends.Count*this.extendSize; } }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
        {
            int absoluteIndex = 0;

            for ( int i = 0 ; i < this.extends.Count ; i++ )
            {
                T[] extend = this.extends[i];

                for ( int j = 0 ; j < this.extendSize ; j++ )
                {
                    T value = extend[j];
                    if ( !IsDefault( value ) )
                    {
                        yield return new KeyValuePair<int, T>( absoluteIndex, value );
                    }

                    absoluteIndex++;
                }
            }
        }

        private static bool IsDefault( T value )
        {
// ReSharper disable CompareNonConstrainedGenericWithNull
            return value == null || value.Equals( default( T ) );
// ReSharper restore CompareNonConstrainedGenericWithNull
        }
    }
}