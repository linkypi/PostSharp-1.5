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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a data section, where initial values of fieds are stored.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Data declarations are owned by the module (<see cref="ModuleDeclaration"/>)
    /// on the <see cref="ModuleDeclaration.Datas"/> property.
    /// </para>
    /// <para>
    /// Fields (<see cref="FieldDefDeclaration"/>) with fixed binary layout may be initialized to a value stored
    /// in a <see cref="DataSectionDeclaration"/> by setting the <see cref="FieldDefDeclaration.InitialValue"/>
    /// property.
    /// </para>
    /// </remarks>
    public sealed class DataSectionDeclaration : Declaration, IWriteILDefinition, IPositioned
    {
        #region Fields


        /// <summary>
        /// Name of the data declaration.
        /// </summary>
        private string name;

        /// <summary>
        /// Binary content of the data declaration.
        /// </summary>
        private byte[] data;

        /// <summary>
        /// Whether the current data is managed (?).
        /// </summary>
        private bool isManaged;

        /// <summary>
        /// Order of the data declaration in the module.
        /// </summary>
        private int ordinal;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="DataSectionDeclaration"/>.
        /// </summary>
        /// <param name="name">Data section name (label).</param>
        /// <param name="data">Raw data.</param>
        /// <param name="isManaged"><b>true</b> if the data section is managed,
        /// otherwise <b>false</b>.</param>
        public DataSectionDeclaration( string name, byte[] data, bool isManaged )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( name, "name" );
            ExceptionHelper.AssertArgumentNotNull( data, "data" );

            #endregion

            this.name = name;
            this.data = data;
            this.isManaged = isManaged;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the name of the data section.
        /// </summary>
        /// <value>
        /// The name (value) of the data section.
        /// </value>
        [ReadOnly( true )]
        public string Name
        {
            get { return name; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( name, "name" );

                #endregion

                name = value;
            }
        }

        /// <summary>
        /// Gets or sets the raw data.
        /// </summary>
        [ReadOnly( true )]
        [SuppressMessage( "Microsoft.Performance", "CA1819",
            Justification="We want to give full access to the byte array." )]
        public byte[] Value
        {
            get { return data; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                this.data = value;
            }
        }

        /// <summary>
        /// Determines whether the data section is managed.
        /// </summary>
        /// <value>
        /// <b>true</b> if the data section is managed, otherwise <b>false</b>.
        /// </value>
        [ReadOnly( true )]
        public bool IsManaged { get { return isManaged; } set { isManaged = value; } }

        /// <summary>
        /// Gets or set the emit order of the data section in the module.
        /// </summary>
        /// <value>
        /// The emit order of the current data section in the containing module.
        /// </value>
        [ReadOnly( true )]
        public int Ordinal
        {
            get { return ordinal; }
            set
            {
                int oldValue = this.ordinal;

                if ( value != this.ordinal )
                {
                    this.ordinal = value;
                    this.OnPropertyChanged( "Ordinal", oldValue, value );
                }
            }
        }

        #endregion

        #region writer IL

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );

            #endregion

            writer.WriteKeyword( ".data" );

            if ( this.isManaged && 
                (writer.Options.Compatibility & ILWriterCompatibility.IgnoreCilQualifierInDataSection) == 0)
            {
                writer.WriteKeyword( "cil" );
            }

            if ( !string.IsNullOrEmpty( this.name ) )
            {
                writer.WriteIdentifier( this.name );
                writer.WriteSymbol( '=' );
            }

            writer.WriteKeyword( "bytearray" );
            writer.WriteBytes( this.data, true );
            writer.WriteLineBreak();
        }


        /// <summary>
        /// Writes the name of the data section.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        internal void WriteILReference( ILWriter writer )
        {
            writer.WriteIdentifier( this.name );
        }

        #endregion

     
    }


    namespace Collections
    {
        /// <summary>
        /// Collection of data sections (<see cref="DataSectionDeclaration"/>.
        /// </summary>
        /// <remarks>
        /// This collection is populated lazily when clients (e.g. fields) are loaded, but it is not
        /// possible to load all the elements without loading all its clients before. Therefore,
        /// it is unsafe to enumerate this collection when it belongs to a module that has
        /// been loaded lazily.
        /// </remarks>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class DataDeclarationCollection : OrdinalDeclarationCollection<DataSectionDeclaration>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DataDeclarationCollection"/> type.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal DataDeclarationCollection( Declaration parent, string role )
                : base( parent, role )
            {
            }

            /// <inheritdoc />
            protected override bool IsLazyLoadingSupported
            {
                get { return false; }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
                throw new NotSupportedException();
            }
        }
    }
}
