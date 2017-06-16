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
using PostSharp.Collections;
using PostSharp.ModuleWriter;
using PostSharp.Utilities;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a reference to another module in the same assembly (<see cref="TokenType.ModuleRef"/>).
    /// </summary>
    /// <remarks>
    /// Module references are contained by modules (<see cref="ModuleDeclaration"/>).
    /// </remarks>
    public sealed class ModuleRefDeclaration : NamedDeclaration, IMemberRefResolutionScope, IModuleInternal,
                                               ITypeRefResolutionScope, IWriteILDefinition
    {
        #region Fields

        /// <summary>
        /// Collection of type references.
        /// </summary>
        private readonly TypeRefDeclarationCollection typeRefs;

        /// <summary>
        /// Collection of field references.
        /// </summary>
        private readonly FieldRefDeclarationCollection fieldRefs;

        /// <summary>
        /// Collection of method references.
        /// </summary>
        private readonly MethodRefDeclarationCollection methodRefs;

        private ModuleDeclaration cachedModuleDef;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="ModuleRefDeclaration"/>.
        /// </summary>
        public ModuleRefDeclaration()
        {
            this.typeRefs = new TypeRefDeclarationCollection( this, "externalTypes" );
            this.fieldRefs = new FieldRefDeclarationCollection( this, "externalFields" );
            this.methodRefs = new MethodRefDeclarationCollection( this, "externalMethods" );
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.ModuleRef;
        }


        GenericMap IGeneric.GetGenericContext( GenericContextOptions options )
        {
            return GenericMap.Empty;
        }

        bool IGeneric.IsGenericDefinition { get { return false; } }

        bool IGeneric.IsGenericInstance { get { return false; } }

        #region Properties

        /// <inheritdoc />
        public IAssembly Assembly { get { return this.Module.Assembly; } }

        /// <summary>
        /// Gets the collection of type references (<see cref="TypeRefDeclaration"/>)
        /// declared in the referenced module.
        /// </summary>
        [Browsable( false )]
        public TypeRefDeclarationCollection TypeRefs
        {
            get
            {
                this.AssertNotDisposed();
                return this.typeRefs;
            }
        }

        /// <summary>
        /// Gets the collection of field references (<see cref="FieldRefDeclaration"/>)
        /// declared in the referenced module.
        /// </summary>
        [Browsable( false )]
        public FieldRefDeclarationCollection FieldRefs
        {
            get
            {
                this.AssertNotDisposed();
                return this.fieldRefs;
            }
        }

        /// <summary>
        /// Gets the collection of method references (<see cref="MethodRefDeclaration"/>)
        /// declared in the referenced module.
        /// </summary>
        [Browsable( false )]
        public MethodRefDeclarationCollection MethodRefs
        {
            get
            {
                this.AssertNotDisposed();
                return this.methodRefs;
            }
        }

        #endregion

        #region writer IL

        /// <inheritdoc />
        void IModuleInternal.WriteILReference( ILWriter writer )
        {
            writer.WriteSpace();
            writer.WriteSymbol( '[' );
            writer.WriteKeyword( ".module" );
            writer.WriteDottedName( this.Name );
            writer.WriteSymbol( ']', SymbolSpacingKind.None, SymbolSpacingKind.None );
        }

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );

            #endregion

            writer.WriteKeyword( ".module extern" );
            writer.WriteDottedName( this.Name );
            writer.WriteLineBreak();
        }

        #endregion

        /// <summary>
        /// Gets the <see cref="ModuleDeclaration"/> corresponding to the current
        /// reference.
        /// </summary>
        /// <returns>The <see cref="ModuleDeclaration"/> corresponding to the current
        /// reference.</returns>
        public ModuleDeclaration GetModuleDefinition()
        {
            if (this.cachedModuleDef == null)
            {
                this.cachedModuleDef = this.Module.Assembly.Modules.GetByName( this.Name );
            }

            return cachedModuleDef;
        }

        internal override object GetReflectionWrapperImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            throw new NotSupportedException();
        }

        internal override object GetReflectionObjectImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            throw new NotSupportedException();
        }

        internal override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            if ( disposing )
            {
                this.methodRefs.Dispose();
                this.fieldRefs.Dispose();
                this.typeRefs.Dispose();
            }
        }

        /// <inheritdoc />
        public ITypeSignature FindType( string typeName, BindingOptions options )
        {
            return this.typeRefs.FindType( typeName, options );
        }

        /// <inheritdoc />
        public override void ClearCache()
        {
            base.ClearCache();
            this.cachedModuleDef = null;
            this.typeRefs.ClearCache();
            this.fieldRefs.ClearCache();
            this.methodRefs.ClearCache();
            
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of module references (<see cref="ModuleRefDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class ModuleRefDeclarationCollection :
            OrderedEmitAndByUniqueNameDeclarationCollection<ModuleRefDeclaration>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ModuleRefDeclarationCollection"/>
            /// type with zero initial capacity.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal ModuleRefDeclarationCollection( Declaration parent, string role )
                : base( parent, role )
            {
            }

            /// <inheritdoc />
            protected override bool RequiresEmitOrdering
            {
                get
                {
#if ORDERED_EMIT
                    return true;
#else
                    return false;
#endif
                }
            }


            /// <inheritdoc />
            protected override bool IsLazyLoadingSupported
            {
                get
                {
                    return true;
                }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
                ((ModuleDeclaration) this.Owner).ModuleReader.ImportModuleRefs();
            }
      
        }
    }
}
