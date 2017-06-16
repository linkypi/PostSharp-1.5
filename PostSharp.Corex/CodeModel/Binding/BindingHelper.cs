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
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using PostSharp.Reflection;
using System.Collections.Generic;

namespace PostSharp.CodeModel.Binding
{
    /// <summary>
    /// Implements some methods used by the binding functionality.
    /// </summary>
    internal static class BindingHelper
    {
        private static readonly Regex escapeReflectionNameRegex = new Regex( "([\\+,\\[\\]])", RegexOptions.None );

        public static string EscapeReflectionName( string name )
        {
            if ( name == null )
            {
                return null;
            }
            return escapeReflectionNameRegex.Replace( name, "\\$1" );
        }

        public static string GetTypeNameWithoutNamespace( string typeName )
        {
            if ( typeName == null )
                throw new ArgumentNullException( "typeName" );

            int dot = typeName.LastIndexOf( '.' );
            if ( dot > 0 )
                return typeName.Substring( dot + 1 );
            else
                return typeName;
        }


        /// <summary>
        /// Determines whether an <see cref="Type"/> is <i>simple</i>, i.e. may be
        /// a <see cref="TypeDefDeclaration"/> or a <see cref="TypeRefDeclaration"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns><b>true</b> if <paramref name="type"/> can be represented
        /// as a <see cref="TypeDefDeclaration"/> or a <see cref="TypeRefDeclaration"/>,
        /// otherwise <b>false</b>.</returns>
        public static bool IsSimpleType( Type type, BindingOptions bindingOptions )
        {
            return !(type.HasElementType || type.IsGenericParameter || (type.IsGenericType &&
                                                                         ( bindingOptions &
                                                                            BindingOptions.RequireGenericMask ) !=
                                                                          BindingOptions.RequireGenericDefinition ) );
        }

