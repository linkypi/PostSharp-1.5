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
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.Extensibility.Tasks;
using PostSharp.PlatformAbstraction;

namespace PostSharp.Laos.Weaver
{
    internal class OnMethodInvocationAspectWeaver : MethodLevelAspectWeaver, IMethodLevelAdvice
    {
        private static readonly Guid insteadOfMethodCallWrappersGuid =
            new Guid( "{1BB038B9-49CC-4297-82FA-E201B95DAD8C}" );

        private IMethod onMethodInvocationAspect_Invoke;
        private IMethod methodInvocationEventArgsConstructor;
        private IMethod methodInvocationEventArgs_getReturnValue;
        private MethodSignature delegateConstructorSignature;
        private Dictionary<TypeDefDeclaration, Dictionary<IMethod, WrapperPair>> wrappers;
        private OnMethodInvocationAspectOptions options;
        private OnMethodInvocationAspectOptions weaveSite;
        private MethodDefDeclaration targetMethodDef;

        private static readonly OnMethodInvocationAspectConfigurationAttribute defaultConfiguration =
            new OnMethodInvocationAspectConfigurationAttribute();

        public OnMethodInvocationAspectWeaver() : base( defaultConfiguration )
        {
        }

        protected internal override void OnTargetAssigned( bool reassigned )
        {
            base.OnTargetAssigned( reassigned );

            this.targetMethodDef = this.TargetMethod.GetMethodDefinition();

            if ( this.weaveSite == OnMethodInvocationAspectOptions.WeaveSiteAuto )
            {
                this.weaveSite = targetMethodDef.Module == this.Task.Project.Module && 
                    ((targetMethodDef.Attributes & MethodAttributes.Abstract) == 0)
                                     ? OnMethodInvocationAspectOptions.WeaveSiteTarget
                                     : OnMethodInvocationAspectOptions.WeaveSiteCall;
            }
        }

