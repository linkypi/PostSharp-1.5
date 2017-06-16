#region Released to Public Domain by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of samples of PostSharp.                                *
 *                                                                             *
 *   This sample is free software: you have an unlimited right to              *
 *   redistribute it and/or modify it.                                         *
 *                                                                             *
 *   This sample is distributed in the hope that it will be useful,            *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.                      *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

using System;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.Collections;
using PostSharp.Laos;
using PostSharp.Laos.Weaver;
using PostSharp.Samples.Librarian.Framework;

namespace PostSharp.Samples.Librarian.Weaver
{
    /// <summary>
    /// Generates code (specifically the <see cref="Aspectable.CopyTo"/> method) for the 'Cloneable' aspect.
    /// </summary>
    internal class ImplementCloneableAspectWeaver : TypeLevelAspectWeaver
    {
        public ImplementCloneableAspectWeaver( ) : base( null )
        {
        }

        public override void Implement()
        {
            TypeDefDeclaration typeDef = (TypeDefDeclaration) this.TargetType;
            ModuleDeclaration module = this.Task.Project.Module;

            // Declare the method.
            MethodDefDeclaration methodDef = new MethodDefDeclaration
                                                 {
                                                     Name = "CopyTo",
                                                     Attributes =
                                                         (MethodAttributes.Family | MethodAttributes.ReuseSlot |
                                                          MethodAttributes.Virtual),
                                                     CallingConvention = CallingConvention.HasThis
                                                 };
            typeDef.Methods.Add( methodDef );
            methodDef.CustomAttributes.Add( this.Task.WeavingHelper.GetDebuggerNonUserCodeAttribute() );

            // Define parameter.
            methodDef.ReturnParameter = new ParameterDeclaration
                                            {
                                                ParameterType = module.Cache.GetIntrinsic( IntrinsicType.Void ),
                                                Attributes = ParameterAttributes.Retval
                                            };

            ParameterDeclaration cloneParameter =
                new ParameterDeclaration( 0, "clone", this.Task.Project.Module.Cache.GetType( typeof(Aspectable) ) );
            methodDef.Parameters.Add( cloneParameter );

            // Define the body
            MethodBodyDeclaration methodBody = new MethodBodyDeclaration();
            methodDef.MethodBody = methodBody;
            InstructionBlock instructionBlock = methodBody.CreateInstructionBlock();
            methodBody.RootInstructionBlock = instructionBlock;
            InstructionSequence sequence = methodBody.CreateInstructionSequence();
            instructionBlock.AddInstructionSequence( sequence, NodePosition.After, null );
            InstructionWriter writer = this.Task.InstructionWriter;
            writer.AttachInstructionSequence( sequence );

            // Cast the argument and store it in a local variable.
            IType typeSpec = GenericHelper.GetTypeCanonicalGenericInstance( typeDef );
            LocalVariableSymbol castedCloneLocal = instructionBlock.DefineLocalVariable( typeSpec, "typedClone" );
            writer.EmitInstruction( OpCodeNumber.Ldarg_1 );
            writer.EmitInstructionType( OpCodeNumber.Castclass, typeSpec );
            writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc, castedCloneLocal );

            // Find the base method.
            IMethod baseCopyToMethod = null;
            IType baseTypeCursor = typeDef.BaseType;
            MethodSignature methodSignature =
                new MethodSignature( module, CallingConvention.HasThis, module.Cache.GetIntrinsic( IntrinsicType.Void ),
                                     new [] {cloneParameter.ParameterType}, 0 );

            while ( baseCopyToMethod == null )
            {
                TypeDefDeclaration baseTypeCursorTypeDef = baseTypeCursor.GetTypeDefinition();

                baseCopyToMethod =
                    baseTypeCursorTypeDef.Methods.GetMethod( "CopyTo",
                                                             methodSignature.Translate( baseTypeCursorTypeDef.Module ),
                                                             BindingOptions.OnlyExisting |
                                                             BindingOptions.DontThrowException );

                baseTypeCursor = baseTypeCursorTypeDef.BaseType;
            }

            // TODO: support generic base types.


            // Call the base method.
            writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
            writer.EmitInstruction( OpCodeNumber.Ldarg_1 );
            writer.EmitInstructionMethod( OpCodeNumber.Call, (IMethod) baseCopyToMethod.Translate( typeDef.Module ) );

            // Loop on all fields and clone cloneable ones.
            TypeRefDeclaration cloneableTypeRef = (TypeRefDeclaration)
                                                  this.Task.Project.Module.Cache.GetType( typeof(ICloneable) );
            MethodRefDeclaration cloneMethodRef = (MethodRefDeclaration) cloneableTypeRef.MethodRefs.GetMethod(
                                                                             "Clone",
                                                                             new MethodSignature(
                                                                                module,
                                                                                 CallingConvention.HasThis,
                                                                                 module.Cache.GetIntrinsic(
                                                                                     IntrinsicType.Object ),
                                                                                 new ITypeSignature[0], 0 ),
                                                                             BindingOptions.Default );

            foreach ( FieldDefDeclaration fieldDef in typeDef.Fields )
            {
                if ( ( fieldDef.Attributes & FieldAttributes.Static ) != 0 )
                    continue;

                if ( fieldDef.FieldType == module.Cache.GetIntrinsic( IntrinsicType.String ) )
                    continue;

                // Does not work?
                //bool cloneable = fieldDef.FieldType.Inherits(cloneableTypeRef, GenericMap.Empty);
                bool cloneable = typeof(ICloneable).IsAssignableFrom( fieldDef.FieldType.GetSystemType( null, null ) );


                if ( cloneable )
                {
                    IField fieldSpec = GenericHelper.GetFieldCanonicalGenericInstance( fieldDef );
                    bool isValueType =
                        fieldSpec.FieldType.BelongsToClassification( TypeClassifications.ValueType ).Equals(
                            NullableBool.True );

                    InstructionSequence nextSequence = null;
                    if ( !isValueType )
                    {
                        nextSequence = methodBody.CreateInstructionSequence();
                        writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                        writer.EmitInstructionField( OpCodeNumber.Ldfld, fieldSpec );
                        writer.EmitBranchingInstruction( OpCodeNumber.Brfalse, nextSequence );
                    }
                    writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, castedCloneLocal );
                    writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                    writer.EmitInstructionField( OpCodeNumber.Ldfld, fieldSpec );
                    if ( isValueType )
                    {
                        writer.EmitInstructionType( OpCodeNumber.Box, fieldSpec.FieldType );
                    }
                    //writer.EmitInstructionType(OpCodeNumber.Castclass, cloneableTypeRef);
                    writer.EmitInstructionMethod( OpCodeNumber.Callvirt, cloneMethodRef );
                    if ( isValueType )
                    {
                        writer.EmitInstructionType( OpCodeNumber.Unbox, fieldSpec.FieldType );
                        writer.EmitInstructionType( OpCodeNumber.Ldobj, fieldSpec.FieldType );
                    }
                    else
                    {
                        writer.EmitInstructionType( OpCodeNumber.Castclass, fieldSpec.FieldType );
                    }
                    writer.EmitInstructionField( OpCodeNumber.Stfld, fieldSpec );


                    if ( !isValueType )
                    {
                        writer.DetachInstructionSequence();
                        instructionBlock.AddInstructionSequence( nextSequence, NodePosition.After, sequence );
                        sequence = nextSequence;
                        writer.AttachInstructionSequence( sequence );
                    }
                }
            }

            writer.EmitInstruction( OpCodeNumber.Ret );
            writer.DetachInstructionSequence();
        }
    }
}