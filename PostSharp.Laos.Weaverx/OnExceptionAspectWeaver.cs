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
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.CodeWeaver;
using PostSharp.Collections;

namespace PostSharp.Laos.Weaver
{
    internal class OnExceptionAspectWeaver : MethodLevelAspectWeaver, IMethodLevelAdvice, ITypedExceptionAdvice
    {
        private IMethod setExceptionMethod;
        private IMethod onExceptionMethod;
        private IMethod getFlowBehaviorMethod;
        private IMethod getReturnValueMethod;
        private ITypeSignature exceptionType;

        private static readonly OnExceptionAspectConfigurationAttribute defaultConfiguration =
            new OnExceptionAspectConfigurationAttribute();


        public OnExceptionAspectWeaver() : base( defaultConfiguration )
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            ModuleDeclaration module = this.Task.Project.Module;

            ITypeSignature methodExecutionEventArgsType = module.GetTypeForFrameworkVariant(typeof(MethodExecutionEventArgs));

            this.onExceptionMethod = this.Task.Project.Module.FindMethod(
                module.GetTypeForFrameworkVariant( typeof(IExceptionHandlerAspect) ),
                "OnException" );

            
            this.getReturnValueMethod = module.Cache.GetItem( () => module.FindMethod(
                                                                        methodExecutionEventArgsType,
                                                                        "get_ReturnValue"));

            this.getFlowBehaviorMethod = module.Cache.GetItem( () => module.FindMethod(
                                                                         methodExecutionEventArgsType,
                                                                         "get_FlowBehavior") );


            this.setExceptionMethod = module.Cache.GetItem( () => module.FindMethod(
                                                                      methodExecutionEventArgsType,
                                                                      "set_Exception"));
        }


        public override void Implement()
        {
            base.Implement();

            MethodDefDeclaration targetMethodDefDeclaration = (MethodDefDeclaration) this.TargetElement;

            // Do not weave if the target method has no body.
            if ( !targetMethodDefDeclaration.MayHaveBody )
            {
                return;
            }

            // Get the caught exception type.
            TypeIdentity exceptionReflectionTypeIdentity =
                this.GetConfigurationObject<IOnExceptionAspectConfiguration, TypeIdentity>(
                    c => c.GetExceptionType( this.TargetReflectionMethod ) );
            if ( exceptionReflectionTypeIdentity != null )
            {
                if ( exceptionReflectionTypeIdentity.Type != null )
                {
                    this.exceptionType = this.Task.Project.Module.FindType( exceptionReflectionTypeIdentity.Type,
                                                                            BindingOptions.RequireGenericInstance );
                }
                else
                {
                    this.exceptionType =
                        this.Task.Project.Module.Cache.GetType( exceptionReflectionTypeIdentity.TypeName );
                }
            }
            else
            {
                this.exceptionType = null;
            }

            // Add a low-level advice.
            this.Task.MethodLevelAdvices.Add( this );

            // Get the instance tag field.
            this.InitializeInstanceTag( targetMethodDefDeclaration.DeclaringType, targetMethodDefDeclaration.IsStatic );
        }

        #region IMethodLevelAdvice Members

        MethodDefDeclaration IMethodLevelAdvice.Method
        {
            get { return (MethodDefDeclaration) this.TargetElement; }
        }

        MetadataDeclaration IMethodLevelAdvice.Operand
        {
            get { return null; }
        }

        JoinPointKinds IMethodLevelAdvice.JoinPointKinds
        {
            get { return JoinPointKinds.AfterMethodBodyException; }
        }

        #endregion

        #region IAdvice Members

        int IAdvice.Priority
        {
            get { return this.AspectPriority; }
        }

        bool IAdvice.RequiresWeave( WeavingContext context )
        {
            return true;
        }

        void IAdvice.Weave( WeavingContext context, InstructionBlock block )
        {
            MethodBodyDeclaration methodBody = block.MethodBody;
            MethodDefDeclaration targetMethod = (MethodDefDeclaration) this.TargetElement;

            InstructionWriter writer = context.InstructionWriter;
            InstructionSequence instructionSequence = methodBody.CreateInstructionSequence();
            InstructionBlock rootInstructionBlock = methodBody.RootInstructionBlock;
            block.AddInstructionSequence( instructionSequence, NodePosition.Before, null );
            writer.AttachInstructionSequence( instructionSequence );

            writer.EmitSymbolSequencePoint( SymbolSequencePoint.Hidden );

            // Save the exception
            methodBody.InitLocalVariables = true;
            LocalVariableSymbol exceptionLocal = rootInstructionBlock.DefineLocalVariable(
                this.exceptionType, "~exception~{0}" );
            writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc, exceptionLocal );

