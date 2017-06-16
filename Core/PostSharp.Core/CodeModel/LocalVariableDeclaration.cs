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
using System.Globalization;
using PostSharp.Collections;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a local variable. Local variables are owned by method bodies 
    /// (<see cref="MethodBodyDeclaration"/>).
    /// </summary>
    /// <remarks>
    /// A <see cref="LocalVariableDeclaration"/> it referenced by ordinal; it has
    /// no name. Positions are assigned to a name in lexical scopes. Lexical
    /// scopes are implemented by the <see cref="InstructionBlock"/> type and
    /// assignment of names to a local variable are done by the <see cref="LocalVariableSymbol"/>
    /// type.
    /// </remarks>
    public sealed class LocalVariableDeclaration : Declaration, ICloneable, IPositioned
    {
        #region Fields

        /// <summary>
        /// Local variable type.
        /// </summary>
        private ITypeSignatureInternal type;

        /// <summary>
        /// Ordinal of this local variable in the collection of variables of
        /// the parent method.
        /// </summary>
        private readonly int ordinal;

        #endregion

        /// <summary>
        /// Initializes a <see cref="LocalVariableDeclaration"/>.
        /// </summary>
        /// <param name="type">Local variable type.</param>
        /// <param name="ordinal">Ordinal.</param>
        /// <exception cref="ArgumentNullException">
        ///		The <paramref name="type"/> parameter is null.
        /// </exception>
        internal LocalVariableDeclaration( ITypeSignature type, int ordinal )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );

            #endregion

            this.type = (ITypeSignatureInternal) type;
            this.ordinal = ordinal;
        }

        /// <summary>
        /// Gets the local variable ordinal.
        /// </summary>
        /// <value>
        /// A positive integer.
        /// </value>
        public int Ordinal { get { return this.ordinal; } }

        int IPositioned.Ordinal { get { return this.ordinal;}}

        /// <summary>
        /// Gets or sets the local variable type.
        /// </summary>
        /// <value>
        /// Any <see cref="PostSharp.CodeModel.TypeSignature"/>.
        /// </value>
        /// <exception cref="ArgumentNullException">Trying to
        /// set the property to <b>null</b>.</exception>
        [ReadOnly( true )]
        public ITypeSignature Type
        {
            get { return type; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );
                this.AssertWritable();

                #endregion

                type = (ITypeSignatureInternal) value;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format( CultureInfo.InvariantCulture, "{0} : {1}",
                                  this.ordinal, this.type );
        }

       

        


        /// <summary>
        /// Determines whether the current variable is read-only.
        /// </summary>
        /// <remarks>
        /// A local variable may be read-only if it is owned by a 
        /// <see cref="StandaloneSignatureDeclaration"/>. This constraint is defined
        /// by PostSharp because, in this case, the variable is shared by many method
        /// bodies.
        /// </remarks>
        public bool IsReadOnly { get { return this.Parent is StandaloneSignatureDeclaration; } }

        [Conditional( "ASSERT" )]
        private void AssertWritable()
        {
            ExceptionHelper.Core.AssertValidOperation( !this.IsReadOnly, "LocalVariableReadOnly" );
        }

        internal override void OnAddingToParent( Element parent, string role )
        {
            this.AssertWritable();
            base.OnAddingToParent( parent, role );
        }

        internal override void OnRemovingFromParent()
        {
            this.AssertWritable();
            base.OnRemovingFromParent();
        }

        #region ICloneable Members

        /// <summary>
        /// Clones the current instance.
        /// </summary>
        /// <returns>A clone of the current instance (but unattached).</returns>
        public LocalVariableDeclaration Clone()
        {
            return new LocalVariableDeclaration( this.type, this.ordinal );
        }

        /// <inheritdoc />
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of local variables (<see cref="LocalVariableDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class LocalVariableDeclarationCollection : OrdinalDeclarationCollection<LocalVariableDeclaration>
        {
            internal LocalVariableDeclarationCollection( Declaration parent, string role )
                : base( parent, role )
            {
            }

            /// <summary>
            /// Determines whether the current collection of variables is read-only.
            /// </summary>
            /// <remarks>
            /// A collection of local variables may be read-only if it is owned by a 
            /// <see cref="StandaloneSignatureDeclaration"/>. This constraint is defined
            /// by PostSharp because, in this case, the collection is shared by many method
            /// bodies.
            /// </remarks>
            public override bool IsReadOnly { get { return this.Owner is StandaloneSignatureDeclaration; } }

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

            internal LocalVariableDeclarationCollection GetLocalCopy( MethodBodyDeclaration methodBody, string role )
            {
                LocalVariableDeclarationCollection localCopy =
                    new LocalVariableDeclarationCollection( methodBody, role );
                localCopy.EnsureCapacity( this.Count );
                localCopy.AddCloneRange( this );
                return localCopy;
            }
        }
    }
}