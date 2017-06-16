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

#region Using directives

#endregion

namespace PostSharp.ModuleReader
{
    /// <summary>
    /// Enumeration of metadata table columns.
    /// </summary>
    /// <remarks>
    /// Column names in this enumeration are composed of two concatenated parts:
    /// the table name followed by the column name.
    /// </remarks>
    internal enum MetadataColumnOrdinal
    {
#pragma warning disable 1591
        // Table Module
        ModuleGeneration = 0x0,
        ModuleName = 0x1,
        ModuleMvid = 0x2,
        ModuleEncId = 0x3,
        ModuleEncBaseId = 0x4,


        // Table TypeRef
        TypeRefResolutionScope = 0x0,
        TypeRefName = 0x1,
        TypeRefNamespace = 0x2,


        // Table TypeDef
        TypeDefFlags = 0x0,
        TypeDefName = 0x1,
        TypeDefNamespace = 0x2,
        TypeDefExtends = 0x3,
        TypeDefFieldList = 0x4,
        TypeDefMethodList = 0x5,


        // Table FieldPtr
        FieldPtrField = 0x0,


        // Table Field
        FieldFlags = 0x0,
        FieldName = 0x1,
        FieldSignature = 0x2,


        // Table MethodPtr
        MethodPtrMethod = 0x0,


        // Table Method
        MethodRVA = 0x0,
        MethodImplFlags = 0x1,
        MethodFlags = 0x2,
        MethodName = 0x3,
        MethodSignature = 0x4,
        MethodParamList = 0x5,


        // Table ParamPtr
        ParamPtrParam = 0x0,


        // Table Param
        ParamFlags = 0x0,
        ParamSequence = 0x1,
        ParamName = 0x2,


        // Table InterfaceImpl
        InterfaceImplClass = 0x0,
        InterfaceImplInterface = 0x1,


        // Table MemberRef
        MemberRefClass = 0x0,
        MemberRefName = 0x1,
        MemberRefSignature = 0x2,


        // Table Constant
        ConstantType = 0x0,
        ConstantParent = 0x1,
        ConstantValue = 0x2,


        // Table CustomAttribute
        CustomAttributeParent = 0x0,
        CustomAttributeType = 0x1,
        CustomAttributeValue = 0x2,


        // Table FieldMarshal
        FieldMarshalParent = 0x0,
        FieldMarshalNativeType = 0x1,


        // Table DeclSecurity
        DeclSecurityAction = 0x0,
        DeclSecurityParent = 0x1,
        DeclSecurityPermissionSet = 0x2,


        // Table ClassLayout
        ClassLayoutPackingSize = 0x0,
        ClassLayoutClassSize = 0x1,
        ClassLayoutParent = 0x2,


        // Table FieldLayout
        FieldLayoutOffSet = 0x0,
        FieldLayoutField = 0x1,


        // Table StandAloneSig
        StandAloneSigSignature = 0x0,


        // Table EventMap
        EventMapParent = 0x0,
        EventMapEventList = 0x1,


        // Table EventPtr
        EventPtrEvent = 0x0,


        // Table Event
        EventEventFlags = 0x0,
        EventName = 0x1,
        EventEventType = 0x2,


        // Table PropertyMap
        PropertyMapParent = 0x0,
        PropertyMapPropertyList = 0x1,


        // Table PropertyPtr
        PropertyPtrProperty = 0x0,


        // Table Property
        PropertyPropFlags = 0x0,
        PropertyName = 0x1,
        PropertyType = 0x2,


        // Table MethodSemantics
        MethodSemanticsSemantic = 0x0,
        MethodSemanticsMethod = 0x1,
        MethodSemanticsAssociation = 0x2,


        // Table MethodImpl
        MethodImplClass = 0x0,
        MethodImplMethodBody = 0x1,
        MethodImplMethodDefDeclaration = 0x2,


        // Table ModuleRef
        ModuleRefName = 0x0,


        // Table TypeSpec
        TypeSpecSignature = 0x0,


        // Table ImplMap
        ImplMapMappingFlags = 0x0,
        ImplMapMemberForwarded = 0x1,
        ImplMapImportName = 0x2,
        ImplMapImportScope = 0x3,


        // Table FieldRVA
        FieldRVARVA = 0x0,
        FieldRVAField = 0x1,


        // Table ENCLog
        ENCLogToken = 0x0,
        ENCLogFuncCode = 0x1,


        // Table ENCMap
        ENCMapToken = 0x0,


        // Table Assembly
        AssemblyHashAlgId = 0x0,
        AssemblyMajorVersion = 0x1,
        AssemblyMinorVersion = 0x2,
        AssemblyBuildNumber = 0x3,
        AssemblyRevisionNumber = 0x4,
        AssemblyFlags = 0x5,
        AssemblyPublicKey = 0x6,
        AssemblyName = 0x7,
        AssemblyLocale = 0x8,


        // Table AssemblyProcessor
        AssemblyProcessorProcessor = 0x0,


        // Table AssemblyOS
        AssemblyOSOSPlatformId = 0x0,
        AssemblyOSOSMajorVersion = 0x1,
        AssemblyOSOSMinorVersion = 0x2,


        // Table AssemblyRef
        AssemblyRefMajorVersion = 0x0,
        AssemblyRefMinorVersion = 0x1,
        AssemblyRefBuildNumber = 0x2,
        AssemblyRefRevisionNumber = 0x3,
        AssemblyRefFlags = 0x4,
        AssemblyRefPublicKeyOrToken = 0x5,
        AssemblyRefName = 0x6,
        AssemblyRefLocale = 0x7,
        AssemblyRefHashValue = 0x8,


        // Table AssemblyRefProcessor
        AssemblyRefProcessorProcessor = 0x0,
        AssemblyRefProcessorAssemblyRef = 0x1,


        // Table AssemblyRefOS
        AssemblyRefOSOSPlatformId = 0x0,
        AssemblyRefOSOSMajorVersion = 0x1,
        AssemblyRefOSOSMinorVersion = 0x2,
        AssemblyRefOSAssemblyRef = 0x3,


        // Table File
        FileFlags = 0x0,
        FileName = 0x1,
        FileHashValue = 0x2,


        // Table ExportedType
        ExportedTypeFlags = 0x0,
        ExportedTypeTypeDefId = 0x1,
        ExportedTypeTypeName = 0x2,
        ExportedTypeTypeNamespace = 0x3,
        ExportedTypeImplementation = 0x4,


        // Table ManifestResource
        ManifestResourceOffset = 0x0,
        ManifestResourceFlags = 0x1,
        ManifestResourceName = 0x2,
        ManifestResourceImplementation = 0x3,


        // Table NestedClass
        NestedClassNestedClass = 0x0,
        NestedClassEnclosingClass = 0x1,


        // Table GenericParam
        GenericParamNumber = 0x0,
        GenericParamFlags = 0x1,
        GenericParamOwner = 0x2,
        GenericParamName = 0x3,
        GenericParamKind = 0x4,


        // Table MethodSpec
        MethodSpecMethod = 0x0,
        MethodSpecInstantiation = 0x1,


        // Table GenericParamConstraint
        GenericParamConstraintOwner = 0x0,
        GenericParamConstraintConstraint = 0x1
#pragma warning restore 1591
    }
}