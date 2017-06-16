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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel.MarshalTypes
{
    /// <summary>
    /// Represents the type of an intrinsic unmanaged scalar.
    /// </summary>
    public sealed class IntrinsicMarshalType : MarshalType
    {
        /// <summary>
        /// Type.
        /// </summary>
        private readonly UnmanagedType type;

        /// <summary>
        /// Maps types to their name.
        /// </summary>
        private static readonly Dictionary<UnmanagedType, string> typeToName = new Dictionary<UnmanagedType, string>( 30 );

        /// <summary>
        /// Initializes the <see cref="IntrinsicMarshalType"/> type.
        /// </summary>
        [SuppressMessage( "Microsoft.Performance",
            "CA1810", Justification="The type cannot be initialized only at declaration." )]
        static IntrinsicMarshalType()
        {
            typeToName.Add( UnmanagedType.AnsiBStr, "ansi bstr" );
            typeToName.Add( UnmanagedType.AsAny, "as any" );
            typeToName.Add( UnmanagedType.Bool, "bool" );
            typeToName.Add( UnmanagedType.BStr, "bstr" );
            typeToName.Add( UnmanagedType.Currency, "currency" );
            typeToName.Add( UnmanagedType.Error, "error" );
            typeToName.Add( UnmanagedType.I1, "int8" );
            typeToName.Add( UnmanagedType.I2, "int16" );
            typeToName.Add( UnmanagedType.I4, "int32" );
            typeToName.Add( UnmanagedType.I8, "int64" );
            typeToName.Add( UnmanagedType.IDispatch, "idispatch" );
            typeToName.Add( UnmanagedType.Interface, "interface" );
            typeToName.Add( UnmanagedType.IUnknown, "iunknown" );
            typeToName.Add( UnmanagedType.LPStr, "lpstr" );
            typeToName.Add( UnmanagedType.LPStruct, "lpstruct" );
            typeToName.Add( UnmanagedType.LPTStr, "lptstr" );
            typeToName.Add( UnmanagedType.LPWStr, "lpwstr" );
            typeToName.Add( UnmanagedType.R4, "float32" );
            typeToName.Add( UnmanagedType.R8, "float64" );
            typeToName.Add( UnmanagedType.Struct, "struct" );
            typeToName.Add( UnmanagedType.SysInt, "int" );
            typeToName.Add( UnmanagedType.SysUInt, "uint" );
            typeToName.Add( UnmanagedType.TBStr, "tbstr" );
            typeToName.Add( UnmanagedType.U1, "unsigned int8" );
            typeToName.Add( UnmanagedType.U2, "unsigned int16" );
            typeToName.Add( UnmanagedType.U4, "unsigned int32" );
            typeToName.Add( UnmanagedType.U8, "unsigned int64" );
            typeToName.Add( UnmanagedType.VariantBool, "variant bool" );
            typeToName.Add( UnmanagedType.VBByRefStr, "byvalstr" );
            typeToName.Add( UnmanagedType.FunctionPtr, "method" );
        }

        /// <summary>
        /// Initializes a new <see cref="IntrinsicMarshalType"/>.
        /// </summary>
        /// <param name="type">The instrinsic type.</param>
        public IntrinsicMarshalType( UnmanagedType type )
        {
            this.type = type;
        }

        /// <summary>
        /// Gets the instrinsic type.
        /// </summary>
        public UnmanagedType Type { get { return this.type; } }


        /// <inheritdoc />
        internal override void WriteILReference( ILWriter writer )
        {
            WriteILReference( this.type, writer );
        }


        /// <summary>
        /// Writes the name of an intrinsic type.
        /// </summary>
        /// <param name="unmanagedType">An intrinsic type.</param>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        internal static void WriteILReference( UnmanagedType unmanagedType, ILWriter writer )
        {
            writer.WriteKeyword( typeToName[unmanagedType] );
        }
    }
}