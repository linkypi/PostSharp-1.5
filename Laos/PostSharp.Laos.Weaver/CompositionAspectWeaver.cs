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
using System.Text;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Binding;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;
using PostSharp.Extensibility;

namespace PostSharp.Laos.Weaver
{
    internal class CompositionAspectWeaver : TypeLevelAspectWeaver, IInstanceInitializationClient
    {
        private MethodSignature getImplementationMethodSignature;
        private MethodSignature setImplementationMethodSignature;
        private IMethod instanceBoundLaosEventArgsConstructor;
        private IMethod composionAspectCreateImplementationObject;
        private IMethod instanceCredentialsAssertEquals;
        private IMethod instanceBoundLaosEventArgsSetInstanceCredentials;
        private IField implementationFieldRef;
        private IType exposedPublicInterfaceType;
        private IType[] exposedProtectedInterfaceTypes;
        private TypeSpecDeclaration[] exposedProtectedInterfaceGenericInstances;
        private IType protectedInterfaceType;
        private TypeDefDeclaration targetType;
        private bool ignore;
        private CompositionAspectOptions options;
        private ITypeSignature instanceCredentialsType;
        private INamedType composedInterfaceGenericType;

        private static readonly CompositionAspectConfigurationAttribute defaultConfiguration =
            new CompositionAspectConfigurationAttribute {Options = CompositionAspectOptions.None, AspectPriority = 0};

        public CompositionAspectWeaver() : base( defaultConfiguration )
        {
        }

        public override void Initialize()
        {
            base.Initialize();


            ModuleDeclaration module = this.Task.Project.Module;


            this.composionAspectCreateImplementationObject =
                module.Cache.GetItem( () => module.FindMethod(
                                                module.GetTypeForFrameworkVariant( typeof(ICompositionAspect) ),
                                                "CreateImplementationObject" ) );

            this.instanceCredentialsAssertEquals =
                module.Cache.GetItem( () => module.FindMethod(
                                                module.GetTypeForFrameworkVariant( typeof(InstanceCredentials) ),
                                                "AssertEquals" ) );

            this.instanceBoundLaosEventArgsConstructor =
                module.Cache.GetItem( () => module.FindMethod(
                                                module.GetTypeForFrameworkVariant( typeof(InstanceBoundLaosEventArgs) ),
                                                ".ctor" ) );

            this.instanceBoundLaosEventArgsSetInstanceCredentials =
                module.Cache.GetItem( () => module.FindMethod(
                                                module.GetTypeForFrameworkVariant( typeof(InstanceBoundLaosEventArgs) ),
                                                "set_InstanceCredentials" ) );

            this.instanceCredentialsType = module.GetTypeForFrameworkVariant( typeof(InstanceCredentials) );

            this.getImplementationMethodSignature =
                module.Cache.GetItem( () =>
                                          {
                                              MethodSignature signature =
                                                  new MethodSignature( module )
                                                      {
                                                          CallingConvention =
                                                              CallingConvention.HasThis
                                                      };
                                              signature.ParameterTypes.Add( this.instanceCredentialsType );
                                              signature.ReturnType =
                                                  module.Cache.GetGenericParameter( 0,
                                                                                    GenericParameterKind
                                                                                        .Type );
                                              return signature;
                                          } );

            this.setImplementationMethodSignature =
                module.Cache.GetItem( () =>
                                          {
                                              MethodSignature signature =
                                                  new MethodSignature( module )
                                                      {
                                                          CallingConvention = CallingConvention.HasThis
                                                      };
                                              signature.ParameterTypes.Add(
                                                  module.GetTypeForFrameworkVariant(
                                                      typeof(InstanceCredentials) ) );
                                              signature.ParameterTypes.Add(
                                                  module.Cache.GetGenericParameter( 0,
                                                                                    GenericParameterKind
                                                                                        .Type ) );
                                              signature.ReturnType =
                                                  module.Cache.GetIntrinsic( IntrinsicType.Void );
                                              return signature;
                                          } );

            this.composedInterfaceGenericType = (INamedType)
                                                module.GetTypeForFrameworkVariant( typeof(IComposed<>) );

            this.protectedInterfaceType = (IType)
                                          module.GetTypeForFrameworkVariant( typeof(IProtectedInterface<>) );


            TypeIdentity publicInterfaceSystemTypeIdentity =
                GetConfigurationObject<ICompositionAspectConfiguration, TypeIdentity>(
                    c => c.GetPublicInterface( this.TargetReflectionType ) );

            if ( publicInterfaceSystemTypeIdentity != null )
            {
                if ( publicInterfaceSystemTypeIdentity.Type != null )
                {
                    this.exposedPublicInterfaceType =
                        (IType)
                        module.FindType( publicInterfaceSystemTypeIdentity.Type,
                                         BindingOptions.RequireGenericInstance | BindingOptions.RequireIType );
                }
                else
                {
                    this.exposedPublicInterfaceType =
                        (IType) module.Cache.GetType( publicInterfaceSystemTypeIdentity.TypeName );
                }
            }

            TypeIdentity[] protectedInterfaceSystemTypeIdentities =
                GetConfigurationObject<ICompositionAspectConfiguration, TypeIdentity[]>(
                    c => c.GetProtectedInterfaces( this.TargetReflectionType ) );
            if ( protectedInterfaceSystemTypeIdentities != null )
            {
                this.exposedProtectedInterfaceGenericInstances =
                    new TypeSpecDeclaration[protectedInterfaceSystemTypeIdentities.Length];

                this.exposedProtectedInterfaceTypes = new IType[protectedInterfaceSystemTypeIdentities.Length];

                for ( int i = 0; i < protectedInterfaceSystemTypeIdentities.Length; i++ )
                {
                    if ( protectedInterfaceSystemTypeIdentities[i].Type != null )
                    {
                        this.exposedProtectedInterfaceTypes[i] =
                            (IType)
                            module.FindType( protectedInterfaceSystemTypeIdentities[i].Type,
                                             BindingOptions.RequireGenericInstance | BindingOptions.RequireIType );
                    }
                    else
                    {
                        this.exposedProtectedInterfaceTypes[i] =
                            (IType)
                            module.Cache.GetType( protectedInterfaceSystemTypeIdentities[i].TypeName );
                    }

                    this.exposedProtectedInterfaceGenericInstances[i] = module.TypeSpecs.GetBySignature(
                        new GenericTypeInstanceTypeSignature(
                            (INamedType) this.protectedInterfaceType,
                            new ITypeSignature[] {this.exposedProtectedInterfaceTypes[i]} ),
                        true );
                }
            }

            this.options =
                GetConfigurationValue<ICompositionAspectConfiguration, CompositionAspectOptions>( c => c.GetOptions() );
        }


