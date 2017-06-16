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

#if DEBUG
#define NICE
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using PostSharp.CodeModel;
using PostSharp.Collections;
using Interop = System.Runtime.InteropServices;

namespace PostSharp.ModuleWriter
{
    /// <summary>
    /// Provides low-level methods to writer text MSIL code.
    /// </summary>
    public sealed class ILWriter : IDisposable
    {
        #region Static fields

        /// <summary>
        /// Array of octal digits.
        /// </summary>
        private static readonly char[] octDigits = new[] {'0', '1', '2', '3', '4', '5', '6', '7'};

    
        /// <summary>
        /// Maps an <see cref="OpCodeNumber"/> to its name.
        /// </summary>
        private static readonly OpCodeDictionary<string> opCodeToString;

        /// <summary>
        /// Length of an IL instruction in characters.
        /// </summary>
        private const int instructionLen = 10;


#if NICE
       
        /// <summary>
        /// Regular expression determining whether a string is a simple identifier.
        /// </summary>
        private static readonly Regex simpleIdentifierRegEx =
         new Regex(@"^[a-zA-Z0-9\$`@_\.][a-zA-Z0-9\$`@\._\?]*$", RegexOptions.Compiled);

        /// <summary>
        /// Regular expression determining whether a string is a dotted name.
        /// </summary>
        private static readonly Regex dottedNameRegEx =
            new Regex(@"^(\.ctor|\.cctor|(?![0-9])[a-zA-Z0-9\$`@_\?]+(\.(?![0-9])[a-zA-Z0-9\$`@_\?]+)*)$", RegexOptions.Compiled);

        /// <summary>
        /// List of reserved keywords. Parsed at runtime into <see cref="reservedKeywords"/>.
        /// </summary>
        private const string reservedKeywordsString = @"	.addon .assembly __.cctor__ .class .corflags __.ctor__ .custom .data .emitbyte .entrypoint .event .export .field
				.file .fire .get .hash .imagebase .import .language .line .locale .localized .locals .manifestres
				.maxstack .method .module .mresource .namespace .other .override .pack .param .pdirect .permission
				.permissionset .property .publickey .publickeytoken .removeon .set .size .subsystem .try .ver
				.vtable .vtentry .vtfixup .zeroinit ^THE_END^ abstract add add.ovf add.ovf.un algorithm 
				and ansi any arglist array as assembly assert at auto autochar beforefieldinit beq beq.s bge bge.s bge.un
				bge.un.s bgt bgt.s bgt.un bgt.un.s ble ble.s ble.un ble.un.s blob blob_object blt blt.s blt.un
				blt.un.s bne.un bne.un.s bool box br br.s break brfalse brfalse.s brinst brinst.s brnull brnull.s
				brtrue brtrue.s brzero brzero.s bstr bytearray byvalstr call calli callmostderived callvirt carray
				castclass catch cdecl ceq cf cgt cgt.un char cil ckfinite class clsid clt clt.un const conv.i conv.i1
				conv.i2 conv.i4 conv.i8 conv.ovf.i conv.ovf.i.un conv.ovf.i1 conv.ovf.i1.un conv.ovf.i2 conv.ovf.i2.un
				conv.ovf.i4 conv.ovf.i4.un conv.ovf.i8 conv.ovf.i8.un conv.ovf.u conv.ovf.u.un conv.ovf.u1 conv.ovf.u1.un
				conv.ovf.u2 conv.ovf.u2.un conv.ovf.u4 conv.ovf.u4.un conv.ovf.u8 conv.ovf.u8.un conv.r.un conv.r4 conv.r8
				conv.u conv.u1 conv.u2 conv.u4 conv.u8 cpblk cpobj currency custom date decimal default demand
				deny div div.un dup endfault endfilter endfinally endmac enum explicit extends extern false
				famandassem family famorassem fastcall fault field filetime filter final finally fixed float
				float32 float64 forwardref fromunmanaged handler hidebysig hresult idispatch il illegal implements 
				implicitcom implicitres import in inheritcheck init initblk initobj initonly instance int int16 uint16
				int32 uint32 int64 uint64 int8 uint8 interface internalcall isinst iunknown jmp lasterr ldarg ldarg.0 ldarg.1 ldarg.2
				ldarg.3 ldarg.s ldarga ldarga.s ldc.i4 ldc.i4.0 ldc.i4.1 ldc.i4.2 ldc.i4.3 ldc.i4.4 ldc.i4.5  ldc.i4.6
				ldc.i4.7 ldc.i4.8 ldc.i4.M1 ldc.i4.m1 ldc.i4.s ldc.i8 ldc.r4 ldc.r8 ldelem.i ldelem.i1 ldelem.i2 ldelem.i4
				ldelem.i8 ldelem.r4 ldelem.r8 ldelem.ref ldelem.u1 ldelem.u2 ldelem.u4 ldelem.u8 ldelema ldfld ldflda
				ldftn ldind.i ldind.i1 ldind.i2 ldind.i4 ldind.i8 ldind.r4 ldind.r8 ldind.ref ldind.u1 ldind.u2 ldind.u4
				ldind.u8 ldlen ldloc ldloc.0 ldloc.1 ldloc.2 ldloc.3 ldloc.s ldloca ldloca.s ldnull ldobj ldsfld ldsflda ldstr
				ldtoken ldvirtftn  leave leave.s linkcheck literal localloc lpstr lpstruct lptstr lpvoid lpwstr managed
				marshal method mkrefany modopt modreq mul mul.ovf mul.ovf.un native neg nested newarr newobj newslot noappdomain
				noinlining nomachine nomangle nometadata noncasdemand noncasinheritance noncaslinkdemand nop noprocess
				not not_in_gc_heap notremotable notserialized null nullref object objectref opt optil or out permitonly
				pinned pinvokeimpl pop prefix1 prefix2 prefix3 prefix4 prefix5 prefix6 prefix7 prefixref prejitdeny prejitgrant
				preservesig private privatescope protected public readonly record refany refanytype refanyval rem rem.un
				reqmin reqopt reqrefuse reqsecobj request ret rethrow rtspecialname runtime safearray sealed
				sequential serializable shl shr shr.un sizeof specialname starg starg.s static stdcall
				stelem.i stelem.i1 stelem.i2 stelem.i4 stelem.i8 stelem.r4 stelem.r8 stelem.ref stfld stind.i stind.i1 stind.i2
				stind.i4 stind.i8 stind.r4 stind.r8 stind.ref stloc stloc.0 stloc.1 stloc.2 stloc.3 stloc.s stobj storage
				stored_object stream streamed_object string struct stsfld sub sub.ovf sub.ovf.un switch synchronized
				syschar sysstring tail. tbstr thiscall throw tls to true typedref unaligned. unbox unicode
				unmanaged unmanagedexp unsigned unused userdefined value valuetype vararg variant vector virtual void
				volatile. wchar winapi with xor type flags property callconv library strict alignment error uint off x86 legacy on";

