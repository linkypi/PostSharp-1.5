#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Extensibility;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Manages initialization of aspects, i.e. the generation aspect initialization
    /// methods and the enhancement of constructors so that these methods are invoked.
    /// </summary>
    /// <remarks>
    /// <para>Aspects that need to be initialized should register themselves using the
    /// <see cref="RegisterClient"/> method.
    /// </para>
    /// <para>An instance of this class is available on the <see cref="LaosTask.InstanceInitializationManager"/>
    /// property of the <see cref="LaosTask"/> class.</para>
    /// <para>The <see cref="InstanceInitializationManager"/> performs the following transformatios:</para>
    /// <list type="bullet">
    /// <item>For each type requiring initialization, it creates a private method (typically named <b>~InitializeAspects~1</b>)
    /// and let all clients (register using <see cref="RegisterClient"/>) emit code in that method. Then, constructors
    /// are enhanced so that they invoke this method.
    /// </item>
    /// <item>It creates a protected virtual method named <b>InitializeAspects</b>, calling the base method (if any),
    /// then calling the private method described above. This protected method can be retrieved using
    /// <see cref="GetInitializeAspectsProtectedMethod"/>.
    /// </item>
    /// </list>
    /// </remarks>
    public sealed class InstanceInitializationManager
    {
        private const string initializeAspectsMethodName = "InitializeAspects";

        private readonly Dictionary<TypeDefDeclaration, MethodMappingPair> cache =
            new Dictionary<TypeDefDeclaration, MethodMappingPair>();

        private readonly MethodSignature initializeAspectsMethodSignature;

        private readonly Dictionary<TypeDefDeclaration, List<IInstanceInitializationClient>> clients =
            new Dictionary<TypeDefDeclaration, List<IInstanceInitializationClient>>();

        private readonly LaosTask task;

        internal InstanceInitializationManager( LaosTask task )
        {
            this.task = task;

            ModuleDeclaration module = this.task.Project.Module;
            initializeAspectsMethodSignature = new MethodSignature(
                module, CallingConvention.HasThis, module.Cache.GetIntrinsic( IntrinsicType.Void ),
                new ITypeSignature[] {module.Cache.GetIntrinsic( IntrinsicType.Boolean )}, 0 );
            initializeAspectsMethodSignature = new MethodSignature(
                module, CallingConvention.HasThis, module.Cache.GetIntrinsic( IntrinsicType.Void ),
                new ITypeSignature[0], 0 );
        }

        /// <summary>
        /// Registers an <see cref="IInstanceInitializationClient"/> for a given <see cref="TypeDefDeclaration"/>.
        /// </summary>
        /// <param name="typeDef">The type to be enhanced.</param>
        /// <param name="client">The <see cref="IInstanceInitializationClient"/> emitting initialization instructions.</param>
        public void RegisterClient(TypeDefDeclaration typeDef, IInstanceInitializationClient client)
        {
            List<IInstanceInitializationClient> list;
            if ( !clients.TryGetValue( typeDef, out list ) )
            {
                list = new List<IInstanceInitializationClient>();
                clients.Add( typeDef, list );
            }

            list.Add( client );
        }

        private bool ProcessType( TypeDefDeclaration typeDef, out MethodMappingPair methodMappingPair )
        {
            // Check in cache.

            if ( cache.TryGetValue( typeDef, out methodMappingPair ) )
            {
                return true;
            }

            MethodMappingPair parentMethodMappingPair;
            GenericMap thisGenericMap;

         

            // Look for the existing protected method.
            MethodDefDeclaration protectedMethod = (MethodDefDeclaration)typeDef.Methods.GetMethod(initializeAspectsMethodName,
                                                                                                     initializeAspectsMethodSignature,
                                                                                                     BindingOptions.DontThrowException);


            if (protectedMethod != null)
            {
                // We found a compatible method. Check its attributes. 

                // If the type is visible outside the assembly and unsealed, we require the method to be virtual
                // and protected and public.
                if (!CheckInitializeAspectsMethodAttributes(protectedMethod))
                {
                    // Invalid attributes.
                    methodMappingPair = null;
                    return false;
                }

                // We won't use this:
                parentMethodMappingPair = null;
                thisGenericMap = GenericMap.Empty;

            }
            else
            {
                // Find the parent protected method. 
              
                if (typeDef.BaseType != null)
                {
                    IType baseType;

                    GenericTypeInstanceTypeSignature genericTypeInstance =
                        typeDef.BaseType.GetNakedType(TypeNakingOptions.None) as GenericTypeInstanceTypeSignature;
                    if (genericTypeInstance != null)
                    {
                        thisGenericMap = genericTypeInstance.GetGenericContext(GenericContextOptions.None);
                        baseType = genericTypeInstance.GenericDefinition.GetTypeDefinition();
                    }
                    else
                    {
                        thisGenericMap = this.task.Project.Module.Cache.IdentityGenericMap;
                        baseType = typeDef.BaseType;
                    }


                    TypeDefDeclaration baseTypeDef = baseType as TypeDefDeclaration;


                    if (baseTypeDef != null)
                    {
                        // The base type is in the current module. We eventually have to generate initialization methods
                        // in the base type before we proceed with the current type.
                        if (!ProcessType(baseTypeDef, out parentMethodMappingPair))
                        {
                            methodMappingPair = null;
                            return false;
                        }
                    }
                    else
                    {
                        // The base type is in a dependency module. We have to find the protected method there.
                        parentMethodMappingPair = FindParentProtectedMethod(typeDef);
                    }
                }
                else
                {
                    parentMethodMappingPair = null;
                    thisGenericMap = GenericMap.Empty;
                }
            }

            // Check if we have to generate anything. Maybe not.
            List<IInstanceInitializationClient> clientsOnThisType;
            if ( !clients.TryGetValue( typeDef, out clientsOnThisType ) )
            {
                // No, we don't need to generate it.
                if ( protectedMethod != null )
                {
                    methodMappingPair = new MethodMappingPair {ProtectedMethod = protectedMethod, GenericMap = typeDef.GetGenericContext( GenericContextOptions.None )};
                }
                else if ( parentMethodMappingPair != null )
                {
                    methodMappingPair = new MethodMappingPair
                                            {ProtectedMethod = parentMethodMappingPair.ProtectedMethod, GenericMap = parentMethodMappingPair.GenericMap.Apply( thisGenericMap )};
                }
                else
                {
                    methodMappingPair = null;
                }

                cache.Add( typeDef, methodMappingPair );
                return true;
            }

            // If we are here, we have to generate a private method and generate or override the protected method.

            // First check the attributes of the parent method, if any.
            if ( parentMethodMappingPair != null )
            {
                MethodDefDeclaration parentProtectedMethod = parentMethodMappingPair.ProtectedMethod;

                if (!(parentProtectedMethod.IsVirtual || parentProtectedMethod.IsAbstract) || parentProtectedMethod.IsSealed)
                {
                    // Invalid attributes.
                    LaosMessageSource.Instance.Write(SeverityType.Error, "LA0043", new object[] { parentProtectedMethod, typeDef });
                    methodMappingPair = null;
                    return false;
                }

                if (!CheckInitializeAspectsMethodAttributes(parentProtectedMethod))
                {
                    // Invalid attributes.
                   
                    methodMappingPair = null;
                    return false;
                }

            }

            MethodDefDeclaration privateMethod;

            using ( InstructionWriter writer = new InstructionWriter() )
            {
                #region Generate the private method

                string privateMethodName;
                int i = 1;
                while (
                    typeDef.Methods.GetOneByName(
                        privateMethodName = "~" + initializeAspectsMethodName + "~" + i.ToString() ) != null )
                    i++;


                privateMethod = new MethodDefDeclaration
                                    {
                                        Attributes = MethodAttributes.Private,
                                        CallingConvention = CallingConvention.HasThis,
                                        Name = privateMethodName
                                    };
                typeDef.Methods.Add( privateMethod );
                privateMethod.ReturnParameter =
                    ParameterDeclaration.CreateReturnParameter( initializeAspectsMethodSignature.ReturnType );
                privateMethod.MethodBody.RootInstructionBlock = privateMethod.MethodBody.CreateInstructionBlock();
                this.task.WeavingHelper.AddCompilerGeneratedAttribute( privateMethod.CustomAttributes );
                clientsOnThisType.Sort((x,y) => x.Priority.CompareTo( y.Priority ) );

                foreach ( IInstanceInitializationClient client in clientsOnThisType )
                {
                    InstructionBlock initializerBlock = privateMethod.MethodBody.CreateInstructionBlock();
                    privateMethod.MethodBody.RootInstructionBlock.AddChildBlock( initializerBlock,
                                                                                 NodePosition.After, null );
                    client.Emit( writer, initializerBlock );
                }

                InstructionBlock privateReturnBlock = privateMethod.MethodBody.CreateInstructionBlock();
                privateMethod.MethodBody.RootInstructionBlock.AddChildBlock( privateReturnBlock, NodePosition.After, null );
                InstructionSequence privateReturnSequence = privateMethod.MethodBody.CreateInstructionSequence();
                privateReturnBlock.AddInstructionSequence( privateReturnSequence, NodePosition.After, null );
                writer.AttachInstructionSequence( privateReturnSequence );
                writer.EmitInstruction( OpCodeNumber.Ret );
                writer.DetachInstructionSequence();

                IMethod privateMethodSpec = GenericHelper.GetMethodCanonicalGenericInstance( privateMethod );

                #endregion

                // Add a call to this private method in constructors.
                this.task.TypeLevelAdvices.Add( new ConstructorAdvice( typeDef, privateMethodSpec ) );


                 if ( protectedMethod == null )
                {
                    // Yes, we have to generate it.

                    #region Generate the protected method

                    // Generate the metadata.

                    protectedMethod = new MethodDefDeclaration
                                          {
                                              Name = initializeAspectsMethodName,
                                              CallingConvention = CallingConvention.HasThis
                                          };

                    if ( parentMethodMappingPair != null )
                    {
                        protectedMethod.Attributes = (parentMethodMappingPair.ProtectedMethod.Attributes & MethodAttributes.MemberAccessMask) | MethodAttributes.ReuseSlot | MethodAttributes.Virtual;
                    }
                    else
                    {
                        protectedMethod.Attributes = MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.NewSlot;
                    }
                    if ( typeDef.IsSealed )
                    {
                        protectedMethod.Attributes |= MethodAttributes.Final;
                    }
                    typeDef.Methods.Add( protectedMethod );
                    protectedMethod.ReturnParameter =
                        ParameterDeclaration.CreateReturnParameter( initializeAspectsMethodSignature.ReturnType );


                    task.WeavingHelper.AddCompilerGeneratedAttribute( protectedMethod.CustomAttributes );

                    // Generate the body, but without jnjecting calls to the initializers.
                    // These will be injected from an OnSuccess advice.
                    protectedMethod.MethodBody.RootInstructionBlock =
                        protectedMethod.MethodBody.CreateInstructionBlock();


                    InstructionBlock firstBlock = protectedMethod.MethodBody.CreateInstructionBlock();
                    protectedMethod.MethodBody.RootInstructionBlock.AddChildBlock( firstBlock, NodePosition.After, null );
                    InstructionSequence firstSequence = protectedMethod.MethodBody.CreateInstructionSequence();
                    InstructionSequence returnSequence = protectedMethod.MethodBody.CreateInstructionSequence();
                    firstBlock.AddInstructionSequence( firstSequence, NodePosition.After, null );
                    firstBlock.AddInstructionSequence( returnSequence, NodePosition.After, null );
                    writer.AttachInstructionSequence( firstSequence );
                    writer.EmitSymbolSequencePoint( SymbolSequencePoint.Hidden );

                    // Call the base method, if a any.
                    if ( parentMethodMappingPair != null )
                    {
                        IMethod protectedMethodSpec = GetProtectedMethodSpec( parentMethodMappingPair, thisGenericMap );


                        writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                        writer.EmitInstructionMethod( OpCodeNumber.Call, protectedMethodSpec );
                    }
                    else
                    {
                        writer.EmitInstruction( OpCodeNumber.Nop );
                    }

                    writer.DetachInstructionSequence();


                    // Emit the return instruction.
                    writer.AttachInstructionSequence( returnSequence );
                    writer.EmitInstruction( OpCodeNumber.Ret );
                    writer.DetachInstructionSequence();

                    #endregion
                }

                // Add a call of the private method to the protected method.
                this.task.MethodLevelAdvices.Add( new ProtectedMethodAdvice( protectedMethod, privateMethodSpec ) );

                methodMappingPair = new MethodMappingPair
                                        {
                                            ProtectedMethod = protectedMethod, 
                                            PrivateMethod = privateMethod,
                                            GenericMap = typeDef.GetGenericContext( GenericContextOptions.None )
                                        };
                this.cache.Add( typeDef, methodMappingPair );
            }

            return true;
        }

        private static bool CheckInitializeAspectsMethodAttributes( MethodDefDeclaration method )
        {
            if ( VisibilityHelper.IsPublic( method.DeclaringType ) && !method.DeclaringType.IsSealed &&
                   (!method.IsVirtual || method.IsSealed || !VisibilityHelper.IsPublic( method )) )
            {
                LaosMessageSource.Instance.Write(SeverityType.Error, "LA0040", new object[] { method });
                return false;
            }

            if ( method.IsStatic )
            {
                LaosMessageSource.Instance.Write(SeverityType.Error, "LA0044", new object[] { method });
                return false;
            }

            return true;
        }

        private IMethod GetProtectedMethodSpec( MethodMappingPair parentMethodMappingPair, GenericMap thisGenericMap )
        {
            ModuleDeclaration module = this.task.Project.Module;
            IMethod protectedMethodSpec;
            TypeDefDeclaration protectedMethodDeclaringTypeDef = parentMethodMappingPair.ProtectedMethod.DeclaringType;

            if ( protectedMethodDeclaringTypeDef.IsGenericDefinition )
            {
                // If the declaring type of the protected method is generic, we have to make a TypeSpec.


                INamedType protectedMethodDeclaringTypeRef = (INamedType) protectedMethodDeclaringTypeDef.Translate( module );
                IType protectedMethodDeclaringTypeSpec;

                if ( protectedMethodDeclaringTypeDef.IsGenericDefinition )
                {
                    protectedMethodDeclaringTypeSpec = module.TypeSpecs.GetBySignature(
                        new GenericTypeInstanceTypeSignature( protectedMethodDeclaringTypeRef,
                                                              parentMethodMappingPair.GenericMap.Apply( thisGenericMap ).GetGenericTypeParameters() ),
                        true );
                }
                else
                {
                    protectedMethodDeclaringTypeSpec = protectedMethodDeclaringTypeRef;
                }

                protectedMethodSpec =
                    protectedMethodDeclaringTypeSpec.Methods.GetMethod( initializeAspectsMethodName, this.initializeAspectsMethodSignature,
                                                                        BindingOptions.Default );
            }
            else
            {
                protectedMethodSpec = parentMethodMappingPair.ProtectedMethod.Translate( module );
            }
            return protectedMethodSpec;
        }

        private MethodMappingPair FindParentProtectedMethod( TypeDefDeclaration typeDef )
        {
            TypeDefDeclaration baseTypeDef;
            GenericMap genericMap = typeDef.Module.Cache.IdentityGenericMap;

            // The base type is in a dependency. We have to find the protected method.
            IType cursor = typeDef.BaseType;

            MethodDefDeclaration parentProtectedMethod = null;
            while ( cursor != null )
            {
                GenericTypeInstanceTypeSignature genericTypeInstance = cursor.GetNakedType( TypeNakingOptions.None ) as GenericTypeInstanceTypeSignature;

                if ( genericTypeInstance != null )
                {
                    genericMap = genericTypeInstance.GetGenericContext( GenericContextOptions.None ).Apply( genericMap );
                }

                baseTypeDef = cursor.GetTypeDefinition();
                parentProtectedMethod = (MethodDefDeclaration) baseTypeDef.Methods.GetMethod( initializeAspectsMethodName,
                                                                                              initializeAspectsMethodSignature.Translate( baseTypeDef.Module ),
                                                                                              BindingOptions.DontThrowException );

                if ( parentProtectedMethod != null )
                    break;

                cursor = baseTypeDef.BaseType;
            }
            return parentProtectedMethod == null ? null : new MethodMappingPair {ProtectedMethod = parentProtectedMethod, GenericMap = genericMap};
        }

        /// <summary>
        /// Gets the generated <b>InitializeAspects</b> protected method
        /// for a given type.
        /// </summary>
        /// <param name="typeDef">The type for which the <b>InitializeAspects</b> method should be returned.</param>
        /// <returns>The <b>InitializeAspects</b> protected method for type <paramref name="typeDef"/>,
        /// or <b>null</b> if this method does not exist for this type (which would mean that this type
        /// required no aspect initialization).</returns>
        public IMethod GetInitializeAspectsProtectedMethod( TypeDefDeclaration typeDef )
        {
            MethodMappingPair value;
            if ( !this.cache.TryGetValue( typeDef, out value ) )
            {
                value = this.FindParentProtectedMethod( typeDef );
                this.cache.Add( typeDef, value );
            }

            return value == null ? null : GetProtectedMethodSpec( value, typeDef.GetGenericContext( GenericContextOptions.None ) );
        }

        /// <summary>
        /// Gets the generated <b>InitializeAspects</b> (exact name may vary) private method for a given type.
        /// </summary>
        /// <param name="typeDef">The type for which the <b>InitializeAspects</b> method should be returned.</param>
        /// <returns>The <b>InitializeAspects</b> protected method for type <paramref name="typeDef"/>,
        /// or <b>null</b> if this method does not exist for this type (which would mean that this type
        /// required no aspect initialization).</returns>
        public MethodDefDeclaration GetInitializeAspectsPrivateMethod(TypeDefDeclaration typeDef)
        {
            MethodMappingPair value;
            return !this.cache.TryGetValue(typeDef, out value) ? null : value.PrivateMethod;
        }

        internal bool Implement()
        {
            bool success = true;
            foreach ( KeyValuePair<TypeDefDeclaration, List<IInstanceInitializationClient>> pair in clients )
            {
                TypeDefDeclaration typeDef = pair.Key;

                MethodMappingPair methodMappingPair;
                if ( !ProcessType( typeDef, out methodMappingPair ) )
                    success = false;
            }

            return success;
        }

        #region Nested type: ConstructorAdvice

        private class ConstructorAdvice : ITypeLevelAdvice
        {
            private readonly IMethod initializeAspectsMethod;
            private readonly TypeDefDeclaration typeDef;

            public ConstructorAdvice( TypeDefDeclaration typeDef, IMethod initializeAspectsMethod )
            {
                this.typeDef = typeDef;
                this.initializeAspectsMethod = initializeAspectsMethod;
            }

            #region ITypeLevelAdvice Members

            int IAdvice.Priority
            {
                get { return int.MinValue; }
            }

            bool IAdvice.RequiresWeave( WeavingContext context )
            {
                return true;
            }

            void IAdvice.Weave( WeavingContext context, InstructionBlock block )
            {
                InstructionSequence sequence = context.Method.MethodBody.CreateInstructionSequence();
                block.AddInstructionSequence( sequence, NodePosition.After, null );
                context.InstructionWriter.AttachInstructionSequence( sequence );
                context.InstructionWriter.EmitInstruction( OpCodeNumber.Ldarg_0 );
                context.InstructionWriter.EmitInstructionMethod( OpCodeNumber.Call, this.initializeAspectsMethod );
                context.InstructionWriter.DetachInstructionSequence();
            }

            TypeDefDeclaration ITypeLevelAdvice.Type
            {
                get { return typeDef; }
            }

            JoinPointKinds ITypeLevelAdvice.JoinPointKinds
            {
                get { return JoinPointKinds.AfterInstanceInitialization; }
            }

            #endregion
        }

        #endregion

        
        #region Nested type: ProtectedMethodAdvice

        private class ProtectedMethodAdvice : IMethodLevelAdvice
        {
            private readonly IMethod privateMethod;
            private readonly MethodDefDeclaration protectedMethod;

            public ProtectedMethodAdvice( MethodDefDeclaration protectedMethod, IMethod privateMethod )
            {
                this.protectedMethod = protectedMethod;
                this.privateMethod = privateMethod;
            }

            #region IMethodLevelAdvice Members

            int IAdvice.Priority
            {
                get { return int.MaxValue; }
            }

            bool IAdvice.RequiresWeave( WeavingContext context )
            {
                return true;
            }

            void IAdvice.Weave( WeavingContext context, InstructionBlock block )
            {
                InstructionSequence sequence = context.Method.MethodBody.CreateInstructionSequence();
                block.AddInstructionSequence( sequence, NodePosition.After, null );
                context.InstructionWriter.AttachInstructionSequence( sequence );
                context.InstructionWriter.EmitInstruction( OpCodeNumber.Ldarg_0 );
                context.InstructionWriter.EmitInstructionMethod( OpCodeNumber.Call, this.privateMethod );
                context.InstructionWriter.DetachInstructionSequence();
            }

            MethodDefDeclaration IMethodLevelAdvice.Method
            {
                get { return protectedMethod; }
            }

            MetadataDeclaration IMethodLevelAdvice.Operand
            {
                get { return null; }
            }

            JoinPointKinds IMethodLevelAdvice.JoinPointKinds
            {
                get { return JoinPointKinds.AfterMethodBodySuccess; }
            }

            #endregion
        }

        #endregion

        private class MethodMappingPair
        {
            public MethodDefDeclaration ProtectedMethod;
            public MethodDefDeclaration PrivateMethod;
            public GenericMap GenericMap;
        }
    }

    /// <summary>
    /// Client of the <see cref="InstanceCredentialsManager"/> class.
    /// Emits aspect initialization instructions into enhanced classes.
    /// </summary>
    public interface IInstanceInitializationClient
    {
        /// <summary>
        /// Emits aspect initialization instructions into enhanced classes.
        /// </summary>
        /// <param name="writer">The <see cref="InstructionWriter"/> to use.</param>
        /// <param name="block">The <see cref="InstructionBlock"/> where instructions shoujld
        /// be emitted.</param>
        void Emit( InstructionWriter writer, InstructionBlock block );

        /// <summary>
        /// Gets the priority of the current client. Clients on the same type will be sorted by
        /// ascending priority.
        /// </summary>
        int Priority { get;  }
    }
    
}