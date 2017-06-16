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

using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;

namespace PostSharp.Laos.Weaver
{
    internal class ImplementMethodAspectWeaver : MethodLevelAspectWeaver
    {
        private IMethod implementMethodBodyAspect_Invoke;
        private IMethod methodExecutionEventArgs_getReturnValue;

        private static readonly ImplementMethodAspectConfigurationAttribute defaultConfiguration =
            new ImplementMethodAspectConfigurationAttribute();

        public ImplementMethodAspectWeaver() : base( defaultConfiguration )
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            ModuleDeclaration module = this.Task.Project.Module;


            this.methodExecutionEventArgs_getReturnValue = module.Cache.GetItem(
                () => module.FindMethod(
                          module.GetTypeForFrameworkVariant( typeof(MethodExecutionEventArgs) ),
                          "get_ReturnValue" ) );

            this.implementMethodBodyAspect_Invoke = module.Cache.GetItem(
                () => module.FindMethod(
                          module.GetTypeForFrameworkVariant( typeof(IImplementMethodAspect) ),
                          "OnExecution" ) );
        }

        public override bool RequiresReflectionWrapper
        {
            get { return true; }
        }

        public override void Implement()
        {
            base.Implement();

            InstructionWriter instructionWriter = this.Task.InstructionWriter;
            bool isAbstract = false;

            MethodDefDeclaration targetMethod = (MethodDefDeclaration) this.TargetMethod;

            CodeWeaver.Weaver.IgnoreMethod( targetMethod );
            targetMethod.CustomAttributes.Add( this.Task.WeavingHelper.GetDebuggerNonUserCodeAttribute() );
            this.Task.WeavingHelper.AddCompilerGeneratedAttribute( targetMethod.CustomAttributes );

            // Change method flags as needed.
            if ( ( targetMethod.Attributes & MethodAttributes.Abstract ) != 0 )
            {
                targetMethod.Attributes &= ~MethodAttributes.Abstract;
                targetMethod.Attributes |= MethodAttributes.Virtual;
                isAbstract = true;
            }
            else if ( ( targetMethod.Attributes & MethodAttributes.UnmanagedExport ) != 0 )
            {
                targetMethod.Attributes &= ~MethodAttributes.UnmanagedExport;
            }

            // Create a new method body.
            MethodBodyDeclaration methodBody = new MethodBodyDeclaration();
            targetMethod.MethodBody = methodBody;


            InstructionBlock block = methodBody.CreateInstructionBlock();

            block.Comment = "Implemented by ImplementMethodBodyAspectWeaver";
            methodBody.RootInstructionBlock = block;

            InstructionSequence sequence = targetMethod.MethodBody.CreateInstructionSequence();
            block.AddInstructionSequence( sequence, NodePosition.Before, null );
            instructionWriter.AttachInstructionSequence( sequence );

            // Build the context object.
            LocalVariableSymbol arrayOfArguments;
            LocalVariableSymbol contextLocal;
            this.Task.EventArgsBuilders.BuildMethodExecutionEventArgs( targetMethod, instructionWriter,
                                                                       this.TargetMethodRuntimeInstanceField,
                                                                       out contextLocal,
                                                                       out arrayOfArguments );

            // Call: OnExecution(MethodExecutionEventArgs context)
            instructionWriter.EmitInstructionField( OpCodeNumber.Ldsfld, this.AspectRuntimeInstanceField );
            instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, contextLocal );
            instructionWriter.EmitInstructionMethod( OpCodeNumber.Callvirt, this.implementMethodBodyAspect_Invoke );

            // Store the array values in output arguments.
            this.Task.WeavingHelper.CopyArgumentsFromArray( arrayOfArguments, targetMethod, instructionWriter );

            // Cast the return value into the correct return type.
            if ( !IntrinsicTypeSignature.Is( targetMethod.ReturnParameter.ParameterType, IntrinsicType.Void ) )
            {
                instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, contextLocal );
                instructionWriter.EmitInstructionMethod( OpCodeNumber.Call, this.methodExecutionEventArgs_getReturnValue );
                this.Task.WeavingHelper.FromObject( targetMethod.ReturnParameter.ParameterType, instructionWriter );
            }

            // Return.
            instructionWriter.EmitInstruction( OpCodeNumber.Ret );
            instructionWriter.DetachInstructionSequence();

            // Check if the class should still be abstract.
            if ( isAbstract )
            {
                bool typeHasAbstractMethod = false;
                foreach ( MethodDefDeclaration methodDef in targetMethod.DeclaringType.Methods )
                {
                    if ( ( methodDef.Attributes & MethodAttributes.Abstract ) != 0 )
                    {
                        typeHasAbstractMethod = true;
                        break;
                    }
                }

                if ( !typeHasAbstractMethod )
                {
                    LaosTrace.Trace.WriteLine(
                        "The type {{{0}}} has no abstract method yet. Remove the abstract flag.",
                        targetMethod.DeclaringType );
                    targetMethod.DeclaringType.Attributes &= ~TypeAttributes.Abstract;

                    // We have also to make protected constructors public.
                    foreach ( MethodDefDeclaration methodDef in targetMethod.DeclaringType.Methods )
                    {
                        if ( methodDef.Name == ".ctor" &&
                             ( methodDef.Attributes & MethodAttributes.MemberAccessMask ) == MethodAttributes.Family )
                        {
                            LaosTrace.Trace.WriteLine( "Making the constructor {{{0}}} public.", methodDef );
                            methodDef.Attributes = ( methodDef.Attributes & ~MethodAttributes.MemberAccessMask ) |
                                                   MethodAttributes.Public;
                        }
                    }
                }
            }
        }
    }
}