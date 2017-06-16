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

namespace PostSharp.CodeWeaver
{
    internal enum JoinPointKindIndex
    {
        BeforeMethodBody = 1,
        AfterMethodBodyAlways = 2,
        AfterMethodBodyException = 3,
        BeforeGetField = 4,
        AfterGetField = 5,
        InsteadOfGetField = 6,
        BeforeSetField = 7,
        AfterSetField = 8,
        InsteadOfSetField = 9,
        BeforeCall = 10,
        AfterCall = 11,
        InsteadOfCall = 12,
        BeforeGetArray = 13,
        AfterGetArray = 14,
        InsteadOfGetArray = 15,
        BeforeSetArray = 16,
        AfterSetArray = 17,
        InsteadOfSetArray = 18,
        BeforeThrow = 19,
        BeforeInstanceConstructor = 0,
        BeforeStaticConstructor = 20,
        AfterMethodBodySuccess = 21,
        AfterInstanceInitialization = 22,
        BeforeNewObject = 23,
        InsteadOfNewObject = 24,
        AfterNewObject = 25,
        BeforeGetFieldAddress = 26,
        AfterGetFieldAddress = 27,
        InsteadOfGetFieldAddress = 28,
        BeforeLoadArgument = 29,
        AfterLoadArgument = 30,
        InsteadOfLoadArgument = 31,
        BeforeStoreArgument = 32,
        AfterStoreArgument = 33,
        InsteadOfStoreArgument = 34,
        BeforeLoadArgumentAddress = 35,
        AfterLoadArgumentAddress = 36,
        InsteadOfLoadArgumentAddress = 37,
        _Count = 38,
        None = Int32.MaxValue
    }


    /// <summary>
    /// Kinds of join points.
    /// </summary>
    [Flags]
    public enum JoinPointKinds : long
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Before the method body. This is a <b>method-level</b> advice.
        /// </summary>
        /// <remarks>
        /// If the method is an instance constructor, this join point is
        /// located just <i>after</i> the <b>this</b> pointer has been initialized.
        /// Use the <see cref="BeforeInstanceConstructor"/> kind to have
        /// a join point located at the real beginning of the constructor.
        /// </remarks>
        BeforeMethodBody = 1 << JoinPointKindIndex.BeforeMethodBody,

        /// <summary>
        /// After the method body, in every case (exception or normal termination).
        /// This is a <b>method-level</b> advice.
        /// </summary>
        /// <remarks>
        /// The advice should not emit the <b>endfinally</b> instruction, since
        /// it is already emitted by the weaver.
        /// </remarks>
        AfterMethodBodyAlways = 1 << JoinPointKindIndex.AfterMethodBodyAlways,

        /// <summary>
        /// After the method body, in case of success.
        /// This is a <b>method-level</b> advice.
        /// </summary>
        AfterMethodBodySuccess = 1 << JoinPointKindIndex.AfterMethodBodySuccess,

        /// <summary>
        /// After the method body, only in case of exception.
        /// This is a <b>method-level</b> advice.
        /// </summary>
        /// <remarks>
        /// <para>The exception object is available on the top of the stack.</para>
        /// <para>The advice should not emit the <b>endfinally</b> instruction, since
        /// it is already emitted by the weaver.</para>
        /// </remarks>
        AfterMethodBodyException = 1 << JoinPointKindIndex.AfterMethodBodyException,

        /// <summary>
        /// Before getting a field.
        ///  This is a <b>method-level</b> advice.
        /// </summary>
        BeforeGetField = 1 << JoinPointKindIndex.BeforeGetField,

        /// <summary>
        /// After getting a field.
        ///  This is a <b>method-level</b> advice.
        /// </summary>
        AfterGetField = 1 << JoinPointKindIndex.AfterGetField,

        /// <summary>
        /// Instead of getting a field.
        /// This is both a <b>field-level</b> and <b>method-level</b> advice.
        /// </summary>
        InsteadOfGetField = 1 << JoinPointKindIndex.InsteadOfGetField,

        /// <summary>
        /// Before setting a field.
        ///  This is a <b>method-level</b> advice.
        /// </summary>
        BeforeSetField = 1 << JoinPointKindIndex.BeforeSetField,

        /// <summary>
        /// After setting a field.
        ///  This is a <b>method-level</b> advice.
        /// </summary>
        AfterSetField = 1 << JoinPointKindIndex.AfterSetField,

        /// <summary>
        /// Instead of setting a field.
        ///  This is a <b>method-level</b> advice.
        /// </summary>
        InsteadOfSetField = 1 << JoinPointKindIndex.InsteadOfSetField,

        /// <summary>
        /// Before calling a method.
        /// This is a <b>method-level</b> advice.
        /// </summary>
        BeforeCall = 1 << JoinPointKindIndex.BeforeCall,

        /// <summary>
        /// After calling a method.
        /// This is a <b>method-level</b> advice.
        /// </summary>
        AfterCall = 1 << JoinPointKindIndex.AfterCall,

        /// <summary>
        /// Instead of calling a method.
        /// This is a <b>method-level</b> advice.
        /// </summary>
        InsteadOfCall = 1 << JoinPointKindIndex.InsteadOfCall,

        /// <summary>
        /// Before getting an array element.
        /// This is a <b>method-level</b> advice.
        /// </summary>
        BeforeGetArray = 1 << JoinPointKindIndex.BeforeGetArray,

