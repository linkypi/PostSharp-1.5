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

using PostSharp.CodeModel;

#endregion

namespace PostSharp.ModuleReader
{
    /// <summary>
    /// Encapsulates the collection of binary metadata tables.
    /// </summary>
    /// <remarks>
    /// The most part of this class is generated automatically from the
    /// definition of metadata table schema (file <c>MetaDataTables_Construct.cs</c>).
    /// This part is generated from a modified version of the <c>MetaModelColumnDefs.h</c>
    /// file, which ships with SSCLI and cannot be distributed with this project
    /// for license reasons.
    /// </remarks>
    internal sealed partial class MetadataTables
    {
        /// <summary>
        /// PE Image reader.
        /// </summary>
        private readonly ImageReader imageReader;

        /// <summary>
        /// Computes the table addresses. 
        /// </summary>
        /// <remarks>
        /// This method is called by the constructor after all tables and columns
        /// have been constructed.
        /// </remarks>
        private void Initialize()
        {
            unsafe
            {
                byte* tableAddress = (byte*) this.imageReader.GetFirstMetadataTable().ToPointer();
                for ( int i = 0 ; i < this.Tables.Length - 1 ; i++ )
                {
                    this.Tables[i].Address = tableAddress;
                    tableAddress += this.Tables[i].RowSize*this.Tables[i].RowCount;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ImageReader"/>.
        /// </summary>
        public ImageReader ImageReader { get { return this.imageReader; } }

        #region Coded Tokens

        /// <summary>
        /// Lexical scope containing mapping of high-value coden token bits to 
        /// their corresponding <see cref="TokenType"/>.
        /// </summary>
        public static class CodedTokens
        {
#pragma warning disable 1591
            public static readonly TokenType[] TypeDefOrRef = new []
                {
                    TokenType.TypeDef,
                    TokenType.TypeRef,
                    TokenType.TypeSpec
                };

            public static readonly TokenType[] HasConstant = new []
                {
                    TokenType.FieldDef,
                    TokenType.ParamDef,
                    TokenType.Property
                };

            public static readonly TokenType[] HasCustomAttribute = new []
                {
                    TokenType.MethodDef,
                    TokenType.FieldDef,
                    TokenType.TypeRef,
                    TokenType.TypeDef,
                    TokenType.ParamDef,
                    TokenType.InterfaceImpl,
                    TokenType.MemberRef,
                    TokenType.Module,
                    TokenType.Permission,
                    TokenType.Property,
                    TokenType.Event,
                    TokenType.Signature,
                    TokenType.ModuleRef,
                    TokenType.TypeSpec,
                    TokenType.Assembly,
                    TokenType.AssemblyRef,
                    TokenType.File,
                    TokenType.ExportedType,
                    TokenType.ManifestResource,
                    TokenType.GenericParam
                };

            public static readonly TokenType[] HasFieldMarshal = new []
                {
                    TokenType.FieldDef,
                    TokenType.ParamDef,
                };

            public static readonly TokenType[] HasDeclSecurity = new []
                {
                    TokenType.TypeDef,
                    TokenType.MethodDef,
                    TokenType.Assembly
                };

            public static readonly TokenType[] MemberRefParent = new []
                {
                    TokenType.TypeDef,
                    TokenType.TypeRef,
                    TokenType.ModuleRef,
                    TokenType.MethodDef,
                    TokenType.TypeSpec
                };

            public static readonly TokenType[] HasSemantic = new []
                {
                    TokenType.Event,
                    TokenType.Property,
                };

            public static readonly TokenType[] MethodDefOrRef = new []
                {
                    TokenType.MethodDef,
                    TokenType.MemberRef
                };

            public static readonly TokenType[] MemberForwarded = new []
                {
                    TokenType.FieldDef,
                    TokenType.MethodDef
                };

            public static readonly TokenType[] Implementation = new []
                {
                    TokenType.File,
                    TokenType.AssemblyRef,
                    TokenType.ExportedType
                };

            public static readonly TokenType[] CustomAttributeType = new TokenType[]
                {
                    0,
                    0,
                    TokenType.MethodDef,
                    TokenType.MemberRef,
                    0
                };

            public static readonly TokenType[] ResolutionScope = new []
                {
                    TokenType.Module,
                    TokenType.ModuleRef,
                    TokenType.AssemblyRef,
                    TokenType.TypeRef
                };

            public static readonly TokenType[] TypeOrMethodDef = new []
                {
                    TokenType.TypeDef,
                    TokenType.MethodDef
                };

#pragma warning restore 1591
        }

        #endregion
    }
}