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
using System.Diagnostics.CodeAnalysis;
using PostSharp.CodeModel;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Collection of tags (see <see cref="ITaggable"/>). Used to pass tags from the
    /// host to the <see cref="Domain"/> and <see cref="Project"/> objects.
    /// </summary>
    [Serializable]
    [SuppressMessage( "Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix" )]
    public sealed class TagCollection : ITaggable
    {
        private readonly Dictionary<Guid, object> implementation = new Dictionary<Guid, object>();

        #region ITaggable Members

        /// <inheritdoc />
        public object GetTag( Guid guid )
        {
            object value;
            this.implementation.TryGetValue( guid, out value );
            return value;
        }

        /// <inheritdoc />
        public void SetTag( Guid guid, object value )
        {
            this.implementation[guid] = value;
        }

        #endregion

        internal void CopyTo( ITaggable taggable )
        {
            foreach ( KeyValuePair<Guid, object> pair in this.implementation )
            {
                taggable.SetTag( pair.Key, pair.Value );
            }
        }
    }
}