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
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Extensibility;

namespace PostSharp.Laos.Weaver
{
    internal class OnMethodBoundaryAspectWeaver : MethodLevelAspectWeaver, IMethodLevelAdvice, ITypedExceptionAdvice
    {
        private LocalVariableSymbol eventArgsLocal;
        private LocalVariableSymbol argumentsLocal;
        private LocalVariableSymbol exceptionLocal;
        private IMethod onEntryMethod;
        private IMethod onExitMethod;
        private IMethod onSuccessMethod;
        private IMethod onExceptionMethod;
        private IMethod setExceptionMethod;
        private IMethod getReturnValueMethod;
        private IMethod getFlowBehaviorMethod;
        private IMethod setReturnValueMethod;
        private IMethod getInstanceMethod;
        private bool hasOutParameter;
        private ITypeSignature exceptionType;
        private MethodDefDeclaration targetMethodDef;
        private bool isStruct;

        private static readonly OnMethodBoundaryAspectConfigurationAttribute defaultConfiguration =
            new OnMethodBoundaryAspectConfigurationAttribute();


        public OnMethodBoundaryAspectWeaver() : base( defaultConfiguration )
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            ModuleDeclaration module = this.Task.Project.Module;

            this.onEntryMethod = module.Cache.GetItem( () => module.FindMethod(
                                                                 module.GetTypeForFrameworkVariant(
                                                                     typeof(IOnMethodBoundaryAspect) ),
                                                                 "OnEntry" ) );

            this.onExitMethod = module.Cache.GetItem( () => module.FindMethod(
                                                                module.GetTypeForFrameworkVariant(
                                                                    typeof(IOnMethodBoundaryAspect) ),
                                                                "OnExit" ) );

            this.onSuccessMethod = module.Cache.GetItem( () => module.FindMethod(
                                                                   module.GetTypeForFrameworkVariant(
                                                                       typeof(IOnMethodBoundaryAspect) ),
                                                                   "OnSuccess" ) );

            this.onExceptionMethod = module.Cache.GetItem( () => module.FindMethod(
                                                                     module.GetTypeForFrameworkVariant(
                                                                         typeof(IExceptionHandlerAspect) ),
                                                                     "OnException" ) );


            this.setExceptionMethod = module.Cache.GetItem( () => module.FindMethod(
                                                                      module.GetTypeForFrameworkVariant(
                                                                          typeof(MethodExecutionEventArgs) ),
                                                                      "set_Exception" ) );


            this.setReturnValueMethod = module.Cache.GetItem( () => module.FindMethod(
                                                                        module.GetTypeForFrameworkVariant(
                                                                            typeof(MethodExecutionEventArgs) ),
                                                                        "set_ReturnValue" ) );

            this.getReturnValueMethod = module.Cache.GetItem( () => module.FindMethod(
                                                                        module.GetTypeForFrameworkVariant(
                                                                            typeof(MethodExecutionEventArgs) ),
                                                                        "get_ReturnValue" ) );

            this.getFlowBehaviorMethod = module.Cache.GetItem( () => module.FindMethod(
                                                                         module.GetTypeForFrameworkVariant(
                                                                             typeof(MethodExecutionEventArgs) ),
                                                                         "get_FlowBehavior" ) );
        }


        protected internal override void OnTargetAssigned( bool reassigned )
        {
            // Determines whether the method has output parameters.
            this.targetMethodDef = (MethodDefDeclaration) this.TargetMethod;

            if ( !reassigned )
            {
                ModuleDeclaration module = this.Task.Project.Module;

                for ( int i = 0; i < this.targetMethodDef.Parameters.Count; i++ )
                {
                    ParameterDeclaration parameter = this.targetMethodDef.Parameters[i];

                    if ( parameter.ParameterType.BelongsToClassification( TypeClassifications.Pointer ) )
                    {
                        this.hasOutParameter = true;
                        break;
                    }
                }

                // Determines whether the declaring type is a struct.
                this.isStruct =
                    this.targetMethodDef.DeclaringType.BelongsToClassification( TypeClassifications.ValueType );

                if ( this.isStruct )
                {
                    this.getInstanceMethod = module.Cache.GetItem( () => module.FindMethod(
                                                                             module.GetTypeForFrameworkVariant(
                                                                                 typeof(InstanceBoundLaosEventArgs) ),
                                                                             "get_Instance" ) );
                }
            }
        }