        /// <summary>
        /// Set of reserved keywords.
        /// </summary>
        private static readonly Set<string> reservedKeywords = new Set<string>( 100 );

           /// <summary>
        /// Number of spaces per indent level.
        /// </summary>
        private const int indentSpaces = 2;

        
        /// <summary>
        /// Determines whether the last written symbol requires a space.
        /// </summary>
        private SymbolSpacingKind lastSymbolKind;

        /// <summary>
        /// Current horizontal position.
        /// </summary>
        private int currentHorizontalPosition;

        /// <summary>
        /// String to be added at the beginning of the next line.
        /// </summary>
        /// <remarks>
        /// This field should contain a number of spaces equal to the product of
        /// <see cref="currentAutoIndentLevel"/> and <see cref="indentSpaces"/>.
        /// It is used to avoid to create a new tempory string
        /// at each line.
        /// </remarks>
        private string currentAutoIndentString = "";

        /// <summary>
        /// Current identation level.
        /// </summary>
        private int currentAutoIndentLevel;

        /// <summary>
        /// Determines whether the current position it at a beginning.
        /// </summary>
        private bool isLineStart = true;
#endif
        /// <summary>
        /// Caches the invariant culture.
        /// </summary>
        private static readonly CultureInfo invariantCulture = CultureInfo.InvariantCulture;

        #endregion

        #region Fields

        /// <summary>
        /// Target writer.
        /// </summary>
        private readonly TextWriter textWriter;

     

        /// <summary>
        /// Default maximal size of a line above which the <see cref="WriteConditionalLineBreak()"/>
        /// method issues a line break.
        /// </summary>
        public const int BreakMargin = 39;

     
        /// <summary>
        /// Options.
        /// </summary>
        private readonly ILWriterOptions options = new ILWriterOptions();

        private readonly bool disposeTextWriter;

        #endregion

        /// <summary>
        /// Initializes the <see cref="ILWriter"/> type.
        /// </summary>
        [SuppressMessage( "Microsoft.Performance", "CA1810",
            Justification = "Static constructor is required." )]
        static ILWriter()
        {
            opCodeToString = new OpCodeDictionary<string>();

            string[] names = Enum.GetNames( typeof(OpCodeNumber) );
            Array values = Enum.GetValues( typeof(OpCodeNumber) );
            for ( int i = 0 ; i < names.Length ; i++ )
            {
                if ( !names[i].StartsWith( "_" ) )
                {
                    opCodeToString.SetValue( (OpCodeNumber) values.GetValue( i ),
                                             names[i].ToLowerInvariant().Replace( '_', '.' ).PadRight( instructionLen ) );
                }
            }

#if NICE
            foreach ( string keyword in reservedKeywordsString.Split( ' ', '\t', '\n', '\r' ) )
            {
                if ( !string.IsNullOrEmpty( keyword ) )
                {
                    reservedKeywords.Add( keyword );
                }
            }
#endif
        }

        /// <summary>
        /// Initializes a new <see cref="ILWriter"/>.
        /// </summary>
        /// <param name="writer">>The <see cref="TextWriter"/> where the IL code shall
        /// be written.</param>
        /// <remarks>The <see cref="TextWriter"/> is not disposed automatically when the
        /// new <see cref="ILWriter"/> instance is disposed. The second constructor overload
        /// offers this feature.</remarks>
         public ILWriter( TextWriter writer ) : this (writer, false)
         {

         }


        /// <summary>
        /// Initializes a new <see cref="ILWriter"/> and specifies whether the underlying
        /// <see cref="TextWriter"/> should be automatically disposed.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> where the IL code shall
        /// be written.</param>
        /// <param name="disposeTextWriter"><b>true</b> if <paramref name="writer"/>
        /// should be disposed when the new <see cref="ILWriter"/> is disposed, otherwise <b>false</b>.</param>
        public ILWriter( TextWriter writer, bool disposeTextWriter )
        {
            this.textWriter = writer;
            this.disposeTextWriter = disposeTextWriter;
        }

        /// <summary>
        /// Gets the underlying <see cref="TextWriter"/>.
        /// </summary>
        public TextWriter TextWriter { get { return this.textWriter;  } }

        /// <inheritdoc />
        public void Dispose()
        {
            if ( this.disposeTextWriter )
            {
                this.textWriter.Dispose();
            }
        }

        /// <summary>
        /// Gets the options of the current <see cref="ILWriter"/>.
        /// </summary>
        public ILWriterOptions Options { get { return this.options; } }

#if NICE
        /// <summary>
        /// Determines the kind of spacing that a symbol needs before itself.
        /// </summary>
        /// <param name="symbol">A symbol.</param>
        /// <returns><see cref="SymbolSpacingKind.Required"/>,
        /// <see cref="SymbolSpacingKind.IfWord"/> or <see cref="SymbolSpacingKind.None"/>.</returns>
        private static SymbolSpacingKind SymbolNeedsSpaceBefore( char symbol )
        {
            switch ( symbol )
            {
                case '"':
                case '\'':
                case '=':
                case '{':
                    return SymbolSpacingKind.Required;

                case '!':
                    return SymbolSpacingKind.IfWord;

                default:
                    return SymbolSpacingKind.None;
            }
        }


        /// <summary>
        /// Determines the kind of spacing that a symbol needs after itself.
        /// </summary>
        /// <param name="symbol">A symbol.</param>
        /// <returns><see cref="SymbolSpacingKind.Required"/>,
        /// <see cref="SymbolSpacingKind.IfWord"/> or <see cref="SymbolSpacingKind.None"/>.</returns>
        private static SymbolSpacingKind SymbolNeedsSpaceAfter( char symbol )
        {
            switch ( symbol )
            {
                case '.':
                case ':':
                case '[':
                case '(':
                case '!':
                case '<':
                case '/':
                case '{':
                    return SymbolSpacingKind.None;

                default:
                    return SymbolSpacingKind.IfWord;
            }
        }
#endif

