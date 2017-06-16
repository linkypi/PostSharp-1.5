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
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.Extensibility.Tasks;
using PostSharp.ModuleWriter;

namespace PostSharp.Laos.Weaver
{
    internal class OnFieldAccessAspectWeaver : FieldLevelAspectWeaver, IFieldLevelAdvice
    {
        private IMethod getStoredFieldValueMethod;
        private IMethod getExposedFieldValueMethod;
        private IMethod onSetValueMethod;
        private IMethod onGetValueMethod;
        private OnFieldAccessAspectOptions options;
        private bool isStatic;
        private bool isValueType;
        private FieldDefDeclaration targetFieldDef;
        private bool generateProperty;

        private static readonly OnFieldAccessAspectConfigurationAttribute defaultConfiguration =
            new OnFieldAccessAspectConfigurationAttribute();

        public OnFieldAccessAspectWeaver() : base( defaultConfiguration )
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            ModuleDeclaration module = this.Task.Project.Module;

            this.getStoredFieldValueMethod = this.Task.Project.Module.Cache
                .GetItem( () => module.FindMethod(
                                    module.GetTypeForFrameworkVariant( typeof(FieldAccessEventArgs) ),
                                    "get_StoredFieldValue" ) );

            this.getExposedFieldValueMethod = this.Task.Project.Module.Cache
                .GetItem( () => module.FindMethod(
                                    module.GetTypeForFrameworkVariant( typeof(FieldAccessEventArgs) ),
                                    "get_ExposedFieldValue" ) );

            this.onSetValueMethod = this.Task.Project.Module.Cache
                .GetItem( () => module.FindMethod(
                                    module.GetTypeForFrameworkVariant( typeof(IOnFieldAccessAspect) ),
                                    "OnSetValue" ) );

            this.onGetValueMethod = this.Task.Project.Module.Cache
                .GetItem( () => module.FindMethod(
                                    module.GetTypeForFrameworkVariant( typeof(IOnFieldAccessAspect) ),
                                    "OnGetValue" ) );


            this.options =
                this.GetConfigurationValue<IOnFieldAccessAspectConfiguration, OnFieldAccessAspectOptions>(
                    c => c.GetOptions() );
        }


        protected internal override void OnTargetAssigned( bool reassigned )
        {
            this.targetFieldDef = (FieldDefDeclaration) this.TargetField;

            if ( !reassigned )
            {
                this.isStatic = ( this.targetFieldDef.Attributes & FieldAttributes.Static ) != 0;
                this.isValueType =
                    this.targetFieldDef.FieldType.BelongsToClassification( TypeClassifications.ReferenceType )
                        ? false
                        : true;
            }
        }

        public override bool ValidateSelf()
        {
            FieldDefDeclaration fieldDef = (FieldDefDeclaration) this.TargetField;

            // Check that the field is not a literal.
            if ( ( fieldDef.Attributes & FieldAttributes.Literal ) != 0 )
            {
                LaosMessageSource.Instance.Write( SeverityType.Error, "LA0017",
                                                  new object[] {this.GetAspectTypeName(), fieldDef.ToString()} );
                return false;
            }

            // Infer property generation.
            switch ( options & OnFieldAccessAspectOptions.GeneratePropertyMask )
            {
                case OnFieldAccessAspectOptions.GeneratePropertyNever:
                    // Refuse to weave a public field without wrapping it into a property.
                    if ( VisibilityHelper.IsPublic( fieldDef ) )
                    {
                        LaosMessageSource.Instance.Write( SeverityType.Error, "LA0011",
                                                          new object[]
                                                              {
                                                                  this.GetAspectTypeName(),
                                                                  fieldDef.ToString()
                                                              } );
                        return false;
                    }
                    break;

                case OnFieldAccessAspectOptions.GeneratePropertyAlways:
                    this.generateProperty = true;
                    break;

                case OnFieldAccessAspectOptions.GeneratePropertyAuto:
                    this.generateProperty = VisibilityHelper.IsPublic( fieldDef );
                    break;

                default:
                    ExceptionHelper.CreateInvalidEnumerationValueException( options, "options" );
                    break;
            }


            return base.ValidateSelf();
        }

