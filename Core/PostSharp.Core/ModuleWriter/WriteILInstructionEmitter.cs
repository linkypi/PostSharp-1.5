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
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using PostSharp.CodeModel;

namespace PostSharp.ModuleWriter
{
    /// <summary>
    /// An <see cref="InstructionEmitter"/> that writes instructions to an <see cref="ILWriter"/>.
    /// </summary>
    internal sealed class WriteILInstructionEmitter : PreparingInstructionEmitter
    {
        #region Fields

        readonly Dictionary<string,int> localVariableSymbolUses = new Dictionary<string, int>();

        /// <summary>
        /// In-memory <see cref="StringBuilder"/> for building temporary expressions.
        /// </summary>
        private StringBuilder memoryIlWriterStringBuilder;

        /// <summary>
        /// In-memory <see cref="StringWriter"/> for building temporary expressions.
        /// </summary>
        private StringWriter memoryIlWriterStringWriter;

        /// <summary>
        /// In-memory <see cref="ILWriter"/> for building temporary expressions.
        /// </summary>
        private ILWriter memoryIlWriter;

        /// <summary>
        /// Target <see cref="ILWriter"/>.
        /// </summary>
        private readonly ILWriter writer;

        /// <summary>
        /// <see cref="GenericMap"/> of the current method body.
        /// </summary>
        private readonly GenericMap genericMap;

        /// <summary>
        /// Reads the intruction flow.
        /// </summary>
        private InstructionReader reader;

        /// <summary>
        /// Next value to be used to identify blocks, when exception handling
        /// clauses are given in non-structured form.
        /// </summary>
        private int nextBlockId;

        private readonly List<string> beforeNextInstruction = new List<string>( 2 );
        private readonly List<string> afterNextInstruction = new List<string>( 2 );

        /// <summary>
        /// Previously emitted ISymbolDocument, or null if no symbol document
        /// was emitted before.
        /// </summary>
        private string lastSymbolDocument;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="WriteILInstructionEmitter"/>.
        /// </summary>
        /// <param name="body">The method body to be written.</param>
        /// <param name="writer">The target <see cref="ILWriter"/>.</param>
        /// <param name="sequenceRanges">Range of instruction sequence addresses.</param>
        /// <param name="localHasMissingSymbol">Determines, for each local variable, whether it is at least one referenced
        /// in a lexical scope where there is no symbol for this local variable.</param>
        /// <param name="localHasReference">Determines, for each local variable, whether it is referenced at least once.</param>
        internal WriteILInstructionEmitter( MethodBodyDeclaration body, ILWriter writer,
                                            AddressRange[] sequenceRanges, bool[] localHasMissingSymbol,
                                            bool[] localHasReference )
            : base( body, sequenceRanges, localHasMissingSymbol, localHasReference, true )
        {
            this.writer = writer;
            this.genericMap = body.Method.GetGenericContext( GenericContextOptions.None );
        }

        /// <summary>
        /// Initializes the in-memory <see cref="ILWriter"/> (<see cref="memoryIlWriter"/>).
        /// </summary>
        private void InitializeMemoryWriter()
        {
            if ( this.memoryIlWriter == null )
            {
                this.memoryIlWriterStringBuilder = new StringBuilder( 256 );
                this.memoryIlWriterStringWriter = new StringWriter( this.memoryIlWriterStringBuilder,
                                                                    CultureInfo.InvariantCulture );
                this.memoryIlWriter = new ILWriter( this.memoryIlWriterStringWriter );
            }
            else
            {
                this.memoryIlWriterStringBuilder.Length = 0;
            }
        }

