#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion

#if !SMALL

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Text;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Maps a point in IL instructions to location in source code.
    /// </summary>
    /// <remarks>
    /// This class implements the <see cref="IComparable{SymbolSequencePoint}"/> interface.
    /// This allows sorting sequence points according to their IL offsets and performing
    /// binary searches in sorted arrays.
    /// </remarks>
    public sealed class SymbolSequencePoint :
        IComparable<SymbolSequencePoint>, IEquatable<SymbolSequencePoint>
    {
        #region Fields

        /// <summary>
        /// Start line in the source code.
        /// </summary>
        private readonly int startLine;

        /// <summary>
        /// Start column in the source code.
        /// </summary>
        private readonly int startColumn;

        /// <summary>
        /// End line in the source code.
        /// </summary>
        private readonly int endLine;

        /// <summary>
        /// End column in the source code.
        /// </summary>
        private readonly int endColumn;

        /// <summary>
        /// Offset in the IL binary code.
        /// </summary>
        private readonly int offset;

        private readonly ISymbolDocument document;

        #endregion

        public const int HiddenValue = 0xfeefee;

        /// <summary>
        /// Gets a symbol meaning that the associated instructions have no source code.
        /// </summary>
        [SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )] public static readonly SymbolSequencePoint Hidden = new SymbolSequencePoint(
            -1, HiddenValue, HiddenValue, HiddenValue, HiddenValue, null );

        public SymbolSequencePoint( int offset ) : this(
            offset, 0, 0, 0, 0, null )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="SymbolSequencePoint"/>.
        /// </summary>
        /// <param name="offset">The offset of the current sequence point in the IL stream,
        /// w.r.t. the first byte of the method body.</param>
        /// <param name="startLine">The start line in the source file.</param>
        /// <param name="startColumn">The start column in the source file.</param>
        /// <param name="endLine">The end line in the source file.</param>
        /// <param name="endColumn">The end column in the source file.</param>
        /// <param name="document">Source file</param>
        public SymbolSequencePoint( int offset, int startLine, int startColumn,
                                      int endLine, int endColumn, ISymbolDocument document )
        {
            this.startColumn = startColumn;
            this.document = document;
            this.startLine = startLine;
            this.endColumn = endColumn;
            this.endLine = endLine;
            this.offset = offset;
        }

        /// <summary>
        /// Initializes a new <see cref="SymbolSequencePoint"/>.
        /// </summary>
        /// <param name="startLine">The start line in the source file.</param>
        /// <param name="startColumn">The start column in the source file.</param>
        /// <param name="endLine">The end line in the source file.</param>
        /// <param name="endColumn">The end column in the source file.</param>
        /// <param name="document">Source file</param>
        public SymbolSequencePoint( int startLine, int startColumn, int endLine, int endColumn, ISymbolDocument document )
        {
            this.startColumn = startColumn;
            this.document = document;
            this.startLine = startLine;
            this.endColumn = endColumn;
            this.endLine = endLine;
        }


        /// <summary>
        /// Determines whether the current symbol means that the associated
        /// instructions have no source code..
        /// </summary>
        public bool IsHidden
        {
            get { return this.startLine == HiddenValue; }
        }


        /// <summary>
        /// Gets the start line in the source file.
        /// </summary>
        public int StartLine
        {
            get { return startLine; }
        }

        /// <summary>
        /// Gets the end line in the source file.
        /// </summary>
        public int EndLine
        {
            get { return endLine; }
        }

        /// <summary>
        /// Gets the start column in the source file.
        /// </summary>
        public int StartColumn
        {
            get { return startColumn; }
        }

        /// <summary>
        /// Gets the end column in the source file.
        /// </summary>
        public int EndColumn
        {
            get { return endColumn; }
        }

        /// <summary>
        /// Gets the offset in the IL method body.
        /// </summary>
        //internal int Offset
        //{
        //    get { return this.offset; }
        //}
        public int Offset
        {
            get { return this.offset; }
        }

        /// <summary>
        /// Gets the document defining the next instructions.
        /// </summary>
        public ISymbolDocument Document
        {
            get { return document; }
        }

        #region IWriteILDefinition Members

        /*
        /// <summary>
        /// Writes the IL definition of the current sequence point.
        /// </summary>
        /// <param name="writer">An <see cref="PostSharp.ModuleWriter.ILWriter"/>.</param>
        internal void WriteILDefinition( ILWriter writer )
        {
            writer.WriteKeyword( ".line" );
            if ( this.IsHidden )
            {
                writer.WriteInteger( HiddenValue, IntegerFormat.HexLower );
            }
            else
            {
                writer.WriteInteger( this.startLine, IntegerFormat.Decimal );
                writer.WriteSymbol( ',', SymbolSpacingKind.None, SymbolSpacingKind.None );
                writer.WriteInteger( this.endLine, IntegerFormat.Decimal );

                if ( this.startColumn >= 0 )
                {
                    writer.WriteSymbol( ':', SymbolSpacingKind.Required, SymbolSpacingKind.Required );
                    writer.WriteInteger( this.startColumn, IntegerFormat.Decimal );
                    writer.WriteSymbol( ',', SymbolSpacingKind.None, SymbolSpacingKind.None );
                    writer.WriteInteger( this.endColumn, IntegerFormat.Decimal );
                }
            }
        }
         */

        #endregion

        #region IComparable<SymbolSequencePoint> Members

        /// <inheritdoc />
        int IComparable<SymbolSequencePoint>.CompareTo( SymbolSequencePoint other )
        {
            return this.offset.CompareTo( other.offset );
        }

        #endregion

        #region IEquatable<SymbolSequencePoint> Members

        /// <inheritdoc />
        public bool Equals( SymbolSequencePoint other )
        {
            if ( ReferenceEquals( other, null ) )
            {
                return false;
            }

            return
                this.endColumn == other.endColumn &&
                this.endLine == other.endLine &&
                this.offset == other.offset &&
                this.startColumn == other.startColumn &&
                this.startLine == other.startLine;
        }

        /// <inheritdoc />
        public override bool Equals( object obj )
        {
            SymbolSequencePoint symbolSequencePoint = obj as SymbolSequencePoint;

            if ( ReferenceEquals( symbolSequencePoint, null ) )
            {
                return this.Equals( symbolSequencePoint );
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether two sequence points are equal.
        /// </summary>
        /// <param name="left">A <see cref="SymbolSequencePoint"/>.</param>
        /// <param name="right">A <see cref="SymbolSequencePoint"/>.</param>
        /// <returns><b>true</b> if both sequence points are equal, otherwise <b>false</b>.</returns>
        public static bool operator ==( SymbolSequencePoint left, SymbolSequencePoint right )
        {
            if ( ReferenceEquals( right, null ) )
            {
                if ( ReferenceEquals( left, null ) )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return left.Equals( right );
        }

        /// <summary>
        /// Determines whether two sequence points are different.
        /// </summary>
        /// <param name="left">A <see cref="SymbolSequencePoint"/>.</param>
        /// <param name="right">A <see cref="SymbolSequencePoint"/>.</param>
        /// <returns><b>true</b> if both sequence points are different, otherwise <b>false</b>.</returns>
        public static bool operator !=( SymbolSequencePoint left, SymbolSequencePoint right )
        {
            if ( ReferenceEquals( right, null ) )
            {
                if ( ReferenceEquals( left, null ) )
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }


            return !left.Equals( right );
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.startLine;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if ( this.offset >= 0 )
                builder.AppendFormat( "{0} -> ", this.offset );

            if ( this.IsHidden )
            {
                builder.Append( "hidden" );
            }
            else
            {
                builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "{0},{1} : {2},{3}",
                    this.startLine, this.endLine,
                    this.startColumn, this.endColumn );
            }

            return builder.ToString();
        }

        #endregion
    }
}

#endif