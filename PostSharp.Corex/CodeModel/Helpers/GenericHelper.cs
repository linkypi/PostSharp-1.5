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

using PostSharp.CodeModel.TypeSignatures;

namespace PostSharp.CodeModel.Helpers
{
    /// <summary>
    /// Provides helper methods to work with generic declarations.
    /// </summary>
    public static class GenericHelper
    {
        /// <summary>
        /// Gets the canonical generic instance of a generic type.
        /// </summary>
        /// <param name="type">Type whose canonical generic instance is requested.</param>
        /// <returns>The canonical generic instance of <paramref name="type"/>, or
        /// the unmodified <paramref name="type"/> if <paramref name="type"/> is not a generic
        /// type definition.</returns>
        /// <remarks>
        /// The canonical generic instance of a type is a generic instance where the n-th formal generic
        /// type argument is mapped to the n-th concrete generic type parameter. For instance, the canonical 
        /// generic instance of the type MyType`3 is MyType`3&lt;!0,!1,!2&gt;.
        /// </remarks>
        public static IType GetTypeCanonicalGenericInstance( TypeDefDeclaration type )
        {
            return GetTypeCanonicalGenericInstance( type, type.Module );
        }

        /// <summary>
        /// Gets the canonical generic instance of a generic type
        /// and express it for a different module than the one of the generic type definition.
        /// </summary>
        /// <param name="type">Type whose canonical generic instance is requested.</param>
        /// <param name="targetModule">Module in which declarations have to be expressed.</param>
        /// <returns>The canonical generic instance of <paramref name="type"/>, or
        /// the unmodified <paramref name="type"/> if <paramref name="type"/> is not a generic
        /// type definition.</returns>
        /// <remarks>
        /// The canonical generic instance of a type is a generic instance where the n-th formal generic
        /// type argument is mapped to the n-th concrete generic type parameter. For instance, the canonical 
        /// generic instance of the type MyType`3 is MyType`3&lt;!0,!1,!2&gt;.
        /// </remarks>
        public static IType GetTypeCanonicalGenericInstance( TypeDefDeclaration type, ModuleDeclaration targetModule )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );

            #endregion

