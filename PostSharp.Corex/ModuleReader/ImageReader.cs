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
using System.Runtime.InteropServices;
using System.Text;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Collections;

namespace PostSharp.ModuleReader
{
    /// <summary>
    /// Reads PE images.
    /// </summary>
    internal sealed class ImageReader
    {
        #region ImageReader

        #region Fields

        private readonly bool isImageMapped;

        /// <summary>
        /// Location of the PE DOS Header (first byte of the image).
        /// </summary>
        private readonly unsafe DosHeader* dosHeader;

        private readonly unsafe PeNtHeader* ntHeader;

        private readonly unsafe OptionalHeader32* optionalHeader32;

        private readonly unsafe OptionalHeader64* optionalHeader64;

        private readonly unsafe CorHeader* corHeader;

        private readonly unsafe SectionHeader* firstSectionHeader;


        /// <summary>
        /// Location of the String heap.
        /// </summary>
        private readonly unsafe byte* stringHeap;

        /// <summary>
        /// Location of the Blob heap.
        /// </summary>
        private readonly unsafe byte* blobHeap;

        /// <summary>
        /// Location of the Guid heap.
        /// </summary>
        private readonly unsafe byte* guidHeap;

        /// <summary>
        /// Location of the UserString heap.
        /// </summary>
        private readonly unsafe byte* userStringHeap;

        private readonly unsafe ResourceDirectory* resourceTable;

        private readonly unsafe byte* managedResources;


        /// <summary>
        /// Location of the .NET Metadata Header.
        /// </summary>
        private readonly unsafe CorMetadataTablesHeader* metadataHeader;

        /// <summary>
        /// Maps a metadata table index to its ordinal in the current image.
        /// </summary>
        private readonly int[] metadataTableMap;

        /// <summary>
        /// Stores the number of records in each metadata table.
        /// </summary>
        private readonly int[] metadataTableSizes;

        /// <summary>
        /// Location of the first metadata table.
        /// </summary>
        private readonly IntPtr firstMetadataTable;

        /// <summary>
        /// Cache of strings (maps an offset to a string).
        /// </summary>
        private readonly Dictionary<int, string> strings = new Dictionary<int, string>( 1024 );

        /// <summary>
        /// Cache of user strings (maps a token to a string).
        /// </summary>
        private readonly Dictionary<MetadataToken, char[]> userStrings = new Dictionary<MetadataToken, char[]>();