        /// <summary>
        /// Flushes the inner <see cref="TextWriter"/>.
        /// </summary>
        public void Flush()
        {
            this.textWriter.Flush();
        }
        #if NICE

        /// <summary>
        /// Inserts spaces if needed. To be called before a symbol or a word is emitted.
        /// </summary>
        /// <param name="firstSymbolKind">The kind of spacing required by the <i>first</i> symbol 
        /// of the expression that will be written.</param>
        /// <param name="lastSymbolKind">The kind of spacing required by the <i>after</i> symbol 
        /// of the expression that will be written.</param>
        private void ManageSpace( SymbolSpacingKind firstSymbolKind, SymbolSpacingKind lastSymbolKind )
        {
            if ( !this.isLineStart )
            {
                if ( this.lastSymbolKind == SymbolSpacingKind.Required ||
                     firstSymbolKind == SymbolSpacingKind.Required ||
                     ( this.lastSymbolKind == SymbolSpacingKind.IfWord && firstSymbolKind == SymbolSpacingKind.IfWord ) )
                {
                    this.WriteRaw( ' ' );
                }
            }
            this.lastSymbolKind = lastSymbolKind;
        }

        /// <summary>
        /// Inserts the identation string if we are just after a line break.
        /// </summary>
        private void ManageLineStart()
        {
            if ( this.isLineStart )
            {
                this.textWriter.Write( this.currentAutoIndentString );
                this.isLineStart = false;
                this.currentHorizontalPosition = this.currentAutoIndentString.Length;

#if TRACE
                if ( this.options.TraceEnabled && Trace.ILWriter.Enabled )
                {
                    Trace.ILWriter.WriteLine( "" );
                }
#endif
            }

        }
#endif


        /// <summary>
        /// Writes a file name.
        /// </summary>
        /// <param name="text">A string.</param>
        public void WriteFileName( string text )
        {
            this.WriteDottedName( text );
        }


        /// <summary>
        /// Writes a dotted name.
        /// </summary>
        /// <param name="text">A string.</param>
        public void WriteDottedName( string text )
        {
#if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.None );

            if ( !text.Contains( "=" ) && !reservedKeywords.Contains( text ) && dottedNameRegEx.IsMatch( text ) )
            {
                this.WriteRaw( text );
            }
            else
            {
                this.WriteQuotedString( text, '\'', 0, WriteStringOptions.IgnoreByteArray );
            }

            this.lastSymbolKind = SymbolSpacingKind.IfWord;
#else
            this.WriteQuotedString(text, '\'', 0, WriteStringOptions.IgnoreByteArray);
#endif
        }

        /// <summary>
        /// Writes a space.
        /// </summary>
        public void WriteSpace()
        {
#if NICE
            this.lastSymbolKind = SymbolSpacingKind.Required;
#else
            this.WriteRaw( ' ' );
#endif
        }

        /// <summary>
        /// Writes an identifier.
        /// </summary>
        /// <param name="text">A string.</param>
        public void WriteIdentifier( string text )
        {
#if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.IfWord );
#else
            this.WriteRaw( ' ' );
#endif

            this.InternalWriteIdentifier( text, true );
        }

        /// <summary>
        /// Writes an identifier (without managing spaces).
        /// </summary>
        /// <param name="text">A string.</param>
        /// <param name="checkReservedKeyword">Whether it should be checked that
        /// <paramref name="text"/> is a reserved keyword.</param>
        private void InternalWriteIdentifier( string text, bool checkReservedKeyword )
        {
#if NICE
            if ( simpleIdentifierRegEx.IsMatch( text ) && !( checkReservedKeyword && reservedKeywords.Contains( text ) ) )
            {
                this.WriteRaw( text );
            }
            else
            {
                this.WriteQuotedString( text, '\'', 0, WriteStringOptions.IgnoreByteArray );
            }
#else
            this.WriteQuotedString(text, '\'', 0, WriteStringOptions.IgnoreByteArray);
#endif
        }

        /// <summary>
        /// Writes an integer in octal format.
        /// </summary>
        /// <param name="padding">Number of digits to display.</param>
        /// <param name="value">An integer.</param>
        private void WriteOct( uint value, int padding )
        {
            int i;
            char[] characters = new char[12];

            for ( i = 0 ; i < 11 ; i++ )
            {
                uint digit = value%8;
                value = value >> 3;
                characters[11 - i] = octDigits[digit];
                if ( value == 0 )
                {
                    i++;
                    break;
                }
            }

            if ( i < padding )
            {
                for ( int j = padding; j < i; i++)
                {
                    this.WriteRaw( '0');
                }
            }

            this.WriteRaw( characters, 12 - i, i );
        }

        /// <summary>
        /// Writes a <see cref="CallingConvention"/>.
        /// </summary>
        /// <param name="callingConvention">A <see cref="CallingConvention"/>.</param>
        public void WriteCallConvention( CallingConvention callingConvention )
        {
            if ( ( callingConvention & CallingConvention.HasThis ) != 0 )
            {
                this.WriteKeyword( "instance" );
            }

            if ( ( callingConvention & CallingConvention.ExplicitThis ) != 0 )
            {
                this.WriteKeyword( "explicit" );
            }

            this.WriteCallKind( callingConvention );
        }

        /// <summary>
        /// Writes the call kind part of a <see cref="CallingConvention"/>.
        /// </summary>
        /// <param name="callingKind">A <see cref="CallingConvention"/>.</param>
        public void WriteCallKind( CallingConvention callingKind )
        {
            switch ( callingKind & CallingConvention.CallingConventionMask )
            {
                case CallingConvention.Default:
                    break;

                case CallingConvention.VarArg:
                    this.WriteKeyword( "vararg" );
                    break;

                case CallingConvention.GenericInstance:
                    break;

                case CallingConvention.UnmanagedStdCall:
                    this.WriteKeyword( "unmanaged stdcall" );
                    break;

                case CallingConvention.UnmanagedThisCall:
                    this.WriteKeyword( "unmanaged thiscall" );
                    break;

                case CallingConvention.UnmanagedCdecl:
                    this.WriteKeyword( "unmanaged cdecl" );
                    break;

                case CallingConvention.UnmanagedFastCall:
                    this.WriteKeyword("unmanaged fastcall");
                    break;

           
                default:
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "Unexpected calling convention." );
            }
        }

        /// <overloads>Writes an array of bytes.</overloads>
        /// <summary>
        /// Writes an array of bytes without issuing a line break before the first byte.
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        public void WriteBytes( byte[] bytes )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( bytes, "bytes" );