        protected internal override void OnTargetAssigned( bool reassigned )
        {
            this.targetType = (TypeDefDeclaration) this.TargetType;
        }

        private static void PopulateImplementedInterfaces( ITypeSignature implementedInterface, Set<ITypeSignature> set )
        {
            set.AddIfAbsent( implementedInterface );

            IType implementedInterfaceType = implementedInterface as IType;

            TypeDefDeclaration implementedInterfaceTypeDef;
            if ( implementedInterfaceType != null )
            {
                implementedInterfaceTypeDef = implementedInterfaceType.GetTypeDefinition();
            }
            else
            {
                implementedInterfaceTypeDef =
                    ((GenericTypeInstanceTypeSignature) implementedInterface).GenericDefinition.GetTypeDefinition();
            }


            foreach (
                InterfaceImplementationDeclaration interfaceImpl in implementedInterfaceTypeDef.InterfaceImplementations
                )
            {
                ITypeSignature childInterface = interfaceImpl.ImplementedInterface;

                ITypeSignature mappedChildInterface =
                    childInterface.MapGenericArguments(
                        implementedInterface.GetGenericContext( GenericContextOptions.None ) );

                PopulateImplementedInterfaces( mappedChildInterface, set );
            }
        }


        public override bool ValidateSelf()
        {
            if ( !base.ValidateSelf() ) return false;

            if ( (this.targetType.Attributes & TypeAttributes.Interface) != 0 )
            {
                LaosMessageSource.Instance.Write( SeverityType.Error, "LA0016",
                                                  new object[] {this.GetAspectTypeName(), this.targetType.Name} );
                return false;
            }

            Set<ITypeSignature> existingInterfaces = new Set<ITypeSignature>( 4, TypeComparer.GetInstance() );
            PopulateImplementedInterfaces( this.targetType, existingInterfaces );

            // Determines whether the type already implements the public interface.
            if ( this.exposedPublicInterfaceType != null )
            {
                if ( this.targetType.IsAssignableTo( this.exposedPublicInterfaceType,
                                                     this.Task.Project.Module.Cache.IdentityGenericMap ) )
                {
                    if ( (this.options & CompositionAspectOptions.IgnoreIfAlreadyImplemented) != 0 )
                    {
                        this.ignore = true;
                        return true;
                    }
                }
                else
                {
                    IntendManager.AddIntend( this.targetType, this.exposedPublicInterfaceType.ToString() );
                }

                Set<ITypeSignature> addedInterfaces = new Set<ITypeSignature>( 4, TypeComparer.GetInstance() );

                PopulateImplementedInterfaces( this.exposedPublicInterfaceType, addedInterfaces );

                bool valid = true;
                foreach ( ITypeSignature addedInterface in addedInterfaces )
                {
                    if ( existingInterfaces.Contains( addedInterface ) )
                    {
                        LaosMessageSource.Instance.Write( SeverityType.Error, "LA0004",
                                                          new object[]
                                                              {
                                                                  addedInterface.ToString(),
                                                                  this.targetType.ToString()
                                                              } );
                        valid = false;
                    }
                }

                if ( !valid )
                    return false;
            }

            this.Task.InstanceCredentialsManager.RequestGetInstanceCredentialsMethod( targetType );

            return true;
        }

