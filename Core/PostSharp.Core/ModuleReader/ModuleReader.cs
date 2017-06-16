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
using System.Configuration.Assemblies;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Collections;
using PostSharp.CodeModel.MarshalTypes;
using PostSharp.CodeModel.SerializationTypes;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;
using PostSharp.PlatformAbstraction;
using CallingConvention=PostSharp.CodeModel.CallingConvention;

namespace PostSharp.ModuleReader
{
    /// <summary>
    /// Builds the Code Object Model from a binary module.
    /// </summary>
    [SuppressMessage( "Microsoft.Naming", "CA172:TypeNamesShouldNotMatchNamespace" )]
    internal sealed partial class ModuleReader : IDisposable
    {
        private readonly AssemblyEnvelope assemblyEnvelope;
        private readonly bool lazyLoading;

        #region Arrays of declarations

        /// <summary>
        /// Collection of binary metadata tables.
        /// </summary>
        private MetadataTables tables;

        /// <summary>
        /// Target <see cref="ModuleDeclaration"/>.
        /// </summary>
        private ModuleDeclaration moduleDeclaration;

        /// <summary>
        /// Type specifications.
        /// </summary>
        private TypeSpecDeclaration[] typeSpecs;

        /// <summary>
        /// Type definitions.
        /// </summary>
        private TypeDefDeclaration[] typeDefs;

        /// <summary>
        /// Type references.
        /// </summary>
        private TypeRefDeclaration[] typeRefs;

        /// <summary>
        /// Module references.
        /// </summary>
        private ModuleRefDeclaration[] moduleRefs;

        /// <summary>
        /// Assembly references.
        /// </summary>
        private AssemblyRefDeclaration[] assemblyRefs;

        /// <summary>
        /// Methods.
        /// </summary>
        private MethodDefDeclaration[] methods;

        /// <summary>
        /// Method specifications.
        /// </summary>
        private MethodSpecDeclaration[] methodSpecs;

        /// <summary>
        /// Permission sets.
        /// </summary>
        private PermissionSetDeclaration[] permissionSets;

        /// <summary>
        /// Parameters.
        /// </summary>
        private ParameterDeclaration[] parameters;

        /// <summary>
        /// Properties.
        /// </summary>
        private PropertyDeclaration[] properties;

        /// <summary>
        /// Events.
        /// </summary>
        private EventDeclaration[] events;

        /// <summary>
        /// Member references.
        /// </summary>
        private MemberRefDeclaration[] memberRefs;

        /// <summary>
        /// Fields.
        /// </summary>
        private FieldDefDeclaration[] fields;

        /// <summary>
        /// Generic parameters.
        /// </summary>
        private GenericParameterDeclaration[] genericParameters;

        /// <summary>
        /// Manifest files.
        /// </summary>
        private ManifestFileDeclaration[] manifestFiles;

        /// <summary>
        /// Data sections.
        /// </summary>
        private readonly Dictionary<uint, DataSectionDeclaration> dataSections =
            new Dictionary<uint, DataSectionDeclaration>();

        /// <summary>
        /// Manifest resources.
        /// </summary>
        private ManifestResourceDeclaration[] manifestResources;

        /// <summary>
        /// Assembly manifest.
        /// </summary>
        private AssemblyManifestDeclaration assemblyManifest;

        /// <summary>
        /// Standalone signatures.
        /// </summary>
        private StandaloneSignatureDeclaration[] standaloneSignatures;

        /// <summary>
        /// Custom attributes.
        /// </summary>
        private CustomAttributeDeclaration[] customAttributes;

        #endregion

        #region Readers

        /// <summary>
        /// Handle of the source module (loaded in the runtime).
        /// </summary>
        private IntPtr hSourceModule;

        private GCHandle imageGCHandle;

        /// <summary>
        /// Source module  (loaded in the runtime).
        /// </summary>
        private readonly Module module;

        /// <summary>
        /// Symbol reader.
        /// </summary>
        private ISymbolReader symbolReader;

        /// <summary>
        /// Whether <see cref="symbolReader"/> has been retrieved.
        /// </summary>
        private bool symbolReaderSet;

        /// <summary>
        /// Image reader.
        /// </summary>
        private ImageReader imageReader;

        private ExportedTypeDeclaration[] exportedTypes;

        private readonly string moduleLocation;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="ModuleReader"/>.
        /// </summary>
        /// <param name="module">The <see cref="Module"/> to be read.</param>
        /// <param name="assemblyEnvelope">Assembly to which the module belongs.</param>
        /// <param name="lazyLoading">Whether the module should be lazily loaded.</param>
        public ModuleReader( Module module, AssemblyEnvelope assemblyEnvelope, bool lazyLoading )
        {
            this.module = module;
            this.moduleLocation = module.Assembly.Location;
            this.assemblyEnvelope = assemblyEnvelope;
            this.lazyLoading = lazyLoading;
        }

        public ModuleReader( string moduleLocation, AssemblyEnvelope assemblyEnvelope, bool lazyLoading )
        {
            this.moduleLocation = moduleLocation;
            this.assemblyEnvelope = assemblyEnvelope;
            this.lazyLoading = lazyLoading;
        }

        private byte[] LoadModuleImage()
        {
            return File.ReadAllBytes( this.moduleLocation );
        }

        private T[] CreateArray<T>( MetadataTableOrdinal table )
        {
            return new T[this.tables.Tables[(int) table].RowCount];
        }

        /// <summary>
        /// Read the module assigned to the current <see cref="ModuleReader"/>
        /// and returns the resulting <see cref="ModuleDeclaration"/>.
        /// </summary>
        /// <returns>The <see cref="ModuleDeclaration"/> built from the module being read.</returns>
        public ModuleDeclaration ReadModule( ReadModuleStrategy strategy )
        {
            if ( strategy == ReadModuleStrategy.FromMemoryImage &&
                 this.module == null )
                throw new InvalidOperationException(
                    "FromMemoryImage strategy not allowed when the reader was constructed from a file name." );

            string moduleName = this.module == null
                                    ? Path.GetFileNameWithoutExtension( this.moduleLocation )
                                    : this.module.Name;


            if ( strategy == ReadModuleStrategy.FromMemoryImage )
            {
                // This gets a pointer to the first byte of the module, i.e. the PE section.
                Trace.ImageReader.WriteLine( "Reading the mapped module {0}.", moduleName );
                this.hSourceModule = Marshal.GetHINSTANCE( module );
                this.imageReader = new ImageReader( this.hSourceModule, true );
            }
            else
            {
                Trace.ImageReader.WriteLine( "Reading the unmapped module {0}.", this.moduleLocation );
                byte[] image = this.LoadModuleImage();
                this.imageGCHandle = GCHandle.Alloc( image, GCHandleType.Pinned );
                this.imageReader = new ImageReader( this.imageGCHandle.AddrOfPinnedObject(), false );
            }

            this.tables = new MetadataTables( this.imageReader );
            this.exportedTypes = this.CreateArray<ExportedTypeDeclaration>( MetadataTableOrdinal.ExportedType );
            this.assemblyRefs = this.CreateArray<AssemblyRefDeclaration>( MetadataTableOrdinal.AssemblyRef );
            this.manifestFiles = this.CreateArray<ManifestFileDeclaration>( MetadataTableOrdinal.File );
            this.moduleRefs = this.CreateArray<ModuleRefDeclaration>( MetadataTableOrdinal.ModuleRef );
            this.typeDefs = this.CreateArray<TypeDefDeclaration>( MetadataTableOrdinal.TypeDef );
            this.typeRefs = this.CreateArray<TypeRefDeclaration>( MetadataTableOrdinal.TypeRef );
            this.memberRefs = this.CreateArray<MemberRefDeclaration>( MetadataTableOrdinal.MemberRef );
            this.typeSpecs = this.CreateArray<TypeSpecDeclaration>( MetadataTableOrdinal.TypeSpec );
            this.methods = this.CreateArray<MethodDefDeclaration>( MetadataTableOrdinal.Method );
            this.fields = this.CreateArray<FieldDefDeclaration>( MetadataTableOrdinal.Field );
            this.events = this.CreateArray<EventDeclaration>( MetadataTableOrdinal.Event );
            this.properties = this.CreateArray<PropertyDeclaration>( MetadataTableOrdinal.Property );
            this.permissionSets = this.CreateArray<PermissionSetDeclaration>( MetadataTableOrdinal.DeclSecurity );
            this.genericParameters = this.CreateArray<GenericParameterDeclaration>( MetadataTableOrdinal.GenericParam );
            this.methodSpecs = this.CreateArray<MethodSpecDeclaration>( MetadataTableOrdinal.MethodSpec );
            this.manifestResources =
                this.CreateArray<ManifestResourceDeclaration>( MetadataTableOrdinal.ManifestResource );
            this.standaloneSignatures =
                this.CreateArray<StandaloneSignatureDeclaration>( MetadataTableOrdinal.StandAloneSig );
            this.parameters = this.CreateArray<ParameterDeclaration>( MetadataTableOrdinal.Param );
            this.customAttributes = new CustomAttributeDeclaration[this.tables.CustomAttributeTable.RowCount];


            MetadataRow moduleRow = this.tables.ModuleTable.GetRow( 0 );

            // Set the trivial properties.
            this.moduleDeclaration = new ModuleDeclaration
                                         {
                                             Name = this.tables.ModuleNameColumn.GetValue( moduleRow ),
                                             FileName = this.moduleLocation,
                                             MetadataToken = new MetadataToken( TokenType.Module, 0 ),
                                             ModuleGuid = this.tables.ModuleMvidColumn.GetValue( moduleRow ),
                                             Subsystem = this.imageReader.Subsystem,
                                             ImageAttributes = ( (ImageAttributes) this.imageReader.CorFlags ),
                                             ImageBase = this.imageReader.ImageBase,
                                             FileAlignment = this.imageReader.FileAlignment,
                                             StackReserve = this.imageReader.StackReserve,
                                             Tables = new MetadataDeclarationTables( this ),
                                             ModuleReader = this
                                         };
            this.assemblyEnvelope.Modules.Add( this.moduleDeclaration );
            this.moduleDeclaration.SetRuntimeModule( module );


            this.ImportAssemblyManifest();

            if ( !this.lazyLoading )
            {
                this.ImportAssemblyRefs();
                this.ImportModuleRefs();
                this.ImportTypeRefs();
                this.ImportTypeDefs();
                this.ImportNestedTypes();

                this.ImportTypeMembers();
                this.ImportGenericParameters();

                this.ImportTypeSpecs();
                this.ImportPropertyMap();
                this.ImportEventMap();
                this.ImportMethodSemantics();

                this.ImportMemberRefs();
                this.ImportInterfaceImplementations();
                this.ImportMethodImplementations();
                this.ImportMethodSpecs();
                this.ImportStandaloneSignatures();
                this.ImportManifestResources();
                this.ImportManifestFiles();
                this.ImportExportedTypes();
                this.ImportPermissionSets();
                this.ImportCustomAttributes();


                // The following stuff is never read in a lazy loaded assembly.
                this.ImportClassLayout();
                this.ImportFieldRVAs();
                this.ImportFieldMarshals();
                this.ImportFieldLayout();
                this.ImportPInvokeMaps();
                this.ImportConstants();
            }


            // Build the module tables
            this.moduleDeclaration.Tables.SetTable( TokenType.TypeSpec, typeSpecs );
            this.moduleDeclaration.Tables.SetTable( TokenType.TypeDef, typeDefs );
            this.moduleDeclaration.Tables.SetTable( TokenType.TypeRef, typeRefs );
            this.moduleDeclaration.Tables.SetTable( TokenType.ModuleRef, moduleRefs );
            this.moduleDeclaration.Tables.SetTable( TokenType.AssemblyRef, assemblyRefs );
            this.moduleDeclaration.Tables.SetTable( TokenType.MethodDef, methods );
            this.moduleDeclaration.Tables.SetTable( TokenType.MethodSpec, methodSpecs );
            this.moduleDeclaration.Tables.SetTable( TokenType.Property, properties );
            this.moduleDeclaration.Tables.SetTable( TokenType.Event, events );
            this.moduleDeclaration.Tables.SetTable( TokenType.MemberRef, memberRefs );
            this.moduleDeclaration.Tables.SetTable( TokenType.FieldDef, fields );
            this.moduleDeclaration.Tables.SetTable( TokenType.GenericParam, genericParameters );
            this.moduleDeclaration.Tables.SetTable( TokenType.File, manifestFiles );
            this.moduleDeclaration.Tables.SetTable( TokenType.ManifestResource, manifestResources );
            this.moduleDeclaration.Tables.SetTable( TokenType.Signature, standaloneSignatures );
            this.moduleDeclaration.Tables.SetTable( TokenType.Permission, permissionSets );
            this.moduleDeclaration.Tables.SetTable( TokenType.CustomAttribute, customAttributes );
            this.moduleDeclaration.Tables.SetTable( TokenType.ParamDef, this.parameters );


            // Set the symbol reader
            this.moduleDeclaration.SymbolReader = this.GetSymbolReader();


            // Set the entry point
            MetadataToken entryPointToken = this.imageReader.EntryPoint;
            if ( !entryPointToken.IsNull && entryPointToken.TokenType == TokenType.MethodDef )
            {
                this.moduleDeclaration.EntryPoint = this.methods[entryPointToken.Index];
            }


            // Set runtime version
            this.moduleDeclaration.RuntimeMajorVersion = this.imageReader.MajorRuntimeVersion;
            this.moduleDeclaration.RuntimeMinorVersion = this.imageReader.MinorRuntimeVersion;


            // Set metadata version 
            {
                int major, minor;
                string versionString;
                this.imageReader.GetMetadataVersion( out major, out minor, out versionString );
                this.moduleDeclaration.MetadataMajorVersion = major;
                this.moduleDeclaration.MetadataMinorVersion = minor;
                this.moduleDeclaration.MetadataVersionString = versionString;
            }


            // Unmanaged resource
            this.imageReader.ReadUnmanagedResources( this.moduleDeclaration.UnmanagedResources );

            return this.moduleDeclaration;
        }

