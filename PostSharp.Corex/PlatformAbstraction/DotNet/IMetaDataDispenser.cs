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

#if !MONO

#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace PostSharp.PlatformAbstraction.DotNet
{
    /// <exclude />
    /// <summary>
    /// Defines the IMetadataDispenser COM interface.
    /// </summary>
    [Guid( "B81FF171-20F3-11d2-8DCC-00A0C9B09C19" )]
    [ComImport]
    [InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
    internal interface IMetadataDispenser
    {
        int DefineScope( // Return code.
            ref Guid rclsid, // [in] What version to create.
            int dwCreateFlags, // [in] Flags on the create.
            ref Guid riid, // [in] The interface desired.
            out IntPtr ppIUnk ); // [out] Return interface on success.

        int OpenScope( // Return code.
            [MarshalAs( UnmanagedType.LPWStr )] string szScope, // [in] The scope to open.
            int dwOpenFlags, // [in] Open mode flags.
            ref Guid riid, // [in] The interface desired.
            out IntPtr ppIUnk ); // [out] Return interface on success.

        int OpenScopeOnMemory( // Return code.
            IntPtr pData, // [in] Location of scope data.
            uint cbData, // [in] Size of the data pointed to by pData.
            int dwOpenFlags, // [in] Open mode flags.
            ref Guid riid, // [in] The interface desired.
            out IntPtr ppIUnk ); // [out] Return interface on success.
    } ;

    /// <exclude />
    /// <summary>
    /// Defines the IMetadataDispenserEx COM interface.
    /// </summary>
    [Guid( "31BCFCE2-DAFB-11D2-9F81-00C04F79A0A3" )]
    [ComImport]
    [InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
    internal interface IMetadataDispenserEx //: IMetadataDispenser
    {
        int DefineScope( // Return code.
            ref Guid rclsid, // [in] What version to create.
            int dwCreateFlags, // [in] Flags on the create.
            ref Guid riid, // [in] The interface desired.
            out IntPtr ppIUnk ); // [out] Return interface on success.

        int OpenScope( // Return code.
            [MarshalAs( UnmanagedType.LPWStr )] string szScope, // [in] The scope to open.
            int dwOpenFlags, // [in] Open mode flags.
            ref Guid riid, // [in] The interface desired.
            out IntPtr ppIUnk ); // [out] Return interface on success.

        int OpenScopeOnMemory( // Return code.
            IntPtr pData, // [in] Location of scope data.
            uint cbData, // [in] Size of the data pointed to by pData.
            int dwOpenFlags, // [in] Open mode flags.
            ref Guid riid, // [in] The interface desired.
            out IntPtr ppIUnk ); // [out] Return interface on success.

        int SetOption( // Return code.
            ref Guid optionid, // [in] GUID for the option to be set.
            VariantWrapper value ); // [in] Value to which the option is to be set.

        int GetOption( // Return code.
            ref Guid optionid, // [in] GUID for the option to be set.
            VariantWrapper pvalue ); // [out] Value to which the option is currently set.

        int OpenScopeOnITypeInfo( // Return code.
            IntPtr pITI, // [in] ITypeInfo to open.
            int dwOpenFlags, // [in] Open mode flags.
            ref Guid riid, // [in] The interface desired.
            out IntPtr ppIUnk ); // [out] Return interface on success.

        int GetCORSystemDirectory( // Return code.
            [MarshalAs( UnmanagedType.LPWStr )] StringBuilder szBuffer, // [out] Buffer for the directory name
            int cchBuffer, // [in] Size of the buffer
            out int pchBuffer ); // [OUT] Number of characters returned

        int FindAssembly( // S_OK or error
            [MarshalAs( UnmanagedType.LPWStr )] string szAppBase, // [IN] optional - can be NULL
            [MarshalAs( UnmanagedType.LPWStr )] string szPrivateBin, // [IN] optional - can be NULL
            [MarshalAs( UnmanagedType.LPWStr )] string szGlobalBin, // [IN] optional - can be NULL
            [MarshalAs( UnmanagedType.LPWStr )] string szAssemblyName,
            // [IN] required - the current is the assembly you are requesting
            [MarshalAs( UnmanagedType.LPWStr )] string szName, // [OUT] buffer - to hold name 
            int cchName, // [IN] the name buffer's size
            out int pcName ); // [OUT] the number of characters returend in the buffer

        int FindAssemblyModule( // S_OK or error
            [MarshalAs( UnmanagedType.LPWStr )] string szAppBase, // [IN] optional - can be NULL
            [MarshalAs( UnmanagedType.LPWStr )] string szPrivateBin, // [IN] optional - can be NULL
            [MarshalAs( UnmanagedType.LPWStr )] string szGlobalBin, // [IN] optional - can be NULL
            [MarshalAs( UnmanagedType.LPWStr )] string szAssemblyName,
            // [IN] required - the current is the assembly you are requesting
            [MarshalAs( UnmanagedType.LPWStr )] string szModuleName, // [IN] required - the name of the module
            [MarshalAs( UnmanagedType.LPWStr )] StringBuilder szName, // [OUT] buffer - to hold name 
            int cchName, // [IN]  the name buffer's size
            out int pcName ); // [OUT] the number of characters returend in the buffer
    }
}

#endif