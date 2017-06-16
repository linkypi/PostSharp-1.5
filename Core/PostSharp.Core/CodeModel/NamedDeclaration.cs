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

using System.ComponentModel;
using System.Diagnostics;
using PostSharp.Collections;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Extends the <see cref="MetadataDeclaration"/> class with a <b>Name</b> property.
    /// </summary>
    [DebuggerDisplay( "{GetType().Name}: {ToString()}" )]
    public abstract class NamedDeclaration : MetadataDeclaration, INamed
    {
        #region Fields

        /// <summary>
        /// Property name.
        /// </summary>
        private string name;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="NamedDeclaration"/>.
        /// </summary>
        internal NamedDeclaration()
        {
        }

        /// <summary>
        /// Gets or sets the name of the current declaration.
        /// </summary>
        [ReadOnly( true )]
        public virtual string Name
        {
            get { return name; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                string oldValue = this.name;
                this.name = value;
                this.OnPropertyChanged( "Name", oldValue, value );
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.name;
        }
    }
}