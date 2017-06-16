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

using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Extensibility;

namespace PostSharp.Laos.Weaver
{
    internal class GetInstanceCredentialAdvice : IAdvice
    {
        private readonly LaosTask task;

        public GetInstanceCredentialAdvice( LaosTask task )
        {
            this.task = task;
        }

        public int Priority
        {
            get { return 0; }
        }

        public bool RequiresWeave( WeavingContext context )
        {
            return true;
        }

        public void Weave( WeavingContext context, InstructionBlock block )
        {
            ModuleDeclaration module = context.Method.Module;

            // Fail if the method is static.
            if ( context.Method.IsStatic )
            {
                LaosMessageSource.Instance.Write(SeverityType.Error, "LA0042", new object[] { "LaosUtils.GetCurrentInstanceCredentials",
                    context.Method }, context.JoinPoint.Instruction.LastSymbolSequencePoint);
                return;
            }


            // Checks that the current method can access the target method.
            TypeDefDeclaration instanceTypeDef = context.Method.DeclaringType;


            // Find the GetCredentials method on the instance type.
       
            IMethod getCredentialsMethod =
                this.task.InstanceCredentialsManager.GetGetInstanceCredentialsMethod( instanceTypeDef,
                                                                                      module.Cache.IdentityGenericMap);

            if ( getCredentialsMethod == null )
            {
                LaosMessageSource.Instance.Write( SeverityType.Error, "LA0020",
                                                  new object[] { instanceTypeDef },
                                                  context.JoinPoint.Instruction.LastSymbolSequencePoint );
                return;
            }

            // Invoke this method.
            InstructionSequence instructionSequence = block.MethodBody.CreateInstructionSequence();
            block.AddInstructionSequence( instructionSequence, NodePosition.After, null );
            context.InstructionWriter.AttachInstructionSequence( instructionSequence );
            context.InstructionWriter.EmitInstruction( OpCodeNumber.Ldarg_0 );
            context.InstructionWriter.EmitInstructionMethod( OpCodeNumber.Call, getCredentialsMethod );
            context.InstructionWriter.DetachInstructionSequence();
        }
    }
}