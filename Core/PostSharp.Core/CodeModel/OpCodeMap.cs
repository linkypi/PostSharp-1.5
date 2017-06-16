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
using System.Reflection.Emit;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Maps <see cref="OpCodeNumber"/> to <see cref="OpCode"/> and provides
    /// the various properties of opcodes.
    /// </summary>
    [SuppressMessage( "Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase" )]
    public static class OpCodeMap
    {
        /// <summary>
        /// Maps an <see cref="OpCodeNumber"/> to an <see cref="OpCode"/>.
        /// </summary>
        private static readonly OpCodeDictionary<OpCode> mapOpCodeNumberToOpCode = new OpCodeDictionary<OpCode>();

        /// <summary>
        /// Maps a compressed <see cref="OpCodeNumber"/> to an <see cref="UncompressedOpCode"/>.
        /// </summary>
        private static readonly OpCodeDictionary<UncompressedOpCode> mapOpCodeNumberToCompressedOperand =
            new OpCodeDictionary<UncompressedOpCode>();

        #region Type initialization

        /// <summary>
        /// Initializes the <see cref="OpCodeMap"/> type.
        /// </summary>
        [SuppressMessage( "Microsoft.Performance", "CA1810",
            Justification="All initialization cannot be done at type declaration." )]
        static OpCodeMap()
        {
            //0x00	nop
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Nop, OpCodes.Nop );
            //0x01	break
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Break, OpCodes.Break );
            //0x02	ldarg.0
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldarg_0, OpCodes.Ldarg_0 );
            AddCompressedOperandMapping( OpCodeNumber.Ldarg_0, OpCodeNumber.Ldarg, OperandType.InlineVar, 0 );
            //0x03	ldarg.1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldarg_1, OpCodes.Ldarg_1 );
            AddCompressedOperandMapping( OpCodeNumber.Ldarg_1, OpCodeNumber.Ldarg, OperandType.InlineVar, 1 );
            //0x04	ldarg.2
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldarg_2, OpCodes.Ldarg_2 );
            AddCompressedOperandMapping( OpCodeNumber.Ldarg_2, OpCodeNumber.Ldarg, OperandType.InlineVar, 2 );
            //0x05	ldarg.3
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldarg_3, OpCodes.Ldarg_3 );
            AddCompressedOperandMapping( OpCodeNumber.Ldarg_3, OpCodeNumber.Ldarg, OperandType.InlineVar, 3 );
            //0x06	ldloc.0
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldloc_0, OpCodes.Ldloc_0 );
            AddCompressedOperandMapping( OpCodeNumber.Ldloc_0, OpCodeNumber.Ldloc, OperandType.InlineVar, 0 );
            //0x07	ldloc.1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldloc_1, OpCodes.Ldloc_1 );
            AddCompressedOperandMapping( OpCodeNumber.Ldloc_1, OpCodeNumber.Ldloc, OperandType.InlineVar, 1 );
            //0x08	ldloc.2
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldloc_2, OpCodes.Ldloc_2 );
            AddCompressedOperandMapping( OpCodeNumber.Ldloc_2, OpCodeNumber.Ldloc, OperandType.InlineVar, 2 );
            //0x09	ldloc.3
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldloc_3, OpCodes.Ldloc_3 );
            AddCompressedOperandMapping( OpCodeNumber.Ldloc_3, OpCodeNumber.Ldloc, OperandType.InlineVar, 3 );
            //0x0A	stloc.0
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stloc_0, OpCodes.Stloc_0 );
            AddCompressedOperandMapping( OpCodeNumber.Stloc_0, OpCodeNumber.Stloc, OperandType.InlineVar, 0 );
            //0x0B	stloc.1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stloc_1, OpCodes.Stloc_1 );
            AddCompressedOperandMapping( OpCodeNumber.Stloc_1, OpCodeNumber.Stloc, OperandType.InlineVar, 1 );
            //0x0C	stloc.2
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stloc_2, OpCodes.Stloc_2 );
            AddCompressedOperandMapping( OpCodeNumber.Stloc_2, OpCodeNumber.Stloc, OperandType.InlineVar, 2 );
            //0x0D	stloc.3
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stloc_3, OpCodes.Stloc_3 );
            AddCompressedOperandMapping( OpCodeNumber.Stloc_3, OpCodeNumber.Stloc, OperandType.InlineVar, 3 );
            //0x0E	ldarg.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldarg_S, OpCodes.Ldarg_S );
            //0x0F	ldarga.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldarga_S, OpCodes.Ldarga_S );
            //0x10	starg.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Starg_S, OpCodes.Starg_S );
            //0x11	ldloc.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldloc_S, OpCodes.Ldloc_S );
            //0x12	ldloca.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldloca_S, OpCodes.Ldloca_S );
            //0x13	stloc.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stloc_S, OpCodes.Stloc_S );
            //0x14	ldnull
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldnull, OpCodes.Ldnull );
            //0x15	ldc.i4.m1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_I4_M1, OpCodes.Ldc_I4_M1 );
            AddCompressedOperandMapping( OpCodeNumber.Ldc_I4_M1, OpCodeNumber.Ldc_I4, OperandType.InlineI, -1 );
            //0x16	ldc.i4.0
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_I4_0, OpCodes.Ldc_I4_0 );
            AddCompressedOperandMapping( OpCodeNumber.Ldc_I4_0, OpCodeNumber.Ldc_I4, OperandType.InlineI, 0 );
            //0x17	ldc.i4.1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_I4_1, OpCodes.Ldc_I4_1 );
            AddCompressedOperandMapping( OpCodeNumber.Ldc_I4_1, OpCodeNumber.Ldc_I4, OperandType.InlineI, 1 );
            //0x18	ldc.i4.2
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_I4_2, OpCodes.Ldc_I4_2 );
            AddCompressedOperandMapping( OpCodeNumber.Ldc_I4_2, OpCodeNumber.Ldc_I4, OperandType.InlineI, 2 );
            //0x19	ldc.i4.3
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_I4_3, OpCodes.Ldc_I4_3 );
            AddCompressedOperandMapping( OpCodeNumber.Ldc_I4_3, OpCodeNumber.Ldc_I4, OperandType.InlineI, 3 );
            //0x1A	ldc.i4.4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_I4_4, OpCodes.Ldc_I4_4 );
            AddCompressedOperandMapping( OpCodeNumber.Ldc_I4_4, OpCodeNumber.Ldc_I4, OperandType.InlineI, 4 );
            //0x1B	ldc.i4.5
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_I4_5, OpCodes.Ldc_I4_5 );
            AddCompressedOperandMapping( OpCodeNumber.Ldc_I4_5, OpCodeNumber.Ldc_I4, OperandType.InlineI, 5 );
            //0x1C	ldc.i4.6
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_I4_6, OpCodes.Ldc_I4_6 );
            AddCompressedOperandMapping( OpCodeNumber.Ldc_I4_6, OpCodeNumber.Ldc_I4, OperandType.InlineI, 6 );
            //0x1D	ldc.i4.7
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_I4_7, OpCodes.Ldc_I4_7 );
            AddCompressedOperandMapping( OpCodeNumber.Ldc_I4_7, OpCodeNumber.Ldc_I4, OperandType.InlineI, 7 );
            //0x1E	ldc.i4.8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_I4_8, OpCodes.Ldc_I4_8 );
            AddCompressedOperandMapping( OpCodeNumber.Ldc_I4_8, OpCodeNumber.Ldc_I4, OperandType.InlineI, 8 );
            //0x1F	ldc.i4.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_I4_S, OpCodes.Ldc_I4_S );
            //0x20	ldc.i4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_I4, OpCodes.Ldc_I4 );
            //0x21	ldc.i8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_I8, OpCodes.Ldc_I8 );
            //0x22	ldc.r4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_R4, OpCodes.Ldc_R4 );
            //0x23	ldc.r8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldc_R8, OpCodes.Ldc_R8 );
            //0x25	dup
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Dup, OpCodes.Dup );
            //0x26	pop
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Pop, OpCodes.Pop );
            //0x27	jmp
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Jmp, OpCodes.Jmp );
            //0x28	call
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Call, OpCodes.Call );
            //0x29	calli
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Calli, OpCodes.Calli );
            //0x2A	ret
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ret, OpCodes.Ret );
            //0x2B	br.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Br_S, OpCodes.Br_S );
            //0x2C	brfalse.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Brfalse_S, OpCodes.Brfalse_S );
            //0x2D	brtrue.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Brtrue_S, OpCodes.Brtrue_S );
            //0x2E	beq.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Beq_S, OpCodes.Beq_S );
            //0x2F	bge.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Bge_S, OpCodes.Bge_S );
            //0x30	bgt.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Bgt_S, OpCodes.Bgt_S );
            //0x31	ble.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ble_S, OpCodes.Ble_S );
            //0x32	blt.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Blt_S, OpCodes.Blt_S );
            //0x33	bne.un.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Bne_Un_S, OpCodes.Bne_Un_S );
            //0x34	bge.un.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Bge_Un_S, OpCodes.Bge_Un_S );
            //0x35	bgt.un.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Bgt_Un_S, OpCodes.Bgt_Un_S );
            //0x36	ble.un.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ble_Un_S, OpCodes.Ble_Un_S );
            //0x37	blt.un.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Blt_Un_S, OpCodes.Blt_Un_S );
            //0x38	br
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Br, OpCodes.Br );
            //0x39	brfalse
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Brfalse, OpCodes.Brfalse );
            //0x3A	brtrue
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Brtrue, OpCodes.Brtrue );
            //0x3B	beq
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Beq, OpCodes.Beq );
            //0x3C	bge
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Bge, OpCodes.Bge );
            //0x3D	bgt
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Bgt, OpCodes.Bgt );
            //0x3E	ble
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ble, OpCodes.Ble );
            //0x3F	blt
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Blt, OpCodes.Blt );
            //0x40	bne.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Bne_Un, OpCodes.Bne_Un );
            //0x41	bge.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Bge_Un, OpCodes.Bge_Un );
            //0x42	bgt.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Bgt_Un, OpCodes.Bgt_Un );
            //0x43	ble.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ble_Un, OpCodes.Ble_Un );
            //0x44	blt.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Blt_Un, OpCodes.Blt_Un );
            //0x45	switch
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Switch, OpCodes.Switch );
            //0x46	ldind.i1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldind_I1, OpCodes.Ldind_I1 );
            //0x47	ldind.u1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldind_U1, OpCodes.Ldind_U1 );
            //0x48	ldind.i2
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldind_I2, OpCodes.Ldind_I2 );
            //0x49	ldind.u2
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldind_U2, OpCodes.Ldind_U2 );
            //0x4A	ldind.i4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldind_I4, OpCodes.Ldind_I4 );
            //0x4B	ldind.u4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldind_U4, OpCodes.Ldind_U4 );
            //0x4C	ldind.i8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldind_I8, OpCodes.Ldind_I8 );
            //0x4D	ldind.i
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldind_I, OpCodes.Ldind_I );
            //0x4E	ldind.r4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldind_R4, OpCodes.Ldind_R4 );
            //0x4F	ldind.r8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldind_R8, OpCodes.Ldind_R8 );
            //0x50	ldind.ref
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldind_Ref, OpCodes.Ldind_Ref );
            //0x51	stind.ref
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stind_Ref, OpCodes.Stind_Ref );
            //0x52	stind.i1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stind_I1, OpCodes.Stind_I1 );
            //0x53	stind.i2
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stind_I2, OpCodes.Stind_I2 );
            //0x54	stind.i4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stind_I4, OpCodes.Stind_I4 );
            //0x55	stind.i8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stind_I8, OpCodes.Stind_I8 );
            //0x56	stind.r4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stind_R4, OpCodes.Stind_R4 );
            //0x57	stind.r8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stind_R8, OpCodes.Stind_R8 );
            //0x58	add
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Add, OpCodes.Add );
            //0x59	sub
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Sub, OpCodes.Sub );
            //0x5A	mul
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Mul, OpCodes.Mul );
            //0x5B	div
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Div, OpCodes.Div );
            //0x5C	div.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Div_Un, OpCodes.Div_Un );
            //0x5D	rem
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Rem, OpCodes.Rem );
            //0x5E	rem.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Rem_Un, OpCodes.Rem_Un );
            //0x5F	and
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.And, OpCodes.And );
            //0x60	or
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Or, OpCodes.Or );
            //0x61	xor
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Xor, OpCodes.Xor );
            //0x62	shl
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Shl, OpCodes.Shl );
            //0x63	shr
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Shr, OpCodes.Shr );
            //0x64	shr.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Shr_Un, OpCodes.Shr_Un );
            //0x65	neg
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Neg, OpCodes.Neg );
            //0x66	not
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Not, OpCodes.Not );
            //0x67	conv.i1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_I1, OpCodes.Conv_I1 );
            //0x68	conv.i2
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_I2, OpCodes.Conv_I2 );
            //0x69	conv.i4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_I4, OpCodes.Conv_I4 );
            //0x6A	conv.i8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_I8, OpCodes.Conv_I8 );
            //0x6B	conv.r4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_R4, OpCodes.Conv_R4 );
            //0x6C	conv.r8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_R8, OpCodes.Conv_R8 );
            //0x6D	conv.u4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_U4, OpCodes.Conv_U4 );
            //0x6E	conv.u8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_U8, OpCodes.Conv_U8 );
            //0x6F	callvirt
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Callvirt, OpCodes.Callvirt );
            //0x70	cpobj
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Cpobj, OpCodes.Cpobj );
            //0x71	ldobj
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldobj, OpCodes.Ldobj );
            //0x72	ldstr
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldstr, OpCodes.Ldstr );
            //0x73	newobj
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Newobj, OpCodes.Newobj );
            //0x74	castclass
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Castclass, OpCodes.Castclass );
            //0x75	isinst
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Isinst, OpCodes.Isinst );
            //0x76	conv.r.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_R_Un, OpCodes.Conv_R_Un );
            //0x79	unbox
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Unbox, OpCodes.Unbox );
            //0x7A	throw
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Throw, OpCodes.Throw );
            //0x7B	ldfld
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldfld, OpCodes.Ldfld );
            //0x7C	ldflda
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldflda, OpCodes.Ldflda );
            //0x7D	stfld
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stfld, OpCodes.Stfld );
            //0x7E	ldsfld
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldsfld, OpCodes.Ldsfld );
            //0x7F	ldsflda
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldsflda, OpCodes.Ldsflda );
            //0x80	stsfld
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stsfld, OpCodes.Stsfld );
            //0x81	stobj
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stobj, OpCodes.Stobj );
            //0x82	conv.ovf.i1.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_I1_Un, OpCodes.Conv_Ovf_I1_Un );
            //0x83	conv.ovf.i2.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_I2_Un, OpCodes.Conv_Ovf_I2_Un );
            //0x84	conv.ovf.i4.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_I4_Un, OpCodes.Conv_Ovf_I4_Un );
            //0x85	conv.ovf.i8.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_I8_Un, OpCodes.Conv_Ovf_I8_Un );
            //0x86	conv.ovf.u1.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_U1_Un, OpCodes.Conv_Ovf_U1_Un );
            //0x87	conv.ovf.u2.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_U2_Un, OpCodes.Conv_Ovf_U2_Un );
            //0x88	conv.ovf.u4.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_U4_Un, OpCodes.Conv_Ovf_U4_Un );
            //0x89	conv.ovf.u8.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_U8_Un, OpCodes.Conv_Ovf_U8_Un );
            //0x8A	conv.ovf.i.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_I_Un, OpCodes.Conv_Ovf_I_Un );
            //0x8B	conv.ovf.u.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_U_Un, OpCodes.Conv_Ovf_U_Un );
            //0x8C	box
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Box, OpCodes.Box );
            //0x8D	newarr
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Newarr, OpCodes.Newarr );
            //0x8E	ldlen
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldlen, OpCodes.Ldlen );
            //0x8F	ldelema
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldelema, OpCodes.Ldelema );
            //0x90	ldelem.i1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldelem_I1, OpCodes.Ldelem_I1 );
            //0x91	ldelem.u1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldelem_U1, OpCodes.Ldelem_U1 );
            //0x92	ldelem.i2
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldelem_I2, OpCodes.Ldelem_I2 );
            //0x93	ldelem.u2
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldelem_U2, OpCodes.Ldelem_U2 );
            //0x94	ldelem.i4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldelem_I4, OpCodes.Ldelem_I4 );
            //0x95	ldelem.u4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldelem_U4, OpCodes.Ldelem_U4 );
            //0x96	ldelem.i8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldelem_I8, OpCodes.Ldelem_I8 );
            //0x97	ldelem.i
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldelem_I, OpCodes.Ldelem_I );
            //0x98	ldelem.r4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldelem_R4, OpCodes.Ldelem_R4 );
            //0x99	ldelem.r8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldelem_R8, OpCodes.Ldelem_R8 );
            //0x9A	ldelem.ref
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldelem_Ref, OpCodes.Ldelem_Ref );
            //0x9B	stelem.i
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stelem_I, OpCodes.Stelem_I );
            //0x9C	stelem.i1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stelem_I1, OpCodes.Stelem_I1 );
            //0x9D	stelem.i2
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stelem_I2, OpCodes.Stelem_I2 );
            //0x9E	stelem.i4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stelem_I4, OpCodes.Stelem_I4 );
            //0x9F	stelem.i8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stelem_I8, OpCodes.Stelem_I8 );
            //0xA0	stelem.r4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stelem_R4, OpCodes.Stelem_R4 );
            //0xA1	stelem.r8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stelem_R8, OpCodes.Stelem_R8 );
            //0xA2	stelem.ref
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stelem_Ref, OpCodes.Stelem_Ref );
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldelem, OpCodes.Ldelem );
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stelem, OpCodes.Stelem );
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Unbox_Any, OpCodes.Unbox_Any );
            //0xB3	conv.ovf.i1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_I1, OpCodes.Conv_Ovf_I1 );
            //0xB4	conv.ovf.u1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_U1, OpCodes.Conv_Ovf_U1 );
            //0xB5	conv.ovf.i2
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_I2, OpCodes.Conv_Ovf_I2 );
            //0xB6	conv.ovf.u2
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_U2, OpCodes.Conv_Ovf_U2 );
            //0xB7	conv.ovf.i4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_I4, OpCodes.Conv_Ovf_I4 );
            //0xB8	conv.ovf.u4
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_U4, OpCodes.Conv_Ovf_U4 );
            //0xB9	conv.ovf.i8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_I8, OpCodes.Conv_Ovf_I8 );
            //0xBA	conv.ovf.u8
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_U8, OpCodes.Conv_Ovf_U8 );
            //0xC2	refanyval
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Refanyval, OpCodes.Refanyval );
            //0xC3	ckfinite
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ckfinite, OpCodes.Ckfinite );
            //0xC6	mkrefany
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Mkrefany, OpCodes.Mkrefany );
            //0xD0	ldtoken
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldtoken, OpCodes.Ldtoken );
            //0xD1	conv.u2
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_U2, OpCodes.Conv_U2 );
            //0xD2	conv.u1
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_U1, OpCodes.Conv_U1 );
            //0xD3	conv.i
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_I, OpCodes.Conv_I );
            //0xD4	conv.ovf.i
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_I, OpCodes.Conv_Ovf_I );
            //0xD5	conv.ovf.u
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_Ovf_U, OpCodes.Conv_Ovf_U );
            //0xD6	add.ovf
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Add_Ovf, OpCodes.Add_Ovf );
            //0xD7	add.ovf.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Add_Ovf_Un, OpCodes.Add_Ovf_Un );
            //0xD8	mul.ovf
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Mul_Ovf, OpCodes.Mul_Ovf );
            //0xD9	mul.ovf.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Mul_Ovf_Un, OpCodes.Mul_Ovf_Un );
            //0xDA	sub.ovf
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Sub_Ovf, OpCodes.Sub_Ovf );
            //0xDB	sub.ovf.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Sub_Ovf_Un, OpCodes.Sub_Ovf_Un );
            //0xDC	endfinally
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Endfinally, OpCodes.Endfinally );
            //0xDD	leave
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Leave, OpCodes.Leave );
            //0xDE	leave.s
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Leave_S, OpCodes.Leave_S );
            //0xDF	stind.i
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stind_I, OpCodes.Stind_I );
            //0xE0	conv.u
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Conv_U, OpCodes.Conv_U );
            //0xFE 0x00	arglist
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Arglist, OpCodes.Arglist );
            //0xFE 0x01	ceq
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ceq, OpCodes.Ceq );
            //0xFE 0x02	cgt
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Cgt, OpCodes.Cgt );
            //0xFE 0x03	cgt.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Cgt_Un, OpCodes.Cgt_Un );
            //0xFE 0x04	clt
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Clt, OpCodes.Clt );
            //0xFE 0x05	clt.un
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Clt_Un, OpCodes.Clt_Un );
            //0xFE 0x06	ldftn
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldftn, OpCodes.Ldftn );
            //0xFE 0x07	ldvirtftn
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldvirtftn, OpCodes.Ldvirtftn );
            //0xFE 0x09	ldarg
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldarg, OpCodes.Ldarg );
            //0xFE 0x0A	ldarga
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldarga, OpCodes.Ldarga );
            //0xFE 0x0B	starg
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Starg, OpCodes.Starg );
            //0xFE 0x0C	ldloc
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldloc, OpCodes.Ldloc );
            //0xFE 0x0D	ldloca
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Ldloca, OpCodes.Ldloca );
            //0xFE 0x0E	stloc
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Stloc, OpCodes.Stloc );
            //0xFE 0x0F	localloc
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Localloc, OpCodes.Localloc );
            //0xFE 0x11	endfilter
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Endfilter, OpCodes.Endfilter );
            //0xFE 0x12	unaligned.
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Unaligned, OpCodes.Unaligned );
            //0xFE 0x13	volatile.
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Volatile, OpCodes.Volatile );
            //0xFE 0x15	initobj
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Initobj, OpCodes.Initobj );
            //0xFE 0x17	cpblk
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Cpblk, OpCodes.Cpblk );
            //0xFE 0x18	initblk
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Initblk, OpCodes.Initblk );
            //0xFE 0x1A	rethrow
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Rethrow, OpCodes.Rethrow );
            //0xFE 0x1C	sizeof
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Sizeof, OpCodes.Sizeof );
            //0xFE 0x1D	refanytype
            mapOpCodeNumberToOpCode.SetValue( OpCodeNumber.Refanytype, OpCodes.Refanytype );
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if an instruction code is incorrect.
        /// </summary>
        /// <param name="opCode">An opcode.</param>
        [Conditional( "ASSERT" )]
        public static void AssertValidInstruction( OpCodeNumber opCode )
        {
            mapOpCodeNumberToOpCode.GetValue( opCode );
        }

        /// <summary>
        /// Adds the mapping between an opcode and its uncompressed form.
        /// </summary>
        /// <param name="compressedOpCode">The compressed opcode.</param>
        /// <param name="uncompressedOpCode">The uncompressed opcode.</param>
        /// <param name="operandType">Type of the uncompressed opcode operand.</param>
        /// <param name="uncompressedOperand">Value of the uncompressed opcode operand.</param>
        private static void AddCompressedOperandMapping( OpCodeNumber compressedOpCode, OpCodeNumber uncompressedOpCode,
                                                         OperandType operandType,
                                                         int uncompressedOperand )
        {
            mapOpCodeNumberToCompressedOperand.SetValue( compressedOpCode,
                                                         new UncompressedOpCode( uncompressedOpCode, operandType,
                                                                                 uncompressedOperand ) );
        }

        #endregion

        #region Get methods

        /// <summary>
        /// Gets the <see cref="OpCode"/> corresponding to an <see cref="OpCodeNumber"/>.
        /// </summary>
        /// <param name="opCodeNumber">An <see cref="OpCodeNumber"/>.</param>
        /// <returns>The <see cref="OpCode"/> corresponding to <paramref name="opCodeNumber"/>.</returns>
        [SuppressMessage( "Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase" )]
        public static OpCode GetOpCode( OpCodeNumber opCodeNumber )
        {
            return mapOpCodeNumberToOpCode.GetValue( opCodeNumber );
        }

        /// <summary>
        /// Gets the <see cref="FlowControl"/> of a given opcode.
        /// </summary>
        /// <param name="opCodeNumber">An <see cref="OpCodeNumber"/>.</param>
        /// <returns>The <see cref="FlowControl"/> corresponding to <paramref name="opCodeNumber"/>.</returns>
        public static FlowControl GetFlowControl( OpCodeNumber opCodeNumber )
        {
            return mapOpCodeNumberToOpCode.GetValue( opCodeNumber ).FlowControl;
        }

        /// <summary>
        /// Gets the name of a given opcode.
        /// </summary>
        /// <param name="opCodeNumber">An <see cref="OpCodeNumber"/>.</param>
        /// <returns>The name of the opcode corresponding to <paramref name="opCodeNumber"/>.</returns>
        public static string GetName( OpCodeNumber opCodeNumber )
        {
            return mapOpCodeNumberToOpCode.GetValue( opCodeNumber ).Name;
        }

        /// <summary>
        /// Gets the <see cref="OpCodeType"/> of a given opcode.
        /// </summary>
        /// <param name="opCodeNumber">An <see cref="OpCodeNumber"/>.</param>
        /// <returns>The <see cref="OpCodeType"/> corresponding to <paramref name="opCodeNumber"/>.</returns>
        [SuppressMessage( "Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase" )]
        public static OpCodeType GetOpCodeType( OpCodeNumber opCodeNumber )
        {
            return mapOpCodeNumberToOpCode.GetValue( opCodeNumber ).OpCodeType;
        }

        /// <summary>
        /// Gets the <see cref="OperandType"/> of a given opcode.
        /// </summary>
        /// <param name="opCodeNumber">An <see cref="OpCodeNumber"/>.</param>
        /// <returns>The <see cref="OpCodeType"/> corresponding to <paramref name="opCodeNumber"/>.</returns>
        public static OperandType GetOperandType( OpCodeNumber opCodeNumber )
        {
            return mapOpCodeNumberToOpCode.GetValue( opCodeNumber ).OperandType;
        }

        /// <summary>
        /// Gets the size of a given opcode.
        /// </summary>
        /// <param name="opCodeNumber">An <see cref="OpCodeNumber"/>.</param>
        /// <returns>The size of the opcode corresponding to <paramref name="opCodeNumber"/>.</returns>
        public static int GetSize( OpCodeNumber opCodeNumber )
        {
            return mapOpCodeNumberToOpCode.GetValue( opCodeNumber ).Size;
        }

        /// <summary>
        /// Gets the <i>pop</i> <see cref="StackBehaviour"/> of a given opcode.
        /// </summary>
        /// <param name="opCodeNumber">An <see cref="OpCodeNumber"/>.</param>
        /// <returns>The <i>pop</i> <see cref="StackBehaviour"/> corresponding to <paramref name="opCodeNumber"/>.</returns>
        [SuppressMessage( "Microsoft.Naming", "CA1704",
            Justification="Spelling is correct." )]
        public static StackBehaviour GetStackBehaviourPop( OpCodeNumber opCodeNumber )
        {
            return mapOpCodeNumberToOpCode.GetValue( opCodeNumber ).StackBehaviourPop;
        }


        /// <summary>
        /// Gets the <i>push</i> <see cref="StackBehaviour"/> of a given opcode.
        /// </summary>
        /// <param name="opCodeNumber">An <see cref="OpCodeNumber"/>.</param>
        /// <returns>The <i>push</i> <see cref="StackBehaviour"/> corresponding to <paramref name="opCodeNumber"/>.</returns>
        [SuppressMessage( "Microsoft.Naming", "CA1704",
            Justification = "Spelling is correct." )]
        public static StackBehaviour GetStackBehaviourPush( OpCodeNumber opCodeNumber )
        {
            return mapOpCodeNumberToOpCode.GetValue( opCodeNumber ).StackBehaviourPush;
        }

        #endregion

        #region Uncompression

        /// <summary>
        /// Determines whether a given opcode is compressed.
        /// </summary>
        /// <param name="opCode">An <see cref="OpCodeNumber"/>.</param>
        /// <returns><b>true</b> if <paramref name="opCode"/> is in compressed form,
        /// otherwise <b>false</b>.</returns>
        [SuppressMessage( "Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase" )]
        public static bool IsCompressedOpCode( OpCodeNumber opCode )
        {
            return !mapOpCodeNumberToCompressedOperand.GetValue( opCode ).IsNull;
        }

        /// <summary>
        /// Gets the uncompressed opcode corresponding to a compressed opcode.
        /// </summary>
        /// <param name="compressedOpCode">The <see cref="OpCodeNumber"/> of a compressed
        /// opcode.</param>
        /// <returns>An <see cref="UncompressedOpCode"/>, or a null <see cref="UncompressedOpCode"/>
        /// if <paramref name="compressedOpCode"/> is not compressed.</returns>
        [SuppressMessage( "Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase" )]
        public static UncompressedOpCode GetUncompressedOpCode( OpCodeNumber compressedOpCode )
        {
            return mapOpCodeNumberToCompressedOperand.GetValue( compressedOpCode );
        }

        #endregion

        /// <summary>
        /// Determines whether an opcode takes a parameter operand.
        /// </summary>
        /// <param name="opCode">An <see cref="OpCodeNumber"/>.</param>
        /// <returns><b>true</b> if <paramref name="opCode"/> takes a parameter
        /// operand, <b>false</b> otherwise.</returns>
        public static bool IsParameterOperand( OpCodeNumber opCode )
        {
            switch ( opCode )
            {
                case OpCodeNumber.Ldarg:
                case OpCodeNumber.Ldarg_S:
                case OpCodeNumber.Ldarga:
                case OpCodeNumber.Ldarga_S:
                case OpCodeNumber.Starg:
                case OpCodeNumber.Starg_S:
                    return true;

                default:
                    return false;
            }
        }
    }


    /// <summary>
    /// Uncompressed opcode (i.e. an opcode with its operand).
    /// </summary>
    [SuppressMessage( "Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase" )]
    public struct UncompressedOpCode : IEquatable<UncompressedOpCode>
    {
        #region Fields

        /// <summary>
        /// Uncompressed <see cref="OpCodeNumber"/>.
        /// </summary>
        private readonly OpCodeNumber opCodeNumber;

        /// <summary>
        /// Operand.
        /// </summary>
        private readonly int operand;

        /// <summary>
        /// Operand type.
        /// </summary>
        private readonly OperandType operandType;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="UncompressedOpCode"/>.
        /// </summary>
        /// <param name="opCodeNumber">The uncompressed <see cref="OpCodeNumber"/>.</param>
        /// <param name="operandType">The <see cref="OperandType"/> of <paramref name="opCodeNumber"/>.</param>
        /// <param name="operand">The operand.</param>
        internal UncompressedOpCode( OpCodeNumber opCodeNumber, OperandType operandType, int operand )
        {
            this.opCodeNumber = opCodeNumber;
            this.operand = operand;
            this.operandType = operandType;
        }

        /// <summary>
        /// Gets the uncompressed <see cref="OpCodeNumber"/>.
        /// </summary>
        [SuppressMessage( "Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase" )]
        public OpCodeNumber OpCodeNumber { get { return opCodeNumber; } }

        /// <summary>
        /// Gets the <see cref="OperandType"/> of <see cref="UncompressedOpCode.OpCodeNumber"/>.
        /// </summary>
        public OperandType OperandType { get { return operandType; } }

        /// <summary>
        /// Gets the uncompressed operand.
        /// </summary>
        public int Operand { get { return operand; } }

        /// <summary>
        /// Determines whether the current <see cref="UncompressedOpCode"/> is null.
        /// </summary>
        /// <remarks>
        /// A null <see cref="UncompressedOpCode"/> means that the original opcode
        /// was not compressed.
        /// </remarks>
        public bool IsNull { get { return this.opCodeNumber == OpCodeNumber.Nop; } }

        #region IEquatable<UncompressedOpCode> Members

        /// <inheritdoc />
        public bool Equals( UncompressedOpCode other )
        {
            return this.opCodeNumber == other.opCodeNumber &&
                   this.operand == other.operand &&
                   this.operandType == other.operandType;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int) this.opCodeNumber;
        }

        /// <inheritdoc />
        public override bool Equals( object obj )
        {
            if ( obj is UncompressedOpCode )
            {
                return this.Equals( (UncompressedOpCode) obj );
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether two <see cref="UncompressedOpCode"/> are equal.
        /// </summary>
        /// <param name="left">An <see cref="UncompressedOpCode"/>.</param>
        /// <param name="right">Another <see cref="UncompressedOpCode"/>.</param>
        /// <returns><b>true</b> if both <see cref="UncompressedOpCode"/> are
        /// equal, otherwise <b>false</b>.</returns>
        public static bool operator ==( UncompressedOpCode left, UncompressedOpCode right )
        {
            return left.Equals( right );
        }

        /// <summary>
        /// Determines whether two <see cref="UncompressedOpCode"/> are different.
        /// </summary>
        /// <param name="left">An <see cref="UncompressedOpCode"/>.</param>
        /// <param name="right">Another <see cref="UncompressedOpCode"/>.</param>
        /// <returns><b>true</b> if both <see cref="UncompressedOpCode"/> are
        /// different, otherwise <b>false</b>.</returns>
        public static bool operator !=( UncompressedOpCode left, UncompressedOpCode right )
        {
            return !left.Equals( right );
        }

        #endregion
    }
}