            #endregion

            this.WriteBytes( bytes, (this.options.Compatibility & ILWriterCompatibility.ForceBreakBeforeBlobs) != 0);
        }

        /// <summary>
        /// Writes an array of bytes and specifies whether a line break should
        /// be issued before the first byte.
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="breakBefore">Whether a line break should be issued before
        /// the first byte.</param>
        public void WriteBytes( byte[] bytes, bool breakBefore )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( bytes, "bytes" );

            #endregion

#if NICE
            this.ManageSpace( SymbolSpacingKind.Required, SymbolSpacingKind.Required );
            this.WriteRaw( '(' );
#else
            this.WriteRaw(" (");
#endif

            if ( breakBefore )
            {
                this.WriteLineBreak();
                this.Indent++;
            }
            else
            {
                this.MarkAutoIndentLocation();
            }
            for ( int i = 0 ; i < bytes.Length ; i++ )
            {
                if ( ( i%16 == 0 ) && i != 0 )
                {
                    this.WriteLineBreak();
                }
                else
                {
                    this.WriteRaw( ' ' );
                }

                this.WriteRaw( ( (uint) bytes[i] ).ToString( "X2", CultureInfo.InvariantCulture ) );
            }
            this.WriteRaw( ")" );
            this.ResetIndentLocation();
            if ( breakBefore )
            {
                this.Indent--;
            }
        }

        /// <overloads>Writes a <see cref="sbyte"/>.</overloads>
        /// <summary>
        /// Writes a signed byte in hexadecimal.
        /// </summary>
        /// <param name="value">A <see cref="sbyte"/>.</param>
        public void WriteInteger( sbyte value )
        {
            this.WriteInteger( value, IntegerFormat.HexLower );
        }

        /// <summary>
        /// Writes a <see cref="sbyte"/> and specifies whether the format is hexadecimal or decimal.
        /// </summary>
        /// <param name="value">A <see cref="sbyte"/>.</param>
        /// <param name="format">The format in which the value has to be written.</param>
        public void WriteInteger( sbyte value, IntegerFormat format )
        {
            string formatString;
            switch ( format )
            {
                case IntegerFormat.Decimal:
                    formatString = "{0:d}";
                    break;

                case IntegerFormat.HexLower:
                    formatString = "0x{0:x2}";
                    break;

                case IntegerFormat.HexUpper:
                    formatString = "0x{0:X2}";
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( format, "format" );
            }
#if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.IfWord );
#else
            this.WriteRaw( ' ' );
#endif 

            this.WriteRaw( string.Format( invariantCulture, formatString, value ) );
        }

        /// <overloads>Writes a <see cref="short"/>.</overloads>
        /// <summary>
        /// Writes an <see cref="short"/> in hexadecimal.
        /// </summary>
        /// <param name="value">A <see cref="short"/>.</param>
        public void WriteInteger( short value )
        {
            this.WriteInteger( value, IntegerFormat.HexLower );
        }


        /// <summary>
        /// Writes a <see cref="short"/> and specifies whether the format is hexadecimal or decimal.
        /// </summary>
        /// <param name="value">A <see cref="short"/>.</param>
        /// <param name="format">The format in which the value has to be written.</param>
        public void WriteInteger( short value, IntegerFormat format )
        {
            string formatString;
            switch ( format )
            {
                case IntegerFormat.Decimal:
                    formatString = "{0:d}";
                    break;

                case IntegerFormat.HexLower:
                    formatString = "0x{0:x4}";
                    break;

                case IntegerFormat.HexUpper:
                    formatString = "0x{0:X4}";
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( format, "format" );
            }
#if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.IfWord );
#else
            this.WriteRaw( ' ' );
#endif

            this.WriteRaw( string.Format( invariantCulture, formatString, value ) );
        }


        /// <overloads>Writes a <see cref="int"/>.</overloads>
        /// <summary>
        /// Writes an <see cref="int"/> in hexadecimal.
        /// </summary>
        /// <param name="value">A <see cref="int"/>.</param>
        public void WriteInteger( int value )
        {
            this.WriteInteger( value, IntegerFormat.HexLower );
        }


        /// <summary>
        /// Writes a <see cref="int"/> and specifies whether the format is hexadecimal or decimal.
        /// </summary>
        /// <param name="value">A <see cref="int"/>.</param>
        /// <param name="format">The format in which the value has to be written.</param>
        public void WriteInteger( int value, IntegerFormat format )
        {
            string formatString;
            switch ( format )
            {
                case IntegerFormat.Decimal:
                    formatString = "{0:d}";
                    break;

                case IntegerFormat.HexLower:
                    formatString = "0x{0:x8}";
                    break;

                case IntegerFormat.HexUpper:
                    formatString = "0x{0:X8}";
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( format, "format" );
            }
 #if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.IfWord );
#else
            this.WriteRaw( ' ' );
#endif
            this.WriteRaw( string.Format( invariantCulture, formatString, value ) );
        }


        /// <overloads>Writes a <see cref="byte"/>.</overloads>
        /// <summary>
        /// Writes a <see cref="byte"/> in hexadecimal.
        /// </summary>
        /// <param name="value">A <see cref="byte"/>.</param>
        public void WriteInteger( byte value )
        {
            this.WriteInteger( value, IntegerFormat.HexLower );
        }

        /// <summary>
        /// Writes a <see cref="byte"/> and specifies whether the format is hexadecimal or decimal.
        /// </summary>
        /// <param name="value">A <see cref="byte"/>.</param>
        /// <param name="format">The format in which the value has to be written.</param>
        public void WriteInteger( byte value, IntegerFormat format )
        {
            string formatString;
            switch ( format )
            {
                case IntegerFormat.Decimal:
                    formatString = "{0:d}";
                    break;

                case IntegerFormat.HexLower:
                    formatString = "0x{0:x2}";
                    break;

                case IntegerFormat.HexUpper:
                    formatString = "0x{0:X2}";
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( format, "format" );
            }
#if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.IfWord );
#else
            this.WriteRaw( ' ' );
#endif
            this.WriteRaw( string.Format( invariantCulture, formatString, value ) );
        }

        /// <overloads>Writes a <see cref="ushort"/>.</overloads>
        /// <summary>
        /// Writes a <see cref="ushort"/> in hexadecimal.
        /// </summary>
        /// <param name="value">A <see cref="ushort"/>.</param>
        public void WriteInteger( UInt16 value )
        {
            this.WriteInteger( value, IntegerFormat.HexLower );
        }

        /// <summary>
        /// Writes a <see cref="UInt16"/> and specifies whether the format is hexadecimal or decimal.
        /// </summary>
        /// <param name="value">A <see cref="UInt16"/>.</param>
        /// <param name="format">The format in which the value has to be written.</param>
        public void WriteInteger( ushort value, IntegerFormat format )
        {
            string formatString;
            switch ( format )
            {
                case IntegerFormat.Decimal:
                    formatString = "{0:d}";
                    break;

                case IntegerFormat.HexLower:
                    formatString = "0x{0:x4}";
                    break;

                case IntegerFormat.HexUpper:
                    formatString = "0x{0:X4}";
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( format, "format" );
            }
#if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.IfWord );
#else
            this.WriteRaw( ' ' );
#endif
            
            this.WriteRaw( string.Format( invariantCulture, formatString, value ) );
        }

        /// <overloads>Writes a <see cref="UInt32"/>.</overloads>
        /// <summary>
        /// Writes a <see cref="UInt32"/> in hexadecimal.
        /// </summary>
        /// <param name="value">A <see cref="UInt32"/>.</param>
        public void WriteInteger( UInt32 value )
        {
            this.WriteInteger( value, IntegerFormat.HexLower );
        }

        /// <summary>
        /// Writes a <see cref="UInt32"/> and specifies whether the format is hexadecimal or decimal.
        /// </summary>
        /// <param name="value">A <see cref="UInt32"/>.</param>
        /// <param name="format">The format in which the value has to be written.</param>
        public void WriteInteger( uint value, IntegerFormat format )
        {
            string formatString;
            switch ( format )
            {
                case IntegerFormat.Decimal:
                    formatString = "{0:d}";
                    break;

                case IntegerFormat.HexLower:
                    formatString = "0x{0:x8}";
                    break;

                case IntegerFormat.HexUpper:
                    formatString = "0x{0:X8}";
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( format, "format" );
            }
#if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.IfWord );
#else
            this.WriteRaw( ' ' );
#endif
            this.WriteRaw( string.Format( invariantCulture, formatString, value ) );
        }

        /// <overloads>Writes a <see cref="Int64"/>.</overloads>
        /// <summary>
        /// Writes a <see cref="Int64"/> in hexadecimal.
        /// </summary>
        /// <param name="value">A <see cref="Int64"/>.</param>
        public void WriteInteger( Int64 value )
        {
            this.WriteInteger( value, IntegerFormat.HexLower );
        }

        /// <summary>
        /// Writes a <see cref="Int64"/> and specifies whether the format is hexadecimal or decimal.
        /// </summary>
        /// <param name="value">A <see cref="Int64"/>.</param>
        /// <param name="format">The format in which the value has to be written.</param>
        public void WriteInteger( long value, IntegerFormat format )
        {
            string formatString;
            switch ( format )
            {
                case IntegerFormat.Decimal:
                    formatString = "{0:d}";
                    break;

                case IntegerFormat.HexLower:
                    formatString = "0x{0:x}";
                    break;

                case IntegerFormat.HexUpper:
                    formatString = "0x{0:X}";
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( format, "format" );
            }
#if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.IfWord );
#else
            this.WriteRaw( ' ' );
#endif
            this.WriteRaw( string.Format( invariantCulture, formatString, value ) );
        }

        /// <overloads>Writes a <see cref="UInt64"/>.</overloads>
        /// <summary>
        /// Writes an <see cref="UInt64"/> in hexadecimal.
        /// </summary>
        /// <param name="value">A <see cref="UInt64"/>.</param>
        public void WriteInteger( UInt64 value )
        {
            this.WriteInteger( value, IntegerFormat.HexLower );
        }

        /// <summary>
        /// Writes a <see cref="UInt64"/> and specifies whether the format is hexadecimal or decimal.
        /// </summary>
        /// <param name="value">A <see cref="UInt64"/>.</param>
        /// <param name="format">The format in which the value has to be written.</param>
        public void WriteInteger( ulong value, IntegerFormat format )
        {
            string formatString;
            switch ( format )
            {
                case IntegerFormat.Decimal:
                    formatString = "{0:d}";
                    break;

                case IntegerFormat.HexLower:
                    formatString = "0x{0:x}";
                    break;

                case IntegerFormat.HexUpper:
                    formatString = "0x{0:X}";
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( format, "format" );
            }
#if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.IfWord );
#else
            this.WriteRaw( ' ' );
#endif
            
            this.WriteRaw( string.Format( invariantCulture, formatString, value ) );
        }


        /// <summary>
        /// Writes a <see cref="Single"/>.
        /// </summary>
        /// <param name="value">A <see cref="Single"/>.</param>
        public void WriteSingle( float value )
        {
#if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.IfWord );
#else
            this.WriteRaw(' ');
#endif
            uint intValue;

            unsafe
            {
                intValue = *( (uint*) &value );
            }

            if ( float.IsNegativeInfinity( value ) )
            {
                this.WriteRaw( "(00 00 80 FF)" );
            }
            else if ( float.IsPositiveInfinity( value ) )
            {
                this.WriteRaw( "(00 00 80 7F)" );
            }
            else if ( float.IsNaN( value ) )
            {
                this.WriteRaw( "(00 00 C0 FF)" );
            }
            else if ( value == 0 )
            {
                if ( ( intValue >> 24 ) != 0 )
                {
                    this.WriteRaw( "-0.0" );
                }
                else
                {
                    this.WriteRaw( "0.0" );
                }
            }
            else
            {
                string s = PadFloat(value.ToString("r", invariantCulture));
                
                if ((this.options.Compatibility & ILWriterCompatibility.LowerCaseFloat) != 0)
                {
                    s = s.ToLowerInvariant();
            	}
                this.WriteRaw( s );
        }
        }

        /// <summary>
        /// Adds a dot at the end of a formatted real value where necessary.
        /// </summary>
        /// <param name="value">A formatted real value.</param>
        /// <returns>A well-formatted real value.</returns>
        private static string PadFloat( string value )
        {
            if ( !value.Contains( "." ) && !value.Contains( "e" ) && !value.Contains( "E" ) )
            {
                return value + ".";
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Writes a <see cref="Double"/>.
        /// </summary>
        /// <param name="value">A <see cref="Double"/>.</param>
        public void WriteDouble( double value )
        {
#if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.IfWord );
#else
            this.WriteRaw(' ');
#endif
            ulong longDoubleValue;

            unsafe
            {
                longDoubleValue = *( (ulong*) &value );
            }

            if ( double.IsNegativeInfinity( value ) )
            {
                this.WriteRaw( "(00 00 00 00 00 00 F0 FF)" );
            }
            else if ( double.IsPositiveInfinity( value ) )
            {
                this.WriteRaw( "(00 00 00 00 00 00 F0 7F)" );
            }
            else if ( double.IsNaN( value ) )
            {
                this.WriteRaw( "(00 00 00 00 00 00 F8 FF)" );
            }
            else if ( value == 0 )
            {
                if ( ( longDoubleValue >> 56 ) != 0 )
                {
                    this.WriteRaw( "-0.0" );
                }
                else
                {
                    this.WriteRaw( "0.0" );
                }
            }
            else
            {
                string s = PadFloat(value.ToString(
                    (this.options.Compatibility & ILWriterCompatibility.AllDigitsFloat) != 0 ? "g17" :
                    "r", invariantCulture));

                if ((this.options.Compatibility & ILWriterCompatibility.LowerCaseFloat) != 0)
                {
                    s = s.ToLowerInvariant();
            }
                this.WriteRaw(s);
        }
        }

        /// <summary>
        /// Writes a quoted string.
        /// </summary>
        /// <param name="text">A text.</param>
        public void WriteQuotedString( string text )
        {
            this.WriteQuotedString( text, WriteStringOptions.None );
        }

        /// <summary>
        /// Writes a single quoted string (passed as a <see cref="string"/>).
        /// </summary>
        /// <param name="text">A text.</param>
        /// <param name="options">Options.</param>
        public void WriteQuotedString( string text, WriteStringOptions options )
        {
            this.WriteQuotedString( (LiteralString) text, options );
        }

        /// <summary>
        /// Writes a single quoted string (passed as a <see cref="LiteralString"/>).
        /// </summary>
        /// <param name="text">A text.</param>
        /// <param name="options">Options.</param>
        public void WriteQuotedString( LiteralString text, WriteStringOptions options )
        {
            char quote = ( options & WriteStringOptions.DoubleQuoted ) != 0 ? '"' : '\'';
#if NICE
            this.ManageSpace( SymbolSpacingKind.Required, SymbolSpacingKind.Required );
#else
            this.WriteRaw( ' ' );
#endif            
            this.WriteQuotedString( text, quote, 0, options );
        }

        private void WriteStringAsByteArray(LiteralString text)
        {
            int length = text.Length;
            this.WriteKeyword("bytearray");
            byte[] bytes = new byte[length * 2];
            for (int j = 0; j < length; j++)
            {
                ushort cc = text.GetChar(j);
                bytes[2 * j + 1] = (byte)(cc >> 8);
                bytes[2 * j] = (byte)(cc & 0xFF);
            }

            this.WriteBytes(bytes);
            
        }

        private static bool IsPureAsciiString(LiteralString text)
        {
            int length = text.Length;

            for (int i = 0; i < length; i++)
            {
                char c = text.GetChar(i);
                if (c > 127 || c == 0 ||
                     (char.GetUnicodeCategory(c) == UnicodeCategory.Control &&
                       c != '\r' && c != '\n' && c != '\a' && c != '\t'
                                                                         ))
                {
                    return false;
                }
            }

            return true;
            
        }

        /// <summary>
        /// Writes a quoted string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="quote">The quoting character.</param>
        /// <param name="lineBreak">Whether the text should be broken in parts
        /// to fit within margins.</param>
        /// <param name="options">Options.</param>
        private void WriteQuotedString( LiteralString text, char quote, int lineBreak, WriteStringOptions options )
        {
            // Look if all characters are ASCII
            int length = text.Length;
            if ( ( options & WriteStringOptions.IgnoreByteArray ) == 0 &&
                !IsPureAsciiString(text))
            {
                this.WriteStringAsByteArray( text);
                return;
            }

            if ( text.Length < 53)
            {
                lineBreak = 0;
            }
#if NICE
            string oldAutoIndentString = this.currentAutoIndentString;
            this.MarkAutoIndentLocation();
#endif

            this.WriteRaw( quote );

            for ( int i = 0 ; i < length ; i++ )
            {
#if NICE
                if ( lineBreak > 0 )
                {
                    if ( i == 50 ||
                         ( i > 50 && ( i - 50 )%lineBreak == 0 ) )
                    {
                        this.WriteRaw( quote );
                        this.WriteLineBreak();
                        this.WriteRaw( "+" );
                        this.WriteRaw( quote );
                    }
                }
#endif

                char c = text.GetChar( i );
                switch ( c )
                {
                    case '\"':
                        this.WriteRaw( "\\\"" );
                        break;

                    case '\n':
                        this.WriteRaw( "\\n" );
                        break;

                    case '\t':
                        this.WriteRaw( "\\t" );
                        break;

                    case '\b':
                        this.WriteRaw( "\\b" );
                        break;

                    case '\f':
                        this.WriteRaw( "\\f" );
                        break;

                    case '\v':
                        this.WriteRaw( "\\v" );
                        break;

                    case '\r':
                        this.WriteRaw( "\\r" );
                        break;

                    case '\a':
                        this.WriteRaw( "\\a" );
                        break;

                    case '\\':
                        this.WriteRaw( "\\\\" );
                        break;
#if NICE
                    case '\0':
                        if ( this.options.Compatibility != ILWriterCompatibility.MsRoundtrip )
                        {
                            this.WriteRaw( "\\0" );
                        }
                        break;
#endif

                    case '?':
                        if ( ( options & WriteStringOptions.IgnoreEscapeQuestionMark ) == 0 &&
                            ( this.options.Compatibility & ILWriterCompatibility.IgnoreEscapeQuestionMark ) == 0)
                        {
                            this.WriteRaw( "\\?" );
                        }
                        else
                        {
                            this.WriteRaw( '?' );
                        }
                        break;


                    default:
                        if ( c == quote )
                        {
                            this.WriteRaw( '\\' );
                            this.WriteRaw( quote );
                        }
                        else
                        {
                            if ( char.GetUnicodeCategory( c ) == UnicodeCategory.Control  )
                            {
                                this.WriteRaw( '\\');
                                this.WriteOct(c, 3 );

                            }
                            else
                            {
                                this.WriteRaw( c );
                            }
                        }
                        break;
                }
            }

            this.WriteRaw( quote );
#if NICE
            this.currentAutoIndentString = oldAutoIndentString;
#endif
        }

        /// <overloads>Writes a symbol.</overloads>
        /// <summary>
        /// Writes a symbol given as a <see cref="string"/>.
        /// </summary>
        /// <param name="text">The symbol.</param>
        public void WriteSymbol( string text )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( text, "text" );

            #endregion

            char[] chars = text.ToCharArray();
#if NICE
            this.ManageSpace( SymbolNeedsSpaceBefore( chars[0] ), SymbolNeedsSpaceAfter( chars[chars.Length - 1] ) );
#else
            this.WriteRaw( ' ' );
#endif
      
            this.WriteRaw( chars );
        }

        /// <summary>
        /// Writes a symbol given as a <see cref="char"/> and determines automatically
        /// the kind of symbol required by the symbol.
        /// </summary>
        /// <param name="character">The symbol.</param>
        public void WriteSymbol( char character )
        {
#if NICE
            this.ManageSpace( SymbolNeedsSpaceBefore( character ), SymbolNeedsSpaceAfter( character ) );
#else
            this.WriteRaw( ' ' );
#endif
            
            this.WriteRaw( character );
        }

        /// <summary>
        /// Writes a symbol given as a <see cref="char"/> and specifies the kind
        /// of spacing required before and after the symbol.
        /// </summary>
        /// <param name="character">The symbol.</param>
        /// <param name="spacingKindBefore">Kind of spacing required before the symbol.
        /// </param>
        /// <param name="spacingKindAfter">Kind of spacing required after the symbol.
        /// </param>
        public void WriteSymbol( char character, SymbolSpacingKind spacingKindBefore,
                                 SymbolSpacingKind spacingKindAfter )
        {
#if NICE
            this.ManageSpace( spacingKindBefore, spacingKindAfter );
#else
            this.WriteRaw( ' ' );
#endif
            this.WriteRaw( character );
        }

        /// <summary>
        /// Writes a keyword.
        /// </summary>
        /// <param name="text">The keyword.</param>
        public void WriteKeyword( string text )
        {
#if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.IfWord );
#else
            this.WriteRaw( ' ' );
#endif

            this.WriteRaw( text );
        }

        /// <summary>
        /// Writes an instruction opcode.
        /// </summary>
        /// <param name="opCode">The opcode</param>
        public void WriteInstruction( OpCodeNumber opCode )
        {
            this.WriteKeyword( opCodeToString.GetValue( opCode ) );
        }


        /// <summary>
        /// Writes an unconditional line break.
        /// </summary>
        public void WriteLineBreak()
        {
            this.textWriter.WriteLine();
#if NICE
            this.lastSymbolKind = SymbolSpacingKind.None;
            this.currentHorizontalPosition = 0;
            this.isLineStart = true;
#endif 
#if TRACE
            if ( this.options.TraceEnabled && Trace.ILWriter.Enabled )
            {
                Trace.ILWriter.WriteLine( "" );
            }
#endif
        }

        /// <overloads>Writes a conditional line break.</overloads>
        /// <summary>
        /// Writes a conditional line break with the default margin.
        /// </summary>
        /// <returns><b>true</b> if a line break was issued, otherwise <b>false</b>.</returns>
        public bool WriteConditionalLineBreak()
        {
            return this.WriteConditionalLineBreak( BreakMargin );
        }

        /// <summary>
        /// Writes a conditional line break and specifies the margin.
        /// </summary>
        /// <param name="breakMargin">Maximal size of a line above which a line break
        /// will be issued.</param>
        /// <returns><b>true</b> if a line break was issued, otherwise <b>false</b>.</returns>
        public bool WriteConditionalLineBreak( int breakMargin )
        {
#if NICE
            if ( this.currentHorizontalPosition > breakMargin )
            {
                this.WriteLineBreak();
                return true;
            }
            else
            {
                return false;
            }
#else
            return false;
#endif
        }


        /// <summary>
        /// Begins a block.
        /// </summary>
        public void BeginBlock()
        {
            this.WriteSymbol( '{' );
            this.WriteLineBreak();
            this.Indent++;
        }

        /// <summary>
        /// Writes a comment line.
        /// </summary>
        /// <param name="comment">Comment text.</param>
        public void WriteCommentLine( string comment )
        {
#if NICE
            if ( !this.isLineStart )
            {
                this.WriteLineBreak();
            }
#else
            this.WriteLineBreak();
#endif
            this.WriteRaw( "// " );
            this.WriteRaw( comment );
            this.WriteLineBreak();
        }

        /// <summary>
        /// Write a comment.
        /// </summary>
        /// <param name="comment">A comment.</param>
        public void WriteComment( string comment )
        {
            this.WriteRaw( " /* " );
            this.WriteRaw( comment );
            this.WriteRaw( " */ " );
        }

        /// <summary>
        /// Ends a block and writes a line break after the closing bracket.
        /// </summary>
        public void EndBlock()
        {
            this.EndBlock( true );
        }

        /// <summary>
        /// Ends a block and specifies whether a line break should be issued
        /// after the closing bracket.
        /// </summary>
        /// <param name="writeLineBreak">Shether a line break should be issued
        /// after the closing bracket.</param>
        public void EndBlock( bool writeLineBreak )
        {
            this.Indent--;
            this.WriteSymbol( '}' );

            if ( writeLineBreak )
            {
                this.WriteLineBreak();
            }
        }


        /// <overloads>Writes a label name.</overloads>
        /// <summary>
        /// Writes a label name with the default prefix.
        /// </summary>
        /// <param name="token">The label token.</param>
        public void WriteLabelReference( int token )
        {
            this.WriteLabelReference( "IL", token );
        }

        /// <summary>
        /// Writes a label name and specifies the prefix.
        /// </summary>
        /// <param name="prefix">The label prefix.</param>
        /// <param name="token">The label token.</param>
        public void WriteLabelReference( string prefix, int token )
        {
#if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.IfWord );
#else
            this.WriteRaw( ' ' );
#endif

            this.WriteRaw(  GetLabel( prefix, token) );
        }

        internal static string GetLabel( string prefix, int token )
        {
            return string.Format( CultureInfo.InvariantCulture, "{0}_{1:X4}", prefix, token );
        }

        /// <summary>
        /// Writes a label definition.
        /// </summary>
        /// <param name="prefix">The label prefix.</param>
        /// <param name="token">The label token.</param>
        public void WriteLabelDefinition( string prefix, int token )
        {
            this.WriteLineBreak();
            this.Indent--;
            this.WriteLabelReference( prefix, token );
            this.WriteSymbol( ':' );
            this.WriteLineBreak();
            this.Indent++;
        }

        /// <summary>
        /// Gets or sets the identation level.
        /// </summary>
        public int Indent
        {
            get 
            { 
#if NICE
                return this.currentAutoIndentLevel; 
#else
                return 0;
#endif
            }
            set
            {
#if NICE
                if ( value < 0 )
                {
                    throw new ArgumentOutOfRangeException( "value" );
                }

                this.currentAutoIndentLevel = value;
                this.currentAutoIndentString = new string( ' ', this.currentAutoIndentLevel*indentSpaces );
#endif
            }
        }


        /// <overloads>Writes a raw string.</overloads>
        /// <summary>
        /// Writes a raw string represented as a <see cref="string"/>.
        /// </summary>
        /// <param name="text">The text.</param>
        public void WriteRaw( string text )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( text, "text" );

            #endregion

#if NICE
            this.ManageLineStart();
#endif
            this.textWriter.Write( text );
#if NICE
            this.currentHorizontalPosition += text.Length;
#endif

#if TRACE
            if ( this.options.TraceEnabled && Trace.ILWriter.Enabled )
            {
                Trace.ILWriter.Write( text );
            }
#endif
        }

        /// <summary>
        /// Writes a raw string represented as an array of characters.
        /// </summary>
        /// <param name="text">The text.</param>
        public void WriteRaw( char[] text )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( text, "text" );

            #endregion

