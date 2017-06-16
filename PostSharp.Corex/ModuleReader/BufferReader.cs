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

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using PostSharp.CodeModel;

#endregion

namespace PostSharp.ModuleReader
{
    /// <summary>
    /// Implements an effective binary reader for reading managed and unmanaged buffers.
    /// </summary>
    /// <remarks>
    /// In case a <see cref="BufferReader"/> is used to read a managed array of byte, this
    /// class will pin the buffer in memory until the <see cref="BufferReader"/> is
    /// assigned to another buffer or is disposed.
    /// </remarks>
    internal unsafe class BufferReader : IDisposable
    {
        #region Fields

        /// <summary>
        /// Current offset.
        /// </summary>
        private int offset;

        /// <summary>
        /// Pointer to the current position.
        /// </summary>
        private byte* cursor;

        /// <summary>
        /// Pointer to the first byte of the buffer.
        /// </summary>
        private byte* start;

        /// <summary>
        /// Buffer size in bytes.
        /// </summary>
        private int size;

        /// <summary>
        /// A GC handle pinning the unmanaged buffer, or an unallocated GC handle
        /// if the <see cref="BufferReader"/> is currently assigned to
        /// a managed buffer or is disposed.
        /// </summary>
        private GCHandle gcHandle;

        #endregion

#if DEBUG
        /// <summary>
        /// Whether the offset should be tested before each read operation.
        /// </summary>
        public bool CheckOffsetEnabled = true;
#endif

        #region Construction