        public override void Implement()
        {
            base.Implement();

            switch ( weaveSite )
            {
                case OnMethodInvocationAspectOptions.WeaveSiteCall:
                    if ( this.targetMethodDef.IsVirtual )
                    {
                        LaosMessageSource.Instance.Write( SeverityType.Warning,
                            "LA0056", new object[] { this.targetMethodDef});
                    }
                    this.Task.MethodLevelAdvices.Add( this );
                    break;

                case OnMethodInvocationAspectOptions.WeaveSiteTarget:
                    this.ImplementTargetSite();
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( this.weaveSite, "weaveSite" );
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            ModuleDeclaration module = this.Task.Project.Module;


            this.onMethodInvocationAspect_Invoke = module.Cache.GetItem(
                () => module.FindMethod(
                          module.GetTypeForFrameworkVariant( typeof(IOnMethodInvocationAspect) ),
                          "OnInvocation" ) );

            this.delegateConstructorSignature = module.Cache.GetItem(
                delegate
                    {
                        MethodSignature signature =
                            new MethodSignature( module )
                                {
                                    CallingConvention =
                                        CallingConvention.HasThis
                                };
                        signature.ParameterTypes.Add(
                            module.Cache.GetIntrinsic(
                                IntrinsicType.Object ) );
                        signature.ParameterTypes.Add(
                            module.Cache.GetIntrinsic(
                                IntrinsicType.IntPtr ) );
                        return signature;
                    } );

            this.methodInvocationEventArgsConstructor = module.Cache.GetItem(
                () => module.FindMethod(
                          module.GetTypeForFrameworkVariant( typeof(MethodInvocationEventArgs) ), ".ctor" ) );

            this.methodInvocationEventArgs_getReturnValue = module.Cache.GetItem(
                () => module.FindMethod(
                          module.GetTypeForFrameworkVariant( typeof(MethodInvocationEventArgs) ),
                          "get_ReturnValue" ) );

            this.wrappers = (Dictionary<TypeDefDeclaration, Dictionary<IMethod, WrapperPair>>)
                            this.Task.Tags.GetTag( insteadOfMethodCallWrappersGuid );

            if ( this.wrappers == null )
            {
                this.wrappers = new Dictionary<TypeDefDeclaration, Dictionary<IMethod, WrapperPair>>();
                this.Task.Tags.SetTag( insteadOfMethodCallWrappersGuid, this.wrappers );
            }


            this.options =
                this.GetConfigurationValue<IOnMethodInvocationAspectConfiguration, OnMethodInvocationAspectOptions>(
                    c => c.GetOptions() );
            this.weaveSite = this.options & OnMethodInvocationAspectOptions.WeaveSiteMask;
        }


        public override bool ValidateSelf()
        {
            if ( !base.ValidateSelf() )
                return false;


            MethodDefDeclaration methodDef = this.TargetElement as MethodDefDeclaration;

            // Cannot make target-site weaving on abstract methods.
            if ( methodDef != null && ( ( methodDef.Attributes & MethodAttributes.Abstract ) != 0 ||
                                        ( methodDef.ImplementationAttributes & MethodImplAttributes.ManagedMask ) !=
                                        MethodImplAttributes.Managed ) &&
                 this.weaveSite == OnMethodInvocationAspectOptions.WeaveSiteTarget )
            {
                LaosMessageSource.Instance.Write( SeverityType.Error, "LA0015",
                                                  new object[] {this.GetAspectTypeName(), methodDef.ToString()} );
                return false;
            }

            IMethod method = this.TargetMethod;

            // Cannot be applied to constructors.
            if ( method.Name == ".ctor" || method.Name == ".cctor" )
            {
                LaosMessageSource.Instance.Write( SeverityType.Error, "LA0018",
                                                  new object[] {this.GetAspectTypeName(), methodDef.ToString()} );
            }

            return true;
        }

        public override void ValidateInteractions( LaosAspectWeaver[] weaversOnSameTarget )
        {
            base.ValidateInteractions( weaversOnSameTarget );

            if ( this.weaveSite == OnMethodInvocationAspectOptions.WeaveSiteCall )
            {
                // We should check that we are the only aspeect of type OnMethodInvocation.
                for ( int i = 0; i < weaversOnSameTarget.Length; i++ )
                {
                    OnMethodInvocationAspectWeaver weaver = weaversOnSameTarget[i] as OnMethodInvocationAspectWeaver;
                    if ( weaver != null && weaver != this )
                    {
                        LaosMessageSource.Instance.Write(
                            SeverityType.Error, "LA0012", new object[] {this.TargetElement.ToString()} );
                        return;
                    }
                }
            }
        }

        private MethodDefDeclaration GetCachedWrapper( TypeDefDeclaration containerType, bool virtualCall )
        {
            Dictionary<IMethod, WrapperPair> wrapperPairsInContainer;
            if ( !this.wrappers.TryGetValue( containerType, out wrapperPairsInContainer ) )
            {
                return null;
            }

            WrapperPair wrapperPair;
            if ( !wrapperPairsInContainer.TryGetValue( targetMethodDef, out wrapperPair ) )
            {
                return null;
            }

            return virtualCall ? wrapperPair.VirtualWrapper : wrapperPair.NonVirtualWrapper;
        }

        private void SetCachedWrapper( TypeDefDeclaration containerType, bool virtualCall, MethodDefDeclaration wrapper )
        {
            Dictionary<IMethod, WrapperPair> wrapperPairsInContainer;
            if ( !this.wrappers.TryGetValue( containerType, out wrapperPairsInContainer ) )
            {
                wrapperPairsInContainer = new Dictionary<IMethod, WrapperPair>();
                this.wrappers.Add( containerType, wrapperPairsInContainer );
            }

            WrapperPair wrapperPair;
            if ( !wrapperPairsInContainer.TryGetValue( targetMethodDef, out wrapperPair ) )
            {
                wrapperPair = new WrapperPair();
                wrapperPairsInContainer.Add( targetMethodDef, wrapperPair );
            }

            if ( virtualCall )
                wrapperPair.VirtualWrapper = wrapper;
            else
                wrapperPair.NonVirtualWrapper = wrapper;
        }

        private void MoveImplementationBoundCustomAttributes( MetadataDeclaration source, MetadataDeclaration target )
        {
            if ( source.CustomAttributes.Count > 0 )
            {
                ImplementationBoundAttributesTask implementationBoundAttributesTask =
                    ImplementationBoundAttributesTask.GetTask( this.Task.Project );

                foreach ( CustomAttributeDeclaration attribute in source.CustomAttributes )
                {
                    if ( implementationBoundAttributesTask.IsImplementationBound( attribute.Constructor.DeclaringType ) )
                    {
                        attribute.Remove();
                        target.CustomAttributes.Add( attribute );
                    }
                }
            }
        }

        private void CreateMethodDeclaration( TypeDefDeclaration containerType,
                                              ITypeSignature calledMethodDeclaringTypeSpec,
                                              GenericMap callerToWrapperGenericMap,
                                              GenericMap callerToWrapperInverseGenericMap, string prefix,
                                              bool forcePrivate, bool forceStatic, bool moveCustomAttributes,
                                              bool hasDelocalizedImplementation, out int parameterOrdinalShift,
                                              out MethodDefDeclaration wrapperMethod )
        {
            ModuleDeclaration module = this.Task.Project.Module;

            wrapperMethod = new MethodDefDeclaration();
            if ( !hasDelocalizedImplementation )
            {
                wrapperMethod.Name = Platform.Current.NormalizeCilIdentifier( prefix + targetMethodDef.Name );
                const MethodAttributes allowedAttributes = ~( MethodAttributes.Final | MethodAttributes.Virtual |
                                                              MethodAttributes.NewSlot );
                if ( forcePrivate )
                {
                    wrapperMethod.Attributes = ( ( this.targetMethodDef.Attributes & ~MethodAttributes.MemberAccessMask ) &
                                                 allowedAttributes ) | MethodAttributes.Private;
                    wrapperMethod.CallingConvention = CallingConvention.Default;
                }
                else
                {
                    wrapperMethod.Attributes = ( this.targetMethodDef.Attributes & allowedAttributes ) &
                                               ~MethodAttributes.MemberAccessMask;
                    MethodAttributes originalVisibility = this.targetMethodDef.Attributes &
                                                          MethodAttributes.MemberAccessMask;

                    switch ( originalVisibility )
                    {
                        case MethodAttributes.Assembly:
                        case MethodAttributes.FamANDAssem:
                        case MethodAttributes.Private:
                        case MethodAttributes.PrivateScope:
                            wrapperMethod.Attributes |= originalVisibility;
                            break;

                        case MethodAttributes.Public:
                        case MethodAttributes.FamORAssem:
                            wrapperMethod.Attributes |= MethodAttributes.Assembly;
                            break;

                        default:
                            throw ExceptionHelper.CreateInvalidEnumerationValueException(
                                originalVisibility,
                                "originalVisibility" );
                    }
                }

                wrapperMethod.CallingConvention = this.targetMethodDef.CallingConvention;
            }
            else
            {
                wrapperMethod.Name =
                    Platform.Current.NormalizeCilIdentifier( prefix + targetMethodDef.DeclaringType.Name + "~" +
                                                             targetMethodDef.Name );
                wrapperMethod.Attributes = MethodAttributes.Assembly;
                if ( forceStatic )
                {
                    wrapperMethod.Attributes |= MethodAttributes.Static;
                    wrapperMethod.CallingConvention = CallingConvention.Default;
                }
                else
                {
                    wrapperMethod.CallingConvention = this.targetMethodDef.CallingConvention;
                }
            }

            containerType.Methods.Add( wrapperMethod );


            if ( moveCustomAttributes )
            {
                this.MoveImplementationBoundCustomAttributes( this.targetMethodDef, wrapperMethod );
            }

            CodeWeaver.Weaver.IgnoreMethod( wrapperMethod );

            wrapperMethod.ReturnParameter = this.targetMethodDef.ReturnParameter.Clone( module );
            if ( !callerToWrapperInverseGenericMap.IsEmpty )
            {
                wrapperMethod.ReturnParameter.ParameterType =
                    wrapperMethod.ReturnParameter.ParameterType.MapGenericArguments( callerToWrapperGenericMap );
            }
            if ( moveCustomAttributes )
            {
                this.MoveImplementationBoundCustomAttributes( this.targetMethodDef.ReturnParameter,
                                                              wrapperMethod.ReturnParameter );
            }

            wrapperMethod.Parameters.EnsureCapacity( this.targetMethodDef.Parameters.Count + 1 );

            // If it is a global method but an instance one, we have to add a parameter for the instance.
            if ( forceStatic && !this.targetMethodDef.IsStatic )
            {
                ParameterDeclaration myParameter = new ParameterDeclaration
                                                       {
                                                           Attributes = ParameterAttributes.In,
                                                           Name = Platform.Current.NormalizeCilIdentifier( "~this" ),
                                                           ParameterType = calledMethodDeclaringTypeSpec
                                                       };
                wrapperMethod.Parameters.Add( myParameter );
                parameterOrdinalShift = 1;
            }
            else
            {
                parameterOrdinalShift = 0;
            }

            // Copy parameters.
            foreach ( ParameterDeclaration invokeMethodParameter in this.targetMethodDef.Parameters )
            {
                ParameterDeclaration myParameter = invokeMethodParameter.Clone( module );
                myParameter.Ordinal = parameterOrdinalShift + invokeMethodParameter.Ordinal;
                if ( !callerToWrapperInverseGenericMap.IsEmpty )
                {
                    myParameter.ParameterType =
                        myParameter.ParameterType.MapGenericArguments( callerToWrapperGenericMap );
                }
                wrapperMethod.Parameters.Add( myParameter );

                if ( moveCustomAttributes )
                {
                    this.MoveImplementationBoundCustomAttributes( invokeMethodParameter, myParameter );
                }
            }

            int genericParameterCursor = 0;

            // If the aspected method does not belong to the current module, we have to copy the type
            // arguments as method arguments.
            if ( hasDelocalizedImplementation && this.targetMethodDef.DeclaringType.IsGenericDefinition )
            {
                foreach ( GenericParameterDeclaration originalGenericParameter in
                    this.targetMethodDef.DeclaringType.GenericParameters )
                {
                    GenericParameterDeclaration genericParameter = originalGenericParameter.Clone( module,
                                                                                                   callerToWrapperGenericMap );
                    genericParameter.Ordinal = genericParameterCursor;
                    genericParameter.Kind = GenericParameterKind.Method;
                    wrapperMethod.GenericParameters.Add( genericParameter );
                    genericParameter.Constraints.AddRangeClone( originalGenericParameter.Constraints,
                                                           callerToWrapperGenericMap );
                    genericParameterCursor++;
                }
            }

            // Copy method generic parameters.
            foreach ( GenericParameterDeclaration originalGenericParameter in this.targetMethodDef.GenericParameters )
            {
                GenericParameterDeclaration genericParameter = originalGenericParameter.Clone( module,
                                                                                               callerToWrapperGenericMap );
                genericParameter.Ordinal = genericParameterCursor;
                genericParameter.Kind = GenericParameterKind.Method;
                wrapperMethod.GenericParameters.Add( genericParameter );
                genericParameter.Constraints.AddRangeClone( originalGenericParameter.Constraints, callerToWrapperGenericMap );
                genericParameterCursor++;
            }
        }

        private void GetCalledMethodSpec( GenericMap callerToWrapperGenericMap, out IMethod calledMethodSpec,
                                          out IType calledMethodDeclaringTypeSpec )
        {
            ModuleDeclaration module = this.Task.Project.Module;

            TypeDefDeclaration calledMethodDeclaringType = this.targetMethodDef.DeclaringType;

            if ( calledMethodDeclaringType.IsGenericDefinition || this.targetMethodDef.IsGenericDefinition )
            {
                IGenericMethodDefinition calledMethodRef;

                if ( calledMethodDeclaringType.IsGenericDefinition )
                {
                    calledMethodDeclaringTypeSpec = module.TypeSpecs.GetBySignature(
                        new GenericTypeInstanceTypeSignature(
                            (INamedType) calledMethodDeclaringType.Translate( module ),
                            callerToWrapperGenericMap.GetGenericTypeParameters() ), true );

                    calledMethodRef = (IGenericMethodDefinition) calledMethodDeclaringTypeSpec.Methods.GetMethod(
                                                                     this.targetMethodDef.Name,
                                                                     this.targetMethodDef.Translate( module ),
                                                                     BindingOptions.Default );
                }
                else
                {
                    calledMethodRef = (IGenericMethodDefinition) this.targetMethodDef.Translate( module );
                    calledMethodDeclaringTypeSpec = calledMethodRef.DeclaringType;
                }

                if ( this.targetMethodDef.IsGenericDefinition )
                {
                    calledMethodSpec = calledMethodRef.MethodSpecs.GetGenericInstance(
                        callerToWrapperGenericMap.GetGenericMethodParameters(), true );
                }
                else
                {
                    calledMethodSpec = calledMethodRef;
                }
            }
            else
            {
                calledMethodSpec = this.targetMethodDef.Translate( module );
                calledMethodDeclaringTypeSpec = calledMethodSpec.DeclaringType;
            }
        }

        private void ImplementWrapperMethodBody( MethodDefDeclaration wrapperMethod, IMethod calledMethodSpec,
                                                 IType delegateType, int parameterOrdinalShift, bool virtualCall,
                                                 InstructionWriter instructionWriter )
        {
            int argumentCount = wrapperMethod.Parameters.Count;

            wrapperMethod.CustomAttributes.Add( this.Task.WeavingHelper.GetDebuggerNonUserCodeAttribute() );
            this.Task.WeavingHelper.AddCompilerGeneratedAttribute( wrapperMethod.CustomAttributes );

            //MethodDefDeclaration delegateInvokeMethod = delegateTypeDef.Methods.GetOneByName("Invoke");
            ModuleDeclaration module = this.Task.Project.Module;

            // Define the method body.
            wrapperMethod.MethodBody = new MethodBodyDeclaration();
            InstructionBlock block = wrapperMethod.MethodBody.CreateInstructionBlock();
            wrapperMethod.MethodBody.RootInstructionBlock = block;
            InstructionSequence sequence = wrapperMethod.MethodBody.CreateInstructionSequence();
            wrapperMethod.MethodBody.RootInstructionBlock.AddInstructionSequence( sequence, NodePosition.Before, null );
            instructionWriter.AttachInstructionSequence( sequence );

            #region Emit instructions

            #region Instantiate the delegate.

            IMethod delegateConstructor = delegateType.Methods.GetMethod(
                ".ctor", this.delegateConstructorSignature, BindingOptions.Default );

            //    instance void  .ctor(object 'object', native int 'method') runtime managed

            // Push the instance on the stack.
            if ( ( this.targetMethodDef.Attributes & MethodAttributes.Static ) != 0 )
            {
                instructionWriter.EmitInstruction( OpCodeNumber.Ldnull );
            }
            else
            {
                instructionWriter.EmitInstruction( OpCodeNumber.Ldarg_0 );
                if ( this.targetMethodDef.DeclaringType.BelongsToClassification( TypeClassifications.ValueType ) )
                {
                    // Value type.
                    ITypeSignature valueType = this.targetMethodDef.DeclaringType.Translate( module );
                    instructionWriter.EmitInstructionLoadIndirect( valueType );
                    instructionWriter.EmitInstructionType( OpCodeNumber.Box, valueType );
                }
            }

            // Get a pointer to the called method.
            if ( virtualCall )
            {
                instructionWriter.EmitInstruction( OpCodeNumber.Dup );
                instructionWriter.EmitInstructionMethod( OpCodeNumber.Ldvirtftn, calledMethodSpec );
                instructionWriter.EmitInstructionMethod( OpCodeNumber.Newobj, delegateConstructor );
            }
            else
            {
                instructionWriter.EmitInstructionMethod( OpCodeNumber.Ldftn, calledMethodSpec );
                instructionWriter.EmitInstructionMethod( OpCodeNumber.Newobj, delegateConstructor );
            }


            // Store the delegate in a variable.
            LocalVariableSymbol delegateVariable = wrapperMethod.MethodBody.RootInstructionBlock.DefineLocalVariable(
                module.Cache.GetType( "System.Delegate, mscorlib" ), "delegateInstance" );
            instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Stloc, delegateVariable );

            #endregion

            // Push the custom attribute instance on the stack.
            instructionWriter.EmitInstructionField( OpCodeNumber.Ldsfld, this.AspectRuntimeInstanceField );

            #region Build the EventArgs

            LocalVariableSymbol eventArgsLocal = wrapperMethod.MethodBody.RootInstructionBlock.DefineLocalVariable(
                module.GetTypeForFrameworkVariant( typeof(MethodInvocationEventArgs) ), "eventArgs" );
            // Load the delegate instance.
            instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, delegateVariable );