        public override void Implement()
        {
            base.Implement();

            if ( !targetMethodDef.MayHaveBody )
            {
                return;
            }

            this.InitializeInstanceTag( this.targetMethodDef.DeclaringType, this.targetMethodDef.IsStatic );


            targetMethodDef.MethodBody.InitLocalVariables = true;

            // Get the exception type.

            TypeIdentity exceptionReflectionTypeIdentity =
                this.GetConfigurationObject<IOnMethodBoundaryAspectConfiguration, TypeIdentity>(
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

            this.Task.MethodLevelAdvices.Add( this );
        }

        public override bool ValidateSelf()
        {
            if ( !base.ValidateSelf() )
                return false;

            if ( !this.targetMethodDef.HasBody )
            {
                LaosMessageSource.Instance.Write( SeverityType.Error, "LA0010",
                                                  new object[]
                                                      {this.GetAspectTypeName(), this.targetMethodDef.ToString()} );

                return false;
            }
            else
            {
                return true;
            }
        }

        #region IMethodLevelAdvice Members

        public MethodDefDeclaration Method
        {
            get { return this.targetMethodDef; }
        }

        public MetadataDeclaration Operand
        {
            get { return null; }
        }

        public JoinPointKinds JoinPointKinds
        {
            get
            {
                return
                    JoinPointKinds.BeforeMethodBody | JoinPointKinds.AfterMethodBodyAlways |
                    JoinPointKinds.AfterMethodBodyException | JoinPointKinds.AfterMethodBodySuccess;
            }
        }

        #endregion

        #region IAdvice Members

        public int Priority
        {
            get { return this.AspectPriority; }
        }

        public bool RequiresWeave( WeavingContext context )
        {
            return true;
        }

        public void Weave( WeavingContext context, InstructionBlock block )
        {
            switch ( context.JoinPoint.JoinPointKind )
            {
                case JoinPointKinds.AfterInstanceInitialization:
                case JoinPointKinds.BeforeMethodBody:
                    this.WeaveOnEntry( context, block );
                    break;

                case JoinPointKinds.AfterMethodBodyAlways:
                    this.WeaveOnExitOrSuccess( context, block, this.onExitMethod );
                    break;

                case JoinPointKinds.AfterMethodBodySuccess:
                    this.WeaveOnExitOrSuccess( context, block, this.onSuccessMethod );
                    break;

                case JoinPointKinds.AfterMethodBodyException:
                    this.WeaveOnException( context, block );
                    break;

                default:
                    throw LaosExceptionHelper.Instance.CreateAssertionFailedException( "UnexpectedJoinPoint",
                                                                                       context.JoinPoint.JoinPointKind );
            }
        }


        private void WeaveOnEntry( WeavingContext context, InstructionBlock block )
        {
            ModuleDeclaration module = context.Method.Module;

            // Define local variables.
            MethodBodyDeclaration methodBody = this.targetMethodDef.MethodBody;
            InstructionBlock rootInstructionBlock = methodBody.RootInstructionBlock;


            this.exceptionLocal = rootInstructionBlock.DefineLocalVariable(
                module.Cache.GetType( "System.Exception, mscorlib" ), "~exception~{0}" );

            // Write the advice.
            InstructionWriter writer = context.InstructionWriter;
            InstructionSequence instructionSequence = block.MethodBody.CreateInstructionSequence();
            block.AddInstructionSequence( instructionSequence, NodePosition.Before, null );
            writer.AttachInstructionSequence( instructionSequence );

            writer.EmitSymbolSequencePoint( SymbolSequencePoint.Hidden );

            // Define the state variable and load it on the stack.

            this.Task.EventArgsBuilders.BuildMethodExecutionEventArgs( this.targetMethodDef, writer,
                                                                       this.TargetMethodRuntimeInstanceField,
                                                                       out this.eventArgsLocal, out argumentsLocal );

            // Load the instance tag.
            this.Task.InstanceTagManager.EmitLoadInstanceTag( this.eventArgsLocal, this.InstanceTagField, writer );

            #region Call OnEntry

            // Load the custom attribute instance on the stack.
            writer.EmitInstructionField( OpCodeNumber.Ldsfld, this.AspectRuntimeInstanceField );
            writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, this.eventArgsLocal );
            writer.EmitInstructionMethod( OpCodeNumber.Callvirt, onEntryMethod );

            #endregion

            // Store the instance (if we have a struct).
            if ( this.isStruct )
            {
                ITypeSignature declaringTypeSpec = GenericHelper.GetTypeCanonicalGenericInstance( this.Method.DeclaringType );
                writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, this.eventArgsLocal );
                writer.EmitInstructionMethod( OpCodeNumber.Call, this.getInstanceMethod );
                writer.EmitInstructionType( OpCodeNumber.Unbox, declaringTypeSpec );
                writer.EmitInstructionType( OpCodeNumber.Cpobj, declaringTypeSpec );
            }

