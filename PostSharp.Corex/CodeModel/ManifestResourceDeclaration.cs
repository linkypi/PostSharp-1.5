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
using System.IO;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a resource referenced by the assembly (<see cref="TokenType.ManifestResource"/>).
    /// </summary>
    /// <remarks>
    /// Resources
    /// are owned by a <see cref="AssemblyManifestDeclaration"/>
    /// but can be stored physically ("implemented") in the current assemly,
    /// an external assembly (<see cref="AssemblyRefDeclaration"/>) or
    /// a file (<see cref="ManifestFileDeclaration"/>).
    /// </remarks>
    public sealed class ManifestResourceDeclaration : NamedDeclaration, IWriteILDefinition
    {
        #region Fields

        /// <summary>
        /// Whether the resource is visible outside the assembly.
        /// </summary>
        private bool isPublic;

        /// <summary>
        /// Determines where the resource is stored.
        /// </summary>
        private IManifestResourceImplementation implementation;

        #endregion

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.ManifestResource;
        }

        #region Properties

        /// <summary>
        /// Gets the offset of the resource in the external file.
        /// </summary>
        /// <value>
        /// A positive integer equal to the offset of the resource in case
        /// <see cref="Implementation"/> is a <see cref="ManifestFileDeclaration"/>,
        /// otherwise 0.
        /// </value>
        [ReadOnly( true )]
        public int FileOffset { get; set; }

        /// <summary>
        /// Determines whether the resource is public.
        /// </summary>
        /// <value>
        /// <b>true</b> if the resource is public, otherwise <b>false</b>.
        /// </value>
        [ReadOnly( true )]
        public bool IsPublic
        {
            get { return isPublic; }
            set { isPublic = value; }
        }

        /// <summary>
        /// Gets or sets the resource implementation, i.e. its physical location.
        /// </summary>
        /// <value>
        /// An <see cref="IManifestResourceImplementation"/> (<see cref="AssemblyRefDeclaration"/>,
        /// <see cref="ManifestFileDeclaration"/>), or <b>null</b> if the resource is
        /// stored in its declaring assembly.
        /// </value>
        [ReadOnly( true )]
        public IManifestResourceImplementation Implementation
        {
            get { return implementation; }
            set { implementation = value; }
        }

        /// <summary>
        /// Gets or sets a <see cref="Stream"/> giving the resource content.
        /// </summary>
        /// <value>
        /// A <see cref="Stream"/> with <b>Seek</b> capability, or <b>null</b>
        /// if the resource is not stored in the declaring assembly.
        /// </value>
        [ReadOnly( true )]
        public Stream ContentStream { get; set; }

        #endregion

        #region IWriteILDefinition Members

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );

            #endregion

            writer.WriteKeyword( ".mresource" );
            writer.WriteKeyword( this.isPublic ? "public" : "private" );
            writer.WriteFileName( this.Name );
            writer.WriteLineBreak();
            writer.BeginBlock();
            this.CustomAttributes.WriteILDefinition( writer );
            if ( this.implementation != null )
            {
                AssemblyRefDeclaration externalAssembly = this.implementation as AssemblyRefDeclaration;

                if ( externalAssembly != null )
                {
                    writer.WriteKeyword( ".assembly extern" );
                    writer.WriteDottedName( externalAssembly.Name );
                    writer.WriteLineBreak();
                }
                else
                {
                    ManifestFileDeclaration file = this.implementation as ManifestFileDeclaration;

                    if ( file != null )
                    {
                        writer.WriteKeyword( ".file" );
                        writer.WriteDottedName( file.Name );
                        writer.WriteKeyword( "at" );
                        writer.WriteInteger( this.FileOffset );
                        writer.WriteLineBreak();
                    }
                    else
                    {
                        throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidType",
                                                                                   this.implementation.GetType(),
                                                                                   "this.Implementation",
                                                                                   "AssemblyRefDeclaration, ManifestFileDeclaration" );
                    }
                }
            }
            writer.EndBlock();
        }

        #endregion

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return new NotSupportedException();
        }

        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            throw new NotSupportedException();
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of resources (<see cref="ManifestResourceDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class ManifestResourceDeclarationCollection :
            SimpleElementCollection<ManifestResourceDeclaration>
        {
            /// <summary>
            /// Initializes a new <see cref="ManifestResourceDeclarationCollection"/>.
            /// </summary>
            internal ManifestResourceDeclarationCollection( AssemblyManifestDeclaration parent, string role )
                : base( parent, role )
            {
            }

            /// <inheritdoc />
            protected override bool IsLazyLoadingSupported
            {
                get { return true; }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
                ( (IModuleScoped) this.Owner ).Module.ModuleReader.ImportManifestResources();
            }
        }
    }
}