        /// <summary>
        /// Variant of the PE format.
        /// </summary>
        private readonly PortableExecutableVariant peVariant;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="ImageReader"/>.
        /// </summary>
        /// <param name="baseAddress">Base address of the assembly image.</param>
        /// <param name="isMapped"><b>true</b> is the PE image is already mapped into memory,
        /// <b>false</b> if the in-memory image is identical to the on-disk image.</param>
        public ImageReader( IntPtr baseAddress, bool isMapped )
        {
			if ( baseAddress == IntPtr.Zero )
				throw new ArgumentNullException("baseAddress");
			
            unsafe
            {
                Trace.ImageReader.WriteLine( "Reading image at address {0:x}, isMapped is {1}.",
                                             baseAddress, isMapped );
                this.isImageMapped = isMapped;
                this.dosHeader = (DosHeader*) baseAddress.ToPointer();

                // Some verifications.
                if ( this.dosHeader->Magic != 0x5a4d )
                {
                    throw new BadImageFormatException( "Invalid DOS Header magic number." );
                }

                this.ntHeader = (PeNtHeader*) ((byte*) this.dosHeader + this.dosHeader->NewExeHeaderRva);

                if ( this.ntHeader->Signature != 0x00004550 )
                {
                    throw new BadImageFormatException( "Invalid NT Header signature." );
                }

                this.peVariant = ((OptionalHeader32*) ((byte*) this.ntHeader + sizeof(PeNtHeader)))->Magic;
                Trace.InstructionReader.WriteLine( "PE kind is: {0}", peVariant );

                switch ( this.peVariant )
                {
                    case PortableExecutableVariant.PE32:
                        this.optionalHeader32 = (OptionalHeader32*) ((byte*) this.ntHeader + sizeof(PeNtHeader));
                        this.firstSectionHeader = &(this.optionalHeader32->FirstSectionHeader);
                        this.corHeader = (CorHeader*) this.RvaToPointer( this.optionalHeader32->CliHeaderRVA );
                        break;

                    case PortableExecutableVariant.PE32Plus:
                        this.optionalHeader64 = (OptionalHeader64*) ((byte*) this.ntHeader + sizeof(PeNtHeader));
                        this.firstSectionHeader = &(this.optionalHeader64->FirstSectionHeader);
                        this.corHeader = (CorHeader*) this.RvaToPointer( this.optionalHeader64->CliHeaderRVA );
                        break;

                    default:
                        throw new BadImageFormatException( "Invalid Optional Header Magic." );
                }

                if ( this.corHeader->Cb != sizeof(CorHeader) )
                {
                    throw new BadImageFormatException( "Invalid size of CLI header." );
                }

                this.managedResources = this.RvaToPointer( this.corHeader->Resources.Rva );


                // Look for .NET streams
                CorMetadataRoot1* root1 = (CorMetadataRoot1*) this.RvaToPointer( this.corHeader->Metadata.Rva );
                CorMetadataRoot2* root2 = CorMetadataRoot1.GetMetadataRoot2( root1, this );

                Trace.ImageReader.WriteLine( "Reading PE image version {0}.{1} [{2}].",
                                             root1->MajorVersion, root1->MinorVersion,
                                             CorMetadataRoot1.GetVersionString( root1 ) );

                CorStreamHeader* streamHeader = &root2->Streams;

                for ( int i = 0; i < root2->StreamNumber; i++ )
                {
                    string name = CorStreamHeader.GetName( streamHeader );
                    byte* ptr = ((byte*) root1 + streamHeader->Offset);

                    Trace.ImageReader.WriteLine( "Found the COR stream named '{0}' at location 0x{1:x}.",
                                                 name, (IntPtr) streamHeader );

                    switch ( name )
                    {
                        case "#~":
                        case "#-":
                            this.metadataHeader = (CorMetadataTablesHeader*) ptr;
                            break;

                        case "#Strings":
                            this.stringHeap = ptr;
                            break;

                        case "#GUID":
                            this.guidHeap = ptr;
                            break;

                        case "#Blob":
                            this.blobHeap = ptr;
                            break;

                        case "#US":
                            this.userStringHeap = ptr;
                            break;

                        default:
                            throw new BadImageFormatException( string.Format( "Unexpected COR stream named '{0}'.", name ) );
                    }

                    streamHeader = CorStreamHeader.GetNext( streamHeader, this );
                }

                // Ensure that all heaps have been found.
                if ( metadataHeader == null )
                {
                    throw new BadImageFormatException( "Cannot find the metadata header section in the PE file." );
                }

                // Build the map of metadata tables.
                this.metadataTableMap = CorMetadataTablesHeader.GetTableMap( metadataHeader );

                // Read the size of metadata tables.
                this.metadataTableSizes = new int[CorMetadataTablesHeader.CountTables( metadataHeader )];
                for ( int i = 0; i < this.metadataTableSizes.Length; i++ )
                {
                    this.metadataTableSizes[i] = (int) CorMetadataTablesHeader.CountRows( metadataHeader, i );
                    Trace.ImageReader.WriteLine( "The metadata table {0} has {1} row(s).",
                                                 (MetadataTableOrdinal) i, this.metadataTableSizes[i] );
                }

                // Computes the location of the first metadata table.
                this.firstMetadataTable = CorMetadataTablesHeader.GetFirstTable( metadataHeader );

                // Compute the location of the resource table;

                DataDirectory dataDirectory;
                switch ( this.peVariant )
                {
                    case PortableExecutableVariant.PE32:
                        dataDirectory = this.optionalHeader32->ResourceTable;
                        break;

                    case PortableExecutableVariant.PE32Plus:
                        dataDirectory = this.optionalHeader64->ResourceTable;
                        break;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException( this.peVariant,
                                                                                      "this.peVariant" );
                }

                if ( dataDirectory.Size > 0 )
                {
                    this.resourceTable = (ResourceDirectory*) this.RvaToPointer( dataDirectory.Rva );
                }
            }
        }

        #region Get information about metadata

        /// <summary>
        /// Gets a bit mask determining which heaps are large.
        /// </summary>
        public HeapSizes HeapSizes
        {
            get
            {
                unsafe
                {
                    return this.metadataHeader->HeapSizes;
                }
            }
        }

        /// <summary>
        /// Gets the number of rows of a givem metadata table.
        /// </summary>
        /// <param name="table">Identifies the metadata table.</param>
        /// <returns>The number of rows in <paramref name="table"/>.</returns>
        public int GetMetadataTableSize( MetadataTableOrdinal table )
        {
            int mappedOrdinal = this.metadataTableMap[(int) table];
            if ( mappedOrdinal < 0 )
            {
                return 0;
            }
            else
            {
                return this.metadataTableSizes[mappedOrdinal];
            }
        }


        /// <summary>
        /// Gets a pointer to the first metadata table.
        /// </summary>
        /// <returns>A pointer to the first metadata table.</returns>
        public IntPtr GetFirstMetadataTable()
        {
            return this.firstMetadataTable;
        }

        #endregion

        #region Get some header fields