        public override void ValidateInteractions( LaosAspectWeaver[] weaversOnSameTarget )
        {
            base.ValidateInteractions( weaversOnSameTarget );

            IType baseType = this.targetType.BaseType;

            if ( baseType != null )
            {
                // The base type is also in the current module, it could have been marked also.

                if ( this.exposedPublicInterfaceType != null )
                {
                    if ( IntendManager.HasInheritedIntend( baseType, this.exposedPublicInterfaceType.ToString() ) )
                    {
                        if ( (this.options & CompositionAspectOptions.IgnoreIfAlreadyImplemented) == 0 )
                        {
                            LaosMessageSource.Instance.Write( SeverityType.Error, "LA0004",
                                                              new object[]
                                                                  {
                                                                      this.exposedPublicInterfaceType.ToString(),
                                                                      this.targetType.ToString()
                                                                  } );
                        }
                        else
                        {
                            this.ignore = true;
                        }
                    }
                }

                if ( this.exposedProtectedInterfaceTypes != null )
                {
                    for ( int i = 0; i < this.exposedProtectedInterfaceTypes.Length; i++ )
                    {
                        if (
                            IntendManager.HasInheritedIntend( baseType,
                                                              this.exposedProtectedInterfaceGenericInstances[i].ToString
                                                                  () ) )
                        {
                            if ( (this.options & CompositionAspectOptions.IgnoreIfAlreadyImplemented) == 0 )
                            {
                                LaosMessageSource.Instance.Write( SeverityType.Error, "LA0007",
                                                                  new object[]
                                                                      {
                                                                          this.exposedProtectedInterfaceGenericInstances
                                                                              [
                                                                              i].ToString(),
                                                                          this.targetType.ToString()
                                                                      } );
                            }
                            else
                            {
                                this.ignore = true;
                            }
                        }
                    }
                }
            }
        }

