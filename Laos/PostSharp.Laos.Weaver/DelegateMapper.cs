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

using System.Collections.Generic;
using System.Globalization;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.TypeSignatures;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Maps method signatures to compatible delegates and generate these
    /// delegates on demand.
    /// </summary>
    public class DelegateMapper
    {
        private readonly Dictionary<IMethod, DelegateMap> delegateMapByMethod =
            new Dictionary<IMethod, DelegateMap>();

        private readonly Dictionary<DelegateSignature, TypeDefDeclaration> delegateTypeDefBySignature =
            new Dictionary<DelegateSignature, TypeDefDeclaration>();

        private readonly ModuleDeclaration module;
        private readonly TypeDefDeclaration containerType;

        internal DelegateMapper( TypeDefDeclaration containerType )
        {
            this.module = containerType.Module;
            this.containerType = containerType;
        }

        /// <summary>
        /// Maps a method signature to a delegate and returns the
        /// corresponding <see cref="DelegateMap"/>. Generates
        /// the delegate if needed.
        /// </summary>
        /// <param name="method">Method for which signature the
        /// <see cref="DelegateMap"/> is returned</param>
        /// <returns>The <see cref="DelegateMap"/> corresponding
        /// to <paramref name="method"/>.</returns>
        public DelegateMap GetDelegateMap( IMethod method )
        {
            DelegateMap delegateMap;
            if ( !this.delegateMapByMethod.TryGetValue( method, out delegateMap ) )
            {
                ITypeSignature delegateTypeSpec;
                GenericMap callerToDelegateInverseGenericMap;
                GenericMap callerToDelegateGenericMap;


                MethodDefDeclaration methodDef = method.GetMethodDefinition();
                DelegateSignature delegateSignature = new DelegateSignature( this.module );
                ITypeSignature[] inverseGenericTypeParameters = null;

                #region Construct the delegate signature

                // Constructs a generic signature of the delegate. Any generic argument should
                // be mapped to a generic argument.
                if ( methodDef.IsGenericDefinition || methodDef.DeclaringType.IsGenericDefinition )
                {
                    // Builds the GenericMap that will map the MethodDef signature on the delegate signature.
                    ITypeSignature[] genericTypeParametersOfMethodDef =
                        new ITypeSignature[methodDef.DeclaringType.GenericParameters.Count];

                    ITypeSignature[] genericMethodParametersOfMethodDef
                        = new ITypeSignature[methodDef.GenericParameters.Count];

                    inverseGenericTypeParameters = new ITypeSignature[
                        genericTypeParametersOfMethodDef.Length + genericMethodParametersOfMethodDef.Length];


                    // For each generic parameter, create a new generic parameter
                    // for the delegate signature, and we prepare generic maps.
                    // We don't add generic constraints yet, since we don't have generic maps.
                    bool hasConstraint = false;

                    if ( methodDef.DeclaringType.IsGenericDefinition )
                    {
                        ExceptionHelper.AssumeNotNull( genericTypeParametersOfMethodDef );

                        for ( int i = 0; i < methodDef.DeclaringType.GenericParameters.Count; i++ )
                        {
                            GenericParameterDeclaration referencedGenericParameter =
                                methodDef.DeclaringType.GenericParameters[i];

                            GenericParameterDeclaration delegateGenericParameter = new GenericParameterDeclaration
                                                                                       {
                                                                                           Kind =
                                                                                               GenericParameterKind.Type,
                                                                                           Ordinal = i
                                                                                       };
                            delegateGenericParameter.Name =
                                string.Format( CultureInfo.InvariantCulture, "T{0}", delegateGenericParameter.Ordinal );

                            if ( referencedGenericParameter.Constraints.Count > 0 )
                                hasConstraint = true;


                            delegateSignature.GenericParameters.Add( delegateGenericParameter );

                            // Build mappings.
                            genericTypeParametersOfMethodDef[referencedGenericParameter.Ordinal] =
                                delegateGenericParameter;

                            inverseGenericTypeParameters[delegateGenericParameter.Ordinal] = referencedGenericParameter;
                        }
                    }

                    if ( methodDef.IsGenericDefinition )
                    {
                        ExceptionHelper.AssumeNotNull( genericMethodParametersOfMethodDef );
                        for ( int i = 0; i < methodDef.GenericParameters.Count; i++ )
                        {
                            GenericParameterDeclaration referencedGenericParameter =
                                methodDef.GenericParameters[i];

                            GenericParameterDeclaration delegateGenericParameter = new GenericParameterDeclaration
                                                                                       {
                                                                                           Kind =
                                                                                               GenericParameterKind.Type,
                                                                                           Ordinal =
                                                                                               (methodDef.DeclaringType
                                                                                                    .
                                                                                                    GenericParameters.
                                                                                                    Count + i)
                                                                                       };
                            delegateGenericParameter.Name =
                                string.Format( CultureInfo.InvariantCulture, "T{0}", delegateGenericParameter.Ordinal );
                            delegateSignature.GenericParameters.Add( delegateGenericParameter );

                            if ( referencedGenericParameter.Constraints.Count > 0 )
                                hasConstraint = true;


                            // Build mappings.
                            genericMethodParametersOfMethodDef[referencedGenericParameter.Ordinal] =
                                delegateGenericParameter;

                            // We cannot give away the generic parameter (issue 305). We have to build an anonymous reference.
                            inverseGenericTypeParameters[delegateGenericParameter.Ordinal] =
                                GenericParameterTypeSignature.GetInstance( module, referencedGenericParameter.Ordinal,
                                                                           GenericParameterKind.Method );
                        }
                    }

                    callerToDelegateInverseGenericMap =
                        new GenericMap( genericTypeParametersOfMethodDef, genericMethodParametersOfMethodDef );
                    callerToDelegateGenericMap = new GenericMap( inverseGenericTypeParameters, null );

                    // Add generic constraints.
                    if ( hasConstraint )
                    {
                        // Contraints on type parameters.
                        for ( int i = 0; i < methodDef.DeclaringType.GenericParameters.Count; i++ )
                        {
                            GenericParameterDeclaration referencedGenericParameter =
                                methodDef.DeclaringType.GenericParameters[i];
                            GenericParameterDeclaration delegateGenericParameter = (GenericParameterDeclaration)
                                                                                   genericTypeParametersOfMethodDef[
                                                                                       referencedGenericParameter.
                                                                                           Ordinal];

                            foreach (
                                GenericParameterConstraintDeclaration constraint in
                                    referencedGenericParameter.Constraints )
                            {
                                delegateGenericParameter.Constraints.Add(
                                    constraint.ConstraintType.MapGenericArguments(
                                        callerToDelegateInverseGenericMap ) );
                            }
                        }

                        for ( int i = 0; i < methodDef.GenericParameters.Count; i++ )
                        {
                            GenericParameterDeclaration referencedGenericParameter =
                                methodDef.GenericParameters[i];
                            GenericParameterDeclaration delegateGenericParameter = (GenericParameterDeclaration)
                                                                                   genericMethodParametersOfMethodDef[
                                                                                       referencedGenericParameter.
                                                                                           Ordinal];
                            foreach (
                                GenericParameterConstraintDeclaration constraint in
                                    referencedGenericParameter.Constraints )
                            {
                                delegateGenericParameter.Constraints.Add(
                                    constraint.ConstraintType.MapGenericArguments(
                                        callerToDelegateInverseGenericMap ) );
                            }
                        }
                    }


                    delegateSignature.ReturnType =
                        methodDef.ReturnParameter.ParameterType.MapGenericArguments( callerToDelegateInverseGenericMap );
                    foreach ( ParameterDeclaration parameter in methodDef.Parameters )
                    {
                        ParameterDeclaration translatedParameter = parameter.Clone( module );
                        translatedParameter.ParameterType =
                            translatedParameter.ParameterType.MapGenericArguments( callerToDelegateInverseGenericMap );
                        delegateSignature.Parameters.Add( translatedParameter );
                    }
                }
                else
                {
                    callerToDelegateGenericMap = GenericMap.Empty;
                    callerToDelegateInverseGenericMap = GenericMap.Empty;

                    // The method definition does not reference any generic parameter.
                    delegateSignature.ReturnType = methodDef.ReturnParameter.ParameterType;
                    delegateSignature.Parameters.AddCloneRange( methodDef.Parameters );
                }

                #endregion

                #region Construct the delegate TypeDef

                TypeDefDeclaration delegateTypeDef;
                if ( !this.delegateTypeDefBySignature.TryGetValue( delegateSignature, out delegateTypeDef ) )
                {
                    string delegateName = string.Format( CultureInfo.InvariantCulture,
                                                         "~delegate~{0}", this.delegateTypeDefBySignature.Count );
                    // We do not have it. Build it.
                    delegateTypeDef = DelegateBuilder.BuildDelegate( this.containerType,
                                                                     delegateName, delegateSignature );

                    this.delegateTypeDefBySignature.Add( delegateSignature, delegateTypeDef );
                }

                #endregion

                #region Construct the delegate TypeSpec

                if ( !delegateTypeDef.IsGenericDefinition )
                {
                    delegateTypeSpec = delegateTypeDef;
                }
                else
                {
                    ExceptionHelper.AssumeNotNull( inverseGenericTypeParameters );

                    // We have to build a generic type instance. The collection of generic parameters
                    // on the DelegateSignature gives us the generic arguments we are looking for.

                    ITypeSignature[] delegateTypeSpecGenericParameters =
                        new ITypeSignature[delegateSignature.GenericParameters.Count];
                    for ( int i = 0; i < delegateTypeSpecGenericParameters.Length; i++ )
                    {
                        delegateTypeSpecGenericParameters[i] = inverseGenericTypeParameters[i];
                    }

                    // Create the delegate type signature.
                    delegateTypeSpec = new GenericTypeInstanceTypeSignature(
                        delegateTypeDef, delegateTypeSpecGenericParameters );
                }

                delegateMap = new DelegateMap( delegateTypeSpec,
                                               callerToDelegateGenericMap,
                                               callerToDelegateInverseGenericMap );

                this.delegateMapByMethod.Add( method, delegateMap );

                #endregion
            }

            return delegateMap;
        }
    }

    /// <summary>
    /// Represent a mapping between a method signature and a delegate.
    /// </summary>
    public sealed class DelegateMap
    {
        private readonly ITypeSignature delegateTypeSpec;
        private readonly GenericMap callerToDelegateGenericMap;
        private readonly GenericMap callerToDelegateInverseGenericMap;

        internal DelegateMap( ITypeSignature delegateTypeSpec, GenericMap callerToDelegateGenericMap,
                              GenericMap callerToDelegateInverseGenericMap )
        {
            this.delegateTypeSpec = delegateTypeSpec;
            this.callerToDelegateInverseGenericMap = callerToDelegateInverseGenericMap;
            this.callerToDelegateGenericMap = callerToDelegateGenericMap;
        }

        /// <summary>
        /// Gets the <b>TypeSpec</b> of the corresponding delegate.
        /// </summary>
        public ITypeSignature DelegateTypeSpec
        {
            get { return delegateTypeSpec; }
        }

        /// <summary>
        /// Gets the <see cref="GenericMap"/> mapping the generic context
        /// of the caller method to generic arguments of the delegate
        /// <b>TypeSpec</b>.
        /// </summary>
        public GenericMap CallerToDelegateGenericMap
        {
            get { return callerToDelegateGenericMap; }
        }

        /// <summary>
        /// Gets the inverse <see cref="GenericMap"/> to <see cref="CallerToDelegateGenericMap"/>.
        /// </summary>
        public GenericMap CallerToDelegateInverseGenericMap
        {
            get { return callerToDelegateInverseGenericMap; }
        }
    }
}