            // Push the array of arguments on the stack.
            LocalVariableSymbol arrayOfArguments;
            bool hasOutParameter = false;
            if ( this.targetMethodDef.Parameters.Count > 0 )
            {
                // Determine whether the method has out parameters.
                foreach ( ParameterDeclaration parameter in this.targetMethodDef.Parameters )
                {
                    if ( parameter.ParameterType.BelongsToClassification( TypeClassifications.Pointer ) )
                    {
                        hasOutParameter = true;
                        break;
                    }
                }

                if ( hasOutParameter )
                {
                    // Define a local variable to store the array of arguments.
                    arrayOfArguments =
                        block.DefineLocalVariable(
                            new ArrayTypeSignature( module.Cache.GetIntrinsic( IntrinsicType.Object ) ), "args" );

                    wrapperMethod.MethodBody.InitLocalVariables = true;
                }
                else
                {
                    arrayOfArguments = null;
                }


                this.Task.WeavingHelper.MakeArrayOfArguments( wrapperMethod, instructionWriter, parameterOrdinalShift,
                                                              argumentCount );

                if ( hasOutParameter )
                {
                    // Store the array in a local variable.
                    instructionWriter.EmitInstruction( OpCodeNumber.Dup );
                    instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Stloc, arrayOfArguments );
                }
            }
            else
            {
                arrayOfArguments = null;
                instructionWriter.EmitInstruction( OpCodeNumber.Ldnull );
            }

