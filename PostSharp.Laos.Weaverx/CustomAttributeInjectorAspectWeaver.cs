using System;
using System.Collections.Generic;
using PostSharp.CodeModel;
using PostSharp.Extensibility;
using PostSharp.ModuleWriter;

namespace PostSharp.Laos.Weaver
{
    internal class CustomAttributeInjectorAspectWeaver : LaosAspectWeaver
    {
        private static readonly CustomAttributeInjectorAspectConfigurationAttribute defaultConfiguration =
            new CustomAttributeInjectorAspectConfigurationAttribute();

        public CustomAttributeInjectorAspectWeaver() : base( defaultConfiguration )
        {
        }

        public override void EmitAspectConstruction( InstructionEmitter writer )
        {
        }

        public override void EmitRuntimeInitialization( InstructionEmitter writer )
        {
        }

        public override Type GetSerializerType()
        {
            return null;
        }

        public override bool ValidateSelf()
        {
            return true;
        }

        public override bool RequiresImplementation
        {
            get { return true; }
        }

        public override bool RequiresRuntimeInstance
        {
            get { return false; }
        }

        public override void Implement()
        {
            ICustomAttributeInjectorAspect aspect = (ICustomAttributeInjectorAspect) this.Aspect;

            IMethod constructor = this.Task.Project.Module.FindMethod( aspect.CustomAttribute.Constructor,
                                                                       BindingOptions.Default );
            TypeDefDeclaration attributeTypeDef = constructor.DeclaringType.GetTypeDefinition();

            CustomAttributeDeclaration customAttribute = new CustomAttributeDeclaration( constructor );

            bool hasError = false;
            int i;

            for ( i = 0; i < aspect.CustomAttribute.ConstructorArguments.Count; i++ )
            {
                object argument = aspect.CustomAttribute.ConstructorArguments[i];

                SerializedValue serializedValue = GetSerializedValue( constructor.DeclaringType,
                                                                      string.Format( "constructor argument {0}", i ),
                                                                      constructor.GetParameterType( i ), argument );

                if ( serializedValue != null )
                {
                    customAttribute.ConstructorArguments.Add( new MemberValuePair( MemberKind.Parameter, i,
                                                                                   i.ToString(),
                                                                                   serializedValue
                                                                  ) );
                }
                else hasError = true;
            }

            i = 0;
            foreach ( KeyValuePair<string, object> pair in aspect.CustomAttribute.NamedArguments )
            {
                MemberKind memberKind;
                ITypeSignature argumentType;
                PropertyDeclaration property = attributeTypeDef.FindProperty( pair.Key );
                if ( property != null )
                {
                    memberKind = MemberKind.Property;
                    argumentType = property.PropertyType.Translate( this.Task.Project.Module );
                }
                else
                {
                    FieldDefDeclaration field = attributeTypeDef.FindField( pair.Key );

                    if ( field != null )
                    {
                        memberKind = MemberKind.Field;
                        argumentType = field.FieldType.Translate( this.Task.Project.Module );
                    }
                    else
                    {
                        throw new AssertionFailedException(
                            string.Format( "Cannot find a field or property named {0} on type {1}.",
                                           pair.Key, attributeTypeDef ) );
                    }
                }

                SerializedValue serializedValue = GetSerializedValue( constructor.DeclaringType,
                                                                      string.Format( "{0} {1}", memberKind, pair.Key ),
                                                                      argumentType, pair.Value );

                if ( serializedValue != null )
                {
                    customAttribute.NamedArguments.Add( new MemberValuePair( memberKind, i,
                                                                             pair.Key,
                                                                             serializedValue ) );
                }
                else hasError = true;

                i++;
            }

            if ( !hasError )
            {
                this.TargetElement.CustomAttributes.Add( customAttribute );
            }
        }


        private SerializedValue GetSerializedValue( IType attributeType, string argumentName,
                                                    ITypeSignature argumentType, object argumentValue )
        {
            try
            {
                SerializationType serializationType = SerializationType.GetSerializationType( argumentType );
                serializationType.ValidateRuntimeValue( argumentValue );
                return serializationType.FromRuntimeValue( argumentValue );
            }
            catch ( ArgumentException e )
            {
                // Cannot serialize the custom attribute of type {0} on {1} {2}: error on {3}: {4}.
                LaosMessageSource.Instance.Write( SeverityType.Error,
                                                  "LA0030", new object[]
                                                                {
                                                                    attributeType, 
                                                                    this.TargetElement.GetTokenType().ToString().ToLowerInvariant(),
                                                                    this.TargetElement,
                                                                    argumentName, 
                                                                    e.Message
                                                                } );
                return null;
            }
        }
    }
}