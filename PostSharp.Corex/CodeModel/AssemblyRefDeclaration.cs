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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using PostSharp.CodeModel.Collections;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a reference to an external assembly (<see cref="TokenType.AssemblyRef"/>).
    /// </summary>
    /// <remarks>
    /// Assembly references are owned
    /// by the module  (<see cref="ModuleDeclaration"/>) an are exposed by
    /// the <see cref="ModuleDeclaration.AssemblyRefs"/> collection.
    /// </remarks>
    public sealed class AssemblyRefDeclaration : NamedDeclaration, ITypeRefResolutionScope,
                                                 IAssemblyInternal, IManifestResourceImplementation, IWriteILDefinition,
                                                 IWeakReferenceable, IRemoveable
    {
        #region Fields

        /// <summary>
        /// Version of the referenced assembly.
        /// </summary>
        private Version version;

        /// <summary>
        /// Public key of the referenced assembly.
        /// </summary>
        /// <value>
        /// The public key, or <b>null</b> if no public key is specified.
        /// </value>
        private byte[] publicKey;

        /// <summary>
        /// Standard name of the culture of the referenced assembly.
        /// </summary>
        /// <value>
        /// A standard culture name, or <b>null</b> if the referenced assembly
        /// is culturally neutral.
        /// </value>
        private string culture;


        /// <summary>
        /// Hash value of the referenced assembly.
        /// </summary>
        /// <remarks>
        /// The hash value, or <b>null</b> if no hash value is specified.
        /// </remarks>
        private byte[] hashValue;

        /// <summary>
        /// Collection of types defined by the referenced assembly and referenced 
        /// to by the declaring assembly.
        /// </summary>
        private readonly TypeRefDeclarationCollection typeRefs;

        /// <summary>
        /// Alias of the external assembly in MSIL source code.
        /// </summary>
        /// <value>
        /// Any valid identifier, or <b>null</b> if the normal assembly
        /// name should be used.
        /// </value>
        private string alias;

        /// <summary>
        /// Hash algorithm used to sign the assembly.
        /// </summary>
        private AssemblyHashAlgorithm hashAlgorithm = AssemblyHashAlgorithm.MD5;


        /// <summary>
        /// Attributes of the assembly referenced.
        /// </summary>
        private AssemblyRefAttributes attributes;

        /// <summary>
        /// Runtime assembly corresponding to the current assembly reference.
        /// </summary>
        /// <remarks>
        /// An <see cref="Assembly"/>, or <b>null</b> if the assembly
        /// has not yet been resolved.
        /// </remarks>
        private Assembly cachedReflectionAssembly;

        private string overwrittenName;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="AssemblyRefDeclaration"/>.
        /// </summary>
        public AssemblyRefDeclaration()
        {
            this.typeRefs = new TypeRefDeclarationCollection( this, "externalTypes" );
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.AssemblyRef;
        }

        /// <inheritdoc />
        public bool Equals( IAssembly other )
        {
            return CompareHelper.Equals( this, other, this.Domain.AssemblyRedirectionPolicies, true );
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.alias ?? this.Name;
        }

        private AssemblyEnvelope cachedAssemblyEnvelope;

        /// <inheritdoc />
        public AssemblyEnvelope GetAssemblyEnvelope()
        {
            if ( cachedAssemblyEnvelope == null )
            {
                cachedAssemblyEnvelope = this.Domain.GetAssembly( this );
            }

            return cachedAssemblyEnvelope;
        }

        /// <inheritdoc />
        public bool MatchesReference( IAssemblyName assemblyName )
        {
            return CompareHelper.Equals( this, assemblyName, this.Domain.AssemblyRedirectionPolicies, false );
        }

        #region IWeakReferenceable Members

        /// <inheritdoc />
        [ReadOnly( true )]
        public bool IsWeaklyReferenced
        {
            get { return this.InternalIsWeaklyReferenced; }
            set { this.InternalIsWeaklyReferenced = value; }
        }

        #endregion

        #region Binding

        /// <inheritdoc />
        public ITypeSignature FindType( string name, BindingOptions bindingOptions )
        {
            return this.typeRefs.FindType( name, bindingOptions );
        }


        /// <inheritdoc />
        public Assembly GetSystemAssembly()
        {
            if ( this.cachedReflectionAssembly == null )
            {
                this.cachedReflectionAssembly =
                    AssemblyLoadHelper.LoadAssemblyFromName(
                        this.Domain.AssemblyRedirectionPolicies.GetCanonicalAssemblyName( this.FullName ) );
            }

            return this.cachedReflectionAssembly;
        }

        /// <inheritdoc />
        public IAssemblyWrapper GetReflectionWrapper()
        {
            return new AssemblyWrapper( this );
        }

        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetReflectionWrapper();
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetSystemAssembly();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the assembly reference attributes.
        /// </summary>
        /// <value>
        /// A combination of <see cref="AssemblyRefAttributes"/>.
        /// </value>
        [ReadOnly( true )]
        public AssemblyRefAttributes Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }

        /// <inheritdoc />
        [ReadOnly( true )]
        public Version Version
        {
            get { return this.version; }
            set { this.version = value; }
        }

        /// <summary>
        /// Gets the public key.
        /// </summary>
        /// <remarks>
        /// The <see cref="AssemblyRefAttributes.PublicKey"/> flag should 
        /// be cleared on the <see cref="Attributes"/> property.
        /// </remarks>
        [ReadOnly( true )]
        public byte[] GetPublicKey()
        {
            if ( ( this.attributes & AssemblyRefAttributes.PublicKey ) != 0 )
            {
                return this.publicKey;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the public key.
        /// </summary>
        /// <param name="value">The public key, or <b>null</b> to remove
        /// the public key.</param>
        /// <remarks>
        /// The <see cref="AssemblyRefAttributes.PublicKey"/> flag should 
        /// be set on the <see cref="Attributes"/> property.
        /// </remarks>
        public void SetPublicKey( byte[] value )
        {
            if ( ( this.attributes & AssemblyRefAttributes.PublicKey ) != 0 )
            {
                this.publicKey = value;
            }
            else
            {
                throw ExceptionHelper.Core.CreateInvalidOperationException(
                    "PublicKeyNeedsAttribute" );
            }
        }

        /// <summary>
        /// Gets the public key token.
        /// </summary>
        /// <remarks>
        /// The <see cref="AssemblyRefAttributes.PublicKey"/> flag should 
        /// be cleared on the <see cref="Attributes"/> property.
        /// </remarks>
        public byte[] GetPublicKeyToken()
        {
            if ( this.publicKey == null )
            {
                return null;
            }
            else
            {
                if ( ( this.attributes & AssemblyRefAttributes.PublicKey ) != 0 )
                {
                    return AssemblyNameHelper.ComputeKeyToken( this.publicKey );
                }
                else
                {
                    return this.publicKey;
                }
            }
        }

        /// <summary>
        /// Sets the public key token.
        /// </summary>
        /// <param name="value">The public key token, or <b>null</b>
        /// to remote the public key token.</param>
        /// <remarks>
        /// The <see cref="AssemblyRefAttributes.PublicKey"/> flag should 
        /// be cleared on the <see cref="Attributes"/> property.
        /// </remarks>
        public void SetPublicKeyToken( byte[] value )
        {
            if ( ( this.attributes & AssemblyRefAttributes.PublicKey ) == 0 )
            {
                this.publicKey = value;
            }
            else
            {
                throw ExceptionHelper.Core.CreateInvalidOperationException(
                    "PublicKeyNeedsAttribute" );
            }
        }

        /// <inheritdoc />
        [ReadOnly( true )]
        public string Culture
        {
            get { return this.culture; }
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
        [SuppressMessage( "Microsoft.Performance", "CA1819",
            Justification = "Returning byte[] is correct in the current case." )]
        public byte[] HashValue
        {
            get { return this.hashValue; }
            set { this.hashValue = value; }
        }

        /// <summary>
        /// Gets or sets the alias of the assembly reference in MSIL source code.
        /// </summary>
        /// <value>
        /// A string if the alias should be used to reference the current assembly in CIL code,
        /// or <b>null</b> if the assembly name should be used instead.
        /// </value>
        /// <remarks>
        /// Although the current is not enforced programatically, all module references and assembly
        /// references should have a distinct name or alias.
        /// </remarks>
        internal string Alias
        {
            //get { return this.alias; }
            set { this.alias = value; }
        }

        /// <inheritdoc />
        [ReadOnly( true )]
        public AssemblyHashAlgorithm HashAlgorithm
        {
            get { return hashAlgorithm; }
            set { hashAlgorithm = value; }
        }

        /// <inheritdoc />
        public bool IsMscorlib
        {
            get { return this.Name == "mscorlib"; }
        }

        /// <inheritdoc />
        public string FullName
        {
            get
            {
                return AssemblyNameHelper.FormatAssemblyFullName(
                    this.Name, this.version, this.culture, this.GetPublicKeyToken() );
            }
        }

        /// <inheritdoc />
        [Browsable( false )]
        public TypeRefDeclarationCollection TypeRefs
        {
            get { return this.typeRefs; }
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
        [Browsable( false )]
        public string OverwrittenName
        {
            get { return this.overwrittenName; }
            set { this.overwrittenName = value; }
        }

        #endregion

        #region writer IL

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );

            #endregion

            writer.WriteKeyword( ".assembly" );
            writer.WriteKeyword( "extern" );

            if ( ( this.attributes & AssemblyRefAttributes.Retargetable ) != 0 )
            {
                writer.WriteKeyword( "retargetable" );
            }

            if ( string.IsNullOrEmpty( this.overwrittenName ) )
            {
                writer.WriteDottedName( this.Name );
            }
            else
            {
                writer.WriteDottedName( this.overwrittenName );
            }

            if ( !string.IsNullOrEmpty( this.alias ) )
            {
                writer.WriteKeyword( "as" );
                writer.WriteDottedName( this.alias );
            }
            else if ( !string.IsNullOrEmpty( this.overwrittenName ) )
            {
                writer.WriteKeyword( "as" );
                writer.WriteDottedName( this.Name );
            }

            #region Assembly declarations

            writer.WriteLineBreak();
            writer.BeginBlock();

            /*	writer.WriteKeyword(".hash algorithm");
            writer.WriteInt32(this.hashAlgorithm);
            writer.LineBreak();*/

            if ( string.IsNullOrEmpty( this.overwrittenName ) )
            {
                if ( this.publicKey != null )
                {
                    if ( ( this.attributes & AssemblyRefAttributes.PublicKey ) == 0 )
                    {
                        writer.WriteKeyword( ".publickeytoken" );
                    }
                    else
                    {
                        writer.WriteKeyword( ".publickey" );
                    }
                    writer.WriteSymbol( '=' );
                    writer.WriteBytes( this.publicKey );
                    writer.WriteLineBreak();
                }

                if ( this.hashValue != null )
                {
                    writer.WriteKeyword( ".hash" );
                    writer.WriteSymbol( '=' );
                    writer.WriteBytes( this.hashValue );
                    writer.WriteLineBreak();
                }
            }

            if ( this.version != null )
            {
                writer.WriteKeyword( ".ver" );
                writer.WriteInteger( this.version.Major, IntegerFormat.Decimal );
                writer.WriteSymbol( ':' );
                writer.WriteInteger( this.version.Minor, IntegerFormat.Decimal );
                writer.WriteSymbol( ':' );
                writer.WriteInteger( this.version.Build, IntegerFormat.Decimal );
                writer.WriteSymbol( ':' );
                writer.WriteInteger( this.version.Revision, IntegerFormat.Decimal );
                writer.WriteLineBreak();
            }

            if ( !string.IsNullOrEmpty( this.culture ) )
            {
                writer.WriteKeyword( ".locale" );
                writer.WriteQuotedString( this.culture, WriteStringOptions.DoubleQuoted );
                writer.WriteLineBreak();
            }

            this.CustomAttributes.WriteILDefinition( writer );

            // Todo: secDecl

            writer.EndBlock();

            #endregion
        }

        /// <inheritdoc />
        void IAssemblyInternal.WriteILReference( ILWriter writer )
        {
            writer.WriteSpace();
            writer.WriteSymbol( '[' );
            if ( !string.IsNullOrEmpty( alias ) )
            {
                writer.WriteDottedName( this.alias );
            }
            else
            {
                writer.WriteDottedName( this.Name );
            }
            writer.WriteSymbol( ']', SymbolSpacingKind.None, SymbolSpacingKind.None );
        }

        bool IAssemblyInternal.Equals( IAssembly assembly, bool strict )
        {
            return CompareHelper.Equals( this, assembly, this.Domain.AssemblyRedirectionPolicies, strict );
        }

        #endregion

        #region IDisposable Members

        /// <inheritdoc />
        internal override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            if ( disposing )
            {
                this.typeRefs.Dispose();
            }
        }

        #endregion

        #region IRemoveable Members

        /// <inheritdoc />
        public void Remove()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.Parent != null, "CannotRemoveBecauseNoParent" );

            #endregion

            this.Module.AssemblyRefs.Remove( this );
        }

        #endregion

        /// <inheritdoc />
        public override void ClearCache()
        {
            base.ClearCache();
            this.cachedAssemblyEnvelope = null;
            this.cachedReflectionAssembly = null;
            this.typeRefs.ClearCache();
        }
    }


    /// <summary>
    /// Options of assembly references (<see cref="AssemblyRefDeclaration"/>).
    /// </summary>
    [Flags]
    public enum AssemblyRefAttributes
    {
        /// <summary>
        /// Default.
        /// </summary>
        None = 0,

        /// <summary>
        /// The assembly reference holds the full (unhashed) public key.
        /// </summary>
        PublicKey = 0x0001,

        /// <summary>
        /// The implementation of the current assembly used at runtime is
        /// not expected to match the version seen at compile time.
        /// </summary>
        [SuppressMessage( "Microsoft.Naming", "CA1704", Justification = "Spelling is correct." )] Retargetable = 0x0100,

        /// <summary>
        /// Reserved.
        /// </summary>
        EnableJitCompileTracking = 0x8000,

        /// <summary>
        /// Reserved.
        /// </summary>
        DisableJitCompileOptimizer = 0x4000
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of <see cref="AssemblyRefDeclaration"/>.
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class AssemblyRefDeclarationCollection :
            OrderedEmitDeclarationCollection<AssemblyRefDeclaration>
        {
            internal AssemblyRefDeclarationCollection( Declaration parent, string role )
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
                get { return true; }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
                ( (ModuleDeclaration) this.Owner ).ModuleReader.ImportAssemblyRefs();
            }

        }
    }
}