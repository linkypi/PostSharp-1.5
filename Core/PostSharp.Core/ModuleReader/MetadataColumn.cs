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
using System.Diagnostics.CodeAnalysis;
using PostSharp.CodeModel;

namespace PostSharp.ModuleReader
{
    /// <summary>
    /// Encapsulates a metadata column. Base class for all concrete types of columns.
    /// </summary>
    internal abstract class MetadataColumn
    {
        #region Fields

        /// <summary>
        /// Parent table.
        /// </summary>
        private readonly MetadataTable table;

#if DEBUG
        private string name;
#endif

        #endregion

        /// <summary>
        /// Initializes a new <see cref="MetadataColumn"/>.
        /// </summary>
        /// <param name="table">Parent <see cref="MetadataTable"/>.</param>
        /// <param name="name">Column name.</param>
        /// <param name="ordinal">Colum ordinal.</param>
        [SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters" )]
#pragma warning disable 168
        protected MetadataColumn( MetadataTable table, string name, int ordinal )
#pragma warning restore 168
        {
            this.table = table;
#if DEBUG
            this.name = name;
#endif
        }

        /// <summary>
        /// Gets the parent table.
        /// </summary>
        public MetadataTable Table { get { return this.table; } }

        /// <summary>
        /// Gets or sets the offset of this column w.r.t. a <see cref="MetadataRow"/>.
        /// </summary>
        internal int Offset { get; set; }

        /// <summary>
        /// Gets or sets the size of this column in bytes.
        /// </summary>
        internal int Size { get; set; }
    }


    /// <summary>
    /// Encapsulates a metadata column that contains a 32-bit integer.
    /// </summary>
    internal sealed class MetadataColumnInt32 : MetadataColumn
    {
        /// <summary>
        /// Initializes a new <see cref="MetadataColumnInt32"/>.
        /// </summary>
        /// <param name="table">Parent <see cref="MetadataTable"/>.</param>
        /// <param name="columnName">Column name.</param>
        /// <param name="columnOrdinal">Colum ordinal.</param>
        public MetadataColumnInt32( MetadataTable table, string columnName, int columnOrdinal )
            : base( table, columnName, columnOrdinal )
        {
            this.Size = sizeof(Int32);
        }


        /// <summary>
        /// Gets the value of this column for a given row.
        /// </summary>
        /// <param name="row">A <see cref="MetadataRow"/>.</param>
        /// <returns>The value of the current column in <paramref name="row"/>.</returns>
        public Int32 GetValue( MetadataRow row )
        {
            unsafe
            {
                return *(Int32*) ( row.Address + this.Offset );
            }
        }
    }

    /// <summary>
    /// Encapsulates a metadata column that contains a 16-bit integer.
    /// </summary>
    internal sealed class MetadataColumnInt16 : MetadataColumn
    {
        /// <summary>
        /// Initializes a new <see cref="MetadataColumnInt16"/>.
        /// </summary>
        /// <param name="table">Parent <see cref="MetadataTable"/>.</param>
        /// <param name="columnName">Column name.</param>
        /// <param name="columnOrdinal">Colum ordinal.</param>
        public MetadataColumnInt16( MetadataTable table, string columnName, int columnOrdinal )
            : base( table, columnName, columnOrdinal )
        {
            this.Size = sizeof(Int16);
        }


        /// <summary>
        /// Gets the value of this column for a given row.
        /// </summary>
        /// <param name="row">A <see cref="MetadataRow"/>.</param>
        /// <returns>The value of the current column in <paramref name="row"/>.</returns>
        public Int16 GetValue( MetadataRow row )
        {
            unsafe
            {
                return *(Int16*) ( row.Address + this.Offset );
            }
        }
    }


    /// <summary>
    /// Encapsulates a metadata column that contains a string.
    /// </summary>
    internal sealed class MetadataColumnString : MetadataColumn
    {
        /// <summary>
        /// Initializes a new <see cref="MetadataColumnString"/>.
        /// </summary>
        /// <param name="table">Parent <see cref="MetadataTable"/>.</param>
        /// <param name="columnName">Column name.</param>
        /// <param name="columnOrdinal">Colum ordinal.</param>
        public MetadataColumnString( MetadataTable table, string columnName, int columnOrdinal )
            : base( table, columnName, columnOrdinal )
        {
            this.Size = ( this.Table.Tables.ImageReader.HeapSizes & HeapSizes.LargeString ) != 0 ? 4 : 2;
        }


        /// <summary>
        /// Gets the value of this column for a given row.
        /// </summary>
        /// <param name="row">A <see cref="MetadataRow"/>.</param>
        /// <returns>The value of the current column in <paramref name="row"/>.</returns>
        public string GetValue( MetadataRow row )
        {
            unsafe
            {
                int index;
                if ( this.Size == 2 )
                {
                    index = *(ushort*) ( row.Address + this.Offset );
                }
                else
                {
                    index = (int) *(uint*) ( row.Address + this.Offset );
                }
                if ( index != 0 )
                {
                    return this.Table.Tables.ImageReader.GetString( index );
                }
                else
                {
                    return null;
                }
            }
        }
    }