        /// <summary>
        /// After getting an array element.
        /// This is a <b>method-level</b> advice.
        /// </summary>
        AfterGetArray = 1 << JoinPointKindIndex.AfterGetArray,

        /// <summary>
        /// Instead of getting an array element.
        /// This is a <b>method-level</b> advice.
        /// </summary>
        InsteadOfGetArray = 1 << JoinPointKindIndex.InsteadOfGetArray,

        /// <summary>
        /// Before setting an array element.
        /// This is a <b>method-level</b> advice.
        /// </summary>
        BeforeSetArray = 1 << JoinPointKindIndex.BeforeSetArray,

        /// <summary>
        /// After setting an array element.
        /// This is a <b>method-level</b> advice.
        /// </summary>
        AfterSetArray = 1 << JoinPointKindIndex.AfterSetArray,

        /// <summary>
        /// Instead of setting an array element.
        /// This is a <b>method-level</b> advice.
        /// </summary>
        InsteadOfSetArray = 1 << JoinPointKindIndex.InsteadOfSetArray,

        /// <summary>
        /// Before the <b>throw</b> or <b>rethrow</b> instruction.
        /// This is a <b>method-level</b> advice.
        /// </summary>
        BeforeThrow = 1 << JoinPointKindIndex.BeforeThrow,

        /// <summary>
        /// In the real beginning of instance constructors, even before the <b>this</b> pointer
        /// is initialized.
        /// This is a <b>method-level</b> advice.
        /// </summary>
        BeforeInstanceConstructor = 1 << JoinPointKindIndex.BeforeInstanceConstructor,

        /// <summary>
        /// Before the class/static constructors (creates the class constructor if not yet present).
        /// This is a <b>type-level</b> advice.
        /// </summary>
        BeforeStaticConstructor = 1 << JoinPointKindIndex.BeforeStaticConstructor,


        /// <summary>
        /// Just after the <b>this</b> pointer has been initialized (in each constructor that calls
        /// the constructor of the base type).
        /// This is a <b>type-level</b> advice.
        /// </summary>
        AfterInstanceInitialization = 1 << JoinPointKindIndex.AfterInstanceInitialization,

        /// <summary>
        /// Just before the <b>newobj</b> instruction.
        /// </summary>
        BeforeNewObject = 1 << JoinPointKindIndex.BeforeNewObject,

        /// <summary>
        /// Instead of the <b>newobj</b> instruction.
        /// </summary>
        InsteadOfNewObject = 1 << JoinPointKindIndex.InsteadOfNewObject,

        /// <summary>
        /// Just before the <b>newobj</b> instruction.
        /// </summary>
        AfterNewObject = 1 << JoinPointKindIndex.AfterNewObject,

        /// <summary>
        /// Just before the <b>ldflda</b> or <b>ldsflda</b> instruction.
        /// </summary>
        BeforeGetFieldAddress = 1 << JoinPointKindIndex.BeforeGetFieldAddress,

        /// <summary>
        /// Just after the <b>ldflda</b> or <b>ldsflda</b> instruction.
        /// </summary>
        AfterGetFieldAddress = 1 << JoinPointKindIndex.AfterGetFieldAddress,

        /// <summary>
        /// Instead of the <b>ldflda</b> or <b>ldsflda</b> instruction.
        /// </summary>
        InsteadOfGetFieldAddress = 1 << JoinPointKindIndex.InsteadOfGetFieldAddress,

        /// <summary>
        /// Just before the <b>ldarg*</b> instructions.
        /// </summary>
        BeforeLoadArgument = 1 << JoinPointKindIndex.BeforeLoadArgument,

        /// <summary>
        /// Just after the <b>ldarg*</b> instructions.
        /// </summary>
        AfterLoadArgument = 1 << JoinPointKindIndex.AfterLoadArgument,

        /// <summary>
        /// Instead of the <b>ldarg*</b> instructions.
        /// </summary>
        InsteadOfLoadArgument = 1 << JoinPointKindIndex.InsteadOfLoadArgument,

        /// <summary>
        /// Just before the <b>starg*</b> instructions.
        /// </summary>
        BeforeStoreArgument = 1 << JoinPointKindIndex.BeforeStoreArgument,

        /// <summary>
        /// Just after the <b>starg*</b> instructions.
        /// </summary>
        AfterStoreArgument = 1 << JoinPointKindIndex.AfterStoreArgument,

        /// <summary>
        /// Instead of the <b>starg*</b> instructions.
        /// </summary>
        InsteadOfStoreArgument = 1 << JoinPointKindIndex.InsteadOfStoreArgument,

        /// <summary>
        /// Just before the <b>ldarga*</b> instructions.
        /// </summary>
        BeforeLoadArgumentAddress = 1 << JoinPointKindIndex.BeforeLoadArgumentAddress,

        /// <summary>
        /// Just after the <b>ldarga*</b> instructions.
        /// </summary>
        AfterLoadArgumentAddress = 1 << JoinPointKindIndex.AfterLoadArgumentAddress,

        /// <summary>
        /// Instead of the <b>ldarga*</b> instructions.
        /// </summary>
        InsteadOfLoadArgumentAddress = 1 << JoinPointKindIndex.InsteadOfLoadArgumentAddress
    }
}