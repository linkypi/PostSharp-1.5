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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Assigns a name to a local variable <see cref="LocalVariableDeclaration"/>.
    /// Local variable symbols belong to an <see cref="InstructionBlock"/>, which
    /// is in the current case the lexical scope of the variable.
    /// </summary>
    public sealed class LocalVariableSymbol : IComparable<LocalVariableSymbol>
    {
        #region Fields

        /// <summary>
        /// The named <see cref="LocalVariableDeclaration"/>.
        /// </summary>
        private LocalVariableDeclaration localVariable;

        /// <summary>
        /// The local variable name, or <b>null</b> if the local variable is
        /// not named in the current scope.
        /// </summary>
        private string name;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="LocalVariableSymbol"/>.
        /// </summary>
        /// <param name="localVariable">The named <see cref="LocalVariableDeclaration"/>.</param>
        /// <param name="name">The name, or <b>null</b> to wrap a local variable without
        /// symbol into a <see cref="LocalVariableSymbol"/>.</param>
        internal LocalVariableSymbol( LocalVariableDeclaration localVariable, string name )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( localVariable, "localVariable" );

            #endregion

            this.localVariable = localVariable;
            this.name = name;
        }

        /// <summary>
        /// Gets or sets the named <see cref="LocalVariableDeclaration"/>.
        /// </summary>
        /// <value>
        /// A <see cref="LocalVariableDeclaration"/>.
        /// </value>
        [ReadOnly( true )]
        public LocalVariableDeclaration LocalVariable
        {
            get { return localVariable; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                localVariable = value;
            }
        }

        /// <summary>
        /// Gets or sets the local variable name.
        /// </summary>
        /// <value>
        /// The local variable name, or <b>null</b> if the local
        /// variable is not named in the current scope.
        /// </value>
        [ReadOnly( true )]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        #region writer IL

        /// <summary>
        /// Writes the name of the current symbol.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        internal void WriteILReference( ILWriter writer )
        {
            if ( this.name != null )
            {
                writer.WriteIdentifier( this.name );
            }
            else
            {
                writer.WriteIdentifier( "V_" + this.localVariable.Ordinal.ToString( CultureInfo.InvariantCulture ) );
            }
        }

        /// <summary>
        /// Writes the IL definition of the current symbol.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        /// <param name="genericMap">The <see cref="GenericMap"/> of the
        /// enclosing method.</param>
        internal void WriteILDefinition( ILWriter writer, GenericMap genericMap )
        {
            writer.WriteSymbol( '[' );
            writer.WriteInteger( this.LocalVariable.Ordinal, IntegerFormat.Decimal );
            writer.WriteSymbol( "] " );
            ( (ITypeSignatureInternal) this.localVariable.Type ).WriteILReference( writer, genericMap,
                                                                                   WriteTypeReferenceOptions.
                                                                                       WriteTypeKind );
            writer.WriteIdentifier( this.Name );
        }

        #endregion

        #region IComparable<LocalVariableSymbol> Members

        /// <inheritdoc />
        public int CompareTo( LocalVariableSymbol other )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( other, "other" );

            #endregion

            return this.localVariable.Ordinal - other.localVariable.Ordinal;
        }

        /// <inheritdoc />
        public bool Equals( LocalVariableSymbol other )
        {
            if ( other == null )
            {
                return false;
            }

            return ReferenceEquals( this, other );
        }

        #endregion
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of local variable symbols (<see cref="LocalVariableSymbol"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class LocalVariableSymbolCollection : Collection<LocalVariableSymbol>
        {
            /// <summary>
            /// Initializes a new <see cref="LocalVariableSymbolCollection"/>.
            /// </summary>
            /// <param name="capacity">Initial capacity.</param>
            internal LocalVariableSymbolCollection( int capacity ) : base( new List<LocalVariableSymbol>( capacity ) )
            {
            }


            /// <summary>
            /// Copies the current collection into a new array.
            /// </summary>
            /// <returns>A new array containing the elements of the current collection.</returns>
            public LocalVariableSymbol[] ToArray()
            {
                return ( (List<LocalVariableSymbol>) this.Items ).ToArray();
            }
        }
    }
}