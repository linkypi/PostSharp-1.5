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
using System.Globalization;
using System.Reflection;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.CodeWeaver;
using PostSharp.Collections;

namespace PostSharp.CodeModel.Helpers
{
    /// <summary>
    /// Helps to build a <see cref="TypeDefDeclaration"/> implementing
    /// a delegate.
    /// </summary>
    public static class DelegateBuilder
    {
        /// <summary>
        /// Builds a <see cref="TypeDefDeclaration"/> implementing
        /// a delegate.
        /// </summary>
        /// <param name="typeContainer">Declaration to which the <see cref="TypeDefDeclaration"/> should be added.</param>
        /// <param name="name">Type name.</param>
        /// <param name="delegateSignature">Delegate signature.</param>
        /// <returns>A <see cref="TypeDefDeclaration"/> implementing the delegate.</returns>
        /// <remarks>
        /// It seems that Microsoft's runtime engine requires delegates to be derived
        /// from <see cref="System.MulticastDelegate"/>.
        /// </remarks>
        public static TypeDefDeclaration BuildDelegate( ITypeContainer typeContainer, string name,
                                                        DelegateSignature delegateSignature )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( name, "name" );
            ExceptionHelper.AssertArgumentNotNull( delegateSignature, "delegateSignature" );

            #endregion

            ModuleDeclaration targetModule = typeContainer.Module;

            // Translate types.
            ITypeSignature translatedReturnType = delegateSignature.ReturnType == null
                                                      ? typeContainer.Module.Cache.GetIntrinsic( IntrinsicType.Void )
                                                      : delegateSignature.ReturnType.Translate( targetModule );
            ITypeSignature[] translatedParameterTypes = new ITypeSignature[delegateSignature.Parameters.Count];
            for ( int i = 0; i < translatedParameterTypes.Length; i++ )
            {
                translatedParameterTypes[i] = delegateSignature.Parameters[i].ParameterType.Translate( targetModule );
            }

            // Define the type.
            TypeDefDeclaration typeDef = new TypeDefDeclaration
                                             {
                                                 Name = name,
                                                 BaseType =
                                                     ( (INamedType)
                                                       targetModule.Cache.GetType( typeof(MulticastDelegate) ) ),
                                                 Attributes = ( ( typeContainer is TypeDefDeclaration
                                                                      ? TypeAttributes.NestedPublic
                                                                      : TypeAttributes.Public ) |
                                                                TypeAttributes.Sealed | TypeAttributes.AnsiClass |
                                                                TypeAttributes.Serializable
                                                                | TypeAttributes.BeforeFieldInit |
                                                                TypeAttributes.AutoLayout )
                                             };
            typeContainer.Types.Add( typeDef );

            // Define generic parameters.
            Set<string> names = new Set<string>( delegateSignature.GenericParameters.Count );
            for ( int i = 0; i < delegateSignature.GenericParameters.Count; i++ )
            {
                GenericParameterDeclaration translatedGenericParameter =
                    delegateSignature.GenericParameters[i].Clone( targetModule, targetModule.Cache.IdentityGenericMap );
                if ( names.Contains( translatedGenericParameter.Name ) )
                {
                    translatedGenericParameter.Name =
                        string.Format( CultureInfo.InvariantCulture, "_T{0}", i );
                }
                names.Add( translatedGenericParameter.Name );

                translatedGenericParameter.Kind = GenericParameterKind.Type;
                translatedGenericParameter.Ordinal = i;


                typeDef.GenericParameters.Add( translatedGenericParameter );
                translatedGenericParameter.Constraints.AddRangeClone( delegateSignature.GenericParameters[i].Constraints,
                                                                 targetModule.Cache.IdentityGenericMap );
            }

            #region Constructor (declaration only, implementation done by VRE).

            MethodDefDeclaration constructor = new MethodDefDeclaration
                                                   {
                                                       Name = ".ctor",
                                                       Attributes =
                                                           ( MethodAttributes.SpecialName |
                                                             MethodAttributes.RTSpecialName |
                                                             MethodAttributes.Public | MethodAttributes.HideBySig ),
                                                       CallingConvention = CallingConvention.HasThis,
                                                       ImplementationAttributes = MethodImplAttributes.Runtime
                                                   };
            typeDef.Methods.Add( constructor );

            constructor.ReturnParameter =
                new ParameterDeclaration( -1, "<return>", typeContainer.Module.Cache.GetIntrinsic( IntrinsicType.Void ) )
                    {Attributes = ParameterAttributes.Retval};
            constructor.Parameters.Add(
                new ParameterDeclaration( 0, "sender", typeContainer.Module.Cache.GetIntrinsic( IntrinsicType.Object ) ) );
            constructor.Parameters.Add(
                new ParameterDeclaration( 1, "method", typeContainer.Module.Cache.GetIntrinsic( IntrinsicType.IntPtr ) ) );

