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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Reflection;
using PostSharp.CodeModel.Binding;
using PostSharp.CodeModel.Collections;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.ModuleReader;
using PostSharp.ModuleWriter;
using PostSharp.Utilities;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a module (<see cref="TokenType.Module"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    ///  Since PostSharp is module-centric, a module contains all other declarations.
    /// </para>
    /// </remarks>
    [SuppressMessage( "Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable" )]
    public sealed class ModuleDeclaration : NamedDeclaration, IModuleInternal,
                                            ITypeRefResolutionScope, IWriteILDefinition, ITypeContainer
    {
        #region Fields

        /// <summary>
        /// Collection of type type specifications.
        /// </summary>
        private readonly TypeSpecDeclarationCollection typeSpecifications;

        private readonly TypeDefDeclarationCollection types;

        /// <summary>
        /// Collection of type references.
        /// </summary>
        private readonly TypeRefDeclarationCollection typeRefs;

        /// <summary>
        /// Collection of module references.
        /// </summary>
        private readonly ModuleRefDeclarationCollection moduleRefs;

        /// <summary>
        /// Collection of assembly references.
        /// </summary>
        private readonly AssemblyRefDeclarationCollection assemblyRefs;

        /// <summary>
        /// Collection of binary data sections.
        /// </summary>
        private readonly DataDeclarationCollection data;

        /// <summary>
        /// Manages metadata tables.
        /// </summary>
        private MetadataDeclarationTables tables;

        /// <summary>
        /// Attributes of the PE image.
        /// </summary>
        private ImageAttributes imageAttributes;

        /// <summary>
        /// Subsystem.
        /// </summary>
        private int subsystem;

        /// <summary>
        /// Assembly manifest.
        /// </summary>
        /// <value>
        /// An <see cref="AssemblyManifestDeclaration"/>, or <b>null</b> if the module
        /// does not contain an assembly manifest.
        /// </value>
        private AssemblyManifestDeclaration assemblyManifest;

        /// <summary>
        /// ImageBase value in the NT Optional header.
        /// </summary>
        private ulong imageBase;

        /// <summary>
        /// FileAlignment value in the NT Optional header.
        /// </summary>
        private uint fileAlignment;

        /// <summary>
        /// StackReserve value in the NT Optional header.
        /// </summary>
        private ulong stackReserve;

        /// <summary>
        /// Module filename.
        /// </summary>
        private string fileName;

        /// <summary>
        /// Runtime <see cref="Module"/> corresponding to the current instance.
        /// </summary>
        private Module runtimeModule;

        /// <summary>
        /// Helps deserializing custom attributes and permission sets defined
        /// in the current assembly.
        /// </summary>
        private readonly DeserializationUtil deserializationUtil;

        /// <summary>
        /// Symbol reader for the current module.
        /// </summary>
        private ISymbolReader symbolReader;

        /// <summary>
        /// Maps full assembly names to <see cref="IAssembly"/> in the current module.
        /// </summary>
        private readonly Dictionary<string, IAssembly> assemblyCache = new Dictionary<string, IAssembly>();


        private readonly UnmanagedResourceCollection unmanagedResources = new UnmanagedResourceCollection();

        private readonly DeclarationCache cache;

        private readonly Dictionary<Guid, MetadataDeclarationDirectory<object>> tagDictionaries =
            new Dictionary<Guid, MetadataDeclarationDirectory<object>>();

        private ModuleReader.ModuleReader moduleReader;

        private IAssembly mscorlib;

        private string frameworkVariant;

        private StandaloneSignatureDeclarationCollection standaloneSignatures;


        #endregion

        /// <summary>
        /// Initializes a new <see cref="ModuleDeclaration"/>.
        /// </summary>
        public ModuleDeclaration()
        {
            this.types = new TypeDefDeclarationCollection( this, "types" );
            this.typeSpecifications = new TypeSpecDeclarationCollection( this, "typeSpecifications" );
            this.typeRefs = new TypeRefDeclarationCollection( this, "externalTypes" );
            this.moduleRefs = new ModuleRefDeclarationCollection( this, "moduleRefs" );
            this.assemblyRefs = new AssemblyRefDeclarationCollection( this, "assemblyRefs" );
            this.data = new DataDeclarationCollection( this, "data" );
            this.standaloneSignatures = new StandaloneSignatureDeclarationCollection(this, "standaloneSignatures");
            this.deserializationUtil = new DeserializationUtil( this );
            this.cache = new DeclarationCache( this );
            this.Module = this;
        }

        /// <inheritdoc />
        protected override bool NotifyChildPropertyChanged( Element child, string property, object oldValue,
                                                            object newValue )
        {
            if ( base.NotifyChildPropertyChanged( child, property, oldValue, newValue ) )
                return true;

            switch ( child.Role )
            {
                case "types":
                    if ( property == "Name" )
                    {
                        this.types.OnItemNameChanged( (TypeDefDeclaration) child, (string) oldValue );
                        return true;
                    }
                    break;

                case "typeRefs":
                    if ( property == "Name" )
                    {
                        this.typeRefs.OnItemNameChanged( (TypeRefDeclaration) child, (string) oldValue );
                        return true;
                    }
                    break;

                case "moduleRefs":
                    if ( property == "Name" )
                    {
                        this.moduleRefs.OnItemNameChanged( (ModuleRefDeclaration) child, (string) oldValue );
                        return true;
                    }
                    break;

                case "data":
                    if ( property == "Ordinal" )
                    {
                        this.data.OnItemOrdinalChanged( (DataSectionDeclaration) child, (int) oldValue );
                        return true;
                    }
                    break;
            }

            return false;
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.Module;
        }

        /// <summary>
        /// Gets the <see cref="DeserializationUtil"/> that can be used
        /// to deserialize serialized values in the current module.
        /// </summary>
        internal DeserializationUtil DeserializationUtil
        {
            get { return this.deserializationUtil; }
        }

        /// <summary>
        /// Caches a set of frequently used declarations.
        /// </summary>
        [Browsable( false )]
        public DeclarationCache Cache
        {
            get { return this.cache; }
        }

        /// <summary>
        /// Gets an enumeration of declarations of a given type.
        /// </summary>
        /// <param name="tokenType">Types of declaration to be retrieved.</param>
        /// <returns>An enumerator of all declarations of type <see cref="TokenType"/> present
        /// in the current module.</returns>
        public IEnumerator<MetadataDeclaration> GetDeclarationEnumerator( TokenType tokenType )
        {
            return this.tables.GetEnumerator( tokenType );
        }

        #region Properties

        internal ModuleReader.ModuleReader ModuleReader
        {
            get
            {
                this.AssertNotDisposed();
                return this.moduleReader;
            }
            set
            {
                this.AssertNotDisposed();
                this.moduleReader = value;
            }
        }

        /// <summary>
        /// Gets the metadata tables containing all declarations of the current module.
        /// </summary>
        internal MetadataDeclarationTables Tables
        {
            get { return this.tables; }
            set { this.tables = value; }
        }

        /// <summary>
        /// Gets or sets the kind of PE file (library, console executable, graphic executable).
        /// </summary>
        [ReadOnly( true )]
        public int Subsystem
        {
            get { return subsystem; }
            set { subsystem = value; }
        }

        /// <summary>
        /// Gets the attributes of the PE image.
        /// </summary>
        [ReadOnly( true )]
        public ImageAttributes ImageAttributes
        {
            get { return imageAttributes; }
            set { imageAttributes = value; }
        }

        /// <summary>
        /// Gets the name of the file containing the module.
        /// </summary>
        [ReadOnly( true )]
        public string FileName
        {
            get { return fileName; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                fileName = value;

                this.IsMscorlib = Path.GetFileName( value ).ToLowerInvariant() == "mscorlib.dll";
            }
        }


        /// <summary>
        /// Gets or sets the ImageBase value in the NT Optional header.
        /// </summary>
        [ReadOnly( true )]
        public ulong ImageBase
        {
            get { return imageBase; }
            set { imageBase = value; }
        }


        /// <summary>
        /// Gets or sets the FileAlignment value in the NT Optional header.
        /// </summary>
        [ReadOnly( true )]
        public uint FileAlignment
        {
            get { return fileAlignment; }
            set { fileAlignment = value; }
        }


        /// <summary>
        /// Gets or sets the SizeOfStackReserve value in the NT Optional header.
        /// </summary>
        [ReadOnly( true )]
        public ulong StackReserve
        {
            get { return stackReserve; }
            set { stackReserve = value; }
        }

        /// <summary>
        /// Gets or sets the VersionString field of the metadata header. 
        /// </summary>
        [ReadOnly( true )]
        public string MetadataVersionString { get; set; }

        /// <summary>
        /// Gets or sets the MinorVersion field of the metadata header. 
        /// </summary>
        [ReadOnly( true )]
        public int MetadataMinorVersion { get; set; }

        /// <summary>
        /// Gets or sets the MajorVersion field of the metadata header. 
        /// </summary>
        [ReadOnly( true )]
        public int MetadataMajorVersion { get; set; }

        /// <summary>
        /// Gets or sets the RuntimeMajorVersion field of the CLI header.
        /// </summary>
        [ReadOnly( true )]
        public int RuntimeMajorVersion { get; set; }

        /// <summary>
        /// Gets or sets the RuntimeMinorVersion field of the CLI header.
        /// </summary>
        [ReadOnly( true )]
        public int RuntimeMinorVersion { get; set; }


        /// <summary>
        /// Gets the collection of type specifications (<see cref="TypeSpecDeclaration"/>).
        /// </summary>
        [Browsable( false )]
        public TypeSpecDeclarationCollection TypeSpecs
        {
            get
            {
                this.AssertNotDisposed();
                return this.typeSpecifications;
            }
        }

        /// <summary>
        /// Gets or sets the entry point of the current module.
        /// </summary>
        /// <value>
        /// A reference to a <see cref="MethodDefDeclaration"/>, or <b>null</b>
        /// if the module has no entry point.
        /// </value>
        [ReadOnly( true )]
        public MethodDefDeclaration EntryPoint { get; set; }

        /// <summary>
        /// Gets the collection of types (<see cref="TypeDefDeclaration"/>).
        /// </summary>
        /// <remarks>
        /// Note that this collection does not contain <i>nested</i> types.
        /// Nested types are nested in their parent type.
        /// </remarks>
        [Browsable( false )]
        public TypeDefDeclarationCollection Types
        {
            get
            {
                this.AssertNotDisposed();
                return this.types;
            }
        }

        /// <summary>
        /// Gets the collection of type references (<see cref="TypeRefDeclaration"/>)
        /// whose resolution scope is the current module.
        /// </summary>
        /// <remarks>
        /// A correct CLI module will <i>not</i> define type references whose
        /// resolution scope is a module. It will use directly a
        /// <see cref="TokenType.TypeDef"/> token instead of a
        /// <see cref="TokenType.TypeRef"/> token.
        /// </remarks>
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
        /// Gets the collection of module references (<see cref="ModuleRefDeclaration"/>).
        /// </summary>
        [Browsable( false )]
        public ModuleRefDeclarationCollection ModuleRefs
        {
            get
            {
                this.AssertNotDisposed();
                return this.moduleRefs;
            }
        }

        /// <summary>
        /// Gets the collection of assembly references (<see cref="AssemblyRefDeclaration"/>).
        /// </summary>
        [Browsable( false )]
        public AssemblyRefDeclarationCollection AssemblyRefs
        {
            get
            {
                this.AssertNotDisposed();
                return this.assemblyRefs;
            }
        }

        /// <summary>
        /// Gets the collection of raw data declarations (<see cref="DataSectionDeclaration"/>).
        /// </summary>
        [Browsable( false )]
        public DataDeclarationCollection Datas
        {
            get
            {
                this.AssertNotDisposed();
                return this.data;
            }
        }

        /// <summary>
        /// Gets the collection of standalone signatures (<see cref="StandaloneSignatureDeclarationCollection"/>).
        /// </summary>
        [Browsable(false)]
        public StandaloneSignatureDeclarationCollection StandaloneSignatures
        {
            get
            {
                this.AssertNotDisposed();
                return this.standaloneSignatures;
            }
        }

        /// <inheritdoc />
        public AssemblyEnvelope Assembly
        {
            get { return (AssemblyEnvelope) this.Parent; }
        }

        /// <inheritdoc />
        IAssembly IModule.Assembly
        {
            get { return this.Assembly; }
        }

        /// <summary>
        /// Gets or sets the assembly manifest contained in the current module.
        /// </summary>
        /// <remarks>
        /// An <see cref="AssemblyManifestDeclaration"/>, or <b>null</b> if this
        /// module does not contain any assembly manifest.
        /// </remarks>
        [Browsable( false )]
        public AssemblyManifestDeclaration AssemblyManifest
        {
            get
            {
                this.AssertNotDisposed();
                return this.assemblyManifest;
            }
            set
            {
                this.AssertNotDisposed();

                if ( this.assemblyManifest != null )
                {
                    this.assemblyManifest.OnRemovingFromParent();
                }
                this.assemblyManifest = value;

                if ( value != null )
                {
                    this.assemblyManifest.OnAddingToParent( this, "assemblyManifest" );
                }

                this.Assembly.ManifestModule = this;
            }
        }

        /// <summary>
        /// Gets or sets the GUID of the current module.
        /// </summary>
        [ReadOnly( true )]
        public Guid ModuleGuid { get; set; }

        /// <summary>
        /// Determines whether the current module is <b>mscorlib</b>.
        /// </summary>
        public bool IsMscorlib { get; private set; }


        /// <summary>
        /// Gets the collection of unmanaged resources.
        /// </summary>
        [Browsable( false )]
        public UnmanagedResourceCollection UnmanagedResources
        {
            get { return this.unmanagedResources; }
        }

        #endregion

        #region Symbols

        /// <summary>
        /// Gets or sets the <see cref="ISymbolReader"/> for the current module.
        /// </summary>
        internal ISymbolReader SymbolReader
        {
            get { return this.symbolReader; }
            set { this.symbolReader = value; }
        }

        /// <summary>
        /// Determines whether the current module has debugging information.
        /// </summary>
        public bool HasDebugInfo
        {
            get { return this.symbolReader != null; }
        }

        #endregion

        #region writer IL

        /// <inheritdoc />
        void IModuleInternal.WriteILReference( ILWriter writer )
        {
        }

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            if ( this.IsMscorlib &&
                 ( writer.Options.Compatibility & ILWriterCompatibility.IgnoreMscorlibHeader ) == 0 )
            {
                writer.WriteKeyword( ".mscorlib" );
                writer.WriteLineBreak();
            }

            if ( ( writer.Options.Compatibility & ILWriterCompatibility.EmitTypeList ) != 0 )
            {
                // writer forward definition
                writer.WriteKeyword( ".typelist" );
                writer.WriteLineBreak();
                writer.BeginBlock();
                IEnumerator<MetadataDeclaration> typeEnumerator = this.tables.GetEnumerator( TokenType.TypeDef );
                while ( typeEnumerator.MoveNext() )
                {
                    TypeDefDeclaration typeDef = (TypeDefDeclaration) typeEnumerator.Current;
                    if ( !typeDef.BelongsToClassification( TypeClassifications.Module ) )
                    {
                        ( (ITypeSignatureInternal) typeDef ).WriteILReference( writer, GenericMap.Empty,
                                                                               WriteTypeReferenceOptions.None );
                        writer.WriteLineBreak();
                    }
                }
                writer.EndBlock();
            }

            foreach ( ModuleRefDeclaration externalModule in this.ModuleRefs )
            {
                externalModule.WriteILDefinition( writer );
            }

            foreach ( AssemblyRefDeclaration externalAssembly in this.AssemblyRefs )
            {
                if ( !externalAssembly.IsWeaklyReferenced )
                {
                    externalAssembly.WriteILDefinition( writer );
                }
            }

            if ( this.AssemblyManifest != null )
            {
                this.AssemblyManifest.WriteILDefinition( writer );
            }

            writer.WriteKeyword( ".module" );
            writer.WriteDottedName( this.Name );
            writer.WriteLineBreak();

            this.CustomAttributes.WriteILDefinition( writer );


            writer.WriteKeyword( ".imagebase" );
            writer.WriteInteger( this.imageBase );
            writer.WriteLineBreak();
            writer.WriteKeyword( ".file alignment" );
            writer.WriteInteger( this.fileAlignment );
            writer.WriteLineBreak();
            writer.WriteKeyword( ".stackreserve" );
            writer.WriteInteger( this.stackReserve );
            writer.WriteLineBreak();
            writer.WriteKeyword( ".subsystem" );
            writer.WriteInteger( (ushort) this.subsystem );
            writer.WriteLineBreak();
            writer.WriteKeyword( ".corflags" );
            writer.WriteInteger( (uint) this.imageAttributes );
            writer.WriteLineBreak();

            if ( ( writer.Options.Compatibility & ILWriterCompatibility.EmitForwardDeclarations ) != 0 )
            {
                writer.Options.InForwardDeclaration = true;
                foreach ( TypeDefDeclaration type in this.Types )
                {
                    if ( type.Name != "<Module>" )
                    {
                        type.WriteILDefinition( writer );
                    }
                }
                writer.Options.InForwardDeclaration = false;
            }

            // writer module fields and methods
            TypeDefDeclaration moduleType = this.types.GetByName( "<Module>" );
            if ( moduleType != null )
            {
                foreach ( FieldDefDeclaration field in moduleType.Fields )
                {
                    field.WriteILDefinition( writer );
                }

                foreach ( MethodDefDeclaration method in moduleType.Methods )
                {
                    method.WriteILDefinition( writer );
                }
            }

            foreach ( TypeDefDeclaration type in this.Types )
            {
                if ( type.Name != "<Module>" )
                {
                    type.WriteILDefinition( writer );
                }
            }


            foreach ( DataSectionDeclaration dataDeclaration in this.data )
            {
                dataDeclaration.WriteILDefinition( writer );
            }
        }

        #endregion

        #region Binding

        /// <summary>
        /// Gets the runtime module corresponding to the current instance.
        /// </summary>
        /// <returns>A <see cref="Module"/>, or <b>null</b> if the current instance does not correspond
        /// to any runtime module.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024",
            Justification = "Keep the same style as GetSystemType and so on." )]
        public Module GetSystemModule()
        {
            return this.runtimeModule;
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetSystemModule();
        }

        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the runtime module.
        /// </summary>
        /// <param name="module">A <see cref="Module"/>.</param>
        internal void SetRuntimeModule( Module module )
        {
            this.runtimeModule = module;
        }


        /// <summary>
        /// Finds the <b>mscorlib</b> assembly referred to by the current module.
        /// </summary>
        /// <returns>An <see cref="AssemblyRefDeclaration"/>, or the current <see cref="AssemblyEnvelope"/>
        /// if the current module is <b>mscorlib</b>.
        /// </returns>
        public IAssembly FindMscorlib()
        {
            if ( this.mscorlib != null )
            {
                return this.mscorlib;
            }
            else if ( this.IsMscorlib )
            {
                this.mscorlib = this.Assembly;
            }
            else
            {
                foreach ( AssemblyRefDeclaration assemblyRef in this.AssemblyRefs )
                {
                    if ( assemblyRef.IsMscorlib )
                    {
                        this.mscorlib = assemblyRef;
                        break;
                    }
                }
            }

            return this.mscorlib;
        }

        /// <overloads>Finds an assembly in the current module.</overloads>
        /// <summary>
        /// Finds in the current module an assembly corresponding to a given runtime assembly.
        /// </summary>
        /// <param name="reflectionAssembly">A runtime <see cref="Assembly"/>.</param>
        /// <param name="bindingOptions">Determines the behavior of the binding
        /// in case that <paramref name="reflectionAssembly"/> could not be found in the current module.</param>
        /// <returns>The <see cref="IAssembly"/> corresponding to <paramref name="reflectionAssembly"/>,
        /// or <b>null</b> if no assembly was found.</returns>
        public IAssembly FindAssembly( Assembly reflectionAssembly, BindingOptions bindingOptions )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( reflectionAssembly, "reflectionAssembly" );

            #endregion

            return this.FindAssembly( AssemblyNameWrapper.GetWrapper( reflectionAssembly ), bindingOptions );
        }


        /// <summary>
        /// Finds in the current module an assembly corresponding to a given <see cref="AssemblyName"/>.
        /// </summary>
        /// <param name="assemblyName">An <see cref="AssemblyName"/>.</param>
        /// <param name="bindingOptions">Determines the behavior of the binding
        /// in case that <paramref name="assemblyName"/> could not be found in the current module.</param>
        /// <returns>The <see cref="IAssembly"/> corresponding to <paramref name="assemblyName"/>,
        /// or <b>null</b> if no assembly was found.</returns>
        public IAssembly FindAssembly( IAssemblyName assemblyName, BindingOptions bindingOptions )
        {
            string assemblyNameString = assemblyName.FullName;

            #region Preconditions

            this.AssertNotDisposed();
            ExceptionHelper.AssertArgumentNotNull( assemblyName, "assemblyName" );

            #endregion

            // Look in cache
            IAssembly assembly;
            if ( !assemblyCache.TryGetValue( assemblyNameString, out assembly ) )
            {
                if ( this.Assembly.MatchesReference( assemblyName ) )
                {
                    Trace.ReflectionBinding.WriteLine(
                        "ModuleDeclaration.FindAssembly( {{{0}}} ): this is the current assembly.",
                        assemblyName );

                    assemblyCache.Add( assemblyNameString, this.Assembly );
                    return this.Assembly;
                }

                string sameNameFound = null;
                foreach ( AssemblyRefDeclaration assemblyRef in this.assemblyRefs )
                {
                    if (
                        string.Compare( assemblyName.Name, assemblyRef.Name,
                                        StringComparison.InvariantCultureIgnoreCase ) == 0 )
                    {
                        sameNameFound = assemblyRef.FullName;
                    }

                    if ( assemblyRef.MatchesReference( assemblyName ) )
                    {
                        Trace.ReflectionBinding.WriteLine(
                            "ModuleDeclaration.FindAssembly( {{{0}}} ): found as AssemblyRef.",
                            assemblyName );

                        // Link the assembly to the module if necessary.
                        if ( assemblyRef.IsWeaklyReferenced &&
                             ( bindingOptions & BindingOptions.ExistenceMask ) == BindingOptions.Default )
                        {
                            Trace.ReflectionBinding.WriteLine(
                                "ModuleDeclaration.FindAssembly( {{{0}}} ): linking the AssemblyRef to the module.",
                                assemblyName );

                            assemblyRef.IsWeaklyReferenced = false;
                        }

                        assemblyCache.Add( assemblyNameString, assemblyRef );
                        return assemblyRef;
                    }
                }

                // Assembly not found.
                if ( ( bindingOptions & BindingOptions.ExistenceMask ) != BindingOptions.OnlyExisting )
                {
                    Trace.ReflectionBinding.WriteLine(
                        "ModuleDeclaration.FindAssembly( {{{0}}} ): not found. Adding an AssemblyRef to the module.",
                        assemblyName );

                    AssemblyRefDeclaration externalAssembly = new AssemblyRefDeclaration
                                                                  {
                                                                      Name = assemblyName.Name,
                                                                      Version = assemblyName.Version,
                                                                      Culture = assemblyName.Culture,
                                                                      IsWeaklyReferenced =
                                                                          ( ( bindingOptions &
                                                                              BindingOptions.ExistenceMask ) ==
                                                                            BindingOptions.WeakReference )
                                                                  };
                    externalAssembly.SetPublicKeyToken( assemblyName.GetPublicKeyToken() );


                    this.assemblyRefs.Add( externalAssembly );

                    assemblyCache.Add( assemblyNameString, externalAssembly );
                    return externalAssembly;
                }
                else
                {
                    if ( ( bindingOptions & BindingOptions.DontThrowException ) != 0 )
                    {
                        return null;
                    }
                    else
                    {
                        if ( sameNameFound != null )
                        {
                            throw ExceptionHelper.Core.CreateBindingException( "CannotFindAssemblyRefSameNameFound",
                                                                               assemblyName.FullName, this.Name,
                                                                               sameNameFound );
                        }
                        else
                        {
                            throw ExceptionHelper.Core.CreateBindingException( "CannotFindAssemblyRef",
                                                                               assemblyName.FullName, this.Name );
                        }
                    }
                }
            }
            else
            {
                if ( ( bindingOptions & BindingOptions.ExistenceMask ) == BindingOptions.Default )
                {
                    AssemblyRefDeclaration assemblyRef = assembly as AssemblyRefDeclaration;
                    if ( assemblyRef != null && assemblyRef.IsWeaklyReferenced )
                    {
                        assemblyRef.IsWeaklyReferenced = false;
                    }
                }
                return assembly;
            }
        }

        /// <overloads>Finds a type in the current module.</overloads>
        /// <summary>
        /// Finds in the current module a type given its full name.
        /// </summary>
        /// <param name="fullTypeName">The full type name, including the assembly name.</param>
        /// <param name="bindingOptions">Determines the behavior of the binder in case
        /// that the assembly and/or type could not be found in the current module.</param>
        /// <returns>The requested type.</returns>
        /// <exception cref="BindingException">The type could not be found.</exception>
        public ITypeSignature FindType( string fullTypeName, BindingOptions bindingOptions )
        {
            #region Preconditions

            if ( string.IsNullOrEmpty( fullTypeName ) )
            {
                throw new ArgumentNullException( "fullTypeName" );
            }

            #endregion

            // If the type name is a type construction, we have to use Reflection
            // because we have no parsing algorithm for this.
            if ( fullTypeName.Contains( "[" ) )
            {
                Type reflectionType;
                try
                {
                    string assemblyPart = fullTypeName.Substring( fullTypeName.LastIndexOf( ']' ) + 1 );
                    if ( assemblyPart.Contains( "," ) )
                    {
                        // The type is in another assembly.
                        reflectionType = Type.GetType( fullTypeName, true );
                    }
                    else
                    {
                        // The type is in the current assembly.
                        if ( this.GetSystemModule() == null )
                            throw new NotImplementedException(
                                "Cannot find non-trivial types when the module is not bound to reflection." );

                        reflectionType = this.GetSystemModule().Assembly.GetType( fullTypeName, true );
                    }
                }
                catch ( Exception e )
                {
                    if ( ( bindingOptions & BindingOptions.DontThrowException ) != 0 )
                    {
                        return null;
                    }
                    else
                    {
                        throw ExceptionHelper.Core.CreateBindingException(
                            "CannotFindTypeSeeInnerException", e, fullTypeName );
                    }
                }
                return this.FindType( reflectionType, bindingOptions );
            }

            int comma = fullTypeName.IndexOf( ',' );

            if ( comma < 0 )
            {
                // The type is defined in the current assembly.

                // Split textWriter types.
                string[] innerTypes = fullTypeName.Split( '+' );
                TypeDefDeclaration type = this.Types.GetByName( innerTypes[0] );
                if ( type == null )
                {
                    if ( ( bindingOptions & BindingOptions.OnlyDefinition ) == 0 )
                    {
                        // If the type was not in the current module, it could be in a ModuleRef.
                        // However, we should look for the FULL type, in this case 
                        // (not the nested type).
                        foreach ( ModuleRefDeclaration moduleRef in this.moduleRefs )
                        {
                            TypeRefDeclaration typeRef = moduleRef.TypeRefs.GetByName( fullTypeName );

                            if ( typeRef != null )
                            {
                                return typeRef;
                            }
                        }

                        // If the type was not found at this point, we may need to create
                        // a ModuleRef->TypeRef.
                        foreach ( ModuleRefDeclaration moduleRef in this.moduleRefs )
                        {
                            ModuleDeclaration module = moduleRef.GetModuleDefinition();
                            if ( module != null )
                            {
                                // Process only managed modules.

                                TypeDefDeclaration typeDef = (TypeDefDeclaration) module.FindType( fullTypeName,
                                                                                                   BindingOptions.
                                                                                                       DontThrowException |
                                                                                                   BindingOptions.
                                                                                                       OnlyExisting |
                                                                                                   BindingOptions.
                                                                                                       RequireGenericDefinition );

                                if ( typeDef != null )
                                {
                                    return typeDef.Translate( this );
                                }
                            }
                        }
                    }


                    // The type was not found.
                    if ( ( bindingOptions & BindingOptions.DontThrowException ) != 0 )
                    {
                        return null;
                    }
                    else
                    {
                        throw ExceptionHelper.Core.CreateBindingException( "CannotFindTypeInCurrentModule",
                                                                           fullTypeName, this.Name );
                    }
                }

                // Get nested types from the outest to the innest
                for ( int i = 1; i < innerTypes.Length; i++ )
                {
                    type = type.Types.GetByName( innerTypes[i] );
                    if ( type == null )
                    {
                        if ( ( bindingOptions & BindingOptions.DontThrowException ) != 0 )
                        {
                            return null;
                        }
                        else
                        {
                            throw ExceptionHelper.Core.CreateBindingException( "CannotFindTypeInCurrentModule",
                                                                               string.Join( "::", innerTypes, 0, i ),
                                                                               this.Name );
                        }
                    }
                }

                return type;
            }
            else
            {
                // The name is given in the following format:
                // System.Security.Permissions.SecurityPermissionFlag,mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089

                IAssembly assembly = this.FindAssembly( fullTypeName.Substring( comma + 1 ), bindingOptions );
                AssemblyRefDeclaration assemblyRef;
                if ( assembly == null )
                {
                    return null;
                }
                else
                {
                    string shortTypeName = fullTypeName.Substring( 0, comma );

                    if ( ( assemblyRef = assembly as AssemblyRefDeclaration ) != null )
                    {
                        // The type is defined in an external assembly.

                        if ( ( bindingOptions & BindingOptions.OnlyDefinition ) != 0 )
                        {
                            if ( ( bindingOptions & BindingOptions.DontThrowException ) != 0 )
                            {
                                return null;
                            }
                            else
                            {
                                throw ExceptionHelper.Core.CreateBindingException( "CannotFindTypeInCurrentModule",
                                                                                   fullTypeName,
                                                                                   this.Name );
                            }
                        }
                        else
                        {
                            return assemblyRef.FindType( shortTypeName, bindingOptions );
                        }
                    }
                    else
                    {
                        // The type is located in the current assembly.
                        // Recall us recursively with the short name.

                        return this.FindType( shortTypeName, bindingOptions );
                    }
                }
            }
        }

        private TypeDefDeclaration FindModuleType( BindingOptions bindingOptions )
        {
            TypeDefDeclaration type = this.types.GetByName( "<Module>" );
            if ( type == null && ( bindingOptions & BindingOptions.ExistenceMask ) != BindingOptions.OnlyExisting )
            {
                TypeDefDeclaration typeDef = new TypeDefDeclaration {Name = "<Module>"};
                this.types.Add( typeDef );

                type = typeDef;
            }

            return type;
        }

        /// <summary>
        /// Finds in the current module a field given its runtime representation (<see cref="FieldInfo"/>).
        /// </summary>
        /// <param name="reflectionField">The runtime field representation.</param>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns>The requested field, or <b>null</b> if the requested field
        /// could not be found.</returns>
        public IField FindField( FieldInfo reflectionField, BindingOptions bindingOptions )
        {
            #region Preconditions

            this.AssertNotDisposed();
            ExceptionHelper.AssertArgumentNotNull( reflectionField, "reflectionField" );

            #endregion

            // Find the declaring type.
            IType type;
            if ( reflectionField.DeclaringType == null )
            {
                Trace.ReflectionBinding.WriteLine(
                    "ModuleDeclaration.Fields.GetField( {{{0}}} ): this is a module field.",
                    reflectionField );

                type = this.FindModuleType( bindingOptions );
            }
            else
            {
                type = (IType) this.FindType( reflectionField.DeclaringType,
                                              bindingOptions | BindingOptions.DisallowIntrinsicSubstitution );
            }

            if ( type == null )
            {
                Trace.ReflectionBinding.WriteLine(
                    "ModuleDeclaration.Fields.GetField('{0}::{1}'): cannot find the declaring type.",
                    reflectionField.DeclaringType, reflectionField );
                return null;
            }

            ITypeSignature fieldType =
                this.FindType( reflectionField.FieldType,
                               ( bindingOptions & ~BindingOptions.RequireGenericMask ) |
                               BindingOptions.RequireGenericInstance );
            if ( fieldType == null )
            {
                Trace.ReflectionBinding.WriteLine(
                    "ModuleDeclaration.Fields.GetField('{0}::{1}'): cannot find the field type.",
                    reflectionField.DeclaringType, reflectionField );
                return null;
            }

            // There may be siblings.
            IEnumerator<IType> siblingTypesEnumerator;
            TypeSpecDeclaration typeSpec = type as TypeSpecDeclaration;
            if ( typeSpec != null )
            {
                siblingTypesEnumerator = EnumeratorEnlarger.EnlargeEnumerator<TypeSpecDeclaration, IType>(
                    typeSpec.GetSiblings().GetEnumerator() );
            }
            else
            {
                siblingTypesEnumerator = new SingletonEnumerator<IType>( type );
            }

            // Look for fields in all siblings.
            while ( siblingTypesEnumerator.MoveNext() )
            {
                IField field = siblingTypesEnumerator.Current.Fields.GetField( reflectionField.Name, fieldType,
                                                                               ( bindingOptions &
                                                                                 ~BindingOptions.ExistenceMask ) |
                                                                               BindingOptions.OnlyExisting | BindingOptions.DontThrowException );

                if ( field != null )
                {
                    return field;
                }
            }

            // Not found but we are allowed to create it. So create it.
            if ( ( bindingOptions & BindingOptions.ExistenceMask ) != BindingOptions.OnlyExisting )
            {
                return type.Fields.GetField( reflectionField.Name, fieldType, bindingOptions );
            }

            if ( ( bindingOptions & BindingOptions.DontThrowException ) != 0 )
            {
                return null;
            }
            else
            {
                throw ExceptionHelper.Core.CreateBindingException( "CannotFindReflectionField",
                                                                   reflectionField.ToString(), this.Name );
            }
        }

        /// <summary>
        /// Finds a method given the name of its declaring type, the method name, and a predicate.
        /// </summary>
        /// <param name="typeName">Name of the declaring type.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="predicate">Predicate evaluated for each method overload. It can be true only once.</param>
        /// <returns>The method.</returns>
        public IMethod FindMethod( string typeName, string methodName, Predicate<MethodDefDeclaration> predicate )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotEmptyOrNull( typeName, "typeName" );

            #endregion

            TypeDefDeclaration typeDef = this.Domain.FindTypeDefinition( typeName );

            return this.FindMethod( typeDef, methodName, predicate );
        }

        /// <summary>
        /// Finds a method in the current module given its declaring type and the method name.
        /// </summary>
        /// <param name="type">Type declaring the method.</param>
        /// <param name="methodName">Method name.</param>
        /// <returns>The method.</returns>
        public IMethod FindMethod( ITypeSignature type, string methodName )
        {
            return this.FindMethod( type, methodName, null );
        }

        /// <summary>
        /// Finds a method in the current module given its declaring type, its name, and its
        /// number of parameters.
        /// </summary>
        /// <param name="type">Type declaring the method.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterCount">Number of parameters of the method.</param>
        /// <returns>The method.</returns>
        public IMethod FindMethod( ITypeSignature type, string methodName, int parameterCount )
        {
            return this.FindMethod( type, methodName, declaration => declaration.Parameters.Count == parameterCount );
        }

        /// <summary>
        /// Finds a method in the current module given its declaring type, its name,
        /// and a predicate.
        /// </summary>
        /// <param name="type">Type declaring the method.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="predicate">Predicate determining of the method is the right one.</param>
        /// <returns>The method.</returns>
        public IMethod FindMethod( ITypeSignature type, string methodName, Predicate<MethodDefDeclaration> predicate )
        {
            return this.FindMethod( type.GetTypeDefinition(), methodName, predicate );
        }

        /// <summary>
        /// Finds a method in the current module given its declaring type, its name,
        /// and a predicate.
        /// </summary>
        /// <param name="typeDef">Type declaring the method.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="predicate">Predicate determining of the method is the right one.</param>
        /// <returns>The method.</returns>
        public IMethod FindMethod( TypeDefDeclaration typeDef, string methodName,
                                   Predicate<MethodDefDeclaration> predicate )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( typeDef, "typeDef" );
            ExceptionHelper.AssertArgumentNotEmptyOrNull( methodName, "methodName" );

            #endregion

            MethodDefDeclaration method = null;
            foreach ( MethodDefDeclaration candidate in typeDef.Methods.GetByName( methodName ) )
            {
                if ( predicate == null || predicate( candidate ) )
                {
                    if ( method != null )
                    {
                        throw ExceptionHelper.Core.CreateBindingException( "AmbiguousMethodMatch",
                                                                           methodName, typeDef );
                    }

                    method = candidate;
                }
            }

            if ( method == null )
                throw ExceptionHelper.Core.CreateBindingException( "CannotFindMethod",
                                                                   methodName, typeDef );

            return method.Translate( this );
        }

        /// <summary>
        /// Finds a method in a type given its name.
        /// </summary>
        /// <param name="typeName">Name of the type declaring the method.</param>
        /// <param name="methodName">Method name (should be unique in that type).</param>
        /// <returns>The method.</returns>
        public IMethod FindMethod( string typeName, string methodName )
        {
            return this.FindMethod( typeName, methodName, (Predicate<MethodDefDeclaration>) null );
        }


        /// <summary>
        /// Finds a method in a type given its name and number of parameters.
        /// </summary>
        /// <param name="typeName">Name of the type declaring the method.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterCount">Number of parameters of the method
        /// (there should be a single method named <paramref name="methodName"/> with that
        /// number of parameters).</param>
        /// <returns>The method.</returns>
        public IMethod FindMethod( string typeName, string methodName, int parameterCount )
        {
            return this.FindMethod( typeName, methodName, method => method.Parameters.Count == parameterCount );
        }

        /// <summary>
        /// Finds a method in a type given its name and the name of the parameter types.
        /// </summary>
        /// <param name="typeName">Name of the type declaring the method.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterTypeNames">Name of the parameter types.</param>
        /// <returns>The method.</returns>
        public IMethod FindMethod( string typeName, string methodName, params string[] parameterTypeNames )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotEmptyOrNull( typeName, "typeName" );
            ExceptionHelper.AssertArgumentNotEmptyOrNull( methodName, "methodName" );

            #endregion

            TypeDefDeclaration typeDef = this.Domain.FindTypeDefinition( typeName );

            ITypeSignature[] parameterTypes =
                new ITypeSignature[parameterTypeNames != null ? parameterTypeNames.Length : 0];

            for ( int i = 0; i < parameterTypes.Length; i++ )
            {
                parameterTypes[i] = typeDef.Module.FindType( parameterTypeNames[i], BindingOptions.Default );
            }

            foreach ( MethodDefDeclaration candidate in typeDef.Methods.GetByName( methodName ) )
            {
                if ( candidate.Parameters.Count != parameterTypes.Length )
                    continue;

                for ( int i = 0; i < parameterTypes.Length; i++ )
                {
                    if ( !candidate.Parameters[i].ParameterType.MatchesReference( parameterTypes[i] ) )
                        continue;
                }

                return candidate.Translate( this );
            }

            throw ExceptionHelper.Core.CreateBindingException( "CannotFindMethod",
                                                               methodName, typeDef );
        }

        /// <summary>
        /// Finds in the current module a method given its runtime representation (<see cref="MethodBase"/>).
        /// </summary>
        /// <param name="reflectionMethod">The method runtime representation.</param>
        /// <param name="bindingOptions">Determines the behavior of the binder in case
        /// that the method could not be found in the current module.</param>
        /// <returns>The requested method, or <b>null</b> if the requested method
        /// could not be found.</returns>
        public IMethod FindMethod( MethodBase reflectionMethod, BindingOptions bindingOptions )
        {
            #region Preconditions

            this.AssertNotDisposed();
            ExceptionHelper.AssertArgumentNotNull( reflectionMethod, "reflectionMethod" );
            ExceptionHelper.Core.AssertValidArgument(
                ( !reflectionMethod.IsGenericMethod && !reflectionMethod.DeclaringType.IsGenericType ) ||
                ( bindingOptions & BindingOptions.RequireGenericMask ) != 0, "bindingOptions",
                "GenericBindingOptionsRequired" );

            #endregion

            MethodBase reflectionGenericMethodDef;

            MethodInfo methodInfo = reflectionMethod as MethodInfo;

            // If the method is a generic instance, look for the generic definition.
            bool buildMethodSpec;
            BindingOptions typeBindingOptions = bindingOptions;
            if ( reflectionMethod.IsGenericMethod )
            {
                switch ( bindingOptions & BindingOptions.RequireGenericMask )
                {
                    case BindingOptions.RequireGenericDefinition:
                        reflectionGenericMethodDef = methodInfo;
                        buildMethodSpec = false;
                        break;

                    case BindingOptions.RequireGenericInstance:
                        reflectionGenericMethodDef = methodInfo.GetGenericMethodDefinition();
                        buildMethodSpec = true;
                        break;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException( bindingOptions, "bindingOptions" );
                }
            }
            else
            {
                reflectionGenericMethodDef = reflectionMethod;
                buildMethodSpec = false;
            }


            // Build the method signature.
            MethodSignature methodSignature = BindingHelper.GetSignature( this.Module,
                                                                          reflectionGenericMethodDef,
                                                                          bindingOptions );
            if ( methodSignature == null )
            {
                Trace.ReflectionBinding.WriteLine(
                    "ModuleDeclaration.FindMethod('{0}::{1}'): cannot find the signature.",
                    reflectionMethod.DeclaringType, reflectionMethod );

                return null;
            }

            // Find the declaring type. There may be many identical TypeRefs or TypeSpecs.
            // We have to inspect each of them.
            IType type;

            if ( reflectionMethod.DeclaringType == null )
            {
                Trace.ReflectionBinding.WriteLine(
                    "ModuleDeclaration.FindMethod( {{{0}}} ): this is a module method.",
                    reflectionMethod );

                type = this.FindModuleType( bindingOptions );
            }
            else
            {
                BindingOptions disallowIntrinsicSubstitution = BindingOptions.Default;

                // If the declaring type is a primitive, we do not substitute to an intrinsic
                // because methods are defined on the wrapping type, not on the intrinsic.
                if ( IntrinsicTypeSignature.IsIntrinsic( reflectionMethod.DeclaringType ) )
                    disallowIntrinsicSubstitution = BindingOptions.DisallowIntrinsicSubstitution;

                ITypeSignature declaringTypeSignature =
                    this.FindType( reflectionMethod.DeclaringType,
                                   typeBindingOptions | disallowIntrinsicSubstitution );
                if ( declaringTypeSignature != null && declaringTypeSignature is TypeSignature )
                {
                    type = this.TypeSpecs.GetBySignature( declaringTypeSignature,
                                                          ( bindingOptions & BindingOptions.OnlyExisting ) == 0 );
                    if ( type == null )
                    {
                        if ( ( bindingOptions & BindingOptions.DontThrowException ) == 0 )
                        {
                            throw new BindingException( string.Format( "There is no TypeSpec for signature '{0}'.",
                                                                       declaringTypeSignature ) );
                        }

                        return null;
                    }
                }
                else
                {
                    type = (IType) declaringTypeSignature;
                }
            }

            if ( type == null )
            {
                Trace.ReflectionBinding.WriteLine(
                    "ModuleDeclaration.FindMethod('{0}::{1}'): cannot find the declaring type.",
                    reflectionMethod.DeclaringType, reflectionMethod );

                return null;
            }

            // There may be siblings.
            IEnumerator<IType> siblingTypesEnumerator;
            TypeSpecDeclaration typeSpec = type as TypeSpecDeclaration;
            if ( typeSpec != null )
            {
                siblingTypesEnumerator = EnumeratorEnlarger.EnlargeEnumerator<TypeSpecDeclaration, IType>(
                    typeSpec.GetSiblings().GetEnumerator() );
            }
            else
            {
                siblingTypesEnumerator = new SingletonEnumerator<IType>( type );
            }

            // Look in all sibling declaring types.
            IMethod genericMethod = null;
            while ( siblingTypesEnumerator.MoveNext() )
            {
                IType siblingType = siblingTypesEnumerator.Current;

                // Look for the method inside the type.
                genericMethod = siblingType.Methods.GetMethod( reflectionMethod.Name, methodSignature,
                                                               ( bindingOptions & ~BindingOptions.ExistenceMask ) |
                                                               BindingOptions.OnlyExisting |
                                                               BindingOptions.DontThrowException );

                if ( genericMethod != null )
                {
                    break;
                }
            }


            // Did not find it?
            if ( genericMethod == null )
            {
                if ( ( bindingOptions & BindingOptions.ExistenceMask ) != BindingOptions.OnlyExisting )
                {
                    // We did not find it but we are allowed to create it. So create it.
                    genericMethod = type.Methods.GetMethod( reflectionMethod.Name, methodSignature, bindingOptions );
                }
                else
                {
                    // We cannot create it. Fail.

                    if ( ( bindingOptions & BindingOptions.DontThrowException ) != 0 )
                    {
                        Trace.ReflectionBinding.WriteLine(
                            "ModuleDeclaration.FindMethod('{0}::{1}'): method not defined in the type.",
                            reflectionMethod.DeclaringType, reflectionMethod );

                        return null;
                    }
                    else
                    {
                        throw ExceptionHelper.Core.CreateBindingException( "CannotFindMethodInType",
                                                                           reflectionMethod.Name, methodSignature,
                                                                           type.ToString() );
                    }
                }
            }

            if ( buildMethodSpec )
            {
                // We have still to find a generic instance of this method.
                IGenericMethodDefinition methodSpecContainer = genericMethod as IGenericMethodDefinition;
                if ( methodSpecContainer == null )
                {
                    throw new AssertionFailedException( string.Format(
                                                            "ModuleDeclaration.FindMethod('{0}::{1}'): the generic method is not an IMethodSpecContainer.",
                                                            reflectionMethod.DeclaringType, reflectionMethod ) );
                }

                Type[] reflectionGenericArguments = methodInfo.GetGenericArguments();
                ITypeSignature[] genericArguments = new ITypeSignature[reflectionGenericArguments.Length];

                for ( int i = 0; i < reflectionGenericArguments.Length; i++ )
                {
                    genericArguments[i] = this.FindType( reflectionGenericArguments[i], bindingOptions );
                    if ( genericArguments[i] == null )
                    {
                        Trace.ReflectionBinding.WriteLine(
                            "ModuleDeclaration.FindMethod('{0}::{1}'): cannot find the generic argument {2}.",
                            reflectionMethod.DeclaringType, reflectionMethod, reflectionGenericArguments[i] );
                        return null;
                    }
                }

                return methodSpecContainer.FindGenericInstance( genericArguments, bindingOptions );
            }

            return genericMethod;
        }

        /// <summary>
        /// Finds in the current module a type given its runtime representation.
        /// </summary>
        /// <param name="reflectionType">The type reflection representation.</param>
        /// <param name="bindingOptions">Determines the behavior of the binder in case
        /// that the type could not be found in the current module.</param>
        /// <returns>The requested type, or <b>null</b> if the requested type
        /// could not be found.</returns>
        public ITypeSignature FindType( Type reflectionType, BindingOptions bindingOptions )
        {
            #region Preconditions

            this.AssertNotDisposed();
            ExceptionHelper.AssertArgumentNotNull( reflectionType, "reflectionType" );
            #endregion

            TypeSignature typeSignature;

            if ( ( ( bindingOptions & BindingOptions.DisallowIntrinsicSubstitution ) == 0 ) &&
                 ( reflectionType.IsPrimitive || reflectionType == typeof(object)
                   || reflectionType == typeof(void) || reflectionType == typeof(string)
                   || reflectionType == typeof(TypedReference) ) )
            {
                typeSignature = this.cache.GetIntrinsic( reflectionType );
            }
            else if ( BindingHelper.IsSimpleType( reflectionType, bindingOptions ) )
            {
                AssemblyNameWrapper assemblyName = AssemblyNameHelper.GetAssemblyName( reflectionType );
                IAssembly assembly = this.FindAssembly(assemblyName, bindingOptions );
                if ( assembly == null )
                {
                    Trace.ReflectionBinding.WriteLine(
                        "ModuleDeclaration.FindType( {{{0}}} ): cannot find the assembly {1}.",
                        reflectionType, assemblyName);

                    return null;
                }

                AssemblyRefDeclaration assemblyRef = assembly as AssemblyRefDeclaration;

                if ( assemblyRef != null )
                {
                    // The type is defined in another assembly.
                    return assemblyRef.FindType( reflectionType.FullName, bindingOptions );
                }
                else
                {
                    // The type is defined in the current assembly.
                    return this.FindType( reflectionType.FullName, bindingOptions );
                }
            }
            else if ( reflectionType.IsGenericParameter )
            {
                // We cannot resolve the generic parameter, because we could provoque
                // an infinite loop. So we use instead non-resolved generic parameters.

                typeSignature = this.cache.GetGenericParameter( reflectionType.GenericParameterPosition,
                                                                reflectionType.DeclaringMethod == null
                                                                    ? GenericParameterKind.Type
                                                                    : GenericParameterKind.Method
                    );
            }
            else if ( reflectionType.HasElementType )
            {
                ITypeSignature nakedType = this.FindType( reflectionType.GetElementType(), bindingOptions );

                if ( reflectionType.IsArray )
                {
                    int rank = reflectionType.GetArrayRank();
                    if ( rank == 1 )
                    {
                        typeSignature = new ArrayTypeSignature( nakedType );
                    }
                    else
                    {
                        ArrayDimension[] dimensions = new ArrayDimension[rank];
                        for ( int i = 0; i < rank; i++ )
                        {
                            dimensions[i] = new ArrayDimension( 0, ArrayDimension.Unlimited );
                        }
                        typeSignature = new ArrayTypeSignature( nakedType, dimensions );
                    }
                }
                else if ( reflectionType.IsPointer )
                {
                    typeSignature = new PointerTypeSignature( nakedType, false );
                }
                else if ( reflectionType.IsByRef )
                {
                    typeSignature = new PointerTypeSignature( nakedType, true );
                }
                else
                {
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "UnreachableCode" );
                }
            }
            else if ( reflectionType.IsGenericType && 
                        (!reflectionType.IsGenericTypeDefinition || (bindingOptions & BindingOptions.RequireGenericInstance) != 0) )
            {
                Type reflectionGenericDefinition = reflectionType.GetGenericTypeDefinition();
                INamedType genericDefinition = (INamedType) this.FindType( reflectionGenericDefinition,
                                                                           ( bindingOptions &
                                                                             ~BindingOptions.RequireGenericMask ) |
                                                                           BindingOptions.RequireGenericDefinition );

                if ( genericDefinition == null )
                {
                    Trace.ReflectionBinding.WriteLine(
                        "ModuleDeclaration.FindType( {{{0}}} ): cannot find the generic type {1}.",
                        reflectionType, reflectionGenericDefinition );

                    return null;
                }
                Type[] reflectionGenericArguments = reflectionType.GetGenericArguments();
                ITypeSignature[] genericArguments = new ITypeSignature[reflectionGenericArguments.Length];

                for ( int i = 0; i < reflectionGenericArguments.Length; i++ )
                {
                    genericArguments[i] =
                        this.FindType( reflectionGenericArguments[i],
                                       bindingOptions & ~BindingOptions.DisallowIntrinsicSubstitution );
                    if ( genericArguments[i] == null )
                    {
                        Trace.ReflectionBinding.WriteLine(
                            "ModuleDeclaration.FindType( {{{0}}} ): cannot find the generic argument {1}.",
                            reflectionType, reflectionGenericArguments[i] );

                        return null;
                    }
                }

                typeSignature = new GenericTypeInstanceTypeSignature( genericDefinition, genericArguments );
            }
            else
            {
                throw ExceptionHelper.Core.CreateAssertionFailedException( "UnreachableCode" );
            }

            if ( ( bindingOptions & BindingOptions.RequireIType ) == 0 )
                return typeSignature;

            // Look for a TypeSpec corresponding to the TypeSignature.
            TypeSpecDeclaration typeSpec =
                this.typeSpecifications.GetBySignature( typeSignature );

            if ( typeSpec != null )
            {
                return typeSpec;
            }
            else
            {
                if ( ( bindingOptions & BindingOptions.OnlyExisting ) != 0 )
                {
                    if ( ( bindingOptions & BindingOptions.DontThrowException ) == 0 )
                    {
                        throw new BindingException( string.Format( "Cannot find a TypeSpec for '{0}'.", reflectionType ) );
                    }

                    return null;
                }

                Trace.ReflectionBinding.WriteLine(
                    "ModuleDeclaration.FindType( {{{0}}} ): creating the TypeSpec.",
                    reflectionType );


                // Should create a new TypeSpec and link it.
                typeSpec = new TypeSpecDeclaration {Signature = typeSignature};
                this.typeSpecifications.Add( typeSpec );

                return typeSpec;
            }
        }

        private IAssembly FindAssembly( string assemblyName, BindingOptions options )
        {
            return this.FindAssembly( AssemblyNameWrapper.GetWrapper( assemblyName ), options );
        }

        #endregion

        /// <inheritdoc />
        public string GetFrameworkVariant()
        {
            if ( this.frameworkVariant == null )
            {
                this.frameworkVariant = FrameworkVariants.FromBytes( this.FindMscorlib().GetPublicKeyToken() );
            }

            return frameworkVariant;
        }

        /// <summary>
        /// Adds a suffix (<b>.CF</b> or <b>.SL</b>) to an assembly name according to the
        /// framework to which the current module is linked.
        /// </summary>
        /// <param name="assemblyName">Assembly name.</param>
        /// <returns>The assembly properly suffixed.</returns>
        public string GetAssemblyNameForFrameworkVariant( string assemblyName )
        {
            string suffix;

            switch ( this.GetFrameworkVariant() )
            {
                case FrameworkVariants.Full:
                    suffix = "";
                    break;

                case FrameworkVariants.Compact:
                    suffix = ".CF";
                    break;

                case FrameworkVariants.Silverlight:
                    suffix = ".SL";
                    break;

                case FrameworkVariants.Micro:
                    suffix = ".MF";
                    break;

                default:
                    throw new AssertionFailedException( "Unexpected framework variant." );
            }

            int comma = assemblyName.IndexOf( ',' );

            if ( comma > 0 )
            {
                return assemblyName.Substring( 0, comma ) + suffix + assemblyName.Substring( comma );
            }
            else
            {
                return assemblyName + suffix;
            }
        }

        /// <summary>
        /// Gets an internal representation for a given type, but for the framework variant
        /// to which the current module is linked, with default binding options.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>The internal type corresponding to <paramref name="type"/>
        /// for the framework variant to which the current module is linked.</returns>
        public ITypeSignature GetTypeForFrameworkVariant( Type type )
        {
            return this.GetTypeForFrameworkVariant( type, BindingOptions.Default );
        }

        /// <summary>
        /// Gets an internal representation for a given type, but for the framework variant
        /// to which the current module is linked, and specifies binding options.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns>The internal type corresponding to <paramref name="type"/>
        /// for the framework variant to which the current module is linked.</returns>
        public ITypeSignature GetTypeForFrameworkVariant( Type type, BindingOptions bindingOptions )
        {
            return
                this.Module.FindType( type.FullName + ", " +
                                      this.GetAssemblyNameForFrameworkVariant( type.Assembly.FullName ),
                                      BindingOptions.Default );
        }

        internal MetadataDeclarationDirectory<object> GetTagDictionary( Guid guid )
        {
            MetadataDeclarationDirectory<object> dictionary;
            if ( !this.tagDictionaries.TryGetValue( guid, out dictionary ) )
            {
                lock ( this.tagDictionaries )
                {
                    if ( !this.tagDictionaries.TryGetValue( guid, out dictionary ) )
                    {
                        dictionary = new MetadataDeclarationDirectory<object>( this );
                        this.tagDictionaries.Add( guid, dictionary );
                    }
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Removes, from all elements of this module (<see cref="MetadataDeclaration"/> only),
        /// all tags of a given <see cref="Guid"/>.
        /// </summary>
        /// <param name="guid">Tag identification.</param>
        public void ClearAllTags( Guid guid )
        {
            this.tagDictionaries.Remove( guid );
        }

        internal override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            if ( disposing )
            {
                this.assemblyRefs.Dispose();
                this.assemblyManifest.Dispose();
                this.data.Dispose();
                this.moduleRefs.Dispose();
                this.typeRefs.Dispose();
                this.typeSpecifications.Dispose();
                this.types.Dispose();
                GC.SuppressFinalize( this );
            }
        }


        /// <summary>
        /// Destructor.
        /// </summary>
        ~ModuleDeclaration()
        {
            if ( !this.IsDisposed )
            {
                this.Dispose( false );
            }
        }

        /// <summary>
        /// Gets a <see cref="MethodSignature"/> for a <see cref="MethodBase"/>.
        /// </summary>
        /// <param name="method">A method.</param>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns>The <see cref="MethodSignature"/> corresponding to <paramref name="method"/>
        /// in the current module.</returns>
        public MethodSignature GetMethodSignature( MethodBase method, BindingOptions bindingOptions )
        {
            return BindingHelper.GetSignature( this, method, bindingOptions );
        }
    }


    namespace Collections
    {
        /// <summary>
        /// Collection of modules (<see cref="ModuleDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class ModuleDeclarationCollection : UniquelyNamedElementCollection<ModuleDeclaration>
        {
            internal ModuleDeclarationCollection( Element parent, string role )
                : base( parent, role )
            {
            }

            #region Overrides of ElementCollection<ModuleDeclaration>

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
