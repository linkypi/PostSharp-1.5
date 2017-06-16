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

namespace PostSharp
{
    /// <summary>
    /// Defines the semantics of a taggable object. User code can add custom pieces of
    /// information to PostSharp objects using this mechanism. DomainTags are identified 
    /// by a <see cref="Guid"/>. There is no difference between an absent tag and
    /// a <b>null</b> tag value.
    /// </summary>
    public interface ITaggable
    {
        /// <summary>
        /// Gets a tag associated with the current declaration.
        /// </summary>
        /// <param name="guid">Tag identifier.</param>
        /// <returns>An object, or <b>null</b> if no tag of type <paramref name="guid"/>
        /// is associated to the current declaration.</returns>
        object GetTag( Guid guid );

        /// <summary>
        /// Set a tag to the current declaration.
        /// </summary>
        /// <param name="guid">Tag identifier.</param>
        /// <param name="value">Tag value.</param>
        void SetTag( Guid guid, object value );
    }
}