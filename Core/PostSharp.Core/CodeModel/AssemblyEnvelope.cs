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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using PostSharp.CodeModel.Collections;
using PostSharp.CodeModel.Helpers;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents an assembly understood basically, internally, as a collection 
    /// of modules (<see cref="ModuleDeclaration"/>) and, externally, as a collection
    /// of exported types.
    /// </summary>
    /// <remarks>
    /// Do not mistake for <see cref="AssemblyManifestDeclaration"/>, which is the assembly manifest and is contained
    /// in one of the module of the assembly.
    /// </remarks>
    public sealed class AssemblyEnvelope : Element, IAssemblyInternal, INamed
    {
        private readonly ModuleDeclarationCollection modules;
        private readonly TagCollection tags = new TagCollection();
        private AssemblyManifestDeclaration manifest;
        private ModuleDeclaration manifestModule;
        private readonly string location;

        internal AssemblyEnvelope( string location )
        {
            this.modules = new ModuleDeclarationCollection( this, "module" );
            this.location = location;
        }

        /// <summary>
        /// Gets a type defined in this assembly given its name.
        /// </summary>
        /// <param name="typeName">Type name (in Reflection format).</param>
        /// <returns>The <see cref="TypeDefDeclaration"/> named <paramref name="typeName"/>,
        /// or <b>null</b> if there is no type named <paramref name="typeName"/> in this assembly.s</returns>
        public TypeDefDeclaration GetTypeDefinition( string typeName )
        {
            return this.GetTypeDefinition( typeName, BindingOptions.Default );
        }


        /// <summary>
        /// Gets a type defined in this assembly given its name.
        /// </summary>
        /// <param name="typeName">Type name (in Reflection format).</param>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns>The <see cref="TypeDefDeclaration"/> named <paramref name="typeName"/>,
        /// or <b>null</b> if there is no type named <paramref name="typeName"/> in this assembly.s</returns>
        public TypeDefDeclaration GetTypeDefinition( string typeName, BindingOptions bindingOptions )
        {
            foreach ( ModuleDeclaration module in modules )
            {
                TypeDefDeclaration typeDef = module.FindType( typeName,
                                                              BindingOptions.OnlyExisting |
                                                              BindingOptions.DontThrowException |
                                                              BindingOptions.OnlyDefinition )
                                             as TypeDefDeclaration;

                if ( typeDef != null )
                    return typeDef;
            }

            if ( ( bindingOptions & BindingOptions.DontThrowException ) == 0 )
            {
                throw ExceptionHelper.Core.CreateBindingException( "CannotFindTypeInCurrentAssembly",
                                                                   typeName, this.FullName );
            }
            else return null;
        }

        /// <inheritdoc />
        AssemblyEnvelope IAssembly.GetAssemblyEnvelope()
        {
            return this;
        }

        /// <inheritdoc />
        public bool MatchesReference( IAssemblyName assemblyName )
        {
            return CompareHelper.Equals( this, assemblyName, this.Domain.AssemblyRedirectionPolicies, false );
        }

        /// <summary>
        /// Gets the full path of the file from which the assembly has been loaded.
        /// </summary>
        public string Location
        {
            get { return this.location; }
        }

        /// <summary>
        /// Gets the collection of modules contained in this assembly.
        /// </summary>
        public ModuleDeclarationCollection Modules
        {
            get { return this.modules; }
        }

        /// <summary>
        /// Gets the module containing the assembly manifest (<see cref="AssemblyManifestDeclaration"/>).
        /// </summary>
        public ModuleDeclaration ManifestModule
        {
            get { return this.manifestModule; }
            internal set
            {
                if ( value != null )
                {
                    string oldName;

                    if ( this.manifestModule != null )
                    {
                        oldName = this.manifestModule.Name;
                        this.manifestModule.AssemblyManifest = null;
                    }
                    else
                    {
                        oldName = "<loading>";
                    }

                    this.manifestModule = value;
                    this.manifest = value.AssemblyManifest;
                    this.OnPropertyChanged( "Name", oldName, value );
                }
                else
                {
                    this.manifestModule = null;
                    this.manifest = null;
                }
            }
        }

        #region IAssembly

        /// <inheritdoc />
        public string Name
        {
            get { return this.manifest == null ? "<loading>" : this.manifest.Name; }
        }

        /// <inheritdoc />
        public string FullName
        {
            get { return this.manifest.FullName; }
        }

        /// <inheritdoc />
        public Version Version
        {
            get { return this.manifest.Version; }
        }

        /// <inheritdoc />
        [SuppressMessage( "Microsoft.Performance", "CA1819",
            Justification = "We will anyway give full access to the byte array." )]
        public byte[] GetPublicKey()
        {
            return this.manifest.GetPublicKey();
        }

        /// <inheritdoc />
        public byte[] GetPublicKeyToken()
        {
            return this.manifest.GetPublicKeyToken();
        }

        /// <inheritdoc />
        public string Culture
        {
            get { return this.manifest.Culture; }
        }

        /// <inheritdoc />
        public bool IsMscorlib
        {
            get { return this.manifest.IsMscorlib; }
        }

        /// <inheritdoc />
        void IAssemblyInternal.WriteILReference( ILWriter writer )
        {
        }

        bool IAssemblyInternal.Equals( IAssembly assembly, bool strict )
        {
            return CompareHelper.Equals( this, assembly, this.Domain.AssemblyRedirectionPolicies, strict );
        }

        #endregion

        #region Binding

        /// <inheritdoc />
        public Assembly GetSystemAssembly()
        {
            Module module = this.manifestModule.GetSystemModule();
            return module != null ? module.Assembly : null;
        }

        #endregion

        /// <inheritdoc />
        public bool Equals( IAssembly other )
        {
            return CompareHelper.Equals( this, other, this.Domain.AssemblyRedirectionPolicies, true );
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "AssemblyEnvelope: " + this.Name;
        }

        #region ITaggable Members

        /// <inheritdoc />
        public object GetTag( Guid guid )
        {
            return this.tags.GetTag( guid );
        }

        /// <inheritdoc />
        public void SetTag( Guid guid, object value )
        {
            this.tags.SetTag( guid, value );
        }

        #endregion

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose()
        {
            this.modules.Dispose();
        }

        #endregion
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of assemblies (<see cref="AssemblyEnvelope"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class AssemblyEnvelopeCollection : NonUniquelyNamedElementCollection<AssemblyEnvelope>
        {
            internal AssemblyEnvelopeCollection( Element parent, string role ) :
                base( parent, role )
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