            #endregion

            #region "Invoke" method (declaration only, implementation done by VRE).

            MethodDefDeclaration invokeMethod = new MethodDefDeclaration
                                                    {
                                                        Name = "Invoke",
                                                        CallingConvention = CallingConvention.HasThis,
                                                        Attributes =
                                                            ( MethodAttributes.Public | MethodAttributes.NewSlot |
                                                              MethodAttributes.Virtual |
                                                              MethodAttributes.HideBySig ),
                                                        ImplementationAttributes = MethodImplAttributes.Runtime
                                                    };
            typeDef.Methods.Add( invokeMethod );

            invokeMethod.ReturnParameter = new ParameterDeclaration
                                               {
                                                   Attributes = ParameterAttributes.Retval,
                                                   ParameterType = translatedReturnType
                                               };

            for ( int i = 0; i < delegateSignature.Parameters.Count; i++ )
            {
                ParameterDeclaration sourceParameter = delegateSignature.Parameters[i];

                ITypeSignature parameterType = translatedParameterTypes[i];
                IGenericParameter genericParameterType = parameterType as IGenericParameter;

                ParameterDeclaration parameter = new ParameterDeclaration
                                                     {
                                                         Ordinal = i,
                                                         Attributes = sourceParameter.Attributes,
                                                         Name =string.Format( CultureInfo.InvariantCulture, "p{0}", i )
                                                     };

                if ( genericParameterType == null )
                {
                    parameter.ParameterType = parameterType;
                }
                else
                {
                    parameter.ParameterType = typeDef.GenericParameters[genericParameterType.Ordinal];
                }
                invokeMethod.Parameters.Add( parameter );
            }

            #endregion

            #region "DynamicInvoke" method.

            // Determine if we can override the method DynamicInvokeImpl.
            TypeDefDeclaration delegateTypeDef = 
                typeContainer.Module.FindMscorlib().GetAssemblyEnvelope().GetTypeDefinition( "System.Delegate" );
            MethodDefDeclaration dynamicInvokeImplBaseMethod =
                delegateTypeDef.Methods.GetOneByName( "DynamicInvokeImpl" );