        #region Read types and type members

        #region Type definitions

        private TypeDefDeclaration GetTypeDef( int i )
        {
            TypeDefDeclaration typeDef = this.typeDefs[i];

            if ( typeDef != null ) return typeDef;

            if ( this.lazyLoading && this.enclosingTypesIndex == null )
            {
                this.enclosingTypesIndex = new Dictionary<int, int>( 64 );
                this.nestedTypesIndex = new MultiDictionary<MetadataToken, int>();
                this.ImportNestedTypes();
            }

            return this.ImportTypeDef( i, null );
        }

        internal void ImportTypeDefs( ITypeContainer typeContainer )
        {
            if ( !this.lazyLoading )
            {
                typeContainer.Types.PrepareImport( 0 );
                return;
            }

            if ( this.enclosingTypesIndex == null )
            {
                this.enclosingTypesIndex = new Dictionary<int, int>( 64 );
                this.nestedTypesIndex = new MultiDictionary<MetadataToken, int>();
                this.ImportNestedTypes();
            }

            TypeDefDeclaration enclosingType = typeContainer as TypeDefDeclaration;

            if ( enclosingType != null )
            {
                // We are looking for the nested types of some type.
                MetadataToken tkEnclosingType = enclosingType.MetadataToken;
                int n = this.nestedTypesIndex.GetCountByKey( tkEnclosingType );
                enclosingType.Types.PrepareImport( n );

                if ( n > 0 )
                {
                    foreach ( int i in this.nestedTypesIndex[tkEnclosingType] )
                    {
                        if ( this.typeDefs[i] == null )
                            this.ImportTypeDef( i, enclosingType );
                    }
                }
            }
            else
            {
                // We are looking for non-nested types.
                typeContainer.Types.PrepareImport( this.typeDefs.Length - this.enclosingTypesIndex.Count );
                for ( int i = 0; i < this.typeDefs.Length; i++ )
                {
                    if ( !this.enclosingTypesIndex.ContainsKey( i ) )
                    {
                        if ( this.typeDefs[i] == null )
                            this.ImportTypeDef( i, null );
                    }
                }
            }
        }

        private TypeDefDeclaration ImportTypeDef( int i, TypeDefDeclaration enclosingType )
        {
            MetadataRow row = this.tables.TypeDefTable.GetRow( i );
            TypeDefDeclaration typeDef = new TypeDefDeclaration
                                             {
                                                 MetadataToken = new MetadataToken( TokenType.TypeDef, i ),
                                                 Name = MakeTypeName(
                                                     this.tables.TypeDefNamespaceColumn.GetValue( row ),
                                                     this.tables.TypeDefNameColumn.GetValue( row ) ),
                                                 Attributes = ( (TypeAttributes)
                                                                this.tables.TypeDefFlagsColumn.GetValue( row ) )
                                             };
            this.typeDefs[i] = typeDef;

            // Import into enclosing type
            switch ( typeDef.Attributes & TypeAttributes.VisibilityMask )
            {
                case TypeAttributes.NestedAssembly:
                case TypeAttributes.NestedFamANDAssem:
                case TypeAttributes.NestedFamily:
                case TypeAttributes.NestedFamORAssem:
                case TypeAttributes.NestedPrivate:
                case TypeAttributes.NestedPublic:
                    // Nested.
                    if ( this.lazyLoading )
                    {
                        // Find the parent from the table of nested types.
                        if ( enclosingType == null )
                        {
                            int enclosingIndex = this.enclosingTypesIndex[i];
                            enclosingType = GetTypeDef( enclosingIndex );
                        }
                        enclosingType.Types.Import( typeDef );
                    }
                    else
                    {
                        // Do nothing here. The type will be added to the parent when processing
                        // the table of nested types.
                    }
                    break;

                default:
                    this.moduleDeclaration.Types.Import( typeDef );
                    break;
            }

            typeDef.BaseType = ((IType)this.ResolveType(
                                 this.tables.TypeDefExtendsColumn.GetValue(row)));


            return typeDef;
        }

        private Dictionary<int, int> enclosingTypesIndex;
        private MultiDictionary<MetadataToken, int> nestedTypesIndex;

        /// <summary>
        /// Relates nested types to their nesting types.
        /// </summary>
        private void ImportNestedTypes()
        {
            for ( int i = 0; i < this.tables.NestedClassTable.RowCount; i++ )
            {
                MetadataRow row = this.tables.NestedClassTable.GetRow( i );
                MetadataToken tkEnclosingType = this.tables.NestedClassEnclosingClassColumn.GetValue( row );
                MetadataToken tkNestedType = this.tables.NestedClassNestedClassColumn.GetValue( row );

                if ( this.lazyLoading )
                {
                    this.enclosingTypesIndex.Add( tkNestedType.Index, tkEnclosingType.Index );
                    this.nestedTypesIndex.Add( tkEnclosingType, tkNestedType.Index );
                }
                else
                {
                    TypeDefDeclaration enclosingType = this.typeDefs[tkEnclosingType.Index];
                    TypeDefDeclaration nestedType = this.typeDefs[tkNestedType.Index];
                    enclosingType.Types.Import( nestedType );
                }
            }
        }

        private void IndexTypeDefMembers()
        {
            for ( int i = 0; i < this.typeDefs.Length; i++ )
            {
                MetadataRow row = this.tables.TypeDefTable.GetRow( i );

                MetadataToken tkFirstField, tkLastField, tkFirstMethod, tkLastMethod;
                this.tables.TypeDefMethodListColumn.GetRange( row, out tkFirstMethod, out tkLastMethod );
                this.tables.TypeDefFieldListColumn.GetRange( row, out tkFirstField, out tkLastField );

                this.IndexMethodDefs( tkFirstMethod, tkLastMethod, i );
                this.IndexFieldDefs( tkFirstField, tkLastField, i );
            }
        }

        /// <summary>
        /// Imports all type definitions.
        /// </summary>
        private void ImportTypeDefs()
        {
            for ( int i = 0; i < this.typeDefs.Length; i++ )
            {
                if ( this.typeDefs[i] == null )
                    ImportTypeDef( i, null );
            }
        }

        private void ImportTypeMembers()
        {
            for ( int i = 0; i < this.typeDefs.Length; i++ )
            {
                MetadataRow row = this.tables.TypeDefTable.GetRow( i );
                MetadataToken tkFirstField, tkLastField, tkFirstMethod, tkLastMethod;
                this.tables.TypeDefMethodListColumn.GetRange( row, out tkFirstMethod, out tkLastMethod );
                this.tables.TypeDefFieldListColumn.GetRange( row, out tkFirstField, out tkLastField );

                this.ImportFieldDefs( tkFirstField, tkLastField, this.typeDefs[i] );
                this.ImportMethodDefs( tkFirstMethod, tkLastMethod, this.typeDefs[i] );
            }
        }

        #endregion

        #region Interface implementations

        private MultiDictionary<MetadataToken, int> interfaceImplementationsIndex;

        private void ImportInterfaceImplementation( TypeDefDeclaration classType, int i )
        {
            MetadataRow row = this.tables.InterfaceImplTable.GetRow( i );
            InterfaceImplementationDeclaration interfaceImpl = new InterfaceImplementationDeclaration
                                                                   {
                                                                       MetadataToken =
                                                                           new MetadataToken( TokenType.InterfaceImpl, i ),
                                                                       ImplementedInterface =
                                                                           this.ResolveType(
                                                                           this.tables.InterfaceImplInterfaceColumn.
                                                                               GetValue( row ) )
                                                                   };
            classType.InterfaceImplementations.Import( interfaceImpl );
        }

        internal void ImportInterfaceImplementations( TypeDefDeclaration typeDef )
        {
            if ( !this.lazyLoading )
            {
                typeDef.InterfaceImplementations.PrepareImport( 0 );
                return;
            }

            if ( this.interfaceImplementationsIndex == null )
            {
                this.interfaceImplementationsIndex = new MultiDictionary<MetadataToken, int>( 64 );
                this.ImportInterfaceImplementations();
            }

            MetadataToken tkTypeDef = typeDef.MetadataToken;
            int n = this.interfaceImplementationsIndex.GetCountByKey( tkTypeDef );
            typeDef.InterfaceImplementations.PrepareImport( n );
            if ( n > 0 )
            {
                foreach ( int i in this.interfaceImplementationsIndex[tkTypeDef] )
                {
                    this.ImportInterfaceImplementation( typeDef, i );
                }
            }
        }

        private void ImportInterfaceImplementations()
        {
            for ( int i = 0; i < this.tables.InterfaceImplTable.RowCount; i++ )
            {
                MetadataRow row = this.tables.InterfaceImplTable.GetRow( i );
                MetadataToken tkClass = this.tables.InterfaceImplClassColumn.GetValue( row );
                if ( this.lazyLoading )
                {
                    this.interfaceImplementationsIndex.Add( tkClass, i );
                }
                else
                {
                    TypeDefDeclaration classType = this.typeDefs[tkClass.Index];

                    this.ImportInterfaceImplementation( classType, i );
                }
            }
        }

        #endregion

        #region Events

        internal void ImportEvents( TypeDefDeclaration typeDef )
        {
            if ( !this.lazyLoading )
            {
                typeDef.Events.PrepareImport( 0 );
                return;
            }

            if ( this.eventsIndex == null )
            {
                this.eventsIndex = new Dictionary<MetadataToken, TokenRange>( 64 );
                this.ImportEventMap();
            }

            MetadataToken tkTypeDef = typeDef.MetadataToken;
            TokenRange tokenRange;
            if ( this.eventsIndex.TryGetValue( tkTypeDef, out tokenRange ) )
            {
                int n = 1 + tokenRange.Last.Index - tokenRange.First.Index;
                typeDef.Events.PrepareImport( n );
                this.ImportEvents( tokenRange.First, tokenRange.Last, typeDef );
            }
            else
            {
                typeDef.Events.PrepareImport( 0 );
            }
        }

        private void ImportEvents( MetadataToken tkFirstEvent, MetadataToken tkLastEvent,
                                   TypeDefDeclaration declaringType )
        {
            for ( MetadataToken j = tkFirstEvent; j.IsSmallerOrEqualThan( tkLastEvent ); j.Increment() )
            {
                MetadataRow row = this.tables.EventTable.GetRow( j );

                EventDeclaration @event = new EventDeclaration
                                              {
                                                  MetadataToken = new MetadataToken( TokenType.Event, j.Index ),
                                                  Name = this.tables.EventNameColumn.GetValue( row ),
                                                  EventType = this.ResolveType(
                                                      this.tables.EventEventTypeColumn.GetValue( row ) ),
                                                  Attributes = ( (EventAttributes)
                                                                 this.tables.EventEventFlagsColumn.GetValue( row ) )
                                              };
                this.events[j.Index] = @event;


                declaringType.Events.Import( @event );
            }
        }

        private Dictionary<MetadataToken, TokenRange> eventsIndex;

        /// <summary>
        /// Imports events.
        /// </summary>
        private void ImportEventMap()
        {
            for ( int i = 0; i < this.tables.EventMapTable.RowCount; i++ )
            {
                MetadataRow eventMapRow = this.tables.EventMapTable.GetRow( i );
                MetadataToken tkFirstEvent, tkLastEvent;
                this.tables.EventMapEventListColumn.GetRange( eventMapRow, out tkFirstEvent, out tkLastEvent );
                MetadataToken tkParent = this.tables.EventMapParentColumn.GetValue( eventMapRow );

                if ( !tkFirstEvent.IsNull )
                {
                    if ( this.lazyLoading )
                    {
                        this.eventsIndex.Add( tkParent, new TokenRange( tkFirstEvent, tkLastEvent ) );
                    }
                    else
                    {
                        TypeDefDeclaration type = this.typeDefs[tkParent.Index];
                        this.ImportEvents( tkFirstEvent, tkLastEvent, type );
                    }
                }
            }
        }

        #endregion

        #region Properties

        internal void ImportProperties( TypeDefDeclaration typeDef )
        {
            if ( !this.lazyLoading )
            {
                typeDef.Properties.PrepareImport( 0 );
                return;
            }

            if ( this.propertiesIndex == null )
            {
                this.propertiesIndex = new Dictionary<MetadataToken, TokenRange>( 64 );
                this.ImportPropertyMap();
            }

            MetadataToken tkTypeDef = typeDef.MetadataToken;
            TokenRange tokenRange;
            if ( this.propertiesIndex.TryGetValue( tkTypeDef, out tokenRange ) )
            {
                int n = 1 + tokenRange.Last.Index - tokenRange.First.Index;
                typeDef.Properties.PrepareImport( n );
                this.ImportProperties( tokenRange.First, tokenRange.Last, typeDef );
            }
            else
            {
                typeDef.Properties.PrepareImport( 0 );
            }
        }