        public override void Implement()
        {
//            base.Implement();

            IOnFieldAccessAspect attributeCompileTimeInstance = (IOnFieldAccessAspect) this.Aspect;

            bool removeField = ( this.options & OnFieldAccessAspectOptions.RemoveFieldStorage ) != 0;


            this.Task.FieldLevelAdvices.Add( this );

            if ( removeField )
            {
                RemoveTask.GetTask( this.Task.Project ).MarkForRemoval( this.targetFieldDef );
            }


            if ( attributeCompileTimeInstance == null ) return;

            // Get the instance tag field.
            this.InitializeInstanceTag( this.targetFieldDef.DeclaringType, this.targetFieldDef.IsStatic );
        }

        public override void EmitRuntimeInitialization( InstructionEmitter writer )
        {
            this.EmitRuntimeInitialization( writer,
                                                ( this.options & OnFieldAccessAspectOptions.RemoveFieldStorage ) != 0 );
        }

        #region IFieldLevelAdvice Members

        IField IFieldLevelAdvice.Field
        {
            get { return this.targetFieldDef; }
        }


        JoinPointKinds IFieldLevelAdvice.JoinPointKinds
        {
            get
            {
                return
                    JoinPointKinds.InsteadOfGetField | JoinPointKinds.InsteadOfSetField |
                    JoinPointKinds.InsteadOfGetFieldAddress;
            }
        }

        public bool ChangeToProperty
        {
            get { return this.generateProperty; }
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

        private void WeaveGetField( WeavingContext context, InstructionBlock block )
        {
            InstructionWriter instructionWriter = context.InstructionWriter;

            InstructionSequence instructionSequence = block.MethodBody.CreateInstructionSequence();
            block.AddInstructionSequence( instructionSequence, NodePosition.Before, null );
            instructionWriter.AttachInstructionSequence( instructionSequence );


            LocalVariableSymbol getEventArgsLocal;

            // Put the field value twice on the stack.
            instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, context.FieldValueLocal );
            this.Task.WeavingHelper.ToObject( this.targetFieldDef.FieldType, instructionWriter );
            instructionWriter.EmitInstruction( OpCodeNumber.Dup );

            // Build the EventArgs object.
            this.Task.EventArgsBuilders.BuildFieldAccessEventArgs( this.targetFieldDef, instructionWriter,
                                                                   context.FieldValueLocal,
                                                                   this.TargetFieldRuntimeInstanceField,
                                                                   out getEventArgsLocal );

            // Load the instance tag.
            this.Task.InstanceTagManager.EmitLoadInstanceTag( getEventArgsLocal, this.InstanceTagField,
                                                              instructionWriter );


            // Call the OnGet method
            instructionWriter.EmitInstructionField( OpCodeNumber.Ldsfld, this.AspectRuntimeInstanceField );
            instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, getEventArgsLocal );
            instructionWriter.EmitInstructionMethod( OpCodeNumber.Callvirt, this.onGetValueMethod );

            // Store the instance tag.
            this.Task.InstanceTagManager.EmitStoreInstanceTag( getEventArgsLocal, this.InstanceTagField,
                                                               instructionWriter );