        /// <summary>
        /// Given a reflection method (<see cref="MethodBase"/>), returns the internal
        /// representation of its calling convention, return type and parameter types.
        /// </summary>
        /// <param name="module">The module in which types have to be found and optionnally created.</param>
        /// <param name="reflectionMethod">The method to be bound.</param>
        /// <param name="bindingOptions">Determines how to process types that do not exist in <paramref name="module"/>.</param>
        /// <returns><b>true</b> if the binding was successful, otherwise <b>false</b>.</returns>
        public static MethodSignature GetSignature( ModuleDeclaration module,
                                                                           MethodBase reflectionMethod,
                                                                           BindingOptions bindingOptions )
        {
            CallingConvention callingConvention;
            ITypeSignature returnType;
            TypeSignatureCollection parameters;
            MethodInfo methodInfo = reflectionMethod as MethodInfo;
            ParameterInfo[] reflectionParameters = reflectionMethod.GetParameters();


            // If the declaring type is a TypeSpec, the method we have has BOUND generic parameters,
            // but we want UNBOUND ones.
            if ( reflectionMethod.DeclaringType.IsGenericType &&
                 !reflectionMethod.DeclaringType.IsGenericTypeDefinition &&
                 ( bindingOptions & BindingOptions.RequireGenericInstance ) != 0 )
            {
                // We have to look in the TypeDef for the method.
                Type declaringTypeDef = reflectionMethod.DeclaringType.GetGenericTypeDefinition();
                MethodBase[] candidateMethods;

                if ( methodInfo != null )
                {
                    candidateMethods = declaringTypeDef.GetMethods( BindingFlags.Public | BindingFlags.NonPublic
                                                                    | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                }
                else
                {
                    candidateMethods = declaringTypeDef.GetConstructors( BindingFlags.Public | BindingFlags.NonPublic
                                                                         | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                }

                ReflectionTypeComparer typeComparer =
                    ReflectionTypeComparer.GetInstance(
                        reflectionMethod.DeclaringType.GetGenericArguments(),
                        null,
                        null,
                        null );

                foreach ( MethodBase candidate in candidateMethods )
                {
                    if ( candidate.Name == reflectionMethod.Name &&
                         candidate.Attributes == reflectionMethod.Attributes )
                    {
                        ParameterInfo[] candidateParameters = candidate.GetParameters();

                        if ( candidateParameters.Length == reflectionParameters.Length )
                        {
                            bool matches = true;

                            // We have to test whether the candidate method can be our method.
                            // We have to compare the candidate method where generic parameters
                            // are mapped to the generic arguments of our TypeSpec.
                            for ( int i = 0 ; i < candidateParameters.Length ; i++ )
                            {
                                if ( !typeComparer.Equals( candidateParameters[i].ParameterType,
                                                           reflectionParameters[i].ParameterType ) )
                                {
                                    matches = false;
                                    break;
                                }
                            }

                            if ( !matches )
                                continue;

                            if ( methodInfo != null )
                            {
                                MethodInfo candidateMethodInfo = (MethodInfo) candidate;
                                if ( !typeComparer.Equals( candidateMethodInfo.ReturnType,
                                                           methodInfo.ReturnType ) )
                                    continue;
                            }

                            // We have found the method! We can recurse with this method signature.
                            return GetSignature( module, candidate, bindingOptions );
                        }
                    }
                }

                throw new AssertionFailedException( "Cannot find the Reflection MethodDef." );
            }

            // Prepare the list of parameters
            parameters = new TypeSignatureCollection( reflectionParameters.Length );


            // Decoding the calling convention.
            switch ( reflectionMethod.CallingConvention & (CallingConventions) 3 )
            {
                case 0:
                case CallingConventions.Standard:
                    callingConvention = CallingConvention.Default;
                    break;

                case CallingConventions.VarArgs:
                    callingConvention = CallingConvention.VarArg;
                    break;

                case CallingConventions.Any:
                    callingConvention = CallingConvention.Any;
                    break;

                default:
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "UnexpectedCallingConvention",
                                                                               reflectionMethod.CallingConvention );
            }

            if ( ( reflectionMethod.CallingConvention & CallingConventions.ExplicitThis ) != 0 )
            {
                callingConvention |= CallingConvention.ExplicitThis;
            }

            if ( ( reflectionMethod.CallingConvention & CallingConventions.HasThis ) != 0 )
            {
                callingConvention |= CallingConvention.HasThis;
            }

            if ( reflectionMethod.IsGenericMethod | reflectionMethod.IsGenericMethodDefinition )
            {
                callingConvention |= CallingConvention.Generic;
            }

            // Bind the return type.
            if ( methodInfo != null )
            {
                if ( methodInfo.ReturnType.IsGenericParameter &&
                     methodInfo.ReturnType.DeclaringMethod == reflectionMethod )
                {
                    // To avoid recursion in resolving method parameters.
                    returnType =
                        module.Cache.GetGenericParameter( methodInfo.ReturnType.GenericParameterPosition,
                                                          GenericParameterKind.Method );
                }
                else
                {
                    returnType = module.FindType( methodInfo.ReturnType,
                                                  ( bindingOptions & ~BindingOptions.RequireGenericMask ) |
                                                  BindingOptions.RequireGenericInstance );
                    if ( returnType == null )
                    {
                        Trace.ReflectionBinding.WriteLine(
                            "BindingHelper.GetSignature({{{0}::{1}}}) : cannot find the return type.",
                            methodInfo.DeclaringType, methodInfo );
                        return null;
                    }
                }
            }
            else
            {
                returnType = module.Cache.GetIntrinsic(IntrinsicType.Void);
            }


            // Bind parameter types.
            for ( int i = 0 ; i < reflectionParameters.Length ; i++ )
            {
                if ( reflectionParameters[i].ParameterType.IsGenericParameter &&
                     reflectionParameters[i].ParameterType.DeclaringMethod == reflectionMethod )
                {
                    // To avoid recursion in resolving method parameters.
                    parameters.Add(
                        module.Cache.GetGenericParameter(
                            reflectionParameters[i].ParameterType.GenericParameterPosition, GenericParameterKind.Method ) );
                }
                else
                {
                    ITypeSignature parameterType = module.FindType( reflectionParameters[i].ParameterType,
                                                                    (bindingOptions & ~BindingOptions.RequireGenericMask) |
                                                                    BindingOptions.RequireGenericInstance );

                    if (parameterType == null)
                    {
                        Trace.ReflectionBinding.WriteLine(
                            "BindingHelper.GetSignature({{{0}::{1}}}) : cannot find the parameter type {{{2}}}.",
                            methodInfo.DeclaringType, methodInfo, reflectionParameters[i].ParameterType );

                        return null;
                    }

                    parameters.Add(parameterType);

                }
            }

            MethodSignature signature = new MethodSignature( module, callingConvention, returnType, parameters,
                                                             ( reflectionMethod.IsGenericMethod |
                                                               reflectionMethod.IsGenericMethodDefinition )
                                                                 ? reflectionMethod.GetGenericArguments().Length
                                                                 : 0 );

            // Set the arity.
            if ( reflectionMethod.IsGenericMethod )
            {
                signature.GenericParameterCount = reflectionMethod.GetGenericArguments().Length;
            }

            return signature;
        }

        private static bool GetReflectionSignature( IMethod method, out Type reflectionReturnType,
                                                    Type[] reflectionParameterTypes, Type[] genericTypeArguments,
                                                    Type[] genericMethodArguments )
        {
            reflectionReturnType = null;

            for ( int i = 0 ; i < method.ParameterCount ; i++ )
            {
                reflectionParameterTypes[i] =
                    method.GetParameterType( i ).GetSystemType( genericTypeArguments, genericMethodArguments );
                if ( reflectionParameterTypes[i] == null )
                {
                    Trace.ReflectionBinding.WriteLine(
                        "BinderHelper.GetSystemMethod({{{0}}}): cannot find the parameter type {1}.",
                        method, method.GetParameterType( i ) );

                    return false;
                }
            }

            if ( method.ReturnType != null )
            {
                reflectionReturnType = method.ReturnType.GetSystemType( genericTypeArguments, genericMethodArguments );

                if ( reflectionReturnType == null )
                {
                    Trace.ReflectionBinding.WriteLine(
                        "BinderHelper.GetSystemMethod({{{0}}}): cannot find the parameter type {1}.",
                        method, method.ReturnType );

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the reflection method (<see cref="MethodBase"/>) corresponding to a given method (<see cref="IMethod"/>)
        /// and generic arguments.
        /// </summary>
        /// <param name="declaringClass">A <see cref="Module"/> or a <see cref="Type"/>
        /// declaring the method.</param>
        /// <param name="method">The method whose reflection representation is required.</param>
        /// <param name="genericTypeArguments">Generic type arguments in the current context.</param>
        /// <param name="genericMethodArguments">Generic method arguments in the current context.</param>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns>The <see cref="MethodBase"/> corresponding to <paramref name="method"/>,
        /// or <b>null</b> if the binding was not successful.</returns>
        [SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters" )]
        public static MethodBase GetSystemMethod( object declaringClass, IMethod method, Type[] genericTypeArguments,
                                                  Type[] genericMethodArguments, BindingOptions bindingOptions )
        {
            Module declaringModule = declaringClass as Module;
            Type declaringType = declaringClass as Type;

            Type[] myReflectionGenericTypeArguments = genericTypeArguments;

            // If the declaring type is a generic construction, 
            // replace the mapping of generic type parameters.
            if ( declaringType != null && declaringType.IsGenericType )
            {
                myReflectionGenericTypeArguments = declaringType.GetGenericArguments();
            }

            // Determines whether the requested method is generic and cache the result on the stack.
            IGenericDefinition genericMethodDefinition = method as IGenericDefinition;
            IGenericInstance genericMethodInstance = method as IGenericInstance;

            bool isGenericDefinition = genericMethodDefinition != null && genericMethodDefinition.IsGenericDefinition;
            bool isGenericInstance = genericMethodInstance != null && genericMethodInstance.IsGenericInstance;
            int genericArgumentCount;

            if ( isGenericDefinition )
            {
                genericArgumentCount = genericMethodDefinition.GenericParameterCount;
            }
            else if ( isGenericInstance )
            {
                genericArgumentCount = genericMethodInstance.GenericArgumentCount;
            }
            else
            {
                genericArgumentCount = 0;
            }

            // Maps the method return and parameter types to reflection.

            Type[] reflectionParameterTypes = new Type[method.ParameterCount];
            Type reflectionReturnType;

            if ( !isGenericDefinition && !isGenericInstance )
            {
                // This is not a generic method. First bind the parameters, then lookup the method.

                // Get the reflection type of parameters
                if ( !GetReflectionSignature( method, out reflectionReturnType, reflectionParameterTypes,
                                              myReflectionGenericTypeArguments, null ) )
                {
                    return null;
                }

                if ( method.Name == ".ctor" )
                {
                    return
                        declaringType.GetConstructor(
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
                            Type.DefaultBinder, reflectionParameterTypes, null );
                }
                else if ( method.Name == ".cctor" )
                {
                    return declaringType.TypeInitializer;
                }
                else
                {
                    MethodInfo methodInfo;

                    if ( declaringType != null )
                    {
                        methodInfo =
                            declaringType.GetMethod( method.Name, BindingFlags.Public | BindingFlags.NonPublic |
                                                                  BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly,
                                                     Type.DefaultBinder, reflectionParameterTypes, null );
                    }
                    else
                    {
                        methodInfo =
                            declaringModule.GetMethod( method.Name, BindingFlags.Public | BindingFlags.NonPublic |
                                                                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly,
                                                       Type.DefaultBinder, CallingConventions.Any,
                                                       reflectionParameterTypes, null );
                    }

                    if ( methodInfo == null || methodInfo.ReturnType != reflectionReturnType )
                    {
                        return null;
                    }

                    return methodInfo;
                }
            }
            else
            {
                // Now loop over all methods.
                foreach ( MethodInfo candidateMethod in declaringType.GetMethods(
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    // Perform trivial tests.
                    if ( candidateMethod.Name != method.Name )
                    {
                        continue;
                    }

                    ParameterInfo[] candidateParameters = candidateMethod.GetParameters();

                    if ( candidateParameters.Length != method.ParameterCount )
                    {
                        continue;
                    }

                    Type[] candidateGenericArguments = candidateMethod.GetGenericArguments();
                    if ( candidateGenericArguments.Length != genericArgumentCount )
                    {
                        continue;
                    }

                    if ( !GetReflectionSignature( method, out reflectionReturnType, reflectionParameterTypes,
                                                  myReflectionGenericTypeArguments, candidateGenericArguments ) )
                    {
                        continue;
                    }

                    bool match = true;

                    for ( int i = 0 ; i < candidateParameters.Length ; i++ )
                    {
                        if ( !reflectionParameterTypes[i].Equals( candidateParameters[i].ParameterType ) )
                        {
                            match = false;
                            break;
                        }
                    }

                    if ( !match )
                    {
                        continue;
                    }

                    return candidateMethod;
                }
                return null;
            }
        }

        /// <summary>
        /// Finds a given signature in a set of candidate methods.
        /// </summary>
        /// <param name="parentGenericContext">Generic context of all signatures.</param>
        /// <param name="signature">The signature to be found</param>
        /// <param name="candidates">The set of candidate methods.</param>
        /// <returns>The method whose signature is <paramref name="signature"/>, or 
        /// <b>null</b> if the method could not be found.</returns>
        public static IMethod FindMethod( GenericMap parentGenericContext,
                                          IMethodSignature signature,
                                          IEnumerable candidates )
        {
            IEnumerator enumerator = candidates.GetEnumerator();
            Domain domain = signature.Module.Domain;

            while ( enumerator.MoveNext() )
            {
                IMethod method = (IMethod) enumerator.Current;
                IMethodSignature candidateSignature;

                // Resolve generic arguments.
                if ( !parentGenericContext.IsEmpty )
                {
                    candidateSignature =
                        method.MapGenericArguments( new GenericMap( parentGenericContext,
                                                                    signature.Module.Cache.GetGenericParameterArray( 32,
                                                                                                                     GenericParameterKind
                                                                                                                         .
                                                                                                                         Method ) ) );
                }
                else
                {
                    candidateSignature = method;
                }

                if ( candidateSignature.MatchesReference( signature ) )
                {
                    return method;
                }
            }

            return null;
        }


        public static ITypeSignature TranslateTypeDefOrRef( ITypeSignature type, ModuleDeclaration targetModule )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( targetModule, "targetModule" );
            ExceptionHelper.Core.AssertValidArgument( targetModule.Domain == type.Module.Domain, "targetModule",
                                                      "ModuleInSameDomainAsType" );

            #endregion

            if ( targetModule == type.Module )
            {
                return type;
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                type.WriteReflectionTypeName(stringBuilder, ReflectionNameOptions.IgnoreGenericTypeDefParameters);
               
                if ( type.DeclaringAssembly == targetModule.DeclaringAssembly )
                {
                    // The type is defined in another
                    ModuleRefDeclaration moduleRef = targetModule.ModuleRefs.GetByName( type.Module.Name );
                    
                    ITypeSignature translatedType =
                        moduleRef.FindType(stringBuilder.ToString(), BindingOptions.Default);

                    return translatedType;
                }
                else
                {
                    IAssembly assembly = targetModule.FindAssembly( type.DeclaringAssembly, BindingOptions.Default );

                    AssemblyRefDeclaration assemblyRef = assembly as AssemblyRefDeclaration;

                    if (assemblyRef != null )
                    {
                        // The type is defined in an external assembly.
                        return assemblyRef.FindType(stringBuilder.ToString(), BindingOptions.Default);                        
                    }
                    else
                    {
                        // The type is defined in the current assembly.
                        return targetModule.FindType(stringBuilder.ToString(), BindingOptions.Default); ;
                    }
                    
                }
            }
        
        }

        public static IMethodSignature TranslateMethodSignature( IMethodSignature methodSignature,
                                                                 ModuleDeclaration targetModule )
        {
            if ( targetModule == methodSignature.Module )
            {
                return methodSignature;
            }

            // Translate parameters.
            TypeSignatureCollection translatedParameterTypes =
                new TypeSignatureCollection( methodSignature.ParameterCount );
            for ( int i = 0 ; i < methodSignature.ParameterCount ; i++ )
            {
                translatedParameterTypes.Add( methodSignature.GetParameterType( i ).Translate( targetModule ) );
            }

            ITypeSignature returnType = methodSignature.ReturnType;
            if ( returnType != null )
                returnType = returnType.Translate(targetModule);

            return new MethodSignature( targetModule,
                methodSignature.CallingConvention, returnType,
                translatedParameterTypes, methodSignature.GenericParameterCount );
        }

        public static IMethod TranslateMethodDefOrRef( IMethod method, ModuleDeclaration targetModule )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( targetModule, "targetModule" );
            if ( method.Module.Domain != targetModule.Domain )
            {
                throw new ArgumentOutOfRangeException( "targetModule",
                                                       "The target module should be in the same domain as the current element." );
            }

            #endregion

            if ( targetModule == method.Module )
            {
                return method;
            }

            // Translate declaring type.
            ITypeSignature translatedDeclaringTypeSig = method.DeclaringType.Translate( targetModule );
            IType translatedDeclaringType = translatedDeclaringTypeSig as IType;

            if ( translatedDeclaringType == null )
            {
                // We have to get the corresponding TypeSpec to get the collection of methods.
                translatedDeclaringType = targetModule.TypeSpecs.GetBySignature( translatedDeclaringTypeSig );
            }

            IMethodSignature translatedMethodSignature = TranslateMethodSignature( method, targetModule );

            return
                translatedDeclaringType.Methods.GetMethod( method.Name, translatedMethodSignature,
                                                           BindingOptions.Default );
        }

        public static bool BindingFlagsMatch( BindingFlags bindingFlags, bool isStatic, Visibility visibility)
        {
            return (((bindingFlags & BindingFlags.Instance) == BindingFlags.Instance && !isStatic) ||
                    ((bindingFlags & BindingFlags.Static) == BindingFlags.Static && isStatic)) &&
                   (((bindingFlags & BindingFlags.Public) == BindingFlags.Public && visibility == Visibility.Public) ||
                    ((bindingFlags & BindingFlags.NonPublic) == BindingFlags.NonPublic &&
                     visibility != Visibility.Public));
        }

        public static bool BindingFlagsMatch(BindingFlags bindingFlags, MethodGroupDeclaration methodGroup )
        {
            IEnumerator<MethodSemanticDeclaration> enumerator = methodGroup.Members.GetEnumerator();
            if ( !enumerator.MoveNext() )
                return false;

            MethodDefDeclaration accessor = enumerator.Current.Method;


            return BindingFlagsMatch( bindingFlags, accessor.IsStatic, accessor.Visibility );
        }

        public static bool SignatureMatches( MethodDefDeclaration method, CallingConventions callinConvention, Type[] parameterTypes )
        {
            throw new NotImplementedException();
        }
    }
}