        private void ImportProperties( MetadataToken tkFirstProperty, MetadataToken tkLastProperty,
                                       TypeDefDeclaration declaringType )
        {
            for ( MetadataToken j = tkFirstProperty; j.IsSmallerOrEqualThan( tkLastProperty ); j.Increment() )
            {
                MetadataRow row = this.tables.PropertyTable.GetRow( j );

                PropertyDeclaration propertyDeclaration = new PropertyDeclaration
                                                              {
                                                                  MetadataToken =
                                                                      new MetadataToken( TokenType.Property, j.Index ),
                                                                  Name = this.tables.PropertyNameColumn.GetValue( row ),
                                                                  Attributes = ( (PropertyAttributes)
                                                                                 this.tables.PropertyPropFlagsColumn.
                                                                                     GetValue( row ) )
                                                              };
                this.properties[j.Index] = propertyDeclaration;

                declaringType.Properties.Import( propertyDeclaration );

                // Read the signature
                BufferReader buffer = this.tables.PropertyTypeColumn.GetValueBufferReader( row );
                CallingConvention callingConvention = (CallingConvention) buffer.ReadByte();
                if ( ( callingConvention & CallingConvention.CallingConventionMask ) !=
                     CallingConvention.Property )
                {
                    throw ExceptionHelper.Core.CreateAssertionFailedException(
                        string.Format( CultureInfo.InvariantCulture,
                                       "callingConvention = 0x{0:X}, expected {1}.",
                                       callingConvention, CallingConvention.Property ) );
                }

                uint paramCount = buffer.ReadCompressedInteger();
                propertyDeclaration.PropertyType = this.ReadTypeSignature( buffer );
                propertyDeclaration.Parameters.Capacity = (int) paramCount;
                propertyDeclaration.CallingConvention = callingConvention &
                                                        ~CallingConvention.CallingConventionMask;

                for ( int k = 0; k < paramCount; k++ )
                {
                    propertyDeclaration.Parameters.Add( this.ReadTypeSignature( buffer ) );
                }
            }
        }

        private Dictionary<MetadataToken, TokenRange> propertiesIndex;


        private void ImportPropertyMap()
        {
            for ( int i = 0; i < this.tables.PropertyMapTable.RowCount; i++ )
            {
                MetadataRow propertyMapRow = this.tables.PropertyMapTable.GetRow( i );
                MetadataToken tkParent = this.tables.PropertyMapParentColumn.GetValue( propertyMapRow );

                MetadataToken tkFirstProperty, tkLastProperty;
                this.tables.PropertyMapPropertyListColumn.GetRange( propertyMapRow, out tkFirstProperty,
                                                                    out tkLastProperty );

                if ( !tkFirstProperty.IsNull )
                {
                    if ( this.lazyLoading )
                    {
                        this.propertiesIndex.Add( tkParent, new TokenRange( tkFirstProperty, tkLastProperty ) );
                    }
                    else
                    {
                        TypeDefDeclaration type = this.typeDefs[tkParent.Index];

                        this.ImportProperties( tkFirstProperty, tkLastProperty, type );
                    }
                }
            }
        }

        #endregion

        #region Method semantics

        private MultiDictionary<MetadataToken, int> methodSemanticsIndex;

        internal void ImportMethodSemantics( MethodGroupDeclaration methodGroup )
        {
            if ( !this.lazyLoading )
            {
                methodGroup.Members.PrepareImport( 0 );
                return;
            }

            if ( this.methodSemanticsIndex == null )
            {
                this.methodSemanticsIndex = new MultiDictionary<MetadataToken, int>( 64 );
                this.ImportMethodSemantics();
            }

            MetadataToken tkMethodGroup = methodGroup.MetadataToken;
            int n = this.methodSemanticsIndex.GetCountByKey( tkMethodGroup );
            methodGroup.Members.PrepareImport( n );
            if ( n > 0 )
            {
                foreach ( int i in this.methodSemanticsIndex[tkMethodGroup] )
                {
                    this.ImportMethodSemantic( i, methodGroup );
                }
            }
        }

        private void ImportMethodSemantic( int i, MethodGroupDeclaration association )
        {
            MetadataRow row = this.tables.MethodSemanticsTable.GetRow( i );

            MethodSemanticDeclaration methodSemantic = new MethodSemanticDeclaration
                                                           {
                                                               MetadataToken =
                                                                   new MetadataToken( TokenType.MethodSemantic, i ),
                                                               Method =
                                                                   ( (MethodDefDeclaration)
                                                                     this.ResolveMethod(
                                                                         this.tables.MethodSemanticsMethodColumn.
                                                                             GetValue( row ) ) ),
                                                               Semantic = ( (MethodSemantics)
                                                                            this.tables.MethodSemanticsSemanticColumn
                                                                                .GetValue( row ) )
                                                           };

            association.Members.Import( methodSemantic );
        }

        /// <summary>
        /// Imports method semantics (for both events and properties).
        /// </summary>
        private void ImportMethodSemantics()
        {
            for ( int i = 0; i < this.tables.MethodSemanticsTable.RowCount; i++ )
            {
                MetadataRow row = this.tables.MethodSemanticsTable.GetRow( i );
                MetadataToken tkAssociation = this.tables.MethodSemanticsAssociationColumn.GetValue( row );

                if ( this.lazyLoading )
                {
                    this.methodSemanticsIndex.Add( tkAssociation, i );
                }
                else
                {
                    MethodGroupDeclaration association;
                    switch ( tkAssociation.TokenType )
                    {
                        case TokenType.Event:
                            association = this.events[tkAssociation.Index];
                            break;

                        case TokenType.Property:
                            association = this.properties[tkAssociation.Index];
                            break;

                        default:
                            throw ExceptionHelper.Core.CreateAssertionFailedException( "UnexpectedTokenType",
                                                                                       tkAssociation.TokenType,
                                                                                       "MethodSemanticsMethodColumn",
                                                                                       "Event, Property" );
                    }


                    this.ImportMethodSemantic( i, association );
                }
            }
        }

        #endregion

        #region Method-level interface implementations.

        internal void ImportMethodImplementations( MethodDefDeclaration methodDef )
        {
            if ( !this.lazyLoading )
            {
                methodDef.InterfaceImplementations.PrepareImport( 0 );
                return;
            }

            if ( this.methodImplementationsIndex == null )
            {
                this.methodImplementationsIndex = new MultiDictionary<MetadataToken, int>( 64 );
                this.ImportMethodImplementations();
            }

            MetadataToken tk = methodDef.MetadataToken;
            int n = this.methodImplementationsIndex.GetCountByKey( tk );
            methodDef.InterfaceImplementations.PrepareImport( n );

            if ( n > 0 )
            {
                foreach ( int i in this.methodImplementationsIndex[tk] )
                {
                    this.ImportMethodImplementation( i, methodDef );
                }
            }
        }

        private MultiDictionary<MetadataToken, int> methodImplementationsIndex;

        private void ImportMethodImplementation( int i, MethodDefDeclaration methodDef )
        {
            MetadataRow row = this.tables.MethodImplTable.GetRow( i );

            MetadataToken tkImplementedMethod = this.tables.MethodImplMethodDefDeclarationColumn.GetValue( row );

            MethodImplementationDeclaration methodImpl = new MethodImplementationDeclaration
                                                             {
                                                                 MetadataToken =
                                                                     new MetadataToken( TokenType.MethodImpl, i ),
                                                                 ImplementedMethod =
                                                                     this.ResolveMethod( tkImplementedMethod )
                                                             };

            methodDef.InterfaceImplementations.Import( methodImpl );
        }

        private void ImportMethodImplementations()
        {
            for ( int i = 0; i < this.tables.MethodImplTable.RowCount; i++ )
            {
                MetadataRow row = this.tables.MethodImplTable.GetRow( i );
                MetadataToken tkMethodDef = this.tables.MethodImplMethodBodyColumn.GetValue( row );

                if ( this.lazyLoading )
                {
                    this.methodImplementationsIndex.Add( tkMethodDef, i );
                }
                else
                {
                    MethodDefDeclaration methodDef = this.GetMethodDef( tkMethodDef.Index );
                    this.ImportMethodImplementation( i, methodDef );
                }
            }
        }

        #endregion

        #region Generic parameters and constraints

        private MultiDictionary<MetadataToken, int> genericParametersIndex;
        private MultiDictionary<MetadataToken, int> genericParameterConstraintsIndex;

        internal void ImportGenericParameters( IGenericDefinitionDefinition owner )
        {
            if ( !this.lazyLoading )
            {
                owner.GenericParameters.PrepareImport( 0 );
                return;
            }

            if ( this.genericParametersIndex == null )
            {
                this.genericParametersIndex = new MultiDictionary<MetadataToken, int>( this.genericParameters.Length/2 );
                this.genericParameterConstraintsIndex = new MultiDictionary<MetadataToken, int>( 64 );
                this.ImportGenericParameters();
            }

            MetadataToken tk = owner.MetadataToken;

            int n = this.genericParametersIndex.GetCountByKey( tk );
            owner.GenericParameters.PrepareImport( n );
            GenericParameterKind kind = owner is MethodDefDeclaration
                                            ? GenericParameterKind.Method
                                            : GenericParameterKind.Type;
            if ( n > 0 )
            {
                foreach ( int i in this.genericParametersIndex[tk] )
                {
                    GenericParameterDeclaration genericParameter = this.genericParameters[i];

                    if ( genericParameter == null )
                        this.ImportGenericParameter( i, kind, owner );
                }
            }
        }

        internal void ImportGenericParameterConstraints( GenericParameterDeclaration genericParameter )
        {
            if ( !this.lazyLoading )
            {
                genericParameter.Constraints.PrepareImport( 0 );
                return;
            }

            if ( this.genericParametersIndex == null )
            {
                this.genericParametersIndex = new MultiDictionary<MetadataToken, int>( this.genericParameters.Length/2 );
                this.genericParameterConstraintsIndex = new MultiDictionary<MetadataToken, int>( 64 );
                this.ImportGenericParameters();
            }

            MetadataToken tk = genericParameter.MetadataToken;

            int n = this.genericParametersIndex.GetCountByKey( tk );
            genericParameter.Constraints.PrepareImport( n );
            if ( n > 0 )
            {
                foreach ( int i in this.genericParameterConstraintsIndex[tk] )
                {
                    this.ImportGenericParameterConstraint( genericParameter, i );
                }
            }
        }


        private void ImportGenericParameter( int i, MetadataToken tkOwner )
        {
            GenericParameterKind kind;
            IGenericDefinitionDefinition owner;

            switch ( tkOwner.TokenType )
            {
                case TokenType.TypeDef:
                    kind = GenericParameterKind.Type;
                    owner = GetTypeDef( tkOwner.Index );
                    break;

                case TokenType.MethodDef:
                    kind = GenericParameterKind.Method;
                    owner = GetMethodDef( tkOwner.Index );
                    break;

                default:
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "UnexpectedTokenType",
                                                                               tkOwner.TokenType,
                                                                               "GenericParamOwnerColumn",
                                                                               "TypeDef, MethodDef" );
            }

            this.ImportGenericParameter( i, kind, owner );
        }

        private void ImportGenericParameter( int i, GenericParameterKind kind,
                                                                    IGenericDefinitionDefinition owner )
        {
            MetadataRow row = this.tables.GenericParamTable.GetRow( i );

            GenericParameterDeclaration parameter = new GenericParameterDeclaration
                                                        {
                                                            MetadataToken =
                                                                new MetadataToken( TokenType.GenericParam, i ),
                                                            Name = this.tables.GenericParamNameColumn.GetValue( row ),
                                                            Attributes = ( (GenericParameterAttributes)
                                                                           this.tables.GenericParamFlagsColumn.GetValue(
                                                                               row ) ),
                                                            Ordinal =
                                                                this.tables.GenericParamNumberColumn.GetValue( row ),
                                                            Kind = kind
                                                        };


            owner.GenericParameters.Import( parameter );
            this.genericParameters[i] = parameter;
        }

        private void ImportGenericParameterConstraint( GenericParameterDeclaration genericParameter, int i )
        {
            MetadataRow row = this.tables.GenericParamConstraintTable.GetRow( i );
            ITypeSignature constraint =
                this.ResolveType( this.tables.GenericParamConstraintConstraintColumn.GetValue( row ) );
            genericParameter.Constraints.Import( new GenericParameterConstraintDeclaration
                                                     {
                                                         MetadataToken =
                                                             new MetadataToken( TokenType.GenericParamConstraint, i ),
                                                         ConstraintType = constraint
                                                     } );
        }

        /// <summary>
        /// Imports all generic parameters and their constraints.
        /// </summary>
        private void ImportGenericParameters()
        {
            for ( int i = 0; i < this.genericParameters.Length; i++ )
            {
                MetadataRow row = this.tables.GenericParamTable.GetRow( i );
                MetadataToken tkOwner = this.tables.GenericParamOwnerColumn.GetValue( row );

                if ( this.lazyLoading )
                {
                    this.genericParametersIndex.Add( tkOwner, i );
                }
                else
                {
                    this.ImportGenericParameter( i, tkOwner );
                }
            }

            for ( int i = 0; i < this.tables.GenericParamConstraintTable.RowCount; i++ )
            {
                MetadataRow row = this.tables.GenericParamConstraintTable.GetRow( i );
                MetadataToken tkOwner = this.tables.GenericParamConstraintOwnerColumn.GetValue( row );

                if ( this.lazyLoading )
                {
                    this.genericParameterConstraintsIndex.Add( tkOwner, i );
                }
                else
                {
                    this.ImportGenericParameterConstraint( this.genericParameters[tkOwner.Index], i );
                }
            }
        }

        #endregion

        #region Constants

        private void ImportConstants()
        {
            for ( int i = 0; i < this.tables.ConstantTable.RowCount; i++ )
            {
                MetadataRow row = this.tables.ConstantTable.GetRow( i );
                MetadataToken tkParent = this.tables.ConstantParentColumn.GetValue( row );

                CorElementType type = (CorElementType) this.tables.ConstantTypeColumn.GetValue( row );
                BufferReader buffer = this.tables.ConstantValueColumn.GetValueBufferReader( row );
                SerializedValue value;
                if ( buffer == null )
                {
                    switch ( type )
                    {
                        case CorElementType.String:
                            value = IntrinsicSerializationType.CreateValue( this.moduleDeclaration, "" );
                            break;

                        default:
                            throw ExceptionHelper.Core.CreateAssertionFailedException( "NullConstantValue",
                                                                                       type );
                    }
                }
                else
                {
                    value = ReadConstant( type, buffer );
                }

                switch ( tkParent.TokenType )
                {
                    case TokenType.FieldDef:
                        this.fields[tkParent.Index].LiteralValue = value;
                        break;

                    case TokenType.ParamDef:
                        this.parameters[tkParent.Index].DefaultValue = value;
                        break;

                    case TokenType.Property:
                        this.properties[tkParent.Index].DefaultValue = value;
                        break;

                    default:
                        throw ExceptionHelper.Core.CreateAssertionFailedException( "UnexpectedTokenType",
                                                                                   tkParent.TokenType,
                                                                                   "ConstantParentColumn",
                                                                                   "FieldDef, ParamDef, Property" );
                }
            }
        }

