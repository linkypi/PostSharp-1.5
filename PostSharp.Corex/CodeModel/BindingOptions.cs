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

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Determines how the binding methods behave when the requested object
    /// is not found.
    /// </summary>
    [Flags]
    [SuppressMessage( "Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue" )]
    public enum BindingOptions
    {
        /// <summary>
        /// Default. The library shall create the object and assign it
        /// a <see cref="MetadataToken"/> if the requested object is not found.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Indicates that the methods shall return <b>null</b> if the requested
        /// object is not found.
        /// </summary>
        OnlyExisting = 1,

        /// <summary>
        /// Indicates that the methods shall create the object but shall not
        /// assign it a <see cref="MetadataToken"/>. This option is typically used
        /// when the object should not be linked statically using the module
        /// metadata, but the full name of the object is included (for instance
        /// with permission sets and custom attributes).
        /// </summary>
        WeakReference = 2,

        /// <summary>
        /// Masks <see cref="OnlyExisting"/> and <see cref="WeakReference"/>.
        /// </summary>
        ExistenceMask = 3,

        /// <summary>
        /// Indicates generic type or method <b>instances</b> are required (all generic parameters should be bound).
        /// </summary>
        RequireGenericInstance = 16,

        /// <summary>
        /// Indicates that generic or method <b>definitions</b> are required (generic parameters should not
        /// be bound).
        /// </summary>
        RequireGenericDefinition = 32,

        /// <summary>
        /// Mask isolating the way how generic types or methods should be bound.
        /// <see cref="RequireGenericInstance"/>, <see cref="RequireGenericDefinition"/> or
        /// <see cref="Default"/>.
        /// </summary>
        RequireGenericMask = RequireGenericDefinition | RequireGenericInstance,


        /// <summary>
        /// Indicates that normal types (<see cref="object"/>, <see cref="int"/>, ...)
        /// should be used instead of intrinsics (<c>object</c>, <c>int32</c>, ...).
        /// </summary>
        DisallowIntrinsicSubstitution = 8,

        /// <summary>
        /// Returns a <b>null</b> pointer instead of throwing an exception if
        /// the declaration is not found.
        /// </summary>
        DontThrowException = 64,

        /// <summary>
        /// Indicates that only definitions (not references or constructions) are allowed.
        /// </summary>
        OnlyDefinition = 128,

        /// <summary>
        /// Indicates that a only objects derived from <see cref="IType"/> should be returned;
        /// if the type appears to be a <see cref="TypeSignature"/>, it will be wrapped into a
        /// <see cref="TypeSpecDeclaration"/>.
        /// </summary>
        RequireIType = 256
    }
}