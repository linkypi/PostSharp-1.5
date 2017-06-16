using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using PostSharp.CodeModel.Binding;

namespace PostSharp.CodeModel.Helpers
{
    internal static class CompareHelper
    {
        internal static bool Equals(INamedType definition, INamedType reference, bool strict)
        {
            
            if ( definition.Name != reference.Name )
                return false;

            if ( definition.DeclaringType != null )
            {
                if ( !((ITypeSignatureInternal) definition.DeclaringType).Equals( reference.DeclaringType, strict ))
                    return false;
            }
            else
            {
                if (!((IAssemblyInternal) definition.DeclaringAssembly).Equals( reference.DeclaringAssembly, strict ))
                    return false;
            }

            return true;

        }

        internal static bool Equals(IMethod x, IMethod y, bool strict)
        {
            // Test for nulls.
            if ( x == null )
            {
                if ( y == null )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if ( y == null )
            {
                return false;
            }


            // Compare the method name
            if ( x.Name != y.Name )
            {
                return false;
            }

            // Compare the declaring type or assembly.
            if ( x.DeclaringType != null )
            {
                if ( !((ITypeSignatureInternal) x.DeclaringType.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers )).Equals( y.DeclaringType.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ), strict ) )
                {
                    return false;
                }
            }
            else
            {
                if (
                    !((IAssemblyInternal) x.DeclaringType.DeclaringAssembly).Equals( y.DeclaringType.DeclaringAssembly, strict ) )
                {
                    return false;
                }
            }

            // Compare signatures.
            return Equals( (IMethodSignature) x, y, strict );
        }

        /// <inheritdoc />
        internal static bool Equals( IMethodSignature definition, IMethodSignature reference, bool strict )
        {
            if ( definition == reference )
                return true;

            // Check nulls.
            if ( definition == null )
            {
                if ( reference == null )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if ( reference == null )
            {
                return false;
            }


            // Compare parameters and return value.
            if ( definition.ParameterCount != reference.ParameterCount )
            {
                return false;
            }

            if ( !((ITypeSignatureInternal) definition.ReturnType.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers )).Equals( reference.ReturnType.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ), strict ) )
            {
                return false;
            }

            for ( int i = 0 ; i < definition.ParameterCount ; i++ )
            {
                if ( !((ITypeSignatureInternal)definition.GetParameterType( i ).GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers )).Equals(  reference.GetParameterType( i ).GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ), strict ) )
                {
                    return false;
                }
            }

            // Compare generic parameters.
            IGenericInstance genericX = definition as IGenericInstance;
            IGenericInstance genericY = reference as IGenericInstance;

            if ( genericX == null && genericY == null )
            {
                return true;
            }

            if ( genericX == null || genericY == null )
            {
                return false;
            }

            if ( genericX.IsGenericInstance != genericY.IsGenericInstance )
            {
                return false;
            }

            if ( genericX.GenericArgumentCount != genericY.GenericArgumentCount )
            {
                return false;
            }

            for ( int i = 0 ; i < genericX.GenericArgumentCount ; i++ )
            {
                if ( !((ITypeSignatureInternal) genericX.GetGenericArgument( i ).GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers )).Equals( genericY.GetGenericArgument( i ).GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ), strict ) )
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public static bool Equals(IAssemblyName definition, IAssemblyName reference, AssemblyRedirectionPolicyManager redirectionPolicyManager, bool strict)
        {
            definition = redirectionPolicyManager.GetCanonicalAssemblyName(definition);
            reference = redirectionPolicyManager.GetCanonicalAssemblyName(reference);


            if (
                String.Compare(definition.Name, reference.Name, StringComparison.InvariantCulture) !=
                0)
            {
                return false;
            }


            // Compare the culture.
            if (String.IsNullOrEmpty(reference.Culture))
            {
                if (strict && !String.IsNullOrEmpty(definition.Culture))
                {
                    return false;
                }
            }
            else if (definition.Culture != reference.Culture)
            {
                return false;
            }

            // Compare the public key.
            byte[] definitionPublicKeyToken = definition.GetPublicKeyToken();
            byte[] referencePublicKeyToken = reference.GetPublicKeyToken();

            if (referencePublicKeyToken != null && referencePublicKeyToken.Length > 0)
            {
                if (definitionPublicKeyToken == null)
                {
                    return false;
                }

                if (!CompareBytes(definitionPublicKeyToken, referencePublicKeyToken))
                {
                    return false;
                }

                // Compare the version.
                if (reference.Version == null)
                {
                    if (strict && definition.Version != null)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!definition.Version.Equals(reference.Version))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (strict && referencePublicKeyToken != null && referencePublicKeyToken.Length > 0)
                {
                    return false;
                }
            }

            


            return true;


        }

        /// <summary>
        /// Determines whether two public keys are equal.
        /// </summary>
        /// <param name="x">First array of bytes.</param>
        /// <param name="y">Second array of bytes.</param>
        /// <returns><b>true</b> if both public keys are equal, otherwise <b>false</b>.</returns>
        internal static bool CompareBytes( byte[] x, byte[] y )
        {
            if ( x == null || y == null )
            {
                return false;
            }

            if ( x.Length != y.Length )
            {
                return false;
            }

            for ( int i = 0 ; i < x.Length ; i++ )
            {
                if ( x[i] != y[i] )
                {
                    return false;
                }
            }

            return true;
        }
    }
}