            if (dynamicInvokeImplBaseMethod != null && dynamicInvokeImplBaseMethod.Visibility == Visibility.Family)
            {
                // Declare the method.
                MethodDefDeclaration dynamicInvokeMethod = new MethodDefDeclaration
                                                               {
                                                                   CallingConvention = CallingConvention.HasThis,
                                                                   Name = "DynamicInvokeImpl",
                                                                   Attributes =
                                                                       ( MethodAttributes.Family |
                                                                         MethodAttributes.Virtual |
                                                                         MethodAttributes.ReuseSlot |
                                                                         MethodAttributes.HideBySig )
                                                               };
                typeDef.Methods.Add( dynamicInvokeMethod );

                dynamicInvokeMethod.ReturnParameter = new ParameterDeclaration
                                                          {
                                                              Attributes = ParameterAttributes.Retval,
                                                              ParameterType =
                                                                  typeContainer.Module.Cache.GetIntrinsic(
                                                                  IntrinsicType.Object )
                                                          };
                ParameterDeclaration argsParameter = new ParameterDeclaration
                                                         {
                                                             Name = "args",
                                                             Attributes = ParameterAttributes.None,
                                                             ParameterType =
                                                                 new ArrayTypeSignature(
                                                                 targetModule.Cache.GetIntrinsic( IntrinsicType.Object ) )
                                                         };
                dynamicInvokeMethod.Parameters.Add( argsParameter );
                dynamicInvokeMethod.MethodBody = new MethodBodyDeclaration();
                dynamicInvokeMethod.MethodBody.RootInstructionBlock =
                    dynamicInvokeMethod.MethodBody.CreateInstructionBlock();
                InstructionSequence instructionSequence = dynamicInvokeMethod.MethodBody.CreateInstructionSequence();
                dynamicInvokeMethod.MethodBody.RootInstructionBlock.AddInstructionSequence( instructionSequence,
                                                                                            NodePosition.Before, null );
                InstructionWriter writer = new InstructionWriter();
                writer.AttachInstructionSequence( instructionSequence );

                // We need a local variable for each parameter given by reference.
                // They will be stored in this array.
                LocalVariableSymbol[] argumentByRefLocals = new LocalVariableSymbol[delegateSignature.Parameters.Count];

                // Check that we have arguments.
                if ( delegateSignature.Parameters.Count > 0 )
                {
                    InstructionSequence nextSequence = dynamicInvokeMethod.MethodBody.CreateInstructionSequence();
                    dynamicInvokeMethod.MethodBody.RootInstructionBlock.AddInstructionSequence( nextSequence,
                                                                                                NodePosition.After,
                                                                                                instructionSequence );
                    writer.EmitInstruction( OpCodeNumber.Ldarg_1 );
                    writer.EmitBranchingInstruction( OpCodeNumber.Brtrue, nextSequence );
                    writer.EmitInstructionMethod( OpCodeNumber.Newobj,
                                                  targetModule.FindMethod(
                                                      typeof(ArgumentNullException).GetConstructor( Type.EmptyTypes ),
                                                      BindingOptions.Default ) );
                    writer.EmitInstruction( OpCodeNumber.Throw );
                    writer.DetachInstructionSequence();
                    writer.AttachInstructionSequence( nextSequence );
                }


                WeavingHelper wh = new WeavingHelper( targetModule );

                #region Store by-ref arguments in a local variable

                for ( int i = 0; i < argumentByRefLocals.Length; i++ )
                {
                    ITypeSignature parameterType = translatedParameterTypes[i];
                    PointerTypeSignature pointerArgumentType =
                        parameterType.GetNakedType( TypeNakingOptions.IgnoreModifiers )
                        as PointerTypeSignature;


                    if ( pointerArgumentType != null && pointerArgumentType.IsManaged )
                    {
                        // We have a pointer. Create a strongly typed local variable, store
                        // the input value in it and load the address on the stack.

                        argumentByRefLocals[i] =
                            dynamicInvokeMethod.MethodBody.RootInstructionBlock.DefineLocalVariable(
                                pointerArgumentType.ElementType,
                                string.Format( CultureInfo.InvariantCulture, "_l{0}", i ) );

                        if ( ( delegateSignature.Parameters[i].Attributes & ParameterAttributes.In ) != 0 ||
                             ( delegateSignature.Parameters[i].Attributes & ParameterAttributes.Out ) == 0 )
                        {
                            writer.EmitInstruction( OpCodeNumber.Ldarg_1 );
                            writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, i );
                            writer.EmitInstruction( OpCodeNumber.Ldelem_Ref );
                            wh.FromObject( pointerArgumentType.ElementType, writer );

                            writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc, argumentByRefLocals[i] );
                        }
                    }
                }

                #endregion

                #region Load arguments on the stack (create variables for byref arguments as needed).

                writer.EmitInstruction( OpCodeNumber.Ldarg_0 );

                for ( int i = 0; i < argumentByRefLocals.Length; i++ )
                {
                    // Load the value on the stack.

                    if ( argumentByRefLocals[i] != null )
                    {
                        writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloca, argumentByRefLocals[i] );
                    }
                    else
                    {
                        writer.EmitInstruction( OpCodeNumber.Ldarg_1 );
                        writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, i );
                        writer.EmitInstruction( OpCodeNumber.Ldelem_Ref );
                        wh.FromObject( translatedParameterTypes[i], writer );
                    }
                }

                #endregion

                // Call the method.
                writer.EmitInstructionMethod( OpCodeNumber.Call,
                                              GenericHelper.GetMethodCanonicalGenericInstance( invokeMethod ) );

                #region Set back arguments passed by reference

                for ( int i = 0; i < argumentByRefLocals.Length; i++ )
                {
                    ITypeSignature parameterType = translatedParameterTypes[i];
                    PointerTypeSignature pointerArgumentType =
                        parameterType.GetNakedType( TypeNakingOptions.IgnoreModifiers )
                        as PointerTypeSignature;

                    if ( pointerArgumentType != null )
                    {
                        writer.EmitInstruction( OpCodeNumber.Ldarg_1 );
                        writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, i );

                        writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, argumentByRefLocals[i] );

                        // If it is a value type, unbox it.
                        wh.ToObject( pointerArgumentType.ElementType, writer );

                        writer.EmitInstruction( OpCodeNumber.Stelem_Ref );
                    }
                }

                #endregion

                // Process the return value.
                if ( IntrinsicTypeSignature.Is( translatedReturnType, IntrinsicType.Void ) )
                {
                    writer.EmitInstruction( OpCodeNumber.Ldnull );
                }
                else
                {
                    // We have the value. Unbox/cast it.
                    wh.ToObject( translatedReturnType, writer );
                }

                writer.EmitInstruction( OpCodeNumber.Ret );
                writer.DetachInstructionSequence();
            }

            #endregion

            return typeDef;
        }
    }
}