            // Store the instance tag.
            this.Task.InstanceTagManager.EmitStoreInstanceTag( this.eventArgsLocal, this.InstanceTagField, writer );

            #region Process the flow control

            // Create the sequences.
            InstructionSequence returnSequence = methodBody.CreateInstructionSequence();
            block.AddInstructionSequence( returnSequence, NodePosition.After, null );

            InstructionSequence continueSequence = methodBody.CreateInstructionSequence();
            block.AddInstructionSequence( continueSequence, NodePosition.After, null );

            // Get the desired flow behavior.
            writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, this.eventArgsLocal );
            writer.EmitInstructionMethod( OpCodeNumber.Call, this.getFlowBehaviorMethod );

            // Conditionally branch.
            writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, (int) FlowBehavior.Return );
            writer.EmitBranchingInstruction( OpCodeNumber.Bne_Un, continueSequence );
            writer.DetachInstructionSequence();

            // Emit the return instruction.
            writer.AttachInstructionSequence( returnSequence );
            ITypeSignature returnType = this.targetMethodDef.ReturnParameter.ParameterType;
            if ( !IntrinsicTypeSignature.Is( returnType, IntrinsicType.Void ) )
            {
                writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, this.eventArgsLocal );
                writer.EmitInstructionMethod( OpCodeNumber.Call, this.getReturnValueMethod );
                context.WeavingHelper.FromObject( returnType, writer );
                writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc, context.ReturnValueVariable );
            }

            // Set output arguments in the context
            if ( this.hasOutParameter )
            {
                context.WeavingHelper.CopyArgumentsFromArray( this.argumentsLocal,
                                                              this.targetMethodDef, writer );
            }

            writer.EmitBranchingInstruction( OpCodeNumber.Leave, context.LeaveBranchTarget );
            writer.DetachInstructionSequence();

            #endregion
        }


        private void EmitLoadOutputArguments( WeavingContext context )
        {
            InstructionWriter writer = context.InstructionWriter;

            // Set into the array all arguments that have an input value.
            int shift = this.targetMethodDef.IsStatic ? 0 : 1;
            for ( int i = 0; i < this.targetMethodDef.Parameters.Count; i++ )
            {
                ParameterDeclaration parameter = this.targetMethodDef.Parameters[i];

                if ( parameter.ParameterType.BelongsToClassification( TypeClassifications.Pointer ) )
                {
                    writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, this.eventArgsLocal );
                    writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, this.argumentsLocal );
                    writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, i );
                    writer.EmitInstructionInt16( OpCodeNumber.Ldarg, (short) ( i + shift ) );
                    context.WeavingHelper.ToObject( parameter.ParameterType, writer );
                    writer.EmitInstruction( OpCodeNumber.Stelem_Ref );
                }
            }
        }


        private void WeaveOnException( WeavingContext context, InstructionBlock block )
        {
            InstructionWriter writer = context.InstructionWriter;
            MethodBodyDeclaration methodBody = block.MethodBody;
            InstructionSequence instructionSequence = methodBody.CreateInstructionSequence();
            block.AddInstructionSequence( instructionSequence, NodePosition.Before, null );
            writer.AttachInstructionSequence( instructionSequence );

            writer.EmitSymbolSequencePoint( SymbolSequencePoint.Hidden );

            // Save the exception
            writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc, exceptionLocal );

            // Set the exception.
            writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, eventArgsLocal );
            writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, exceptionLocal );
            writer.EmitInstructionMethod( OpCodeNumber.Call, this.setExceptionMethod );


            // Set output arguments in the context
            if ( this.hasOutParameter )
            {
                this.EmitLoadOutputArguments( context );
            }

            // Load the instance tag.
            this.Task.InstanceTagManager.EmitLoadInstanceTag( this.eventArgsLocal, this.InstanceTagField, writer );

            #region Call the OnException method

            // Load the custom attribute instance on the stack.
            writer.EmitInstructionField( OpCodeNumber.Ldsfld, this.AspectRuntimeInstanceField );

            // Load the state variable on the stack.
            writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, this.eventArgsLocal );
            // Call the method.
            writer.EmitInstructionMethod( OpCodeNumber.Callvirt, onExceptionMethod );

            #endregion

            // Store the instance tag.
            this.Task.InstanceTagManager.EmitStoreInstanceTag( eventArgsLocal, this.InstanceTagField, writer );


            // Write back the value of output parameters
            if ( this.hasOutParameter )
            {
                context.WeavingHelper.CopyArgumentsFromArray( this.argumentsLocal,
                                                              this.targetMethodDef, writer );
            }

            #region Process the flow control

            // Create the sequences.
            InstructionSequence rethrowSequence = methodBody.CreateInstructionSequence();
            block.AddInstructionSequence( rethrowSequence, NodePosition.After, null );

            InstructionSequence returnSequence = methodBody.CreateInstructionSequence();
            block.AddInstructionSequence( returnSequence, NodePosition.After, null );

            InstructionSequence continueSequence = methodBody.CreateInstructionSequence();
            block.AddInstructionSequence( continueSequence, NodePosition.After, null );

            // Get the desired flow behavior.
            writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, this.eventArgsLocal );
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

            // Emit the return instruction.
            writer.AttachInstructionSequence( returnSequence );
            ITypeSignature returnType = this.targetMethodDef.ReturnParameter.ParameterType;
            if ( !IntrinsicTypeSignature.Is( returnType, IntrinsicType.Void ) )
            {
                writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, this.eventArgsLocal );
                writer.EmitInstructionMethod( OpCodeNumber.Call, this.getReturnValueMethod );
                context.WeavingHelper.FromObject( returnType, writer );
                writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc, context.ReturnValueVariable );
            }
            writer.EmitBranchingInstruction( OpCodeNumber.Leave, context.LeaveBranchTarget );
            writer.DetachInstructionSequence();

            #endregion
        }

        private void WeaveOnExitOrSuccess( WeavingContext context, InstructionBlock block, IMethod onExitOrSuccessMethod )
        {
            InstructionWriter writer = context.InstructionWriter;
            MethodBodyDeclaration methodBody = block.MethodBody;

            InstructionSequence instructionSequence = methodBody.CreateInstructionSequence();
            block.AddInstructionSequence( instructionSequence, NodePosition.Before, null );
            writer.AttachInstructionSequence( instructionSequence );

            writer.EmitSymbolSequencePoint( SymbolSequencePoint.Hidden );


            // Set output arguments in the context
            if ( this.hasOutParameter )
            {
                this.EmitLoadOutputArguments( context );
            }

            // We do not set the exception in the context, because it has
            // been done by the exception handler.

            #region Set the return value in the context

            // Load the return value, if any.
            ITypeSignature returnType = this.targetMethodDef.ReturnParameter.ParameterType;
            if ( !IntrinsicTypeSignature.Is( returnType, IntrinsicType.Void ) )
            {
                writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, this.eventArgsLocal );
                writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, context.ReturnValueVariable );
                context.WeavingHelper.ToObject( returnType, writer );
                writer.EmitInstructionMethod( OpCodeNumber.Call,
                                              this.setReturnValueMethod );
            }

            #endregion

            // Load the instance tag.
            this.Task.InstanceTagManager.EmitLoadInstanceTag( this.eventArgsLocal, this.InstanceTagField, writer );

            #region Call the OnExit method

            // Load the custom attribute instance on the stack.
            writer.EmitInstructionField( OpCodeNumber.Ldsfld, this.AspectRuntimeInstanceField );

            // Load the state variable on the stack.
            writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, this.eventArgsLocal );
            // Call the method.
            writer.EmitInstructionMethod( OpCodeNumber.Callvirt, onExitOrSuccessMethod );

            #endregion

            // Store the instance tag.
            this.Task.InstanceTagManager.EmitStoreInstanceTag( eventArgsLocal, this.InstanceTagField, writer );

            // Write back the value of output parameters
            if ( this.hasOutParameter )
            {
                context.WeavingHelper.CopyArgumentsFromArray( this.argumentsLocal,
                                                              this.targetMethodDef, writer );
            }

            #region Write back the return value

            if ( !IntrinsicTypeSignature.Is( returnType, IntrinsicType.Void ) )
            {
                writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, this.eventArgsLocal );
                writer.EmitInstructionMethod( OpCodeNumber.Call, this.getReturnValueMethod );
                context.WeavingHelper.FromObject( returnType, writer );
                writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc,
                                                     context.ReturnValueVariable );
            }
            writer.DetachInstructionSequence();

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