            // Return the field value.
            instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, getEventArgsLocal );
            instructionWriter.EmitInstructionMethod( OpCodeNumber.Call, this.getExposedFieldValueMethod );
            InstructionSequence nullInstructionSequence, continueInstructionSequence;
            if ( isValueType )
            {
                // Transform null --> 0
                nullInstructionSequence = block.MethodBody.CreateInstructionSequence();
                continueInstructionSequence = block.MethodBody.CreateInstructionSequence();
                block.AddInstructionSequence( nullInstructionSequence, NodePosition.After, null );
                block.AddInstructionSequence( continueInstructionSequence, NodePosition.After, null );
                instructionWriter.EmitInstruction( OpCodeNumber.Dup );
                instructionWriter.EmitBranchingInstruction( OpCodeNumber.Brfalse, nullInstructionSequence );
            }
            else
            {
                nullInstructionSequence = null;
                continueInstructionSequence = null;
            }

            this.Task.WeavingHelper.FromObject( this.targetFieldDef.FieldType, instructionWriter );
            instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Stloc, context.FieldValueLocal );

            if ( isValueType )
            {
                instructionWriter.EmitBranchingInstruction( OpCodeNumber.Br, continueInstructionSequence );
            }
            instructionWriter.DetachInstructionSequence();

            if ( isValueType )
            {
                LocalVariableSymbol emptyValueLocal = block.MethodBody.RootInstructionBlock.DefineLocalVariable(
                    this.targetFieldDef.FieldType, "~emptyValue~{0}" );
                instructionWriter.AttachInstructionSequence( nullInstructionSequence );
                instructionWriter.EmitInstruction( OpCodeNumber.Pop );
                instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloca, emptyValueLocal );
                instructionWriter.EmitInstructionType( OpCodeNumber.Initobj, this.targetFieldDef.FieldType );
                instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, emptyValueLocal );
                instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Stloc, context.FieldValueLocal );
                instructionWriter.DetachInstructionSequence();
            }
        }

        private void WeaveSetField( WeavingContext context, InstructionBlock block )
        {
            InstructionWriter instructionWriter = context.InstructionWriter;

            InstructionSequence instructionSequence = block.MethodBody.CreateInstructionSequence();
            block.AddInstructionSequence( instructionSequence, NodePosition.Before, null );
            instructionWriter.AttachInstructionSequence( instructionSequence );


            LocalVariableSymbol setEventArgsLocal;

            // Put the new value on the stack.
            instructionWriter.EmitInstruction( isStatic ? OpCodeNumber.Ldarg_0 : OpCodeNumber.Ldarg_1 );
            this.Task.WeavingHelper.ToObject( this.targetFieldDef.FieldType, instructionWriter );

            // Put the old value on the stack.
            instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, context.FieldValueLocal );
            this.Task.WeavingHelper.ToObject( this.targetFieldDef.FieldType, instructionWriter );

            // Build the EventArgs object.
            this.Task.EventArgsBuilders.BuildFieldAccessEventArgs( this.targetFieldDef, instructionWriter,
                                                                   context.FieldValueLocal,
                                                                   this.TargetFieldRuntimeInstanceField,
                                                                   out setEventArgsLocal );


            // Load the instance tag.
            this.Task.InstanceTagManager.EmitLoadInstanceTag( setEventArgsLocal, this.InstanceTagField,
                                                              instructionWriter );

            // Call the OnSet method.
            instructionWriter.EmitInstructionField( OpCodeNumber.Ldsfld, this.AspectRuntimeInstanceField );
            instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, setEventArgsLocal );
            instructionWriter.EmitInstructionMethod( OpCodeNumber.Callvirt, onSetValueMethod );

            // store the instance tag.
            this.Task.InstanceTagManager.EmitStoreInstanceTag( setEventArgsLocal, this.InstanceTagField,
                                                               instructionWriter );

            // Store the value.
            instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, setEventArgsLocal );
            instructionWriter.EmitInstructionMethod( OpCodeNumber.Call, this.getStoredFieldValueMethod );
            this.Task.WeavingHelper.FromObject( this.targetFieldDef.FieldType, instructionWriter );
            instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Stloc, context.FieldValueLocal );

            instructionWriter.DetachInstructionSequence();
        }

        void IAdvice.Weave( WeavingContext context, InstructionBlock block )
        {
            switch ( context.JoinPoint.JoinPointKind )
            {
                case JoinPointKinds.InsteadOfGetField:
                    this.WeaveGetField( context, block );
                    break;

                case JoinPointKinds.InsteadOfSetField:
                    this.WeaveSetField( context, block );
                    break;

                default:
                    throw LaosExceptionHelper.Instance.CreateAssertionFailedException( "UnexpectedJoinPoint",
                                                                                       context.JoinPoint.JoinPointKind );
            }
        }

        #endregion
    }
}