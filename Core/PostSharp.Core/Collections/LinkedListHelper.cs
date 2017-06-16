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
using System.Collections.Generic;

namespace PostSharp.Collections
{
    internal static class LinkedListHelper
    {
        public static T GetFirstValue<T>( LinkedList<T> list )
        {
            if ( list == null )
                return default( T );

            LinkedListNode<T> firstNode = list.First;

            if ( firstNode == null )
                return default( T );

            return firstNode.Value;
        }

        public static T GetLastValue<T>( LinkedList<T> list )
        {
            if ( list == null )
                return default( T );

            LinkedListNode<T> lastNode = list.Last;

            if ( lastNode == null )
                return default( T );

            return lastNode.Value;
        }

        public static void AddNode<T>( LinkedList<T> list, LinkedListNode<T> newNode, NodePosition position,
                                       LinkedListNode<T> referenceNode )
        {
            if ( position == NodePosition.Before )
            {
                if ( referenceNode == null )
                {
                    list.AddFirst( newNode );
                }
                else
                {
                    list.AddBefore( referenceNode, newNode );
                }
            }
            else if ( position == NodePosition.After )
            {
                if ( referenceNode == null )
                {
                    list.AddLast( newNode );
                }
                else
                {
                    list.AddAfter( referenceNode, newNode );
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException( "position" );
            }
        }
    }
}