        /// <inheritdoc />
        protected override void InternalEmitPrefix( InstructionPrefixes prefix )
        {
            base.InternalEmitPrefix( prefix );

            if ( prefix == InstructionPrefixes.None )
            {
                return;
            }

            if ( ( prefix & InstructionPrefixes.Tail ) != 0 )
            {
                writer.WriteKeyword( "tail." );
                writer.WriteLineBreak();
            }

            if ( ( prefix & InstructionPrefixes.ReadOnly ) != 0 )
            {
                writer.WriteKeyword( "readonly." );
                writer.WriteLineBreak();
            }

            if ( ( prefix & InstructionPrefixes.Volatile ) != 0 )
            {
                writer.WriteKeyword( "volatile." );
                writer.WriteLineBreak();
            }

            if ( ( prefix & InstructionPrefixes.UnalignedMask ) != 0 )
            {
                writer.WriteKeyword( "unaligned." );

                switch ( prefix & InstructionPrefixes.UnalignedMask )
                {
                    case InstructionPrefixes.Unaligned1:
                        writer.WriteInteger( 1, IntegerFormat.Decimal );
                        break;

                    case InstructionPrefixes.Unaligned2:
                        writer.WriteInteger( 2, IntegerFormat.Decimal );
                        break;

                    case InstructionPrefixes.Unaligned4:
                        writer.WriteInteger( 4, IntegerFormat.Decimal );
                        break;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException(
                            prefix, "prefix" );
                }
                writer.WriteLineBreak();
            }
        }