        /// <summary>
        /// Gets the StackReserveSize field of the Optional header.
        /// </summary>
        public ulong StackReserve
        {
            get
            {
                unsafe
                {
                    switch ( this.peVariant )
                    {
                        case PortableExecutableVariant.PE32:
                            return this.optionalHeader32->StackReserveSize;

                        case PortableExecutableVariant.PE32Plus:
                            return this.optionalHeader64->StackReserveSize;

                        default:
                            throw ExceptionHelper.CreateInvalidEnumerationValueException( this.peVariant,
                                                                                          "this.peVariant" );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Subsystem field of the Optional header.
        /// </summary>
        public ushort Subsystem
        {
            get
            {
                unsafe
                {
                    switch ( this.peVariant )
                    {
                        case PortableExecutableVariant.PE32:
                            return this.optionalHeader32->Subsystem;

                        case PortableExecutableVariant.PE32Plus:
                            return this.optionalHeader64->Subsystem;

                        default:
                            throw ExceptionHelper.CreateInvalidEnumerationValueException( this.peVariant,
                                                                                          "this.peVariant" );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the ImageBase field of the Optional header.
        /// </summary>
        public ulong ImageBase
        {
            get
            {
                unsafe
                {
                    switch ( this.peVariant )
                    {
                        case PortableExecutableVariant.PE32:
                            return this.optionalHeader32->ImageBase;

                        case PortableExecutableVariant.PE32Plus:
                            return this.optionalHeader64->ImageBase;

                        default:
                            throw ExceptionHelper.CreateInvalidEnumerationValueException( this.peVariant,
                                                                                          "this.peVariant" );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the FileAlignment field of the Optional header.
        /// </summary>
        public uint FileAlignment
        {
            get
            {
                unsafe
                {
                    switch ( this.peVariant )
                    {
                        case PortableExecutableVariant.PE32:
                            return this.optionalHeader32->FileAlignment;

                        case PortableExecutableVariant.PE32Plus:
                            return this.optionalHeader64->FileAlignment;

                        default:
                            throw ExceptionHelper.CreateInvalidEnumerationValueException( this.peVariant,
                                                                                          "this.peVariant" );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Flags field of the CLI header.
        /// </summary>
        public uint CorFlags
        {
            get
            {
                unsafe
                {
                    return this.corHeader->Flags;
                }
            }
        }

        /// <summary>
        /// Gets the metadata version info of the CLI metadata header.
        /// </summary>
        /// <param name="major">Major version number.</param>
        /// <param name="minor">Minor version number.</param>
        /// <param name="versionString">Version string.</param>
        public void GetMetadataVersion( out int major, out int minor, out string versionString )
        {
            unsafe
            {
                CorMetadataRoot1* root1 = (CorMetadataRoot1*) this.RvaToPointer( this.corHeader->Metadata.Rva );
                major = root1->MajorVersion;
                minor = root1->MinorVersion;
                versionString = CorMetadataRoot1.GetVersionString( root1 ).TrimEnd( '\0' );
            }
        }

        /// <summary>
        /// Gets the CLI entry point.
        /// </summary>
        public MetadataToken EntryPoint
        {
            get
            {
                unsafe
                {
                    return new MetadataToken( this.corHeader->EntryPointToken );
                }
            }
        }

        /// <summary>
        /// Gets the MajorRuntimeVersion field of the CLI header.
        /// </summary>
        public int MajorRuntimeVersion
        {
            get
            {
                unsafe
                {
                    return this.corHeader->MajorRuntimeVersion;
                }
            }
        }

        /// <summary>
        /// Gets the MinorRuntimeVersion field of the CLI header.
        /// </summary>
        public int MinorRuntimeVersion
        {
            get
            {
                unsafe
                {
                    return this.corHeader->MinorRuntimeVersion;
                }
            }
        }

        /// <summary>
        /// Gets the buffer containing the metadata tables.
        /// </summary>
        public UnmanagedBuffer MetadataDirectory
        {
            get
            {
                unsafe
                {
                    CorHeader* cliHeader = this.corHeader;
                    return new UnmanagedBuffer( RvaToIntPtr( cliHeader->Metadata.Rva ),
                                                (int) cliHeader->Metadata.Size );
                }
            }
        }

        /// <summary>
        /// Determines whether the PE image has reserved space for the strong
        /// name signature.
        /// </summary>
        public bool HasStrongName
        {
            get
            {
                unsafe
                {
                    CorHeader* cliHeader = this.corHeader;
                    return cliHeader->StrongNameSignature.Size != 0;
                }
            }
        }

        #endregion

        #region Get strings, blobs, GUIDs, ...

        /// <summary>
        /// Gets a string from the String heap.
        /// </summary>
        /// <param name="index">Offset of the string.</param>
        /// <returns>The string at offset <paramref name="index"/>.</returns>
        public string GetString( int index )
        {
            #region Preconditions

            unsafe
            {
                ExceptionHelper.Core.AssertValidOperation( this.stringHeap != null, "NoHeapInPE", "#String" );
            }

            #endregion

            string value;

            if ( !this.strings.TryGetValue( index, out value ) )
            {
                unsafe
                {
                    // Count the string size;
                    byte* ptr = this.stringHeap + index;

                    int size = 0;
                    while ( *ptr != 0 )
                    {
                        size++;
                        ptr++;
                    }

                    value = new string( (sbyte*) this.stringHeap + index, 0, size, Encoding.UTF8 );

                    this.strings.Add( index, value );
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the address and the size of a blob on a binary heap.
        /// </summary>
        /// <param name="heap">Pointer to the heap origin.</param>
        /// <param name="index">Offset of the blob header with respect to <paramref name="heap"/>.</param>
        /// <param name="address">Pointer to the first data byte.</param>
        /// <param name="size">Data size.</param>
        private static unsafe void GetBlob( byte* heap, int index, out byte* address, out uint size )
        {
            address = heap + index;
            size = 0;

            if ( (*address & 128) == 0 )
            {
                size = *address;
                address++;
            }
            else if ( (*address & (128 + 64)) == 128 )
            {
                size = (((uint) (*address & (255 ^ (128 + 64)))) << 8) |
                       ((*(address + 1)));
                address += 2;
            }
            else if ( (*address & (128 + 64 + 32)) == 128 + 64 )
            {
                size = (((uint) (*address & (255 ^ (128 + 64 + 32)))) << 24) |
                       (((uint) *(address + 1)) << 16) |
                       (((uint) *(address + 2)) << 8) |
                       ((*(address + 3)));
                address += 4;
            }
            else
            {
                throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidBlobAddress" );
            }
        }

        /// <summary>
        /// Gets a blob from the Blob heap.
        /// </summary>
        /// <param name="index">Offset of the blob.</param>
        /// <returns>An <see cref="UnmanagedBuffer"/>.</returns>
        public UnmanagedBuffer GetBlobHeapSegment( int index )
        {
            unsafe
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation( this.blobHeap != null, "NoHeapInPE", "#Blob" );

                #endregion

                byte* address;
                uint size;
                GetBlob( this.blobHeap, index, out address, out size );

                return new UnmanagedBuffer( (IntPtr) address, (int) size );
            }
        }

        /// <summary>
        /// Gets a user string.
        /// </summary>
        /// <param name="token">A <see cref="TokenType.String"/> token.</param>
        /// <returns>The string at <paramref name="token"/>.</returns>
        public char[] GetUserString( MetadataToken token )
        {
            char[] value;

            if ( !this.userStrings.TryGetValue( token, out value ) )
            {
                unsafe
                {
                    byte* address;
                    uint size;
                    GetBlob( this.userStringHeap, token.Index + 1, out address, out size );
                    value = new char[size/2];
                    Marshal.Copy( (IntPtr) address, value, 0, (int) size/2 );
                    this.userStrings.Add( token, value );
                }
            }

            return value;
        }


        /// <summary>
        /// Gets a GUID from the GUID heap.
        /// </summary>
        /// <param name="index">Offset of the GUID in the heap.</param>
        /// <returns>A <see cref="Guid"/>.</returns>
        public Guid GetGuid( int index )
        {
            unsafe
            {
                byte* ptr = this.guidHeap + index;
                return *(Guid*) ptr;
            }
        }

        public UnmanagedBuffer GetManagedResource( int offset )
        {
            unsafe
            {
                byte* start = this.managedResources + offset;
                int size = *((int*) start);
                return new UnmanagedBuffer( (IntPtr) (start + 4), size );
            }
        }

        #endregion

        #region Parse unmanaged resources

        public void ReadUnmanagedResources( UnmanagedResourceCollection collection )
        {
            unsafe
            {
                if ( this.resourceTable != null )
                {
                    ParseResourceDirectory( this, this.resourceTable, collection, new UnmanagedResourceName(),
                                            new UnmanagedResourceName(), 0 );
                }
            }
        }

        private static unsafe void ParseResourceDirectory( ImageReader image, ResourceDirectory* source,
                                                           UnmanagedResourceCollection collection,
                                                           UnmanagedResourceName type,
                                                           UnmanagedResourceName resourceName, int level )
        {
            ResourceDirectoryEntry* entry = (ResourceDirectoryEntry*) ((byte*) source + sizeof(ResourceDirectory));

            Trace.ModuleReader.WriteLine(
                "Entering resource directory, NumberOfNamedEntries = {0}, NumberOfIdEntries = {1}",
                source->NumberOfNamedEntries, source->NumberOfIdEntries );

            for ( int i = 0; i < source->NumberOfNamedEntries; i++ )
            {
                UnmanagedResourceName name = new UnmanagedResourceName(
                    ResourceDirectoryEntry.GetName( image, entry ) );

                Trace.ModuleReader.WriteLine( "Parsing resource directory entry with name '{0}'.", name );
                ParseResourceDirectoryEntry(
                    image, entry, collection, name,
                    type, resourceName, level,
                    new Version( source->MajorVersion, source->MinorVersion ),
                    source->Characteristics );
                entry++;
            }

            for ( int i = 0; i < source->NumberOfIdEntries; i++ )
            {
                Trace.ModuleReader.WriteLine( "Parsing resource directory entry with id '{0}'.", entry->Id );
                ParseResourceDirectoryEntry(
                    image, entry, collection, new UnmanagedResourceName( (int) entry->Id ),
                    type, resourceName, level,
                    new Version( source->MajorVersion, source->MinorVersion ),
                    source->Characteristics );
                entry++;
            }

            Trace.ModuleReader.WriteLine( "Leaving resource directory." );
        }

        private static unsafe void ParseResourceDirectoryEntry( ImageReader image, ResourceDirectoryEntry* source,
                                                                UnmanagedResourceCollection collection,
                                                                UnmanagedResourceName entryName,
                                                                UnmanagedResourceName type,
                                                                UnmanagedResourceName resourceName, int level,
                                                                Version version, int characteristics )
        {
            if ( source->IsDirectory )
            {
                switch ( level )
                {
                    case 0:
                        type = entryName;
                        break;

                    case 1:
                        resourceName = entryName;
                        break;

                    default:
                        throw new AssertionFailedException();
                }

                ParseResourceDirectory(
                    image, ResourceDirectoryEntry.GetDirectoryEntry( image, source ),
                    collection, type, resourceName, level + 1 );
            }
            else
            {
                int language;

                if ( level == 2 )
                {
                    language = entryName.Id;
                }
                else
                {
                    language = 0;
                }

                ResourceData* data = ResourceDirectoryEntry.GetData( image, source );
                byte[] bytes = new byte[data->Size];
                Marshal.Copy( image.RvaToIntPtr( data->OffsetToData ), bytes, 0, bytes.Length );

                collection.Add( new RawUnmanagedResource( resourceName, type, (int) data->CodePage, language, version,
                                                          characteristics, bytes ) );
            }
        }

        #endregion

        /// <summary>
        /// Computes the absolute address (<see cref="IntPtr"/>) corresponding to a relative address.
        /// </summary>
        /// <param name="rva">A RVA (offset from the origin of the <i>mapped</i> PE image.</param>
        /// <returns>The absolute address corresponding to <paramref name="rva"/>.</returns>
        public IntPtr RvaToIntPtr( uint rva )
        {
            unsafe
            {
                return (IntPtr) this.RvaToPointer( rva );
            }
        }

        /// <summary>
        /// Computes the absolute address (<c>byte *</c>) corresponding to a relative address.
        /// </summary>
        /// <param name="rva">A RVA (offset from the origin of the <i>mapped</i> PE image.</param>
        /// <returns>The absolute address corresponding to <paramref name="rva"/>.</returns>
        public unsafe byte* RvaToPointer( uint rva )
        {
            if ( rva == 0 ) return null;

            if ( this.isImageMapped )
            {
                return (byte*) this.dosHeader + rva;
            }
            else
            {
                SectionHeader* header = this.firstSectionHeader;
                for ( int i = 0; i < this.ntHeader->FileHeader.NumberOfSections; i++ )
                {
                    if ( rva >= header->VirtualAddress &&
                         rva < header->VirtualAddress + header->VirtualSize )
                    {
                        return (byte*) this.dosHeader + (header->PointerToRawData + rva - header->VirtualAddress);
                    }

                    header = SectionHeader.GetNext( header );
                }

                throw new BadImageFormatException(
                    string.Format( "Cannot map the RVA 0x{0:x}.",
                                   rva ) );
            }
        }

        /// <summary>
        /// Align a pointer at a 4-byte boundary.
        /// </summary>
        /// <param name="ptr">A potentially non-aligned pointer.</param>
        /// <returns><paramref name="ptr"/> itself if the pointer is already properly aligned,
        /// otherwise the next 4-byte boundary.</returns>
        internal IntPtr AlignPointerAt4( IntPtr ptr )
        {
            unsafe
            {
                long arithmeticPtr = (byte*) ptr - (byte*) this.dosHeader;
                if ( arithmeticPtr%4 == 0 )
                {
                    return ptr;
                }
                else
                {
                    return new IntPtr( (byte*) this.dosHeader + (arithmeticPtr + 4 - (arithmeticPtr%4)) );
                }
            }
        }

        #endregion

        #region Structures

        private enum PortableExecutableVariant : ushort
        {
            PE32 = 0x10b,
            PE32Plus = 0x20b
        }


        /// <summary>
        /// NT header of a Portable Executable.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct PeNtHeader
        {
            /// <summary>
            /// Signature.
            /// </summary>
            public uint Signature;

            /// <summary>
            /// File Header.
            /// </summary>
            public FileHeader FileHeader;
        }

        /// <summary>
        /// DOS header of a Portable Executable.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct DosHeader
        {
            /// <summary>
            /// Magic number.
            /// </summary>
            public ushort Magic;

            /// <summary>
            /// Bytes on last page of file.
            /// </summary>
            public ushort BytesOnLastPage;

            /// <summary>
            ///  Pages in file.
            /// </summary>
            public ushort PagesCount;

            /// <summary>
            /// Relocations.
            /// </summary>
            public ushort RelocationsCount;

            /// <summary>
            /// Size of header in paragraphs.
            /// </summary>
            public ushort HeaderParagraphsCount;

            /// <summary>
            /// Minimum extra paragraphs needed.
            /// </summary>
            public ushort MinAlloc;

            /// <summary>
            /// Maximum extra paragraphs needed.
            /// </summary>
            public ushort MaxAlloc;

            /// <summary>
            /// Initial (relative) SS value.
            /// </summary>
            public ushort SS;

            /// <summary>
            /// Initial SP value.
            /// </summary>
            public ushort SP;

            /// <summary>
            /// Checksum.
            /// </summary>
            public ushort Checksum;

            /// <summary>
            /// Initial IP value.
            /// </summary>
            public ushort IP;

            /// <summary>
            /// Initial (relative) CS value.
            /// </summary>
            public ushort CS;

            /// <summary>
            /// File address of relocation table.
            /// </summary>
            public ushort RelocationRva;

            /// <summary>
            /// Overlay number
            /// </summary>
            public ushort OverlayNumber;

            public ushort Reserved1;
            public ushort Reserved2;
            public ushort Reserved3;
            public ushort Reserved4;

            /// <summary>
            /// OEM identifier (for oeminfo).
            /// </summary>
            public ushort OemId;

            /// <summary>
            /// OEM information (oemid specific).
            /// </summary>
            public ushort OemInfo;

            /// <summary>
            /// Reserved.
            /// </summary>
            public ushort Reserved5;

            public ushort Reserved6;
            public ushort Reserved7;
            public ushort Reserved8;
            public ushort Reserved9;
            public ushort Reserved10;
            public ushort Reserved11;
            public ushort Reserved12;
            public ushort Reserved13;
            public ushort Reserved14;

            /// <summary>
            /// File address of new exe header.
            /// </summary>
            public uint NewExeHeaderRva;
        }


        /// <summary>
        /// File header of a Portable Executable (size=20).
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct FileHeader
        {
#pragma warning disable 1591
            public ushort Machine;
            public ushort NumberOfSections;
            public uint Timestamp;
            public uint SymbolTablePointer;
            public uint NumberOfSymbols;
            public ushort OptionalHeaderSize;
            public ushort Characteristics;
#pragma warning restore 1591
        }


        /// <summary>
        /// Optional header of a Portable Executable (PE32).
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct OptionalHeader32
        {
#pragma warning disable 1591
            public PortableExecutableVariant Magic;
            public byte LMajor;
            public byte LMinor;
            public uint CodeSize;
            public uint InitializedDataSize;
            public uint UninitializedDataSize;
            public uint EntryPointRva;
            public uint BaseOfCode;
            public uint BaseOfData;
            public uint ImageBase;
            public uint SectionAlignment;
            public uint FileAlignment;
            public ushort OsMajor;
            public ushort OsMinor;
            public ushort UserMajor;
            public ushort UserMinor;
            public ushort SubsysMajor;
            public ushort SubsysMinor;
            public uint Reserved1;
            public uint ImageSize;
            public uint HeaderSize;
            public uint Checksum;
            public ushort Subsystem;
            public ushort DllFlags;
            public uint StackReserveSize;
            public uint StackCommitSize;
            public uint HeapReserveSize;
            public uint HeapCommitSize;
            public uint LoaderFlags;
            public uint NumberOfDataDirectories;
            public DataDirectory ExportTable;
            public DataDirectory ImportTable;
            public DataDirectory ResourceTable;
            public DataDirectory ExceptionTable;
            public DataDirectory CertificateTable;
            public DataDirectory BaseRelocationTable;
            public ulong Debug;
            public ulong Copyright;
            public ulong GlobalPtr;
            public ulong TlsTable;
            public ulong LoadConfigTable;
            public ulong BoundImport;
            public ulong ImportAddressTable;
            public ulong DelayImportDescriptor;
            public uint CliHeaderRVA;
            public uint CliHeaderSize;
            public ulong Reserved2;
            public SectionHeader FirstSectionHeader;
#pragma warning restore 1591
        }

        /// <summary>
        /// Optional header of a Portable Executable (PE32+).
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct OptionalHeader64
        {
#pragma warning disable 1591
            public PortableExecutableVariant Magic;
            public byte LMajor;
            public byte LMinor;
            public uint CodeSize;
            public uint InitializedDataSize;
            public uint UninitializedDataSize;
            public uint EntryPointRva;
            public uint BaseOfCode;
            public ulong ImageBase;
            public uint SectionAlignment;
            public uint FileAlignment;
            public ushort OsMajor;
            public ushort OsMinor;
            public ushort UserMajor;
            public ushort UserMinor;
            public ushort SubsysMajor;
            public ushort SubsysMinor;
            public uint Reserved1;
            public uint ImageSize;
            public uint HeaderSize;
            public uint Checksum;
            public ushort Subsystem;
            public ushort DllFlags;
            public ulong StackReserveSize;
            public ulong StackCommitSize;
            public ulong HeapReserveSize;
            public ulong HeapCommitSize;
            public uint LoaderFlags;
            public uint NumberOfDataDirectories;
            public DataDirectory ExportTable;
            public DataDirectory ImportTable;
            public DataDirectory ResourceTable;
            public DataDirectory ExceptionTable;
            public DataDirectory CertificateTable;
            public DataDirectory BaseRelocationTable;
            public ulong Debug;
            public ulong Copyright;
            public ulong GlobalPtr;
            public ulong TlsTable;
            public ulong LoadConfigTable;
            public ulong BoundImport;
            public ulong ImportAddressTable;
            public ulong DelayImportDescriptor;
            public uint CliHeaderRVA;
            public uint CliHeaderSize;
            public ulong Reserved2;
            public SectionHeader FirstSectionHeader;
#pragma warning restore 1591
        }


        /// <summary>
        /// Section header of a Portable Executable.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct SectionHeader
        {
#pragma warning disable 1591
            public byte Name1;
            public byte Name2;
            public byte Name3;
            public byte Name4;
            public byte Name5;
            public byte Name6;
            public byte Name7;
            public byte Name8;
            public uint VirtualSize;
            public uint VirtualAddress;
            public uint SizeOfRawData;
            public uint PointerToRawData;
            public uint PointerToRelovations;
            public uint PointerToLineNumbers;
            public ushort NumberOfRelocations;
            public ushort NumberOfLineNumbers;
            public uint Characteristics;
#pragma warning restore 1591

            internal static unsafe object GetName( SectionHeader* header )
            {
                return new string( (sbyte*) &(header->Name1), 0, 8 );
            }

            internal static unsafe SectionHeader* GetNext( SectionHeader* header )
            {
                return (SectionHeader*) (((byte*) header) + sizeof(SectionHeader));
            }
        }

        /// <summary>
        /// Represents a section of a PE image.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct DataDirectory
        {
            /// <summary>
            /// Relative address.
            /// </summary>
            public uint Rva;

            /// <summary>
            /// Size in bytes.
            /// </summary>
            public uint Size;
        }

        /// <summary>
        /// CLI header of a Portable Executable.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct CorHeader
        {
#pragma warning disable 1591
            public uint Cb;
            public ushort MajorRuntimeVersion;
            public ushort MinorRuntimeVersion;
            public DataDirectory Metadata;
            public uint Flags;
            public uint EntryPointToken;
            public DataDirectory Resources;
            public DataDirectory StrongNameSignature;
            public DataDirectory CodeManagerTable;
            public DataDirectory VTableFixups;
            public DataDirectory ExportAddressTableJumps;
            public DataDirectory ManagedNativeHeader;
#pragma warning restore 1591
        }

        /// <summary>
        /// Root of the CLI metadata section (first part).
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct CorMetadataRoot1
        {
#pragma warning disable 1591
            public uint Signature;
            public ushort MajorVersion;
            public ushort MinorVersion;
            public uint Reserved;
            public uint VersionStringLength;
            private byte versionString;
#pragma warning restore 1591

            /// <summary>
            /// Gets the version string of a <see cref="CorMetadataRoot1"/>.
            /// </summary>
            /// <param name="instance">Pointer to a <see cref="CorMetadataRoot1"/>.</param>
            /// <returns>The version string of <paramref name="instance"/>.</returns>
            public static unsafe string GetVersionString( CorMetadataRoot1* instance )
            {
                byte[] bytes = new byte[instance->VersionStringLength];
                Marshal.Copy( (IntPtr) (&instance->versionString), bytes, 0, (int) instance->VersionStringLength );
                return Encoding.UTF8.GetString( bytes );
            }

            /// <summary>
            /// Gets the second part of the metadata root structure.
            /// </summary>
            /// <param name="instance">Pointer to the first part of the structure.</param>
            /// <param name="imageReader">The <see cref="ImageReader"/>.</param>
            /// <returns>The second part of the structure.</returns>
            public static unsafe CorMetadataRoot2* GetMetadataRoot2( CorMetadataRoot1* instance, ImageReader imageReader )
            {
                return
                    (CorMetadataRoot2*)
                    imageReader.AlignPointerAt4( new IntPtr( &instance->versionString + instance->VersionStringLength ) )
                        .
                        ToPointer();
            }
        }

        /// <summary>
        /// Root of the CLI metadata section (second part).
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct CorMetadataRoot2
        {
            /// <summary>
            /// Flags.
            /// </summary>
            public ushort Flags;

            /// <summary>
            /// Number of streams.
            /// </summary>
            public ushort StreamNumber;

            /// <summary>
            /// First stream.
            /// </summary>
            public CorStreamHeader Streams;
        }

        /// <summary>
        /// Stream header in a CLI image.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct CorStreamHeader
        {
            /// <summary>
            /// Offset.
            /// </summary>
            public uint Offset;

            /// <summary>
            /// Size.
            /// </summary>
            public uint Size;

            /// <summary>
            /// First stream name.
            /// </summary>
            private byte name;

            public static unsafe string GetName( CorStreamHeader* instance )
            {
                return Marshal.PtrToStringAnsi( (IntPtr) (&instance->name) );
            }

            public static unsafe CorStreamHeader* GetNext( CorStreamHeader* instance, ImageReader imageReader )
            {
                byte* cursor = &instance->name;
                while ( *cursor != 0 )
                {
                    cursor++;
                }
                cursor++;

                return (CorStreamHeader*) imageReader.AlignPointerAt4( new IntPtr( cursor ) ).ToPointer();
            }
        }

        /// <summary>
        /// Header of CLI metadata tables.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct CorMetadataTablesHeader
        {
            /// <summary>
            /// Reserved.
            /// </summary>
            public uint Reserved1;

            /// <summary>
            /// Major version.
            /// </summary>
            public byte MajorVersion;

            /// <summary>
            /// Minor version.
            /// </summary>
            public byte MinorVersion;

            /// <summary>
            /// Bit mask determining the size of heaps.
            /// </summary>
            public HeapSizes HeapSizes;

            /// <summary>
            /// Reserved.
            /// </summary>
            public byte Reserved2;

            /// <summary>
            /// Bit mask determining which tables are included in the image.
            /// </summary>
            public ulong Valid;

            /// <summary>
            /// Sorted.
            /// </summary>
            public ulong Sorted;

            /// <summary>
            /// Number of rows in the first valid table.
            /// </summary>
            private uint rows;

            /// <summary>
            /// Gets an array mapping normal table index to ordinal in this image.
            /// </summary>
            /// <param name="instance">Pointer to a <see cref="CorMetadataTablesHeader"/>.</param>
            /// <returns>A mapping between table index and ordinal in this image</returns>
            public static unsafe int[] GetTableMap( CorMetadataTablesHeader* instance )
            {
                int[] map = new int[64];

                ulong valid = instance->Valid;

                int count = 0;
                for ( int i = 0; i < 64; i++ )
                {
                    if ( (valid & 1) != 0 )
                    {
                        map[i] = count;
                        count++;
                    }
                    else
                    {
                        map[i] = -1;
                    }

                    valid = valid >> 1;
                }

                return map;
            }

            /// <summary>
            /// Gets the number of valid tables.
            /// </summary>
            /// <param name="instance">Pointer to a <see cref="CorMetadataTablesHeader"/>.</param>
            /// <returns>The number of valid tables.</returns>
            public static unsafe uint CountTables( CorMetadataTablesHeader* instance )
            {
                uint count = 0;
                ulong valid = instance->Valid;

                for ( int i = 0; i < 64; i++ )
                {
                    if ( (valid & 1) != 0 )
                    {
                        count++;
                    }

                    valid = valid >> 1;
                }

                return count;
            }

            /// <summary>
            /// Gets the number of rows in a given metadata table.
            /// </summary>
            /// <param name="instance">Pointer to a <see cref="CorMetadataTablesHeader"/>.</param>
            /// <param name="index">Ordinal of the table in this file.</param>
            /// <returns>The number of rows of the table that is in position <paramref name="index"/>
            /// in this file.</returns>
            public static unsafe uint CountRows( CorMetadataTablesHeader* instance, int index )
            {
                return *(&instance->rows + index);
            }

            /// <summary>
            /// Gets a pointer to the first table.
            /// </summary>
            /// <param name="instance">Pointer to a <see cref="CorMetadataTablesHeader"/>.</param>
            /// <returns>A pointer to the first table</returns>
            public static unsafe IntPtr GetFirstTable( CorMetadataTablesHeader* instance )
            {
                return new IntPtr( &instance->rows + CountTables( instance ) );
            }
        }

        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct ResourceDirectory // Defined in WinNT.h
        {
            public int Characteristics;
            public int TimeDateStamp;
            public short MajorVersion;
            public short MinorVersion;
            public short NumberOfNamedEntries;
            public short NumberOfIdEntries;
        }


        // http://msdn.microsoft.com/msdnmag/issues/02/03/PE2/

        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct ResourceDirectoryEntry // Defined in WinNT.h
        {
            private uint nameOrId;
            private int offsetToData;

            public bool IsName
            {
                get { return (nameOrId & 0x80000000) != 0; }
            }

            public uint Id
            {
                get { return nameOrId & 0xFFFF; }
            }

            public bool IsDirectory
            {
                get { return (offsetToData & 0x80000000) != 0; }
            }

            public static unsafe string GetName( ImageReader image, ResourceDirectoryEntry* entry )
            {
                ResourceDirectoryEntryName* nameStruct = (ResourceDirectoryEntryName*)
                                                         ((byte*) image.resourceTable + (entry->nameOrId & ~0x80000000));

                return ResourceDirectoryEntryName.GetName( nameStruct );
            }

            public static unsafe ResourceData* GetData( ImageReader image, ResourceDirectoryEntry* directoryEntry )
            {
                return (ResourceData*)
                       ((byte*) image.resourceTable + (directoryEntry->offsetToData & ~0x80000000));
            }

            public static unsafe ResourceDirectory* GetDirectoryEntry( ImageReader image,
                                                                       ResourceDirectoryEntry* directoryEntry )
            {
                return (ResourceDirectory*)
                       ((byte*) image.resourceTable + (directoryEntry->offsetToData & ~0x80000000));
            }
        }

        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct ResourceDirectoryEntryName
        {
            private short lenght;
            private char firstChar;

            public static unsafe string GetName( ResourceDirectoryEntryName* name )
            {
                return new string( &name->firstChar, 0, name->lenght );
            }
        }


        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct ResourceData // Defined in WinNT.h
        {
            public uint OffsetToData;
            public uint Size;
            public uint CodePage;
            public uint Reserved;
        }

        #endregion
    }


    /// <summary>
    /// Bit mask determining which heaps are large.
    /// </summary>
    [Flags]
    internal enum HeapSizes : byte
    {
        /// <summary>
        /// All heaps are small.
        /// </summary>
        None = 0,

        /// <summary>
        /// The String heap is large.
        /// </summary>
        LargeString = 1,

        /// <summary>
        /// The Guid heap is large.
        /// </summary>
        LargeGuid = 2,

        /// <summary>
        /// The Blob heap is large.
        /// </summary>
        LargeBlob = 4
    }
}