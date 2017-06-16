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
using System.Reflection;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a type exported by the current assembly but not defined in the current
    /// module.
    /// </summary>
    /// <remarks>
    /// Exported types are owned by the assembly manifest (<see cref="AssemblyManifestDeclaration"/>.
    /// </remarks>
    public sealed class ExportedTypeDeclaration : NamedDeclaration, IWriteILDefinition,
                                                  IManifestResourceImplementation
    {
        private TypeAttributes attributes;
        private long typeDefId;

        private IManifestResourceImplementation implementation;

        /// <summary>
        /// Gets or sets the type attributes.
        /// </summary>
        public TypeAttributes Attributes { get { return attributes; } set { attributes = value; } }


        /// <summary>
        /// Gets or sets the identifier of the type definition.
        /// </summary>
        public long TypeDefId { get { return typeDefId; } set { typeDefId = value; } }

        /// <summary>
        /// Gets or sets the external class or the external file implementing
        /// this type.
        /// </summary>
        public IManifestResourceImplementation Implementation { get { return implementation; } set { implementation = value; } }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.ExportedType;
        }

        /// <summary>
        /// Gets the parent exported type, if the current type is a nested one.
        /// </summary>
        public ExportedTypeDeclaration DeclaringExportedType { get { return this.implementation as ExportedTypeDeclaration; } }

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            writer.WriteKeyword( ".class extern" );
            switch ( this.attributes & TypeAttributes.VisibilityMask )
            {
                case TypeAttributes.Public:
                    writer.WriteKeyword( "public" );
                    break;

                case TypeAttributes.NotPublic:
                    writer.WriteKeyword( "private" );
                    break;

                case TypeAttributes.NestedAssembly:
                    writer.WriteKeyword( "nested assembly" );
                    break;

                case TypeAttributes.NestedPrivate:
                    writer.WriteKeyword( "nested private" );
                    break;

                case TypeAttributes.NestedFamily:
                    writer.WriteKeyword( "nested family" );
                    break;

                case TypeAttributes.NestedFamANDAssem:
                    writer.WriteKeyword( "nested famandassem" );
                    break;

                case TypeAttributes.NestedFamORAssem:
                    writer.WriteKeyword( "nested famorassem" );
                    break;

                case TypeAttributes.NestedPublic:
                    writer.WriteKeyword( "nested public" );
                    break;
            }
            writer.WriteDottedName( this.Name );
            writer.WriteLineBreak();
            writer.BeginBlock();

            AssemblyRefDeclaration externalAssembly = this.implementation as AssemblyRefDeclaration;
            ManifestFileDeclaration file;
            ExportedTypeDeclaration otherExportedFile;
            if ( externalAssembly != null )
            {
                writer.WriteKeyword( ".assembly extern" );
                writer.WriteDottedName( externalAssembly.Name );
                writer.WriteLineBreak();
            }
            else if ( ( file = this.implementation as ManifestFileDeclaration ) != null )
            {
                writer.WriteKeyword( ".file" );
                writer.WriteDottedName( file.Name );
                writer.WriteLineBreak();
            }
            else if ( ( otherExportedFile = this.implementation as ExportedTypeDeclaration ) != null )
            {
                writer.WriteKeyword( ".class" );
                otherExportedFile.WriteILReference( writer, WriteTypeReferenceOptions.WriteTypeKind );
                writer.WriteLineBreak();
            }
            else
            {
                throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidType",
                                                                           this.implementation.GetType(),
                                                                           "this.Implementation",
                                                                           "AssemblyRefDeclaration, ManifestFileDeclaration" );
            }

            writer.WriteKeyword( ".class" );
            writer.WriteInteger( this.typeDefId, IntegerFormat.HexLower );
            writer.WriteLineBreak();
            writer.EndBlock();
        }

        internal void WriteILReference( ILWriter writer, WriteTypeReferenceOptions options )
        {
            if ( ( options & WriteTypeReferenceOptions.WriteTypeKind ) != 0 )
            {
                writer.WriteKeyword( "extern" );
            }

            ExportedTypeDeclaration parent = this.DeclaringExportedType;

            if ( parent != null )
            {
                parent.WriteILReference( writer, options & ~WriteTypeReferenceOptions.WriteTypeKind );
                writer.WriteSymbol( '/' );
            }

            writer.WriteDottedName( this.Name );
        }

        internal override object GetReflectionObjectImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            throw new NotSupportedException();
        }

        internal override object GetReflectionWrapperImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            throw new NotSupportedException();
        }
    }


    namespace Collections
    {
        /// <summary>
        /// Collection of exported types (<see cref="ExportedTypeDeclaration"/>).
        /// </summary>
        public sealed class ExportedTypeDeclarationCollection :
            OrderedEmitAndByUniqueNameDeclarationCollection<ExportedTypeDeclaration>
        {
            internal ExportedTypeDeclarationCollection( AssemblyManifestDeclaration parent, string role )
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

                ((IModuleScoped) this.Owner).Module.ModuleReader.ImportExportedTypes();
            }
        }
    }
}