        public override void Implement()
        {
            if ( this.ignore )
            {
                return;
            }

            ModuleDeclaration module = this.Task.Project.Module;

            IMethod getCredentialsMethod =
                this.Task.InstanceCredentialsManager.GetGetInstanceCredentialsMethod( this.targetType );

            // Get and control the composed interface.
            if ( this.exposedPublicInterfaceType != null )
            {
                TypeDefDeclaration composedInterfaceTypeDef = exposedPublicInterfaceType.GetTypeDefinition();

                if ( (composedInterfaceTypeDef.Attributes & TypeAttributes.Interface) == 0 )
                {
                    LaosMessageSource.Instance.Write( SeverityType.Error, "LA0002",
                                                      new object[]
                                                          {
                                                              this.TargetReflectionType.FullName,
                                                              composedInterfaceTypeDef.Name
                                                          } );
                    return;
                }

                // Add the interface to the type.
                targetType.InterfaceImplementations.Add( exposedPublicInterfaceType );
            }

            // Define a new field on the type.
            FieldDefDeclaration composedField = new FieldDefDeclaration();
            StringBuilder fieldNameBuilder = new StringBuilder();
            fieldNameBuilder.Append( '~' );
            if ( exposedPublicInterfaceType != null )
            {
                exposedPublicInterfaceType.WriteReflectionTypeName( fieldNameBuilder, ReflectionNameOptions.None );
                composedField.Name = fieldNameBuilder.ToString();
            }
            else
            {
                composedField.Name = string.Format( "~composed~{0}", targetType.Fields.Count + 1 );
            }

            composedField.FieldType = (ITypeSignature) exposedPublicInterfaceType ??
                                      module.Cache.GetIntrinsic( IntrinsicType.Object );
            composedField.Attributes = FieldAttributes.Private;
            if ( (this.options & CompositionAspectOptions.NonSerializedImplementationField) != 0 )
            {
                composedField.Attributes |= FieldAttributes.NotSerialized;
            }

            targetType.Fields.Add( composedField );
            this.Task.WeavingHelper.AddCompilerGeneratedAttribute( composedField.CustomAttributes );
            this.implementationFieldRef = GenericHelper.GetFieldCanonicalGenericInstance( composedField );

            // Implement interface methods.
            Set<IType> alreadyImplementedInterfaces = new Set<IType>();

            if ( exposedPublicInterfaceType != null )
            {
                this.ImplementRecursive( exposedPublicInterfaceType,
                                         alreadyImplementedInterfaces );

                #region Implement the IComposed interface

                if ( (this.options & CompositionAspectOptions.GenerateImplementationAccessor) != 0 )
                {
                    TypeSpecDeclaration composedInterface = module.TypeSpecs.GetBySignature(
                        new GenericTypeInstanceTypeSignature( this.composedInterfaceGenericType,
                                                              new ITypeSignature[] {this.exposedPublicInterfaceType} ),
                        true );
                    this.targetType.InterfaceImplementations.Add( composedInterface );

                    #region GetImplementation method

                    {
                        IMethod getImplementationMethod = composedInterface.MethodRefs.GetMethod(
                            "GetImplementation", this.getImplementationMethodSignature, BindingOptions.Default );

                        MethodDefDeclaration getImplementationMethodImpl = new MethodDefDeclaration();
                        StringBuilder nameBuilder = new StringBuilder();
                        composedInterface.WriteReflectionTypeName( nameBuilder, ReflectionNameOptions.None );
                        nameBuilder.Append( ".GetImplementation" );
                        getImplementationMethodImpl.Name = nameBuilder.ToString();
                        getImplementationMethodImpl.Attributes = MethodAttributes.Private | MethodAttributes.HideBySig |
                                                                 MethodAttributes.NewSlot | MethodAttributes.Virtual |
                                                                 MethodAttributes.Final;
                        getImplementationMethodImpl.CallingConvention = CallingConvention.HasThis;
                        targetType.Methods.Add( getImplementationMethodImpl );
                        getImplementationMethodImpl.InterfaceImplementations.Add( getImplementationMethod );

                        getImplementationMethodImpl.ReturnParameter = new ParameterDeclaration
                                                                          {
                                                                              Attributes = ParameterAttributes.Retval,
                                                                              ParameterType =
                                                                                  this.exposedPublicInterfaceType
                                                                          };
                        ParameterDeclaration credentialsParam = new ParameterDeclaration( 0, "credentials",
                                                                                          this.instanceCredentialsType );
                        getImplementationMethodImpl.Parameters.Add( credentialsParam );
                        getImplementationMethodImpl.CustomAttributes.Add(
                            this.Task.WeavingHelper.GetDebuggerNonUserCodeAttribute() );
                        this.Task.WeavingHelper.AddCompilerGeneratedAttribute(
                            getImplementationMethodImpl.CustomAttributes );

                        InstructionSequence sequence =
                            getImplementationMethodImpl.MethodBody.CreateInstructionSequence();
                        getImplementationMethodImpl.MethodBody.RootInstructionBlock =
                            getImplementationMethodImpl.MethodBody.CreateInstructionBlock();
                        getImplementationMethodImpl.MethodBody.RootInstructionBlock.AddInstructionSequence( sequence,
                                                                                                            NodePosition
                                                                                                                .
                                                                                                                After,
                                                                                                            null );


                        LocalVariableSymbol theseCredentialsLocal =
                            getImplementationMethodImpl.MethodBody.RootInstructionBlock.DefineLocalVariable(
                                credentialsParam.ParameterType, "myCredentials" );


                        InstructionWriter writer = this.Task.InstructionWriter;
                        writer.AttachInstructionSequence( sequence );

                        // Assert that credentials are correct.
                        writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                        writer.EmitInstructionMethod( OpCodeNumber.Call, getCredentialsMethod );
                        writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc, theseCredentialsLocal );
                        writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloca, theseCredentialsLocal );
                        writer.EmitInstructionParameter( OpCodeNumber.Ldarg, credentialsParam );
                        writer.EmitInstructionMethod( OpCodeNumber.Call, this.instanceCredentialsAssertEquals );

                        // Return the implementation.
                        writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                        writer.EmitInstructionField( OpCodeNumber.Ldfld, this.implementationFieldRef );
                        writer.EmitInstruction( OpCodeNumber.Ret );
                        writer.DetachInstructionSequence();
                    }

                    #endregion

                    #region SetImplementation method

                    {
                        IMethod setImplementationMethod = composedInterface.MethodRefs.GetMethod(
                            "SetImplementation", this.setImplementationMethodSignature, BindingOptions.Default );

                        MethodDefDeclaration setImplementationMethodImpl = new MethodDefDeclaration();
                        StringBuilder nameBuilder = new StringBuilder();
                        composedInterface.WriteReflectionTypeName( nameBuilder, ReflectionNameOptions.None );
                        nameBuilder.Append( ".SetImplementation" );
                        setImplementationMethodImpl.Name = nameBuilder.ToString();
                        setImplementationMethodImpl.Attributes = MethodAttributes.Private | MethodAttributes.HideBySig |
                                                                 MethodAttributes.NewSlot | MethodAttributes.Virtual |
                                                                 MethodAttributes.Final;
                        setImplementationMethodImpl.CallingConvention = CallingConvention.HasThis;
                        targetType.Methods.Add( setImplementationMethodImpl );
                        setImplementationMethodImpl.InterfaceImplementations.Add( setImplementationMethod );

                        setImplementationMethodImpl.ReturnParameter = new ParameterDeclaration
                                                                          {
                                                                              Attributes = ParameterAttributes.Retval,
                                                                              ParameterType =
                                                                                  module.Cache.GetIntrinsic(
                                                                                  IntrinsicType.Void )
                                                                          };
                        ParameterDeclaration credentialsParam = new ParameterDeclaration( 0, "credentials",
                                                                                          this.instanceCredentialsType );
                        ParameterDeclaration implementationParam = new ParameterDeclaration( 1, "implementation",
                                                                                             this.
                                                                                                 exposedPublicInterfaceType );
                        setImplementationMethodImpl.Parameters.Add( credentialsParam );
                        setImplementationMethodImpl.Parameters.Add( implementationParam );

                        setImplementationMethodImpl.CustomAttributes.Add(
                            this.Task.WeavingHelper.GetDebuggerNonUserCodeAttribute() );
                        this.Task.WeavingHelper.AddCompilerGeneratedAttribute(
                            setImplementationMethodImpl.CustomAttributes );

                        InstructionSequence sequence =
                            setImplementationMethodImpl.MethodBody.CreateInstructionSequence();
                        setImplementationMethodImpl.MethodBody.RootInstructionBlock =
                            setImplementationMethodImpl.MethodBody.CreateInstructionBlock();
                        setImplementationMethodImpl.MethodBody.RootInstructionBlock.AddInstructionSequence( sequence,
                                                                                                            NodePosition
                                                                                                                .
                                                                                                                After,
                                                                                                            null );


                        LocalVariableSymbol theseCredentialsLocal =
                            setImplementationMethodImpl.MethodBody.RootInstructionBlock.DefineLocalVariable(
                                credentialsParam.ParameterType, "myCredentials" );


                        InstructionWriter writer = this.Task.InstructionWriter;
                        writer.AttachInstructionSequence( sequence );

                        // Assert that credentials are correct.
                        writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                        writer.EmitInstructionMethod( OpCodeNumber.Call, getCredentialsMethod );
                        writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc, theseCredentialsLocal );
                        writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloca, theseCredentialsLocal );
                        writer.EmitInstructionParameter( OpCodeNumber.Ldarg, credentialsParam );
                        writer.EmitInstructionMethod( OpCodeNumber.Call, this.instanceCredentialsAssertEquals );