        /// <summary>
        /// Initializes a new unassigned <see cref="BufferReader"/>.
        /// </summary>
        public BufferReader()
        {
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Initializes a new <see cref="BufferReader"/> assigned to an unmanaged buffer
        /// represented by an <see cref="UnmanagedBuffer"/>.
        /// </summary>
        /// <param name="unmanagedBuffer">An unmanaged buffer</param>
        public BufferReader( UnmanagedBuffer unmanagedBuffer )
            :
                this( unmanagedBuffer.Origin, unmanagedBuffer.Size )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BufferReader"/> assigned to an unmanaged buffer
        /// represented by a pointer (<see cref="IntPtr"/>) and a size.
        /// </summary>
        /// <param name="buffer">Pointer to the first byte of the buffer.</param>
        /// <param name="size">Size of the buffer in bytes.</param>
        public BufferReader( IntPtr buffer, int size )
            :
                this( (byte*) buffer.ToPointer(), size )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BufferReader"/> assigned to an unmanaged buffer
        /// represented by a pointer (<c>byte *</c>) and a size.
        /// </summary>
        /// <param name="buffer">Pointer to the first byte of the buffer.</param>
        /// <param name="size">Size of the buffer in bytes.</param>
        public BufferReader( byte* buffer, int size )
        {
            this.Initialize( buffer, size );
            GC.SuppressFinalize( this );
        }

        /*
		/// <summary>
		/// Initializes a new <see cref="BufferReader"/> assigned to a managed buffer.
		/// </summary>
		/// <param name="bytes">An array of bytes.</param>
		public BufferReader( byte[] bytes )
		{
			this.Initialize( bytes );
		}
         */


        /// <summary>
        /// Initializes the current <see cref="BufferReader"/> to a managed array.
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        public void Initialize( byte[] bytes )
        {
            if ( this.gcHandle.IsAllocated )
            {
                this.gcHandle.Free();
            }

            this.gcHandle = GCHandle.Alloc( bytes, GCHandleType.Pinned );
            this.cursor = (byte*) (void*) this.gcHandle.AddrOfPinnedObject();
            this.start = this.cursor;
            this.size = bytes.Length;
            this.offset = 0;
            GC.ReRegisterForFinalize( this );
        }

        /// <summary>
        /// Initializes the current <see cref="BufferReader"/> to an unmanaged buffer
        /// represented by a pointer (<see cref="IntPtr"/>) and a size.
        /// </summary>
        /// <param name="buffer">Pointer to the first byte of the buffer.</param>
        /// <param name="size">Size of the buffer in bytes.</param>
        public void Initialize( IntPtr buffer, int size )
        {
            this.Initialize( (byte*) buffer.ToPointer(), size );
        }

        /// <summary>
        /// Initializes the current <see cref="BufferReader"/> to an unmanaged buffer
        /// represented by a pointer (<c>byte *</c>) and a size.
        /// </summary>
        /// <param name="buffer">Pointer to the first byte of the buffer.</param>
        /// <param name="size">Size of the buffer in bytes.</param>
        public void Initialize( byte* buffer, int size )
        {
            ExceptionHelper.AssertArgumentNotNull( buffer, "buffer" );
            if ( size < 0 )
                throw new ArgumentOutOfRangeException( "size" );

            if ( this.gcHandle.IsAllocated )
            {
                this.gcHandle.Free();
                GC.SuppressFinalize( this );
            }

            this.cursor = buffer;
            this.start = buffer;
            this.offset = 0;
            this.size = size;
        }

        #endregion

        /// <summary>
        /// Returns a copy of the current buffer as an array of bytes.
        /// </summary>
        /// <returns>An array of bytes initialized with the content of the current buffer.</returns>
        public byte[] ToByteArray()
        {
            byte[] array = new byte[this.size];
            Marshal.Copy( (IntPtr) this.start, array, 0, size );
            return array;
        }


        /// <summary>
        /// Checks that the current offset is not larger than the buffer size.
        /// </summary>
        [Conditional( "DEBUG" )]
        [SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic" )]
        private void CheckOffset()
        {
#if DEBUG
            if ( !this.CheckOffsetEnabled )
            {
                return;
            }
            if ( this.cursor == null )
            {
                throw new InvalidOperationException();
            }
            if ( this.offset > this.size )
            {
                throw new BufferOverflowException();
            }
#endif
        }

        #region Properties

        /// <summary>
        /// Gets or sets the current offset.
        /// </summary>
        public int Offset
        {
            get { return this.offset; }
            set
            {
                this.offset = value;
                this.cursor = this.start + value;
                this.CheckOffset();
            }
        }

        /// <summary>
        /// Gets the buffer size.
        /// </summary>
        public int Size { get { return this.size; } }

        /// <summary>
        /// Gets a pointer to the current
        /// </summary>
        public IntPtr CurrentPosition { get { return (IntPtr) this.cursor; } }

        #endregion

        #region Read

        public object Read( IntrinsicType type )
        {
            switch ( type )
            {
                case IntrinsicType.SByte:
                    return this.ReadSByte();

                case IntrinsicType.Byte:
                    return this.ReadByte();

                case IntrinsicType.Int16:
                    return this.ReadInt16();

                case IntrinsicType.UInt16:
                    return this.ReadUInt16();

                case IntrinsicType.Int32:
                    return this.ReadInt32();

                case IntrinsicType.UInt32:
                    return this.ReadUInt32();

                case IntrinsicType.Int64:
                    return this.ReadInt64();

                case IntrinsicType.UInt64:
                    return this.ReadUInt64();

                case IntrinsicType.Char:
                    return this.ReadChar();

                case IntrinsicType.Single:
                    return this.ReadSingle();

                case IntrinsicType.Double:
                    return this.ReadDouble();

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( type, "type" );
            }
        }

        /// <summary>
        /// Reads the next <see cref="Int32"/> and moves the cursor forward.
        /// </summary>
        /// <returns>The <see cref="Int32"/> at the current position.</returns>
        public Int32 ReadInt32()
        {
            Int32 result = *( (Int32*) this.cursor );
            cursor += sizeof(Int32);
            offset += sizeof(Int32);
            this.CheckOffset();
            return result;
        }


        /// <summary>
        /// Reads the next <see cref="Int64"/> and moves the cursor forward.
        /// </summary>
        /// <returns>The <see cref="Int64"/> at the current position.</returns>
        public Int64 ReadInt64()
        {
            Int64 result = *( (Int64*) this.cursor );
            cursor += sizeof(Int64);
            offset += sizeof(Int64);
            this.CheckOffset();
            return result;
        }

        /// <summary>
        /// Reads the next <see cref="Int16"/> and moves the cursor forward.
        /// </summary>
        /// <returns>The <see cref="Int16"/> at the current position.</returns>
        public Int16 ReadInt16()
        {
            Int16 result = *( (Int16*) this.cursor );
            cursor += sizeof(Int16);
            offset += sizeof(Int16);
            this.CheckOffset();
            return result;
        }

        /// <summary>
        /// Reads the next <see cref="UInt32"/> and moves the cursor forward.
        /// </summary>
        /// <returns>The <see cref="UInt32"/> at the current position.</returns>
        public UInt32 ReadUInt32()
        {
            UInt32 result = *( (UInt32*) this.cursor );
            cursor += sizeof(UInt32);
            offset += sizeof(UInt32);
            this.CheckOffset();
            return result;
        }

        /// <summary>
        /// Reads the next <see cref="UInt64"/> and moves the cursor forward.
        /// </summary>
        /// <returns>The <see cref="UInt64"/> at the current position.</returns>
        public UInt64 ReadUInt64()
        {
            UInt64 result = *( (UInt64*) this.cursor );
            cursor += sizeof(UInt64);
            offset += sizeof(UInt32);
            this.CheckOffset();
            return result;
        }

        /// <summary>
        /// Reads the next <see cref="UInt16"/> and moves the cursor forward.
        /// </summary>
        /// <returns>The <see cref="UInt16"/> at the current position.</returns>
        public UInt16 ReadUInt16()
        {
            UInt16 result = *( (UInt16*) this.cursor );
            cursor += sizeof(UInt16);
            offset += sizeof(UInt16);
            this.CheckOffset();
            return result;
        }

        /// <summary>
        /// Reads the next <see cref="Byte"/> and moves the cursor forward.
        /// </summary>
        /// <returns>The <see cref="Byte"/> at the current position.</returns>
        public Byte ReadByte()
        {
            Byte result = *this.cursor;
            cursor += sizeof(Byte);
            offset += sizeof(Byte);
            this.CheckOffset();
            return result;
        }

        /// <summary>
        /// Reads the next <see cref="SByte"/> and moves the cursor forward.
        /// </summary>
        /// <returns>The <see cref="SByte"/> at the current position.</returns>
        public SByte ReadSByte()
        {
            SByte result = *( (SByte*) this.cursor );
            cursor += sizeof(SByte);
            offset += sizeof(SByte);
            this.CheckOffset();
            return result;
        }


        /// <summary>
        /// Reads the next <see cref="Single"/> and moves the cursor forward.
        /// </summary>
        /// <returns>The <see cref="Single"/> at the current position.</returns>
        public Single ReadSingle()
        {
            Single result = *( (Single*) this.cursor );
            cursor += sizeof(Single);
            offset += sizeof(Single);
            this.CheckOffset();
            return result;
        }

        /// <summary>
        /// Reads the next <see cref="Double"/> and moves the cursor forward.
        /// </summary>
        /// <returns>The <see cref="Double"/> at the current position.</returns>
        public Double ReadDouble()
        {
            Double result = *( (Double*) this.cursor );
            cursor += sizeof(Double);
            offset += sizeof(Double);
            this.CheckOffset();
            return result;
        }

        /// <summary>
        /// Reads the next <see cref="char"/> and moves the cursor forward.
        /// </summary>
        /// <returns>The <see cref="char"/> at the current position.</returns>
        public char ReadChar()
        {
            char result = *( (char*) this.cursor );
            cursor += sizeof(char);
            offset += sizeof(char);
            this.CheckOffset();
            return result;
        }


        /// <summary>
        /// Reads the next compressed integer and moves the cursor forward.
        /// </summary>
        /// <returns>The compressed integer at the current position.</returns>
        public uint ReadCompressedInteger()
        {
            int res;
            // Handle smallest data inline. 
            if ( ( *cursor & 0x80 ) == 0x00 ) // 0??? ????    
            {
                res = *this.cursor++;
                this.offset++;
                this.CheckOffset();
                return (uint) res;
            }
            else
            {
                // 1 byte data is handled in CorSigUncompressData   
                //  _ASSERTE(*pData & 0x80);    

                // Medium.  
                if ( ( *cursor & 0xC0 ) == 0x80 ) // 10?? ????  
                {
                    res = ( *cursor++ & 0x3f ) << 8;
                    res |= *cursor++;
                    this.offset += 2;
                }
                else // 110? ???? 
                {
                    res = ( *cursor++ & 0x1f ) << 24;
                    res |= *cursor++ << 16;
                    res |= *cursor++ << 8;
                    res |= *cursor++;
                    this.offset += 4;
                }
                this.CheckOffset();
                return (uint) res;
            }
        }

        /// <summary>
        /// Reads the next <b>SerString</b> and moves the cursor forward.
        /// </summary>
        /// <returns>The <b>SerString</b> at the current position.</returns>
        /// <remarks>
        /// A <b>SerString</b> is a UTF-8 encoded string where the first compressed
        /// integer determines the string length.
        /// </remarks>
        public string ReadSerString()
        {
            if ( *this.cursor == 0xFF )
            {
                this.ReadByte();
                return null;
            }

            uint len = this.ReadCompressedInteger();

            return this.ReadStringUtf8( (int) len );
        }

/*
		/// <summary>
		/// Reads the next null-terminated UTF-8 string and moves the cursor forward.
		/// </summary>
		/// <returns>The null-terminated UTF-8 string at the current position.</returns>
		public string ReadStringUtf8()
		{
			// Count the string size;
			byte* ptr = this.cursor;

			int length = 0;
			while ( *ptr != 0 )
			{
				length++;
				ptr++;
			}

			string value = this.ReadStringUtf8( length );
			this.cursor++;
			this.offset++;

			return value;
		}
*/

        /// <overloads>Reads the next UTF-8 string and moves the cursor forward.</overloads>
        /// <summary>
        /// Reads the next fixed-length UTF-8 string and moves the cursor forward.
        /// </summary>
        /// <param name="length">String length in bytes.</param>
        /// <returns>The fixed-length UTF-8 string at the current position.</returns>
        public string ReadStringUtf8( int length )
        {
            #region Preconditions

            if ( length > 1024 )
            {
                throw new ArgumentOutOfRangeException( "length" );
            }

            #endregion

            string value = new string( (sbyte*) this.cursor, 0, length, Encoding.UTF8);
            this.cursor += length;
            this.offset += length;
            return value;
        }

        /// <summary>
        /// Reads the next fixed-length UTF-16 string and moves the cursor forward.
        /// </summary>
        /// <param name="length">String length in characters.</param>
        /// <returns>The fixed-length UTF-16 string at the current position.</returns>
        public string ReadStringUtf16( int length )
        {
            string value = new string((sbyte*)this.cursor, 0, length*2, Encoding.Unicode);
            this.cursor += length;
            this.offset += length;
            return value;
        }

        #endregion

        #region Destruction

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose( true );
        }

        /// <summary>
        /// Releases the pinned managed buffer is relevant.
        /// </summary>
        /// <param name="disposing">Whether the method is called from the <see cref="Dispose()"/> method.</param>
        private void Dispose( bool disposing )
        {
            if ( this.gcHandle.IsAllocated )
            {
                this.gcHandle.Free();
                if ( disposing )
                {
                    GC.SuppressFinalize( this );
                }
            }
        }

        /// <summary>
        /// Type destructor.
        /// </summary>
        ~BufferReader()
        {
            this.Dispose( false );
        }

        #endregion
    }
}