        /// <inheritdoc />
        protected override void InternalEmitInstruction( OpCodeNumber code )
        {
            if ( code != OpCodeNumber.Nop || !this.IgnoreNop )
            {
                base.InternalEmitInstruction( code );
                writer.WriteInstruction( code );
            }
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionSignature( OpCodeNumber code,
                                                                  StandaloneSignatureDeclaration signature )
        {
            base.InternalEmitInstructionSignature( code, signature );
            writer.WriteInstruction( code );
            signature.WriteILReference( this.writer, this.genericMap, WriteTypeReferenceOptions.None );
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionField( OpCodeNumber code, IField field )
        {
            base.InternalEmitInstructionField( code, field );
            writer.WriteInstruction( code );
            if ( code == OpCodeNumber.Ldtoken )
            {
                writer.WriteKeyword( "field" );
            }
            ( (IFieldInternal) field ).WriteILReference( this.writer, this.genericMap );
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionByte( OpCodeNumber code, byte operand )
        {
            base.InternalEmitInstructionByte( code, operand );
            writer.WriteInstruction( code );
            writer.WriteInteger( (int) (sbyte) operand, IntegerFormat.Decimal );
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionInt64( OpCodeNumber code, long operand )
        {
            base.InternalEmitInstructionInt64( code, operand );
            writer.WriteInstruction( code );
            writer.WriteInteger( operand );
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionInt32( OpCodeNumber code, int operand )
        {
            base.InternalEmitInstructionInt32( code, operand );
            writer.WriteInstruction( code );
            writer.WriteInteger( (ulong) (uint) operand, IntegerFormat.HexLower );
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionInt16( OpCodeNumber code, short operand )
        {
            base.InternalEmitInstructionInt16( code, operand );
            writer.WriteInstruction( code );
            writer.WriteInteger( operand );
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionSingle( OpCodeNumber code, float operand )
        {
            base.InternalEmitInstructionSingle( code, operand );
            writer.WriteInstruction( code );
            writer.WriteSingle( operand );
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionDouble( OpCodeNumber code, double operand )
        {
            base.InternalEmitInstructionDouble( code, operand );
            writer.WriteInstruction( code );
            writer.WriteDouble( operand );
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionString( OpCodeNumber code, LiteralString operand )
        {
            base.InternalEmitInstructionString( code, operand );
            writer.WriteInstruction( code );
            writer.WriteQuotedString( operand, WriteStringOptions.DoubleQuoted );
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionMethod( OpCodeNumber code, IMethod method )
        {
            base.InternalEmitInstructionMethod( code, method );
            writer.WriteInstruction( code );
            if ( code == OpCodeNumber.Ldtoken )
            {
                writer.WriteKeyword( "method" );
            }
            ( (IMethodInternal) method ).WriteILReference( this.writer, this.genericMap,
                                                           WriteMethodReferenceOptions.None );
        }

        /// <inheritdoc />
        protected override void InternalEmitSwitchInstruction( InstructionSequence[] switchTargets )
        {
            base.InternalEmitSwitchInstruction( switchTargets );

            writer.WriteInstruction( OpCodeNumber.Switch );
            writer.WriteSymbol( '(' );
            writer.MarkAutoIndentLocation();
            writer.WriteLineBreak();
            for ( int i = 0; i < switchTargets.Length; i++ )
            {
                if ( i > 0 )
                {
                    writer.WriteSymbol( ',' );
                    writer.WriteLineBreak();
                }

                switchTargets[i].WriteILReference( writer );
            }

            writer.WriteSymbol( ')' );
            writer.ResetIndentLocation();
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionType( OpCodeNumber code, ITypeSignature type )
        {
            base.InternalEmitInstructionType( code, type );
            writer.WriteInstruction( code );
            ( (ITypeSignatureInternal) type ).WriteILReference( this.writer, this.genericMap,
                                                                WriteTypeReferenceOptions.None );
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionParameter( OpCodeNumber code, ParameterDeclaration parameter,
                                                                  OperandType operandType )
        {
            base.InternalEmitInstructionParameter( code, parameter, operandType );
            writer.WriteInstruction( code );
            parameter.WriteILReference( this.writer );
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionLocalVariable( OpCodeNumber code,
                                                                      LocalVariableSymbol localVariableSymbol,
                                                                      OperandType operandType )
        {
            base.InternalEmitInstructionLocalVariable( code, localVariableSymbol, operandType );
            writer.WriteInstruction( code );

            int usages;
            if (!string.IsNullOrEmpty(localVariableSymbol.Name))
            {
                this.localVariableSymbolUses.TryGetValue( localVariableSymbol.Name, out usages );
            }
            else
            {
                // If the name is null, a name will be auto-generated anyway.
                usages = 1;
            }

            if ( usages > 1)
            {
                // We have an ambiguity. We need to use the variable number instead of its name.
                this.writer.WriteInteger( localVariableSymbol.LocalVariable.Ordinal );
            }
            else
            {
                localVariableSymbol.WriteILReference( this.writer );
            }
        }

        /// <inheritdoc />
        protected override AddressDistance InternalEmitBranchingInstruction( OpCodeNumber nearCode, OpCodeNumber farCode,
                                                                             InstructionSequence sequence )
        {
            AddressDistance distance = base.InternalEmitBranchingInstruction( nearCode, farCode, sequence );
            switch ( distance )
            {
                case AddressDistance.Near:
                    writer.WriteInstruction( nearCode );
                    break;

                case AddressDistance.Undeterminate:
                case AddressDistance.Far:
                    writer.WriteInstruction( farCode );
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( distance, "distance" );
            }

            sequence.WriteILReference( this.writer );
            if ( !string.IsNullOrEmpty( sequence.Comment ) )
            {
                this.writer.WriteComment( sequence.Comment );
            }

            return distance;
        }


        /// <inheritdoc />
        protected override void InternalEmitPrefixType( OpCodeNumber prefix, ITypeSignature type )
        {
            base.InternalEmitPrefixType( prefix, type );
            switch ( prefix )
            {
                case OpCodeNumber.Constrained:
                    this.writer.WriteKeyword( "constrained. " );
                    ( (ITypeSignatureInternal) type ).WriteILReference( this.writer, this.genericMap,
                                                                        WriteTypeReferenceOptions.None );
                    break;

                default:
                    throw ExceptionHelper.Core.CreateArgumentException( "prefix", "UnexpectedInstruction", prefix );
            }

            this.writer.WriteLineBreak();
        }

        /// <inheritdoc />
        protected override void InternalEmitSymbolSequencePoint( SymbolSequencePoint sequencePoint )
        {
            writer.WriteKeyword( ".line" );
            if ( sequencePoint.IsHidden )
            {
                writer.WriteInteger( SymbolSequencePoint.HiddenValue, IntegerFormat.HexLower );
            }
            else
            {
                writer.WriteInteger( sequencePoint.StartLine, IntegerFormat.Decimal );
                writer.WriteSymbol( ',', SymbolSpacingKind.None, SymbolSpacingKind.None );
                writer.WriteInteger( sequencePoint.EndLine, IntegerFormat.Decimal );

                if ( sequencePoint.StartColumn >= 0 )
                {
                    writer.WriteSymbol( ':', SymbolSpacingKind.Required, SymbolSpacingKind.Required );
                    writer.WriteInteger( sequencePoint.StartColumn, IntegerFormat.Decimal );
                    writer.WriteSymbol( ',', SymbolSpacingKind.None, SymbolSpacingKind.None );
                    writer.WriteInteger( sequencePoint.EndColumn, IntegerFormat.Decimal );
                }

                if ( sequencePoint.Document.URL != this.lastSymbolDocument )
                {
                    writer.WriteQuotedString( sequencePoint.Document.URL, WriteStringOptions.IgnoreByteArray );
                    this.lastSymbolDocument = sequencePoint.Document.URL;
                }
            }

            writer.WriteLineBreak();
        }

        /// <summary>
        /// Writes a particular <see cref="InstructionBlock"/>.
        /// </summary>
        /// <param name="block">An <see cref="InstructionBlock"/>.</param>
        /// <param name="openBlock"><b>true</b> to force opening the block
        /// (i.e. emitting opering curle parenthesis, indenting, ...), otherwise
        /// <b>false</b>.</param>
        /// <returns>The identifier of <paramref name="block"/>.</returns>
        private void ProcessBlock( InstructionBlock block, bool openBlock )
        {
            int currentBlockId = this.nextBlockId++;
            int firstFilter = -1;

            reader.EnterInstructionBlock( block );

            openBlock = openBlock | block.HasLocalVariableSymbols | block.Comment != null;

            // Determines whether we have to open a block and/or 
            // whether there is a filter.
            if ( !openBlock && block.HasExceptionHandlers )
            {
                ExceptionHandler handler = block.FirstExceptionHandler;
                int i = 0;
                while ( handler != null )
                {
                    if ( handler.Options != ExceptionHandlingClauseOptions.Filter )
                    {
                        openBlock = true;
                    }
                    else
                    {
                        firstFilter = i;
                        break;
                        // we stop seach handlers, because handlers next to filter clauses
                        // do not need a try block
                    }

                    handler = handler.NextSiblingExceptionHandler;
                    i++;
                }
            }


            if ( openBlock )
            {
                if ( block.HasExceptionHandlers )
                {
                    this.writer.WriteKeyword( ".try" );
                    this.writer.WriteLineBreak();
                }

                this.writer.BeginBlock();
#if DEBUG
                this.writer.WriteCommentLine( block.ToString() );
#endif
            }


            if ( firstFilter >= 0 )
            {
                this.beforeNextInstruction.Add( ILWriter.GetLabel( "IL_BEGIN_TRY", currentBlockId ) + ":" );
            }

            #region Declare local variables

            bool hasStandaloneVariables = false;
            if ( ReferenceEquals( this.MethodBody, block.Parent ) )
            {
                if ( this.MethodBody.LocalVariableCount > 0 )
                {
                    for ( int i = 0; i < this.MethodBody.LocalVariableCount; i++ )
                    {
                        if ( this.localVariableHasMissingSymbol[i] || !this.localVariableHasReference[i] )
                        {
                            hasStandaloneVariables = true;
                            break;
                        }
                    }
                }
            }

            LocalVariableSymbol[] symbols = null;
            if ( block.HasLocalVariableSymbols || hasStandaloneVariables )
            {
                this.writer.WriteKeyword( ".locals" );
                if ( this.MethodBody.InitLocalVariables )
                {
                    this.writer.WriteKeyword( "init" );
                }
                this.writer.WriteSymbol( '(', SymbolSpacingKind.Required, SymbolSpacingKind.Required );
                this.writer.MarkAutoIndentLocation();

                bool first = true;
                if ( block.HasLocalVariableSymbols )
                {
                    symbols = block.LocalVariableSymbols.ToArray();
                    Array.Sort( symbols );

                    for ( int i = 0; i < symbols.Length; i++ )
                    {
                        if ( first )
                        {
                            first = false;
                        }
                        else
                        {
                            this.writer.WriteSymbol( ',' );
                            this.writer.WriteLineBreak();
                        }
                        symbols[i].WriteILDefinition( this.writer, genericMap );

                        // Detect naming ambiguities.
                        int usages;
                        this.localVariableSymbolUses.TryGetValue( symbols[i].Name, out usages );
                        this.localVariableSymbolUses[symbols[i].Name] = usages + 1;

                    }
                }
                

                if ( hasStandaloneVariables )
                {
                    bool firstStandalone = true;
                    int naturalOrdinal = 0;
                    for ( int i = 0; i < this.MethodBody.LocalVariableCount; i++ )
                    {
                        if ( this.localVariableHasMissingSymbol[i] ||
                             !this.localVariableHasReference[i] )
                        {
                            if ( !first )
                            {
                                this.writer.WriteSymbol( ',' );
                                this.writer.WriteLineBreak();
                            }
                            else
                            {
                                first = false;
                            }

                            if ( firstStandalone )
                            {
                                firstStandalone = false;
                            }

                            if ( block.HasLocalVariableSymbols || naturalOrdinal != i )
                            {
                                this.writer.WriteSymbol( '[' );
                                this.writer.WriteInteger( i, IntegerFormat.Decimal );
                                this.writer.WriteSymbol( ']' );
                            }

                            ( (ITypeSignatureInternal) this.MethodBody.GetLocalVariable( i ).Type ).WriteILReference(
                                this.writer, this.genericMap, WriteTypeReferenceOptions.WriteTypeKind );
                            this.writer.WriteIdentifier( "V_" + i.ToString( CultureInfo.InvariantCulture ) );

                            naturalOrdinal++;

#if DEBUG
                            // Write comments for debugging.
                            if ( this.localVariableHasMissingSymbol[i] )
                            {
                                this.writer.WriteComment( "reference without symbol" );
                            }

                            if ( !this.localVariableHasReference[i] )
                            {
                                this.writer.WriteComment( "no reference" );
                            }
#endif
                        }
                    }
                }
                this.writer.WriteSymbol( ')' );
                this.writer.ResetIndentLocation();
                this.writer.WriteLineBreak();
            }

            #endregion

            if ( block.HasChildrenBlocks )
            {
                InstructionBlock child = block.FirstChildBlock;
                while ( child != null )
                {
                    if ( !child.IsExceptionHandler )
                    {
                        ProcessBlock( child, false );
                    }

                    child = child.NextSiblingBlock;
                }
            }
            else if ( block.HasInstructionSequences )
            {
                InstructionSequence sequence = block.FirstInstructionSequence;
                while ( sequence != null )
                {
                    reader.EnterInstructionSequence( sequence );

                    this.writer.WriteLineBreak();
                    this.writer.Indent--;
                    this.writer.WriteLabelReference( sequence.Token );
                    this.writer.WriteSymbol( ':' );
                    this.writer.WriteLineBreak();
                    this.writer.Indent++;

#if DEBUG
                    this.writer.WriteCommentLine( sequence.ToString() );
#endif

                    while ( reader.ReadInstruction() )
                    {
                        if ( this.beforeNextInstruction.Count > 0 )
                        {
                            foreach ( string line in this.beforeNextInstruction )
                            {
                                writer.WriteRaw( line );
                                writer.WriteLineBreak();
                            }
                            this.beforeNextInstruction.Clear();
                        }

                        reader.CurrentInstruction.Write( this );
                        this.writer.WriteLineBreak();

                        if ( this.afterNextInstruction.Count > 0 )
                        {
                            foreach ( string line in this.afterNextInstruction )
                            {
                                writer.WriteRaw( line );
                                writer.WriteLineBreak();
                            }
                            this.afterNextInstruction.Clear();
                        }
                    }


                    reader.LeaveInstructionSequence();

                    sequence = sequence.NextSiblingSequence;
                }
            }

            if ( openBlock )
            {
                this.writer.EndBlock();
            }

            reader.LeaveInstructionBlock();

            if ( firstFilter >= 0 )
            {
                this.beforeNextInstruction.Add( ILWriter.GetLabel( "IL_END_TRY", currentBlockId ) + ":" );
            }

            if ( block.HasExceptionHandlers )
            {
                ExceptionHandler handler = block.FirstExceptionHandler;
                int i = 0;
                while ( handler != null )
                {
                    int handlerBlockId = 0;
                    bool useBlockSyntax = !( firstFilter >= 0 && i >= firstFilter );

                    if ( !useBlockSyntax )
                    {
                        handlerBlockId = this.nextBlockId++;
                    }

                    switch ( handler.Options )
                    {
                        case ExceptionHandlingClauseOptions.Fault:
                            if ( useBlockSyntax )
                            {
                                this.writer.WriteKeyword( "fault" );
                                this.writer.WriteLineBreak();
                                ProcessBlock( handler.HandlerBlock, true );
                            }
                            else
                            {
                                this.beforeNextInstruction.Add(
                                    ILWriter.GetLabel( "IL_BEGIN_HANDLER", handlerBlockId ) + ":" );
                                this.ProcessBlock( handler.HandlerBlock, false );
                                this.beforeNextInstruction.Add( ILWriter.GetLabel( "END_HANDLER", handlerBlockId ) + ":" );

                                this.afterNextInstruction.Add( string.Format( CultureInfo.InvariantCulture,
                                                                              ".try IL_BEGIN_TRY_{0:X4} to IL_END_TRY_{0:X4} " +
                                                                              "fault handler IL_BEGIN_HANDLER_{0:X4} to IL_END_HANDLER_{0:X4}",
                                                                              handlerBlockId ) );
                            }
                            break;

                        case ExceptionHandlingClauseOptions.Filter:
                            {
                                if ( useBlockSyntax )
                                {
                                    this.writer.WriteKeyword( "filter" );
                                    this.writer.WriteLineBreak();
                                    ProcessBlock( handler.FilterBlock, true );
                                    ProcessBlock( handler.HandlerBlock, true );
                                }
                                else
                                {
                                    
                                    this.beforeNextInstruction.Add( ILWriter.GetLabel( "IL_FILTER", handlerBlockId ) +
                                                                    ":" );

                                    this.ProcessBlock( handler.FilterBlock, false );

                                    this.beforeNextInstruction.Add(
                                        ILWriter.GetLabel( "IL_BEGIN_HANDLER", handlerBlockId ) + ":" );

                                    this.ProcessBlock( handler.HandlerBlock, false );

                                    this.beforeNextInstruction.Add(
                                        ILWriter.GetLabel( "IL_END_HANDLER", handlerBlockId ) + ":" );

                                    this.afterNextInstruction.Add(
                                        string.Format( CultureInfo.InvariantCulture,
                                                       ".try IL_BEGIN_TRY_{0:X4} to IL_END_TRY_{0:X4} " +
                                                       "filter IL_FILTER_{1:X4} handler IL_BEGIN_HANDLER_{1:X4} to IL_END_HANDLER_{1:X4}",
                                                       currentBlockId,
                                                       handlerBlockId ) );
                                }
                            }
                            break;

                        case ExceptionHandlingClauseOptions.Finally:
                            if ( useBlockSyntax )
                            {
                                this.writer.WriteKeyword( "finally" );
                                this.writer.WriteLineBreak();
                                ProcessBlock( handler.HandlerBlock, true );
                            }
                            else
                            {
                                this.beforeNextInstruction.Add(
                                    ILWriter.GetLabel( "IL_BEGIN_HANDLER", handlerBlockId ) + ":" );
                                this.ProcessBlock( handler.HandlerBlock, false );
                                this.beforeNextInstruction.Add( ILWriter.GetLabel( "IL_END_HANDLER", handlerBlockId ) +
                                                                ":" );

                                this.afterNextInstruction.Add(
                                    string.Format(
                                        CultureInfo.InvariantCulture,
                                        ".try IL_BEGIN_TRY_{0:X4} to IL_END_TRY_{0:X4} " +
                                        "finally handler IL_BEGIN_HANDLER_{1:X4} to IL_END_HANDLER_{1:X4}",
                                        currentBlockId, handlerBlockId ) );
                            }
                            break;

                        case ExceptionHandlingClauseOptions.Clause:
                            // Get the caught type.
                            this.InitializeMemoryWriter();
                            ( (ITypeSignatureInternal) handler.CatchType ).WriteILReference( this.memoryIlWriter,
                                                                                             this.genericMap,
                                                                                             WriteTypeReferenceOptions.
                                                                                                 None );
                            this.memoryIlWriter.Flush();
                            this.memoryIlWriterStringWriter.Flush();

                            if ( useBlockSyntax )
                            {
                                this.writer.WriteKeyword( "catch " + memoryIlWriterStringWriter );
                                this.writer.WriteLineBreak();
                                ProcessBlock( handler.HandlerBlock, true );
                                break;
                            }
                            else
                            {
                                this.beforeNextInstruction.Add(
                                    ILWriter.GetLabel( "IL_BEGIN_HANDLER", handlerBlockId ) + ":" );
                                this.ProcessBlock( handler.HandlerBlock, false );
                                this.beforeNextInstruction.Add( ILWriter.GetLabel( "IL_END_HANDLER", handlerBlockId ) +
                                                                ":" );

                                this.afterNextInstruction.Add(
                                    string.Format( CultureInfo.InvariantCulture,
                                                   ".try IL_BEGIN_TRY_{0:X4} to IL_END_TRY_{0:X4} " +
                                                   "catch {2} handler IL_BEGIN_HANDLER_{1:X4} to IL_END_HANDLER_{1:X4}",
                                                   currentBlockId, handlerBlockId, memoryIlWriterStringWriter.ToString() ) );
                            }
                            break;
                    }

                    handler = handler.NextSiblingExceptionHandler;
                    i++;
                }
            }

            // Pop local variable symbol usage count.
            if ( symbols != null )
            {
                foreach ( LocalVariableSymbol symbol in symbols )
                {
                    this.localVariableSymbolUses[symbol.Name] = this.localVariableSymbolUses[symbol.Name] - 1;
                }
            }

            return;
        }


        public void WriteIL()
        {
            this.reader = this.MethodBody.CreateInstructionReader();

            ISymbolDocument anySymbolDocument = this.MethodBody.AnySymbolDocument;
            if ( anySymbolDocument != null )
            {
                writer.WriteKeyword( ".language" );
                writer.WriteQuotedString( "{" + anySymbolDocument.Language.ToString() + "}" );
                writer.WriteSymbol( ',' );
                writer.WriteQuotedString( "{" + anySymbolDocument.LanguageVendor.ToString() + "}" );
                writer.WriteSymbol( ',' );
                writer.WriteQuotedString( "{" + anySymbolDocument.DocumentType.ToString() + "}" );
                writer.WriteLineBreak();
                writer.WriteLineBreak();
            }

            ProcessBlock( this.MethodBody.RootInstructionBlock, false );

            if ( this.beforeNextInstruction.Count > 0 )
            {
                foreach ( string line in this.beforeNextInstruction )
                {
                    writer.WriteRaw( line );
                    writer.WriteLineBreak();
                }
                this.beforeNextInstruction.Clear();
            }


            this.reader = null;
        }
    }

    /// <summary>
    /// Exposes a method that writes the IL definition of the current instance
    /// to an <see cref="ILWriter"/>.
    /// </summary>
    public interface IWriteILDefinition
    {
        /// <summary>
        /// Writes the IL definition of the current instance.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        void WriteILDefinition( ILWriter writer );
    }
}