    /// <summary>
    /// Encapsulates a metadata column that contains a blob.
    /// </summary>
    internal sealed class MetadataColumnBlob : MetadataColumn
    {
        /// <summary>
        /// Initializes a new <see cref="MetadataColumnBlob"/>.
        /// </summary>
        /// <param name="table">Parent <see cref="MetadataTable"/>.</param>
        /// <param name="columnName">Column name.</param>
        /// <param name="columnOrdinal">Colum ordinal.</param>
        public MetadataColumnBlob( MetadataTable table, string columnName, int columnOrdinal )
            : base( table, columnName, columnOrdinal )
        {
            this.Size = ( this.Table.Tables.ImageReader.HeapSizes & HeapSizes.LargeBlob ) != 0 ? 4 : 2;
        }

        /// <summary>
        /// Gets the value of this column for a given row and returns it as a managed array
        /// of bytes.
        /// </summary>
        /// <param name="row">A <see cref="MetadataRow"/>.</param>
        /// <returns>The value of the current column in <paramref name="row"/>.</returns>
        public byte[] GetValueByteArray( MetadataRow row )
        {
            BufferReader reader = this.GetValueBufferReader( row );
            if ( reader == null )
            {
                return null;
            }
            else
            {
                return reader.ToByteArray();
            }
        }

        /// <summary>
        /// Gets the value of this column for a given row and returns it as 
        /// an <see cref="UnmanagedBuffer"/>.
        /// </summary>
        /// <param name="row">A <see cref="MetadataRow"/>.</param>
        /// <returns>The value of the current column in <paramref name="row"/>.</returns>
        public UnmanagedBuffer GetValueUnmanagedBuffer( MetadataRow row )
        {
            unsafe
            {
                int index;

                if ( this.Size == 2 )
                {
                    index = *(ushort*) ( row.Address + this.Offset );
                }
                else
                {
                    index = (int) *(uint*) ( row.Address + this.Offset );
                }
                if ( index == 0 )
                {
                    return UnmanagedBuffer.Void;
                }
                else
                {
                    return this.Table.Tables.ImageReader.GetBlobHeapSegment( index );
                }
            }
        }


        /// <summary>
        /// Gets the value of this column for a given row and returns it as 
        /// a <see cref="BufferReader"/>.
        /// </summary>
        /// <param name="row">A <see cref="MetadataRow"/>.</param>
        /// <returns>The value of the current column in <paramref name="row"/>.</returns>
        public BufferReader GetValueBufferReader( MetadataRow row )
        {
            UnmanagedBuffer hs = this.GetValueUnmanagedBuffer( row );
            return hs.IsVoid ? null : new BufferReader( hs );
        }

        
    }

    /// <summary>
    /// Encapsulates a metadata column that contains a GUID.
    /// </summary>
    internal sealed class MetadataColumnGuid : MetadataColumn
    {
        /// <summary>
        /// Initializes a new <see cref="MetadataColumnGuid"/>.
        /// </summary>
        /// <param name="table">Parent <see cref="MetadataTable"/>.</param>
        /// <param name="columnName">Column name.</param>
        /// <param name="columnOrdinal">Colum ordinal.</param>
        public MetadataColumnGuid( MetadataTable table, string columnName, int columnOrdinal )
            : base( table, columnName, columnOrdinal )
        {
            this.Size = ( this.Table.Tables.ImageReader.HeapSizes & HeapSizes.LargeGuid ) != 0 ? 4 : 2;
        }


        /// <summary>
        /// Gets the value of this column for a given row.
        /// </summary>
        /// <param name="row">A <see cref="MetadataRow"/>.</param>
        /// <returns>The value of the current column in <paramref name="row"/>.</returns>
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
        public Guid GetValue( MetadataRow row )
        {
            unsafe
            {
                int index;
                if ( this.Size == 2 )
                {
                    index = *(ushort*) ( row.Address + this.Offset );
                }
                else
                {
                    index = *(int*) ( row.Address + this.Offset );
                }
                return this.Table.Tables.ImageReader.GetGuid( index );
            }
        }
    }

    /// <summary>
    /// Encapsulates a metadata column that a coded token, i.e. a token whose
    /// low-value bits are an index in an array specifying the token type.
    /// </summary>
    internal class MetadataColumnCodedToken : MetadataColumn
    {
        #region Fields

        /// <summary>
        /// Array mapping the coded token type bits to a <see cref="TokenType"/>.
        /// </summary>
        private readonly TokenType[] targetTables;

        /// <summary>
        /// Bit mask isolating the low-value bits.
        /// </summary>
        private readonly uint tableMask;

