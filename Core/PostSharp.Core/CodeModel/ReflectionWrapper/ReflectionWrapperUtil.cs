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
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Collections;

namespace PostSharp.CodeModel.ReflectionWrapper
{
    /// <summary>
    /// Provides utility methods to work with reflection wrappers.
    /// </summary>
    public static class ReflectionWrapperUtil
    {
        internal static object[] GetCustomAttributes( IDeclarationWithCustomAttributes declaration, Type attributeType )
        {
            IType internalAttributeType;

            if ( attributeType == null )
                internalAttributeType = null;
            else
            {
                internalAttributeType =
                    (IType)
                    declaration.Module.FindType( attributeType,
                                                 BindingOptions.OnlyExisting | BindingOptions.DontThrowException );
                if ( internalAttributeType == null )
                    return EmptyArray<object>.GetInstance();
            }


            ArrayList objects = new ArrayList();

            declaration.CustomAttributes.ConstructRuntimeObjects( internalAttributeType, objects );

            return (object[]) objects.ToArray( attributeType ?? typeof(Attribute) );
        }

        internal static object[] GetCustomAttributes( IDeclarationWithCustomAttributes declaration )
        {
            return GetCustomAttributes( declaration, null );
        }

        internal static bool IsCustomAttributeDefined( IDeclarationWithCustomAttributes declaration, Type attributeType )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( attributeType, "attributeType" );

            #endregion

            IType internalAttributeType = (IType) declaration.Module.FindType(
                                                      attributeType,
                                                      BindingOptions.OnlyExisting | BindingOptions.DontThrowException );
            if ( internalAttributeType == null )
                return false;

            return declaration.CustomAttributes.Contains( internalAttributeType );
        }

        internal static void GetGenericMapWrapper( GenericMap map,
                                                   Type[] inputGenericTypeArguments, Type[] inputGenericMethodArguments,
                                                   out Type[] outputGenericTypeArguments,
                                                   out Type[] outputGenericMethodArguments )
        {
            try
            {
                outputGenericTypeArguments = new Type[map.GenericTypeParameterCount];
                for ( int i = 0; i < outputGenericTypeArguments.Length; i++ )
                {
                    outputGenericTypeArguments[i] = map.GetGenericTypeParameter( i ).
                        GetReflectionWrapper( inputGenericTypeArguments, inputGenericMethodArguments );
                }

                outputGenericMethodArguments = new Type[map.GenericMethodParameterCount];
                for ( int i = 0; i < outputGenericMethodArguments.Length; i++ )
                {
                    outputGenericMethodArguments[i] = map.GetGenericMethodParameter( i ).
                        GetReflectionWrapper( inputGenericTypeArguments, inputGenericMethodArguments );
                }
            }
            catch ( ArgumentNullException e )
            {
                throw new BindingException( string.Format( "Cannot map {0} with arguments Type[{1}], Type[{2}].",
                                                           map,
                                                           inputGenericTypeArguments != null
                                                               ? inputGenericTypeArguments.Length
                                                               : 0,
                                                           inputGenericMethodArguments != null
                                                               ? inputGenericMethodArguments.Length
                                                               : 0 ), e );
            }
        }

        internal static ParameterInfo[] GetMethodParameters( MethodDefDeclaration method, Type[] typeArgs,
                                                             Type[] methodArgs )
        {
            ParameterInfo[] parameters = new ParameterInfo[method.Parameters.Count];

            for ( int i = 0; i < parameters.Length; i++ )
            {
                parameters[i] = method.Parameters[i].GetReflectionWrapper( typeArgs, methodArgs );
            }

            return parameters;
        }

        internal static MethodInfo GetMethodSemantic( MethodGroupDeclaration group, MethodSemantics semantic,
                                                      bool nonPublic,
                                                      Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            MethodSemanticDeclaration methodSemantic = group.Members.GetBySemantic( semantic );
            if ( methodSemantic != null )
            {
                if ( !nonPublic && methodSemantic.Method.Visibility != Visibility.Public )
                    return null;

                return
                    (MethodInfo)
                    methodSemantic.Method.GetReflectionWrapper( genericTypeArguments, genericMethodArguments );
            }
            else
            {
                return null;
            }
        }

        internal static MethodInfo[] GetOtherMethodSemantics( MethodGroupDeclaration group, bool nonPublic,
                                                              Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            List<MethodInfo> methods = new List<MethodInfo>( group.Members.Count );
            foreach ( MethodSemanticDeclaration methodSemantic in group.Members )
            {
                if ( methodSemantic.Semantic == MethodSemantics.Other &&
                     ( nonPublic || methodSemantic.Method.Visibility == Visibility.Public ) )
                {
                    methods.Add( (MethodInfo)
                                 methodSemantic.Method.GetReflectionWrapper( genericTypeArguments,
                                                                             genericMethodArguments )
                        );
                }
            }

            return methods.ToArray();
        }

        internal static Type[] GetGenericArguments( IGeneric generic,
                                                    Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            if ( generic.IsGenericDefinition )
            {
                IGenericDefinition genericDefinition = (IGenericDefinition) generic;
                Type[] genericParameters = new Type[genericDefinition.GenericParameterCount];
                for ( int i = 0; i < genericParameters.Length; i++ )
                {
                    genericParameters[i] = genericDefinition.GetGenericParameter( i ).GetReflectionWrapper(
                        genericTypeArguments, genericMethodArguments );
                }

                return genericParameters;
            }
            else if ( generic.IsGenericInstance )
            {
                IGenericInstance genericInstance = (IGenericInstance) generic;
                Type[] genericParameters = new Type[genericInstance.GenericArgumentCount];
                for ( int i = 0; i < genericParameters.Length; i++ )
                {
                    genericParameters[i] = genericInstance.GetGenericArgument( i ).GetReflectionWrapper(
                        genericTypeArguments, genericMethodArguments );
                }

                return genericParameters;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Marks a <see cref="TypeDefDeclaration"/> to that it won't be enumerated by
        /// the <see cref="IAssemblyWrapper.GetTypes"/> method of <see cref="AssemblyWrapper"/>.
        /// </summary>
        /// <param name="typeDef">Type to hide from reflection wrappers.</param>
        public static void HideTypeFromAssembly( TypeDefDeclaration typeDef )
        {
            AssemblyWrapper.HideType( typeDef );
        }
    }
}