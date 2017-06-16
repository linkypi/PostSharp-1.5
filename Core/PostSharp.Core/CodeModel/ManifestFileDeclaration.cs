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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a file reference (<see cref="TokenType.File"/>). 
    /// </summary>
    /// <remarks>
    /// File references are owned
    /// by the <see cref="AssemblyManifestDeclaration"/>.
    /// </remarks>
    public sealed class ManifestFileDeclaration : NamedDeclaration, IWriteILDefinition,
                                                  IManifestResourceImplementation
    {
        #region Fields

        /// <summary>
        /// Hash value.
        /// </summary>
        private byte[] hash;

        /// <summary>
        /// Whether the file contains CLI metadata.
        /// </summary>
        private bool hasMetadata = true;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="ManifestFileDeclaration"/>.
        /// </summary>
        public ManifestFileDeclaration()
        {
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.File;
        }

        /// <summary>
        /// Determines whether the referenced file has CLI metadata.
        /// </summary>
        [ReadOnly(true)]
        public bool HasMetadata
        {
            get { return hasMetadata; }
            set { hasMetadata = value; }
        }

        /// <summary>
        /// Gets or sets the file hash value.
        /// </summary>
        /// <value>
        /// An array of bytes containing the file hash, or <b>null</b> if the hash value
        /// was not computed.
        /// </value>
        [ReadOnly(true)]
        [SuppressMessage("Microsoft.Performance", "CA1819",
            Justification = "We want to give full access to the byte array.")]
        public byte[] Hash
        {
            get { return hash; }
            set { hash = value; }
        }

        #region IWriteILDefinition Members

        /// <inheritdoc />
        public void WriteILDefinition(ILWriter writer)
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull(writer, "writer");

            #endregion

            writer.WriteKeyword(".file");

            if (!this.HasMetadata)
            {
                writer.WriteKeyword("nometadata");
            }

            writer.WriteFileName(this.Name);
            if (this.hash != null && this.hash.Length > 0)
            {
                writer.WriteLineBreak();
                writer.Indent++;
                writer.WriteKeyword(".hash");
                writer.WriteSymbol('=');
                writer.WriteBytes(this.hash);
                writer.Indent--;
            }
            writer.WriteLineBreak();

            // TODO: entry point.
        }

        #endregion

        internal override object GetReflectionObjectImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            return new NotSupportedException();
        }

        internal override object GetReflectionWrapperImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            throw new NotSupportedException();
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of assembly manifestFiles (<see cref="ManifestFileDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy(typeof (CollectionDebugViewer))]
        [DebuggerDisplay("{GetType().Name}, Count={Count}")]
        public sealed class ManifestFileDeclarationCollection : SimpleElementCollection<ManifestFileDeclaration>
        {
            internal ManifestFileDeclarationCollection( AssemblyManifestDeclaration parent, string role ) : base( parent, role )
            {
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
               ((IModuleScoped) this.Owner).Module.ModuleReader.ImportManifestFiles();
            }

        }
    }
}