            // Build the context object.
            LocalVariableSymbol arrayOfArgumentsLocal;
            LocalVariableSymbol eventArgsLocal;
            this.Task.EventArgsBuilders.BuildMethodExecutionEventArgs( targetMethod, writer,
                                                                       this.TargetMethodRuntimeInstanceField,
                                                                       out eventArgsLocal, out arrayOfArgumentsLocal );

            // Set the exception.
            writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, eventArgsLocal );
            writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, exceptionLocal );
            writer.EmitInstructionMethod( OpCodeNumber.Call, this.setExceptionMethod );

            // Load the instance tag.
            this.Task.InstanceTagManager.EmitLoadInstanceTag( eventArgsLocal, this.InstanceTagField, writer );

            // Call: void OnException(MethodExecutionEventArgs context)
            writer.EmitInstructionField( OpCodeNumber.Ldsfld, this.AspectRuntimeInstanceField );
            writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, eventArgsLocal );
            writer.EmitInstructionMethod( OpCodeNumber.Callvirt, onExceptionMethod );

            // Store the instance tag.
            this.Task.InstanceTagManager.EmitStoreInstanceTag( eventArgsLocal, this.InstanceTagField, writer );

            #region Process the flow control

            // Create the sequences.
            InstructionSequence rethrowSequence = methodBody.CreateInstructionSequence();
            block.AddInstructionSequence( rethrowSequence, NodePosition.After, null );

            InstructionSequence returnSequence = methodBody.CreateInstructionSequence();
            block.AddInstructionSequence( returnSequence, NodePosition.After, null );

            InstructionSequence continueSequence = methodBody.CreateInstructionSequence();
            block.AddInstructionSequence( continueSequence, NodePosition.After, null );

            // Get the desired flow behavior.
            writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, eventArgsLocal );
            writer.EmitInstructionMethod( OpCodeNumber.Call, this.getFlowBehaviorMethod );

            // Conditionally branch.
            writer.EmitSwitchInstruction(
                new[]
                    {
                        rethrowSequence /* Default */,
                        continueSequence /* Continue */,
                        rethrowSequence /* Rethrow */,
                        returnSequence /* Return */
                    } );

            writer.DetachInstructionSequence();

            // By default we fall in the 'rethrow' sequence.
            writer.AttachInstructionSequence( rethrowSequence );
            writer.EmitInstruction( OpCodeNumber.Rethrow );
            writer.DetachInstructionSequence();

            #region Emit the return sequence

            writer.AttachInstructionSequence( returnSequence );
            ITypeSignature returnType = targetMethod.ReturnParameter.ParameterType;
            if ( !IntrinsicTypeSignature.Is( returnType, IntrinsicType.Void ) )
            {
                writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, eventArgsLocal );
                writer.EmitInstructionMethod( OpCodeNumber.Call, this.getReturnValueMethod );
                context.WeavingHelper.FromObject( returnType, writer );
                writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc, context.ReturnValueVariable );
            }

            // Write back the value of output parameters
            bool hasOutParameter = false;
            for ( int i = 0; i < targetMethod.Parameters.Count; i++ )
            {
                ParameterDeclaration parameter = targetMethod.Parameters[i];

                if ( parameter.ParameterType.BelongsToClassification( TypeClassifications.Pointer ) )
                {
                    hasOutParameter = true;
                    break;
                }
            }

            if ( hasOutParameter )
            {
                context.WeavingHelper.CopyArgumentsFromArray( arrayOfArgumentsLocal,
                                                              targetMethod, writer );
            }

            writer.EmitBranchingInstruction( OpCodeNumber.Leave, context.LeaveBranchTarget );
            writer.DetachInstructionSequence();

            #endregion

            #endregion
        }

        #endregion

        #region ITypedExceptionAdvice Members

        ITypeSignature ITypedExceptionAdvice.GetExceptionType( WeavingContext context )
        {
            return this.exceptionType;
        }

        #endregion
    }
}