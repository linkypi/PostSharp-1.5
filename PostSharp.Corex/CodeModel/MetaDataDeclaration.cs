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
using PostSharp.CodeModel.Collections;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Base class for all declarations represented in the metadata tables.
    /// Metadata declarations have a token (<see cref="MetadataToken"/>) and may have 
    /// custom attributes (<see cref="CustomAttributeDeclaration"/>).
    /// </summary>
    public abstract class MetadataDeclaration :
        Declaration, IMetadataDeclaration, IDisposable

    {
        #region Fields

        /// <summary>
        /// Metadata token of the current declaration.
        /// </summary>
        private MetadataToken metadataToken = MetadataToken.Null;

        /// <summary>
        /// Collection of custom attributes.
        /// </summary>
        private readonly CustomAttributeDeclarationCollection customAttributes;


        private bool isWeaklyReferenced;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="MetadataToken"/>.
        /// </summary>
        internal MetadataDeclaration()
        {
            this.customAttributes = new CustomAttributeDeclarationCollection( this, "customAttributes" );
        }

        /// <summary>
        /// Gets the <see cref="TokenType"/> of the derived declaration.
        /// </summary>
        /// <returns>A <see cref="TokenType"/>.</returns>
        /// <remarks>
        /// This allows to determines to which metadata table the declaration
        /// belongs.
        /// </remarks>
        [SuppressMessage( "Microsoft.Design", "CA1024",
            Justification = "It is rather a property of the class than of the instance." )]
        public abstract TokenType GetTokenType();


        /// <summary>
        /// Gets or sets the token of the current declaration.
        /// </summary>
        /// <value>
        /// A <see cref="MetadataToken"/>.
        /// </value>
        [ReadOnly( true )]
        public MetadataToken MetadataToken
        {
            get { return this.metadataToken; }
            internal set
            {
                if ( this.metadataToken != value )
                {
                    this.SetMetadataToken( value, true );
                }
            }
        }

        internal void SetMetadataToken( MetadataToken token, bool imported )
        {
#if ORDERED_EMIT
            MetadataToken oldValue = this.metadataToken;
            this.metadataToken = token;
            this.OnPropertyChanged( "MetadataToken", oldValue, token );
#else
            this.metadataToken = token;
#endif
            this.IsImported = true;
        }


        /// <summary>
        /// Gets the collection of custom attributes.
        /// </summary>
        [Browsable( false )]
        public CustomAttributeDeclarationCollection CustomAttributes
        {
            get
            {
                this.AssertNotDisposed();
                return this.customAttributes;
            }
        }

        object IMetadataDeclaration.GetReflectionWrapperObject( Type[] genericTypeArguments,
                                                                Type[] genericMethodArguments )
        {
            return this.GetReflectionWrapperImpl( genericTypeArguments, genericMethodArguments );
        }

        object IMetadataDeclaration.GetReflectionSystemObject( Type[] genericTypeArguments,
                                                               Type[] genericMethodArguments )
        {
            return this.GetReflectionObjectImpl( genericTypeArguments, genericMethodArguments );
        }

        internal abstract object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments );
        internal abstract object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments );


        /// <inheritdoc />
        public object GetTag( Guid guid )
        {
            ModuleDeclaration module = this.Module;

            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( module != null, "DeclarationHasNoModule" );

            #endregion

            return module.GetTagDictionary( guid )[this];
        }

        /// <inheritdoc />
        public void SetTag( Guid guid, object value )
        {
            ModuleDeclaration module = this.Module;

            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( module != null, "DeclarationHasNoModule" );
            ExceptionHelper.Core.AssertValidOperation( !this.isWeaklyReferenced, "TagOnWeaklyReferencedDeclaration" );

            #endregion

            module.GetTagDictionary( guid )[this] = value;
        }


        internal bool InternalIsWeaklyReferenced
        {
            get { return isWeaklyReferenced; }
            set
            {
                if ( this.isWeaklyReferenced != value )
                {
                    if ( !this.metadataToken.IsNull )
                    {
                        if ( !this.isWeaklyReferenced )
                        {
                            throw new InvalidOperationException();
                        }
                        else
                        {
                            this.Module.Tables.SetStrongReference( this );
                        }
                    }

                    isWeaklyReferenced = value;
                }
            }
        }


        internal override void OnAddingToParent( Element parent, string role )
        {
            base.OnAddingToParent( parent, role );

            if ( this.metadataToken.IsNull )
            {
                ModuleDeclaration module = this.Module;

                if ( this == module )
                {
                    return;
                }

                
                if ( this.isWeaklyReferenced )
                {
                    module.Tables.AddWeaklyReferencedDeclaration( this );
                }
                else
                {
                    module.Tables.AddStronglyReferencedDeclaration( this );
                }
            }
        }

        internal override void OnRemovingFromParent()
        {
            if ( !this.metadataToken.IsNull )
            {
                this.Module.Tables.RemoveDeclaration( this );
            }

            base.OnRemovingFromParent();
        }

        #region IDisposable Members

        /// <summary>
        /// Determines whether the current instance is disposed.
        /// </summary>
        [Browsable( false )]
        public bool IsDisposed { get; private set; }


        /// <summary>
        /// Throws an exception if the current 
        /// </summary>
        [Conditional( "ASSERT" )]
        internal void AssertNotDisposed()
        {
            if ( this.IsDisposed )
            {
                throw new ObjectDisposedException( this.GetType().FullName );
            }
        }


        /// <summary>
        /// Disposes whe current instance.
        /// </summary>
        /// <param name="disposing"><b>true</b> if the <see cref="Dispose()"/> method
        /// was explicitely called, <b>false</b> if this method is called by
        /// the destructor.</param>
        internal virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                this.customAttributes.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if ( !this.IsDisposed )
            {
                this.Dispose( true );
                this.IsDisposed = true;
            }
        }

        #endregion
    }
}