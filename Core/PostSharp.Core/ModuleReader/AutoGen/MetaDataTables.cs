namespace PostSharp.ModuleReader
{
    internal partial class MetadataTables
    {
        public readonly MetadataTable[] Tables;


        public readonly MetadataTable ModuleTable;
        private readonly MetadataColumn[] ModuleColumns;
        public readonly MetadataColumnInt16 ModuleGenerationColumn;
        public readonly MetadataColumnString ModuleNameColumn;
        public readonly MetadataColumnGuid ModuleMvidColumn;
        public readonly MetadataColumnGuid ModuleEncIdColumn;
        public readonly MetadataColumnGuid ModuleEncBaseIdColumn;


        public readonly MetadataTable TypeRefTable;
        private readonly MetadataColumn[] TypeRefColumns;
        public readonly MetadataColumnCodedToken TypeRefResolutionScopeColumn;
        public readonly MetadataColumnString TypeRefNameColumn;
        public readonly MetadataColumnString TypeRefNamespaceColumn;


        public readonly MetadataTable TypeDefTable;
        private readonly MetadataColumn[] TypeDefColumns;
        public readonly MetadataColumnInt32 TypeDefFlagsColumn;
        public readonly MetadataColumnString TypeDefNameColumn;
        public readonly MetadataColumnString TypeDefNamespaceColumn;
        public readonly MetadataColumnCodedToken TypeDefExtendsColumn;
        public readonly MetadataColumnRowIndex TypeDefFieldListColumn;
        public readonly MetadataColumnRowIndex TypeDefMethodListColumn;


        public readonly MetadataTable FieldPtrTable;
        private readonly MetadataColumn[] FieldPtrColumns;

        public readonly MetadataColumnRowIndex FieldPtrFieldColumn;


        public readonly MetadataTable FieldTable;
        private readonly MetadataColumn[] FieldColumns;
        public readonly MetadataColumnInt16 FieldFlagsColumn;
        public readonly MetadataColumnString FieldNameColumn;
        public readonly MetadataColumnBlob FieldSignatureColumn;


        public readonly MetadataTable MethodPtrTable;
        private readonly MetadataColumn[] MethodPtrColumns;

        public readonly MetadataColumnRowIndex MethodPtrMethodColumn;


        public readonly MetadataTable MethodTable;
        private readonly MetadataColumn[] MethodColumns;
        public readonly MetadataColumnInt32 MethodRVAColumn;
        public readonly MetadataColumnInt16 MethodImplFlagsColumn;
        public readonly MetadataColumnInt16 MethodFlagsColumn;
        public readonly MetadataColumnString MethodNameColumn;
        public readonly MetadataColumnBlob MethodSignatureColumn;
        public readonly MetadataColumnRowIndex MethodParamListColumn;


        public readonly MetadataTable ParamPtrTable;
        private readonly MetadataColumn[] ParamPtrColumns;

        public readonly MetadataColumnRowIndex ParamPtrParamColumn;


        public readonly MetadataTable ParamTable;
        private readonly MetadataColumn[] ParamColumns;
        public readonly MetadataColumnInt16 ParamFlagsColumn;
        public readonly MetadataColumnInt16 ParamSequenceColumn;
        public readonly MetadataColumnString ParamNameColumn;


        public readonly MetadataTable InterfaceImplTable;
        private readonly MetadataColumn[] InterfaceImplColumns;
        public readonly MetadataColumnRowIndex InterfaceImplClassColumn;
        public readonly MetadataColumnCodedToken InterfaceImplInterfaceColumn;


        public readonly MetadataTable MemberRefTable;
        private readonly MetadataColumn[] MemberRefColumns;

        public readonly MetadataColumnCodedToken MemberRefClassColumn;
        public readonly MetadataColumnString MemberRefNameColumn;
        public readonly MetadataColumnBlob MemberRefSignatureColumn;


        public readonly MetadataTable ConstantTable;
        private readonly MetadataColumn[] ConstantColumns;
        public readonly MetadataColumnInt16 ConstantTypeColumn;
        public readonly MetadataColumnCodedToken ConstantParentColumn;
        public readonly MetadataColumnBlob ConstantValueColumn;


        public readonly MetadataTable CustomAttributeTable;
        private readonly MetadataColumn[] CustomAttributeColumns;

        public readonly MetadataColumnCodedToken CustomAttributeParentColumn;
        public readonly MetadataColumnCodedToken CustomAttributeTypeColumn;
        public readonly MetadataColumnBlob CustomAttributeValueColumn;


        public readonly MetadataTable FieldMarshalTable;
        private readonly MetadataColumn[] FieldMarshalColumns;

        public readonly MetadataColumnCodedToken FieldMarshalParentColumn;
        public readonly MetadataColumnBlob FieldMarshalNativeTypeColumn;


        public readonly MetadataTable DeclSecurityTable;
        private readonly MetadataColumn[] DeclSecurityColumns;
        public readonly MetadataColumnInt16 DeclSecurityActionColumn;
        public readonly MetadataColumnCodedToken DeclSecurityParentColumn;
        public readonly MetadataColumnBlob DeclSecurityPermissionSetColumn;


        public readonly MetadataTable ClassLayoutTable;
        private readonly MetadataColumn[] ClassLayoutColumns;
        public readonly MetadataColumnInt16 ClassLayoutPackingSizeColumn;
        public readonly MetadataColumnInt32 ClassLayoutClassSizeColumn;
        public readonly MetadataColumnRowIndex ClassLayoutParentColumn;


        public readonly MetadataTable FieldLayoutTable;
        private readonly MetadataColumn[] FieldLayoutColumns;
        public readonly MetadataColumnInt32 FieldLayoutOffSetColumn;
        public readonly MetadataColumnRowIndex FieldLayoutFieldColumn;


        public readonly MetadataTable StandAloneSigTable;
        private readonly MetadataColumn[] StandAloneSigColumns;

        public readonly MetadataColumnBlob StandAloneSigSignatureColumn;


        public readonly MetadataTable EventMapTable;
        private readonly MetadataColumn[] EventMapColumns;

        public readonly MetadataColumnRowIndex EventMapParentColumn;
        public readonly MetadataColumnRowIndex EventMapEventListColumn;


        public readonly MetadataTable EventPtrTable;
        private readonly MetadataColumn[] EventPtrColumns;

        public readonly MetadataColumnRowIndex EventPtrEventColumn;


        public readonly MetadataTable EventTable;
        private readonly MetadataColumn[] EventColumns;
        public readonly MetadataColumnInt16 EventEventFlagsColumn;
        public readonly MetadataColumnString EventNameColumn;
        public readonly MetadataColumnCodedToken EventEventTypeColumn;


        public readonly MetadataTable PropertyMapTable;
        private readonly MetadataColumn[] PropertyMapColumns;

        public readonly MetadataColumnRowIndex PropertyMapParentColumn;
        public readonly MetadataColumnRowIndex PropertyMapPropertyListColumn;


        public readonly MetadataTable PropertyPtrTable;
        private readonly MetadataColumn[] PropertyPtrColumns;

        public readonly MetadataColumnRowIndex PropertyPtrPropertyColumn;


        public readonly MetadataTable PropertyTable;
        private readonly MetadataColumn[] PropertyColumns;
        public readonly MetadataColumnInt16 PropertyPropFlagsColumn;
        public readonly MetadataColumnString PropertyNameColumn;
        public readonly MetadataColumnBlob PropertyTypeColumn;


        public readonly MetadataTable MethodSemanticsTable;
        private readonly MetadataColumn[] MethodSemanticsColumns;
        public readonly MetadataColumnInt16 MethodSemanticsSemanticColumn;
        public readonly MetadataColumnRowIndex MethodSemanticsMethodColumn;
        public readonly MetadataColumnCodedToken MethodSemanticsAssociationColumn;


        public readonly MetadataTable MethodImplTable;
        private readonly MetadataColumn[] MethodImplColumns;
        public readonly MetadataColumnRowIndex MethodImplClassColumn;
        public readonly MetadataColumnCodedToken MethodImplMethodBodyColumn;
        public readonly MetadataColumnCodedToken MethodImplMethodDefDeclarationColumn;


        public readonly MetadataTable ModuleRefTable;
        private readonly MetadataColumn[] ModuleRefColumns;

        public readonly MetadataColumnString ModuleRefNameColumn;


        public readonly MetadataTable TypeSpecTable;
        private readonly MetadataColumn[] TypeSpecColumns;

        public readonly MetadataColumnBlob TypeSpecSignatureColumn;


        public readonly MetadataTable ImplMapTable;
        private readonly MetadataColumn[] ImplMapColumns;
        public readonly MetadataColumnInt16 ImplMapMappingFlagsColumn;
        public readonly MetadataColumnCodedToken ImplMapMemberForwardedColumn;
        public readonly MetadataColumnString ImplMapImportNameColumn;
        public readonly MetadataColumnRowIndex ImplMapImportScopeColumn;


        public readonly MetadataTable FieldRVATable;
        private readonly MetadataColumn[] FieldRVAColumns;
        public readonly MetadataColumnInt32 FieldRVARVAColumn;
        public readonly MetadataColumnRowIndex FieldRVAFieldColumn;


        public readonly MetadataTable ENCLogTable;
        private readonly MetadataColumn[] ENCLogColumns;
        public readonly MetadataColumnInt32 ENCLogTokenColumn;
        public readonly MetadataColumnInt32 ENCLogFuncCodeColumn;


        public readonly MetadataTable ENCMapTable;
        private readonly MetadataColumn[] ENCMapColumns;
        public readonly MetadataColumnInt32 ENCMapTokenColumn;


        public readonly MetadataTable AssemblyTable;
        private readonly MetadataColumn[] AssemblyColumns;
        public readonly MetadataColumnInt32 AssemblyHashAlgIdColumn;
        public readonly MetadataColumnInt16 AssemblyMajorVersionColumn;
        public readonly MetadataColumnInt16 AssemblyMinorVersionColumn;
        public readonly MetadataColumnInt16 AssemblyBuildNumberColumn;
        public readonly MetadataColumnInt16 AssemblyRevisionNumberColumn;
        public readonly MetadataColumnInt32 AssemblyFlagsColumn;
        public readonly MetadataColumnBlob AssemblyPublicKeyColumn;
        public readonly MetadataColumnString AssemblyNameColumn;
        public readonly MetadataColumnString AssemblyLocaleColumn;


        public readonly MetadataTable AssemblyProcessorTable;
        private readonly MetadataColumn[] AssemblyProcessorColumns;
        public readonly MetadataColumnInt32 AssemblyProcessorProcessorColumn;


        public readonly MetadataTable AssemblyOSTable;
        private readonly MetadataColumn[] AssemblyOSColumns;
        public readonly MetadataColumnInt32 AssemblyOSOSPlatformIdColumn;
        public readonly MetadataColumnInt32 AssemblyOSOSMajorVersionColumn;
        public readonly MetadataColumnInt32 AssemblyOSOSMinorVersionColumn;


        public readonly MetadataTable AssemblyRefTable;
        private readonly MetadataColumn[] AssemblyRefColumns;
        public readonly MetadataColumnInt16 AssemblyRefMajorVersionColumn;
        public readonly MetadataColumnInt16 AssemblyRefMinorVersionColumn;
        public readonly MetadataColumnInt16 AssemblyRefBuildNumberColumn;
        public readonly MetadataColumnInt16 AssemblyRefRevisionNumberColumn;
        public readonly MetadataColumnInt32 AssemblyRefFlagsColumn;
        public readonly MetadataColumnBlob AssemblyRefPublicKeyOrTokenColumn;
        public readonly MetadataColumnString AssemblyRefNameColumn;
        public readonly MetadataColumnString AssemblyRefLocaleColumn;
        public readonly MetadataColumnBlob AssemblyRefHashValueColumn;


        public readonly MetadataTable AssemblyRefProcessorTable;
        private readonly MetadataColumn[] AssemblyRefProcessorColumns;
        public readonly MetadataColumnInt32 AssemblyRefProcessorProcessorColumn;
        public readonly MetadataColumnRowIndex AssemblyRefProcessorAssemblyRefColumn;


        public readonly MetadataTable AssemblyRefOSTable;
        private readonly MetadataColumn[] AssemblyRefOSColumns;
        public readonly MetadataColumnInt32 AssemblyRefOSOSPlatformIdColumn;
        public readonly MetadataColumnInt32 AssemblyRefOSOSMajorVersionColumn;
        public readonly MetadataColumnInt32 AssemblyRefOSOSMinorVersionColumn;
        public readonly MetadataColumnRowIndex AssemblyRefOSAssemblyRefColumn;


        public readonly MetadataTable FileTable;
        private readonly MetadataColumn[] FileColumns;
        public readonly MetadataColumnInt32 FileFlagsColumn;
        public readonly MetadataColumnString FileNameColumn;
        public readonly MetadataColumnBlob FileHashValueColumn;


        public readonly MetadataTable ExportedTypeTable;
        private readonly MetadataColumn[] ExportedTypeColumns;
        public readonly MetadataColumnInt32 ExportedTypeFlagsColumn;
        public readonly MetadataColumnInt32 ExportedTypeTypeDefIdColumn;
        public readonly MetadataColumnString ExportedTypeTypeNameColumn;
        public readonly MetadataColumnString ExportedTypeTypeNamespaceColumn;
        public readonly MetadataColumnCodedToken ExportedTypeImplementationColumn;


        public readonly MetadataTable ManifestResourceTable;
        private readonly MetadataColumn[] ManifestResourceColumns;
        public readonly MetadataColumnInt32 ManifestResourceOffsetColumn;
        public readonly MetadataColumnInt32 ManifestResourceFlagsColumn;
        public readonly MetadataColumnString ManifestResourceNameColumn;
        public readonly MetadataColumnCodedToken ManifestResourceImplementationColumn;


        public readonly MetadataTable NestedClassTable;
        private readonly MetadataColumn[] NestedClassColumns;
        public readonly MetadataColumnRowIndex NestedClassNestedClassColumn;
        public readonly MetadataColumnRowIndex NestedClassEnclosingClassColumn;


        public readonly MetadataTable GenericParamTable;
        private readonly MetadataColumn[] GenericParamColumns;
        public readonly MetadataColumnInt16 GenericParamNumberColumn;
        public readonly MetadataColumnInt16 GenericParamFlagsColumn;
        public readonly MetadataColumnCodedToken GenericParamOwnerColumn;
        public readonly MetadataColumnString GenericParamNameColumn;


        public readonly MetadataTable MethodSpecTable;
        private readonly MetadataColumn[] MethodSpecColumns;

        public readonly MetadataColumnCodedToken MethodSpecMethodColumn;
        public readonly MetadataColumnBlob MethodSpecInstantiationColumn;


        public readonly MetadataTable GenericParamConstraintTable;
        private readonly MetadataColumn[] GenericParamConstraintColumns;

        public readonly MetadataColumnRowIndex GenericParamConstraintOwnerColumn;
        public readonly MetadataColumnCodedToken GenericParamConstraintConstraintColumn;


        public MetadataTables( ImageReader reader )
        {
            this.imageReader = reader;
            int tableOrdinal = 0, columnOrdinal;


            columnOrdinal = 0;
            ModuleTable = new MetadataTable( this, "Module", tableOrdinal++ );
            ModuleGenerationColumn = new MetadataColumnInt16( ModuleTable, "Generation", columnOrdinal++ );
            ModuleNameColumn = new MetadataColumnString( ModuleTable, "Name", columnOrdinal++ );
            ModuleMvidColumn = new MetadataColumnGuid( ModuleTable, "Mvid", columnOrdinal++ );
            ModuleEncIdColumn = new MetadataColumnGuid( ModuleTable, "EncId", columnOrdinal++ );
            ModuleEncBaseIdColumn = new MetadataColumnGuid( ModuleTable, "EncBaseId", columnOrdinal++ );


            columnOrdinal = 0;
            TypeRefTable = new MetadataTable( this, "TypeRef", tableOrdinal++ );
            TypeRefResolutionScopeColumn =
                new MetadataColumnCodedToken( TypeRefTable, "ResolutionScope", CodedTokens.ResolutionScope,
                                              columnOrdinal++ );
            TypeRefNameColumn = new MetadataColumnString( TypeRefTable, "Name", columnOrdinal++ );
            TypeRefNamespaceColumn = new MetadataColumnString( TypeRefTable, "Namespace", columnOrdinal++ );


            columnOrdinal = 0;
            TypeDefTable = new MetadataTable( this, "TypeDef", tableOrdinal++ );
            TypeDefFlagsColumn = new MetadataColumnInt32( TypeDefTable, "Flags", columnOrdinal++ );
            TypeDefNameColumn = new MetadataColumnString( TypeDefTable, "Name", columnOrdinal++ );
            TypeDefNamespaceColumn = new MetadataColumnString( TypeDefTable, "Namespace", columnOrdinal++ );
            TypeDefExtendsColumn =
                new MetadataColumnCodedToken( TypeDefTable, "Extends", CodedTokens.TypeDefOrRef, columnOrdinal++ );
            TypeDefFieldListColumn =
                new MetadataColumnRowIndex( TypeDefTable, "FieldList", MetadataTableOrdinal.Field, columnOrdinal++ );
            TypeDefMethodListColumn =
                new MetadataColumnRowIndex( TypeDefTable, "MethodList", MetadataTableOrdinal.Method, columnOrdinal++ );


            columnOrdinal = 0;
            FieldPtrTable = new MetadataTable( this, "FieldPtr", tableOrdinal++ );

            FieldPtrFieldColumn =
                new MetadataColumnRowIndex( FieldPtrTable, "Field", MetadataTableOrdinal.Field, columnOrdinal++ );


            columnOrdinal = 0;
            FieldTable = new MetadataTable( this, "Field", tableOrdinal++ );
            FieldFlagsColumn = new MetadataColumnInt16( FieldTable, "Flags", columnOrdinal++ );
            FieldNameColumn = new MetadataColumnString( FieldTable, "Name", columnOrdinal++ );
            FieldSignatureColumn = new MetadataColumnBlob( FieldTable, "Signature", columnOrdinal++ );


            columnOrdinal = 0;
            MethodPtrTable = new MetadataTable( this, "MethodPtr", tableOrdinal++ );

            MethodPtrMethodColumn =
                new MetadataColumnRowIndex( MethodPtrTable, "Method", MetadataTableOrdinal.Method, columnOrdinal++ );


            columnOrdinal = 0;
            MethodTable = new MetadataTable( this, "Method", tableOrdinal++ );
            MethodRVAColumn = new MetadataColumnInt32( MethodTable, "RVA", columnOrdinal++ );
            MethodImplFlagsColumn = new MetadataColumnInt16( MethodTable, "ImplFlags", columnOrdinal++ );
            MethodFlagsColumn = new MetadataColumnInt16( MethodTable, "Flags", columnOrdinal++ );
            MethodNameColumn = new MetadataColumnString( MethodTable, "Name", columnOrdinal++ );
            MethodSignatureColumn = new MetadataColumnBlob( MethodTable, "Signature", columnOrdinal++ );
            MethodParamListColumn =
                new MetadataColumnRowIndex( MethodTable, "ParamList", MetadataTableOrdinal.Param, columnOrdinal++ );


            columnOrdinal = 0;
            ParamPtrTable = new MetadataTable( this, "ParamPtr", tableOrdinal++ );

            ParamPtrParamColumn =
                new MetadataColumnRowIndex( ParamPtrTable, "Param", MetadataTableOrdinal.Param, columnOrdinal++ );


            columnOrdinal = 0;
            ParamTable = new MetadataTable( this, "Param", tableOrdinal++ );
            ParamFlagsColumn = new MetadataColumnInt16( ParamTable, "Flags", columnOrdinal++ );
            ParamSequenceColumn = new MetadataColumnInt16( ParamTable, "Sequence", columnOrdinal++ );
            ParamNameColumn = new MetadataColumnString( ParamTable, "Name", columnOrdinal++ );


            columnOrdinal = 0;
            InterfaceImplTable = new MetadataTable( this, "InterfaceImpl", tableOrdinal++ );
            InterfaceImplClassColumn =
                new MetadataColumnRowIndex( InterfaceImplTable, "Class", MetadataTableOrdinal.TypeDef, columnOrdinal++ );
            InterfaceImplInterfaceColumn =
                new MetadataColumnCodedToken( InterfaceImplTable, "Interface", CodedTokens.TypeDefOrRef, columnOrdinal++ );


            columnOrdinal = 0;
            MemberRefTable = new MetadataTable( this, "MemberRef", tableOrdinal++ );

            MemberRefClassColumn =
                new MetadataColumnCodedToken( MemberRefTable, "Class", CodedTokens.MemberRefParent, columnOrdinal++ );
            MemberRefNameColumn = new MetadataColumnString( MemberRefTable, "Name", columnOrdinal++ );
            MemberRefSignatureColumn = new MetadataColumnBlob( MemberRefTable, "Signature", columnOrdinal++ );


            columnOrdinal = 0;
            ConstantTable = new MetadataTable( this, "Constant", tableOrdinal++ );
            ConstantTypeColumn = new MetadataColumnInt16( ConstantTable, "Type", columnOrdinal++ );
            ConstantParentColumn =
                new MetadataColumnCodedToken( ConstantTable, "Parent", CodedTokens.HasConstant, columnOrdinal++ );
            ConstantValueColumn = new MetadataColumnBlob( ConstantTable, "Value", columnOrdinal++ );


            columnOrdinal = 0;
            CustomAttributeTable = new MetadataTable( this, "CustomAttribute", tableOrdinal++ );

            CustomAttributeParentColumn =
                new MetadataColumnCodedToken( CustomAttributeTable, "Parent", CodedTokens.HasCustomAttribute,
                                              columnOrdinal++ );
            CustomAttributeTypeColumn =
                new MetadataColumnCodedToken( CustomAttributeTable, "Type", CodedTokens.CustomAttributeType,
                                              columnOrdinal++ );
            CustomAttributeValueColumn = new MetadataColumnBlob( CustomAttributeTable, "Value", columnOrdinal++ );


            columnOrdinal = 0;
            FieldMarshalTable = new MetadataTable( this, "FieldMarshal", tableOrdinal++ );

            FieldMarshalParentColumn =
                new MetadataColumnCodedToken( FieldMarshalTable, "Parent", CodedTokens.HasFieldMarshal, columnOrdinal++ );
            FieldMarshalNativeTypeColumn = new MetadataColumnBlob( FieldMarshalTable, "NativeType", columnOrdinal++ );


            columnOrdinal = 0;
            DeclSecurityTable = new MetadataTable( this, "DeclSecurity", tableOrdinal++ );
            DeclSecurityActionColumn = new MetadataColumnInt16( DeclSecurityTable, "Action", columnOrdinal++ );
            DeclSecurityParentColumn =
                new MetadataColumnCodedToken( DeclSecurityTable, "Parent", CodedTokens.HasDeclSecurity, columnOrdinal++ );
            DeclSecurityPermissionSetColumn =
                new MetadataColumnBlob( DeclSecurityTable, "PermissionSet", columnOrdinal++ );


            columnOrdinal = 0;
            ClassLayoutTable = new MetadataTable( this, "ClassLayout", tableOrdinal++ );
            ClassLayoutPackingSizeColumn = new MetadataColumnInt16( ClassLayoutTable, "PackingSize", columnOrdinal++ );
            ClassLayoutClassSizeColumn = new MetadataColumnInt32( ClassLayoutTable, "ClassSize", columnOrdinal++ );
            ClassLayoutParentColumn =
                new MetadataColumnRowIndex( ClassLayoutTable, "Parent", MetadataTableOrdinal.TypeDef, columnOrdinal++ );


            columnOrdinal = 0;
            FieldLayoutTable = new MetadataTable( this, "FieldLayout", tableOrdinal++ );
            FieldLayoutOffSetColumn = new MetadataColumnInt32( FieldLayoutTable, "OffSet", columnOrdinal++ );
            FieldLayoutFieldColumn =
                new MetadataColumnRowIndex( FieldLayoutTable, "Field", MetadataTableOrdinal.Field, columnOrdinal++ );


            columnOrdinal = 0;
            StandAloneSigTable = new MetadataTable( this, "StandAloneSig", tableOrdinal++ );

            StandAloneSigSignatureColumn = new MetadataColumnBlob( StandAloneSigTable, "Signature", columnOrdinal++ );


            columnOrdinal = 0;
            EventMapTable = new MetadataTable( this, "EventMap", tableOrdinal++ );

            EventMapParentColumn =
                new MetadataColumnRowIndex( EventMapTable, "Parent", MetadataTableOrdinal.TypeDef, columnOrdinal++ );
            EventMapEventListColumn =
                new MetadataColumnRowIndex( EventMapTable, "EventList", MetadataTableOrdinal.Event, columnOrdinal++ );


            columnOrdinal = 0;
            EventPtrTable = new MetadataTable( this, "EventPtr", tableOrdinal++ );

            EventPtrEventColumn =
                new MetadataColumnRowIndex( EventPtrTable, "Event", MetadataTableOrdinal.Event, columnOrdinal++ );


            columnOrdinal = 0;
            EventTable = new MetadataTable( this, "Event", tableOrdinal++ );
            EventEventFlagsColumn = new MetadataColumnInt16( EventTable, "EventFlags", columnOrdinal++ );
            EventNameColumn = new MetadataColumnString( EventTable, "Name", columnOrdinal++ );
            EventEventTypeColumn =
                new MetadataColumnCodedToken( EventTable, "EventType", CodedTokens.TypeDefOrRef, columnOrdinal++ );


            columnOrdinal = 0;
            PropertyMapTable = new MetadataTable( this, "PropertyMap", tableOrdinal++ );

            PropertyMapParentColumn =
                new MetadataColumnRowIndex( PropertyMapTable, "Parent", MetadataTableOrdinal.TypeDef, columnOrdinal++ );
            PropertyMapPropertyListColumn =
                new MetadataColumnRowIndex( PropertyMapTable, "PropertyList", MetadataTableOrdinal.Property,
                                            columnOrdinal++ );


            columnOrdinal = 0;
            PropertyPtrTable = new MetadataTable( this, "PropertyPtr", tableOrdinal++ );

            PropertyPtrPropertyColumn =
                new MetadataColumnRowIndex( PropertyPtrTable, "Property", MetadataTableOrdinal.Property, columnOrdinal++ );


            columnOrdinal = 0;
            PropertyTable = new MetadataTable( this, "Property", tableOrdinal++ );
            PropertyPropFlagsColumn = new MetadataColumnInt16( PropertyTable, "PropFlags", columnOrdinal++ );
            PropertyNameColumn = new MetadataColumnString( PropertyTable, "Name", columnOrdinal++ );
            PropertyTypeColumn = new MetadataColumnBlob( PropertyTable, "Type", columnOrdinal++ );


            columnOrdinal = 0;
            MethodSemanticsTable = new MetadataTable( this, "MethodSemantics", tableOrdinal++ );
            MethodSemanticsSemanticColumn = new MetadataColumnInt16( MethodSemanticsTable, "Semantic", columnOrdinal++ );
            MethodSemanticsMethodColumn =
                new MetadataColumnRowIndex( MethodSemanticsTable, "Method", MetadataTableOrdinal.Method, columnOrdinal++ );
            MethodSemanticsAssociationColumn =
                new MetadataColumnCodedToken( MethodSemanticsTable, "Association", CodedTokens.HasSemantic,
                                              columnOrdinal++ );


            columnOrdinal = 0;
            MethodImplTable = new MetadataTable( this, "MethodImpl", tableOrdinal++ );
            MethodImplClassColumn =
                new MetadataColumnRowIndex( MethodImplTable, "Class", MetadataTableOrdinal.TypeDef, columnOrdinal++ );
            MethodImplMethodBodyColumn =
                new MetadataColumnCodedToken( MethodImplTable, "MethodBody", CodedTokens.MethodDefOrRef, columnOrdinal++ );
            MethodImplMethodDefDeclarationColumn =
                new MetadataColumnCodedToken( MethodImplTable, "MethodDefDeclaration", CodedTokens.MethodDefOrRef,
                                              columnOrdinal++ );


            columnOrdinal = 0;
            ModuleRefTable = new MetadataTable( this, "ModuleRef", tableOrdinal++ );

            ModuleRefNameColumn = new MetadataColumnString( ModuleRefTable, "Name", columnOrdinal++ );


            columnOrdinal = 0;
            TypeSpecTable = new MetadataTable( this, "TypeSpec", tableOrdinal++ );

            TypeSpecSignatureColumn = new MetadataColumnBlob( TypeSpecTable, "Signature", columnOrdinal++ );


            columnOrdinal = 0;
            ImplMapTable = new MetadataTable( this, "ImplMap", tableOrdinal++ );
            ImplMapMappingFlagsColumn = new MetadataColumnInt16( ImplMapTable, "MappingFlags", columnOrdinal++ );
            ImplMapMemberForwardedColumn =
                new MetadataColumnCodedToken( ImplMapTable, "MemberForwarded", CodedTokens.MemberForwarded,
                                              columnOrdinal++ );
            ImplMapImportNameColumn = new MetadataColumnString( ImplMapTable, "ImportName", columnOrdinal++ );
            ImplMapImportScopeColumn =
                new MetadataColumnRowIndex( ImplMapTable, "ImportScope", MetadataTableOrdinal.ModuleRef, columnOrdinal++ );


            columnOrdinal = 0;
            FieldRVATable = new MetadataTable( this, "FieldRVA", tableOrdinal++ );
            FieldRVARVAColumn = new MetadataColumnInt32( FieldRVATable, "RVA", columnOrdinal++ );
            FieldRVAFieldColumn =
                new MetadataColumnRowIndex( FieldRVATable, "Field", MetadataTableOrdinal.Field, columnOrdinal++ );


            columnOrdinal = 0;
            ENCLogTable = new MetadataTable( this, "ENCLog", tableOrdinal++ );
            ENCLogTokenColumn = new MetadataColumnInt32( ENCLogTable, "Token", columnOrdinal++ );
            ENCLogFuncCodeColumn = new MetadataColumnInt32( ENCLogTable, "FuncCode", columnOrdinal++ );


            columnOrdinal = 0;
            ENCMapTable = new MetadataTable( this, "ENCMap", tableOrdinal++ );
            ENCMapTokenColumn = new MetadataColumnInt32( ENCMapTable, "Token", columnOrdinal++ );


            columnOrdinal = 0;
            AssemblyTable = new MetadataTable( this, "Assembly", tableOrdinal++ );
            AssemblyHashAlgIdColumn = new MetadataColumnInt32( AssemblyTable, "HashAlgId", columnOrdinal++ );
            AssemblyMajorVersionColumn = new MetadataColumnInt16( AssemblyTable, "MajorVersion", columnOrdinal++ );
            AssemblyMinorVersionColumn = new MetadataColumnInt16( AssemblyTable, "MinorVersion", columnOrdinal++ );
            AssemblyBuildNumberColumn = new MetadataColumnInt16( AssemblyTable, "BuildNumber", columnOrdinal++ );
            AssemblyRevisionNumberColumn = new MetadataColumnInt16( AssemblyTable, "RevisionNumber", columnOrdinal++ );
            AssemblyFlagsColumn = new MetadataColumnInt32( AssemblyTable, "Flags", columnOrdinal++ );
            AssemblyPublicKeyColumn = new MetadataColumnBlob( AssemblyTable, "PublicKey", columnOrdinal++ );
            AssemblyNameColumn = new MetadataColumnString( AssemblyTable, "Name", columnOrdinal++ );
            AssemblyLocaleColumn = new MetadataColumnString( AssemblyTable, "Locale", columnOrdinal++ );


            columnOrdinal = 0;
            AssemblyProcessorTable = new MetadataTable( this, "AssemblyProcessor", tableOrdinal++ );
            AssemblyProcessorProcessorColumn =
                new MetadataColumnInt32( AssemblyProcessorTable, "Processor", columnOrdinal++ );


            columnOrdinal = 0;
            AssemblyOSTable = new MetadataTable( this, "AssemblyOS", tableOrdinal++ );
            AssemblyOSOSPlatformIdColumn = new MetadataColumnInt32( AssemblyOSTable, "OSPlatformId", columnOrdinal++ );
            AssemblyOSOSMajorVersionColumn =
                new MetadataColumnInt32( AssemblyOSTable, "OSMajorVersion", columnOrdinal++ );
            AssemblyOSOSMinorVersionColumn =
                new MetadataColumnInt32( AssemblyOSTable, "OSMinorVersion", columnOrdinal++ );


            columnOrdinal = 0;
            AssemblyRefTable = new MetadataTable( this, "AssemblyRef", tableOrdinal++ );
            AssemblyRefMajorVersionColumn = new MetadataColumnInt16( AssemblyRefTable, "MajorVersion", columnOrdinal++ );
            AssemblyRefMinorVersionColumn = new MetadataColumnInt16( AssemblyRefTable, "MinorVersion", columnOrdinal++ );
            AssemblyRefBuildNumberColumn = new MetadataColumnInt16( AssemblyRefTable, "BuildNumber", columnOrdinal++ );
            AssemblyRefRevisionNumberColumn =
                new MetadataColumnInt16( AssemblyRefTable, "RevisionNumber", columnOrdinal++ );
            AssemblyRefFlagsColumn = new MetadataColumnInt32( AssemblyRefTable, "Flags", columnOrdinal++ );
            AssemblyRefPublicKeyOrTokenColumn =
                new MetadataColumnBlob( AssemblyRefTable, "PublicKeyOrToken", columnOrdinal++ );
            AssemblyRefNameColumn = new MetadataColumnString( AssemblyRefTable, "Name", columnOrdinal++ );
            AssemblyRefLocaleColumn = new MetadataColumnString( AssemblyRefTable, "Locale", columnOrdinal++ );
            AssemblyRefHashValueColumn = new MetadataColumnBlob( AssemblyRefTable, "HashValue", columnOrdinal++ );


            columnOrdinal = 0;
            AssemblyRefProcessorTable = new MetadataTable( this, "AssemblyRefProcessor", tableOrdinal++ );
            AssemblyRefProcessorProcessorColumn =
                new MetadataColumnInt32( AssemblyRefProcessorTable, "Processor", columnOrdinal++ );
            AssemblyRefProcessorAssemblyRefColumn =
                new MetadataColumnRowIndex( AssemblyRefProcessorTable, "AssemblyRef", MetadataTableOrdinal.AssemblyRef,
                                            columnOrdinal++ );


            columnOrdinal = 0;
            AssemblyRefOSTable = new MetadataTable( this, "AssemblyRefOS", tableOrdinal++ );
            AssemblyRefOSOSPlatformIdColumn =
                new MetadataColumnInt32( AssemblyRefOSTable, "OSPlatformId", columnOrdinal++ );
            AssemblyRefOSOSMajorVersionColumn =
                new MetadataColumnInt32( AssemblyRefOSTable, "OSMajorVersion", columnOrdinal++ );
            AssemblyRefOSOSMinorVersionColumn =
                new MetadataColumnInt32( AssemblyRefOSTable, "OSMinorVersion", columnOrdinal++ );
            AssemblyRefOSAssemblyRefColumn =
                new MetadataColumnRowIndex( AssemblyRefOSTable, "AssemblyRef", MetadataTableOrdinal.AssemblyRef,
                                            columnOrdinal++ );


            columnOrdinal = 0;
            FileTable = new MetadataTable( this, "File", tableOrdinal++ );
            FileFlagsColumn = new MetadataColumnInt32( FileTable, "Flags", columnOrdinal++ );
            FileNameColumn = new MetadataColumnString( FileTable, "Name", columnOrdinal++ );
            FileHashValueColumn = new MetadataColumnBlob( FileTable, "HashValue", columnOrdinal++ );


            columnOrdinal = 0;
            ExportedTypeTable = new MetadataTable( this, "ExportedType", tableOrdinal++ );
            ExportedTypeFlagsColumn = new MetadataColumnInt32( ExportedTypeTable, "Flags", columnOrdinal++ );
            ExportedTypeTypeDefIdColumn = new MetadataColumnInt32( ExportedTypeTable, "TypeDefId", columnOrdinal++ );
            ExportedTypeTypeNameColumn = new MetadataColumnString( ExportedTypeTable, "TypeName", columnOrdinal++ );
            ExportedTypeTypeNamespaceColumn =
                new MetadataColumnString( ExportedTypeTable, "TypeNamespace", columnOrdinal++ );
            ExportedTypeImplementationColumn =
                new MetadataColumnCodedToken( ExportedTypeTable, "implementation", CodedTokens.Implementation,
                                              columnOrdinal++ );


            columnOrdinal = 0;
            ManifestResourceTable = new MetadataTable( this, "ManifestResource", tableOrdinal++ );
            ManifestResourceOffsetColumn = new MetadataColumnInt32( ManifestResourceTable, "Offset", columnOrdinal++ );
            ManifestResourceFlagsColumn = new MetadataColumnInt32( ManifestResourceTable, "Flags", columnOrdinal++ );
            ManifestResourceNameColumn = new MetadataColumnString( ManifestResourceTable, "Name", columnOrdinal++ );
            ManifestResourceImplementationColumn =
                new MetadataColumnCodedToken( ManifestResourceTable, "implementation", CodedTokens.Implementation,
                                              columnOrdinal++ );


            columnOrdinal = 0;
            NestedClassTable = new MetadataTable( this, "NestedClass", tableOrdinal++ );
            NestedClassNestedClassColumn =
                new MetadataColumnRowIndex( NestedClassTable, "NestedClass", MetadataTableOrdinal.TypeDef,
                                            columnOrdinal++ );
            NestedClassEnclosingClassColumn =
                new MetadataColumnRowIndex( NestedClassTable, "EnclosingClass", MetadataTableOrdinal.TypeDef,
                                            columnOrdinal++ );


            columnOrdinal = 0;
            GenericParamTable = new MetadataTable( this, "GenericParam", tableOrdinal++ );
            GenericParamNumberColumn = new MetadataColumnInt16( GenericParamTable, "Number", columnOrdinal++ );
            GenericParamFlagsColumn = new MetadataColumnInt16( GenericParamTable, "Flags", columnOrdinal++ );
            GenericParamOwnerColumn =
                new MetadataColumnCodedToken( GenericParamTable, "Owner", CodedTokens.TypeOrMethodDef, columnOrdinal++ );
            GenericParamNameColumn = new MetadataColumnString( GenericParamTable, "Name", columnOrdinal++ );


            columnOrdinal = 0;
            MethodSpecTable = new MetadataTable( this, "MethodSpec", tableOrdinal++ );

            MethodSpecMethodColumn =
                new MetadataColumnCodedToken( MethodSpecTable, "Method", CodedTokens.MethodDefOrRef, columnOrdinal++ );
            MethodSpecInstantiationColumn = new MetadataColumnBlob( MethodSpecTable, "Instantiation", columnOrdinal++ );


            columnOrdinal = 0;
            GenericParamConstraintTable = new MetadataTable( this, "GenericParamConstraint", tableOrdinal++ );

            GenericParamConstraintOwnerColumn =
                new MetadataColumnRowIndex( GenericParamConstraintTable, "Owner", MetadataTableOrdinal.GenericParam,
                                            columnOrdinal++ );
            GenericParamConstraintConstraintColumn =
                new MetadataColumnCodedToken( GenericParamConstraintTable, "Constraint", CodedTokens.TypeDefOrRef,
                                              columnOrdinal++ );


            ModuleColumns = new MetadataColumn[]
                                {
                                    ModuleGenerationColumn,
                                    ModuleNameColumn,
                                    ModuleMvidColumn,
                                    ModuleEncIdColumn,
                                    ModuleEncBaseIdColumn,
                                    null
                                };
            ModuleTable.SetColumns( ModuleColumns );


            TypeRefColumns = new MetadataColumn[]
                                 {
                                     TypeRefResolutionScopeColumn,
                                     TypeRefNameColumn,
                                     TypeRefNamespaceColumn,
                                     null
                                 };
            TypeRefTable.SetColumns( TypeRefColumns );


            TypeDefColumns = new MetadataColumn[]
                                 {
                                     TypeDefFlagsColumn,
                                     TypeDefNameColumn,
                                     TypeDefNamespaceColumn,
                                     TypeDefExtendsColumn,
                                     TypeDefFieldListColumn,
                                     TypeDefMethodListColumn,
                                     null
                                 };
            TypeDefTable.SetColumns( TypeDefColumns );


            FieldPtrColumns = new MetadataColumn[]
                                  {
                                      FieldPtrFieldColumn,
                                      null
                                  };
            FieldPtrTable.SetColumns( FieldPtrColumns );


            FieldColumns = new MetadataColumn[]
                               {
                                   FieldFlagsColumn,
                                   FieldNameColumn,
                                   FieldSignatureColumn,
                                   null
                               };
            FieldTable.SetColumns( FieldColumns );


            MethodPtrColumns = new MetadataColumn[]
                                   {
                                       MethodPtrMethodColumn,
                                       null
                                   };
            MethodPtrTable.SetColumns( MethodPtrColumns );


            MethodColumns = new MetadataColumn[]
                                {
                                    MethodRVAColumn,
                                    MethodImplFlagsColumn,
                                    MethodFlagsColumn,
                                    MethodNameColumn,
                                    MethodSignatureColumn,
                                    MethodParamListColumn,
                                    null
                                };
            MethodTable.SetColumns( MethodColumns );


            ParamPtrColumns = new MetadataColumn[]
                                  {
                                      ParamPtrParamColumn,
                                      null
                                  };
            ParamPtrTable.SetColumns( ParamPtrColumns );


            ParamColumns = new MetadataColumn[]
                               {
                                   ParamFlagsColumn,
                                   ParamSequenceColumn,
                                   ParamNameColumn,
                                   null
                               };
            ParamTable.SetColumns( ParamColumns );


            InterfaceImplColumns = new MetadataColumn[]
                                       {
                                           InterfaceImplClassColumn,
                                           InterfaceImplInterfaceColumn,
                                           null
                                       };
            InterfaceImplTable.SetColumns( InterfaceImplColumns );


            MemberRefColumns = new MetadataColumn[]
                                   {
                                       MemberRefClassColumn,
                                       MemberRefNameColumn,
                                       MemberRefSignatureColumn,
                                       null
                                   };
            MemberRefTable.SetColumns( MemberRefColumns );


            ConstantColumns = new MetadataColumn[]
                                  {
                                      ConstantTypeColumn,
                                      ConstantParentColumn,
                                      ConstantValueColumn,
                                      null
                                  };
            ConstantTable.SetColumns( ConstantColumns );


            CustomAttributeColumns = new MetadataColumn[]
                                         {
                                             CustomAttributeParentColumn,
                                             CustomAttributeTypeColumn,
                                             CustomAttributeValueColumn,
                                             null
                                         };
            CustomAttributeTable.SetColumns( CustomAttributeColumns );


            FieldMarshalColumns = new MetadataColumn[]
                                      {
                                          FieldMarshalParentColumn,
                                          FieldMarshalNativeTypeColumn,
                                          null
                                      };
            FieldMarshalTable.SetColumns( FieldMarshalColumns );


            DeclSecurityColumns = new MetadataColumn[]
                                      {
                                          DeclSecurityActionColumn,
                                          DeclSecurityParentColumn,
                                          DeclSecurityPermissionSetColumn,
                                          null
                                      };
            DeclSecurityTable.SetColumns( DeclSecurityColumns );


            ClassLayoutColumns = new MetadataColumn[]
                                     {
                                         ClassLayoutPackingSizeColumn,
                                         ClassLayoutClassSizeColumn,
                                         ClassLayoutParentColumn,
                                         null
                                     };
            ClassLayoutTable.SetColumns( ClassLayoutColumns );


            FieldLayoutColumns = new MetadataColumn[]
                                     {
                                         FieldLayoutOffSetColumn,
                                         FieldLayoutFieldColumn,
                                         null
                                     };
            FieldLayoutTable.SetColumns( FieldLayoutColumns );


            StandAloneSigColumns = new MetadataColumn[]
                                       {
                                           StandAloneSigSignatureColumn,
                                           null
                                       };
            StandAloneSigTable.SetColumns( StandAloneSigColumns );


            EventMapColumns = new MetadataColumn[]
                                  {
                                      EventMapParentColumn,
                                      EventMapEventListColumn,
                                      null
                                  };
            EventMapTable.SetColumns( EventMapColumns );


            EventPtrColumns = new MetadataColumn[]
                                  {
                                      EventPtrEventColumn,
                                      null
                                  };
            EventPtrTable.SetColumns( EventPtrColumns );


            EventColumns = new MetadataColumn[]
                               {
                                   EventEventFlagsColumn,
                                   EventNameColumn,
                                   EventEventTypeColumn,
                                   null
                               };
            EventTable.SetColumns( EventColumns );


            PropertyMapColumns = new MetadataColumn[]
                                     {
                                         PropertyMapParentColumn,
                                         PropertyMapPropertyListColumn,
                                         null
                                     };
            PropertyMapTable.SetColumns( PropertyMapColumns );


            PropertyPtrColumns = new MetadataColumn[]
                                     {
                                         PropertyPtrPropertyColumn,
                                         null
                                     };
            PropertyPtrTable.SetColumns( PropertyPtrColumns );


            PropertyColumns = new MetadataColumn[]
                                  {
                                      PropertyPropFlagsColumn,
                                      PropertyNameColumn,
                                      PropertyTypeColumn,
                                      null
                                  };
            PropertyTable.SetColumns( PropertyColumns );


            MethodSemanticsColumns = new MetadataColumn[]
                                         {
                                             MethodSemanticsSemanticColumn,
                                             MethodSemanticsMethodColumn,
                                             MethodSemanticsAssociationColumn,
                                             null
                                         };
            MethodSemanticsTable.SetColumns( MethodSemanticsColumns );


            MethodImplColumns = new MetadataColumn[]
                                    {
                                        MethodImplClassColumn,
                                        MethodImplMethodBodyColumn,
                                        MethodImplMethodDefDeclarationColumn,
                                        null
                                    };
            MethodImplTable.SetColumns( MethodImplColumns );


            ModuleRefColumns = new MetadataColumn[]
                                   {
                                       ModuleRefNameColumn,
                                       null
                                   };
            ModuleRefTable.SetColumns( ModuleRefColumns );


            TypeSpecColumns = new MetadataColumn[]
                                  {
                                      TypeSpecSignatureColumn,
                                      null
                                  };
            TypeSpecTable.SetColumns( TypeSpecColumns );


            ImplMapColumns = new MetadataColumn[]
                                 {
                                     ImplMapMappingFlagsColumn,
                                     ImplMapMemberForwardedColumn,
                                     ImplMapImportNameColumn,
                                     ImplMapImportScopeColumn,
                                     null
                                 };
            ImplMapTable.SetColumns( ImplMapColumns );


            FieldRVAColumns = new MetadataColumn[]
                                  {
                                      FieldRVARVAColumn,
                                      FieldRVAFieldColumn,
                                      null
                                  };
            FieldRVATable.SetColumns( FieldRVAColumns );


            ENCLogColumns = new MetadataColumn[]
                                {
                                    ENCLogTokenColumn,
                                    ENCLogFuncCodeColumn,
                                    null
                                };
            ENCLogTable.SetColumns( ENCLogColumns );


            ENCMapColumns = new MetadataColumn[]
                                {
                                    ENCMapTokenColumn,
                                    null
                                };
            ENCMapTable.SetColumns( ENCMapColumns );


            AssemblyColumns = new MetadataColumn[]
                                  {
                                      AssemblyHashAlgIdColumn,
                                      AssemblyMajorVersionColumn,
                                      AssemblyMinorVersionColumn,
                                      AssemblyBuildNumberColumn,
                                      AssemblyRevisionNumberColumn,
                                      AssemblyFlagsColumn,
                                      AssemblyPublicKeyColumn,
                                      AssemblyNameColumn,
                                      AssemblyLocaleColumn,
                                      null
                                  };
            AssemblyTable.SetColumns( AssemblyColumns );


            AssemblyProcessorColumns = new MetadataColumn[]
                                           {
                                               AssemblyProcessorProcessorColumn,
                                               null
                                           };
            AssemblyProcessorTable.SetColumns( AssemblyProcessorColumns );


            AssemblyOSColumns = new MetadataColumn[]
                                    {
                                        AssemblyOSOSPlatformIdColumn,
                                        AssemblyOSOSMajorVersionColumn,
                                        AssemblyOSOSMinorVersionColumn,
                                        null
                                    };
            AssemblyOSTable.SetColumns( AssemblyOSColumns );


            AssemblyRefColumns = new MetadataColumn[]
                                     {
                                         AssemblyRefMajorVersionColumn,
                                         AssemblyRefMinorVersionColumn,
                                         AssemblyRefBuildNumberColumn,
                                         AssemblyRefRevisionNumberColumn,
                                         AssemblyRefFlagsColumn,
                                         AssemblyRefPublicKeyOrTokenColumn,
                                         AssemblyRefNameColumn,
                                         AssemblyRefLocaleColumn,
                                         AssemblyRefHashValueColumn,
                                         null
                                     };
            AssemblyRefTable.SetColumns( AssemblyRefColumns );


            AssemblyRefProcessorColumns = new MetadataColumn[]
                                              {
                                                  AssemblyRefProcessorProcessorColumn,
                                                  AssemblyRefProcessorAssemblyRefColumn,
                                                  null
                                              };
            AssemblyRefProcessorTable.SetColumns( AssemblyRefProcessorColumns );


            AssemblyRefOSColumns = new MetadataColumn[]
                                       {
                                           AssemblyRefOSOSPlatformIdColumn,
                                           AssemblyRefOSOSMajorVersionColumn,
                                           AssemblyRefOSOSMinorVersionColumn,
                                           AssemblyRefOSAssemblyRefColumn,
                                           null
                                       };
            AssemblyRefOSTable.SetColumns( AssemblyRefOSColumns );


            FileColumns = new MetadataColumn[]
                              {
                                  FileFlagsColumn,
                                  FileNameColumn,
                                  FileHashValueColumn,
                                  null
                              };
            FileTable.SetColumns( FileColumns );


            ExportedTypeColumns = new MetadataColumn[]
                                      {
                                          ExportedTypeFlagsColumn,
                                          ExportedTypeTypeDefIdColumn,
                                          ExportedTypeTypeNameColumn,
                                          ExportedTypeTypeNamespaceColumn,
                                          ExportedTypeImplementationColumn,
                                          null
                                      };
            ExportedTypeTable.SetColumns( ExportedTypeColumns );


            ManifestResourceColumns = new MetadataColumn[]
                                          {
                                              ManifestResourceOffsetColumn,
                                              ManifestResourceFlagsColumn,
                                              ManifestResourceNameColumn,
                                              ManifestResourceImplementationColumn,
                                              null
                                          };
            ManifestResourceTable.SetColumns( ManifestResourceColumns );


            NestedClassColumns = new MetadataColumn[]
                                     {
                                         NestedClassNestedClassColumn,
                                         NestedClassEnclosingClassColumn,
                                         null
                                     };
            NestedClassTable.SetColumns( NestedClassColumns );


            GenericParamColumns = new MetadataColumn[]
                                      {
                                          GenericParamNumberColumn,
                                          GenericParamFlagsColumn,
                                          GenericParamOwnerColumn,
                                          GenericParamNameColumn,
                                          null
                                      };
            GenericParamTable.SetColumns( GenericParamColumns );


            MethodSpecColumns = new MetadataColumn[]
                                    {
                                        MethodSpecMethodColumn,
                                        MethodSpecInstantiationColumn,
                                        null
                                    };
            MethodSpecTable.SetColumns( MethodSpecColumns );


            GenericParamConstraintColumns = new MetadataColumn[]
                                                {
                                                    GenericParamConstraintOwnerColumn,
                                                    GenericParamConstraintConstraintColumn,
                                                    null
                                                };
            GenericParamConstraintTable.SetColumns( GenericParamConstraintColumns );


            Tables = new[]
                         {
                             ModuleTable,
                             TypeRefTable,
                             TypeDefTable,
                             FieldPtrTable,
                             FieldTable,
                             MethodPtrTable,
                             MethodTable,
                             ParamPtrTable,
                             ParamTable,
                             InterfaceImplTable,
                             MemberRefTable,
                             ConstantTable,
                             CustomAttributeTable,
                             FieldMarshalTable,
                             DeclSecurityTable,
                             ClassLayoutTable,
                             FieldLayoutTable,
                             StandAloneSigTable,
                             EventMapTable,
                             EventPtrTable,
                             EventTable,
                             PropertyMapTable,
                             PropertyPtrTable,
                             PropertyTable,
                             MethodSemanticsTable,
                             MethodImplTable,
                             ModuleRefTable,
                             TypeSpecTable,
                             ImplMapTable,
                             FieldRVATable,
                             ENCLogTable,
                             ENCMapTable,
                             AssemblyTable,
                             AssemblyProcessorTable,
                             AssemblyOSTable,
                             AssemblyRefTable,
                             AssemblyRefProcessorTable,
                             AssemblyRefOSTable,
                             FileTable,
                             ExportedTypeTable,
                             ManifestResourceTable,
                             NestedClassTable,
                             GenericParamTable,
                             MethodSpecTable,
                             GenericParamConstraintTable,
                             null
                         };


            this.Initialize();
        }
    }
}