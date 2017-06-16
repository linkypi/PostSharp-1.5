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

namespace PostSharp.Collections
{
    /// <summary>
    /// Minimalist implementation of a one-direction linked list.
    /// </summary>
    /// <typeparam name="T">Type of values.</typeparam>
    /// <remarks>There is no node implementing the <i>list</i>. Everything
    /// is a <i>node</i>. When using the <see cref="IEnumerable{T}"/> interface,
    /// you get an enumeration of the current node and all the next nodes.</remarks>
    [SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    public sealed class SimpleLinkedListNode<T> : IEnumerable<T>, ICloneable
    {
        private T value;
        private SimpleLinkedListNode<T> next;


        private SimpleLinkedListNode()
        {
        }

        /// <summary>
        /// Initializes a new node.
        /// </summary>
        /// <param name="value">Node value.</param>
        /// <param name="next">Next node.</param>
        public SimpleLinkedListNode( T value, SimpleLinkedListNode<T> next )
        {
            this.value = value;
            this.next = next;
        }


        /// <summary>
        /// Gets or sets the node value.
        /// </summary>
        public T Value { get { return this.value; } set { this.value = value; } }

        /// <summary>
        /// Gets the next node.
        /// </summary>
        public SimpleLinkedListNode<T> Next { get { return this.next; } internal set { this.next = value; } }

        /// <summary>
        /// Gets the last node in the list.
        /// </summary>
        /// <returns>The last node in the list.</returns>
        public SimpleLinkedListNode<T> GetLast()
        {
            SimpleLinkedListNode<T> cursor = this;
            SimpleLinkedListNode<T> last = this;

            while ( cursor != null )
            {
                last = cursor;
                cursor = cursor.next;
            }

            return last;
        }


        /// <summary>
        /// Gets an enumerator of the list starting at a given node.
        /// </summary>
        /// <param name="node">The first node of the list, or <b>null</b>.</param>
        /// <returns>An enumerator for <paramref name="node"/> and all following nodex.</returns>
        public static IEnumerator<T> GetEnumerator( SimpleLinkedListNode<T> node )
        {
            if ( node != null )
            {
                SimpleLinkedListNode<T> cursor = node;
                while ( cursor != null )
                {
                    yield return cursor.value;
                    cursor = cursor.next;
                }
            }
        }

        /// <summary>
        /// Inserts a value at the beginning of a list.
        /// </summary>
        /// <param name="node">Reference to the head node. May safely be a reference to a <b>null</b> node.</param>
        /// <param name="value">Value.</param>
        [SuppressMessage( "Microsoft.Design", "CA1045:DoNotPassTypesByReference" )]
        public static void Insert( ref SimpleLinkedListNode<T> node, T value )
        {
            node = new SimpleLinkedListNode<T>( value, node );
        }

        /// <summary>
        /// Appends a value at the end of a list.
        /// </summary>
        /// <param name="node">Reference to a node of the list. May safely be a reference to a <b>null</b> node.</param>
        /// <param name="value">Value.</param>
        [SuppressMessage( "Microsoft.Design", "CA1045:DoNotPassTypesByReference" )]
        public static void Append( ref SimpleLinkedListNode<T> node, T value )
        {
            if ( node == null )
            {
                node = new SimpleLinkedListNode<T>( value, node );
            }
            else
            {
                SimpleLinkedListNode<T> last = node.GetLast();
                last.next = new SimpleLinkedListNode<T>( value, node );
            }
        }

        /// <summary>
        /// Appends a list at the end of another one.
        /// </summary>
        /// <param name="node">Reference of a node of the 'left' list.
        /// May safely be a reference to a <b>null</b> node.</param>
        /// <param name="list">Reference to the head of the 'right' list.
        /// May safely be a <b>null</b> node.</param>
        /// <remarks>
        /// The 'right' list (<paramref name="list"/>) is cloned, so
        /// nodes are never shared between lists.
        /// </remarks>
        [SuppressMessage( "Microsoft.Design", "CA1045:DoNotPassTypesByReference" )]
        public static void Append( ref SimpleLinkedListNode<T> node, SimpleLinkedListNode<T> list )
        {
            if ( list == null )
            {
                return;
            }

            if ( node == null )
            {
                node = list.Clone();
            }
            else
            {
                SimpleLinkedListNode<T> last = node.GetLast();
                last.next = list.Clone();
            }
        }


        /// <summary>
        /// Finds a node in a list and removes it.
        /// </summary>
        /// <param name="node">Reference to the head node. May safely be a reference to a <b>null</b> node.</param>
        /// <param name="value">The value to remove.</param>
        /// <returns><b>true</b> if the node was found and removed, otherwise <b>false</b>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1045:DoNotPassTypesByReference" )]
        public static bool Remove( ref SimpleLinkedListNode<T> node, T value )
        {
            if ( node == null )
            {
                return false;
            }
            else if ( ( value == null && node.value == null ) || node.value.Equals( value ) )
            {
                node = node.next;
                return true;
            }
            else
            {
                SimpleLinkedListNode<T> previousNode = node;

                for ( SimpleLinkedListNode<T> cursor = node.next ; cursor != null ; cursor = cursor.next )
                {
                    if ( ( value == null && cursor.value == null ) || cursor.value.Equals( value ) )
                    {
                        previousNode.next = cursor.next;
                        return true;
                    }

                    previousNode = cursor;
                }
            }

            return false;
        }

        #region IEnumerable<T> Members

        /// <summary>
        /// Gets an enumerator for the current and following nodes.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return GetEnumerator( this );
        }

        #endregion

        #region IEnumerable Members

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Clone the current node (but not the value).
        /// </summary>
        /// <returns>A copy of the current node.</returns>
        public SimpleLinkedListNode<T> Clone()
        {
            SimpleLinkedListNode<T> clonedRoot = new SimpleLinkedListNode<T>();

            SimpleLinkedListNode<T> cursor = this;
            SimpleLinkedListNode<T> clonedCursor = clonedRoot;

            while ( true )
            {
                clonedRoot.value = cursor.value;

                if ( cursor.next != null )
                {
                    clonedCursor.next = new SimpleLinkedListNode<T>();
                    clonedCursor = clonedCursor.next;
                    cursor = cursor.next;
                }
                else
                {
                    break;
                }
            }

            return clonedRoot;
        }

        /// <inheritdoc />
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion
    }
}