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
using PostSharp.CodeModel.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a permission attribute. 
    /// </summary>
    /// <remarks>
    /// Permission attributes are owned
    /// by permission sets (<see cref="PermissionSetDeclaration"/>).
    /// </remarks>
    public sealed class PermissionDeclaration : Declaration, IWriteILDefinition, IDisposable
    {
        #region Fields

        /// <summary>
        /// Collection of set properties.
        /// </summary>
        private MemberValuePairCollection properties = new MemberValuePairCollection( 1 );

        /// <summary>
        /// Type of the permission attribute.
        /// </summary>
        private ITypeSignatureInternal type;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="PermissionDeclaration"/>.
        /// </summary>
        public PermissionDeclaration()
        {
        }

        /// <summary>
        /// Gets the type of permission attribute.
        /// </summary>
        public ITypeSignature Type
        {
            get { return type; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                type = (ITypeSignatureInternal) value;
            }
        }

        /// <summary>
        /// Gets the list of properties.
        /// </summary>
        [Browsable( false )]
        public MemberValuePairCollection Properties
        {
            get
            {
                this.AssertNotDisposed();
                return properties;
            }
        }

        #region IWriteILDefinition Members

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            if ( this.type.DeclaringAssembly == this.Module.Assembly )
            {
                this.type.WriteILReference( writer, GenericMap.Empty, WriteTypeReferenceOptions.SerializedTypeReference );
            }
            else
            {
                this.type.WriteILReference( writer, GenericMap.Empty, WriteTypeReferenceOptions.None );
            }


            writer.WriteSymbol( '=' );
            writer.WriteSymbol( '{' );
            bool first = true;
            foreach ( MemberValuePair property in this.properties )
            {
                if ( first )
                {
                    first = false;
                }
                else
                {
                    //writer.WriteSymbol(',');
                    writer.WriteLineBreak();
                }

                property.WriteILDefinition( this.Module, writer );
            }
            writer.WriteSymbol( '}' );
        }

        #endregion

        [Conditional( "ASSERT" )]
        private void AssertNotDisposed()
        {
            if ( this.properties == null )
            {
                throw new ObjectDisposedException( "PermissionDeclaration" );
            }
        }

        /// <summary>
        /// Determines whether the current instance has been disposed.
        /// </summary>
        public bool IsDisposed { get { return this.properties == null; } }

        /// <inheritdoc />
        public void Dispose()
        {
            if ( this.properties != null )
            {
                this.properties.Dispose();
                this.properties = null;
            }
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of permission attributes (<see cref="PermissionDeclaration"/>).
        /// </summary>
        public sealed class PermissionDeclarationCollection : SimpleElementCollection<PermissionDeclaration>
        {
            /// <summary>
            /// Initializes a new <see cref="PermissionDeclarationCollection"/>.
            /// </summary>
            internal PermissionDeclarationCollection( Declaration parent, string role ) : base( parent, role )
            {
            }

            #region Overrides of ElementCollection<PermissionDeclaration>

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

            #endregion
        }
    }
}