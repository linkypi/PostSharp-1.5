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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PostSharp.Collections;

namespace PostSharp.CodeModel.Collections
{
    /// <summary>
    /// Dictionary where keys are declarations (<see cref="MetadataDeclaration"/>).
    /// The implementation is partially thread-safe and is optimized to have 
    /// low lock contention.
    /// </summary>
    /// <typeparam name="T">Type of items stored in the dictionary.</typeparam>
    /// <remarks>
    /// This implementation does make the difference between an <i>absent</i>
    /// and a <i>null</i> item (or an item with default value, if <typeparamref name="T"/>
    /// is a value type).
    /// </remarks>
    [SuppressMessage( "Microsoft.Naming", "CA1710" /*IdentifiersShouldHaveCorrectSuffix*/ )]
    public class MetadataDeclarationDirectory<T> : IDictionary<MetadataDeclaration, T>
    {
        private readonly ModuleDeclaration module;
        private readonly ExtensibleArray<T>[] tables;

        /// <summary>
        /// Initializes a new <see cref="MetadataDeclarationDirectory{T}"/>.
        /// </summary>
        /// <param name="module">Module to which declarations should belong.</param>
        public MetadataDeclarationDirectory( ModuleDeclaration module )
        {
            ExceptionHelper.AssertArgumentNotNull( module, "module" );

            this.module = module;
            //  this.module.Tables.MetadataTokenChanging += new PropertyChangedEventHandler<MetadataToken>(OnMetadataTokenChanging);
            //  this.module.Tables.MetadataTokenChanged += new PropertyChangedEventHandler<MetadataToken>(OnMetadataTokenChanged);

            this.tables = new ExtensibleArray<T>[MetadataDeclarationTables.TableCount];
        }

        /*
        /// <summary>
        /// Handles the <see cref="MetadataDeclarationTables.MetadataTokenChanging"/> event.
        /// </summary>
        /// <param name="sender">Declaration whose token is being changed.</param>
        /// <param name="e">Arguments.</param>
        void OnMetadataTokenChanging(object sender, PropertyChangedEventArgs<MetadataToken> e)
        {
            this.SetValue(e.NewValue, this.GetValue(e.OldValue));
        }

        /// <summary>
        /// Handles the <see cref="MetadataDeclarationTables.MetadataTokenChanged"/> event.
        /// </summary>
        /// <param name="sender">Declaration whose token is being changed.</param>
        /// <param name="e">Arguments.</param>
        void OnMetadataTokenChanged(object sender, PropertyChangedEventArgs<MetadataToken> e)
        {
            this.SetValue(e.OldValue, default(T));
        }

        */

        #region IDictionary<MetadataDeclaration,T> Members

        /// <inheritdoc />
        void IDictionary<MetadataDeclaration, T>.Add( MetadataDeclaration key, T value )
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public bool ContainsKey( MetadataDeclaration key )
        {
            return !IsDefault( this[key] );
        }

        /// <inheritdoc />
        ICollection<MetadataDeclaration> IDictionary<MetadataDeclaration, T>.Keys { get { throw new NotSupportedException(); } }

