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
using System.IO;
using System.Text;
using PostSharp.CodeModel;

namespace PostSharp.ModuleWriter
{
    internal static class UnmanagedResourceWriter
    {
        public static void WriteResources( IEnumerable<UnmanagedResource> resources, Stream stream )
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Unicode))
            {
                // We should output a empty resource before.
                RawUnmanagedResource empty = new RawUnmanagedResource( new UnmanagedResourceName(),
                    new UnmanagedResourceName(), 0, 0, new Version( ), 0, null  );
                WriteResource( empty, writer );

                foreach ( UnmanagedResource resource in resources )
                {
                    // Align at DWORD.
                    int position = (int) stream.Position;
                    if (position % 4 != 0)
                    {
                        for ( int i = 0 ; i < 4 - position%4 ; i++ )
                        {
                            writer.Write( (byte) 0 );
                        }
                    }
                    WriteResource(resource, writer);
                }
            }
        }

        //See http://msdn2.microsoft.com/en-us/library/ms648007.aspx
        public static void WriteResource( UnmanagedResource resource, BinaryWriter writer )
        {
            // Remember the position of the head, so we can come back.
            long head = writer.BaseStream.Position;

            /*
              struct RESOURCEHEADER { 
                  DWORD DataSize; 
                  DWORD HeaderSize; 
                  [Ordinal or name TYPE]; 
                  [Ordinal or name NAME]; 
                  DWORD DataVersion; 
                  WORD MemoryFlags; 
                  WORD LanguageId; 
                  DWORD Version; 
                  DWORD Characteristics; 
            };  */
            writer.Write( 0 ); // Data Size
            writer.Write( 0 ); // Header Size
            Write( resource.Type, writer );
            Write( resource.Name, writer );
            writer.Write( 0 ); // Data Version
            //writer.Write( (short) GetMemoryFlags(resource.TypeId)); // Memory flags
            writer.Write((short)0); // Memory flags
            writer.Write( (short) resource.Language );
            writer.Write( (short) resource.Version.Major );
            writer.Write( (short) resource.Version.Minor );
            writer.Write( resource.Characteristics );

            int headerSize = (int) (writer.BaseStream.Position - head);

            resource.Write( writer );

            long end = writer.BaseStream.Position;

            int dataSize = (int) (( end - head ) - headerSize);

            writer.Seek( (int) head, SeekOrigin.Begin );

            writer.Write( dataSize ); // Data Size
            writer.Write( headerSize ); // Header Size

            writer.Seek( (int) end, SeekOrigin.Begin ); 
        }

        private static void Write( UnmanagedResourceName name, BinaryWriter writer )
        {
            if ( name.IsId )
            {
                writer.Write( (short) -1 );
                writer.Write( (short) name.Id );
            }
            else
            {
                string nameStr = name.Name;
                writer.Write( nameStr.ToCharArray() );
                writer.Write( (short) 0 );

                // Bad to the next word boundary.
                int length = ( nameStr.Length + 1 )*sizeof(ushort);
                for ( ; ( length & ( sizeof(uint) - 1 ) ) != 0 ; length++ )
                {
                    writer.Write( (byte) 0 );
                }
                return;
            }
        }

        private static MemoryFlags GetMemoryFlags( UnmanagedResourceType resourceType )
        {
            switch ( resourceType )
            {
                case UnmanagedResourceType.Cursor:
                case UnmanagedResourceType.Icon:
                    return MemoryFlags.Discardable | MemoryFlags.Movable;

                case UnmanagedResourceType.Menu:
                case UnmanagedResourceType.Dialog:
                case UnmanagedResourceType.String:
                case UnmanagedResourceType.GroupCursor:
                case UnmanagedResourceType.IconCursor:
                    return MemoryFlags.Discardable | MemoryFlags.Pure | MemoryFlags.Movable;
                case UnmanagedResourceType.Bitmap:
                case UnmanagedResourceType.Accelerator:
                case UnmanagedResourceType.Version:
                case UnmanagedResourceType.Html:
                    return MemoryFlags.Pure | MemoryFlags.Movable;

                default:
                    return MemoryFlags.None;
            }
        }

        private enum MemoryFlags : short
        {
            None = 0,
            Movable = 0x0010,
            Pure = 0x0020,
            ResourcePreload = 0x0040,
            Discardable = 0x1000
        }
    }
}