        #endregion

        #region Field definitions

        internal FieldDefDeclaration GetFieldDef( int i )
        {
            if ( this.methodDefsIndex == null )
            {
                this.methodDefsIndex = new int[this.methods.Length];
                this.fieldDefsIndex = new int[this.fields.Length];
                this.IndexTypeDefMembers();
            }

            return this.fields[i] ?? this.ImportFieldDef( i, GetTypeDef( this.fieldDefsIndex[i] ) );
        }

        private FieldDefDeclaration ImportFieldDef( int i, TypeDefDeclaration typeDef )
        {
            MetadataRow row = this.tables.FieldTable.GetRow( i );

            FieldDefDeclaration fieldDef = new FieldDefDeclaration
                                               {
                                                   MetadataToken = new MetadataToken( TokenType.FieldDef, i ),
                                                   Name = this.tables.FieldNameColumn.GetValue( row ),
                                                   Attributes =
                                                       ( (FieldAttributes)
                                                         this.tables.FieldFlagsColumn.GetValue( row ) )
                                               };
            this.fields[i] = fieldDef;

            BufferReader buffer = this.tables.FieldSignatureColumn.GetValueBufferReader( row );

            // Calling conventions
            CallingConvention callingConvention = (CallingConvention) buffer.ReadByte();
            if ( callingConvention != CallingConvention.Field )
            {
                throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidToken", callingConvention,
                                                                           "field calling convention" );
            }

            // Type
            fieldDef.FieldType = this.ReadTypeSignature( buffer );
            typeDef.Fields.Import( fieldDef );

