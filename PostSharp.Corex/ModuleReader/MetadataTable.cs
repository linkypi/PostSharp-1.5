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
    /// Encapsulates a binary metadata table.
    /// </summary>
    internal sealed class MetadataTable
    {
        #region Fields

        /// <summary>
        /// Parent collection of tables.
        /// </summary>
        private readonly MetadataTables tables;

        /// <summary>
        /// Table ordinal.
        /// </summary>
        private readonly MetadataTableOrdinal tableOrdinal;

        /// <summary>
        /// Array of table columns.
        /// </summary>
        private MetadataColumn[] columns;

        /// <summary>
        /// Address of the first row.
        /// </summary>
        private unsafe byte* address;

        /// <summary>
        /// Size in bytes of a row of this table.
        /// </summary>
        private int rowSize;

        /// <summary>
        /// Number of rows in this table.
        /// </summary>
        private readonly int rowCount;

#if DEBUG
#pragma warning disable 219
        private string tableName;
#pragma warning restore 219
#endif

        #endregion

        /// <summary>
        /// Initializes a new <see cref="MetadataTable"/>.
        /// </summary>
        /// <param name="tables">Parent collection of tables.</param>
        /// <param name="tableName">Table name.</param>
        /// <param name="tableOrdinal">Table ordinal.</param>
        [SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters" )]
        public MetadataTable( MetadataTables tables, string tableName, int tableOrdinal )
        {
            this.tableOrdinal = (MetadataTableOrdinal) tableOrdinal;
            this.tables = tables;
            this.rowCount = this.tables.ImageReader.GetMetadataTableSize( (MetadataTableOrdinal) tableOrdinal );
#if DEBUG
            this.tableName = tableName;
#endif
        }

        /// <summary>
        /// Gets the number of rows in the table.
        /// </summary>
        public int RowCount { get { return this.rowCount; } }

        /// <summary>
        /// Gets the size of a single row in bytes.
        /// </summary>
        public int RowSize { get { return this.rowSize; } }

        /// <summary>
        /// Gets the table ordinal.
        /// </summary>
        public MetadataTableOrdinal Ordinal { get { return this.tableOrdinal; } }

        /// <summary>
        /// Sets the array of columns and computes the size of rows.
        /// </summary>
        /// <param name="columns">Array of table columns.</param>
        public void SetColumns( MetadataColumn[] columns )
        {
            this.columns = columns;

            this.rowSize = 0;
            for ( int i = 0 ; i < this.columns.Length - 1 ; i++ )
            {
                this.columns[i].Offset = this.rowSize;
                this.rowSize += this.columns[i].Size;
            }
        }

        /// <summary>
        /// Gets the parent collection of metadata tables.
        /// </summary>
        public MetadataTables Tables { get { return this.tables; } }

        /// <summary>
        /// Gets the address of the first row.
        /// </summary>
        public unsafe byte* Address
        {
            //get { return this.address; }
            set { this.address = value; }
        }

        /// <summary>
        /// Gets the <see cref="MetadataRow"/> corresponding to a <see cref="MetadataToken"/>.
        /// </summary>
        /// <param name="token">A <see cref="MetadataToken"/> referring to the current table.</param>
        /// <returns>A <see cref="MetadataRow"/> for <paramref name="token"/>.</returns>
        public MetadataRow GetRow( MetadataToken token )
        {
            return this.GetRow( token.Index );
        }

        /// <summary>
        /// Gets a <see cref="MetadataRow"/> given its index.
        /// </summary>
        /// <param name="index">Zero-based row index.</param>
        /// <returns>The <see cref="MetadataRow"/> at position <paramref name="index"/>.</returns>
        public MetadataRow GetRow( int index )
        {
#if ASSERT
            if ( index < 0 || index >= this.rowCount )
            {
                throw new ArgumentOutOfRangeException();
            }
#endif

            unsafe
            {
                return new MetadataRow( this.address + ( index*this.rowSize ), this.tableOrdinal );
            }
        }


        /// <summary>
        /// Gets the <see cref="MetadataRow"/> following a given <see cref="MetadataRow"/>.
        /// </summary>
        /// <param name="row">A <see cref="MetadataRow"/> of this table.</param>
        /// <returns>The <see cref="MetadataRow"/> following <paramref name="row"/>,
        /// or a null <see cref="MetadataRow"/> if <paramref name="row"/> is the
        /// last row of the table.</returns>
        public MetadataRow GetNextRow( MetadataRow row )
        {
            unsafe
            {
                MetadataRow newRow = new MetadataRow( row.Address + this.rowSize, this.Ordinal );

                if ( newRow.Address >= this.address + this.rowSize*this.rowCount )
                {
                    return new MetadataRow( null, this.Ordinal );
                }
                else
                {
                    return newRow;
                }
            }
        }
    }
}