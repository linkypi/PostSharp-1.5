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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Maps a method declaration to an external unmanaged method.
    /// </summary>
    public sealed class PInvokeMap
    {
        #region Fields

        /// <summary>
        /// Target method name.
        /// </summary>
        private string methodName;

        /// <summary>
        /// Attributes.
        /// </summary>
        private PInvokeAttributes attributes;

        /// <summary>
        /// Module defining the target method.
        /// </summary>
        private ModuleRefDeclaration module;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="PInvokeMap"/>.
        /// </summary>
        public PInvokeMap()
        {
        }

        #region Properties

        /// <summary>
        /// Gets or sets the module reference containing the unmanaged method.
        /// </summary>
        [ReadOnly( true )]
        public ModuleRefDeclaration Module
        {
            get { return module; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                module = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the method.
        /// </summary>
        [ReadOnly( true )]
        public string MethodName
        {
            get { return methodName; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                methodName = value;
            }
        }

        /// <summary>
        /// Gets or sets the PInvoke attributes.
        /// </summary>
        [ReadOnly( true )]
        public PInvokeAttributes Attributes { get { return attributes; } set { attributes = value; } }

        #endregion

        #region writer IL

        /// <summary>
        /// Writes the IL definition of the current P-Invoke map.
        /// </summary>
        /// <param name="parent">The declaring method.</param>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        internal void WriteILDefinition( MethodDefDeclaration parent, ILWriter writer )
        {
            writer.WriteKeyword( "pinvokeimpl" );
            writer.WriteSymbol( '(' );
            if ( !this.module.Name.StartsWith( "$MOR$" ) )
            {
                writer.WriteQuotedString( this.module.Name, WriteStringOptions.DoubleQuoted );
            }

            if ( !string.IsNullOrEmpty( this.methodName ) && this.methodName != parent.Name )
            {
                writer.WriteKeyword( "as" );
                writer.WriteQuotedString( this.methodName, WriteStringOptions.DoubleQuoted );
            }

            if ( ( this.attributes & PInvokeAttributes.NoMangle ) != 0 )
            {
                writer.WriteKeyword( "nomangle" );
            }


            switch ( this.attributes & PInvokeAttributes.CharSetMask )
            {
                case PInvokeAttributes.CharSetAuto:
                    writer.WriteKeyword( "autochar" );
                    break;

                case PInvokeAttributes.CharSetAnsi:
                    writer.WriteKeyword( "ansi" );
                    break;

                case PInvokeAttributes.CharSetUnicode:
                    writer.WriteKeyword( "unicode" );
                    break;

                case PInvokeAttributes.CharSetNotSpec:
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( this.attributes,
                                                                                  "this.Attributes" );
            }

            if ( ( this.attributes & PInvokeAttributes.SupportsLastError ) != 0 )
            {
                writer.WriteKeyword( "lasterr" );
            }

            switch ( this.attributes & PInvokeAttributes.CallConventionMask )
            {
                case PInvokeAttributes.CallConventionCdecl:
                    writer.WriteKeyword( "cdecl" );
                    break;

                case PInvokeAttributes.CallConventionFastCall:
                    writer.WriteKeyword( "fastcall" );
                    break;

                case PInvokeAttributes.CallConventionStdCall:
                    writer.WriteKeyword( "stdcall" );
                    break;

                case PInvokeAttributes.CallConventionThisCall:
                    writer.WriteKeyword( "thiscall" );
                    break;

                case PInvokeAttributes.CallConventionWinApi:
                    writer.WriteKeyword( "winapi" );
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( this.attributes, "this.Attributes" );
            }


            if ( ( this.attributes & PInvokeAttributes.BestFitDisabled ) != 0 )
            {
                writer.WriteKeyword( "bestfit:off" );
            }

            if ( ( this.attributes & PInvokeAttributes.BestFitEnabled ) != 0 )
            {
                writer.WriteKeyword( "bestfit:on" );
            }

            if ( ( this.attributes & PInvokeAttributes.ThrowOnUnmappableCharEnabled ) != 0 )
            {
                writer.WriteKeyword( "charmaperror:on" );
            }


            writer.WriteSymbol( ')' );
        }

        #endregion
    }

    /// <summary>
    /// P-Invoke attributes.
    /// </summary>
    [Flags]
    [SuppressMessage( "Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue" )]
    public enum PInvokeAttributes
    {
        /// <summary>
        /// Default.
        /// </summary>
        None = 0,

        /// <summary>
        /// Parameter types will not be mangled into the method name.
        /// </summary>
        NoMangle = 0x0001, //PInvoke is to use the member name as specified

        //Character set

        /// <summary>
        /// Character set mask.
        /// </summary>
        CharSetMask = 0x0006,

        /// <summary>
        /// Character set not specified.
        /// </summary>
        CharSetNotSpec = 0x0000,

        /// <summary>
        /// Character set is ANSI.
        /// </summary>
        CharSetAnsi = 0x0002,

        /// <summary>
        /// Character set is Unicode.
        /// </summary>
        CharSetUnicode = 0x0004,

        /// <summary>
        /// Character set choice is automatic.
        /// </summary>
        CharSetAuto = 0x0006,

        /// <summary>
        /// Supports Windows GetLastError().
        /// </summary>
        SupportsLastError = 0x0040,

        /// <summary>
        /// Best fit disabled.
        /// </summary>
        BestFitDisabled = 0x20,

        /// <summary>
        /// Best fit enabled.
        /// </summary>
        BestFitEnabled = 0x10,

        /// <summary>
        /// Throws an exception when characters cannot be mapped to the
        /// target character set.
        /// </summary>
        [SuppressMessage( "Microsoft.Naming", "CA1704", Justification = "Spelling is correct." )] ThrowOnUnmappableCharEnabled = 0x1000,

        //Calling convention

        /// <summary>
        /// Calling convention mask.
        /// </summary>
        CallConventionMask = 0x0700,

        /// <summary>
        /// Calling convention is <b>WINAPI</b>.
        /// </summary>
        CallConventionWinApi = 0x0100,

        /// <summary>
        /// Calling convention is <b>cdecl</b>.
        /// </summary>
        CallConventionCdecl = 0x0200,

        /// <summary>
        /// Calling convention is <b>stdcall</b>.
        /// </summary>
        CallConventionStdCall = 0x0300,

        /// <summary>
        /// Calling convention is <b>thiscall</b>.
        /// </summary>
        CallConventionThisCall = 0x0400,

        /// <summary>
        /// Calling convention is <b>fastcall</b>.
        /// </summary>
        CallConventionFastCall = 0x0500
    }
}