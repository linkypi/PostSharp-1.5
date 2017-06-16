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

using System.Diagnostics.CodeAnalysis;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>Enumeration of all opcode numbers.</summary>
    [SuppressMessage( "Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase" )]
    public enum OpCodeNumber
    {
        /// <summary>0x00	nop</summary>
        Nop = 0x00,
        /// <summary>0x01	break</summary>
        Break = 0x01,
        /// <summary>0x02	ldarg.0</summary>
        Ldarg_0 = 0x02,
        /// <summary>0x03	ldarg.1</summary>
        Ldarg_1 = 0x03,
        /// <summary>0x04	ldarg.2</summary>
        Ldarg_2 = 0x04,
        /// <summary>0x05	ldarg.3</summary>
        Ldarg_3 = 0x05,
        /// <summary>0x06	ldloc.0</summary>
        Ldloc_0 = 0x06,
        /// <summary>0x07	ldloc.1</summary>
        Ldloc_1 = 0x07,
        /// <summary>0x08	ldloc.2</summary>
        Ldloc_2 = 0x08,
        /// <summary>0x09	ldloc.3</summary>
        Ldloc_3 = 0x09,
        /// <summary>0x0A	stloc.0</summary>
        Stloc_0 = 0x0A,
        /// <summary>0x0B	stloc.1</summary>
        Stloc_1 = 0x0B,
        /// <summary>0x0C	stloc.2</summary>
        Stloc_2 = 0x0C,
        /// <summary>0x0D	stloc.3</summary>
        Stloc_3 = 0x0D,
        /// <summary>0x0E	ldarg.s</summary>
        Ldarg_S = 0x0E,
        /// <summary>0x0F	ldarga.s</summary>
        Ldarga_S = 0x0F,
        /// <summary>0x10	starg.s</summary>
        Starg_S = 0x10,
        /// <summary>0x11	ldloc.s</summary>
        Ldloc_S = 0x11,
        /// <summary>0x12	ldloca.s</summary>
        Ldloca_S = 0x12,
        /// <summary>0x13	stloc.s</summary>
        Stloc_S = 0x13,
        /// <summary>0x14	ldnull</summary>
        Ldnull = 0x14,
        /// <summary>0x15	ldc.i4.m1</summary>
        Ldc_I4_M1 = 0x15,
        /// <summary>0x16	ldc.i4.0</summary>
        Ldc_I4_0 = 0x16,
        /// <summary>0x17	ldc.i4.1</summary>
        Ldc_I4_1 = 0x17,
        /// <summary>0x18	ldc.i4.2</summary>
        Ldc_I4_2 = 0x18,
        /// <summary>0x19	ldc.i4.3</summary>
        Ldc_I4_3 = 0x19,
        /// <summary>0x1A	ldc.i4.4</summary>
        Ldc_I4_4 = 0x1A,
        /// <summary>0x1B	ldc.i4.5</summary>
        Ldc_I4_5 = 0x1B,
        /// <summary>0x1C	ldc.i4.6</summary>
        Ldc_I4_6 = 0x1C,
        /// <summary>0x1D	ldc.i4.7</summary>
        Ldc_I4_7 = 0x1D,
        /// <summary>0x1E	ldc.i4.8</summary>
        Ldc_I4_8 = 0x1E,
        /// <summary>0x1F	ldc.i4.s</summary>
        Ldc_I4_S = 0x1F,
        /// <summary>0x20	ldc.i4</summary>
        Ldc_I4 = 0x20,
        /// <summary>0x21	ldc.i8</summary>
        Ldc_I8 = 0x21,
        /// <summary>0x22	ldc.r4</summary>
        Ldc_R4 = 0x22,
        /// <summary>0x23	ldc.r8</summary>
        Ldc_R8 = 0x23,
        /// <summary>0x25	dup</summary>
        Dup = 0x25,
        /// <summary>0x26	pop</summary>
        Pop = 0x26,
        /// <summary>0x27	jmp</summary>
        Jmp = 0x27,
        /// <summary>0x28	call</summary>
        Call = 0x28,
        /// <summary>0x29	calli</summary>
        Calli = 0x29,
        /// <summary>0x2A	ret</summary>
        Ret = 0x2A,
        /// <summary>0x2B	br.s</summary>
        Br_S = 0x2B,
        /// <summary>0x2C	brfalse.s</summary>
        Brfalse_S = 0x2C,
        /// <summary>0x2D	brtrue.s</summary>
        Brtrue_S = 0x2D,
        /// <summary>0x2E	beq.s</summary>
        Beq_S = 0x2E,
        /// <summary>0x2F	bge.s</summary>
        Bge_S = 0x2F,
        /// <summary>0x30	bgt.s</summary>
        Bgt_S = 0x30,
        /// <summary>0x31	ble.s</summary>
        Ble_S = 0x31,
        /// <summary>0x32	blt.s</summary>
        Blt_S = 0x32,
        /// <summary>0x33	bne.un.s</summary>
        Bne_Un_S = 0x33,
        /// <summary>0x34	bge.un.s</summary>
        Bge_Un_S = 0x34,
        /// <summary>0x35	bgt.un.s</summary>
        Bgt_Un_S = 0x35,
        /// <summary>0x36	ble.un.s</summary>
        Ble_Un_S = 0x36,
        /// <summary>0x37	blt.un.s</summary>
        Blt_Un_S = 0x37,
        /// <summary>0x38	br</summary>
        Br = 0x38,
        /// <summary>0x39	brfalse</summary>
        Brfalse = 0x39,
        /// <summary>0x3A	brtrue</summary>
        Brtrue = 0x3A,
        /// <summary>0x3B	beq</summary>
        Beq = 0x3B,
        /// <summary>0x3C	bge</summary>
        Bge = 0x3C,
        /// <summary>0x3D	bgt</summary>
        Bgt = 0x3D,
        /// <summary>0x3E	ble</summary>
        Ble = 0x3E,
        /// <summary>0x3F	blt</summary>
        Blt = 0x3F,
        /// <summary>0x40	bne.un</summary>
        Bne_Un = 0x40,
        /// <summary>0x41	bge.un</summary>
        Bge_Un = 0x41,
        /// <summary>0x42	bgt.un</summary>
        Bgt_Un = 0x42,
        /// <summary>0x43	ble.un</summary>
        Ble_Un = 0x43,
        /// <summary>0x44	blt.un</summary>
        Blt_Un = 0x44,
        /// <summary>0x45	switch</summary>
        Switch = 0x45,
        /// <summary>0x46	ldind.i1</summary>
        Ldind_I1 = 0x46,
        /// <summary>0x47	ldind.u1</summary>
        Ldind_U1 = 0x47,
        /// <summary>0x48	ldind.i2</summary>
        Ldind_I2 = 0x48,
        /// <summary>0x49	ldind.u2</summary>
        Ldind_U2 = 0x49,
        /// <summary>0x4A	ldind.i4</summary>
        Ldind_I4 = 0x4A,
        /// <summary>0x4B	ldind.u4</summary>
        Ldind_U4 = 0x4B,
        /// <summary>0x4C	ldind.i8</summary>
        Ldind_I8 = 0x4C,
        /// <summary>0x4D	ldind.i</summary>
        Ldind_I = 0x4D,
        /// <summary>0x4E	ldind.r4</summary>
        Ldind_R4 = 0x4E,
        /// <summary>0x4F	ldind.r8</summary>
        Ldind_R8 = 0x4F,
        /// <summary>0x50	ldind.ref</summary>
        Ldind_Ref = 0x50,
        /// <summary>0x51	stind.ref</summary>
        Stind_Ref = 0x51,
        /// <summary>0x52	stind.i1</summary>
        Stind_I1 = 0x52,
        /// <summary>0x53	stind.i2</summary>
        Stind_I2 = 0x53,
        /// <summary>0x54	stind.i4</summary>
        Stind_I4 = 0x54,
        /// <summary>0x55	stind.i8</summary>
        Stind_I8 = 0x55,
        /// <summary>0x56	stind.r4</summary>
        Stind_R4 = 0x56,
        /// <summary>0x57	stind.r8</summary>
        Stind_R8 = 0x57,
        /// <summary>0x58	add</summary>
        Add = 0x58,
        /// <summary>0x59	sub</summary>
        Sub = 0x59,
        /// <summary>0x5A	mul</summary>
        Mul = 0x5A,
        /// <summary>0x5B	div</summary>
        Div = 0x5B,
        /// <summary>0x5C	div.un</summary>
        Div_Un = 0x5C,
        /// <summary>0x5D	rem</summary>
        Rem = 0x5D,
        /// <summary>0x5E	rem.un</summary>
        Rem_Un = 0x5E,
        /// <summary>0x5F	and</summary>
        And = 0x5F,
        /// <summary>0x60	or</summary>
        Or = 0x60,
        /// <summary>0x61	xor</summary>
        Xor = 0x61,
        /// <summary>0x62	shl</summary>
        Shl = 0x62,
        /// <summary>0x63	shr</summary>
        Shr = 0x63,
        /// <summary>0x64	shr.un</summary>
        Shr_Un = 0x64,
        /// <summary>0x65	neg</summary>
        Neg = 0x65,
        /// <summary>0x66	not</summary>
        Not = 0x66,
        /// <summary>0x67	conv.i1</summary>
        Conv_I1 = 0x67,
        /// <summary>0x68	conv.i2</summary>
        Conv_I2 = 0x68,
        /// <summary>0x69	conv.i4</summary>
        Conv_I4 = 0x69,
        /// <summary>0x6A	conv.i8</summary>
        Conv_I8 = 0x6A,
        /// <summary>0x6B	conv.r4</summary>
        Conv_R4 = 0x6B,
        /// <summary>0x6C	conv.r8</summary>
        Conv_R8 = 0x6C,
        /// <summary>0x6D	conv.u4</summary>
        Conv_U4 = 0x6D,
        /// <summary>0x6E	conv.u8</summary>
        Conv_U8 = 0x6E,
        /// <summary>0x6F	callvirt</summary>
        Callvirt = 0x6F,
        /// <summary>0x70	cpobj</summary>
        Cpobj = 0x70,
        /// <summary>0x71	ldobj</summary>
        Ldobj = 0x71,
        /// <summary>0x72	ldstr</summary>
        Ldstr = 0x72,
        /// <summary>0x73	newobj</summary>
        Newobj = 0x73,
        /// <summary>0x74	castclass</summary>
        Castclass = 0x74,
        /// <summary>0x75	isinst</summary>
        Isinst = 0x75,
        /// <summary>0x76	conv.r.un</summary>
        Conv_R_Un = 0x76,
        /// <summary>0x79	unbox</summary>
        Unbox = 0x79,
        /// <summary>0x7A	throw</summary>
        Throw = 0x7A,
        /// <summary>0x7B	ldfld</summary>
        Ldfld = 0x7B,
        /// <summary>0x7C	ldflda</summary>
        Ldflda = 0x7C,
        /// <summary>0x7D	stfld</summary>
        Stfld = 0x7D,
        /// <summary>0x7E	ldsfld</summary>
        Ldsfld = 0x7E,
        /// <summary>0x7F	ldsflda</summary>
        Ldsflda = 0x7F,
        /// <summary>0x80	stsfld</summary>
        Stsfld = 0x80,
        /// <summary>0x81	stobj</summary>
        Stobj = 0x81,
        /// <summary>0x82	conv.ovf.i1.un</summary>
        Conv_Ovf_I1_Un = 0x82,
        /// <summary>0x83	conv.ovf.i2.un</summary>
        Conv_Ovf_I2_Un = 0x83,
        /// <summary>0x84	conv.ovf.i4.un</summary>
        Conv_Ovf_I4_Un = 0x84,
        /// <summary>0x85	conv.ovf.i8.un</summary>
        Conv_Ovf_I8_Un = 0x85,
        /// <summary>0x86	conv.ovf.u1.un</summary>
        Conv_Ovf_U1_Un = 0x86,
        /// <summary>0x87	conv.ovf.u2.un</summary>
        Conv_Ovf_U2_Un = 0x87,
        /// <summary>0x88	conv.ovf.u4.un</summary>
        Conv_Ovf_U4_Un = 0x88,
        /// <summary>0x89	conv.ovf.u8.un</summary>
        Conv_Ovf_U8_Un = 0x89,
        /// <summary>0x8A	conv.ovf.i.un</summary>
        Conv_Ovf_I_Un = 0x8A,
        /// <summary>0x8B	conv.ovf.u.un</summary>
        Conv_Ovf_U_Un = 0x8B,
        /// <summary>0x8C	box</summary>
        Box = 0x8C,
        /// <summary>0x8D	newarr</summary>
        Newarr = 0x8D,
        /// <summary>0x8E	ldlen</summary>
        Ldlen = 0x8E,
        /// <summary>0x8F	ldelema</summary>
        Ldelema = 0x8F,
        /// <summary>0x90	ldelem.i1</summary>
        Ldelem_I1 = 0x90,
        /// <summary>0x91	ldelem.u1</summary>
        Ldelem_U1 = 0x91,
        /// <summary>0x92	ldelem.i2</summary>
        Ldelem_I2 = 0x92,
        /// <summary>0x93	ldelem.u2</summary>
        Ldelem_U2 = 0x93,
        /// <summary>0x94	ldelem.i4</summary>
        Ldelem_I4 = 0x94,
        /// <summary>0x95	ldelem.u4</summary>
        Ldelem_U4 = 0x95,
        /// <summary>0x96	ldelem.i8</summary>
        Ldelem_I8 = 0x96,
        /// <summary>0x97	ldelem.i</summary>
        Ldelem_I = 0x97,
        /// <summary>0x98	ldelem.r4</summary>
        Ldelem_R4 = 0x98,
        /// <summary>0x99	ldelem.r8</summary>
        Ldelem_R8 = 0x99,
        /// <summary>0x9A	ldelem.ref</summary>
        Ldelem_Ref = 0x9A,
        /// <summary>0x9B	stelem.i</summary>
        Stelem_I = 0x9B,
        /// <summary>0x9C	stelem.i1</summary>
        Stelem_I1 = 0x9C,
        /// <summary>0x9D	stelem.i2</summary>
        Stelem_I2 = 0x9D,
        /// <summary>0x9E	stelem.i4</summary>
        Stelem_I4 = 0x9E,
        /// <summary>0x9F	stelem.i8</summary>
        Stelem_I8 = 0x9F,
        /// <summary>0xA0	stelem.r4</summary>
        Stelem_R4 = 0xA0,
        /// <summary>0xA1	stelem.r8</summary>
        Stelem_R8 = 0xA1,
        /// <summary>0xA2	stelem.ref</summary>
        Stelem_Ref = 0xA2,
        /// <summary>0xA3   ldelem</summary>
        Ldelem = 0xA3,
        /// <summary>0xA4   stelem</summary>
        Stelem = 0xA4,
        /// <summary>0xA5   unbox.any</summary>
        Unbox_Any = 0xA5,
        /// <summary>0xB3	conv.ovf.i1</summary>
        Conv_Ovf_I1 = 0xB3,
        /// <summary>0xB4	conv.ovf.u1</summary>
        Conv_Ovf_U1 = 0xB4,
        /// <summary>0xB5	conv.ovf.i2</summary>
        Conv_Ovf_I2 = 0xB5,
        /// <summary>0xB6	conv.ovf.u2</summary>
        Conv_Ovf_U2 = 0xB6,
        /// <summary>0xB7	conv.ovf.i4</summary>
        Conv_Ovf_I4 = 0xB7,
        /// <summary>0xB8	conv.ovf.u4</summary>
        Conv_Ovf_U4 = 0xB8,
        /// <summary>0xB9	conv.ovf.i8</summary>
        Conv_Ovf_I8 = 0xB9,
        /// <summary>0xBA	conv.ovf.u8</summary>
        Conv_Ovf_U8 = 0xBA,
        /// <summary>0xC2	refanyval</summary>
        Refanyval = 0xC2,
        /// <summary>0xC3	ckfinite</summary>
        Ckfinite = 0xC3,
        /// <summary>0xC6	mkrefany</summary>
        Mkrefany = 0xC6,
        /// <summary>0xD0	ldtoken</summary>
        Ldtoken = 0xD0,
        /// <summary>0xD1	conv.u2</summary>
        Conv_U2 = 0xD1,
        /// <summary>0xD2	conv.u1</summary>
        Conv_U1 = 0xD2,
        /// <summary>0xD3	conv.i</summary>
        Conv_I = 0xD3,
        /// <summary>0xD4	conv.ovf.i</summary>
        Conv_Ovf_I = 0xD4,
        /// <summary>0xD5	conv.ovf.u</summary>
        Conv_Ovf_U = 0xD5,
        /// <summary>0xD6	add.ovf</summary>
        Add_Ovf = 0xD6,
        /// <summary>0xD7	add.ovf.un</summary>
        Add_Ovf_Un = 0xD7,
        /// <summary>0xD8	mul.ovf</summary>
        Mul_Ovf = 0xD8,
        /// <summary>0xD9	mul.ovf.un</summary>
        Mul_Ovf_Un = 0xD9,
        /// <summary>0xDA	sub.ovf</summary>
        Sub_Ovf = 0xDA,
        /// <summary>0xDB	sub.ovf.un</summary>
        Sub_Ovf_Un = 0xDB,
        /// <summary>0xDC	endfinally</summary>
        Endfinally = 0xDC,
        /// <summary>0xDD	leave</summary>
        Leave = 0xDD,
        /// <summary>0xDE	leave.s</summary>
        Leave_S = 0xDE,
        /// <summary>0xDF	stind.i</summary>
        Stind_I = 0xDF,
        /// <summary>0xE0	conv.u</summary>
        Conv_U = 0xE0,
        /// <summary>
        /// Number of short opcodes.
        /// </summary>
        [SuppressMessage( "Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores" )] _CountShort = 0xE1,
        /// <summary>0xFE 0x00	arglist</summary>
        Arglist = 0xFE00,
        /// <summary>0xFE 0x01	ceq</summary>
        Ceq = 0xFE01,
        /// <summary>0xFE 0x02	cgt</summary>
        Cgt = 0xFE02,
        /// <summary>0xFE 0x03	cgt.un</summary>
        Cgt_Un = 0xFE03,
        /// <summary>0xFE 0x04	clt</summary>
        Clt = 0xFE04,
        /// <summary>0xFE 0x05	clt.un</summary>
        Clt_Un = 0xFE05,
        /// <summary>0xFE 0x06	ldftn</summary>
        Ldftn = 0xFE06,
        /// <summary>0xFE 0x07	ldvirtftn</summary>
        Ldvirtftn = 0xFE07,
        /// <summary>0xFE 0x09	ldarg</summary>
        Ldarg = 0xFE09,
        /// <summary>0xFE 0x0A	ldarga</summary>
        Ldarga = 0xFE0A,
        /// <summary>0xFE 0x0B	starg</summary>
        Starg = 0xFE0B,
        /// <summary>0xFE 0x0C	ldloc</summary>
        Ldloc = 0xFE0C,
        /// <summary>0xFE 0x0D	ldloca</summary>
        Ldloca = 0xFE0D,
        /// <summary>0xFE 0x0E	stloc</summary>
        Stloc = 0xFE0E,
        /// <summary>0xFE 0x0F	localloc</summary>
        Localloc = 0xFE0F,
        /// <summary>0xFE 0x11	endfilter</summary>
        Endfilter = 0xFE11,
        /// <summary>0xFE 0x12	unaligned.</summary>
        Unaligned = 0xFE12,
        /// <summary>0xFE 0x13	volatile.</summary>
        Volatile = 0xFE13,
        /// <summary>0xFE 0x14	tail.</summary>
        Tail = 0xFE14,
        /// <summary>0xFE 0x15	initobj</summary>
        Initobj = 0xFE15,
        /// <summary>0xFE 0x16  constrained.</summary>
        Constrained = 0xFE16,
        /// <summary>0xFE 0x17	cpblk</summary>
        Cpblk = 0xFE17,
        /// <summary>0xFE 0x18	initblk</summary>
        Initblk = 0xFE18,
        /// <summary>0xFE 0x1A	rethrow</summary>
        Rethrow = 0xFE1A,
        /// <summary>0xFE 0x1C	sizeof</summary>
        Sizeof = 0xFE1C,
        /// <summary>0xFE 0x1D	refanytype</summary>
        Refanytype = 0xFE1D,
        /// <summary>0xFE 0x1E readonly.</summary>
        Readonly = 0xFE1E,
        /// <summary>
        /// Pseudo-instruction used internally by to denote a <see cref="SymbolSequencePoint"/>.
        /// </summary>
        /// <remarks>
        /// The operand of this pseudo-instruction is an <b>int16</b>, which is the token
        /// of the symbol sequence point, i.e. its position in the array of the
        /// <see cref="MethodBodyDeclaration"/>.
        /// </remarks>
        [SuppressMessage( "Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores" )] _SequencePoint = 0xFEFF
        ,
        /// <summary>
        /// Number of large opcodes.
        /// </summary>
        [SuppressMessage( "Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores" )] _CountLarge = 0x1F
    }
}