        /// <inheritdoc />
        public bool Remove( MetadataDeclaration key )
        {
            ExceptionHelper.AssertArgumentNotNull( key, "key" );

            // this.module.Tables.MetadataTokenChangingLock.AcquireReaderLock(100);


            int tableIndex = (int) key.GetTokenType();
            int rowIndex = key.MetadataToken.Index;

            ExtensibleArray<T> table = this.tables[tableIndex];
            if ( table == null )
            {
                return false;
            }
            else
            {
                if ( !IsDefault( table[rowIndex] ) )
                {
                    table[rowIndex] = default( T );
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <inheritdoc />
        bool IDictionary<MetadataDeclaration, T>.TryGetValue( MetadataDeclaration key, out T value )
        {
            value = this[key];
            return true;
        }

        /// <inheritdoc />
        ICollection<T> IDictionary<MetadataDeclaration, T>.Values { get { throw new NotSupportedException(); } }

        /// <inheritdoc />
        public T this[ MetadataDeclaration key ]
        {
            get
            {
                ExceptionHelper.AssertArgumentNotNull( key, "key" );

                return this.GetValue( key.MetadataToken );
            }
            set
            {
                ExceptionHelper.AssertArgumentNotNull( key, "key" );


                this.SetValue( key.MetadataToken, value );
            }
        }

        #endregion

        /// <summary>
        /// Gets a value given a token.
        /// </summary>
        /// <param name="token">Token.</param>
        /// <returns>The value associated with token <paramref name="token"/>.</returns>
        private T GetValue( MetadataToken token )
        {
            int rowIndex = token.Index;

            // Create the table if it does not exist.
            ExtensibleArray<T> table = this.tables[token.TableIndex];
            if ( table == null )
            {
                return default( T );
            }
            else
            {
                return table[rowIndex];
            }
        }

        /// <summary>
        /// Sets the value associated to a token (without acquiring locks).
        /// </summary>
        /// <param name="token">Token.</param>
        /// <param name="value">The value to associate to <paramref name="token"/>.</param>
        private void SetValue( MetadataToken token, T value )
        {
            int tableIndex = token.TableIndex;
            int rowIndex = token.Index;

            // Create the table if it does not exist.
            ExtensibleArray<T> table = this.tables[tableIndex];
            if ( table == null )
            {
                lock ( this )
                {
                    table = this.tables[tableIndex];
                    if ( table == null )
                    {
                        table =
                            new ExtensibleArray<T>( this.module.Tables.GetTableSize( token.TokenType ) );
                        this.tables[tableIndex] = table;
                    }
                }
            }

            table[rowIndex] = value;
        }

        #region ICollection<KeyValuePair<MetadataDeclaration,T>> Members

        /// <inheritdoc />
        void ICollection<KeyValuePair<MetadataDeclaration, T>>.Add( KeyValuePair<MetadataDeclaration, T> item )
        {
            this[item.Key] = item.Value;
        }

        /// <inheritdoc />
        public void Clear()
        {
            for ( int i = 0 ; i < this.tables.Length ; i++ )
            {
                this.tables[i] = null;
            }
        }

        /// <inheritdoc />
        bool ICollection<KeyValuePair<MetadataDeclaration, T>>.Contains( KeyValuePair<MetadataDeclaration, T> item )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CopyTo( KeyValuePair<MetadataDeclaration, T>[] array, int arrayIndex )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int Count { get { throw new NotSupportedException(); } }

        /// <inheritdoc />
        public bool IsReadOnly { get { return false; } }

        /// <inheritdoc />
        public bool Remove( KeyValuePair<MetadataDeclaration, T> item )
        {
            T currentValue = this[item.Key];
            if ( !IsDefault( currentValue ) )
            {
                if ( Equals( currentValue, item.Value ) )
                {
                    return this.Remove( item.Key );
                }
            }

            return false;
        }

        #endregion

        #region IEnumerable<KeyValuePair<MetadataDeclaration,T>> Members

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<MetadataDeclaration, T>> GetEnumerator()
        {
            for ( int i = 0 ; i < this.tables.Length ; i++ )
            {
                ExtensibleArray<T> table = this.tables[i];

                IEnumerator<KeyValuePair<int, T>> enumerator = table.GetEnumerator();

                while ( enumerator.MoveNext() )
                {
                    MetadataDeclaration declaration = this.module.Tables.GetDeclaration(
                        new MetadataToken( (TokenType) i, enumerator.Current.Key ) );
                    yield return new KeyValuePair<MetadataDeclaration, T>( declaration, enumerator.Current.Value );
                }
            }
        }

        #endregion

        /// <inheritdoc />
        private static bool IsDefault( T value )
        {
            return Equals( value, null );
        }

        private static bool Equals( T left, T right )
        {
            return ReferenceEquals( left, right );
        }


        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }
}