                        // Set the implementation.
                        writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                        writer.EmitInstruction( OpCodeNumber.Ldarg_2 );
                        writer.EmitInstructionField( OpCodeNumber.Stfld, this.implementationFieldRef );
                        writer.EmitInstruction( OpCodeNumber.Ret );
                        writer.DetachInstructionSequence();
                    }

                    #endregion
                }

                #endregion
            }

            #region Implement the protected interfaces

            if ( this.exposedProtectedInterfaceTypes != null )
            {
                for ( int i = 0; i < this.exposedProtectedInterfaceTypes.Length; i++ )
                {
                    TypeSpecDeclaration accessorInterface = this.exposedProtectedInterfaceGenericInstances[i];
                    this.targetType.InterfaceImplementations.Add( accessorInterface );

                    IMethod accessorMethod = accessorInterface.MethodRefs.GetMethod(
                        "GetInterface", this.getImplementationMethodSignature, BindingOptions.Default );

                    MethodDefDeclaration accessorMethodImpl = new MethodDefDeclaration();
                    StringBuilder nameBuilder = new StringBuilder();
                    accessorInterface.WriteReflectionTypeName( nameBuilder, ReflectionNameOptions.None );
                    nameBuilder.Append( ".GetInterface" );
                    accessorMethodImpl.Name = nameBuilder.ToString();
                    accessorMethodImpl.Attributes = MethodAttributes.Private | MethodAttributes.HideBySig |
                                                    MethodAttributes.NewSlot | MethodAttributes.Virtual |
                                                    MethodAttributes.Final;
                    targetType.Methods.Add( accessorMethodImpl );
                    accessorMethodImpl.InterfaceImplementations.Add( accessorMethod );

                    accessorMethodImpl.ReturnParameter = new ParameterDeclaration
                                                             {
                                                                 Attributes = ParameterAttributes.Retval,
                                                                 ParameterType = this.exposedProtectedInterfaceTypes[i]
                                                             };
                    ParameterDeclaration credentialsParam = new ParameterDeclaration( 0, "credentials",
                                                                                      this.instanceCredentialsType );
                    accessorMethodImpl.Parameters.Add( credentialsParam );
                    accessorMethodImpl.CustomAttributes.Add( this.Task.WeavingHelper.GetDebuggerNonUserCodeAttribute() );
                    this.Task.WeavingHelper.AddCompilerGeneratedAttribute( accessorMethodImpl.CustomAttributes );
                    InstructionSequence sequence = accessorMethodImpl.MethodBody.CreateInstructionSequence();
                    accessorMethodImpl.MethodBody.RootInstructionBlock =
                        accessorMethodImpl.MethodBody.CreateInstructionBlock();
                    accessorMethodImpl.MethodBody.RootInstructionBlock.AddInstructionSequence( sequence,
                                                                                               NodePosition.After, null );
                    InstructionWriter writer = this.Task.InstructionWriter;

                    LocalVariableSymbol theseCredentialsLocal =
                        accessorMethodImpl.MethodBody.RootInstructionBlock.DefineLocalVariable(
                            credentialsParam.ParameterType, "myCredentials" );

                    writer.AttachInstructionSequence( sequence );
                    writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                    writer.EmitInstructionMethod( OpCodeNumber.Call, getCredentialsMethod );
                    writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc, theseCredentialsLocal );
                    writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloca, theseCredentialsLocal );
                    writer.EmitInstructionParameter( OpCodeNumber.Ldarg, credentialsParam );
                    writer.EmitInstructionMethod( OpCodeNumber.Call, this.instanceCredentialsAssertEquals );
                    writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                    writer.EmitInstructionField( OpCodeNumber.Ldfld, this.implementationFieldRef );
                    writer.EmitInstructionType( OpCodeNumber.Castclass, this.exposedProtectedInterfaceTypes[i] );
                    writer.EmitInstruction( OpCodeNumber.Ret );
                    writer.DetachInstructionSequence();
                }
            }

            #endregion

            // Add an advice.
            this.Task.InstanceInitializationManager.RegisterClient( targetType, this );
        }

        private void ImplementRecursive(
            IType composedInterfaceType,
            Set<IType> alreadyImplementedInterfaces )
        {
            InstructionWriter instructionWriter = this.Task.InstructionWriter;
            ModuleDeclaration module = this.Task.Project.Module;

            alreadyImplementedInterfaces.Add( composedInterfaceType );

            TypeDefDeclaration composedInterfaceTypeDef = composedInterfaceType.GetTypeDefinition();
            StringBuilder composedInterfaceTypeName = new StringBuilder( 200 );
            composedInterfaceType.WriteReflectionTypeName( composedInterfaceTypeName, ReflectionNameOptions.None );

            // Emit the methods of this type.
            foreach ( MethodDefDeclaration targetMethodDef in composedInterfaceTypeDef.Methods )
            {
                IMethod targetMethod =
                    composedInterfaceType.Methods.GetMethod( targetMethodDef.Name,
                                                             targetMethodDef.Translate
                                                                 ( module ),
                                                             BindingOptions.Default );

                IMethod targetMethodSpec;
                if ( targetMethodDef.IsGenericDefinition )
                {
                    targetMethodSpec = ((IGenericMethodDefinition) targetMethod).MethodSpecs.GetGenericInstance(
                        module.Cache.GetGenericParameterArray(
                            targetMethodDef.GenericParameters.Count, GenericParameterKind.Method ), true );
                }
                else
                {
                    targetMethodSpec = targetMethod;
                }


                // Define the new method.
                MethodDefDeclaration newMethod = new MethodDefDeclaration
                                                     {
                                                         Name = (composedInterfaceType + "." + targetMethodDef.Name)
                                                     };
                targetType.Methods.Add( newMethod );

                newMethod.Attributes = MethodAttributes.Private | MethodAttributes.HideBySig |
                                       MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
                newMethod.InterfaceImplementations.Add( targetMethod );
                this.Task.WeavingHelper.AddCompilerGeneratedAttribute( newMethod.CustomAttributes );

                // Add generic parameters.
                foreach ( GenericParameterDeclaration genericParameter in targetMethodDef.GenericParameters )
                {
                    GenericParameterDeclaration newGenericParameter =
                        new GenericParameterDeclaration
                            {
                                Kind = GenericParameterKind.Method,
                                Name = genericParameter.Name,
                                Ordinal = genericParameter.Ordinal,
                                Attributes = genericParameter.Attributes
                            };

                    foreach ( GenericParameterConstraintDeclaration constraint in genericParameter.Constraints )
                    {
                        newGenericParameter.Constraints.Add( constraint.ConstraintType.Translate( module ) );
                    }

                    newMethod.GenericParameters.Add( newGenericParameter );
                }

                GenericMap genericMap =
                    new GenericMap( composedInterfaceType.GetGenericContext( GenericContextOptions.None ),
                                    targetMethodDef.GenericParameters );

                // Create the method body.
                MethodBodyDeclaration methodBody = newMethod.MethodBody;
                InstructionBlock block = methodBody.CreateInstructionBlock();
                methodBody.RootInstructionBlock = block;
                InstructionSequence sequence = methodBody.CreateInstructionSequence();
                block.AddInstructionSequence( sequence, NodePosition.After, null );
                instructionWriter.AttachInstructionSequence( sequence );
                instructionWriter.EmitInstruction( OpCodeNumber.Ldarg_0 );
                instructionWriter.EmitInstructionField( OpCodeNumber.Ldfld, this.implementationFieldRef );
                for ( int i = 0; i < targetMethodDef.Parameters.Count; i++ )
                {
                    ParameterDeclaration targetParameter = targetMethodDef.Parameters[i];

                    ITypeSignature parameterType = targetParameter.ParameterType.Translate( module );
                    parameterType =
                        parameterType.MapGenericArguments( genericMap );

                    ParameterDeclaration newParameter = new ParameterDeclaration(
                        targetParameter.Ordinal, targetParameter.Name, parameterType
                        ) {Attributes = targetParameter.Attributes};

                    newMethod.Parameters.Add( newParameter );
                    instructionWriter.EmitInstructionParameter( OpCodeNumber.Ldarg, newParameter );
                }
                newMethod.ReturnParameter = new ParameterDeclaration
                                                {
                                                    Attributes = targetMethodDef.ReturnParameter.Attributes
                                                };
                if ( targetMethodDef.ReturnParameter.ParameterType != null )
                {
                    newMethod.ReturnParameter.ParameterType =
                        targetMethodDef.ReturnParameter.ParameterType.
                            Translate( module ).MapGenericArguments( genericMap );
                }


                instructionWriter.EmitPrefix( InstructionPrefixes.Tail );
                instructionWriter.EmitInstructionMethod( OpCodeNumber.Callvirt, targetMethodSpec );
                instructionWriter.EmitInstruction( OpCodeNumber.Ret );
                instructionWriter.DetachInstructionSequence();
            }

            // Recurse on inherited interfaces.
            foreach (
                InterfaceImplementationDeclaration interfaceImpl in composedInterfaceTypeDef.InterfaceImplementations )
            {
                ITypeSignature inheritedInterface = interfaceImpl.ImplementedInterface;

                ITypeSignature translatedInheritedInterface =
                    inheritedInterface.Translate( module ).MapGenericArguments(
                        composedInterfaceType.GetGenericContext( GenericContextOptions.None ) );
                IType translatedInheritedInterfaceType = translatedInheritedInterface as IType ??
                                                         module.TypeSpecs.GetBySignature( translatedInheritedInterface,
                                                                                          true );

                if ( !alreadyImplementedInterfaces.Contains( translatedInheritedInterfaceType ) )
                {
                    this.ImplementRecursive(
                        translatedInheritedInterfaceType,
                        alreadyImplementedInterfaces );
                }
            }
        }

        #region IInstanceInitializationClient Members

        void IInstanceInitializationClient.Emit(InstructionWriter writer, InstructionBlock block)
        {
            InstructionSequence sequence = block.MethodBody.CreateInstructionSequence();
            block.AddInstructionSequence(sequence, NodePosition.After, null);
            writer.AttachInstructionSequence(sequence);
            writer.EmitInstruction(OpCodeNumber.Ldarg_0);
            writer.EmitInstructionField(OpCodeNumber.Ldsfld, this.AspectRuntimeInstanceField);
            writer.EmitInstruction(OpCodeNumber.Ldarg_0);
            writer.EmitInstructionMethod(OpCodeNumber.Newobj,
                                          this.instanceBoundLaosEventArgsConstructor);
            writer.EmitInstruction(OpCodeNumber.Dup);
            writer.EmitInstruction(OpCodeNumber.Ldarg_0);
            writer.EmitInstructionMethod(OpCodeNumber.Call,
                                          this.Task.InstanceCredentialsManager.
                                              GetGetInstanceCredentialsMethod(this.targetType));
            writer.EmitInstructionMethod(OpCodeNumber.Call,
                                          this.instanceBoundLaosEventArgsSetInstanceCredentials);
            writer.EmitInstructionMethod(OpCodeNumber.Callvirt,
                                          this.composionAspectCreateImplementationObject);
            writer.EmitInstructionType(OpCodeNumber.Castclass, this.implementationFieldRef.FieldType);
            writer.EmitInstructionField(OpCodeNumber.Stfld, this.implementationFieldRef);
            writer.DetachInstructionSequence();
        }

        int IInstanceInitializationClient.Priority
        {
            get { return this.AspectPriority; }
        }

        #endregion
    }
}