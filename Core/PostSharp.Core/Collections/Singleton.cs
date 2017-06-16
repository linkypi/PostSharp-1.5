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

#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace PostSharp.Collections
{
    /// <summary>
    /// Wraps a single value into a list. A singleton contains zero or one value.
    /// </summary>
    /// <typeparam name="V">Value type.</typeparam>
    [SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    public class Singleton<V> : IList<V>
    {
        /// <summary>
        /// Flags.
        /// </summary>
        [Flags]
        private enum Flags : byte
        {
            /// <summary>
            /// Read only.
            /// </summary>
            ReadOnly = 1,

            /// <summary>
            /// If set, means that the singleton contains a value. Otherwise, 
            /// the collection is empty.
            /// </summary>
            HasValue = 2
        }


        /// <summary>
        /// Flags.
        /// </summary>
        private Flags flags;

        /// <summary>
        /// Value.
        /// </summary>
        private V value;

        /// <summary>
        /// Initializes a new read-only <see cref="Singleton{T}"/> containing one element
        /// </summary>
        /// <param name="value">Value.</param>
        public Singleton( V value ) : this( value, true )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Singleton{T}"/> containing one element
        /// and specifies whether the collection should be read-only.
        /// </summary>
        /// <param name="value">Initial value.</param>
        /// <param name="readOnly"><b>true</b> the singleton is read-only, <b>false</b>
        /// if the value can be removed or overwritten.</param>
        public Singleton( V value, bool readOnly )
        {
            this.value = value;
            this.flags = Flags.HasValue | ( readOnly ? Flags.ReadOnly : 0 );
        }

        /// <summary>
        /// Throws an exception if the current <see cref="Singleton{T}"/> is read-only.
        /// </summary>
        private void AssertWritable()
        {
            ExceptionHelper.Core.AssertValidOperation( !this.IsReadOnly, "ReadOnlyCollection" );
        }

        /// <summary>
        /// Determines whether the current <see cref="Singleton{T}"/> has a value.
        /// </summary>
        private bool HasValue
        {
            get { return ( this.flags & Flags.HasValue ) != 0; }
        }

        #region IList<V> Members

        /// <inheritdoc />
        public int IndexOf( V item )
        {
            if ( this.HasValue )
            {
                return this.value.Equals( item ) ? 0 : -1;
            }
            else
            {
                return -1;
            }
        }

        /// <inheritdoc />
        public void Insert( int index, V item )
        {
            this.AssertWritable();
            ExceptionHelper.Core.AssertValidOperation( !this.HasValue && index == 0, "SingletonIsSingleton" );

            this[0] = item;
        }

        /// <inheritdoc />
        public void RemoveAt( int index )
        {
            this.AssertWritable();
            ExceptionHelper.Core.AssertValidOperation( this.HasValue && index == 0, "SingletonIsSingleton" );

            this.flags &= ~Flags.HasValue;
        }

        /// <inheritdoc />
        public V this[ int index ]
        {
            get
            {
                #region Preconditions

                if ( index > 0 || !this.HasValue )
                {
                    throw new ArgumentOutOfRangeException();
                }

                #endregion

                return this.value;
            }

            set
            {
                this.AssertWritable();
                ExceptionHelper.Core.AssertValidOperation( index == 0, "SingletonIsSingleton" );

                this.value = value;
                this.flags |= ~Flags.HasValue;
            }
        }

        #endregion

        #region ICollection<V> Members

        /// <inheritdoc />
        public void Add( V item )
        {
            this.AssertWritable();
            ExceptionHelper.Core.AssertValidOperation( !this.HasValue, "SingletonIsSingleton" );

            this[0] = item;
        }

        /// <inheritdoc />
        public void Clear()
        {
            this.AssertWritable();

            this.value = default( V );
            this.flags &= ~Flags.HasValue;
        }

        /// <inheritdoc />
        public bool Contains( V item )
        {
            return this.HasValue && this.value.Equals( item );
        }

        /// <inheritdoc />
        public void CopyTo( V[] array, int arrayIndex )
        {
            if ( this.HasValue )
            {
                array[arrayIndex] = value;
            }
        }

        /// <inheritdoc />
        public int Count
        {
            get { return this.HasValue ? 1 : 0; }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get { return ( this.flags & Flags.ReadOnly ) != 0; }
        }

        /// <inheritdoc />
        public bool Remove( V item )
        {
            this.AssertWritable();
            ExceptionHelper.Core.AssertValidOperation( !this.HasValue, "SingletonIsSingleton" );

            if ( this.HasValue )
            {
                if ( this.value.Equals( item ) )
                {
                    this.Clear();
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region IEnumerable<V> Members

        /// <inheritdoc />
        public IEnumerator<V> GetEnumerator()
        {
            if ( this.HasValue )
            {
                yield return this.value;
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// Enumerator that returns a single value.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public sealed class SingletonEnumerator<T> : IEnumerator<T>
    {
        private readonly T value;
        private int position;

        /// <summary>
        /// Initializes a new <see cref="SingletonEnumerator{T}"/>
        /// </summary>
        /// <param name="value">Value returned by the <see cref="Current"/> property.</param>
        public SingletonEnumerator( T value )
        {
            this.value = value;
        }

        #region IEnumerator<T> Members

        /// <inheritdoc />
        public T Current
        {
            get
            {
                if ( position != 1 )
                {
                    throw new InvalidOperationException();
                }
                return this.value;
            }
        }

        #endregion

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose()
        {
        }

        #endregion

        #region IEnumerator Members

        /// <inheritdoc />
        object IEnumerator.Current
        {
            get { return this.Current; }
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            this.position++;
            return this.position == 1;
        }

        /// <inheritdoc />
        public void Reset()
        {
            this.position = 0;
        }

        #endregion
    }
}