            instructionWriter.EmitInstructionMethod( OpCodeNumber.Newobj, this.methodInvocationEventArgsConstructor );
            instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Stloc, eventArgsLocal );

            #endregion

            // Call the method.
            instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, eventArgsLocal );
            instructionWriter.EmitInstructionMethod( OpCodeNumber.Callvirt, this.onMethodInvocationAspect_Invoke );

            // Store the array values in output arguments.
            if ( hasOutParameter )
            {
                this.Task.WeavingHelper.CopyArgumentsFromArray( arrayOfArguments, wrapperMethod, instructionWriter,
                                                                parameterOrdinalShift );
            }

            // Cast the return value into the correct return type.
            ITypeSignature returnType = wrapperMethod.ReturnParameter.ParameterType;
            if ( !IntrinsicTypeSignature.Is( returnType, IntrinsicType.Void ) )
            {
                instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, eventArgsLocal );
                instructionWriter.EmitInstructionMethod( OpCodeNumber.Call,
                                                         this.methodInvocationEventArgs_getReturnValue );
                this.Task.WeavingHelper.FromObject( returnType, instructionWriter );
            }

            // Return.
            instructionWriter.EmitInstruction( OpCodeNumber.Ret );

            #endregion

            instructionWriter.DetachInstructionSequence();
        }

        private MethodDefDeclaration ImplementCallSite( TypeDefDeclaration containerType, IType delegateType,
                                                        GenericMap callerToWrapperGenericMap,
                                                        GenericMap callerToWrapperInverseGenericMap,
                                                        InstructionWriter instructionWriter,
                                                        bool hasDelocalizedImplementation, bool forceStatic,
                                                        bool virtualCall )
        {
            IMethod calledMethodSpec;
            IType calledMethodDeclaringTypeSpec;
            MethodDefDeclaration wrapperMethod;
            int parameterOrdinalShift;

            this.GetCalledMethodSpec( callerToWrapperGenericMap,
                                      out calledMethodSpec,
                                      out calledMethodDeclaringTypeSpec );

            this.CreateMethodDeclaration( containerType,
                                          calledMethodDeclaringTypeSpec,
                                          callerToWrapperGenericMap,
                                          callerToWrapperInverseGenericMap,
                                          virtualCall ? "~instead~of~virtual~" : "~instead~of~",
                                          false,
                                          forceStatic,
                                          false,
                                          hasDelocalizedImplementation,
                                          out parameterOrdinalShift,
                                          out wrapperMethod );

            this.ImplementWrapperMethodBody(
                wrapperMethod,
                calledMethodSpec,
                delegateType,
                parameterOrdinalShift, virtualCall,
                instructionWriter );

            return wrapperMethod;
        }

        private DelegateAndMappings GetDelegateAndMappings( IMethod calledMethod, bool hasDelocalizedImplementation )
        {
            DelegateAndMappings result = new DelegateAndMappings();
            ModuleDeclaration module = calledMethod.Module;

            // Get the delegate. In case of generics, the method returns a TypeSpec
            // where generic arguments are valid in the context of the caller (not the wrapper!).
            DelegateMap delegateMap = this.Task.DelegateMapper.GetDelegateMap( calledMethod );
            result.DelegateType = delegateMap.DelegateTypeSpec;


            GenericTypeInstanceTypeSignature outerDelegateGenericInstance =
                result.DelegateType as GenericTypeInstanceTypeSignature;


            if ( outerDelegateGenericInstance != null )
            {
                // Build generic mappings.

                IList<ITypeSignature> callerToWrapperGenericTypeParameters;
                IList<ITypeSignature> callerToWrapperGenericMethodParameters;
                IList<ITypeSignature> callerToWrapperInverseGenericMethodParameters;
                IList<ITypeSignature> callerToWrapperInverseGenericTypeParameters;

                if ( hasDelocalizedImplementation )
                {
                    callerToWrapperInverseGenericMethodParameters =
                        new ITypeSignature[outerDelegateGenericInstance.GenericArguments.Count];
                    ITypeSignature[] wrapperToDelegateGenericMethodParameters =
                        new ITypeSignature[outerDelegateGenericInstance.GenericArguments.Count];
                    ITypeSignature[] wrapperToDelegateInverseGenericTypeParameters =
                        new ITypeSignature[outerDelegateGenericInstance.GenericArguments.Count];
                    callerToWrapperInverseGenericTypeParameters = null;
                    ITypeSignature[] wrapperToDelegateInverseGenericMethodParameters = null;

                    if ( targetMethodDef.DeclaringType.IsGenericDefinition )
                    {
                        callerToWrapperGenericTypeParameters =
                            new ITypeSignature[targetMethodDef.DeclaringType.GenericParameters.Count];

                        for ( int i = 0; i < callerToWrapperGenericTypeParameters.Count; i++ )
                        {
                            callerToWrapperGenericTypeParameters[i] =
                                module.Cache.GetGenericParameter( i, GenericParameterKind.Method );
                            callerToWrapperInverseGenericMethodParameters[i] =
                                module.Cache.GetGenericParameter( i, GenericParameterKind.Type );
                            wrapperToDelegateGenericMethodParameters[i] =
                                module.Cache.GetGenericParameter( i, GenericParameterKind.Type );
                            wrapperToDelegateInverseGenericTypeParameters[i] =
                                module.Cache.GetGenericParameter( i, GenericParameterKind.Method );
                        }
                    }
                    else
                    {
                        callerToWrapperGenericTypeParameters = null;
                    }

                    if ( targetMethodDef.IsGenericDefinition )
                    {
                        callerToWrapperGenericMethodParameters =
                            new ITypeSignature[targetMethodDef.GenericParameters.Count];
                        int shift = callerToWrapperGenericTypeParameters == null
                                        ? 0
                                        : callerToWrapperGenericTypeParameters.Count;

                        for ( int i = 0; i < callerToWrapperGenericMethodParameters.Count; i++ )
                        {
                            callerToWrapperGenericMethodParameters[i] =
                                module.Cache.GetGenericParameter( i + shift, GenericParameterKind.Method );
                            callerToWrapperInverseGenericMethodParameters[i + shift] =
                                module.Cache.GetGenericParameter( i, GenericParameterKind.Method );
                            wrapperToDelegateGenericMethodParameters[i + shift] =
                                module.Cache.GetGenericParameter( i + shift, GenericParameterKind.Type );
                            wrapperToDelegateInverseGenericTypeParameters[i + shift] =
                                module.Cache.GetGenericParameter( i + shift, GenericParameterKind.Method );
                        }
                    }
                    else
                    {
                        callerToWrapperGenericMethodParameters = null;
                    }

                    result.WrapperToDelegateInverseGenericMap =
                        new GenericMap( wrapperToDelegateInverseGenericTypeParameters,
                                        wrapperToDelegateInverseGenericMethodParameters );
                }
                else
                {
                    // Caller-->Wrapper is identity.
                    if ( targetMethodDef.DeclaringType.IsGenericDefinition )
                    {
                        callerToWrapperGenericTypeParameters =
                            module.Cache.GetGenericParameterArray(
                                targetMethodDef.DeclaringType.GenericParameters.Count, GenericParameterKind.Type );
                        callerToWrapperInverseGenericTypeParameters = callerToWrapperGenericTypeParameters;
                    }
                    else
                    {
                        callerToWrapperGenericTypeParameters = null;
                        callerToWrapperInverseGenericTypeParameters = null;
                    }

                    if ( targetMethodDef.IsGenericDefinition )
                    {
                        callerToWrapperGenericMethodParameters =
                            module.Cache.GetGenericParameterArray( targetMethodDef.GenericParameters.Count,
                                                                   GenericParameterKind.Method );
                        callerToWrapperInverseGenericMethodParameters = callerToWrapperGenericMethodParameters;
                    }
                    else
                    {
                        callerToWrapperGenericMethodParameters = null;
                        callerToWrapperInverseGenericMethodParameters = null;
                    }

                    result.WrapperToDelegateInverseGenericMap = delegateMap.CallerToDelegateInverseGenericMap;
                }

                result.CallerToWrapperGenericMap =
                    new GenericMap( callerToWrapperGenericTypeParameters, callerToWrapperGenericMethodParameters );
                result.CallerToWrapperInverseGenericMap =
                    new GenericMap( callerToWrapperInverseGenericTypeParameters,
                                    callerToWrapperInverseGenericMethodParameters );
            }
            else
            {
                result.CallerToWrapperGenericMap = GenericMap.Empty;
                result.CallerToWrapperInverseGenericMap = GenericMap.Empty;
                result.WrapperToDelegateInverseGenericMap = GenericMap.Empty;
            }

            return result;
        }

        private void ImplementTargetSite()
        {
            DelegateAndMappings delegateAndMappings = GetDelegateAndMappings(
                GenericHelper.GetMethodCanonicalGenericInstance( this.targetMethodDef ), false );

            // Create the new method, containing the original implementation,
            // and move the method body.
            MethodDefDeclaration wrapperMethod;
            int parameterOrdinalShift;
            this.CreateMethodDeclaration(
                this.targetMethodDef.DeclaringType, null, delegateAndMappings.CallerToWrapperGenericMap,
                delegateAndMappings.CallerToWrapperInverseGenericMap, "~",
                true,
                false,
                true,
                false,
                out parameterOrdinalShift,
                out wrapperMethod );


            MethodBodyDeclaration originalMethodBody = this.targetMethodDef.MethodBody;
            this.targetMethodDef.MethodBody = null;
            wrapperMethod.MethodBody = originalMethodBody;
            IndexUsagesTask.MoveMethodBody( this.targetMethodDef, wrapperMethod );

            // Implement the original method differently.
            IType delegateType = delegateAndMappings.DelegateType as IType ??
                                 this.Task.Project.Module.TypeSpecs.GetBySignature(
                                     delegateAndMappings.DelegateType, true );
            this.ImplementWrapperMethodBody( this.targetMethodDef,
                                             GenericHelper.GetMethodCanonicalGenericInstance( wrapperMethod ),
                                             delegateType,
                                             parameterOrdinalShift,
                                             false,
                                             this.Task.InstructionWriter );

            this.SetOwnTargetRedirection( wrapperMethod );
        }


        internal IMethod GetCallSiteWrapperMethodSpec( IMethod targetMethodSpec, IField customAttributeInstanceField,
                                                       InstructionWriter instructionWriter,
                                                       TypeDefDeclaration originatingType, bool virtualCall )
        {
            bool forceStatic = false;
            GenericMap wrapperMethodGenericMap = GenericMap.Empty;

            ModuleDeclaration module = this.Task.Project.Module;

            TypeDefDeclaration wrapperMethodDeclaringTypeDef;

            // Decide who should contain the wrapper.
            bool hasDelocalizedImplementation = targetMethodDef.Module != module ||
                                                ( targetMethodDef.DeclaringType.Attributes & TypeAttributes.Interface ) !=
                                                0;

            bool isVirtual = ( this.targetMethodDef.Attributes & MethodAttributes.Virtual ) != 0;
            bool isSealed = ( this.targetMethodDef.Attributes & MethodAttributes.Final ) != 0;


            if ( hasDelocalizedImplementation )
            {
                MethodAttributes visibility = this.targetMethodDef.Attributes &
                                              MethodAttributes.MemberAccessMask;

                if ( visibility == MethodAttributes.Family ||
                     visibility == MethodAttributes.FamORAssem ||
                     ( !virtualCall && isVirtual && !isSealed ) )
                {
                    // The target method is a protected method, or this is a non-virtual call
                    // to a non-sealed virtual method.

                    // We should implement the method in the first child type that is defined in 
                    // the current module.

                    TypeDefDeclaration cursor = originatingType;

                    wrapperMethodDeclaringTypeDef = null;
                    wrapperMethodGenericMap = originatingType.GetGenericContext( GenericContextOptions.None );

                    while ( cursor != null )
                    {
                        wrapperMethodDeclaringTypeDef = cursor;
                        TypeSpecDeclaration typeSpec = cursor.BaseType as TypeSpecDeclaration;
                        if ( typeSpec != null )
                        {
                            GenericTypeInstanceTypeSignature genericTypeInstanceTypeSignature =
                                (GenericTypeInstanceTypeSignature) typeSpec.Signature;

                            cursor = genericTypeInstanceTypeSignature.GenericDefinition
                                     as TypeDefDeclaration;

                            wrapperMethodGenericMap =
                                genericTypeInstanceTypeSignature.GetGenericContext( GenericContextOptions.None ).Apply(
                                    wrapperMethodGenericMap );
                        }
                        else
                        {
                            cursor = cursor.BaseType as TypeDefDeclaration;
                        }
                    }
                }
                else
                {
                    forceStatic = true;
                    wrapperMethodDeclaringTypeDef = this.Task.ImplementationDetailsType;
                }
            }
            else
            {
                wrapperMethodDeclaringTypeDef = targetMethodDef.DeclaringType;
            }


            // Look for existing implementations.
            IMethod wrapperMethod;

            DelegateAndMappings delegateAndMappings = this.GetDelegateAndMappings(
                targetMethodSpec, hasDelocalizedImplementation );

            // If the delegate type is a generic construction, we should create a generic
            // method. We may already have the generic method.
            if ( delegateAndMappings.DelegateType.IsGenericInstance )
            {
                // If we have a TypeSpec, the generic arguments are the one
                // seen by the _calling_ methods, not by the wrapper,
                // that's why they it is called the "outer" signature.
                // The "inner" signature is the signature inside the wrapping method.

                #region Create a wrapper for the called method.

                MethodDefDeclaration wrapperMethodDef =
                    this.GetCachedWrapper( wrapperMethodDeclaringTypeDef, virtualCall );

                if ( wrapperMethodDef == null )
                {
                    // We don't have a wrapper for the called method.

                    TypeSpecDeclaration innerDelegateTypeSpec;

                    if ( hasDelocalizedImplementation )
                    {
                        innerDelegateTypeSpec =
                            module.TypeSpecs.GetBySignature(
                                new GenericTypeInstanceTypeSignature(
                                    ( (GenericTypeInstanceTypeSignature) delegateAndMappings.DelegateType ).
                                        GenericDefinition,
                                    delegateAndMappings.WrapperToDelegateInverseGenericMap.GetGenericTypeParameters() ),
                                true );
                    }
                    else
                    {
                        innerDelegateTypeSpec = module.TypeSpecs.GetBySignature(
                            delegateAndMappings.DelegateType, true );
                    }

                    wrapperMethodDef = this.ImplementCallSite(
                        wrapperMethodDeclaringTypeDef,
                        innerDelegateTypeSpec,
                        delegateAndMappings.CallerToWrapperGenericMap,
                        delegateAndMappings.CallerToWrapperInverseGenericMap,
                        instructionWriter,
                        hasDelocalizedImplementation,
                        forceStatic, virtualCall );

                    this.SetCachedWrapper( wrapperMethodDeclaringTypeDef, virtualCall, wrapperMethodDef );
                } // if (!methodsInContainer.TryGetValue(this.targetMethodDef, out wrapperMethodDef))

                #endregion

                #region Create the MethodSpec for the wrapper.

                // Create MethodSpec.
                // We have to resolve the generic
                // arguments of the delegate type signature with the generic context of
                // the called function.
                IGenericMethodDefinition wrapperMethodRef;
                GenericMap concreteWrapperGenericMap =
                    delegateAndMappings.CallerToWrapperInverseGenericMap.Apply(
                        targetMethodSpec.GetGenericContext( GenericContextOptions.None ) );

                if ( wrapperMethodDeclaringTypeDef.IsGenericDefinition )
                {
                    IType wrapperMethodDeclaringTypeSpec = module.TypeSpecs.GetBySignature(
                        new GenericTypeInstanceTypeSignature( wrapperMethodDeclaringTypeDef,
                                                              concreteWrapperGenericMap.GetGenericTypeParameters() ),
                        true );
                    wrapperMethodRef = (IGenericMethodDefinition) wrapperMethodDeclaringTypeSpec.Methods.GetMethod(
                                                                      wrapperMethodDef.Name, wrapperMethodDef,
                                                                      BindingOptions.Default );
                }
                else
                {
                    wrapperMethodRef = wrapperMethodDef;
                }

                if ( wrapperMethodDef.IsGenericDefinition )
                {
                    MethodSpecDeclaration wrapperMethodSpec = new MethodSpecDeclaration();
                    wrapperMethodSpec.GenericArguments.AddRange(
                        concreteWrapperGenericMap.GetGenericMethodParameters() );
                    wrapperMethodRef.MethodSpecs.Add( wrapperMethodSpec );
                    wrapperMethod = wrapperMethodSpec;
                }
                else
                {
                    wrapperMethod = wrapperMethodRef;
                }

                #endregion
            }
            else // DelegateAndMappings.DelegateType.IsGenericInstance
            {
                MethodDefDeclaration wrapperMethodDef =
                    this.GetCachedWrapper( wrapperMethodDeclaringTypeDef, virtualCall );

                if ( wrapperMethodDef == null )
                {
                    // The delegate is not a generic one.
                    wrapperMethodDef = this.ImplementCallSite(
                        wrapperMethodDeclaringTypeDef,
                        (INamedType) delegateAndMappings.DelegateType,
                        GenericMap.Empty,
                        GenericMap.Empty,
                        instructionWriter,
                        hasDelocalizedImplementation,
                        forceStatic, virtualCall );


                    // Cache the method.
                    this.SetCachedWrapper( wrapperMethodDeclaringTypeDef, virtualCall, wrapperMethodDef );
                }


                // Even if the target method is not generic, the wrapper may be defined
                // on a generic type.
                if ( !wrapperMethodGenericMap.IsEmpty )
                {
                    IType wrapperMethodDeclaringTypeSpec = module.TypeSpecs.GetBySignature(
                        new GenericTypeInstanceTypeSignature( wrapperMethodDeclaringTypeDef,
                                                              wrapperMethodGenericMap.GetGenericTypeParameters() ),
                        true );

                    wrapperMethod = wrapperMethodDeclaringTypeSpec.Methods.GetMethod(
                        wrapperMethodDef.Name, wrapperMethodDef,
                        BindingOptions.Default );
                }
                else
                {
                    wrapperMethod = wrapperMethodDef;
                }
            } // delegateAndMappings.DelegateType.IsGenericInstance


            return wrapperMethod;
        }

        private class WrapperPair
        {
            public MethodDefDeclaration VirtualWrapper;
            public MethodDefDeclaration NonVirtualWrapper;
        }

        private class DelegateAndMappings
        {
            public ITypeSignature DelegateType;
            public GenericMap CallerToWrapperGenericMap;
            public GenericMap CallerToWrapperInverseGenericMap;
            public GenericMap WrapperToDelegateInverseGenericMap;
        }

        #region IMethodLevelAdvice Members

        MethodDefDeclaration IMethodLevelAdvice.Method
        {
            get { return null; }
        }

        MetadataDeclaration IMethodLevelAdvice.Operand
        {
            get { return this.TargetElement; }
        }

        JoinPointKinds IMethodLevelAdvice.JoinPointKinds
        {
            get { return JoinPointKinds.InsteadOfCall; }
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
            // Get the method to be called instead.
            IMethod wrapperMethod = this.GetCallSiteWrapperMethodSpec( context.JoinPoint.Instruction.MethodOperand,
                                                                       this.AspectRuntimeInstanceField,
                                                                       context.InstructionWriter,
                                                                       context.Method.DeclaringType,
                                                                       context.JoinPoint.Instruction.OpCodeNumber ==
                                                                       OpCodeNumber.Callvirt );

            // Generate call instructions.
            InstructionSequence sequence = block.MethodBody.CreateInstructionSequence();
            block.AddInstructionSequence( sequence, NodePosition.Before, null );
            context.InstructionWriter.AttachInstructionSequence( sequence );
            context.InstructionWriter.EmitSymbolSequencePoint( context.JoinPoint.Instruction.SymbolSequencePoint );

            context.InstructionWriter.EmitInstructionMethod( OpCodeNumber.Call, wrapperMethod );
            context.InstructionWriter.DetachInstructionSequence();
        }

        #endregion
    }
}