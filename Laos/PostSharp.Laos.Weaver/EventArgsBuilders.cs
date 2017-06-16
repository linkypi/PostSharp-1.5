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
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Extensibility.Tasks;
using PostSharp.ModuleWriter;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Build event argument objects used in Laos.
    /// </summary>
    public class EventArgsBuilders
    {
        private readonly LaosTask task;
        private readonly MethodRefDeclaration methodExecutionEventArgsConstructor;
        private readonly MethodRefDeclaration fieldAccessEventArgsConstructor;
        private readonly IMethod setInstanceCredentialsMethod;
        private readonly ITypeSignature fieldAccessEventArgsType;


        internal EventArgsBuilders( LaosTask task )
        {
            this.task = task;

            ModuleDeclaration module = this.task.Project.Module;

            this.methodExecutionEventArgsConstructor =
                module.Cache.GetItem(
                    () => (MethodRefDeclaration) module.FindMethod(
                                                     module.
                                                         GetTypeForFrameworkVariant
                                                         ( typeof(
                                                               MethodExecutionEventArgs
                                                               ) ),
                                                     ".ctor" ) );

            this.fieldAccessEventArgsConstructor = module.Cache.GetItem(
                () => (MethodRefDeclaration) module.FindMethod(
                                                 module.GetTypeForFrameworkVariant( typeof(FieldAccessEventArgs) ),
                                                 ".ctor" ) );

            this.setInstanceCredentialsMethod =
                module.Cache.GetItem( () => module.FindMethod(
                                                module.GetTypeForFrameworkVariant( typeof(InstanceBoundLaosEventArgs) ),
                                                "set_InstanceCredentials" ) );

            this.fieldAccessEventArgsType = module.GetTypeForFrameworkVariant( typeof(FieldAccessEventArgs) );
        }

        /// <summary>
        /// Emit instructions that build a <see cref="MethodExecutionEventArgs"/> instance.
        /// </summary>
        /// <param name="method">Executed method.</param>
        /// <param name="writer">Writer where instructions are written.</param>
        /// <param name="arrayOfArgumentsLocal">An output parameter filled with
        /// a local variable containing the array of local arguments.</param>
        /// <param name="eventArgsLocal">An output parameter filled with a
        /// local variable containing the built event arguments.</param>
        /// <param name="methodInfoFieldDef">Field containing the <see cref="MethodBase"/> at runtime.</param>
        /// <remarks>
        /// The exposed value should be on the stack.
        /// </remarks>
        public void BuildMethodExecutionEventArgs( MethodDefDeclaration method, InstructionEmitter writer,
                                                   FieldDefDeclaration methodInfoFieldDef,
                                                   out LocalVariableSymbol eventArgsLocal,
                                                   out LocalVariableSymbol arrayOfArgumentsLocal )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( method, "method" );
            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );
            LaosExceptionHelper.Instance.AssertValidOperation(
                writer.MethodBody.Method == method, "WriterNotInMethodBody" );

            #endregion

            InstructionBlock rootInstructionBlock = method.MethodBody.RootInstructionBlock;
            ModuleDeclaration module = method.Module;

            // Load the method instance on the stack.
            IMethod methodInstance = GenericHelper.GetMethodCanonicalGenericInstance( method );
            IType typeInstance = methodInstance.DeclaringType;

            if ( methodInstance.IsGenericInstance || typeInstance.IsGenericInstance || methodInfoFieldDef == null )
            {
                writer.EmitInstructionMethod( OpCodeNumber.Ldtoken, methodInstance );
                writer.EmitInstructionType( OpCodeNumber.Ldtoken, typeInstance );
                writer.EmitInstructionMethod( OpCodeNumber.Call,
                                              module.Cache.GetItem( module.Cache.MethodBaseGetMethodFromHandle2 ) );
            }
            else
            {
                writer.EmitInstructionField( OpCodeNumber.Ldsfld, methodInfoFieldDef );
            }

            // Load the instance on the stack.
            if ( method.IsStatic )
            {
                writer.EmitInstruction( OpCodeNumber.Ldnull );
            }
            else
            {
                writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                if ( method.DeclaringType.BelongsToClassification( TypeClassifications.ValueType ) )
                {
                    ITypeSignature declaringTypeSpec = GenericHelper.GetTypeCanonicalGenericInstance( method.DeclaringType );
                    writer.EmitInstructionType(OpCodeNumber.Ldobj, declaringTypeSpec);
                    writer.EmitInstructionType(OpCodeNumber.Box, declaringTypeSpec);
                }
            }

            // Create the array of parameters and load it on the stack. 
            if ( method.Parameters.Count > 0 )
            {
                arrayOfArgumentsLocal = rootInstructionBlock.DefineLocalVariable(
                    new ArrayTypeSignature( module.Cache.GetIntrinsic( IntrinsicType.Object ) ),
                    "~arguments~{0}" );
                task.WeavingHelper.MakeArrayOfArguments( method, writer );
                writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc, arrayOfArgumentsLocal );
                writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, arrayOfArgumentsLocal );
            }
            else
            {
                arrayOfArgumentsLocal = null;
                writer.EmitInstruction( OpCodeNumber.Ldnull );
            }

            // Call the constructor
            // public MethodExecutionEventArgs( MethodBase method, object instance, object[] arguments )
            writer.EmitInstructionMethod( OpCodeNumber.Newobj, this.methodExecutionEventArgsConstructor );

            // Store the event arguments in a local variable.
            eventArgsLocal = rootInstructionBlock.DefineLocalVariable(
                module.GetTypeForFrameworkVariant( typeof(MethodExecutionEventArgs) ), "~laosEventArgs~{0}" );
            writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc, eventArgsLocal );

            // Set the credentials.
            if ( !method.IsStatic )
            {
                IMethod getInstanceCredentialsMethod =
                    this.task.InstanceCredentialsManager.GetGetInstanceCredentialsMethod( method.DeclaringType );
                if ( getInstanceCredentialsMethod != null )
                {
                    writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, eventArgsLocal );
                    writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                    writer.EmitInstructionMethod( OpCodeNumber.Call, getInstanceCredentialsMethod );
                    writer.EmitInstructionMethod( OpCodeNumber.Call, this.setInstanceCredentialsMethod );
                }
            }
        }

        /// <summary>
        /// Emit instructions that build a <see cref="FieldAccessEventArgs"/> instance.
        /// </summary>
        /// <param name="field">Accessed field, or <b>null</b> if the field value and metadata don't have to be loaded.</param>
        /// <param name="fieldValueLocal">Local variable in which the field value is stored.</param>
        /// <param name="eventArgsLocal">An output parameter filled with a
        /// local variable containing the built event arguments.</param>
        /// <param name="writer">Writer where instructions are written.</param>
        /// <param name="fieldRuntimeInstanceField">Field containing the <see cref="FieldInfo"/> at runtime.</param>
        /// <remarks>
        /// Expected on the stack: exposed value, stored value.
        /// </remarks>
        public void BuildFieldAccessEventArgs( FieldDefDeclaration field,
                                               InstructionWriter writer,
                                               LocalVariableSymbol fieldValueLocal,
                                               FieldDefDeclaration fieldRuntimeInstanceField,
                                               out LocalVariableSymbol eventArgsLocal )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );
            ExceptionHelper.AssertArgumentNotNull( field, "field" );
            ExceptionHelper.AssertArgumentNotNull( fieldValueLocal, "fieldValueLocal" );

            // BUG-1.0: don't use task.InstructionWriter
            LaosExceptionHelper.Instance.AssertValidOperation(
                writer.MethodBody.Method.DeclaringType == field.DeclaringType,
                "WriterNotInDeclaringType" );

            #endregion

            bool removeField = RemoveTask.IsMarkedForRemoval( field );
            IField fieldInstance = GenericHelper.GetFieldCanonicalGenericInstance( field );

            // We have to call: FieldAccessEventArgs(object exposedFieldValue, object storedFieldValue, 
            // FieldInfo fieldInfo, object instance )

            ModuleDeclaration module = this.task.Project.Module;


            if ( !removeField )
            {
                if ( field.DeclaringType.IsGenericDefinition )
                {
                    // Get the FieldInfo
                    writer.EmitInstructionField( OpCodeNumber.Ldtoken, fieldInstance );

                    // call GetFieldFromHandle(RuntimeFieldHandle,RuntimeTypeHandle)
                    writer.EmitInstructionType( OpCodeNumber.Ldtoken, fieldInstance.DeclaringType );
                    writer.EmitInstructionMethod( OpCodeNumber.Call,
                                                  module.Cache.GetItem(
                                                      module.Cache.FieldInfoGetFieldFromHandle2 ) );
                }
                else if ( fieldRuntimeInstanceField == null )
                {
                    // Get the FieldInfo
                    writer.EmitInstructionField( OpCodeNumber.Ldtoken, fieldInstance );

                    writer.EmitInstructionMethod( OpCodeNumber.Call,
                                                  module.Cache.GetItem(
                                                      module.Cache.FieldInfoGetFieldFromHandle ) );
                }
                else
                {
                    writer.EmitInstructionField( OpCodeNumber.Ldsfld, fieldRuntimeInstanceField );
                }
            }
            else
            {
                writer.EmitInstruction( OpCodeNumber.Ldnull );
            }

            // Load the current instance on the stack.
            if ( ( field.Attributes & FieldAttributes.Static ) != 0 )
            {
                writer.EmitInstruction( OpCodeNumber.Ldnull );
            }
            else
            {
                writer.EmitInstruction( OpCodeNumber.Ldarg_0 );

                if ( field.DeclaringType.BelongsToClassification( TypeClassifications.ValueType ) )
                {
                    writer.EmitInstructionType( OpCodeNumber.Ldobj, field.DeclaringType );
                    writer.EmitInstructionType( OpCodeNumber.Box, field.DeclaringType );
                }
            }


            // Finally call the constructor.
            writer.EmitInstructionMethod( OpCodeNumber.Newobj, this.fieldAccessEventArgsConstructor );

            // Store the event arguments in a local variable.
            eventArgsLocal = writer.CurrentInstructionSequence.ParentInstructionBlock.DefineLocalVariable(
                this.fieldAccessEventArgsType, "~laosEventArgs~{0}" );
            writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc, eventArgsLocal );

            // Set the credentials.
            if ( ( field.Attributes & FieldAttributes.Static ) == 0 )
            {
                IMethod getInstanceCredentialsMethod =
                    this.task.InstanceCredentialsManager.GetGetInstanceCredentialsMethod( field.DeclaringType );
                if ( getInstanceCredentialsMethod != null )
                {
                    writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, eventArgsLocal );
                    writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                    writer.EmitInstructionMethod( OpCodeNumber.Call, getInstanceCredentialsMethod );
                    writer.EmitInstructionMethod( OpCodeNumber.Call, this.setInstanceCredentialsMethod );
                }
            }
        }
    }
}