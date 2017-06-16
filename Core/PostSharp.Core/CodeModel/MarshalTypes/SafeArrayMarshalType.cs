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
    /// Represents the type of an unmanaged Safe Array.
    /// </summary>
    public sealed class SafeArrayMarshalType : MarshalType
    {
        /// <summary>
        /// Type of array elements.
        /// </summary>
        private readonly VarEnum elementType;

        /// <summary>
        /// Maps a <see cref="VarEnum"/> to its IL name.
        /// </summary>
        private static readonly Dictionary<VarEnum, string> varNumToName =
            new Dictionary<VarEnum, string>( 20 );

        /// <summary>
        /// Initializes the <see cref="SafeArrayMarshalType"/> type.
        /// </summary>
        [SuppressMessage( "Microsoft.Performance",
            "CA1810", Justification="The type cannot be initialized only at declaration." )]
        static SafeArrayMarshalType()
        {
            varNumToName.Add( VarEnum.VT_NULL, "null" );
            varNumToName.Add( VarEnum.VT_VARIANT, "variant" );
            varNumToName.Add( VarEnum.VT_CY, "currency" );
            varNumToName.Add( VarEnum.VT_VOID, "void" );
            varNumToName.Add( VarEnum.VT_BOOL, "bool" );
            varNumToName.Add( VarEnum.VT_I1, "int8" );
            varNumToName.Add( VarEnum.VT_I2, "int16" );
            varNumToName.Add( VarEnum.VT_I4, "int32" );
            varNumToName.Add( VarEnum.VT_I8, "int64" );
            varNumToName.Add( VarEnum.VT_R4, "float32" );
            varNumToName.Add( VarEnum.VT_R8, "float64" );
            varNumToName.Add( VarEnum.VT_UI1, "unsigned int8" );
            varNumToName.Add( VarEnum.VT_UI2, "unsigned int16" );
            varNumToName.Add( VarEnum.VT_UI4, "unsigned int32" );
            varNumToName.Add( VarEnum.VT_UI8, "unsigned int64" );
            /*  varNumToName.Add(VarEnum.VT_I2, "*" 
			varNumToName.Add(VarEnum.VT_I2, variantType "[" "]"  );
			varNumToName.Add(VarEnum.VT_I2, variantType "vector"  );
			varNumToName.Add(VarEnum.VT_I2, variantType "&"  ); */
            varNumToName.Add( VarEnum.VT_DECIMAL, "decimal" );
            varNumToName.Add( VarEnum.VT_DATE, "date" );
            varNumToName.Add( VarEnum.VT_BSTR, "bstr" );
            varNumToName.Add( VarEnum.VT_LPSTR, "lpstr" );
            varNumToName.Add( VarEnum.VT_LPWSTR, "lpwstr" );
            varNumToName.Add( VarEnum.VT_UNKNOWN, "iunknown" );
            varNumToName.Add( VarEnum.VT_DISPATCH, "idispatch" );
            varNumToName.Add( VarEnum.VT_SAFEARRAY, "safearray" );
            varNumToName.Add( VarEnum.VT_INT, "int" );
            varNumToName.Add( VarEnum.VT_UINT, "unsigned int" );
            varNumToName.Add( VarEnum.VT_ERROR, "error" );
            varNumToName.Add( VarEnum.VT_HRESULT, "hresult" );
            varNumToName.Add( VarEnum.VT_CARRAY, "carray" );
            varNumToName.Add( VarEnum.VT_USERDEFINED, "userdefined" );
            varNumToName.Add( VarEnum.VT_RECORD, "record" );
            varNumToName.Add( VarEnum.VT_FILETIME, "filetime" );
            varNumToName.Add( VarEnum.VT_BLOB, "blob" );
            varNumToName.Add( VarEnum.VT_STREAM, "stream" );
            varNumToName.Add( VarEnum.VT_STORAGE, "storage" );
            varNumToName.Add( VarEnum.VT_STREAMED_OBJECT, "streamed_object" );
            varNumToName.Add( VarEnum.VT_STORED_OBJECT, "stored_object" );
            varNumToName.Add( VarEnum.VT_BLOB_OBJECT, "blob_object" );
            varNumToName.Add( VarEnum.VT_CF, "cf" );
            varNumToName.Add( VarEnum.VT_CLSID, "clsid" );
        }

        /// <summary>
        /// Initializes a new <see cref="SafeArrayMarshalType"/>.
        /// </summary>
        /// <param name="elementType">Type of array elements.</param>
        public SafeArrayMarshalType( VarEnum elementType )
        {
            this.elementType = elementType;
        }

        /// <summary>
        /// Gets the type of array elements.
        /// </summary>
        public VarEnum ElementType { get { return this.elementType; } }


        /// <inheritdoc />
        internal override void WriteILReference( ILWriter writer )
        {
            writer.WriteKeyword( "safearray" );
            if ( this.elementType != VarEnum.VT_EMPTY )
            {
                writer.WriteKeyword( varNumToName[this.elementType] );
            }
        }
    }
}