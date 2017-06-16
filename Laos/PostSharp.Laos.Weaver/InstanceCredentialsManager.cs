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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.PlatformAbstraction;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Manages the creation of the <b>GetInstanceCredentials</b> method.
    /// </summary>
    public class InstanceCredentialsManager : IDisposable
    {
        private readonly LaosTask task;
        private readonly ModuleDeclaration module;
        private readonly InstructionWriter writer = new InstructionWriter();
        private readonly IType instanceCredentialsType;
        private readonly MethodSignature getInstanceCredentialsMethodSignature;
        private readonly IMethod makeNewInstanceCredentialsMethod;

        internal InstanceCredentialsManager( LaosTask task )
        {
            this.task = task;
            this.module = task.Project.Module;

            this.instanceCredentialsType = (IType) this.module.GetTypeForFrameworkVariant( typeof(InstanceCredentials) );

            this.getInstanceCredentialsMethodSignature = new MethodSignature( this.module, CallingConvention.HasThis,
                                                                              this.instanceCredentialsType, null, 0 );

            this.makeNewInstanceCredentialsMethod =
                this.module.Cache.GetItem( () => module.FindMethod(
                                                     module.GetTypeForFrameworkVariant(
                                                         typeof(InstanceCredentials) ),
                                                     "MakeNew" ) );
        }

        /// <summary>
        /// Specifies that the <b>GetInstanceCredentials</b> method will be requested
        /// on a type.
        /// </summary>
        /// <param name="type">The type for which the <b>GetInstanceCredentials</b> will
        /// be requested.</param>
        /// <remarks>
        /// This method is typically called during the initialization phase.
        /// </remarks>
        [SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters" )]
        public void RequestGetInstanceCredentialsMethod( TypeDefDeclaration type )
        {
            IntendManager.AddIntend( type, "GetInstanceCredentials" );
        }

        /// <summary>
        /// Gets the <b>GetInstanceCredentials</b> method for a given type
        /// for the identity <see cref="GenericMap"/>.
        /// </summary>
        /// <param name="type">The type for which the <b>GetInstanceCredentials</b> method
        /// is requested.</param>
        /// <returns>A <see cref="IMethod"/> representing the <b>GetInstanceCredentials</b>
        /// method, either on <paramref name="type"/> either on one of its base types.</returns>
        /// <remarks>
        /// This method is typically called during the implementation phase.
        /// </remarks>
        public IMethod GetGetInstanceCredentialsMethod( TypeDefDeclaration type )
        {
            GenericMap genericMap = type.Module.Cache.IdentityGenericMap;
            return this.GetGetInstanceCredentialsMethod( type, genericMap );
        }

        /// <summary>
        /// Gets the <b>GetInstanceCredentials</b> method for a given type and a <see cref="GenericMap"/>.
        /// </summary>
        /// <param name="type">The type for which the <b>GetInstanceCredentials</b> method
        /// is requested.</param>
        /// <param name="genericMap">Initial <see cref="GenericMap"/>.</param>
        /// <returns>A <see cref="IMethod"/> representing the <b>GetInstanceCredentials</b>
        /// method, either on <paramref name="type"/> either on one of its base types.</returns>
        /// <remarks>
        /// This method is typically called during the implementation phase.
        /// </remarks>
        public IMethod GetGetInstanceCredentialsMethod( TypeDefDeclaration type, GenericMap genericMap )
        {
            Stack<Pair<TypeDefDeclaration, GenericMap>> parentTypes =
                new Stack<Pair<TypeDefDeclaration, GenericMap>>( 4 );

            // Get the stack of base types.
            IType cursor = type;
            while ( cursor != null )
            {
                TypeDefDeclaration cursorTypeDef = cursor.GetTypeDefinition();
                genericMap = cursor.GetGenericContext( GenericContextOptions.None ).Apply( genericMap );

                parentTypes.Push( new Pair<TypeDefDeclaration, GenericMap>( cursorTypeDef, genericMap ) );
                cursor = cursorTypeDef.BaseType;
            }

            // Find the first type (from the root) that implement the GetInstanceCredentials method.
            while ( parentTypes.Count > 0 )
            {
                Pair<TypeDefDeclaration, GenericMap> pair = parentTypes.Pop();
                TypeDefDeclaration typeDef = pair.First;

                MethodDefDeclaration methodDef = typeDef.Methods.GetOneByName( "GetInstanceCredentials" );

                if ( methodDef != null )
                {
                    // The method is defined here.
                }
                else if ( IntendManager.HasIntend( typeDef, "GetInstanceCredentials" ) )
                {
                    // The method is requested here. Implement it.

                    FieldDefDeclaration fieldDef = new FieldDefDeclaration
                                                       {
                                                           Name =
                                                               Platform.Current.NormalizeCilIdentifier(
                                                               "~instanceCredentials" ),
                                                           FieldType = instanceCredentialsType,
                                                           Attributes = FieldAttributes.Private | FieldAttributes.NotSerialized
                                                       };
                    typeDef.Fields.Add( fieldDef );
                    this.task.WeavingHelper.AddCompilerGeneratedAttribute( fieldDef.CustomAttributes );
                    IField field = GenericHelper.GetFieldCanonicalGenericInstance( fieldDef );

                    methodDef = new MethodDefDeclaration {Name = "GetInstanceCredentials"};
                    typeDef.Methods.Add( methodDef );
                    methodDef.ReturnParameter = new ParameterDeclaration
                                                    {
                                                        Attributes = ParameterAttributes.Retval,
                                                        ParameterType = instanceCredentialsType
                                                    };
                    methodDef.MethodBody.RootInstructionBlock = methodDef.MethodBody.CreateInstructionBlock();
                    methodDef.Attributes = MethodAttributes.Family;
                    methodDef.CallingConvention = CallingConvention.HasThis;
                    this.task.WeavingHelper.AddCompilerGeneratedAttribute(methodDef.CustomAttributes);

                    InstructionSequence sequence = methodDef.MethodBody.CreateInstructionSequence();
                    methodDef.MethodBody.RootInstructionBlock.AddInstructionSequence( sequence, NodePosition.After, null );
                    this.writer.AttachInstructionSequence( sequence );
                    this.writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                    this.writer.EmitInstructionField( OpCodeNumber.Ldfld, field );
                    this.writer.EmitInstruction( OpCodeNumber.Ret );
                    this.writer.DetachInstructionSequence();

                    this.task.InstanceInitializationManager.RegisterClient(typeDef,
                        new InitializationClient( field, this ));
                    
                }

                if ( methodDef == null ) continue;


                // We will return this method, but we should get the proper generic instance
                // of the defining type.

                INamedType typeRef = (INamedType) typeDef.Translate( this.module );
                IType typeSpec;

                if ( typeDef.IsGenericDefinition )
                {
                    typeSpec = this.module.TypeSpecs.GetBySignature(
                        new GenericTypeInstanceTypeSignature( typeRef, pair.Second.GetGenericTypeParameters() ),
                        true );
                }
                else
                {
                    typeSpec = typeRef;
                }

                return
                    typeSpec.Methods.GetMethod( "GetInstanceCredentials", this.getInstanceCredentialsMethodSignature,
                                                BindingOptions.Default );
            }

            // The method is not and will not be implemented.
            return null;
        }

     

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose()
        {
            this.writer.Dispose();
        }

        #endregion

        class InitializationClient : IInstanceInitializationClient
        {
            private readonly IField credentialsField;
            private readonly InstanceCredentialsManager parent;

            public InitializationClient( IField field, InstanceCredentialsManager parent )
            {
                this.credentialsField = field;
                this.parent = parent;
            }

            public void Emit( InstructionWriter instructionWriter, InstructionBlock block )
            {
                InstructionSequence sequence = block.MethodBody.CreateInstructionSequence();
                block.AddInstructionSequence(sequence, NodePosition.After, null);
                instructionWriter.AttachInstructionSequence(sequence);
                instructionWriter.EmitInstruction(OpCodeNumber.Ldarg_0);
                instructionWriter.EmitInstructionMethod(OpCodeNumber.Call,
                                                                 this.parent.makeNewInstanceCredentialsMethod);
                instructionWriter.EmitInstructionField(OpCodeNumber.Stfld, credentialsField);
                instructionWriter.DetachInstructionSequence();
            }

            public int Priority
            {
                get { return int.MinValue; }
            }
        }
       
    }
}