            return fieldDef;
        }

        internal void ImportFieldDefs( TypeDefDeclaration parent )
        {
            if ( !this.lazyLoading )
            {
                parent.Fields.PrepareImport( 0 );
                return;
            }

            MetadataToken tkFirst, tkLast;
            MetadataRow row = this.tables.TypeDefTable.GetRow( parent.MetadataToken );
            this.tables.TypeDefFieldListColumn.GetRange( row, out tkFirst, out tkLast );

            this.ImportFieldDefs( tkFirst, tkLast, parent );
        }


        private int[] fieldDefsIndex;

        private void IndexFieldDefs( MetadataToken tkFirst, MetadataToken tkLast, int iParent )
        {
            for ( MetadataToken i = tkFirst; i.IsSmallerOrEqualThan( tkLast ); i.Increment() )
            {
                this.fieldDefsIndex[i.Index] = iParent;
            }
        }

        private void ImportFieldDefs( MetadataToken tkFirst, MetadataToken tkLast, TypeDefDeclaration parent )
        {
            if ( !tkFirst.IsNull && !tkLast.IsNull )
            {
                parent.Fields.PrepareImport( 1 + tkLast.Index - tkFirst.Index );

                for ( int i = tkFirst.Index; i <= tkLast.Index; i++ )
                {
                    if ( this.fields[i] == null )
                    {
                        this.ImportFieldDef( i, parent );
                    }
                }
            }
            else
            {
                parent.Fields.PrepareImport( 0 );
            }
        }

        #endregion

        #region Method definitions

        internal MethodDefDeclaration GetMethodDef( int i )
        {
            if ( this.methodDefsIndex == null )
            {
                this.methodDefsIndex = new int[this.methods.Length];
                this.fieldDefsIndex = new int[this.fields.Length];
                this.IndexTypeDefMembers();
            }

            return this.methods[i] ?? this.ImportMethodDef( i, GetTypeDef( this.methodDefsIndex[i] ) );
        }

        private int[] methodDefsIndex;

        private MethodDefDeclaration ImportMethodDef( int i, TypeDefDeclaration parent )
        {
            MetadataRow row = this.tables.MethodTable.GetRow( i );
            MethodDefDeclaration methodDef = new MethodDefDeclaration
                                                 {
                                                     MetadataToken = new MetadataToken( TokenType.MethodDef, i ),
                                                     Name = this.tables.MethodNameColumn.GetValue( row ),
                                                     Attributes =
                                                         ( (MethodAttributes)
                                                           this.tables.MethodFlagsColumn.GetValue( row ) ),
                                                     ImplementationAttributes =
                                                         ( (MethodImplAttributes)
                                                           this.tables.MethodImplFlagsColumn.GetValue( row ) ),
                                                     RVA = ( (uint) this.tables.MethodRVAColumn.GetValue( row ) )
                                                 };
            parent.Methods.Import( methodDef );
            this.methods[i] = methodDef;


            //
            // Parse the method signature so we have at least parametersCount.
            //
            BufferReader buffer = this.tables.MethodSignatureColumn.GetValueBufferReader( row );

            // Read calling convention.
            CallingConvention callingConvention = (CallingConvention) buffer.ReadByte();
            methodDef.CallingConvention = callingConvention;

            if ( ( callingConvention & CallingConvention.Generic ) != 0 )
            {
                uint genericArguments = buffer.ReadByte();
                methodDef.GenericParameters.EnsureCapacity( (int) genericArguments );
            }

            // Reading the number of parameters.
            uint parametersCount = buffer.ReadCompressedInteger();

            //
            // Read custom attributes on parameters, which allows to get the preexisting
            // instances of ParameterDeclaration.
            //
            MetadataToken firstParam, lastParam;
            this.tables.MethodParamListColumn.GetRange( row, out firstParam, out lastParam );

            ParameterDeclaration[] ourParameters = new ParameterDeclaration[parametersCount];
            ParameterDeclaration returnParameter = null;

            if ( !firstParam.IsNull )
            {
                for ( MetadataToken j = firstParam; j.IsSmallerOrEqualThan( lastParam ); j.Increment() )
                {
                    MetadataRow paramRow = this.tables.ParamTable.GetRow( j );

                    int sequence = this.tables.ParamSequenceColumn.GetValue( paramRow );

                    ParameterDeclaration parameter = new ParameterDeclaration
                                                         {
                                                             MetadataToken = j,
                                                             Ordinal = ( sequence - 1 ),
                                                             Name = this.tables.ParamNameColumn.GetValue( paramRow ),
                                                             Attributes =
                                                                 (ParameterAttributes)
                                                                 this.tables.ParamFlagsColumn.GetValue( paramRow )
                                                         };

                    this.parameters[j.Index] = parameter;

                    if ( sequence == 0 )
                        returnParameter = parameter;
                    else
                        ourParameters[sequence - 1] = parameter;
                }
            }


            //
            // Continue to read the signature. Create missing parameters.
            //

            // Reading the return type.
            if ( returnParameter == null )
            {
                returnParameter = new ParameterDeclaration {Ordinal = ( -1 )};
            }

            returnParameter.ParameterType = this.ReadTypeSignature( buffer );
            returnParameter.Attributes |= ParameterAttributes.Retval;
            methodDef.ReturnParameter = returnParameter;


            if ( parametersCount > 0 )
            {
                // Allocate array of parameters.
                methodDef.Parameters.PrepareImport( (int) parametersCount );


                // Read parameter types from the signatures.
                for ( int j = 0; j < parametersCount; j++ )
                {
                    ParameterDeclaration parameter = ourParameters[j];

                    if ( parameter == null )
                    {
                        parameter = new ParameterDeclaration
                                        {
                                            Attributes = ParameterAttributes.None,
                                            Ordinal = j
                                        };
                    }

                    parameter.ParameterType = this.ReadTypeSignature( buffer );

                    methodDef.Parameters.Import( parameter );
                }
            }

            return methodDef;
        }

        private void IndexMethodDefs( MetadataToken tkFirst, MetadataToken tkLast, int iParent )
        {
            for ( MetadataToken i = tkFirst; i.IsSmallerOrEqualThan( tkLast ); i.Increment() )
            {
                this.methodDefsIndex[i.Index] = iParent;
            }
        }

        private void ImportMethodDefs( MetadataToken tkFirst, MetadataToken tkLast, TypeDefDeclaration parent )
        {
            if ( !tkFirst.IsNull && !tkLast.IsNull )
            {
                parent.Methods.PrepareImport( 1 + tkLast.Index - tkFirst.Index );

                for ( int i = tkFirst.Index; i <= tkLast.Index; i++ )
                {
                    if ( this.methods[i] == null )
                    {
                        this.ImportMethodDef( i, parent );
                    }
                }
            }
            else
            {
                parent.Methods.PrepareImport( 0 );
            }
        }

        internal void ImportMethodDefs( TypeDefDeclaration parent )
        {
            if ( !this.lazyLoading )
            {
                parent.Methods.PrepareImport( 0 );
                return;
            }

            MetadataToken tk = parent.MetadataToken;
            MetadataRow row = this.tables.TypeDefTable.GetRow( tk.Index );

            MetadataToken tkFirstMethod, tkLastMethod;
            this.tables.TypeDefMethodListColumn.GetRange( row, out tkFirstMethod, out tkLastMethod );

            this.ImportMethodDefs( tkFirstMethod, tkLastMethod, parent );
        }

        #endregion

        #region Type specifications

        internal TypeSpecDeclaration GetTypeSpec( int i )
        {
            return this.typeSpecs[i] ?? this.ImportTypeSpec( i );
        }

        private TypeSpecDeclaration ImportTypeSpec( int i )
        {
            MetadataRow row = this.tables.TypeSpecTable.GetRow( i );
            TypeSpecDeclaration typeSpec = new TypeSpecDeclaration
                                               {
                                                   MetadataToken = new MetadataToken( TokenType.TypeSpec, i ),
                                                   Signature =
                                                       this.ReadTypeSignature(
                                                       this.tables.TypeSpecSignatureColumn.GetValueBufferReader( row ) )
                                               };

            this.moduleDeclaration.TypeSpecs.Import( typeSpec );
            this.typeSpecs[i] = typeSpec;
            return typeSpec;
        }

        internal void ImportTypeSpecs()
        {
            this.moduleDeclaration.TypeSpecs.PrepareImport( this.typeSpecs.Length );

            // Read all signatures.
            for ( int i = 0; i < this.typeSpecs.Length; i++ )
            {
                if ( this.typeSpecs[i] == null )
                {
                    this.ImportTypeSpec( i );
                }
            }
        }

        #endregion

        #region Method specifications

        private MultiDictionary<MetadataToken, int> methodSpecsIndex;
        private MetadataToken[] methodSpecParentIndex;

        internal MethodSpecDeclaration GetMethodSpec( int i )
        {
            MethodSpecDeclaration methodSpec = this.methodSpecs[i];

            if ( methodSpec == null )
            {
                if ( this.methodSpecParentIndex == null )
                {
                    this.methodSpecsIndex = new MultiDictionary<MetadataToken, int>( 64 );
                    this.methodSpecParentIndex = new MetadataToken[this.methodSpecs.Length];
                    this.ImportMethodSpecs();
                }

                return this.ImportMethodSpec( i,
                                              (IGenericMethodDefinition)
                                              this.ResolveMethod( this.methodSpecParentIndex[i] ) );
            }

            return methodSpec;
        }

        private MethodSpecDeclaration ImportMethodSpec( int i, IGenericMethodDefinition parent )
        {
            MetadataRow row = this.tables.MethodSpecTable.GetRow( i );
            BufferReader buffer = this.tables.MethodSpecInstantiationColumn.GetValueBufferReader( row );

            MethodSpecDeclaration methodSpec = new MethodSpecDeclaration
                                                   {
                                                       MetadataToken = new MetadataToken( TokenType.MethodSpec, i ),
                                                       CallingConvention = ( (CallingConvention) buffer.ReadByte() )
                                                   };


            uint count = buffer.ReadCompressedInteger();
            methodSpec.GenericArguments.Capacity = (int) count;

            for ( int j = 0; j < count; j++ )
            {
                methodSpec.GenericArguments.Add( this.ReadTypeSignature( buffer ) );
            }

            parent.MethodSpecs.Add( methodSpec );

            this.methodSpecs[i] = methodSpec;
            return methodSpec;
        }

        internal void ImportMethodSpecs( IGenericMethodDefinition parent )
        {
            if ( !this.lazyLoading )
            {
                parent.MethodSpecs.PrepareImport( 0 );
                return;
            }

            if ( this.methodSpecParentIndex == null )
            {
                this.methodSpecsIndex = new MultiDictionary<MetadataToken, int>( 64 );
                this.methodSpecParentIndex = new MetadataToken[this.methodSpecs.Length];
                this.ImportMethodSpecs();
            }

            MetadataToken tk = parent.MetadataToken;
            int n = this.methodSpecsIndex.GetCountByKey( tk );
            parent.MethodSpecs.PrepareImport( n );
            if ( n > 0 )
            {
                foreach ( int i in this.methodSpecsIndex[tk] )
                {
                    this.ImportMethodSpec( i, parent );
                }
            }
        }

        /// <summary>
        /// Imports all method specifications.
        /// </summary>
        private void ImportMethodSpecs()
        {
            for ( int i = 0; i < this.methodSpecs.Length; i++ )
            {
                MetadataRow row = this.tables.MethodSpecTable.GetRow( i );
                MetadataToken tkParent = this.tables.MethodSpecMethodColumn.GetValue( row );

                if ( this.lazyLoading )
                {
                    this.methodSpecsIndex.Add( tkParent, i );
                    this.methodSpecParentIndex[i] = tkParent;
                }
                else
                {
                    IGenericMethodDefinition parent = (IGenericMethodDefinition) this.ResolveMethod( tkParent );
                    this.ImportMethodSpec( i, parent );
                }
            }
        }

        #endregion

        #region Standalone signatures

        internal StandaloneSignatureDeclaration GetStandaloneSignature( int i )
        {
            return this.standaloneSignatures[i] ?? ImportStandaloneSignature( i );
        }
                
        private StandaloneSignatureDeclaration ImportStandaloneSignature( int i )
        {
            MetadataRow row = this.tables.StandAloneSigTable.GetRow( i );
            StandaloneSignatureDeclaration signature = new StandaloneSignatureDeclaration
                                                           {MetadataToken = new MetadataToken( TokenType.Signature, i )};
            this.standaloneSignatures[i] = signature;
            this.moduleDeclaration.StandaloneSignatures.Import(signature);

            BufferReader buffer = this.tables.StandAloneSigSignatureColumn.GetValueBufferReader( row );

            CallingConvention signatureKind = (CallingConvention) buffer.ReadByte();

            switch ( signatureKind )
            {
                case CallingConvention.Field:
                    signature.SetTypeSignature( this.ReadTypeSignature( buffer ),
                                                StandaloneSignatureKind.FieldSignature );
                    break;

                case CallingConvention.Property:
                    signature.SetTypeSignature( this.ReadTypeSignature( buffer ),
                                                StandaloneSignatureKind.PropertySignature );
                    break;


                case CallingConvention.LocalSig:
                    {
                        uint count = buffer.ReadCompressedInteger();
                        LocalVariableDeclarationCollection localVariables = signature.SetLocalVariables();
                        localVariables.EnsureCapacity( (int) count );

                        for ( int j = 0; j < count; j++ )
                        {
                            ITypeSignature type = this.ReadTypeSignature( buffer );
                            localVariables.Add( new LocalVariableDeclaration( type, j ) );
                        }

                        break;
                    }

                default:
                    buffer.Offset -= sizeof(byte);
                    signature.SetMethodSignature( this.ReadMethodSignature( buffer ) );
                    break;
            }



            return signature;


        }

        /// <summary>
        /// Import all standalone signatures.
        /// </summary>
        internal void ImportStandaloneSignatures()
        {
            this.moduleDeclaration.StandaloneSignatures.PrepareImport(this.tables.StandAloneSigTable.RowCount);
            for ( int i = 0; i < this.tables.StandAloneSigTable.RowCount; i++ )
            {
                if ( this.standaloneSignatures[i] == null )
                    this.ImportStandaloneSignature( i );
            }
        }

        #endregion

        #endregion

        #region Import Platform Invoke and Marshaling

        /// <summary>
        /// Imports all field and parameter marshalling.
        /// </summary>
        private void ImportFieldMarshals()
        {
            for ( int i = 0; i < this.tables.FieldMarshalTable.RowCount; i++ )
            {
                MetadataRow row = this.tables.FieldMarshalTable.GetRow( i );
                MetadataToken tkParent = this.tables.FieldMarshalParentColumn.GetValue( row );
                BufferReader reader = this.tables.FieldMarshalNativeTypeColumn.GetValueBufferReader( row );
                MarshalType marshalType = ReadMarshalType( reader );

                switch ( tkParent.TokenType )
                {
                    case TokenType.FieldDef:
                        this.fields[tkParent.Index].MarshalType = marshalType;
                        break;

                    case TokenType.ParamDef:
                        this.parameters[tkParent.Index].MarshalType = marshalType;
                        break;

                    default:
                        throw ExceptionHelper.Core.CreateAssertionFailedException( "UnexpectedTokenType",
                                                                                   tkParent.TokenType,
                                                                                   "FieldMarshalParentColumn",
                                                                                   "FieldDef, ParamDef" );
                }
            }
        }

        /// <summary>
        /// Imports all field RVAs.
        /// </summary>
        private void ImportFieldRVAs()
        {
            // TODO: Make it platform-independent
            PlatformInfo platform = new PlatformInfo( IntPtr.Size );

            this.moduleDeclaration.Datas.EnsureCapacity( this.tables.FieldRVATable.RowCount );

            for ( int i = 0; i < this.tables.FieldRVATable.RowCount; i++ )
            {
                MetadataRow row = this.tables.FieldRVATable.GetRow( i );
                FieldDefDeclaration fieldDeclaration =
                    this.fields[this.tables.FieldRVAFieldColumn.GetValue( row ).Index];
                uint rva = (uint) this.tables.FieldRVARVAColumn.GetValue( row );

                DataSectionDeclaration dataDeclaration;

                if ( !this.dataSections.TryGetValue( rva, out dataDeclaration ) )
                {
                    int size = fieldDeclaration.FieldType.GetValueSize( platform );

                    if ( size == 0 )
                    {
                        size = 1;
                    }
                    else if ( size < 0 )
                    {
                        // Retry to have a chance to debug.
                        fieldDeclaration.FieldType.GetValueSize( platform );
                        throw ExceptionHelper.Core.CreateAssertionFailedException(
                            "CannotDetermineTypeSize", fieldDeclaration.FieldType );
                    }

                    byte[] data = new byte[size];
                    Marshal.Copy( this.imageReader.RvaToIntPtr( rva ), data, 0, size );

                    // todo: determines whether the data is managed 

                    dataDeclaration = new DataSectionDeclaration(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "I_{0:X8}", (int) rva ), data, true ) {Ordinal = ( (int) rva )};
                    this.dataSections.Add( rva, dataDeclaration );
                    this.moduleDeclaration.Datas.Add( dataDeclaration );
                }

                fieldDeclaration.InitialValue = dataDeclaration;
            }
        }

        /// <summary>
        /// Imports all P-Invoke maps.
        /// </summary>
        private void ImportPInvokeMaps()
        {
            for ( int i = 0; i < this.tables.ImplMapTable.RowCount; i++ )
            {
                MetadataRow row = this.tables.ImplMapTable.GetRow( i );
                PInvokeMap map = new PInvokeMap
                                     {
                                         Attributes =
                                             ( (PInvokeAttributes) this.tables.ImplMapMappingFlagsColumn.GetValue( row ) )
                                     };
                string methodName = this.tables.ImplMapImportNameColumn.GetValue( row );
                MethodDefDeclaration method =
                    this.methods[this.tables.ImplMapMemberForwardedColumn.GetValue( row ).Index];

                // System.EnterpriseServices.dll has non-standard behavior here.
                if ( methodName == null )
                {
                    methodName = method.Name;
                }

                map.MethodName = methodName;
                map.Module = this.moduleRefs[this.tables.ImplMapImportScopeColumn.GetValue( row ).Index];
                method.PInvokeMap = map;
            }
        }

        /// <summary>
        /// Imports all field layouts.
        /// </summary>
        private void ImportFieldLayout()
        {
            for ( int i = 0; i < this.tables.FieldLayoutTable.RowCount; i++ )
            {
                MetadataRow row = this.tables.FieldLayoutTable.GetRow( i );

                FieldDefDeclaration field = this.fields[
                    this.tables.FieldLayoutFieldColumn.GetValue( row ).Index];
                field.Offset = this.tables.FieldLayoutOffSetColumn.GetValue( row );
            }
        }

        /// <summary>
        /// Imports all class layouts.
        /// </summary>
        private void ImportClassLayout()
        {
            for ( int i = 0; i < this.tables.ClassLayoutTable.RowCount; i++ )
            {
                MetadataRow row = this.tables.ClassLayoutTable.GetRow( i );

                TypeDefDeclaration typeDeclaration = this.typeDefs[
                    this.tables.ClassLayoutParentColumn.GetValue( row ).Index];
                typeDeclaration.ExplicitAlignment = this.tables.ClassLayoutPackingSizeColumn.GetValue( row );
                typeDeclaration.ExplicitTypeSize = this.tables.ClassLayoutClassSizeColumn.GetValue( row );
            }
        }

        #endregion

        #region Import manifest and references

        /// <summary>
        /// Imports the assemby manifest.
        /// </summary>
        private void ImportAssemblyManifest()
        {
            if ( this.tables.AssemblyTable.RowCount > 0 )
            {
                MetadataRow row = this.tables.AssemblyTable.GetRow( 0 );

                AssemblyManifestDeclaration assembly =
                    new AssemblyManifestDeclaration
                        {
                            MetadataToken = new MetadataToken( TokenType.Assembly, 0 ),
                            Culture = this.tables.AssemblyLocaleColumn.GetValue( row ),
                            HashAlgorithm =
                                ( (AssemblyHashAlgorithm) this.tables.AssemblyHashAlgIdColumn.GetValue( row ) ),
                            Name = this.tables.AssemblyNameColumn.GetValue( row ),
                            Version = new Version( (ushort) this.tables.AssemblyMajorVersionColumn.GetValue( row ),
                                                   (ushort) this.tables.AssemblyMinorVersionColumn.GetValue( row ),
                                                   (ushort) this.tables.AssemblyBuildNumberColumn.GetValue( row ),
                                                   (ushort) this.tables.AssemblyRevisionNumberColumn.GetValue( row ) )
                        };

                assembly.SetPublicKey( this.tables.AssemblyPublicKeyColumn.GetValueByteArray( row ) );


                this.moduleDeclaration.AssemblyManifest = assembly;

                this.assemblyManifest = assembly;
            }
            else
            {
                this.assemblyManifest = null;
            }
        }

        #region Manifest files

        private ManifestFileDeclaration ImportManifestFile( int i )
        {
            MetadataRow row = this.tables.FileTable.GetRow( i );
            ManifestFileDeclaration file = new ManifestFileDeclaration
                                               {
                                                   MetadataToken = new MetadataToken( TokenType.File, i ),
                                                   Name = this.tables.FileNameColumn.GetValue( row ),
                                                   Hash = this.tables.FileHashValueColumn.GetValueByteArray( row ),
                                                   HasMetadata = this.tables.FileFlagsColumn.GetValue( row ) == 0
                                               };
            this.manifestFiles[i] = file;
            this.moduleDeclaration.AssemblyManifest.Files.Import( file );

            return file;
        }

        internal ManifestFileDeclaration GetManifestFile( int i )
        {
            return this.manifestFiles[i] ?? this.ImportManifestFile( i );
        }

        /// <summary>
        /// Imports the manifest files.
        /// </summary>
        internal void ImportManifestFiles()
        {
            this.moduleDeclaration.AssemblyManifest.Files.PrepareImport( this.manifestFiles.Length );
            for ( int i = 0; i < this.manifestFiles.Length; i++ )
            {
                if ( this.manifestFiles[i] == null )
                {
                    this.ImportManifestFile( i );
                }
            }
        }

        #endregion

        #region Manifest resources

        private ManifestResourceDeclaration GetManifestResource( int i )
        {
            return this.manifestResources[i] ?? ImportManifestResource( i );
        }

        private ManifestResourceDeclaration ImportManifestResource( int i )
        {
            MetadataRow row = this.tables.ManifestResourceTable.GetRow( i );
            ManifestResourceDeclaration resource = new ManifestResourceDeclaration
                                                       {
                                                           MetadataToken =
                                                               new MetadataToken( TokenType.ManifestResource, i ),
                                                           Implementation =
                                                               this.ResolveManifestResourceImplementation(
                                                               this.tables.ManifestResourceImplementationColumn.
                                                                   GetValue( row ) ),
                                                           FileOffset =
                                                               this.tables.ManifestResourceOffsetColumn.GetValue(
                                                               row ),
                                                           IsPublic =
                                                               ( this.tables.ManifestResourceFlagsColumn.GetValue(
                                                                     row ) == 0x0001 ),
                                                           Name =
                                                               this.tables.ManifestResourceNameColumn.GetValue( row )
                                                       };
            if ( this.module != null )
            {
                resource.ContentStream = this.module.Assembly.GetManifestResourceStream( resource.Name );
            }
            else
            {
                UnmanagedBuffer buffer = this.imageReader.GetManagedResource( resource.FileOffset );
                unsafe
                {
                    resource.ContentStream = new UnmanagedMemoryStream( (byte*) buffer.Origin, buffer.Size,
                                                                        buffer.Size,
                                                                        FileAccess.Read );
                }
            }

            this.manifestResources[i] = resource;
            this.moduleDeclaration.AssemblyManifest.Resources.Import( resource );

            return resource;
        }

        /// <summary>
        /// Imports the manifest resources.
        /// </summary>
        internal void ImportManifestResources()
        {
            this.moduleDeclaration.AssemblyManifest.Resources.PrepareImport( this.tables.ManifestResourceTable.RowCount );
            for ( int i = 0; i < this.tables.ManifestResourceTable.RowCount; i++ )
            {
                if ( this.manifestResources[i] == null )
                {
                    this.ImportManifestResource( i );
                }
            }
        }

        #endregion

        #region Exported types

        private ExportedTypeDeclaration ImportExportedType( int i )
        {
            MetadataRow row = this.tables.ExportedTypeTable.GetRow( i );
            ExportedTypeDeclaration exportedType = new ExportedTypeDeclaration
                                                       {
                                                           Implementation = this.ResolveManifestResourceImplementation(
                                                               this.tables.ExportedTypeImplementationColumn.GetValue(
                                                                   row ) ),
                                                           Attributes =
                                                               (TypeAttributes)
                                                               this.tables.ExportedTypeFlagsColumn.GetValue( row ),
                                                           TypeDefId =
                                                               this.tables.ExportedTypeTypeDefIdColumn.GetValue( row ),
                                                           Name =
                                                               MakeTypeName(
                                                               this.tables.ExportedTypeTypeNamespaceColumn.GetValue( row ),
                                                               this.tables.ExportedTypeTypeNameColumn.GetValue( row ) ),
                                                           MetadataToken = new MetadataToken(TokenType.ExportedType, i)
                                                       };

            this.moduleDeclaration.AssemblyManifest.ExportedTypes.Import( exportedType );
            this.exportedTypes[i] = exportedType;
            return exportedType;
        }

        internal void ImportExportedTypes()
        {
            this.moduleDeclaration.AssemblyManifest.ExportedTypes.PrepareImport( this.tables.ExportedTypeTable.RowCount );
            for ( int i = 0; i < this.tables.ExportedTypeTable.RowCount; i++ )
            {
                if ( this.exportedTypes[i] != null )
                {
                    this.ImportExportedType( i );
                }
            }
        }

        internal ExportedTypeDeclaration GetExportedType( int i )
        {
            return this.exportedTypes[i] ?? this.ImportExportedType( i );
        }

        #endregion

        #region Assembly references

        private readonly Dictionary<string, int> assemblyRefAliases = new Dictionary<string, int>();

        internal AssemblyRefDeclaration GetAssemblyRef( int i )
        {
            return this.assemblyRefs[i] ?? ImportAssemblyRef( i );
        }

        private AssemblyRefDeclaration ImportAssemblyRef( int i )
        {
            MetadataRow row = this.tables.AssemblyRefTable.GetRow( i );

            AssemblyRefDeclaration assemblyRef = new AssemblyRefDeclaration
                                                     {
                                                         MetadataToken = new MetadataToken( TokenType.AssemblyRef, i ),
                                                         Version = new Version(
                                                             (ushort)
                                                             this.tables.AssemblyRefMajorVersionColumn.GetValue( row ),
                                                             (ushort)
                                                             this.tables.AssemblyRefMinorVersionColumn.GetValue( row ),
                                                             (ushort)
                                                             this.tables.AssemblyRefBuildNumberColumn.GetValue( row ),
                                                             (ushort)
                                                             this.tables.AssemblyRefRevisionNumberColumn.GetValue( row ) ),
                                                         Name = this.tables.AssemblyRefNameColumn.GetValue( row ),
                                                         Culture = this.tables.AssemblyRefLocaleColumn.GetValue( row ),
                                                         HashValue =
                                                             this.tables.AssemblyRefHashValueColumn.GetValueByteArray(
                                                             row ),
                                                         Attributes =
                                                             (AssemblyRefAttributes)
                                                             this.tables.AssemblyRefFlagsColumn.GetValue( row )
                                                     };

            if ( ( assemblyRef.Attributes & AssemblyRefAttributes.PublicKey ) != 0 )
            {
                assemblyRef.SetPublicKey( this.tables.AssemblyRefPublicKeyOrTokenColumn.GetValueByteArray( row ) );
            }
            else
            {
                assemblyRef.SetPublicKeyToken( this.tables.AssemblyRefPublicKeyOrTokenColumn.GetValueByteArray( row ) );
            }


            if ( assemblyRefAliases.ContainsKey( assemblyRef.Name ) )
            {
                assemblyRef.Alias = assemblyRef.Name + "_" + i.ToString( CultureInfo.InvariantCulture );
            }
            else
            {
                assemblyRefAliases.Add( assemblyRef.Name, i );
            }

            this.moduleDeclaration.AssemblyRefs.Import( assemblyRef );
            this.assemblyRefs[i] = assemblyRef;
            return assemblyRef;
        }

        internal void ImportAssemblyRefs()
        {
            this.moduleDeclaration.AssemblyRefs.PrepareImport( this.assemblyRefs.Length );
            for ( int i = 0; i < this.assemblyRefs.Length; i++ )
            {
                if ( this.assemblyRefs[i] == null )
                {
                    this.ImportAssemblyRef( i );
                }
            }
        }

        #endregion

        #region Module references

        internal ModuleRefDeclaration GetModuleRef( int i )
        {
            return this.moduleRefs[i] ?? this.ImportModuleRef( i );
        }

        private ModuleRefDeclaration ImportModuleRef( int i )
        {
            MetadataRow row = this.tables.ModuleRefTable.GetRow( i );
            ModuleRefDeclaration moduleRef = new ModuleRefDeclaration();
            string name = this.tables.ModuleRefNameColumn.GetValue( row );

            if ( name == null )
            {
                name = string.Format( CultureInfo.InvariantCulture, "$MOR${0}", i + 1 );
            }

            moduleRef.Name = name;
            this.moduleDeclaration.ModuleRefs.Import( moduleRef );
            this.moduleRefs[i] = moduleRef;

            return moduleRef;
        }

        /// <summary>
        /// Imports the module references.
        /// </summary>
        internal void ImportModuleRefs()
        {
            this.moduleDeclaration.ModuleRefs.PrepareImport( this.moduleRefs.Length );
            for ( int i = 0; i < this.moduleRefs.Length; i++ )
            {
                if ( this.moduleRefs[i] == null )
                {
                    this.ImportModuleRef( i );
                }
            }
        }

        #endregion

        #region Type references

        private TypeRefDeclaration GetTypeRef( int i )
        {
            return this.typeRefs[i] ?? this.ImportTypeRef( i, MetadataToken.Null );
        }

        internal void ImportTypeRefs( ITypeRefResolutionScope scope )
        {
            // If the module was not loaded lazily, the collection is already ready.
            if ( !this.lazyLoading )
                scope.TypeRefs.PrepareImport( 0 );

            // Check if we have to first index type references.
            if ( this.typeRefsIndex == null )
            {
                this.typeRefsIndex = new MultiDictionary<MetadataToken, int>( 64 );
                this.ImportTypeRefs();
            }

            MetadataToken tkScope = scope.MetadataToken;
            int n = this.typeRefsIndex.GetCountByKey( tkScope );
            scope.TypeRefs.PrepareImport( n );
            if ( n > 0 )
            {
                foreach ( int i in typeRefsIndex[tkScope] )
                {
                    if ( this.typeRefs[i] == null )
                        this.ImportTypeRef( i, tkScope );
                }
            }
        }

        private TypeRefDeclaration ImportTypeRef( int i, MetadataToken tkScope )
        {
            MetadataRow row = this.tables.TypeRefTable.GetRow( i );

            if ( tkScope.IsNull )
                tkScope = this.tables.TypeRefResolutionScopeColumn.GetValue( row );

            ITypeRefResolutionScope scope;
            if (!tkScope.IsNull)
            {
                switch ( tkScope.TokenType )
                {
                    case TokenType.TypeRef:
                        scope = this.GetTypeRef( tkScope.Index );
                        break;

                    case TokenType.ModuleRef:
                        scope = this.GetModuleRef( tkScope.Index );
                        break;

                    case TokenType.Module:
                        scope = this.moduleDeclaration;
                        break;

                    case TokenType.AssemblyRef:
                        scope = this.GetAssemblyRef( tkScope.Index );
                        break;

                    default:
                        throw ExceptionHelper.Core.CreateAssertionFailedException( "UnexpectedTokenType",
                                                                                   tkScope.TokenType,
                                                                                   "TypeRefResolutionScopeColumn",
                                                                                   "TypeRef, ModuleRef, Module, AssemblyRef." );
                }
            }
            else
            {
                scope = this.moduleDeclaration;
            }

            return this.ImportTypeRef( i, scope );
        }

        private TypeRefDeclaration ImportTypeRef( int i, ITypeRefResolutionScope scope )
        {
            MetadataRow row = this.tables.TypeRefTable.GetRow( i );

            string typeName = MakeTypeName(
                this.tables.TypeRefNamespaceColumn.GetValue( row ),
                this.tables.TypeRefNameColumn.GetValue( row ) );

            // There may be a compiler bug. Check duplicates and use an existing instance
            // if any.
            TypeRefDeclaration typeRef = scope.TypeRefs.GetByName( typeName );
            if (typeRef == null)
            {
                typeRef = new TypeRefDeclaration
                    {
                        Name = typeName,
                        MetadataToken = new MetadataToken( TokenType.TypeRef, i )
                    };
                scope.TypeRefs.Import( typeRef );
            }

            this.typeRefs[i] = typeRef;
            return typeRef;
        }

        private MultiDictionary<MetadataToken, int> typeRefsIndex;

        private void ImportTypeRefs()
        {
            for ( int i = 0; i < this.typeRefs.Length; i++ )
            {
                MetadataRow row = this.tables.TypeRefTable.GetRow( i );
                MetadataToken tkScope = this.tables.TypeRefResolutionScopeColumn.GetValue( row );

                if ( tkScope.IsNull )
                {
                    tkScope = this.moduleDeclaration.MetadataToken;
                }

                if ( this.lazyLoading )
                {
                    this.typeRefsIndex.Add( tkScope, i );
                }
                else if ( this.typeRefs[i] == null )
                {
                    this.ImportTypeRef( i, tkScope );
                }
            }
        }

        #endregion

        #region Member references

        internal MemberRefDeclaration GetMemberRef( int i )
        {
            MemberRefDeclaration memberRef = this.memberRefs[i];

            if ( memberRef != null ) return memberRef;

            if ( this.fieldRefIndex == null )
            {
                this.fieldRefIndex = new MultiDictionary<MetadataToken, int>( 64 );
                this.methodRefIndex = new MultiDictionary<MetadataToken, int>( 64 );
                this.ImportMemberRefs();
            }

            return this.ImportMemberRef( i );
        }

        private MemberRefDeclaration ImportMemberRef( int i )
        {
            MetadataRow row = this.tables.MemberRefTable.GetRow( i );
            MetadataToken tkScope = this.tables.MemberRefClassColumn.GetValue( row );
            return this.ImportMemberRef( i, tkScope );
        }

        internal void ImportFieldRefs( IMemberRefResolutionScope scope )
        {
            this.ImportMemberRefs( scope, true );
        }

        internal void ImportMethodRefs( IMemberRefResolutionScope scope )
        {
            this.ImportMemberRefs( scope, false );
        }

        private void ImportMemberRefs( IMemberRefResolutionScope scope, bool fields )
        {
            if ( !this.lazyLoading )
            {
                scope.FieldRefs.PrepareImport( 0 );
                scope.MethodRefs.PrepareImport( 0 );
                return;
            }

            if ( this.fieldRefIndex == null )
            {
                this.fieldRefIndex = new MultiDictionary<MetadataToken, int>( 64 );
                this.methodRefIndex = new MultiDictionary<MetadataToken, int>( 64 );
                this.ImportMemberRefs();
            }

            MultiDictionary<MetadataToken, int> index = fields ? this.fieldRefIndex : this.methodRefIndex;

            MetadataToken tkScope = scope.MetadataToken;
            int n = index.GetCountByKey( tkScope );

            if ( fields )
            {
                scope.FieldRefs.PrepareImport( n );
            }
            else
            {
                scope.MethodRefs.PrepareImport( n );
            }

            if ( n > 0 )
            {
                foreach ( int i in index[tkScope] )
                {
                    this.ImportMemberRef( i, scope );
                }
            }
        }

        private MemberRefDeclaration ImportMemberRef( int i, IMemberRefResolutionScope scope )
        {
            MetadataToken token = new MetadataToken( TokenType.MemberRef, i );
            MetadataRow row = this.tables.MemberRefTable.GetRow( i );

            BufferReader signature = this.tables.MemberRefSignatureColumn.GetValueBufferReader( row );

            CallingConvention callingConvention = (CallingConvention) signature.ReadByte();
            signature.Offset -= sizeof(byte);

            if ( callingConvention == CallingConvention.Field )
            {
                this.memberRefs[i] = ImportFieldRef( token, row, scope, signature );
            }
            else
            {
                this.memberRefs[i] = ImportMethodRef( token, row, scope, signature );
            }

            return this.memberRefs[i];
        }

        /// <summary>
        /// Imports a specific field reference.
        /// </summary>
        /// <param name="token">FieldRef token.</param>
        /// <param name="row">A row of the MemberRef table.</param>
        /// <param name="declaringType">Declaring type.</param>
        /// <param name="buffer">Buffer of the signature.</param>
        /// <returns>The <see cref="FieldRefDeclaration"/>.</returns>
        private FieldRefDeclaration ImportFieldRef( MetadataToken token, MetadataRow row,
                                                    IMemberRefResolutionScope declaringType, BufferReader buffer )
        {
            CallingConvention callingConvention = (CallingConvention) buffer.ReadByte();
            if ( callingConvention != CallingConvention.Field )
            {
                throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidToken", callingConvention,
                                                                           "FieldRef calling convention" );
            }

            string fieldName = this.tables.MemberRefNameColumn.GetValue( row );

            // Check duplicates. There may be a compiler bug. If we find an existing instance, use it.
            FieldRefDeclaration fieldRef = declaringType.FieldRefs.GetByName( fieldName );
            if (fieldRef == null)
            {
                ITypeSignature typeSignature = this.ReadTypeSignature( buffer );
                fieldRef = new FieldRefDeclaration
                                    {
                                        Name = fieldName,
                                        FieldType = typeSignature,
                                        MetadataToken = token
                                    };
                declaringType.FieldRefs.Import(fieldRef);
            }

            return fieldRef;
        }

        /// <summary>
        /// Imports a specific methods reference.
        /// </summary>
        /// <param name="token">MethodRef token.</param>
        /// <param name="row">A row of the MemberRef table.</param>
        /// <param name="declaringType">Declaring type.</param>
        /// <param name="buffer">Buffer of the signature.</param>
        /// <returns>The <see cref="FieldRefDeclaration"/>.</returns>
        private MethodRefDeclaration ImportMethodRef( MetadataToken token, MetadataRow row,
                                                      IMemberRefResolutionScope declaringType, BufferReader buffer )
        {
            MethodRefDeclaration externalMethod = new MethodRefDeclaration
                                                      {
                                                          MetadataToken = token,
                                                          Name = this.tables.MemberRefNameColumn.GetValue( row ),
                                                          Signature = this.ReadMethodSignature( buffer )
                                                      };

            declaringType.MethodRefs.Import( externalMethod );

            return externalMethod;
        }

        private MemberRefDeclaration ImportMemberRef( int i, MetadataToken tkScope )
        {
            IMemberRefResolutionScope scope =
                this.ResolveMemberRefResolutionScope( tkScope );

            return this.ImportMemberRef( i, scope );
        }

        private MultiDictionary<MetadataToken, int> fieldRefIndex;
        private MultiDictionary<MetadataToken, int> methodRefIndex;


        private void ImportMemberRefs()
        {
            for ( int i = 0; i < this.memberRefs.Length; i++ )
            {
                MetadataRow row = this.tables.MemberRefTable.GetRow( i );
                MetadataToken tkScope = this.tables.MemberRefClassColumn.GetValue( row );

                if ( this.lazyLoading )
                {
                    // Determine whether it is a field or a method.
                    UnmanagedBuffer signature = this.tables.MemberRefSignatureColumn.GetValueUnmanagedBuffer( row );
                    CallingConvention callingConvention;
                    unsafe
                    {
                        callingConvention = (CallingConvention) ( *( (byte*) signature.Origin ) );
                    }

                    if ( callingConvention == CallingConvention.Field )
                    {
                        this.fieldRefIndex.Add( tkScope, i );
                    }
                    else
                    {
                        this.methodRefIndex.Add( tkScope, i );
                    }
                }
                else
                {
                    this.ImportMemberRef( i, tkScope );
                }
            }
        }

        #endregion

        #endregion

        #region Utilities

        /// <summary>
        /// Merge a namespace and a type name.
        /// </summary>
        /// <param name="ns">Namespace.</param>
        /// <param name="name">Type.</param>
        /// <returns>Full type name.</returns>
        private static string MakeTypeName( string ns, string name )
        {
            ExceptionHelper.AssertArgumentNotEmptyOrNull( name, "name" );

            if ( string.IsNullOrEmpty( ns ) )
            {
                return name;
            }
            else
            {
                return ns + "." + name;
            }
        }

        #endregion

        #region Read blobs

        /// <summary>
        /// Reads a <b>TypeDefOrRefEncoded</b> token.
        /// </summary>
        /// <param name="buffer">A <see cref="BufferReader"/>.</param>
        /// <param name="learnedClassifications">Type classifications learned from the usage
        /// context of this type signature.</param>
        /// <returns>The decoded <see cref="IType"/>.</returns>
        private ITypeSignature ReadTypeDefOrRefEncoded( BufferReader buffer, TypeClassifications learnedClassifications )
        {
            uint value = buffer.ReadCompressedInteger();
            int index = (int) ( ( value & ( ~3 ) ) >> 2 ) - 1;
            ITypeSignature type;

            switch ( value & 3 )
            {
                case 1: // TypeRef
                    type = GetTypeRef( index );
                    this.typeRefs[index].TypeClassifications |= learnedClassifications;
                    break;

                case 0: // TypeDef
                    type = GetTypeDef( index );
                    break;

                case 2: // TypeSpec
                    type = GetTypeSpec( index );
                    break;
                default:
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "UnexpectedEncodedTokenType",
                                                                               "TypeDefOrRef" );
            }

            return type;
        }

        /// <summary>
        /// Reads a method signature.
        /// </summary>
        /// <param name="buffer">A <see cref="BufferReader"/>.</param>
        /// <returns>A <see cref="MethodSignature"/>.</returns>
        private MethodSignature ReadMethodSignature( BufferReader buffer )
        {
            int arity = 0;


            CallingConvention callingConvention = (CallingConvention) buffer.ReadByte();

            if ( ( callingConvention & CallingConvention.Generic ) != 0 )
            {
                arity = buffer.ReadByte();
            }

            uint paramCount = buffer.ReadCompressedInteger();

            ITypeSignature returnType = this.ReadTypeSignature( buffer );

            List<ITypeSignature> parameterTypes = new List<ITypeSignature>( (int) paramCount );
            List<ITypeSignature> variableParameterTypes = null;

            List<ITypeSignature> currentCollection = parameterTypes;

            for ( int i = 0; i < paramCount; i++ )
            {
                ITypeSignature parameterType = this.ReadTypeSignature( buffer );

                if ( parameterType == TypeSignature.Sentinel )
                {
                    variableParameterTypes = new List<ITypeSignature>( (int) paramCount - i );
                    currentCollection = variableParameterTypes;
                    i--;
                }
                else
                {
                    currentCollection.Add( parameterType );
                }
            }

            MethodSignature signature = new MethodSignature(
                this.moduleDeclaration,
                callingConvention, returnType, parameterTypes, arity );
            if ( callingConvention == CallingConvention.VarArg && variableParameterTypes != null )
            {
                signature.VariableParameterTypes.AddRange( variableParameterTypes );
            }

            return signature;
        }

        /// <summary>
        /// Reads a type signature.
        /// </summary>
        /// <param name="buffer">A <see cref="BufferReader"/>.</param>
        /// <returns>A <see cref="TypeSignature"/>.</returns>
        private ITypeSignature ReadTypeSignature( BufferReader buffer )
        {
            CorElementType elementType = (CorElementType) buffer.ReadByte();

            switch ( elementType )
            {
                    #region Intrinsics

                case CorElementType.Boolean:
                case CorElementType.Char:
                case CorElementType.SByte:
                case CorElementType.Int16:
                case CorElementType.Int32:
                case CorElementType.Int64:
                case CorElementType.IntPtr:
                case CorElementType.Byte:
                case CorElementType.UInt16:
                case CorElementType.UInt32:
                case CorElementType.UInt64:
                case CorElementType.UIntPtr:
                case CorElementType.Single:
                case CorElementType.Double:
                case CorElementType.String:
                case CorElementType.Object:
                case CorElementType.Void:
                case CorElementType.TypedByRef:
                    return
                        this.moduleDeclaration.Cache.GetIntrinsic(
                            IntrinsicTypeSignature.MapCorElementTypeToIntrinsicType( elementType ) );

                case CorElementType.Sentinel:
                    return TypeSignature.Sentinel;

                    #endregion

                case CorElementType.TValue:
                    return this.ReadTypeDefOrRefEncoded( buffer, TypeClassifications.ValueType );

                case CorElementType.Class:
                    return this.ReadTypeDefOrRefEncoded( buffer, TypeClassifications.ReferenceType );

                case CorElementType.Pointer:
                    {
                        ITypeSignature type = this.ReadTypeSignature( buffer );
                        return new PointerTypeSignature( type, false );
                    }

                case CorElementType.MethodPointer:
                    return new MethodPointerTypeSignature( this.ReadMethodSignature( buffer ) );

                case CorElementType.Array:
                    return new ArrayTypeSignature( this.ReadTypeSignature( buffer ), ReadArrayShape( buffer ) );

                case CorElementType.SzArray:
                    {
                        return new ArrayTypeSignature( this.ReadTypeSignature( buffer ) );
                    }

                case CorElementType.GenericTypeParameter:
                    {
                        int ordinal = buffer.ReadByte();
                        return this.moduleDeclaration.Cache.GetGenericParameter( ordinal, GenericParameterKind.Type );
                    }

                case CorElementType.GenericMethodParameter:
                    {
                        int ordinal = buffer.ReadByte();
                        return this.moduleDeclaration.Cache.GetGenericParameter( ordinal, GenericParameterKind.Method );
                    }
                case CorElementType.GenericInstance:
                    {
                        ITypeSignature genericDeclaration = this.ReadTypeSignature( buffer );
                        int count = buffer.ReadByte();
                        ITypeSignature[] arguments = new ITypeSignature[count];
                        for ( int i = 0; i < count; i++ )
                        {
                            arguments[i] = this.ReadTypeSignature( buffer );
                        }

                        TypeSignature genericInstance =
                            new GenericTypeInstanceTypeSignature( (INamedType) genericDeclaration, arguments );

                        return genericInstance;
                    }

                case CorElementType.CustomModifierOptional:
                case CorElementType.CustomModifierRequired:
                    {
                        CustomModifier customModifier = new CustomModifier
                                                            {
                                                                Required =
                                                                    ( elementType ==
                                                                      CorElementType.CustomModifierRequired ),
                                                                Type =
                                                                    this.ReadTypeDefOrRefEncoded( buffer,
                                                                                                  TypeClassifications.
                                                                                                      None )
                                                            };

                        ITypeSignature parentType = this.ReadTypeSignature( buffer );
                        return new CustomModifierTypeSignature( customModifier, parentType );
                    }

                case CorElementType.ByRef:
                    return new PointerTypeSignature( this.ReadTypeSignature( buffer ), true );

                case CorElementType.Pinned:
                    {
                        ITypeSignature innerType = this.ReadTypeSignature( buffer );
                        return new PinnedTypeSignature( innerType );
                    }


                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( elementType,
                                                                                  "element type in type signature" );
            }
        }

        /// <summary>
        /// Reads the shape (dimensions) of an array.
        /// </summary>
        /// <param name="buffer">A <see cref="BufferReader"/>.</param>
        /// <returns>The array dimensions.</returns>
        private static ArrayDimension[] ReadArrayShape( BufferReader buffer )
        {
            uint rank = buffer.ReadCompressedInteger();

            uint numSizes = buffer.ReadCompressedInteger();
            uint[] sizes = new uint[numSizes];

            for ( uint i = 0; i < numSizes; i++ )
            {
                sizes[i] = buffer.ReadCompressedInteger();
            }

            uint numLoBounds = buffer.ReadCompressedInteger();
            uint[] loBounds = new uint[numLoBounds];

            for ( uint i = 0; i < numLoBounds; i++ )
            {
                loBounds[(int) i] = buffer.ReadCompressedInteger();
            }

            // Build the shape.
            ArrayDimension[] bounds = new ArrayDimension[rank];

            for ( uint i = 0; i < rank; i++ )
            {
                bounds[i] = new ArrayDimension(
                    i < numLoBounds ? (int) loBounds[i] : ArrayDimension.Unlimited,
                    i < numSizes ? (int) sizes[i] : ArrayDimension.Unlimited );
            }

            return bounds;
        }

        /// <summary>
        /// Reads a serialized value.
        /// </summary>
        /// <param name="valueType">Value type.</param>
        /// <param name="buffer">A <see cref="BufferReader"/>.</param>
        /// <returns>The serialized value.</returns>
        private SerializedValue ReadConstant( CorElementType valueType, BufferReader buffer )
        {
            switch ( valueType )
            {
                case CorElementType.Boolean:
                case CorElementType.Char:
                case CorElementType.SByte:
                case CorElementType.Int16:
                case CorElementType.Int32:
                case CorElementType.Int64:
                case CorElementType.Byte:
                case CorElementType.UInt16:
                case CorElementType.UInt32:
                case CorElementType.UInt64:
                case CorElementType.Single:
                case CorElementType.Double:
                    {
                        IntrinsicSerializationType intrinsicSerializationType =
                            this.moduleDeclaration.Cache.GetIntrinsicSerializationType(
                                IntrinsicTypeSignature.MapCorElementTypeToIntrinsicType( valueType ) );
                        return
                            new SerializedValue( intrinsicSerializationType,
                                                 intrinsicSerializationType.ReadValue( buffer ) );
                    }

                case CorElementType.Class:
                    {
                        uint value = buffer.ReadUInt32();
                        if ( value == 0 )
                        {
                            return null;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }

                case CorElementType.String:
                    {
                        string str;
                        int size = ( buffer.Size - buffer.Offset )/2;

                        if ( size > 0 )
                        {
                            str = Marshal.PtrToStringUni( buffer.CurrentPosition, size );
                        }
                        else
                        {
                            str = "";
                        }
                        buffer.Offset += buffer.Size;
                        return IntrinsicSerializationType.CreateValue( this.moduleDeclaration, str );
                    }

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( valueType, "valueType" );
            }
        }

        /// <summary>
        /// Reads a marshal type signature.
        /// </summary>
        /// <param name="buffer">A <see cref="BufferReader"/>.</param>
        /// <returns>A <see cref="MarshalType"/>.</returns>
        private static MarshalType ReadMarshalType( BufferReader buffer )
        {
            UnmanagedType unmanagedType = (UnmanagedType) buffer.ReadByte();

            switch ( unmanagedType )
            {
                case UnmanagedType.LPArray: // Array
                    {
                        UnmanagedType elementType = (UnmanagedType) buffer.ReadByte();
                        int additionalSizeParameter = -1;
                        int arraySize = -1;
                        if ( buffer.Offset < buffer.Size )
                        {
                            additionalSizeParameter = (int) buffer.ReadCompressedInteger();

                            if ( buffer.Offset < buffer.Size )
                            {
                                arraySize = (int) buffer.ReadCompressedInteger();
                            }
                        }

                        return new ArrayMarshalType( elementType, arraySize, additionalSizeParameter );
                    }

                case UnmanagedType.SafeArray:
                    if ( buffer.Offset < buffer.Size )
                    {
                        return new SafeArrayMarshalType( (VarEnum) buffer.ReadByte() );
                    }
                    else
                    {
                        return new SafeArrayMarshalType( VarEnum.VT_EMPTY );
                    }

                case UnmanagedType.ByValTStr: // FixedString
                    {
                        int size = (int) buffer.ReadCompressedInteger();
                        return new FixedStringMarshalType( size );
                    }

                case UnmanagedType.ByValArray: // FixedArray
                    {
                        int elementNumber = (int) buffer.ReadCompressedInteger();

                        UnmanagedType elementType = buffer.Offset < buffer.Size
                                                        ?
                                                            (UnmanagedType) buffer.ReadByte()
                                                        : ArrayMarshalType.UnknownElementType;
                        return new FixedArrayMarshalType( elementType, elementNumber );
                    }

                case UnmanagedType.CustomMarshaler:
                    {
                        string str = buffer.ReadSerString();
                        Guid guid = string.IsNullOrEmpty( str ) ? Guid.Empty : new Guid( str );
                        string unmanagedType2 = buffer.ReadSerString();
                        string managedType = buffer.ReadSerString();
                        string cookie = buffer.ReadSerString();
                        return new CustomMarshallerMarshalType( guid, unmanagedType2, managedType, cookie );
                    }

                default:
                    return new IntrinsicMarshalType( unmanagedType );
            }
        }

        #endregion

        #region Token resolution

        /// <summary>
        /// Resolves a type token.
        /// </summary>
        /// <param name="token">A token.</param>
        /// <returns>The <see cref="IType"/> corresponding to <paramref name="token"/>.</returns>
        private ITypeSignature ResolveType( MetadataToken token )
        {
            if ( token.IsNull )
            {
                return null;
            }

            switch ( token.TokenType )
            {
                case TokenType.TypeDef:
                    return GetTypeDef( token.Index );

                case TokenType.TypeRef:
                    return GetTypeRef( token.Index );

                case TokenType.TypeSpec:
                    return GetTypeSpec( token.Index );

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( token.TokenType, "token.TokenType" );
            }
        }

        /// <summary>
        /// Resolves a <see cref="IManifestResourceImplementation"/> token.
        /// </summary>
        /// <param name="token">A token.</param>
        /// <returns>The <see cref="IManifestResourceImplementation"/> corresponding to <paramref name="token"/>.</returns>
        private IManifestResourceImplementation ResolveManifestResourceImplementation( MetadataToken token )
        {
            if ( token.IsNull )
            {
                return null;
            }

            switch ( token.TokenType )
            {
                case TokenType.AssemblyRef:
                    return GetAssemblyRef( token.Index );

                case TokenType.File:
                    return GetManifestFile( token.Index );

                case TokenType.ExportedType:
                    return GetExportedType( token.Index );


                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( token.TokenType, "token.TokenType" );
            }
        }


        /// <summary>
        /// Resolves a method token
        /// </summary>
        /// <param name="token">A token.</param>
        /// <returns>The <see cref="IMethod"/> corresponding to <paramref name="token"/>.</returns>
        private IMethod ResolveMethod( MetadataToken token )
        {
            switch ( token.TokenType )
            {
                case TokenType.MemberRef:
                    return (IMethod) GetMemberRef( token.Index );

                case TokenType.MethodDef:
                    return GetMethodDef( token.Index );

                case TokenType.MethodSpec:
                    return GetMethodSpec( token.Index );

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( token.TokenType, "token.TokenType" );
            }
        }

        /// <summary>
        /// Resolves a member reference token
        /// </summary>
        /// <param name="token">A token.</param>
        /// <returns>The <see cref="IMemberRefResolutionScope"/> corresponding to <paramref name="token"/>.</returns>
        private IMemberRefResolutionScope ResolveMemberRefResolutionScope( MetadataToken token )
        {
            switch ( token.TokenType )
            {
                case TokenType.TypeRef:
                    return GetTypeRef( token.Index );

                case TokenType.TypeDef:
                    return GetTypeDef( token.Index );

                case TokenType.TypeSpec:
                    return GetTypeSpec( token.Index );

                case TokenType.ModuleRef:
                    return GetModuleRef( token.Index );

                case TokenType.MethodDef:
                    return GetMethodDef( token.Index );

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( token.TokenType, "token.TokenType" );
            }
        }

        private ISecurable ResolveSecurable( MetadataToken tkParent )
        {
            switch ( tkParent.TokenType )
            {
                case TokenType.Assembly:
                    return this.moduleDeclaration.AssemblyManifest;

                case TokenType.TypeDef:
                    return this.GetTypeDef( tkParent.Index );

                case TokenType.MethodDef:
                    return this.GetMethodDef( tkParent.Index );

                default:
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "UnexpectedTokenType",
                                                                               tkParent.TokenType,
                                                                               "DeclSecurityParentColumn",
                                                                               "Assembly, TypeDef, MethodDef" );
            }
        }

        internal MetadataDeclaration ResolveToken( MetadataToken token )
        {
            MetadataDeclaration declaration;
            switch ( token.TokenType )
            {
                case TokenType.MethodDef:
                    declaration = GetMethodDef( token.Index );
                    break;

                case TokenType.FieldDef:
                    declaration = GetFieldDef( token.Index );
                    break;

                case TokenType.TypeRef:
                    declaration = GetTypeRef( token.Index );
                    break;

                case TokenType.TypeDef:
                    declaration = GetTypeDef( token.Index );
                    break;

                case TokenType.ParamDef:
                    // TODO:
                    declaration = this.parameters[token.Index];
                    break;

                case TokenType.MemberRef:
                    declaration = GetMemberRef( token.Index );
                    break;

                case TokenType.MethodSpec:
                    declaration = GetMethodSpec( token.Index );
                    break;

                case TokenType.Module:
                    declaration = this.moduleDeclaration;
                    break;

                case TokenType.Permission:
                    // TODO: lazy resolving.
                    declaration = this.permissionSets[token.Index];
                    break;

                case TokenType.Property:
                    // TODO: lazy resolving.
                    declaration = this.properties[token.Index];
                    break;

                case TokenType.Event:
                    // TODO: lazy resolving.
                    declaration = this.events[token.Index];
                    break;

                case TokenType.Signature:
                    declaration = this.GetStandaloneSignature( token.Index );
                    break;

                case TokenType.ModuleRef:
                    declaration = GetModuleRef( token.Index );
                    break;

                case TokenType.TypeSpec:
                    declaration = GetTypeSpec( token.Index );
                    break;

                case TokenType.Assembly:
                    declaration = this.assemblyManifest;
                    break;

                case TokenType.AssemblyRef:
                    declaration = GetAssemblyRef( token.Index );
                    break;

                case TokenType.File:
                    declaration = GetManifestFile( token.Index );
                    break;

                case TokenType.ExportedType:
                    declaration = GetExportedType( token.Index );
                    break;

                case TokenType.ManifestResource:
                    declaration = GetManifestResource( token.Index );
                    break;

                case TokenType.GenericParam:
                    // TODO: lazy resolving.
                    declaration = this.genericParameters[token.Index];
                    break;

                case TokenType.CustomAttribute:
                    // TODO: lazy resolving.
                    declaration = this.customAttributes[token.Index];
                    break;


                default:
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "UnexpectedTokenType",
                                                                               token.TokenType,
                                                                               "token.TokenType",
                                                                               "a lot" );
            }

