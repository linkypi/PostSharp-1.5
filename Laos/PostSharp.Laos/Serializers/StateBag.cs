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
using System.Collections.Generic;
using System.IO;

namespace PostSharp.Laos.Serializers
{
    /// <summary>
    /// Simple recursive structure of name-value collections used by the <see cref="StateBagSerializer"/>
    /// to implement portable serialization.
    /// </summary>
    public sealed class StateBag
    {
        private const byte signature = 0x12;

        private readonly string value;
        private readonly Dictionary<string, StateBag> items;

        /// <summary>
        /// Initializes a new <see cref="StateBag"/>.
        /// </summary>
        public StateBag()
        {
            items = new Dictionary<string, StateBag>(
                StringComparer.InvariantCulture );
        }

        private StateBag( string value, int capacity )
        {
            this.value = value;
            if ( capacity > 0 )
            {
                this.items = new Dictionary<string, StateBag>( capacity, StringComparer.InvariantCulture );
            }
        }

        /// <summary>
        /// Sets a value in the current <see cref="StateBag"/>.
        /// </summary>
        /// <param name="name">Item name.</param>
        /// <param name="value">Item value.</param>
        public void SetValue( string name, string value )
        {
            if ( string.IsNullOrEmpty( name ) )
                throw new ArgumentNullException( "name" );

            this.items[name] = new StateBag( value, 0 );
        }

        /// <summary>
        /// Gets a value of the current <see cref="StateBag"/>.
        /// </summary>
        /// <param name="name">Name of the requested value.</param>
        /// <returns>The value named <paramref name="name"/>, or <b>null</b>
        /// if the <see cref="StateBag"/> does not contain this value.</returns>
        public string GetValue( string name )
        {
            if ( string.IsNullOrEmpty( name ) )
                throw new ArgumentNullException( "name" );

            StateBag node;
            if ( this.items.TryGetValue( name, out node ) )
            {
                return node.value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets or sets a value in the current <see cref="StateBag"/>.
        /// </summary>
        /// <param name="name">Value name.</param>
        /// <returns>The value named <paramref name="name"/>, or <b>null</b> if the
        /// current <see cref="StateBag"/> does not contain any value named <paramref name="name"/>.</returns>
        public string this[ string name ]
        {
            get { return this.GetValue( name ); }
            set { this.SetValue( name, value ); }
        }

        internal void SetBag( string name, StateBag node )
        {
            this.items[name] = node;
        }

        /// <summary>
        /// Create a child <see cref="StateBag"/> and assign it a name.
        /// </summary>
        /// <param name="name">Name of the child bag.</param>
        /// <returns>The new <see cref="StateBag"/>.</returns>
        public StateBag CreateBag( string name )
        {
            StateBag stateBag = new StateBag();
            this.items[name] = stateBag;
            return stateBag;
        }

        /// <summary>
        /// Sets a child <see cref="StateBag"/> to the <see cref="StateBag"/> obtained
        /// by deserializing an object.
        /// </summary>
        /// <param name="name">Name of the child <see cref="StateBag"/> in the current <see cref="StateBag"/>.</param>
        /// <param name="obj">Object to deserialize.</param>
        public void SetBag( string name, IStateBagSerializable obj )
        {
            if ( string.IsNullOrEmpty( name ) )
                throw new ArgumentNullException( "name" );

            if ( obj != null )
            {
                StateBag stateBag = new StateBag();
                obj.Serialize( stateBag );
                this.items[name] = stateBag;
            }
        }

        /// <summary>
        /// Gets a child <see cref="StateBag"/>.
        /// </summary>
        /// <param name="name">Name of the child <see cref="StateBag"/>.</param>
        /// <returns>The child <see cref="StateBag"/> named <paramref name="name"/>, or <b>null</b>
        /// if there is no child <see cref="StateBag"/> of this name.</returns>
        public StateBag GetBag( string name )
        {
            if ( string.IsNullOrEmpty( name ) )
                throw new ArgumentNullException( "name" );

            StateBag node;
            if ( this.items.TryGetValue( name, out node ) )
            {
                if ( node.items == null )
                    throw new InvalidOperationException( "The item under this name is a value, not a node." );

                return node;
            }
            else
            {
                return null;
            }
        }


        private const string nullString = "\0";

        /// <summary>
        /// Serializes the current <see cref="StateBag"/> into a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="BinaryWriter"/>.</param>
        public void Serialize( BinaryWriter writer )
        {
            writer.Write( signature );
            writer.Write( this.value ?? nullString );
            if ( this.items == null )
            {
                writer.Write( -1 );
            }
            else
            {
                writer.Write( this.items.Count );
                foreach ( KeyValuePair<string, StateBag> pair in this.items )
                {
                    writer.Write( pair.Key );
                    pair.Value.Serialize( writer );
                }
            }
        }

        /// <summary>
        /// Deserializes a stream (given a <see cref="BinaryReader"/>) into a <see cref="StateBag"/>.
        /// </summary>
        /// <param name="reader">A <see cref="BinaryReader"/>.</param>
        /// <returns>The <see cref="StateBag"/> built from <paramref name="reader"/>.</returns>
        public static StateBag Deserialize( BinaryReader reader )
        {
            if ( reader.ReadByte() != signature )
            {
                throw new StateBagSerializerException( "Invalid signature." );
            }


            string value = reader.ReadString();
            int n = reader.ReadInt32();

            StateBag node = new StateBag( value, n );

            for ( int i = 0; i < n; i++ )
            {
                string name = reader.ReadString();
                StateBag child = Deserialize( reader );
                node.items.Add( name, child );
            }

            return node;
        }
    }
}

#endif