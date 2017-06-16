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
using System.Configuration.Assemblies;
using PostSharp.CodeModel.Collections;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents an assembly manifest (<see cref="TokenType.Assembly"/>).
    /// </summary>
    /// <remarks>
    /// The assembly manifest
    /// is owned by the module (<see cref="ModuleDeclaration"/>) and is exposed by the
    /// <see cref="ModuleDeclaration.AssemblyManifest"/>
    /// property.
    /// </remarks>
    public sealed class AssemblyManifestDeclaration : NamedDeclaration,
                                                      IWriteILDefinition, ISecurable, IAssemblyName
    {
        #region Fields

        /// <summary>
        /// Collection of permission sets (<see cref="PermissionSetDeclaration"/>).
        /// </summary>
        private readonly PermissionSetDeclarationCollection permissionSets;

        /// <summary>
        /// Collection of manifestFiles referenced to by the assembly (<see cref="ManifestFileDeclaration"/>).
        /// </summary>
        private readonly ManifestFileDeclarationCollection files;

        /// <summary>
        /// Collection of resources referenced to by the assembly (<see cref="ManifestResourceDeclaration"/>).
        /// </summary>
        private readonly ManifestResourceDeclarationCollection resources;

        private readonly ExportedTypeDeclarationCollection exportedTypes;

        /// <summary>
        /// Name of the assembly culture.
        /// </summary>
        /// <value>
        /// A standard culture name, or <b>null</b> if the assembly is culturally neutral.
        /// </value>
        private string culture;

        /// <summary>
        /// Assembly version.
        /// </summary>
        private Version version;

        /// <summary>
        /// Assembly public key.
        /// </summary>
        /// <value>
        /// The public key, or <b>null</b> if the assembly is not strongly signed.
        /// </value>
        private byte[] publicKey;

        /// <summary>
        /// The hash algorithm to be used when signing the assembly.
        /// </summary>
        private AssemblyHashAlgorithm hashAlgorithm;

        private string overwrittenName;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="AssemblyManifestDeclaration"/>.
        /// </summary>
        public AssemblyManifestDeclaration()
        {
            this.permissionSets = new PermissionSetDeclarationCollection( this, "permissionSets" );
            this.exportedTypes = new ExportedTypeDeclarationCollection( this, "exportedTypes" );
            this.resources = new ManifestResourceDeclarationCollection( this, "resources" );
            this.files = new ManifestFileDeclarationCollection( this, "files" );
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.Assembly;
        }

        #region Properties

        /// <summary>
        /// Gets the parent <see cref="ModuleDeclaration"/>.
        /// </summary>
        [Browsable( false )]
        public new ModuleDeclaration Parent
        {
            get { return (ModuleDeclaration) base.Parent; }
        }

        /// <summary>
        /// Gets the name of the assembly culture.
        /// </summary>
        /// <value>
        /// A standard culture name (see <see cref="System.Globalization.CultureInfo"/>), or
        /// <b>null</b> to specify the neutral culture.
        /// </value>
        [ReadOnly( true )]
        public string Culture
        {
            get { return culture; }
            set
            {
                if ( string.IsNullOrEmpty( value ) )
                {
                    this.culture = null;
                }
                else
                {
                    culture = value;
                }
            }
        }

        /// <inheritdoc />
        [ReadOnly( true )]
        public byte[] GetPublicKey()
        {
            return publicKey;
        }

        /// <summary>
        /// Sets the public key.
        /// </summary>
        /// <param name="value"></param>
        public void SetPublicKey( byte[] value )
        {
            publicKey = value;
        }

        /// <inheritdoc />
        public byte[] GetPublicKeyToken()
        {
            if ( this.publicKey != null )
            {
                return AssemblyNameHelper.ComputeKeyToken( this.publicKey );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets or set the hash algorithm identifier.
        /// </summary>
        /// <value>
        /// The identifier of the algorithm (an <see cref="AssemblyHashAlgorithm"/>) used to produce the assembly signature.
        /// </value>
        [ReadOnly( true )]
        public AssemblyHashAlgorithm HashAlgorithm
        {
            get { return hashAlgorithm; }
            set { hashAlgorithm = value; }
        }

        /// <summary>
        /// Gets or set the assembly version.
        /// </summary>
        /// <value>
        /// A <see cref="System.Version"/>.
        /// </value>
        [ReadOnly( true )]
        public Version Version
        {
            get { return version; }
            set { version = value; }
        }

        /// <inheritdoc />
        [Browsable( false )]
        public PermissionSetDeclarationCollection PermissionSets
        {
            get
            {
                this.AssertNotDisposed();
                return this.permissionSets;
            }
        }

        /// <summary>
        /// Gets the assembly full name.
        /// </summary>
        /// <value>
        /// A value of the form: <c>{Name}, Version={Version}, Culture={Culture}|neutral, 
        /// PublicKey[Token]={PublicKey}</c>.
        /// </value>
        public string FullName
        {
            get { return GetFullName( this.Name, false ); }
        }

        /// <summary>
        /// Gets the full name based on the <see cref="OverwrittenName"/> instead
        /// of the normal <see cref="NamedDeclaration.Name"/>.
        /// </summary>
        /// <value>
        /// A value of the form: <c>{Name}, Version={Version}, Culture={Culture}|neutral</c>
        /// (in any case without public key), or <b>null</b> if the <see cref="OverwrittenName"/>
        /// property is <b>null</b>.
        /// </value>
        public string OverwrittenFullName
        {
            get
            {
                if ( string.IsNullOrEmpty( this.overwrittenName ) )
                {
                    return null;
                }
                else
                {
                    return GetFullName( this.overwrittenName, true );
                }
            }
        }

        private string GetFullName( string name, bool ignorePublicKey )
        {
            return AssemblyNameHelper.FormatAssemblyFullName(
                name, this.version, this.culture, ignorePublicKey ? null : this.GetPublicKeyToken() );
        }

        /// <summary>
        /// Gets the collection of manifestFiles (<see cref="ManifestFileDeclaration"/>) linked to
        /// the assembly.
        /// </summary>
        [Browsable( false )]
        public ManifestFileDeclarationCollection Files
        {
            get { return this.files; }
        }

        /// <summary>
        /// Gets the collection of resources (<see cref="ManifestResourceDeclaration"/>)
        /// referred to by the assembly.
        /// </summary>
        [Browsable( false )]
        public ManifestResourceDeclarationCollection Resources
        {
            get { return this.resources; }
        }

        /// <summary>
        /// Determines whether the current assembly is <b>mscorlib</b>.
        /// </summary>
        /// <value>
        /// <b>true</b> if the current assembly is <b>mscorlib</b>, otherwise <b>false</b>.
        /// </value>
        public bool IsMscorlib
        {
            get { return this.Module.IsMscorlib; }
        }

        /// <summary>
        /// Name of the assembly as it will be <i>emitted</i> during compilation.
        /// </summary>
        /// <remarks>
        /// <para>Overwriting an assembly name allows to change the name of the <i>compiled</i>
        /// assembly without breaking references in the current domain (because changing
        /// the <see cref="NamedDeclaration.Name"/> would make it impossible to resolve assembly references).</para>
        /// <para>When the name is overwritten, the public key is automatically ignored
        /// during compilation.</para>
        /// </remarks>
        /// <value>A valid name, or <b>null</b> if the name is not overwritten.</value>
        [ReadOnly( true )]
        public string OverwrittenName
        {
            get { return this.overwrittenName; }
            set { this.overwrittenName = value; }
        }

        /// <summary>
        /// Gets the collection of types exported outside the current assembly but not defined
        /// inside the current module.
        /// </summary>
        [Browsable( false )]
        public ExportedTypeDeclarationCollection ExportedTypes
        {
            get { return this.exportedTypes; }
        }

        #endregion

        #region IWriteILDefinition Members

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );

            #endregion

            // Assembly name
            writer.WriteKeyword( ".assembly" );

            if ( string.IsNullOrEmpty( this.overwrittenName ) )
            {
                writer.WriteDottedName( this.Name );
            }
            else
            {
                writer.WriteDottedName( this.overwrittenName );
            }

            writer.WriteLineBreak();
            writer.BeginBlock();

            // writer custom attributes
            this.CustomAttributes.WriteILDefinition( writer );
            this.permissionSets.WriteILDefinition( writer );

            if ( string.IsNullOrEmpty( this.overwrittenName ) )
            {
                // Public key
                if ( this.publicKey != null )
                {
                    writer.WriteKeyword( ".publickey" );
                    writer.WriteSymbol( '=' );
                    writer.WriteBytes( publicKey );
                    writer.WriteLineBreak();
                }

                // Hash algorithm
                writer.WriteKeyword( ".hash algorithm" );
                writer.WriteInteger( (int) this.hashAlgorithm );
                writer.WriteLineBreak();
            }

            // Version
            writer.WriteKeyword( ".ver" );
            writer.WriteInteger( this.version.Major, IntegerFormat.Decimal );
            writer.WriteSymbol( ':' );
            writer.WriteInteger( this.version.Minor, IntegerFormat.Decimal );
            writer.WriteSymbol( ':' );
            writer.WriteInteger( this.version.Build, IntegerFormat.Decimal );
            writer.WriteSymbol( ':' );
            writer.WriteInteger( this.version.Revision, IntegerFormat.Decimal );
            writer.WriteLineBreak();

            // Culture
            if ( !string.IsNullOrEmpty( this.culture ) )
            {
                writer.WriteKeyword( ".culture " );
                writer.WriteQuotedString( this.culture, WriteStringOptions.DoubleQuoted );
                writer.WriteLineBreak();
            }
            writer.EndBlock();

            foreach ( ManifestFileDeclaration file in this.files )
            {
                file.WriteILDefinition( writer );
            }

            foreach ( ExportedTypeDeclaration exportedType in this.exportedTypes )
            {
                exportedType.WriteILDefinition( writer );
            }

            foreach ( ManifestResourceDeclaration resource in this.resources )
            {
                resource.WriteILDefinition( writer );
            }
        }

        #endregion

        #region IDisposable Members

        internal override void Dispose( bool disposing )
        {
            base.Dispose( disposing );

            if ( disposing )
            {
                this.permissionSets.Dispose();
            }
        }

        #endregion

        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return new AssemblyWrapper( this.DeclaringAssembly );
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.Module.GetSystemModule().Assembly;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.FullName;
        }
    }
}