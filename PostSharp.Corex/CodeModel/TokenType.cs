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

using System.Diagnostics.CodeAnalysis;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Specifies the type of metadata token.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Module definition (<see cref="ModuleDeclaration"/>).
        /// </summary>
        Module = 0x00000000,

        /// <summary>
        /// Type reference (<see cref="TypeRefDeclaration"/>).
        /// </summary>
        TypeRef = 0x01000000,

        /// <summary>
        /// Type definition (<see cref="TypeDefDeclaration"/>).
        /// </summary>
        TypeDef = 0x02000000,

        /// <summary>
        /// Field definition (<see cref="FieldDefDeclaration"/>).
        /// </summary>
        FieldDef = 0x04000000,

        /// <summary>
        /// Method definition (<see cref="MethodDefDeclaration"/>).
        /// </summary>
        MethodDef = 0x06000000,

        /// <summary>
        /// Parameter definition (<see cref="ParameterDeclaration"/>).
        /// </summary>
        [SuppressMessage( "Microsoft.Naming", "CA1704",
            Justification = "Should be compatible with external code." )] ParamDef = 0x08000000, //

        /// <summary>
        /// Interface implementation (<see cref="MethodDefDeclaration.InterfaceImplementations"/>).
        /// </summary>
        InterfaceImpl = 0x09000000,

        /// <summary>
        /// References to fields or methods (<see cref="FieldRefDeclaration"/>,
        /// <see cref="MethodRefDeclaration"/>).
        /// </summary>
        MemberRef = 0x0a000000,

        /// <summary>
        /// Custom attribute (<see cref="CustomAttributeDeclaration"/>).
        /// </summary>
        CustomAttribute = 0x0c000000,

        /// <summary>
        /// Permission set (<see cref="PermissionSetDeclaration"/>).
        /// </summary>
        Permission = 0x0e000000,

        /// <summary>
        /// Stand-alone signature (<see cref="StandaloneSignatureDeclaration"/>).
        /// </summary>
        Signature = 0x11000000,

        /// <summary>
        /// Event definition (<see cref="EventDeclaration"/>).
        /// </summary>
        Event = 0x14000000,

        /// <summary>
        /// Property definition (<see cref="PropertyDeclaration"/>).
        /// </summary>
        Property = 0x17000000,

        /// <summary>
        /// Method semantic (<see cref="MethodSemanticDeclaration"/>).
        /// </summary>
        MethodSemantic = 0x18000000,

        /// <summary>
        /// Method implementation (<see cref="MethodImplementationDeclaration"/>).
        /// </summary>
        MethodImpl = 0x19000000,

        /// <summary>
        /// Module reference (<see cref="ModuleRefDeclaration"/>).
        /// </summary>
        ModuleRef = 0x1a000000,

        /// <summary>
        /// Type specification (<see cref="TypeSpecDeclaration"/>).
        /// </summary>
        TypeSpec = 0x1b000000,

        /// <summary>
        /// Assembly manifest (<see cref="AssemblyManifestDeclaration"/>).
        /// </summary>
        Assembly = 0x20000000,

        /// <summary>
        /// Assembly reference (<see cref="AssemblyRefDeclaration"/>).
        /// </summary>
        AssemblyRef = 0x23000000,

        /// <summary>
        /// External file reference (<see cref="ManifestFileDeclaration"/>).
        /// </summary>
        File = 0x26000000,

        /// <notSupported />
        ExportedType = 0x27000000,

        /// <summary>
        /// Resource declaration or reference (<see cref="ManifestResourceDeclaration"/>).
        /// </summary>
        ManifestResource = 0x28000000,

        /// <summary>
        /// Generic parameter (<see cref="GenericParameterDeclaration"/>).
        /// </summary>
        [SuppressMessage( "Microsoft.Naming", "CA1704",
            Justification = "Should be compatible with external code." )] GenericParam = 0x2a000000,

        /// <summary>
        /// Synonym for <see cref="TokenType.GenericParam"/>).
        /// </summary>
        [SuppressMessage( "Microsoft.Naming", "CA1704",
            Justification = "Should be compatible with external code." )] GenericPar = GenericParam,

        /// <summary>
        /// Method specification (<see cref="MethodSpecDeclaration"/>).
        /// </summary>
        MethodSpec = 0x2b000000,

        /// <summary>
        /// Constraint on a generic parameter (<see cref="GenericParameterDeclaration"/>.<see cref="GenericParameterDeclaration.Constraints"/>.
        /// </summary>
        [SuppressMessage( "Microsoft.Naming", "CA1704",
            Justification = "Should be compatible with external code." )] GenericParamConstraint = 0x2c000000,

        /// <summary>
        /// User string (<see cref="LiteralString"/>).
        /// </summary>
        /// <remarks>
        /// The low bytes of string tokens represent the string RVA.
        /// </remarks>
        String = 0x70000000,

        // Leave the current on the high end value. This does not correspond to metadata table

        /// <internal />
        /// <summary>
        /// Custom string (<see cref="CustomStringDeclaration"/>).
        /// </summary>
        CustomString = 0x2d000000,

        /// <summary>
        /// Non-linked declaration (see <see cref="MetadataDeclarationTables.AddWeaklyReferencedDeclaration"/>.
        /// </summary>
        WeaklyReferencedDeclaration = 0x2e000000,

        /// <summary>
        /// Number of token types that are represented as a table in <see cref="MetadataDeclarationTables"/>.
        /// </summary>
        TableCount = 0x2f000000
    }
}