#if ASSERT
            if ( declaration == null )
                throw new InvalidOperationException(
                    string.Format(
                        "The declaration has not yet been loaded, and lazy loading is not available for tokens of type {0}.",
                        token.TokenType ) );
#endif

            return declaration;
        }

        #endregion

        public ImageReader ImageReader
        {
            get { return this.imageReader; }
        }

        public string ModuleLocation
        {
            get { return this.moduleLocation; }
        }

        /// <summary>
        /// Gets the symbol reader of the current module.
        /// </summary>
        /// <returns>A symbol reader, or <b>null</b> if there are no symbols for this module.</returns>
        [SuppressMessage( "Microsoft.Performance", "CA180:RemoveUnusedLocal" )]
        private ISymbolReader GetSymbolReader()
        {
            if ( !this.symbolReaderSet )
            {
                this.symbolReader = Platform.Current.GetSymbolReader( this );
                this.symbolReaderSet = true;
            }

            return this.symbolReader;
        }

        #region Import serialized attributes (permission sets and custom attributes)

        #region Permission sets

        private MultiDictionary<MetadataToken, int> permissionSetsIndex;


        internal void ImportPermissionSets( ISecurable securable )
        {
            if ( !this.lazyLoading )
            {
                securable.PermissionSets.PrepareImport( 0 );
                return;
            }


            if ( this.permissionSetsIndex == null )
            {
                this.permissionSetsIndex = new MultiDictionary<MetadataToken, int>( 64 );
                this.ImportPermissionSets();
            }

            MetadataToken tk = securable.MetadataToken;

            int n = this.permissionSetsIndex.GetCountByKey( tk );
            securable.PermissionSets.PrepareImport( n );

            if ( n > 0 )
            {
                foreach ( int i in this.permissionSetsIndex[tk] )
                {
                    this.ImportPermissionSet( i, securable );
                }
            }
        }


        /// <summary>
        /// Imports all permission sets.
        /// </summary>
        private void ImportPermissionSets()
        {
            for ( int i = 0; i < this.tables.DeclSecurityTable.RowCount; i++ )
            {
                MetadataRow row = this.tables.DeclSecurityTable.GetRow( i );
                MetadataToken tkSecurable = this.tables.DeclSecurityParentColumn.GetValue( row );

                if ( this.lazyLoading )
                {
                    this.permissionSetsIndex.Add( tkSecurable, i );
                }
                else
                {
                    ImportPermissionSet( i, this.ResolveSecurable( tkSecurable ) );
                }
            }
        }


        private void ImportPermissionSet( int i, ISecurable securable )
        {
            MetadataRow row = this.tables.DeclSecurityTable.GetRow( i );


            PermissionSetDeclaration permissionSet = new PermissionSetDeclaration
                                                         {
                                                             MetadataToken =
                                                                 new MetadataToken( TokenType.Permission, i ),
                                                             SecurityAction =
                                                                 ( (SecurityAction)
                                                                   this.tables.DeclSecurityActionColumn.GetValue( row ) )
                                                         };


            securable.PermissionSets.Import( permissionSet );
            this.permissionSets[i] = permissionSet;


            BufferReader buffer = this.tables.DeclSecurityPermissionSetColumn.GetValueBufferReader( row );

            if ( buffer != null && buffer.Size != 0 )
            {
                byte serializationKind = buffer.ReadByte();

                if ( serializationKind == 0x2e )
                {
                    uint attributeCount = buffer.ReadCompressedInteger();

                    for ( int j = 0; j < attributeCount; j++ )
                    {
                        PermissionDeclaration attribute = new PermissionDeclaration();
                        permissionSet.Attributes.Add( attribute );

                        string className = buffer.ReadSerString();

                        attribute.Type = this.moduleDeclaration.DeserializationUtil.GetTypeByName( className );

                        buffer.ReadCompressedInteger();

                        uint paramCount = buffer.ReadByte();
                        attribute.Properties.Capacity = (int) paramCount;

                        for ( int k = 0; k < paramCount; k++ )
                        {
                            attribute.Properties.Add(
                                this.moduleDeclaration.DeserializationUtil.ReadSerializedNamedArgument( k, buffer ) );
                        }
                    }
                }
                else if ( serializationKind == 0x3c )
                {
                    buffer.Offset -= sizeof(byte);
                    permissionSet.Xml = buffer.ReadStringUtf16( buffer.Size/2 );
                }
            }
        }

        #endregion

        #region Custom attributes

        private MultiDictionary<MetadataToken, int> customAttributesIndex;


        internal void ImportCustomAttributes( MetadataDeclaration target )
        {
            if ( !this.lazyLoading )
            {
                target.CustomAttributes.PrepareImport( 0 );
                return;
            }

            if ( this.customAttributesIndex == null )
            {
                this.customAttributesIndex = new MultiDictionary<MetadataToken, int>( 64 );
                this.ImportCustomAttributes();
            }

            MetadataToken tk = target.MetadataToken;

            int n = this.customAttributesIndex.GetCountByKey( tk );
            target.CustomAttributes.PrepareImport( n );
            if ( n > 0 )
            {
                foreach ( int i in customAttributesIndex[tk] )
                {
                    ImportCustomAttribute( target, i );
                }
            }
        }

        private void ImportCustomAttribute( MetadataDeclaration target, int i )
        {
            MetadataRow row = this.tables.CustomAttributeTable.GetRow( i );

            IMethod constructor = this.ResolveMethod( this.tables.CustomAttributeTypeColumn.GetValue( row ) );

            CustomAttributeDeclaration customAttribute =
                new CustomAttributeDeclaration( constructor,
                                                this.tables.CustomAttributeValueColumn.GetValueUnmanagedBuffer( row ) )
                    {
                        MetadataToken = new MetadataToken( TokenType.CustomAttribute, i )
                    };


            target.CustomAttributes.Import( customAttribute );

            this.customAttributes[i] = customAttribute;
        }

        /// <summary>
        /// Import all custom attributes.
        /// </summary>
        private void ImportCustomAttributes()
        {
            for ( int i = 0; i < this.tables.CustomAttributeTable.RowCount; i++ )
            {
                MetadataRow row = this.tables.CustomAttributeTable.GetRow( i );
                MetadataToken tkParent = this.tables.CustomAttributeParentColumn.GetValue( row );

                if ( this.lazyLoading )
                {
                    this.customAttributesIndex.Add( tkParent, i );
                }
                else
                {
                    MetadataDeclaration target = ResolveToken( tkParent );
                    ImportCustomAttribute( target, i );
                }
            }
        }

        #endregion

        #endregion

        public IntPtr AlignPointerAt4( IntPtr ptr )
        {
            return this.imageReader.AlignPointerAt4( ptr );
        }

        #region IDisposable Members

        private void Dispose( bool disposing )
        {
            if ( this.imageGCHandle.IsAllocated )
                this.imageGCHandle.Free();

            if ( disposing )
                GC.SuppressFinalize( this );
        }

        public void Dispose()
        {
            this.Dispose( true );
        }

        ~ModuleReader()
        {
            this.Dispose( false );
        }

        #endregion

        private struct TokenRange
        {
            public TokenRange( MetadataToken first, MetadataToken last )
            {
                this.First = first;
                this.Last = last;
            }

            public MetadataToken First;
            public MetadataToken Last;
        }
    }
}
