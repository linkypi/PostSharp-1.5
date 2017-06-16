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

#endregion

using System;
using PostSharp.CodeModel;

namespace PostSharp.ModuleWriter
{
    /// <summary>
    /// Options influencing the process of writing the code model to an IL source file.
    /// </summary>
    /// <remarks>
    /// This class contains additionally some information about the current context
    /// of the IL source file.
    /// </remarks>
    public sealed class ILWriterOptions
    {
        private bool releaseBodyAfterWrite = true;


        /// <summary>
        /// Initializes a new <see cref="ILWriterOptions"/>.
        /// </summary>
        internal ILWriterOptions()
        {
        }


        /// <summary>
        /// Determines whether the current context is a method definition.
        /// </summary>
        internal bool InMethodDefinition { get; set; }

        /// <summary>
        /// Determines whether the current context is a method body.
        /// </summary>
        internal bool InMethodBody { get; set; }


        /// <summary>
        /// Gets or sets the compatibility of the output. Currently not used consistently.
        /// </summary>
        public ILWriterCompatibility Compatibility { get; set; }

        internal bool InForwardDeclaration { get; set; }

        /// <summary>
        /// Determines whether tracing of output is enabled.
        /// </summary>
        public bool TraceEnabled { get; set; }


        /// <summary>
        /// Determines whether custom attributes should inconditionally
        /// be rendered in the "verbose" form (i.e. in deserialized form).
        /// </summary>
        public bool VerboseCustomAttributes { get; set; }


        /// <summary>
        /// Determines whether the method body should be released (that is, deleted from memory)
        /// after having been written to MSIL. Default is <b>true</b>.
        /// </summary>
        public bool ReleaseBodyAfterWrite
        {
            get { return releaseBodyAfterWrite; }
            set { releaseBodyAfterWrite = value; }
        }
    }

    /// <summary>
    /// Compatibility options for the generation of the IL source file.
    /// </summary>
    [Flags]
    public enum ILWriterCompatibility
    {
        /// <summary>
        /// Default behavior.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Writes type packing options even if the type packing is default.
        /// </summary>
        ForceWriteTypePacking = 1,

        /// <summary>
        /// Mask of all compatibility flags that are actually bugs.
        /// </summary>
        Bugs = 8196, /* Should be non-zero. */

        /// <summary>
        /// Emit a forward declaration type list at the beginning of the MSIL file.
        /// </summary>
        EmitTypeList = 2,

        /// <summary>
        /// Emit method semantics (inside properties and events) by semantic order
        /// (see <see cref="MethodSemanticDeclaration.BySemanticComparer"/> instead
        /// of by original order.
        /// </summary>
        EmitMethodSemanticsBySemanticOrder = 4,

        /// <summary>
        /// Does not escape the question mark in quoted strings.
        /// </summary>
        IgnoreEscapeQuestionMark = 8,

        /// <summary>
        /// Write floating numbers with a lower case 'e' sign.
        /// </summary>
        LowerCaseFloat = 16,

        /// <summary>
        /// Write all digits in floating numbers.
        /// </summary>
        AllDigitsFloat = 32,

        /// <summary>
        /// Ignore the 'cil' qualifier in data sections (<see cref="DataSectionDeclaration"/>).
        /// </summary>
        IgnoreCilQualifierInDataSection = 64,

        /// <summary>
        /// Force line breaks before binary blocks.
        /// </summary>
        ForceBreakBeforeBlobs = 128,

        /// <summary>
        /// Emit a forward declaration list of all methods at the beginning of the MSIL file.
        /// </summary>
        EmitForwardDeclarations = 256,

        /// <summary>
        /// Ignore the <c>.mscorlib</c> header directive.
        /// </summary>
        IgnoreMscorlibHeader = 512,

        /// <summary>
        /// Do not write verbose custom attributes. Serialize them instead.
        /// </summary>
        ForbidVerboseCustomAttribute = 1024,

        /// <summary>
        /// All options necessary for compatibility with Microsoft ILASM.
        /// </summary>
        Ms = ForbidVerboseCustomAttribute, 
        /// <summary>
        /// All options necessary for a roundtrip test using Microsoft ILASM/ILDASM.
        /// </summary>
        MsRoundtrip = ForceWriteTypePacking | EmitTypeList | EmitForwardDeclarations,

        /// <summary>
        /// All options necessary for compatibility with Mono ILASM.
        /// </summary>
        Mono =
            IgnoreEscapeQuestionMark | LowerCaseFloat | AllDigitsFloat | IgnoreCilQualifierInDataSection |
            IgnoreMscorlibHeader | ForbidVerboseCustomAttribute,

        /// <summary>
        /// All options necessary for a roundtrip test using Mono ILASM/ILDASM.
        /// </summary>
        MonoRoundtrip = Mono | ForceBreakBeforeBlobs
    }
}