        /// <summary>
        /// Number of low-value bits.
        /// </summary>
        private readonly byte indexShift;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="MetadataColumnCodedToken"/>.
        /// </summary>
        /// <param name="table">Parent table.</param>
        /// <param name="columnName">Column name.</param>
        /// <param name="targetTables">Array mapping the coded token type bits to a <see cref="TokenType"/>.</param>
        /// <param name="columnOrdinal">Column ordinal.</param>
        public MetadataColumnCodedToken( MetadataTable table, string columnName, TokenType[] targetTables,
                                         int columnOrdinal )
            : base( table, columnName, columnOrdinal )
        {
            this.targetTables = targetTables;

            // Compute the number of bits reserved
            int size = this.targetTables.Length - 1;
            while ( size > 0 )
            {
                size = size >> 1;
                this.indexShift++;
                this.tableMask = ( this.tableMask << 1 ) | 1;
            }

            uint maximalShortNumber = (uint) ( ushort.MaxValue >> this.indexShift );

            for ( int i = 0 ; i < this.targetTables.Length ; i++ )
            {
                if ( this.Table.Tables.ImageReader.GetMetadataTableSize(
                         (MetadataTableOrdinal) ( (int) this.targetTables[i] >> 24 ) ) > maximalShortNumber )
                {
                    this.Size = 4;
                    return;
                }
            }

            this.Size = 2;
        }


        /// <summary>
        /// Gets the value of this column for a given row.
        /// </summary>
        /// <param name="row">A <see cref="MetadataRow"/>.</param>
        /// <returns>The value of the current column in <paramref name="row"/>.</returns>
        public MetadataToken GetValue( MetadataRow row )
        {
            uint value;
            unsafe
            {
                if ( this.Size == 4 )
                {
                    value = *( (uint*) ( row.Address + this.Offset ) );
                }
                else
                {
                    value = *( (ushort*) ( row.Address + this.Offset ) );
                }
            }

            uint table = ( value & this.tableMask );
            uint index = value >> this.indexShift;

            if ( index == 0 )
            {
                return new MetadataToken();
            }

            return new MetadataToken( this.targetTables[(int) table], (int) index - 1 );
        }
    }

    /// <summary>
    /// Encapsulates a metadata column that contains a row index.
    /// </summary>
    internal sealed class MetadataColumnRowIndex : MetadataColumn
    {
        /// <summary>
        /// Ordinal of the target table.
        /// </summary>
        private readonly MetadataTableOrdinal targetTable;

        /// <summary>
        /// Initializes a new <see cref="MetadataColumnRowIndex"/>.
        /// </summary>
        /// <param name="table">Parent table.</param>
        /// <param name="columnName">Column name.</param>
        /// <param name="targetTable">Target table ordinal.</param>
        /// <param name="columnOrdinal">Column ordinal.</param>
        public MetadataColumnRowIndex( MetadataTable table, string columnName, MetadataTableOrdinal targetTable,
                                       int columnOrdinal )
            : base( table, columnName, columnOrdinal )
        {
            this.targetTable = targetTable;

            if ( this.Table.Tables.ImageReader.GetMetadataTableSize( this.TargetTable ) > ushort.MaxValue )
            {
                this.Size = 4;
            }
            else
            {
                this.Size = 2;
            }
        }

        /// <summary>
        /// Gets the target table ordinal.
        /// </summary>
        public MetadataTableOrdinal TargetTable { get { return this.targetTable; } }


        /// <summary>
        /// Gets the value of this column for a given row.
        /// </summary>
        /// <param name="row">A <see cref="MetadataRow"/>.</param>
        /// <returns>The value of the current column in <paramref name="row"/>.</returns>
        public MetadataToken GetValue( MetadataRow row )
        {
            int value;
            unsafe
            {
                if ( this.Size == 4 )
                {
                    value = *( (int*) ( row.Address + this.Offset ) );
                }
                else
                {
                    value = *( (ushort*) ( row.Address + this.Offset ) );
                }
            }

            if ( value == 0 )
            {
                return new MetadataToken();
            }
            else
            {
                return new MetadataToken( this.TargetTable, value - 1 );
            }
        }


        /// <summary>
        /// Gets the range of tokens included between the value of the current column in a
        /// given row and the value of the current column in the next row.
        /// </summary>
        /// <param name="row">A <see cref="MetadataRow"/>.</param>
        /// <param name="firstChild">First token of the range.</param>
        /// <param name="lastChild">Last token of the range.</param>
        internal void GetRange( MetadataRow row, out MetadataToken firstChild, out MetadataToken lastChild )
        {
            firstChild = this.GetValue( row );
            MetadataRow nextParentRow = this.Table.GetNextRow( row );
            if ( nextParentRow.IsNull )
            {
                lastChild =
                    new MetadataToken( this.TargetTable, this.Table.Tables.Tables[(int) this.TargetTable].RowCount - 1 );
            }
            else
            {
                lastChild = new MetadataToken( this.GetValue( nextParentRow ).Value - 1 );
            }
        }
    }
}