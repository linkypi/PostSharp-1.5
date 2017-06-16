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
    /// Enumeration of metadata tables.
    /// </summary>
    internal enum MetadataTableOrdinal
    {
#pragma warning disable 1591
        Module = 0x0,
        TypeRef = 0x1,
        TypeDef = 0x2,
        FieldPtr = 0x3,
        Field = 0x4,
        MethodPtr = 0x5,
        Method = 0x6,
        ParamPtr = 0x7,
        Param = 0x8,
        InterfaceImpl = 0x9,
        MemberRef = 0xa,
        Constant = 0xb,
        CustomAttribute = 0xc,
        FieldMarshal = 0xd,
        DeclSecurity = 0xe,
        ClassLayout = 0xf,
        FieldLayout = 0x10,
        StandAloneSig = 0x11,
        EventMap = 0x12,
        EventPtr = 0x13,
        Event = 0x14,
        PropertyMap = 0x15,
        PropertyPtr = 0x16,
        Property = 0x17,
        MethodSemantics = 0x18,
        MethodImpl = 0x19,
        ModuleRef = 0x1a,
        TypeSpec = 0x1b,
        ImplMap = 0x1c,
        FieldRVA = 0x1d,
        ENCLog = 0x1e,
        ENCMap = 0x1f,
        Assembly = 0x20,
        AssemblyProcessor = 0x21,
        AssemblyOS = 0x22,
        AssemblyRef = 0x23,
        AssemblyRefProcessor = 0x24,
        AssemblyRefOS = 0x25,
        File = 0x26,
        ExportedType = 0x27,
        ManifestResource = 0x28,
        NestedClass = 0x29,
        GenericParam = 0x2a,
        MethodSpec = 0x2b,
        GenericParamConstraint = 0x2c
#pragma warning restore 1591
    }
}