#if NICE
            this.ManageLineStart();
#endif
            this.textWriter.Write( text );
#if NICE
            this.currentHorizontalPosition += text.Length;
#endif

#if TRACE
            if ( this.options.TraceEnabled && Trace.ILWriter.Enabled )
            {
                Trace.ILWriter.Write( text.ToString() );
            }
#endif
        }

        /// <summary>
        /// Writes a raw string represented as an array of characters.
        /// </summary>
        /// <param name="text">An array of characters.</param>
        /// <param name="index">Index of the first character to be written.</param>
        /// <param name="length">Number of characters to be written.</param>
        public void WriteRaw( char[] text, int index, int length )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( text, "text" );

            #endregion

#if NICE
            this.ManageLineStart();
#endif
            this.textWriter.Write( text, index, length );
#if NICE
            this.currentHorizontalPosition += length;
#endif

#if TRACE
            if ( this.options.TraceEnabled && Trace.ILWriter.Enabled )
            {
                Trace.ILWriter.Write( new string( text, index, length ) );
            }
#endif
        }


        /// <summary>
        /// Writes a single raw character.
        /// </summary>
        /// <param name="character">A character.</param>
        public void WriteRaw( char character )
        {
#if NICE
            this.ManageLineStart();
#endif

            this.textWriter.Write( character );
#if NICE
            this.currentHorizontalPosition += 1;
#endif

#if TRACE
            if ( this.options.TraceEnabled && Trace.ILWriter.Enabled )
            {
                Trace.ILWriter.Write( character.ToString() );
            }
#endif
        }

        /// <summary>
        /// Sets the value of the indent location of the the current horizontal.
        /// </summary>
        /// <returns>The current horizontal position.</returns>
        public int MarkAutoIndentLocation()
        {
#if NICE
            this.ManageSpace( SymbolSpacingKind.IfWord, SymbolSpacingKind.None );
            this.currentAutoIndentString = new string( ' ', this.currentHorizontalPosition );
            return this.currentHorizontalPosition;
#else
            return 0;
#endif
        }

        /// <summary>
        /// Sets the indent location to an explicit number of spaces.
        /// </summary>
        /// <param name="horizontalPosition">The horizontal position of indentation.</param>
        [SuppressMessage( "Microsoft.Design", "CA1024",
            Justification = "This is not a property because there is no value that can be read." )]
        public void SetIndentLocation( int horizontalPosition )
        {
#if NICE
            if ( horizontalPosition >= 0 )
            {
                this.currentAutoIndentString = new string( ' ', horizontalPosition );
            }
            else
            {
                this.ResetIndentLocation();
            }
#endif
        }

        /// <summary>
        /// Resets the indent location to the value computed from the current indent level.
        /// </summary>
        public void ResetIndentLocation()
        {
#if NICE
            this.currentAutoIndentString = new string( ' ', this.currentAutoIndentLevel*indentSpaces );
#endif
        }

        /// <summary>
        /// Gets the current horizontal position.
        /// </summary>
        public int CurrentHorizontalPosition
        {
            get
            {
#if NICE
                return this.currentHorizontalPosition;
#else
                return 0;
#endif
            }
        }
    }

    /// <summary>
    /// Determines how to format an integer.
    /// </summary>
    public enum IntegerFormat
    {
        /// <summary>
        /// Decimal.
        /// </summary>
        Decimal,

        /// <summary>
        /// Hexadecimal, upper case.
        /// </summary>
        HexUpper,

        /// <summary>
        /// Hexadecimal, lower case.
        /// </summary>
        HexLower
    }


    /// <summary>
    /// Enumerates the kind of spacing requirements of symbols.
    /// </summary>
    public enum SymbolSpacingKind
    {
        /// <summary>
        /// The symbol does not require any space.
        /// </summary>
        None = 0,

        /// <summary>
        /// The symbol requires a space only if the next or previous 
        /// symbol is a word.
        /// </summary>
        IfWord = 1,

        /// <summary>
        /// The symbol does not require any symbol.
        /// </summary>
        Required = 2
    }

    /// <summary>
    /// Influences the behavior of <see cref="ILWriter.WriteQuotedString(string,WriteStringOptions)"/>.
    /// </summary>
    [Flags]
    public enum WriteStringOptions
    {
        /// <summary>
        /// Default.
        /// </summary>
        None,

        /// <summary>
        /// Never render the string as a byte array.
        /// </summary>
        IgnoreByteArray = 1,

        /// <summary>
        /// Do not escape question marks.
        /// </summary>
        IgnoreEscapeQuestionMark = 2,

        /// <summary>
        /// Separate the string in many lines if it is too large for one line.
        /// </summary>
        LineBreak = 4,

        /// <summary>
        /// Use double quotes instead of simple quotes.
        /// </summary>
        DoubleQuoted = 8
    }
}