            if ( type.IsGenericDefinition )
            {
                return targetModule.TypeSpecs.GetBySignature(
                    new GenericTypeInstanceTypeSignature(
                        (INamedType) type.Translate( targetModule ),
                        targetModule.Cache.GetGenericParameterArray(
                            type.GenericParameters.Count,
                            GenericParameterKind.Type ) ), true );
            }
            else
            {
                return type;
            }
        }

        /// <summary>
        /// Gets the canonical generic instance of a generic method. 
        /// </summary>
        /// <param name="method">Method whose canonical generic instance is requested.</param>
        /// <returns>The canonical generic instance of <paramref name="method"/>, or
        /// the unmodified <paramref name="method"/> if <paramref name="method"/> is not a generic
        /// method definition.</returns>
        /// <remarks>
        /// <para>
        /// The canonical generic instance of a method is a generic instance where the n-th formal generic
        /// method argument is mapped to the n-th concrete generic method parameter. If the declaring type
        /// is a generic type definition, the canonical generic instance of the method is defined on
        /// the canonical generic instance of the declaring type (see <see cref="GetTypeCanonicalGenericInstance(PostSharp.CodeModel.TypeDefDeclaration)"/>).
        /// </para>
        /// <para>
        /// For instance, the canonical 
        /// generic instance of the type MyType`3::MyMethod is MyType`3&lt;!0,!1,!2&gt;::MyMethod&lt;!0,!1&gt;.
        /// if MyMethod has two generic arguments.
        /// </para>
        /// </remarks>
        public static IMethod GetMethodCanonicalGenericInstance( MethodDefDeclaration method )
        {
            return GetMethodCanonicalGenericInstance( method, method.Module );
        }

        /// <summary>
        /// Gets the canonical generic instance of a generic method and express it for a different
        /// module than the one of the generic method definition.
        /// </summary>
        /// <param name="method">Method whose canonical generic instance is requested.</param>
        /// <param name="targetModule">Module for which the generic instance has to be expressed.</param>
        /// <returns>The canonical generic instance of <paramref name="method"/>, or
        /// the unmodified <paramref name="method"/> if <paramref name="method"/> is not a generic
        /// method definition.</returns>
        /// <remarks>
        /// <para>
        /// The canonical generic instance of a method is a generic instance where the n-th formal generic
        /// method argument is mapped to the n-th concrete generic method parameter. If the declaring type
        /// is a generic type definition, the canonical generic instance of the method is defined on
        /// the canonical generic instance of the declaring type (see <see cref="GetTypeCanonicalGenericInstance(PostSharp.CodeModel.TypeDefDeclaration,PostSharp.CodeModel.ModuleDeclaration)"/>).
        /// </para>
        /// <para>
        /// For instance, the canonical 
        /// generic instance of the type MyType`3::MyMethod is MyType`3&lt;!0,!1,!2&gt;::MyMethod&lt;!0,!1&gt;.
        /// if MyMethod has two generic arguments.
        /// </para>
        /// </remarks>
        public static IMethod GetMethodCanonicalGenericInstance( MethodDefDeclaration method, ModuleDeclaration targetModule )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( method, "method" );

            #endregion

            IMethod methodInstance;
            IType typeInstance;

            if ( method.IsGenericDefinition ||
                 method.DeclaringType.IsGenericDefinition )
            {
                IGenericMethodDefinition methodRef;

                if ( method.DeclaringType.IsGenericDefinition )
                {
                    typeInstance = targetModule.TypeSpecs.GetBySignature(
                        new GenericTypeInstanceTypeSignature(
                            (INamedType) method.DeclaringType.Translate( targetModule ),
                            targetModule.Cache.GetGenericParameterArray(
                                method.DeclaringType.GenericParameters.Count,
                                GenericParameterKind.Type ) ), true );
                    methodRef = (IGenericMethodDefinition) typeInstance.Methods.GetMethod(
                                                               method.Name, method, BindingOptions.Default );
                }
                else
                {
                    methodRef = (IGenericMethodDefinition) method.Translate( targetModule );
                }


                if ( method.IsGenericDefinition )
                {
                    methodInstance =
                        methodRef.MethodSpecs.GetGenericInstance(
                            targetModule.Cache.GetGenericParameterArray(
                                method.GenericParameters.Count, GenericParameterKind.Method ), true );
                }
                else
                {
                    methodInstance = methodRef;
                }
            }
            else
            {
                methodInstance = method;
            }

            return methodInstance;
        }

        /// <summary>
        /// Gets the canonical generic instance of a field. 
        /// </summary>
        /// <param name="field">field whose canonical generic instance is requested.</param>
        /// <returns>The canonical generic instance of <paramref name="field"/>, or
        /// the unmodified <paramref name="field"/> if the declaring type of <paramref name="field"/> 
        /// is not a generic type definition.</returns>
        /// <remarks>
        /// The canonical generic instance of a field is the field defined on the canonical generic
        /// instance of the declaring type. For instance, the canonical 
        /// generic instance of the type MyType`3::myField is MyType`3&lt;!0,!1,!2&gt;::myField.
        /// </remarks>
        public static IField GetFieldCanonicalGenericInstance( FieldDefDeclaration field )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( field, "field" );

            #endregion

            ModuleDeclaration module = field.Module;

            if ( field.DeclaringType.IsGenericDefinition )
            {
                TypeSpecDeclaration typeInstance = module.TypeSpecs.GetBySignature(
                    new GenericTypeInstanceTypeSignature(
                        field.DeclaringType,
                        field.Module.Cache.GetGenericParameterArray(
                            field.DeclaringType.GenericParameters.Count,
                            GenericParameterKind.Type ) ), true );

                return typeInstance.FieldRefs.GetField(
                    field.Name, field.FieldType, BindingOptions.Default );
            }
            else
            {
                return field;
            }
        }
    }
}