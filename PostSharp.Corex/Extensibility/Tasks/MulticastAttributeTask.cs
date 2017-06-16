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
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.SerializationTypes;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;

namespace PostSharp.Extensibility.Tasks
{
    /// <summary>
    /// This task analyze instances of custom attributes derived from <see cref="MulticastAttribute"/> and propagate
    /// them on their target elements in the custom attribute dictionary (<see cref="AnnotationRepositoryTask"/>)
    /// of this module.
    /// </summary>
    public class MulticastAttributeTask : Task
    {
        private readonly Dictionary
            <TokenType,
                Dictionary<IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>>
            instances =
                new Dictionary
                    <TokenType,
                        Dictionary
                            <IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>>();

        private long nextAttributeId;
        private AnnotationRepositoryTask annotationRepository;
        private readonly Dictionary<IAssembly, bool> assemblyHasInheritedAttributes = new Dictionary<IAssembly, bool>();

        private readonly Dictionary<TypeDefDeclaration, MulticastAttributeUsageAttribute> multicastAttributeUsages =
            new Dictionary<TypeDefDeclaration, MulticastAttributeUsageAttribute>();

        private readonly Set<MethodDefDeclaration> methodsWithInheritedAttributes = new Set<MethodDefDeclaration>();
        private static readonly Set<string> systemProperties = new Set<string>();

        private readonly Dictionary<MetadataDeclaration, CustomAttributeDeclaration> hasInheritedAttributesAttributes =
            new Dictionary<MetadataDeclaration, CustomAttributeDeclaration>();

        private readonly Dictionary<CustomAttributeDeclaration, MulticastAttributeInstanceInfo> instanceInfos =
            new Dictionary<CustomAttributeDeclaration, MulticastAttributeInstanceInfo>();

        private readonly Dictionary<long, CustomAttributeDeclaration> writtenPooledInheritableAttributes =
            new Dictionary<long, CustomAttributeDeclaration>();

        private IType compilerGeneratedAttributeType;

        static MulticastAttributeTask()
        {
            foreach (
                PropertyInfo property in
                    typeof(MulticastAttribute).GetProperties( BindingFlags.Instance | BindingFlags.Public ) )
            {
                systemProperties.Add( property.Name );
            }
        }

        private static MulticastAttributeUsageAttribute BuildMulticastAttributeUsageAttribute( IAnnotationValue data )
        {
            MulticastAttributeUsageAttribute attribute =
                new MulticastAttributeUsageAttribute( (MulticastTargets) data.ConstructorArguments[0].Value.Value );
            foreach ( MemberValuePair namedArgument in data.NamedArguments )
            {
                switch ( namedArgument.MemberName )
                {
                    case "AllowMultiple":
                        attribute.AllowMultiple = (bool) namedArgument.Value.Value;
                        break;

                    case "AllowExternalAssemblies":
                        attribute.AllowExternalAssemblies = (bool) namedArgument.Value.Value;
                        break;

                    case "PersistMetaData":
                        attribute.PersistMetaData = (bool) namedArgument.Value.Value;
                        break;

                    case "TargetMemberAttributes":
                        attribute.TargetMemberAttributes = (MulticastAttributes) namedArgument.Value.Value;
                        break;

                    case "TargetTypeAttributes":
                        attribute.TargetTypeAttributes = (MulticastAttributes) namedArgument.Value.Value;
                        break;

                    case "Inheritance":
                        attribute.Inheritance = (MulticastInheritance) namedArgument.Value.Value;
                        break;


                    default:
                        throw new AssertionFailedException();
                }
            }

            return attribute;
        }

        #region Task implementation

        /// <inheritdoc />
        public override bool Execute()
        {
            this.module = this.Project.Module;

            #region Read or compute the assembly identifier

            CustomAttributeDeclaration assemblyIdAttribute = 
                this.module.AssemblyManifest.CustomAttributes.GetOneByType( (IType) this.module.GetTypeForFrameworkVariant( typeof(AssemblyIdAttribute) ) );
            if ( assemblyIdAttribute != null )
            {
                this.nextAttributeId = (int) assemblyIdAttribute.ConstructorArguments[0].Value.GetRuntimeValue();
            }
            else
            {
                MD5 md5 = MD5.Create();
                byte[] hash = md5.ComputeHash( Encoding.UTF8.GetBytes( this.module.Name ) );
                this.nextAttributeId = 
                    hash[0] << 24 |
                    hash[1] << 16 |
                    hash[2] << 8 |
                    hash[3];
            }

            this.nextAttributeId = (this.nextAttributeId << 32) + 1;


            #endregion

            #region Read and index custom attributs


            this.multicastAttributesType =
                (INamedType) this.module.GetTypeForFrameworkVariant( typeof(MulticastAttributes) );
            this.multicastTargetsType = (INamedType) this.module.GetTypeForFrameworkVariant( typeof(MulticastTargets) );
            this.multicastInheritanceType =
                (INamedType) this.module.GetTypeForFrameworkVariant( typeof(MulticastInheritance) );
            this.hasInheritedAttributesAttributeConstructor =
                this.module.FindMethod(
                    this.module.GetTypeForFrameworkVariant( typeof(HasInheritedAttributeAttribute) ),
                    ".ctor" );
            this.annotationRepository = AnnotationRepositoryTask.GetTask( this.Project );
            this.compilerGeneratedAttributeType =
                (IType) this.Project.Module.Cache.GetType( typeof(CompilerGeneratedAttribute) );

            Trace.MulticastAttributeTask.WriteLine( "Starting to load custom attributes." );

            TypeDefDeclaration multicastAttributeType =
                this.module.GetTypeForFrameworkVariant( typeof(MulticastAttribute) ).GetTypeDefinition();

            IEnumerator<TypeDefDeclaration> typeEnumerator =
                TypeHierarchyTask.GetDerivedTypesEnumerator( multicastAttributeType, true, null );

            while ( typeEnumerator.MoveNext() )
            {
                // Retrieve the Usage custom attribute on the MulticastAttribute class.
                // This requires a recursion on parent types.
                TypeDefDeclaration typeDef = typeEnumerator.Current;

                if ( ( typeDef.Attributes & TypeAttributes.Abstract ) != 0 )
                {
                    continue;
                }

                Trace.MulticastAttributeTask.WriteLine( "Loading custom attribute type {{{0}}}.", typeDef );

                MulticastAttributeUsageAttribute customAttributeUsage = GetCustomAttributeUsage( typeDef );

                if ( customAttributeUsage == null )
                    continue;

                // Index all instances of this type of custom attributes.
                IEnumerator<IAnnotationInstance> attributeEnumerator =
                    annotationRepository.GetAnnotationsOfType( typeEnumerator.Current, false );

                List<IAnnotationInstance> allInstances = new List<IAnnotationInstance>();
                while ( attributeEnumerator.MoveNext() )
                {

                    // We have to ignore custom attributes that are on compiler-generated code.
                    if (CompilerWorkarounds.IsCompilerGenerated(attributeEnumerator.Current.TargetElement))
                    {
                        continue;
                    }

                    MulticastAttributeInstanceInfo instanceInfo =
                        GetInstanceInfo( attributeEnumerator.Current.TargetElement, attributeEnumerator.Current.Value,
                                         true );

                    if ( instanceInfo == null )
                        continue;

                    PropagateIntermediateInstance( instanceInfo.DeclaredOn, instanceInfo );

                    allInstances.Add( attributeEnumerator.Current );
                }

                // Remove these custom attributes.
                foreach ( IAnnotationInstance instance in allInstances )
                {
                    annotationRepository.RemoveAnnotation( instance );
                    CustomAttributeDeclaration customAttribute = instance.Value as CustomAttributeDeclaration;
                    if (customAttribute != null)
                    {
                        instance.TargetElement.CustomAttributes.Remove( customAttribute );
                    }
                }
            }

            // Fail here in case of problem.
            if ( Messenger.Current.ErrorCount > 0 )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0055", null );
            }

            #endregion

            this.ProcessInheritedAttributesAcrossAssemblies();

            // Process elements (from the most external container to the most internal).
            Trace.MulticastAttributeTask.WriteLine( "Processing custom attributes on Assembly." );
            this.ProcessAssemblyOrModule( TokenType.Assembly );
            Trace.MulticastAttributeTask.WriteLine( "Processing custom attributes on Module." );
            this.ProcessAssemblyOrModule( TokenType.Module );
            Trace.MulticastAttributeTask.WriteLine( "Processing custom attributes on TypeDef." );
            this.ProcessTypes( TokenType.TypeDef );
            Trace.MulticastAttributeTask.WriteLine( "Processing custom attributes on TypeRef." );
            this.ProcessTypes( TokenType.TypeRef );
            Trace.MulticastAttributeTask.WriteLine( "Processing custom attributes on Property." );
            this.ProcessMethodSemanticCollections( TokenType.Property, MulticastTargets.Property );
            Trace.MulticastAttributeTask.WriteLine( "Processing custom attributes on Event." );
            this.ProcessMethodSemanticCollections( TokenType.Event, MulticastTargets.Event );
            Trace.MulticastAttributeTask.WriteLine( "Processing custom attributes on FieldDef." );
            this.ProcessFields( TokenType.FieldDef );
            Trace.MulticastAttributeTask.WriteLine( "Processing custom attributes on FieldRef." );
            this.ProcessFields( TokenType.MemberRef );
            Trace.MulticastAttributeTask.WriteLine( "Processing custom attributes on MethodDef." );


            // Before processing the methods, we should propagate inherited attributes.
            foreach ( MethodDefDeclaration methodDef in this.methodsWithInheritedAttributes )
            {
                foreach (
                    MethodDefDeclaration childMethod in FindMethodOverrides( methodDef.DeclaringType, 
                        new MappedGenericDeclaration<MethodDefDeclaration>(methodDef, methodDef.GetGenericContext( GenericContextOptions.None )), false )
                    )
                {
                    CopyInheritedIntermediateInstances( methodDef, childMethod );
                }
            }

            this.ProcessMethods( TokenType.MethodDef );
            Trace.MulticastAttributeTask.WriteLine( "Processing custom attributes on MethodRef." );
            this.ProcessMethods( TokenType.MemberRef );
            Trace.MulticastAttributeTask.WriteLine( "Processing custom attributes on Parameters." );
            this.ProcessParameters();

            return true;
        }

        private void PropagateIntermediateInstance( IMetadataDeclaration target,
                                                    MulticastAttributeInstanceInfo instanceInfo )
        {
            switch ( target.GetTokenType() )
            {
                case TokenType.TypeDef:
                    this.PropagateIntermediateInstance( (TypeDefDeclaration) target, instanceInfo );
                    break;

                case TokenType.MethodDef:
                    this.PropagateIntermediateInstance( (MethodDefDeclaration) target, instanceInfo );
                    break;

                case TokenType.ParamDef:
                    this.PropagateIntermediateInstance( (ParameterDeclaration) target, instanceInfo );
                    break;

                default:
                    this.AddIntermediateInstance( instanceInfo );
                    break;
            }
        }


        private MulticastAttributeUsageAttribute GetCustomAttributeUsage( TypeDefDeclaration typeDef )
        {
            MulticastAttributeUsageAttribute customAttributeUsage;

            if ( !multicastAttributeUsages.TryGetValue( typeDef, out customAttributeUsage ) )
            {
                TypeDefDeclaration attributeDefiningType = typeDef;
                bool hasError = false;
                Stack<MulticastAttributeUsageAttribute> usageStack = new Stack<MulticastAttributeUsageAttribute>();
                while ( attributeDefiningType != null )
                {
                    IEnumerator<CustomAttributeDeclaration> customAttributeEnumerator =
                        attributeDefiningType.CustomAttributes.GetByTypeEnumerator( (IType)
                                                                                    attributeDefiningType.Module.
                                                                                        GetTypeForFrameworkVariant(
                                                                                        typeof(
                                                                                            MulticastAttributeUsageAttribute
                                                                                            ) ) );

                    if ( customAttributeEnumerator != null && customAttributeEnumerator.MoveNext() )
                    {
                        Trace.MulticastAttributeTask.WriteLine( "Usage information were found on type {{{0}}}.",
                                                                attributeDefiningType );

                        usageStack.Push( BuildMulticastAttributeUsageAttribute( customAttributeEnumerator.Current ) );

                        // Allow only a single instance per type.
                        if ( customAttributeEnumerator.MoveNext() )
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Error, "PS0054",
                                                              new object[] {attributeDefiningType} );
                            hasError = true;
                        }
                    }

                    attributeDefiningType = attributeDefiningType.BaseType != null
                                                ? attributeDefiningType.BaseType.GetTypeDefinition()
                                                : null;
                }

                // Skip this type?
                if ( hasError )
                {
                    customAttributeUsage = null;
                    goto end;
                }

                // No custom attribute. Skip this type.
                if ( usageStack.Count == 0 )
                {
                    CoreMessageSource.Instance.Write( SeverityType.Error, "PS0051",
                                                      new object[] {typeDef} );
                    customAttributeUsage = null;
                    goto end;
                }

                // Get the base usage attribute.
                customAttributeUsage = MulticastAttributeUsageAttribute.GetMaximumValue();

                // Override it.
                while ( usageStack.Count > 0 )
                {
                    MulticastAttributeUsageAttribute usageOverride = usageStack.Pop();

                    // Override ValidOn.
                    if ( ( usageOverride.ValidOn & ~customAttributeUsage.ValidOn ) != 0 )
                    {
                        CoreMessageSource.Instance.Write( SeverityType.Error, "PS0072",
                                                          new object[] {typeDef, "ValidOn"} );
                        hasError = true;
                        break;
                    }

                    customAttributeUsage.ValidOn &= usageOverride.ValidOn;

                    // Override AllowExternalAssemblies.
                    if ( usageOverride.IsAllowExternalAssembliesSpecified )
                    {
                        if ( usageOverride.AllowExternalAssemblies && !customAttributeUsage.AllowExternalAssemblies )
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Error, "PS0072",
                                                              new object[] {typeDef, "AllowExternalAssemblies"} );
                            hasError = true;
                            break;
                        }

                        customAttributeUsage.AllowExternalAssemblies &= usageOverride.AllowExternalAssemblies;
                    }

                    // Override AllowMultiple.
                    if ( usageOverride.IsAllowMultipleSpecified )
                    {
                        if ( usageOverride.AllowMultiple && !customAttributeUsage.AllowMultiple )
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Error, "PS0072",
                                                              new object[] {typeDef, "AllowMultiple"} );
                            hasError = true;
                            break;
                        }

                        customAttributeUsage.AllowMultiple &= usageOverride.AllowMultiple;
                    }

                    // Override PersistMetaData.
                    if ( usageOverride.IsPersistMetaDataSpecified )
                    {
                        customAttributeUsage.PersistMetaData = usageOverride.PersistMetaData;
                    }

                    // Override Inherited.
                    if ( usageOverride.IsInheritanceSpecified )
                    {
                        if ( customAttributeUsage.IsInheritanceSpecified )
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Error, "PS0103",
                                                              new object[] {typeDef, "Inheritance"} );
                            hasError = true;
                            break;
                        }
                        customAttributeUsage.Inheritance = usageOverride.Inheritance;
                    }

                    // Override TargetParameterAttributes
                    if ( usageOverride.IsTargetParameterAttributesSpecified )
                    {
                        // We should apply the attribute part by part (or mask by mask).
                        // If a part is not specified, it is inherited from above.
                        usageOverride.TargetParameterAttributes =
                            ApplyMulticastAttribute( customAttributeUsage.TargetParameterAttributes,
                                                     usageOverride.TargetParameterAttributes );

                        if ( ( usageOverride.TargetParameterAttributes & ~customAttributeUsage.TargetParameterAttributes ) !=
                             0 )
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Error, "PS0072",
                                                              new object[] {typeDef, "TargetParameterAttributes"} );
                            hasError = true;
                            break;
                        }

                        customAttributeUsage.TargetParameterAttributes &= usageOverride.TargetParameterAttributes;
                    }

                    // Override TargetMemberAttributes
                    if ( usageOverride.IsTargetMemberAttributesSpecified )
                    {
                        // We should apply the attribute part by part (or mask by mask).
                        // If a part is not specified, it is inherited from above.
                        usageOverride.TargetMemberAttributes =
                            ApplyMulticastAttribute( customAttributeUsage.TargetMemberAttributes,
                                                     usageOverride.TargetMemberAttributes );

                        if ( ( usageOverride.TargetMemberAttributes & ~customAttributeUsage.TargetMemberAttributes ) !=
                             0 )
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Error, "PS0072",
                                                              new object[] {typeDef, "TargetMemberAttributes"} );
                            hasError = true;
                            break;
                        }

                        customAttributeUsage.TargetMemberAttributes &= usageOverride.TargetMemberAttributes;
                    }

                    // Override TargetTypeAttributes
                    if ( usageOverride.IsTargetTypeAttributesSpecified )
                    {
                        // We should apply the attribute part by part (or mask by mask).
                        // If a part is not specified, it is inherited from above.
                        usageOverride.TargetTypeAttributes =
                            ApplyMulticastAttribute( customAttributeUsage.TargetTypeAttributes,
                                                     usageOverride.TargetTypeAttributes );


                        if ( ( usageOverride.TargetTypeAttributes & ~customAttributeUsage.TargetTypeAttributes ) != 0 )
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Error, "PS0072",
                                                              new object[] {typeDef, "TargetTypeAttributes"} );
                            hasError = true;
                            break;
                        }

                        customAttributeUsage.TargetTypeAttributes &= usageOverride.TargetTypeAttributes;
                    }
                }

                // Skip this type?
                if ( hasError )
                {
                    customAttributeUsage = null;
                    goto end;
                }

                end:
                this.multicastAttributeUsages.Add( typeDef, customAttributeUsage );
            }

            return customAttributeUsage;
        }

        #endregion

        #region Process declarations

        private void ProcessAssemblyOrModule( TokenType tokenType )
        {
            string assemblyName = this.module.AssemblyManifest.Name;

            // Detect instances on this assembly.
            List<MulticastAttributeInstanceInfo> instancesOnThisAssembly = new List<MulticastAttributeInstanceInfo>();
            MultiDictionary<AssemblyRefDeclaration, MulticastAttributeInstanceInfo> instancesOnAssemblyRefs =
                new MultiDictionary<AssemblyRefDeclaration, MulticastAttributeInstanceInfo>();


            // Browser all custom attributes defined on the module or the assembly and
            // copy it into the proper subcollection.
            Dictionary<IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>
                instancesOnTokenType;
            if ( this.instances.TryGetValue( tokenType, out instancesOnTokenType ) )
            {
                foreach (
                    MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo> myInstances in
                        instancesOnTokenType.Values )
                {
                    IEnumerator<KeyValuePair<ITypeSignature, MulticastAttributeInstanceInfo>> enumerator =
                        myInstances.GetEnumerator();

                    while ( enumerator.MoveNext() )
                    {
                        MulticastAttributeInstanceInfo instanceInfo = enumerator.Current.Value;

                        // If relevant, copy the current instance into the collections of instances 
                        // on TypeDef.
                        if ( instanceInfo.TargetAssembliesRegex == null ||
                             instanceInfo.TargetAssembliesRegex.IsMatch( assemblyName ) )
                        {
                            if ( ( instanceInfo.TargetElements & MulticastTargets.Assembly ) != 0 )
                            {
                                instancesOnThisAssembly.Add( instanceInfo );
                            }


                            // Apply on TypeDefs.
                            IEnumerator<MetadataDeclaration> typeDefEnumerator =
                                this.module.Tables.GetEnumerator( TokenType.TypeDef );

                            while ( typeDefEnumerator.MoveNext() )
                            {
                                TypeDefDeclaration typeDef = (TypeDefDeclaration) typeDefEnumerator.Current;

                                if (CompareTypeAttributes(instanceInfo.TargetTypeAttributes, typeDef))
                                {
                                    if ( instanceInfo.TargetTypesRegex == null )
                                    {
                                        this.PropagateIntermediateInstance( typeDef, instanceInfo );
                                    }
                                    else
                                    {
                                        StringBuilder builder = new StringBuilder();
                                        typeDef.WriteReflectionTypeName( builder, ReflectionNameOptions.None );

                                        if ( instanceInfo.TargetTypesRegex.IsMatch( builder.ToString() ) )
                                        {
                                            this.PropagateIntermediateInstance( typeDef, instanceInfo );
                                        }
                                    }
                                }
                            }
                        }

                        // Copy the current instance into the collection of instances
                        // on TypeRef in the relevant assembly.
                        if ( instanceInfo.TargetAssembliesRegex != null && instanceInfo.Usage.AllowExternalAssemblies )
                        {
                            foreach ( AssemblyRefDeclaration assemblyRef in this.module.AssemblyRefs )
                            {
                                if ( instanceInfo.TargetAssembliesRegex.IsMatch( assemblyRef.Name ) )
                                {
                                    foreach ( TypeRefDeclaration typeRef in assemblyRef.TypeRefs )
                                    {
                                        if ( instanceInfo.TargetTypesRegex == null ||
                                             instanceInfo.TargetTypesRegex.IsMatch( typeRef.Name ) )
                                        {
                                            if ( ( instanceInfo.TargetElements & MulticastTargets.Assembly ) != 0 )
                                            {
                                                instancesOnAssemblyRefs.Add( assemblyRef, instanceInfo );
                                            }

                                            this.AddIntermediateInstance( typeRef, instanceInfo );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            AddMergedCustomAttributes( this.module.AssemblyManifest, instancesOnThisAssembly, false );

            foreach ( AssemblyRefDeclaration assemblyRef in instancesOnAssemblyRefs.Keys )
            {
                this.AddMergedCustomAttributes( assemblyRef,
                                                new List<MulticastAttributeInstanceInfo>(
                                                    instancesOnAssemblyRefs[assemblyRef] ), false );
            }
        }

        private void ProcessTypes( TokenType tokenType )
        {
            Dictionary<IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>
                instancesOnTokenType;
            if ( !this.instances.TryGetValue( tokenType, out instancesOnTokenType ) )
            {
                return;
            }


            IEnumerator
                <KeyValuePair<IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>>
                typeEnumerator = instancesOnTokenType.GetEnumerator();

            while ( typeEnumerator.MoveNext() )
            {
                INamedType type = (INamedType) typeEnumerator.Current.Key;

                if ( type.Name == inheritedAttributesAttributesPoolTypeName )
                    continue;

                TypeDefDeclaration typeDef = type.GetTypeDefinition();

                List<MulticastAttributeInstanceInfo> instancesOnFields = null,
                                                     instancesOnMethods = null,
                                                     instancesOnProperties = null,
                                                     instancesOnEvents = null;

                foreach ( ITypeSignature attributeType in typeEnumerator.Current.Value.Keys )
                {
                    // List of instances of type "attributeType" on the type "type".
                    List<MulticastAttributeInstanceInfo> concreteInstancesOnThisType =
                        new List<MulticastAttributeInstanceInfo>();
                    List<MulticastAttributeInstanceInfo> abstractInstancesOnThisType =
                        new List<MulticastAttributeInstanceInfo>();

                    #region Detect instances

                    foreach ( MulticastAttributeInstanceInfo instance in typeEnumerator.Current.Value[attributeType] )
                    {
                        MulticastTargets candidateTarget = GetTypeTarget( typeDef );
                        bool isAbstract = false;

                        // Check that the annotation is applied to the proper element kind.
                        if ( ( ( instance.TargetElements & MulticastTargets.AnyType ) == MulticastTargets.AnyType ) ||
                             ( ( instance.TargetElements & MulticastTargets.Class ) != 0 &&
                               candidateTarget == MulticastTargets.Class ) ||
                             ( ( instance.TargetElements & MulticastTargets.Delegate ) != 0 &&
                               candidateTarget == MulticastTargets.Delegate ) ||
                             ( ( instance.TargetElements & MulticastTargets.Enum ) != 0 &&
                               candidateTarget == MulticastTargets.Enum ) ||
                             ( ( instance.TargetElements & MulticastTargets.Interface ) != 0 &&
                               candidateTarget == MulticastTargets.Interface ) ||
                             ( ( instance.TargetElements & MulticastTargets.Struct ) != 0 &&
                               candidateTarget == MulticastTargets.Struct ) )
                        {
                            // Check attributes.

                            if (
                                !CompareTypeAttributes( instance.TargetTypeAttributes, typeDef ) )
                            {
                                if ( instance.Inherited )
                                {
                                    isAbstract = true;
                                }
                                else if ( type == instance.DeclaredOn )
                                {
                                    // Cannot apply the custom attribute "{0}" on {1} "{2}" because of incompatible attributes (visibility, scope, virtuality, ...).
                                    CoreMessageSource.Instance.Write( SeverityType.Error,
                                                                      "PS0091",
                                                                      new object[]
                                                                          {
                                                                              attributeType.ToString(),
                                                                              "type",
                                                                              typeDef.Name
                                                                          } );
                                    continue;
                                }
                                else
                                {
                                    throw new AssertionFailedException(
                                        string.Format(
                                            "Cannot apply instance {0} to type {1}: incompatible attributes.",
                                            instance, type ) );
                                }
                            }
                        }
                        else
                        {
                            if ( instance.Inheritance != MulticastInheritance.None )
                            {
                                isAbstract = true;
                            }
                            else if ( instance.DeclaredOn == typeDef &&
                                      ( instance.TargetElements &
                                        ( MulticastTargets.AnyMember | MulticastTargets.Parameter |
                                          MulticastTargets.ReturnValue ) ) == 0 )
                            {
                                // Cannot apply the custom attribute {{{0}}} on the {1} "{2}": cannot apply this custom attribute on a {1}.

                                CoreMessageSource.Instance.Write( SeverityType.Error,
                                                                  "PS0071",
                                                                  new object[]
                                                                      {
                                                                          ( (INamedType) attributeType ).Name,
                                                                          candidateTarget.ToString().ToLowerInvariant(),
                                                                          typeDef.ToString(),
                                                                          instance.TargetElements
                                                                      } );
                                continue;
                            }
                        }

                        if ( isAbstract )
                        {
                            abstractInstancesOnThisType.Add( instance );
                        }
                        else if ( ( instance.TargetElements & MulticastTargets.AnyType ) != 0 )
                        {
                            concreteInstancesOnThisType.Add( instance );
                        }


                        // Detect instances on children.
                        if ( !instance.Inherited || instance.Inheritance == MulticastInheritance.Multicast )
                        {
                            if ( ( instance.TargetElements & MulticastTargets.Field ) != 0 )
                            {
                                if ( instancesOnFields == null )
                                {
                                    instancesOnFields = new List<MulticastAttributeInstanceInfo>();
                                }
                                instancesOnFields.Add( instance );
                            }

                            if ( ( instance.TargetElements &
                                   ( MulticastTargets.InstanceConstructor | MulticastTargets.StaticConstructor |
                                     MulticastTargets.Method |
                                     MulticastTargets.Parameter | MulticastTargets.ReturnValue ) ) !=
                                 0 )
                            {
                                if ( instancesOnMethods == null )
                                {
                                    instancesOnMethods = new List<MulticastAttributeInstanceInfo>();
                                }
                                instancesOnMethods.Add( instance );
                            }

                            if ( ( instance.TargetElements & MulticastTargets.Event ) != 0 )
                            {
                                if ( instancesOnEvents == null )
                                {
                                    instancesOnEvents = new List<MulticastAttributeInstanceInfo>();
                                }
                                instancesOnEvents.Add( instance );
                            }

                            if ( ( instance.TargetElements & MulticastTargets.Property ) != 0 )
                            {
                                if ( instancesOnProperties == null )
                                {
                                    instancesOnProperties = new List<MulticastAttributeInstanceInfo>();
                                }
                                instancesOnProperties.Add( instance );
                            }
                        }
                    }

                    #endregion

                    AddMergedCustomAttributes( typeDef, abstractInstancesOnThisType, true );
                    AddMergedCustomAttributes( typeDef, concreteInstancesOnThisType, false );
                }

                #region Broadcast to children

                // We never broadcast to members of enumerations or delegates.
                if ( typeDef.BelongsToClassification( TypeClassifications.Enum ) ||
                     typeDef.BelongsToClassification( TypeClassifications.Delegate ) )
                    continue;


                // If the type is generic and is defined outside the current assembly, we will have
                // to look at all TypeSpecs instead of the unique TypeRef.
                ICollection<MetadataDeclaration> typeSpecs = null;
                if ( type is TypeRefDeclaration )
                {
                    typeSpecs = IndexGenericInstancesTask.GetGenericInstances( type.GetTypeDefinition() );
                }

                if ( typeSpecs == null || typeSpecs.Count == 0 )
                {
                    typeSpecs = new Singleton<MetadataDeclaration>( (MetadataDeclaration) type );
                }

                foreach ( IType typeSpec in typeSpecs )
                {
                    if ( instancesOnMethods != null )
                    {
                        foreach ( IMethod method in typeSpec.Methods )
                        {
                            MethodDefDeclaration methodDef = method.GetMethodDefinition();

                            foreach ( MulticastAttributeInstanceInfo instance in instancesOnMethods )
                            {
                                if ( CompareMethodAttributes( instance.TargetMemberAttributes, methodDef.Attributes ) &&
                                     ( instance.TargetMembersRegex == null ||
                                       instance.TargetMembersRegex.IsMatch( method.Name ) )
                                     &&
                                     ( ( instance.TargetElements & GetMethodTarget( method ) ) != 0 ) )
                                {
                                    methodDef = method.GetMethodDefinition();

                                    if ( instance.AppliedOn.AddIfAbsent( methodDef ) )
                                    {
                                        this.PropagateIntermediateInstance( methodDef, instance );
                                    }
                                }
                            }
                        }
                    }

                    if ( instancesOnFields != null )
                    {
                        foreach ( IField field in typeSpec.Fields )
                        {
                            FieldDefDeclaration fieldDef = field.GetFieldDefinition();

                            foreach ( MulticastAttributeInstanceInfo instance in instancesOnFields )
                            {
                                if ( CompareFieldAttributes( instance.TargetMemberAttributes, fieldDef.Attributes ) &&
                                     ( instance.TargetMembersRegex == null ||
                                       instance.TargetMembersRegex.IsMatch( field.Name ) ) )
                                {
                                    if ( instance.AppliedOn.AddIfAbsent( fieldDef ) )
                                    {
                                        this.AddIntermediateInstance( fieldDef, instance );
                                    }
                                }
                            }
                        }
                    }
                }

                if ( instancesOnProperties != null )
                {
                    foreach ( PropertyDeclaration property in typeDef.Properties )
                    {
                        foreach ( MulticastAttributeInstanceInfo instance in instancesOnProperties )
                        {
                            if ( instance.TargetMembersRegex == null ||
                                 instance.TargetMembersRegex.IsMatch( property.Name ) )
                            {
                                this.AddIntermediateInstance( property, instance );
                            }
                        }
                    }
                }


                if ( instancesOnEvents != null )
                {
                    foreach ( EventDeclaration @event in typeDef.Events )
                    {
                        foreach ( MulticastAttributeInstanceInfo instance in instancesOnEvents )
                        {
                            if ( instance.TargetMembersRegex == null ||
                                 instance.TargetMembersRegex.IsMatch( @event.Name ) )
                            {
                                this.AddIntermediateInstance( @event, instance );
                            }
                        }
                    }
                }

                #endregion
            }

            instances.Remove( tokenType );
        }

        private static MulticastTargets GetTypeTarget( TypeDefDeclaration typeDef )
        {
            MulticastTargets candidateTarget;

            if ( typeDef.BelongsToClassification( TypeClassifications.Interface ) )
            {
                candidateTarget = MulticastTargets.Interface;
            }
            else if ( typeDef.BelongsToClassification( TypeClassifications.Enum ) )
            {
                candidateTarget = MulticastTargets.Enum;
            }
            else if ( typeDef.BelongsToClassification( TypeClassifications.Struct ) )
            {
                candidateTarget = MulticastTargets.Struct;
            }
            else if ( typeDef.BelongsToClassification( TypeClassifications.Delegate ) )
            {
                candidateTarget = MulticastTargets.Delegate;
            }
            else
            {
                candidateTarget = MulticastTargets.Class;
            }
            return candidateTarget;
        }

        private void ProcessMethodSemanticCollections( TokenType tokenType, MulticastTargets target )
        {
            Dictionary<IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>
                instancesOnTokenType;
            if ( !this.instances.TryGetValue( tokenType, out instancesOnTokenType ) )
            {
                return;
            }

            Dictionary<ITypeSignature, List<MulticastAttributeInstanceInfo>> instancesApplyingOnThisDeclaration =
                new Dictionary<ITypeSignature, List<MulticastAttributeInstanceInfo>>( 16 );


            IEnumerator
                <KeyValuePair<IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>>
                declarationEnumerator = instancesOnTokenType.GetEnumerator();

            while ( declarationEnumerator.MoveNext() )
            {
                MethodGroupDeclaration declaration = (MethodGroupDeclaration) declarationEnumerator.Current.Key;

                instancesApplyingOnThisDeclaration.Clear();

                List<MulticastAttributeInstanceInfo> instancesOnMethods = null;

                foreach ( ITypeSignature attributeType in declarationEnumerator.Current.Value.Keys )
                {
                    List<MulticastAttributeInstanceInfo> instancesOfThisType;

                    instancesApplyingOnThisDeclaration.TryGetValue( attributeType, out instancesOfThisType );

                    #region Detect instances

                    foreach (
                        MulticastAttributeInstanceInfo instance in declarationEnumerator.Current.Value[attributeType]
                        )
                    {
                        // Detect instances applying on this type.
                        if ( ( instance.TargetElements & target ) != 0 )
                        {
                            if ( instancesOfThisType == null )
                            {
                                instancesOfThisType = new List<MulticastAttributeInstanceInfo>();
                                instancesApplyingOnThisDeclaration.Add( attributeType, instancesOfThisType );
                            }

                            instancesOfThisType.Add( instance );
                        }

                        // Detect instances on children (methods).
                        if ( ( instance.TargetElements & MulticastTargets.Method ) != 0 )
                        {
                            if ( instancesOnMethods == null )
                            {
                                instancesOnMethods = new List<MulticastAttributeInstanceInfo>();
                            }
                            instancesOnMethods.Add( instance );
                        }
                    }

                    #endregion

                    AddMergedCustomAttributes( declaration, instancesOfThisType, false );
                }

                #region Broadcast to children

                if ( instancesOnMethods != null )
                {
                    foreach ( MethodSemanticDeclaration semantic in declaration.Members )
                    {
                        MethodDefDeclaration methodDef = semantic.Method;

                        foreach ( MulticastAttributeInstanceInfo instance in instancesOnMethods )
                        {
                            if ( CompareMethodAttributes( instance.TargetMemberAttributes, methodDef.Attributes ) )
                            {
                                if ( instance.TargetMembersRegex == null ||
                                     instance.TargetMembersRegex.IsMatch( methodDef.Name ) )
                                {
                                    this.PropagateIntermediateInstance( methodDef, instance );
                                }
                            }
                        }
                    }
                }

                #endregion
            }

            instances.Remove( tokenType );
        }

        private void ProcessField( IField field,
                                   MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo> instances )
        {
            foreach ( INamedType attributeType in instances.Keys )
            {
                List<MulticastAttributeInstanceInfo> instancesOfThisType =
                    new List<MulticastAttributeInstanceInfo>();

                #region Detect instances

                foreach ( MulticastAttributeInstanceInfo instance in instances[attributeType] )
                {
                    // Detect instances applying on this type.
                    if ( ( instance.TargetElements & MulticastTargets.Field ) != 0 )
                    {
                        if ( instance.DeclaredOn == field )
                        {
                            if ( !CompareFieldAttributes( instance.TargetMemberAttributes, field.Attributes ) )
                            {
                                // Cannot apply the custom attribute "{0}" on {1} "{2}" because of incompatible attributes (visibility, scope, virtuality, ...).
                                CoreMessageSource.Instance.Write( SeverityType.Error,
                                                                  "PS0091",
                                                                  new object[]
                                                                      {
                                                                          attributeType.ToString(),
                                                                          "field",
                                                                          field.ToString()
                                                                      } );
                                continue;
                            }
                        }

                        instancesOfThisType.Add( instance );
                    }
                    else
                    {
                        // Cannot apply the custom attribute {{{0}}} on the {1} "{2}": cannot apply this custom attribute on a {1}.
                        CoreMessageSource.Instance.Write( SeverityType.Error,
                                                          "PS0071", new object[]
                                                                        {
                                                                            attributeType.Name,
                                                                            "field",
                                                                            instance.DeclaredOn.ToString(),
                                                                            instance.TargetElements
                                                                        } );
                    }
                }

                #endregion

                AddMergedCustomAttributes( (MetadataDeclaration) field, instancesOfThisType, false );
            }
        }

        private void ProcessParameter( MulticastTargets target,
                                       ParameterDeclaration parameter,
                                       MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo> instances )
        {
            foreach ( INamedType attributeType in instances.Keys )
            {
                List<MulticastAttributeInstanceInfo> concreteInstancesOfThisType =
                    new List<MulticastAttributeInstanceInfo>();
                List<MulticastAttributeInstanceInfo> abstractInstancesOfThisType =
                    new List<MulticastAttributeInstanceInfo>();

                #region Detect instances

                foreach ( MulticastAttributeInstanceInfo instance in instances[attributeType] )
                {
                    bool isAbstract = false;
                    // Verify filters.
                    if ( ( instance.TargetElements & target ) != 0 )
                    {
                        if ( !CompareParameterAttributes( instance.TargetMemberAttributes, parameter ) )
                        {
                            if ( instance.DeclaredOn == parameter )
                            {
                                CoreMessageSource.Instance.Write( SeverityType.Error,
                                                                  "PS0106",
                                                                  new object[]
                                                                      {
                                                                          attributeType.ToString(),
                                                                          parameter.ToString(),
                                                                          parameter.DeclaringMethod.ToString()
                                                                      } );

                                continue;
                            }
                            else
                            {
                                throw new AssertionFailedException(
                                    string.Format(
                                        "Cannot apply the attribute {0} on {1} '{2}' of method {3}: invalid target element kind.",
                                        instance, target.ToString().ToLowerInvariant(), parameter,
                                        parameter.DeclaringMethod ) );
                            }
                        }
                    }
                    else
                    {
                        // Cannot apply the custom attribute "{0}" on parameter "{1}" of method "{2}": 
                        // cannot apply this attribute on a {3}. Valid targets are: {4}.
                        CoreMessageSource.Instance.Write( SeverityType.Error,
                                                          "PS0071", new object[]
                                                                        {
                                                                            attributeType.Name,
                                                                            parameter.ToString(),
                                                                            parameter.DeclaringMethod.ToString(),
                                                                            target.ToString(),
                                                                            instance.TargetElements
                                                                        } );
                        continue;
                    }

                    // If the attribute was applied directly to the parameter (or eventually inherited from
                    // an attribute on another parameter), we should also check the attributes of the
                    // declaring method.
                    if ( instance.DeclaredOn == parameter || instance.Inheritance == MulticastInheritance.Strict )
                    {
                        if (
                            !CompareMethodAttributes( instance.TargetMemberAttributes,
                                                      parameter.DeclaringMethod.Attributes ) )
                        {
                            if ( instance.Inheritance != MulticastInheritance.None )
                            {
                                isAbstract = true;
                            }
                            else
                            {
                                CoreMessageSource.Instance.Write( SeverityType.Error,
                                                                  "PS0105",
                                                                  new object[]
                                                                      {
                                                                          attributeType.ToString(),
                                                                          parameter.ToString(),
                                                                          parameter.DeclaringMethod.ToString()
                                                                      } );
                                continue;
                            }
                        }
                    }

                    if ( isAbstract )
                    {
                        abstractInstancesOfThisType.Add( instance );
                    }
                    else
                    {
                        concreteInstancesOfThisType.Add( instance );
                    }
                }

                #endregion

                AddMergedCustomAttributes( parameter, concreteInstancesOfThisType, false );
                AddMergedCustomAttributes( parameter, abstractInstancesOfThisType, true );
            }
        }

        private void ProcessMethod( IMethod method,
                                    MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo> instances
            )
        {
            MethodDefDeclaration methodDef = method.GetMethodDefinition();

            MulticastTargets target = GetMethodTarget( method );
            foreach ( INamedType attributeType in instances.Keys )
            {
                List<MulticastAttributeInstanceInfo> concreteInstancesOnThisType =
                    new List<MulticastAttributeInstanceInfo>();
                List<MulticastAttributeInstanceInfo> abstractInstancesOnThisType =
                    new List<MulticastAttributeInstanceInfo>();

                List<MulticastAttributeInstanceInfo> instancesOnParameters = null;

                #region Detect instances

                foreach ( MulticastAttributeInstanceInfo instance in instances[attributeType] )
                {
                    bool isAbstract = false;

                    // Detect instances applying on this type.
                    if ( ( instance.TargetElements & target ) == 0 )
                    {
                        if ( instance.Inheritance != MulticastInheritance.None )
                        {
                            isAbstract = true;
                        }
                        else if ( instance.DeclaredOn == method &&
                                  ( instance.TargetElements &
                                    ( MulticastTargets.Parameter | MulticastTargets.ReturnValue ) ) ==
                                  0 )
                        {
                            // Cannot apply the custom attribute {{{0}}} on the {1} "{2}": cannot apply this custom attribute on a {1}.
                            CoreMessageSource.Instance.Write( SeverityType.Error,
                                                              "PS0071", new object[]
                                                                            {
                                                                                attributeType.Name,
                                                                                target.ToString().ToLowerInvariant(),
                                                                                instance.DeclaredOn.ToString(),
                                                                                instance.TargetElements
                                                                            } );
                            continue;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                string.Format( "Cannot apply {0} on {1}: invalid target element kind.", instance, method ) );
                        }
                    }


                    // If the instance was declared on that method, we should further check attributes.
                    if ( !CompareMethodAttributes( instance.TargetMemberAttributes, method.Attributes ) )
                    {
                        if ( instance.Inheritance != MulticastInheritance.None )
                        {
                            isAbstract = true;
                        }
                        else if ( instance.DeclaredOn == method )
                        {
                            // Cannot apply the custom attribute "{0}" on {1} "{2}" because of incompatible attributes (visibility, scope, virtuality, ...).
                            CoreMessageSource.Instance.Write( SeverityType.Error,
                                                              "PS0091",
                                                              new object[]
                                                                  {
                                                                      attributeType.ToString(),
                                                                      target.ToString().ToLowerInvariant(),
                                                                      method.ToString()
                                                                  } );
                            continue;
                        }
                        else
                        {
                            throw new AssertionFailedException(
                                string.Format( "Cannot apply {0} on {1}: invalid attributes.",
                                               instance, method ) );
                        }
                    }

                    // If we are here, the instance was legal.

                    // Add the instance to the method if adequate.
                    if ( ( instance.TargetElements & target ) != 0 )
                    {
                        if ( isAbstract )
                        {
                            abstractInstancesOnThisType.Add( instance );
                        }
                        else
                        {
                            concreteInstancesOnThisType.Add( instance );
                        }
                    }

                    // Add the instance to parameters if adequate.
                    if ( ( !instance.Inherited || instance.Inheritance == MulticastInheritance.Multicast ) &&
                         ( instance.TargetElements & ( MulticastTargets.Parameter | MulticastTargets.ReturnValue ) ) !=
                         0 )
                    {
                        if ( methodDef.Module != module )
                        {
                            // Cannot apply a MulticastAttribute ({0}) to the parameters or the return value of a method ({1}) located outside the current assembly.
                            CoreMessageSource.Instance.Write( SeverityType.Error,
                                                              "PS0104",
                                                              new object[]
                                                                  {
                                                                      attributeType.ToString(),
                                                                      method.ToString()
                                                                  } );
                            continue;
                        }

                        if ( ( instance.TargetElements & MulticastTargets.Parameter ) != 0 )
                        {
                            if ( instancesOnParameters == null )
                                instancesOnParameters = new List<MulticastAttributeInstanceInfo>();

                            instancesOnParameters.Add( instance );
                        }

                        if ( ( instance.TargetElements & MulticastTargets.ReturnValue ) != 0 )
                        {
                            this.PropagateIntermediateInstance( methodDef.ReturnParameter, instance );
                        }
                    }
                }

                #endregion

                AddMergedCustomAttributes( (MetadataDeclaration) method, abstractInstancesOnThisType, true );
                AddMergedCustomAttributes( (MetadataDeclaration) method, concreteInstancesOnThisType, false );

                // Process attributes on parameters and return values.
                if ( instancesOnParameters != null )
                {
                    foreach ( ParameterDeclaration parameter in methodDef.Parameters )
                    {
                        foreach ( MulticastAttributeInstanceInfo instance in instancesOnParameters )
                        {
                            if ( CompareParameterAttributes( instance.TargetParameterAttributes, parameter ) &&
                                 ( instance.TargetParametersRegex == null ||
                                   instance.TargetParametersRegex.IsMatch( parameter.Name ) ) )
                            {
                                if ( instance.AppliedOn.AddIfAbsent( parameter ) )
                                {
                                    this.PropagateIntermediateInstance( parameter, instance );
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool CompareParameterAttributes( MulticastAttributes attributes, ParameterDeclaration parameter )
        {
            if ( ( attributes & MulticastAttributes.AnyParameter ) == MulticastAttributes.AnyParameter )
                return true;

            PointerTypeSignature pointerTypeSignature =
                parameter.ParameterType.GetNakedType( TypeNakingOptions.IgnoreAllCustomModifiers ) as
                PointerTypeSignature;
            bool isManagedPointer = pointerTypeSignature != null ? pointerTypeSignature.IsManaged : false;

            if ( ( attributes & MulticastAttributes.InParameter ) != 0 &&
                 !isManagedPointer )
                return true;

            if ( ( attributes & MulticastAttributes.OutParameter ) != 0 &&
                 isManagedPointer && ( parameter.Attributes & ParameterAttributes.In ) == 0 )
                return true;

            if ( ( attributes & MulticastAttributes.RefParameter ) != 0 &&
                 isManagedPointer &&
                 ( parameter.Attributes & ParameterAttributes.In ) != 0 )
                return true;

            return false;
        }

        private void ProcessFields( TokenType tokenType )
        {
            Dictionary<IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>
                instancesOnFields;
            if ( !this.instances.TryGetValue( tokenType, out instancesOnFields ) )
            {
                return;
            }

            IEnumerator
                <KeyValuePair<IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>>
                declarationEnumerator = instancesOnFields.GetEnumerator();

            while ( declarationEnumerator.MoveNext() )
            {
                IField field = declarationEnumerator.Current.Key as IField;
                if ( field != null )
                {
                    this.ProcessField( field, declarationEnumerator.Current.Value );
                }
            }
        }

        private void ProcessMethods( TokenType tokenType )
        {
            Dictionary<IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>
                instancesOnMethods;
            if ( !this.instances.TryGetValue( tokenType, out instancesOnMethods ) )
            {
                return;
            }

            IEnumerator
                <KeyValuePair<IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>>
                declarationEnumerator = instancesOnMethods.GetEnumerator();

            while ( declarationEnumerator.MoveNext() )
            {
                IMethod method = declarationEnumerator.Current.Key as IMethod;
                if ( method != null )
                {
                    this.ProcessMethod( method, declarationEnumerator.Current.Value );
                }
            }
        }

        private void ProcessParameters()
        {
            Dictionary<IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>
                instancesOnParameters;
            if ( !this.instances.TryGetValue( TokenType.ParamDef, out instancesOnParameters ) )
            {
                return;
            }

            IEnumerator
                <KeyValuePair<IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>>
                declarationEnumerator = instancesOnParameters.GetEnumerator();

            while ( declarationEnumerator.MoveNext() )
            {
                ParameterDeclaration parameter = declarationEnumerator.Current.Key as ParameterDeclaration;
                if ( parameter != null )
                {
                    MulticastTargets target = ( parameter.Attributes & ParameterAttributes.Retval ) != 0
                                                  ?
                                                      MulticastTargets.ReturnValue
                                                  : MulticastTargets.Parameter;
                    this.ProcessParameter( target, parameter, declarationEnumerator.Current.Value );
                }
            }
        }


        private static MulticastTargets GetMethodTarget( IMethod method )
        {
            MulticastTargets target;
            switch ( method.Name )
            {
                case ".ctor":
                    target = MulticastTargets.InstanceConstructor;
                    break;

                case ".cctor":
                    target = MulticastTargets.StaticConstructor;
                    break;

                default:
                    target = MulticastTargets.Method;
                    break;
            }
            return target;
        }

        #endregion

        #region Compare attributes

        private static bool CompareMethodAttributes( MulticastAttributes acceptableAttributes,
                                                     MethodAttributes candidateAttributes )
        {
            if ( acceptableAttributes == MulticastAttributes.All )
            {
                return true;
            }

            MethodAttributes candidateVisibility = candidateAttributes & MethodAttributes.MemberAccessMask;

            // Visibility
            if ( ( ( acceptableAttributes & MulticastAttributes.Internal ) != 0 &&
                   ( candidateVisibility == MethodAttributes.Assembly ) ) ||
                 ( ( acceptableAttributes & MulticastAttributes.Public ) != 0 &&
                   candidateVisibility == MethodAttributes.Public ) ||
                 ( ( acceptableAttributes & MulticastAttributes.Protected ) != 0 &&
                   candidateVisibility == MethodAttributes.Family ) ||
                 ( ( acceptableAttributes & MulticastAttributes.InternalAndProtected ) != 0 &&
                   candidateVisibility == MethodAttributes.FamANDAssem ) ||
                 ( ( acceptableAttributes & MulticastAttributes.InternalOrProtected ) != 0 &&
                   candidateVisibility == MethodAttributes.FamORAssem ) ||
                 ( ( acceptableAttributes & MulticastAttributes.Private ) != 0 &&
                   candidateVisibility == MethodAttributes.Private ) )
            {
                // Scope
                if ( ( ( acceptableAttributes & MulticastAttributes.Static ) != 0 &&
                       ( candidateAttributes & MethodAttributes.Static ) != 0 ) ||
                     ( ( acceptableAttributes & MulticastAttributes.Instance ) != 0 &&
                       ( candidateAttributes & MethodAttributes.Static ) == 0 ) )
                {
                    // Virtuality
                    if ( ( ( acceptableAttributes & MulticastAttributes.Abstract ) != 0 &&
                           ( candidateAttributes & MethodAttributes.Abstract ) != 0 ) ||
                         ( ( acceptableAttributes & MulticastAttributes.NonAbstract ) != 0 &&
                           ( candidateAttributes & MethodAttributes.Abstract ) == 0 ) )
                    {
                        if ( ( ( acceptableAttributes & MulticastAttributes.Virtual ) != 0 &&
                               ( candidateAttributes & MethodAttributes.Virtual ) != 0 ) ||
                             ( ( acceptableAttributes & MulticastAttributes.NonVirtual ) != 0 &&
                               ( ( candidateAttributes & MethodAttributes.Virtual ) == 0 ) ) )
                        {
                            // Implementation
                            bool isManaged = ( candidateAttributes & ( MethodAttributes.UnmanagedExport |
                                                                       MethodAttributes.PinvokeImpl ) ) == 0;

                            if ( ( ( acceptableAttributes & MulticastAttributes.Managed ) != 0 &&
                                   isManaged ) ||
                                 ( ( acceptableAttributes & MulticastAttributes.NonManaged ) != 0 &&
                                   !isManaged ) )
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static bool CompareFieldAttributes( MulticastAttributes acceptableAttributes,
                                                    FieldAttributes candidateAttributes )
        {
            if ( acceptableAttributes == MulticastAttributes.All )
            {
                return true;
            }

            FieldAttributes candidateVisibility = candidateAttributes & FieldAttributes.FieldAccessMask;

            // Visibility
            if ( ( ( acceptableAttributes & MulticastAttributes.Internal ) != 0 &&
                   ( candidateVisibility == FieldAttributes.Assembly ) ) ||
                 ( ( acceptableAttributes & MulticastAttributes.Public ) != 0 &&
                   candidateVisibility == FieldAttributes.Public ) ||
                 ( ( acceptableAttributes & MulticastAttributes.Protected ) != 0 &&
                   candidateVisibility == FieldAttributes.Family ) ||
                 ( ( acceptableAttributes & MulticastAttributes.InternalAndProtected ) != 0 &&
                   candidateVisibility == FieldAttributes.FamANDAssem ) ||
                 ( ( acceptableAttributes & MulticastAttributes.InternalOrProtected ) != 0 &&
                   candidateVisibility == FieldAttributes.FamORAssem ) ||
                 ( ( acceptableAttributes & MulticastAttributes.Private ) != 0 &&
                   candidateVisibility == FieldAttributes.Private ) )
            {
                // Scope
                if ( ( ( acceptableAttributes & MulticastAttributes.Static ) != 0 &&
                       ( candidateAttributes & FieldAttributes.Static ) != 0 ) ||
                     ( ( acceptableAttributes & MulticastAttributes.Instance ) != 0 &&
                       ( candidateAttributes & FieldAttributes.Static ) == 0 ) )
                {
                    // Literality.
                    bool candidateLiteral = ( candidateAttributes & FieldAttributes.Literal ) != 0;
                    if ( ( ( acceptableAttributes & MulticastAttributes.Literal ) != 0 ) && candidateLiteral ||
                         ( ( acceptableAttributes & MulticastAttributes.NonLiteral ) != 0 ) && !candidateLiteral )
                    {
                        return true;
                    }
                }
            }

            return false;
        }


         private bool IsCompilerGenerated(TypeDefDeclaration type)
        {
            if (type.CustomAttributes.Contains(this.compilerGeneratedAttributeType))
                return true;

           return CompilerWorkarounds.IsCompilerGenerated( type );
        }

        private bool CompareTypeAttributes( MulticastAttributes acceptableAttributes,
                                                   TypeDefDeclaration candidateType)
        {
            TypeAttributes candidateAttributes = candidateType.Attributes;

            if ( acceptableAttributes == MulticastAttributes.All )
            {
                return true;
            }

            TypeAttributes candidateVisibility = candidateAttributes & TypeAttributes.VisibilityMask;

            if ( ( ( acceptableAttributes & MulticastAttributes.Internal ) != 0 &&
                   ( candidateVisibility == TypeAttributes.NestedAssembly ||
                     candidateVisibility == TypeAttributes.NotPublic ) ) ||
                 ( ( acceptableAttributes & MulticastAttributes.Public ) != 0 &&
                   ( candidateVisibility == TypeAttributes.NestedPublic ||
                     candidateVisibility == TypeAttributes.Public ) ) ||
                 ( ( acceptableAttributes & MulticastAttributes.InternalOrProtected ) != 0 &&
                   candidateVisibility == TypeAttributes.NestedFamORAssem ) ||
                 ( ( acceptableAttributes & MulticastAttributes.InternalAndProtected ) != 0 &&
                   candidateVisibility == TypeAttributes.NestedFamANDAssem ) ||
                 ( ( acceptableAttributes & MulticastAttributes.Protected ) != 0 &&
                   candidateVisibility == TypeAttributes.NestedFamily ) ||
                 ( ( acceptableAttributes & MulticastAttributes.Private ) != 0 &&
                   candidateVisibility == TypeAttributes.NestedPrivate ) )
            {
                if ( ( acceptableAttributes & MulticastAttributes.AnyGeneration ) != MulticastAttributes.AnyGeneration )
                {
                    bool isCompilerGenerated = this.IsCompilerGenerated( candidateType );

                    if ( ( ( acceptableAttributes & MulticastAttributes.UserGenerated ) != 0 && !isCompilerGenerated ) ||
                         ( ( acceptableAttributes & MulticastAttributes.CompilerGenerated ) != 0 && isCompilerGenerated ) )
                    {
                        return true;
                    }
                }
                else return true;
            }

            return false;
        }

        #endregion

        #region Misc. utility methods

        private static MulticastAttributes ApplyMulticastAttribute( MulticastAttributes parent,
                                                                    MulticastAttributes child )
        {
            MulticastAttributes result = child;

            if ( ( child & MulticastAttributes.AnyImplementation ) == 0 )
            {
                result |=
                    parent & MulticastAttributes.AnyImplementation;
            }

            if ( ( child & MulticastAttributes.AnyScope ) == 0 )
            {
                result |=
                    parent & MulticastAttributes.AnyScope;
            }

            if ( ( child & MulticastAttributes.AnyVirtuality ) == 0 )
            {
                result |=
                    parent & MulticastAttributes.AnyVirtuality;
            }

            if ( ( child & MulticastAttributes.AnyAbstraction ) == 0 )
            {
                result |=
                    parent & MulticastAttributes.AnyAbstraction;
            }

            if ( ( child & MulticastAttributes.AnyVisibility ) == 0 )
            {
                result |=
                    parent & MulticastAttributes.AnyVisibility;
            }

            if ( ( child & MulticastAttributes.AnyLiterality ) == 0 )
            {
                result |=
                    parent & MulticastAttributes.AnyLiterality;
            }

            if ( ( child & MulticastAttributes.AnyGeneration ) == 0 )
            {
                result |=
                    parent & MulticastAttributes.AnyGeneration;
            }

            if ( ( child & MulticastAttributes.AnyParameter ) == 0 )
            {
                result |=
                    parent & MulticastAttributes.AnyParameter;
            }

            return result;
        }


        private void PropagateIntermediateInstance( TypeDefDeclaration targetTypeDef,
                                                    MulticastAttributeInstanceInfo instanceInfo )
        {
            this.AddIntermediateInstance( targetTypeDef, instanceInfo );

            if ( instanceInfo.Inheritance != MulticastInheritance.None )
            {
                this.HasInheritedAttribute();

                IEnumerator<TypeDefDeclaration> childrenEnumerator =
                    TypeHierarchyTask.GetDerivedTypesEnumerator( targetTypeDef,
                                                                 true, null );
                MulticastAttributeInstanceInfo inheritedInstanceInfo = instanceInfo.Clone();
                inheritedInstanceInfo.Inherited = true;

                while ( childrenEnumerator.MoveNext() )
                {
                    this.AddIntermediateInstance( childrenEnumerator.Current, inheritedInstanceInfo );
                }
            }
        }

        private void HasInheritedAttribute()
        {
            if ( this.hasInheritedAttribute )
                return;

            TypeHierarchyTask.GetTask( this.Project ).IndexAllTypeDefinitions();
            this.hasInheritedAttribute = true;
        }


        private void PropagateIntermediateInstance( MethodDefDeclaration targetMethodDef,
                                                    MulticastAttributeInstanceInfo instanceInfo )
        {
            this.AddIntermediateInstance( targetMethodDef, instanceInfo );

            if ( instanceInfo.Inheritance != MulticastInheritance.None )
            {
                this.methodsWithInheritedAttributes.AddIfAbsent( targetMethodDef );
            }
        }

        private void PropagateIntermediateInstance( ParameterDeclaration targetParameter,
                                                    MulticastAttributeInstanceInfo instanceInfo )
        {
            this.AddIntermediateInstance( targetParameter, instanceInfo );

            if ( instanceInfo.Inheritance != MulticastInheritance.None )
            {
                this.methodsWithInheritedAttributes.AddIfAbsent( targetParameter.DeclaringMethod );
            }
        }

        private MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo> GetIntermediateInstances(
            IMetadataDeclaration targetElement, bool create )
        {
            Dictionary<IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>
                instancesOnTokenType;
            MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo> instancesOnElement;

            TokenType tokenType = targetElement.GetTokenType();
            if ( !this.instances.TryGetValue( tokenType, out instancesOnTokenType ) )
            {
                if ( create )
                {
                    instancesOnTokenType =
                        new Dictionary
                            <IMetadataDeclaration, MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>>();
                    this.instances.Add( tokenType, instancesOnTokenType );
                }
                else return null;
            }

            if ( !instancesOnTokenType.TryGetValue( targetElement, out instancesOnElement ) )
            {
                if ( create )
                {
                    instancesOnElement = new MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo>();
                    instancesOnTokenType.Add( targetElement, instancesOnElement );
                }
                else return null;
            }

            return instancesOnElement;
        }

        private void AddIntermediateInstance( IMetadataDeclaration targetElement,
                                              MulticastAttributeInstanceInfo instanceInfo )
        {
            if ( Trace.MulticastAttributeTask.Enabled )
            {
                Trace.MulticastAttributeTask.WriteLine(
                    "Add intermediate instance {{{2}}} on {0} {{{1}}}.",
                    targetElement.MetadataToken.TokenType.ToString(),
                    targetElement.ToString(),
                    CustomAttributeHelper.Render( instanceInfo.Annotation ) );
            }


            MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo> instancesOnElement =
                GetIntermediateInstances( targetElement, true );

            instancesOnElement.Add( instanceInfo.Annotation.Constructor.DeclaringType, instanceInfo );
        }

        private void AddIntermediateInstance( MulticastAttributeInstanceInfo instanceInfo )
        {
            this.AddIntermediateInstance( instanceInfo.DeclaredOn, instanceInfo );
        }

        private void AddMergedCustomAttributes( MetadataDeclaration targetElement,
                                                List<MulticastAttributeInstanceInfo> instances,
                                                bool isAbstract )
        {
            if ( instances == null || instances.Count == 0 )
            {
                return;
            }

            if ( instances.Count == 1 )
            {
                this.AddFinalInstance( targetElement, instances[0], isAbstract );
            }
            else
            {
                instances.Sort();

                List<MulticastAttributeInstanceInfo> choosenInstances =
                    new List<MulticastAttributeInstanceInfo>( instances.Count );

                foreach ( MulticastAttributeInstanceInfo instance in instances )
                {
                    if ( instance.Exclude )
                    {
                        choosenInstances.Clear();
                        continue;
                    }

                    if ( instance.Replace || !instance.Usage.AllowMultiple )
                    {
                        choosenInstances.Clear();
                    }


                    if ( !instance.Usage.AllowMultiple && choosenInstances.Count > 0 )
                    {
                        CoreMessageSource.Instance.Write( SeverityType.Error,
                                                          "PS0065", new object[]
                                                                        {
                                                                            ( (INamedType)
                                                                              instance.Annotation.Constructor.
                                                                                  DeclaringType ).Name,
                                                                            targetElement.ToString()
                                                                        } );
                    }
                    else
                    {
                        // We add it unless the same instance has already been added.
                        bool alreadyAdded = false;
                        foreach ( MulticastAttributeInstanceInfo previousInstance in choosenInstances )
                        {
                            if ( previousInstance.Id == instance.Id )
                            {
                                alreadyAdded = true;
                                break;
                            }
                        }
                        if ( !alreadyAdded ) choosenInstances.Add( instance );
                    }
                }

                foreach ( MulticastAttributeInstanceInfo choosenInstance in choosenInstances )
                {
                    this.AddFinalInstance( targetElement, choosenInstance, isAbstract );
                }
            }
        }

        private void AddFinalInstance( MetadataDeclaration targetElement, MulticastAttributeInstanceInfo instanceInfo,
                                       bool isAbstract )
        {
            if ( instanceInfo.Exclude )
            {
                return;
            }

            if ( !isAbstract )
            {
                SharedAnnotationInstance instance = new SharedAnnotationInstance( instanceInfo.Annotation, targetElement );
                this.annotationRepository.AddAnnotation( instance );
            }


            if ( Trace.MulticastAttributeTask.Enabled )
            {
                Trace.MulticastAttributeTask.WriteLine(
                    "Add final instance of {{{2}}} on {0} {{{1}}}, persist = {3}, id = {4}, abstract = {5}",
                    targetElement.MetadataToken.TokenType,
                    targetElement,
                    CustomAttributeHelper.Render( instanceInfo.Annotation ),
                    instanceInfo.Usage.PersistMetaData,
                    instanceInfo.Id,
                    isAbstract );
            }


            MulticastInheritance externalInheritance;
            TokenType targetTokenType = targetElement.GetTokenType();

            if ( instanceInfo.Inheritance != MulticastInheritance.None )
            {
                // Determine if the target element is visible from outside.
                TypeDefDeclaration typeDef = null;
                MethodDefDeclaration methodDef = null;

                switch ( targetTokenType )
                {
                    case TokenType.Assembly:
                        externalInheritance = MulticastInheritance.Multicast;
                        break;

                    case TokenType.TypeDef:
                        typeDef = (TypeDefDeclaration) targetElement;
                        externalInheritance = VisibilityHelper.IsPublic( typeDef ) &&
                                                !typeDef.IsSealed
                                                  ?
                                                      MulticastInheritance.Multicast
                                                  : MulticastInheritance.None;
                        break;

                    case TokenType.MethodDef:
                        methodDef = (MethodDefDeclaration) targetElement;
                        typeDef = methodDef.DeclaringType;
                        externalInheritance = VisibilityHelper.IsPublic( methodDef ) &&
                                              methodDef.IsVirtual && !methodDef.IsSealed && !typeDef.IsSealed
                                                  ?
                                                      MulticastInheritance.Strict
                                                  : MulticastInheritance.None;
                        break;

                    case TokenType.ParamDef:
                        methodDef = ( (ParameterDeclaration) targetElement ).DeclaringMethod;
                        typeDef = methodDef.DeclaringType;
                        externalInheritance = VisibilityHelper.IsPublic( methodDef ) &&
                                              methodDef.IsVirtual && !methodDef.IsSealed && !typeDef.IsSealed
                                                  ?
                                                      MulticastInheritance.Strict
                                                  : MulticastInheritance.None;
                        break;

                    default:
                        externalInheritance = MulticastInheritance.None;
                        break;
                }

                externalInheritance =
                    (MulticastInheritance) Math.Min( (int) externalInheritance, (int) instanceInfo.Inheritance );

                if ( externalInheritance > MulticastInheritance.None )
                {
                    // If the element can be inherited and has an inherited custom attribute, we
                    // mark the eleement with [HasInheritedAttributesAttribute].
                    AddHasInheritedAttributesAttribute( this.module.AssemblyManifest, 0 );

                    if ( typeDef != null )
                    {
                        AddHasInheritedAttributesAttribute( typeDef, 0 );
                    }

                    if ( methodDef != null )
                    {
                        AddHasInheritedAttributesAttribute( methodDef, 0 );
                    }
                }
            }
            else
            {
                externalInheritance = MulticastInheritance.None;
            }


            if ( instanceInfo.Usage.PersistMetaData || externalInheritance != MulticastInheritance.None )
            {
                CustomAttributeDeclaration customAttribute;

                // If metadata dont have to be persisted, we try to use a pooled attribute.
                bool pooled = !instanceInfo.Usage.PersistMetaData;
                if ( pooled )
                {
                    writtenPooledInheritableAttributes.TryGetValue( instanceInfo.Id, out customAttribute );
                }
                else
                {
                    customAttribute = null;
                }


                if ( customAttribute == null )
                {
                    // We shoudld create a new custom attribute.
                    customAttribute = new CustomAttributeDeclaration( instanceInfo.Annotation.Constructor );

                    foreach ( MemberValuePair memberValuePair in instanceInfo.Annotation.ConstructorArguments )
                    {
                        customAttribute.ConstructorArguments.Add( memberValuePair.Clone( memberValuePair.Ordinal ) );
                    }

                    int ordinal = 0;
                    foreach ( MemberValuePair memberValuePair in instanceInfo.Annotation.NamedArguments )
                    {
                        if ( !( memberValuePair.MemberKind == MemberKind.Property &&
                                systemProperties.Contains( memberValuePair.MemberName ) ) )
                        {
                            customAttribute.NamedArguments.Add( memberValuePair.Clone( ordinal ) );
                            ordinal++;
                        }
                    }

                    if ( externalInheritance != MulticastInheritance.None )
                    {
                        if ( externalInheritance != instanceInfo.Usage.Inheritance )
                        {
                            customAttribute.NamedArguments.Add( new MemberValuePair(
                                                                    MemberKind.Property,
                                                                    customAttribute.NamedArguments.Count,
                                                                    "AttributeInheritance",
                                                                    new SerializedValue(
                                                                        new EnumerationSerializationType(
                                                                            this.multicastInheritanceType ),
                                                                        (int) externalInheritance ) ) );
                        }

                        customAttribute.NamedArguments.Add( new MemberValuePair(
                                                                MemberKind.Property,
                                                                customAttribute.NamedArguments.Count,
                                                                "AttributeId",
                                                                IntrinsicSerializationType.CreateValue( this.module,
                                                                                                        instanceInfo.Id ) ) );

                        customAttribute.NamedArguments.Add( new MemberValuePair(
                                                                MemberKind.Property,
                                                                customAttribute.NamedArguments.Count,
                                                                "AttributePriority",
                                                                IntrinsicSerializationType.CreateValue( this.module,
                                                                                                        instanceInfo.
                                                                                                            Priority |
                                                                                                        (int)
                                                                                                        InstancePriority
                                                                                                            .Inherited ) ) );

                        if ( pooled || instanceInfo.Inheritance == MulticastInheritance.Multicast )
                        {
                            // Since the attribute will be multicasted, we need to store multicasting information,
                            // at least the one relevant for the multicasting process. We add only information that
                            // is not redundant with usage information.
                            if ( pooled ||
                                 targetTokenType == TokenType.TypeDef ||
                                 targetTokenType == TokenType.Assembly )
                            {
                                MulticastTargets targetsUnderType =
                                    pooled
                                        ? MulticastTargets.All
                                        :
                                            MulticastTargets.AnyMember | MulticastTargets.Parameter |
                                            MulticastTargets.ReturnValue;

                                MulticastTargets targets = instanceInfo.TargetElements & targetsUnderType;

                                if ( targets != ( instanceInfo.Usage.ValidOn & targetsUnderType ) )
                                {
                                    customAttribute.NamedArguments.Add( new MemberValuePair(
                                                                            MemberKind.Property,
                                                                            customAttribute.NamedArguments.Count,
                                                                            "AttributeTargetElements",
                                                                            new SerializedValue(
                                                                                new EnumerationSerializationType(
                                                                                    this.multicastTargetsType ),
                                                                                (int) targets ) ) );
                                }

                                if ( ( instanceInfo.TargetMemberAttributes & MulticastAttributes.All ) !=
                                     instanceInfo.Usage.TargetMemberAttributes )
                                {
                                    customAttribute.NamedArguments.Add( new MemberValuePair(
                                                                            MemberKind.Property,
                                                                            customAttribute.NamedArguments.Count,
                                                                            "AttributeTargetMemberAttributes",
                                                                            new SerializedValue(
                                                                                new EnumerationSerializationType(
                                                                                    this.multicastAttributesType ),
                                                                                (int) instanceInfo.TargetTypeAttributes ) ) );
                                }

                                if ( instanceInfo.TargetMembersRegex != null )
                                {
                                    customAttribute.NamedArguments.Add( new MemberValuePair(
                                                                            MemberKind.Property,
                                                                            customAttribute.NamedArguments.Count,
                                                                            "AttributeTargetMembers",
                                                                            IntrinsicSerializationType.CreateValue(
                                                                                this.module,
                                                                                "regex:" +
                                                                                instanceInfo.TargetMembersRegex ) ) );
                                }

                                if ( ( instanceInfo.TargetParameterAttributes & MulticastAttributes.All ) !=
                                     instanceInfo.Usage.TargetParameterAttributes )
                                {
                                    customAttribute.NamedArguments.Add( new MemberValuePair(
                                                                            MemberKind.Property,
                                                                            customAttribute.NamedArguments.Count,
                                                                            "AttributeTargetParameterAttributes",
                                                                            new SerializedValue(
                                                                                new EnumerationSerializationType(
                                                                                    this.multicastAttributesType ),
                                                                                (int) instanceInfo.TargetTypeAttributes ) ) );
                                }

                                if ( instanceInfo.TargetParametersRegex != null )
                                {
                                    customAttribute.NamedArguments.Add( new MemberValuePair(
                                                                            MemberKind.Property,
                                                                            customAttribute.NamedArguments.Count,
                                                                            "AttributeTargetParameters",
                                                                            IntrinsicSerializationType.CreateValue(
                                                                                this.module,
                                                                                "regex:" +
                                                                                instanceInfo.TargetParametersRegex ) ) );
                                }

                                if ( pooled || targetTokenType == TokenType.Assembly )
                                {
                                    if ( ( instanceInfo.TargetTypeAttributes & MulticastAttributes.All ) !=
                                         instanceInfo.Usage.TargetTypeAttributes )
                                    {
                                        customAttribute.NamedArguments.Add( new MemberValuePair(
                                                                                MemberKind.Property,
                                                                                customAttribute.NamedArguments.Count,
                                                                                "AttributeTargetTypeAttributes",
                                                                                new SerializedValue(
                                                                                    new EnumerationSerializationType(
                                                                                        this.multicastAttributesType ),
                                                                                    (int)
                                                                                    instanceInfo.TargetTypeAttributes ) ) );
                                    }

                                    if ( instanceInfo.TargetTypesRegex != null )
                                    {
                                        customAttribute.NamedArguments.Add( new MemberValuePair(
                                                                                MemberKind.Property,
                                                                                customAttribute.NamedArguments.Count,
                                                                                "AttributeTargetTypes",
                                                                                IntrinsicSerializationType.CreateValue(
                                                                                    this.module,
                                                                                    "regex:" +
                                                                                    instanceInfo.TargetTypesRegex ) ) );
                                    }
                                }
                            }
                        }
                    }

                    if ( !pooled )
                    {
                        targetElement.CustomAttributes.Add( customAttribute );
                    }
                    else
                    {
                        CreateInheritedAttributesAttributesPoolType();
                        this.inheritedAttributesAttributesPoolType.CustomAttributes.Add( customAttribute );
                        this.writtenPooledInheritableAttributes.Add( instanceInfo.Id, customAttribute );
                    }
                }

                if ( pooled )
                {
                    // We should add a reference to the pooled attribute.
                    AddHasInheritedAttributesAttribute( targetElement, instanceInfo.Id );
                }
            }
        }

        private TypeDefDeclaration inheritedAttributesAttributesPoolType;
        private const string inheritedAttributesAttributesPoolTypeName = "<>MulticastImplementationDetails";

        private void CreateInheritedAttributesAttributesPoolType()
        {
            if ( this.inheritedAttributesAttributesPoolType != null ) return;
            this.inheritedAttributesAttributesPoolType = new TypeDefDeclaration
                                                             {
                                                                 Name = inheritedAttributesAttributesPoolTypeName,
                                                                 Attributes = TypeAttributes.NotPublic
                                                             };
            this.module.Types.Add( inheritedAttributesAttributesPoolType );
        }

        private void AddHasInheritedAttributesAttribute( MetadataDeclaration declaration, long id )
        {
            CustomAttributeDeclaration attribute;

            if ( !hasInheritedAttributesAttributes.TryGetValue( declaration, out attribute ) )
            {
                attribute = new CustomAttributeDeclaration( this.hasInheritedAttributesAttributeConstructor );
                attribute.ConstructorArguments.Add( new MemberValuePair( MemberKind.Parameter,
                                                                         0, "ids",
                                                                         new SerializedValue(
                                                                             new ArraySerializationType(
                                                                                 IntrinsicSerializationType.GetInstance(
                                                                                     this.module, IntrinsicType.Int64 ) ),
                                                                             null ) ) );
                declaration.CustomAttributes.Add( attribute );
                hasInheritedAttributesAttributes.Add( declaration, attribute );
            }

            if ( id != 0 )
            {
                List<long> ids = (List<long>) attribute.ConstructorArguments[0].Value.Value;
                if ( ids == null )
                {
                    ids = new List<long>();
                    attribute.ConstructorArguments[0].Value =
                        new SerializedValue( attribute.ConstructorArguments[0].Value.Type, ids );
                }
                ids.Add( id );
            }

            return;
        }

        #endregion

        #region Inheritance

        private readonly Dictionary<long, MulticastAttributeInstanceInfo> importedPooledInstances =
            new Dictionary<long, MulticastAttributeInstanceInfo>();

        private void ProcessInheritedAttributesAcrossAssemblies()
        {
            // Look for attributes on referenced assemblies.
            // We should take a copy of the collection AssemblyRefs, because it may be modified (issue 362).
            foreach ( AssemblyRefDeclaration assemblyRef in module.AssemblyRefs.ToArray() )
            {
                if ( AssemblyHasInheritedAttributes( assemblyRef ) )
                {
                    // Check for pooled attributes.
                    AssemblyEnvelope assemblyEnvelope = assemblyRef.GetAssemblyEnvelope();
                    TypeDefDeclaration pool =
                        assemblyEnvelope.ManifestModule.Types.GetByName( inheritedAttributesAttributesPoolTypeName );

                    if ( pool != null )
                    {
                        foreach ( CustomAttributeDeclaration attribute in pool.CustomAttributes )
                        {
                            if ( IsAttributeInherited( attribute, false ) )
                            {
                                MulticastAttributeInstanceInfo instanceInfo = GetInstanceInfo( assemblyRef, attribute.Translate( this.module ),
                                                                                               false );
                                instanceInfo.Inherited = true;

                                // If the attribute is imported from more than 1 assembly, there can be duplicates.
                                // We need a single copy of the attribute.
                                if ( !importedPooledInstances.ContainsKey( instanceInfo.Id ))
                                    this.importedPooledInstances.Add( instanceInfo.Id, instanceInfo );
                            }
                        }
                    }

                    // Apply inherited custom attributes.
                    foreach (CustomAttributeDeclaration customAttribute in assemblyEnvelope.ManifestModule.AssemblyManifest.CustomAttributes)
                    {
                        ImportCustomAttribute( module.AssemblyManifest, customAttribute );
                    }
                }
            }

            // Look for attributes on base types.
            IEnumerator<MetadataDeclaration> typeEnumerator = module.GetDeclarationEnumerator( TokenType.TypeDef );

            while ( typeEnumerator.MoveNext() )
            {
                TypeDefDeclaration typeDef = (TypeDefDeclaration) typeEnumerator.Current;

                // Process the base type.
                if ( typeDef.BaseType != null )
                {
                    ProcessInheritedAttributesAcrossAssemblies( typeDef, typeDef.BaseType );


                    // Process interface implementations.
                    foreach (
                        InterfaceImplementationDeclaration interfaceImplementation in typeDef.InterfaceImplementations )
                    {
                        ProcessInheritedAttributesAcrossAssemblies( typeDef,
                                                                    interfaceImplementation.ImplementedInterface );
                    }
                }
            }
        }

        private void ImportCustomAttribute( IMetadataDeclaration target, CustomAttributeDeclaration customAttribute )
        {
            if ( IsAttributeInherited( customAttribute ) )
            {
                MulticastAttributeInstanceInfo instanceInfo = GetInstanceInfo( customAttribute );
                if ( instanceInfo == null ) return;

                instanceInfo.Inherited = true;
                this.PropagateIntermediateInstance( target, instanceInfo );
            }
            else if (
                customAttribute.Constructor.DeclaringType.Equals(
                    customAttribute.Module.Cache.GetType( typeof(HasInheritedAttributeAttribute) ) ) )
            {
                // This may be an attribute reference.
                object[] ids = (object[]) customAttribute.ConstructorArguments[0].Value.Value;
                if ( ids != null )
                {
                    foreach ( long id in ids )
                    {
                        this.PropagateIntermediateInstance( target, this.importedPooledInstances[id] );
                    }
                }
            }
        }

        private void ProcessInheritedAttributesAcrossAssemblies( TypeDefDeclaration typeDef, ITypeSignature baseType )
        {
            // Get the base type.
            TypeRefDeclaration baseTypeRef;
            GenericTypeInstanceTypeSignature baseTypeGenericInstance = baseType as GenericTypeInstanceTypeSignature;
            GenericMap genericMap; 
            if (baseTypeGenericInstance != null)
            {
                baseTypeRef = baseTypeGenericInstance.GenericDefinition as TypeRefDeclaration;
                genericMap = baseTypeGenericInstance.GetGenericContext( GenericContextOptions.None );
            }
            else
            {
                baseTypeRef = baseType as TypeRefDeclaration;
                genericMap = typeDef.GetGenericContext( GenericContextOptions.None );
            }

            if ( baseTypeRef == null ) return;


            AssemblyRefDeclaration baseAssemblyRef = baseTypeRef.ResolutionScope as AssemblyRefDeclaration;

            if ( baseAssemblyRef == null )
                return;

            if ( AssemblyHasInheritedAttributes( baseAssemblyRef ) )
            {
                TypeDefDeclaration baseTypeDef = baseTypeRef.GetTypeDefinition();
                IType hasInheritedAttributeAttributeType =
                    (IType) baseTypeDef.Module.Cache.GetType( typeof(HasInheritedAttributeAttribute) );

                if ( baseTypeDef.CustomAttributes.Contains( hasInheritedAttributeAttributeType ) )
                {
                    // The base type (or one of its items) has inherited attributes.
                    // We should look at all custom attributes of the type.
                    foreach ( CustomAttributeDeclaration customAttribute in baseTypeDef.CustomAttributes )
                    {
                        ImportCustomAttribute( typeDef, customAttribute );
                    }

                    // Apply attributes on methods.
                    foreach (
                        MappedGenericDeclaration<MethodDefDeclaration> baseMethodDef in FindMethodsWithInheritedAttributes( baseTypeDef ) )
                    {
                        foreach (
                            MethodDefDeclaration childMethodDef in FindMethodOverrides( typeDef, baseMethodDef.Apply( genericMap ), true ) )
                        {
                            ImportCustomAttributes( baseMethodDef.Declaration, childMethodDef );
                        }
                    }
                }
            }
        }


        private MulticastAttributeInstanceInfo GetInstanceInfo( CustomAttributeDeclaration customAttribute )
        {
            MulticastAttributeInstanceInfo instanceInfo;
            if ( !instanceInfos.TryGetValue( customAttribute, out instanceInfo ) )
            {
                CustomAttributeDeclaration translatedCustomAttribute =
                    customAttribute.Translate( this.module );

                instanceInfo = GetInstanceInfo( customAttribute.Parent, translatedCustomAttribute, true );

                instanceInfos.Add( customAttribute, instanceInfo );
            }

            return instanceInfo;
        }

        private long GetNextAttributeId()
        {
            return this.nextAttributeId++;
        }

        private MulticastAttributeInstanceInfo GetInstanceInfo( MetadataDeclaration target, IAnnotationValue annotation,
                                                                bool preventInheritanceOverwrite )
        {
            // Determine the attribute unique identifier. Import it if already defined; otherwise, define
            // a new one.
            long attributeId;
            MemberValuePair attributeIdMemberValuePair = annotation.NamedArguments["AttributeId"];
            if ( attributeIdMemberValuePair != null )
            {
                attributeId = (long) attributeIdMemberValuePair.Value.Value;
            }
            else
            {
                attributeId = this.GetNextAttributeId();
            }

            bool hasError = false;
            MulticastAttributeUsageAttribute customAttributeUsage =
                GetCustomAttributeUsage( annotation.Constructor.DeclaringType );

            MulticastAttributeInstanceInfo instanceInfo = new MulticastAttributeInstanceInfo(
                target,
                annotation,
                customAttributeUsage,
                attributeId
                );

            // Check that the custom attribute has valid target attributes, if specified.
            if ( instanceInfo.TargetTypeAttributes != MulticastAttributes.Default )
            {
                if ( ( instanceInfo.TargetTypeAttributes & MulticastAttributes.AnyVisibility &
                       ~customAttributeUsage.TargetTypeAttributes ) != 0 )
                {
                    // Invalid property {2} for the multicast attribute [{0}] instantiated
                    // on {1} "{2}": targets {4} are not supported by this custom attribute type.
                    CoreMessageSource.Instance.Write( SeverityType.Error, "PS0090",
                                                      new object[]
                                                          {
                                                              ( (INamedType)
                                                                instanceInfo.Annotation.Constructor.DeclaringType ).Name
                                                              ,
                                                              instanceInfo.DeclaredOn.GetTokenType(),
                                                              instanceInfo.DeclaredOn.ToString(),
                                                              "TargetTypeAttributes",
                                                              ( instanceInfo.TargetTypeAttributes &
                                                                MulticastAttributes.AnyVisibility ) &
                                                              ~customAttributeUsage.TargetTypeAttributes
                                                          } );
                    hasError = true;
                }

                instanceInfo.TargetTypeAttributes =
                    ApplyMulticastAttribute( customAttributeUsage.TargetTypeAttributes,
                                             instanceInfo.TargetTypeAttributes );
            }
            else
            {
                instanceInfo.TargetTypeAttributes = customAttributeUsage.TargetTypeAttributes;
            }

            if ( instanceInfo.TargetMemberAttributes != MulticastAttributes.Default )
            {
                if ( ( instanceInfo.TargetMemberAttributes & MulticastAttributes.All &
                       ~customAttributeUsage.TargetMemberAttributes ) != 0 )
                {
                    // Invalid property {2} for the multicast attribute [{0}] instantiated
                    // on {1} "{2}": targets {4} are not supported by this custom attribute type.
                    CoreMessageSource.Instance.Write( SeverityType.Error, "PS0090",
                                                      new object[]
                                                          {
                                                              ( (INamedType)
                                                                instanceInfo.Annotation.Constructor.DeclaringType ).Name
                                                              ,
                                                              instanceInfo.DeclaredOn.GetTokenType(),
                                                              instanceInfo.DeclaredOn.ToString(),
                                                              "AttributeTargetMemberAttributes",
                                                              instanceInfo.TargetMemberAttributes &
                                                              MulticastAttributes.All &
                                                              ~customAttributeUsage.TargetMemberAttributes
                                                          } );
                    hasError = true;
                }

                instanceInfo.TargetMemberAttributes =
                    ApplyMulticastAttribute( customAttributeUsage.TargetMemberAttributes,
                                             instanceInfo.TargetMemberAttributes );
            }
            else
            {
                instanceInfo.TargetMemberAttributes = customAttributeUsage.TargetMemberAttributes;
            }

            if ( instanceInfo.TargetParameterAttributes != MulticastAttributes.Default )
            {
                if ( ( instanceInfo.TargetParameterAttributes & MulticastAttributes.All &
                       ~customAttributeUsage.TargetParameterAttributes ) != 0 )
                {
                    // Invalid property {2} for the multicast attribute [{0}] instantiated
                    // on {1} "{2}": targets {4} are not supported by this custom attribute type.
                    CoreMessageSource.Instance.Write( SeverityType.Error, "PS0090",
                                                      new object[]
                                                          {
                                                              ( (INamedType)
                                                                instanceInfo.Annotation.Constructor.DeclaringType ).Name
                                                              ,
                                                              instanceInfo.DeclaredOn.GetTokenType(),
                                                              instanceInfo.DeclaredOn.ToString(),
                                                              "AttributeTargetParameterAttributes",
                                                              instanceInfo.TargetParameterAttributes &
                                                              MulticastAttributes.All &
                                                              ~customAttributeUsage.TargetParameterAttributes
                                                          } );
                    hasError = true;
                }

                instanceInfo.TargetParameterAttributes =
                    ApplyMulticastAttribute( customAttributeUsage.TargetParameterAttributes,
                                             instanceInfo.TargetParameterAttributes );
            }
            else
            {
                instanceInfo.TargetParameterAttributes = customAttributeUsage.TargetParameterAttributes;
            }

            if ( instanceInfo.TargetElements != MulticastTargets.Default )
            {
                if ( ( instanceInfo.TargetElements & ~customAttributeUsage.ValidOn ) != 0 )
                {
                    // Invalid property {2} for the multicast attribute [{0}] instantiated
                    // on {1} "{2}": targets {4} are not supported by this custom attribute type.
                    CoreMessageSource.Instance.Write( SeverityType.Error, "PS0090",
                                                      new object[]
                                                          {
                                                              ( (INamedType)
                                                                instanceInfo.Annotation.Constructor.DeclaringType ).Name
                                                              ,
                                                              instanceInfo.DeclaredOn.GetTokenType(),
                                                              instanceInfo.DeclaredOn.ToString(),
                                                              "AttributeTargetElements",
                                                              instanceInfo.TargetElements &
                                                              ~customAttributeUsage.ValidOn
                                                          } );
                    hasError = true;
                }
            }
            else
            {
                instanceInfo.TargetElements = customAttributeUsage.ValidOn;
            }

            if ( instanceInfo.IsInheritanceSpecified )
            {
                if ( customAttributeUsage.IsInheritanceSpecified )
                {
                    if ( preventInheritanceOverwrite )
                    {
                        // Invalid property {3} for the multicast attribute [{0}] instantiated on {1} "{2}": this property is sealed and cannot be overwritten.
                        CoreMessageSource.Instance.Write( SeverityType.Error, "PS0108",
                                                          new object[]
                                                              {
                                                                  ( (INamedType)
                                                                    instanceInfo.Annotation.Constructor.DeclaringType ).
                                                                      Name,
                                                                  instanceInfo.DeclaredOn.GetTokenType(),
                                                                  instanceInfo.DeclaredOn.ToString(),
                                                                  "AttributeInheritance"
                                                              } );

                        hasError = true;
                    }
                }
            }
            else
            {
                instanceInfo.Inheritance = customAttributeUsage.Inheritance;
            }

            return hasError ? null : instanceInfo;
        }

        private IEnumerable<MethodDefDeclaration> FindMethodOverrides( TypeDefDeclaration baseTypeDef,
                                                                       MappedGenericDeclaration<MethodDefDeclaration> baseMethod,
                                                                       bool includeBaseType )
        {
            // Here is the performance killer. For each method, we have to determine if
            // it is overriden by any child. We have possibly also to look at the current type.
            this.HasInheritedAttribute();

            IEnumerator<TypeHierarchyTask.DerivedTypeInfo> childrenEnumerator =
                TypeHierarchyTask.GetDerivedTypeInfosEnumerator( baseTypeDef );

            TypeDefDeclaration childTypeDef;
            GenericMap genericMapToParent;

            if (includeBaseType)
            {
                childTypeDef = baseTypeDef;
                genericMapToParent = baseTypeDef.GetGenericContext( GenericContextOptions.None );
            }
            else
            {
                if (!childrenEnumerator.MoveNext())
                    yield break;

                childTypeDef = childrenEnumerator.Current.ChildType;
                genericMapToParent = childrenEnumerator.Current.GenericMapToParent;
            }

            while ( true )
            {
                foreach ( MethodDefDeclaration childMethodDef in childTypeDef.Methods )
                {
                    if ( !childMethodDef.IsVirtual )
                        continue;

                    if ( childMethodDef.InterfaceImplementations.Count == 0 )
                    {
                        // Overriding by name and signature.
                        if ( childMethodDef.Name == baseMethod.Declaration.Name &&
                             childMethodDef.Parameters.Count == baseMethod.Declaration.Parameters.Count &&
                             childMethodDef.MatchesReference(
                                 new MethodSignature(baseMethod.Declaration).MapGenericArguments(baseMethod.GenericMap.Apply(genericMapToParent)).Translate(childMethodDef.Module)))
                        {
                            yield return childMethodDef;
                        }
                    }
                    else
                    {
                        // Explicit overriding.
                        foreach (
                            MethodImplementationDeclaration interfaceImpl in childMethodDef.InterfaceImplementations )
                        {
                            IMethod overridenMethod = interfaceImpl.ImplementedMethod;

                            if ( overridenMethod.ParameterCount == baseMethod.Declaration.Parameters.Count &&
                                 overridenMethod.GetMethodDefinition() == baseMethod.Declaration )
                            {
                                yield return childMethodDef;
                            }
                        }
                    }
                }

                if ( !childrenEnumerator.MoveNext() ) break;
                childTypeDef = childrenEnumerator.Current.ChildType;
                genericMapToParent = childrenEnumerator.Current.GenericMapToParent;
            }
        }

        private void CopyInheritedIntermediateInstances( MethodDefDeclaration sourceMethodDef,
                                                         MethodDefDeclaration targetMethodDef )
        {
            CopyInheritedIntermediateInstances( (IMetadataDeclaration) sourceMethodDef, targetMethodDef );
            CopyInheritedIntermediateInstances( sourceMethodDef.ReturnParameter, targetMethodDef.ReturnParameter );


            int n = sourceMethodDef.Parameters.Count;
            for ( int i = 0; i < n; i++ )
            {
                this.CopyInheritedIntermediateInstances( sourceMethodDef.Parameters[i], targetMethodDef.Parameters[i] );
            }
        }

        private void CopyInheritedIntermediateInstances( IMetadataDeclaration source, IMetadataDeclaration target )
        {
            MultiDictionary<ITypeSignature, MulticastAttributeInstanceInfo> instancesOnElement =
                GetIntermediateInstances( source, false );

            if ( instancesOnElement != null )
            {
                foreach ( KeyValuePair<ITypeSignature, MulticastAttributeInstanceInfo> pair in instancesOnElement )
                {
                    if ( pair.Value.Inheritance != MulticastInheritance.None )
                    {
                        MulticastAttributeInstanceInfo inheritedInstanceInfo = pair.Value.Clone();
                        inheritedInstanceInfo.Inherited = true;
                        this.AddIntermediateInstance( target, inheritedInstanceInfo );
                    }
                }
            }
        }

        private void ImportCustomAttributes( MethodDefDeclaration sourceMethodDef, MethodDefDeclaration targetMethodDef )
        {
            ImportCustomAttributes( (IMetadataDeclaration) sourceMethodDef, targetMethodDef );
            ImportCustomAttributes( sourceMethodDef.ReturnParameter, targetMethodDef.ReturnParameter );


            int n = sourceMethodDef.Parameters.Count;
            for ( int i = 0; i < n; i++ )
            {
                this.ImportCustomAttributes( sourceMethodDef.Parameters[i], targetMethodDef.Parameters[i] );
            }
        }


        private void ImportCustomAttributes( IMetadataDeclaration source, IMetadataDeclaration target )
        {
            foreach ( CustomAttributeDeclaration customAttribute in source.CustomAttributes )
            {
                ImportCustomAttribute( target, customAttribute );
            }
        }


        private static readonly Guid methodsWithInheritedAttributesGuid =
            new Guid( "B927221D-60CA-412E-90CF-C5846DB47E21" );
        
        private static IEnumerable<MappedGenericDeclaration<MethodDefDeclaration>> FindMethodsWithInheritedAttributes( TypeDefDeclaration typeDef )
        {
            GenericMap genericMap = typeDef.GetGenericContext( GenericContextOptions.None );
            TypeDefDeclaration typeCursor = typeDef;

            while ( true )
            {
                List<MethodDefDeclaration> methodsWithInheritedAttributes =
                    (List<MethodDefDeclaration>) typeCursor.GetTag( methodsWithInheritedAttributesGuid );

                if ( methodsWithInheritedAttributes == null )
                {
                    methodsWithInheritedAttributes = new List<MethodDefDeclaration>();
                    typeCursor.SetTag( methodsWithInheritedAttributesGuid, methodsWithInheritedAttributes );

                    foreach ( MethodDefDeclaration method in typeCursor.Methods )
                    {
                        if ( !method.IsVirtual || method.IsSealed )
                            continue;


                        if ( method.CustomAttributes.Contains( (IType)
                                                               typeCursor.Module.Cache.GetType(
                                                                   typeof(HasInheritedAttributeAttribute) ) ) )
                        {
                            methodsWithInheritedAttributes.Add( method );
                        }
                    }
                }

                foreach ( MethodDefDeclaration method in methodsWithInheritedAttributes )
                {
                    yield return new MappedGenericDeclaration<MethodDefDeclaration>(method, genericMap);
                }


                if ( typeCursor.BaseType == null ) break;

                GenericTypeInstanceTypeSignature genericBaseType = typeCursor.BaseType.GetNakedType(TypeNakingOptions.None) as GenericTypeInstanceTypeSignature;
                if ( genericBaseType != null )
                {
                    genericMap = genericBaseType.GetGenericContext( GenericContextOptions.None ).Apply( genericMap );
                }
                
                typeCursor = typeCursor.BaseType.GetTypeDefinition();

                if ( typeCursor.CustomAttributes.Contains( (IType)
                                                           typeCursor.Module.Cache.GetType(
                                                               typeof(HasInheritedAttributeAttribute) ) ) )
                    break;
            }
        }

        private MulticastAttributeUsageAttribute GetCustomAttributeUsage( ITypeSignature type )
        {
            return GetCustomAttributeUsage( type.GetTypeDefinition() );
        }

        private static readonly Guid attributeTypeDerivedFromMulticastGuid =
            new Guid( "49AF1BCD-07B6-41C1-91C2-BF4810C4F12A" );

        private static readonly object attributeTypeDerivedFromMulticastYes = new object();
        private static readonly object attributeTypeDerivedFromMulticastNo = new object();
        private bool hasInheritedAttribute;
        private INamedType multicastInheritanceType;
        private ModuleDeclaration module;
        private IMethod hasInheritedAttributesAttributeConstructor;
        private INamedType multicastAttributesType;
        private INamedType multicastTargetsType;

        private bool IsAttributeInherited( CustomAttributeDeclaration attribute )
        {
            return IsAttributeInherited( attribute, true );
        }

        private bool IsAttributeInherited( CustomAttributeDeclaration attribute, bool requiresInheritanceProperty )
        {
            TypeDefDeclaration attributeTypeDef = attribute.Constructor.DeclaringType.GetTypeDefinition();

            if ( IsTypeDerivedFromMulticastAttribute( attributeTypeDef ) )
            {
                if ( !requiresInheritanceProperty )
                    return true;

                MulticastAttributeUsageAttribute usageAttribute =
                    this.GetCustomAttributeUsage( attributeTypeDef );

                if (usageAttribute.IsInheritanceSpecified)
                    return usageAttribute.Inheritance != MulticastInheritance.None;

                MemberValuePair memberValuePair =
                    attribute.NamedArguments["AttributeInheritance"];
                if ( memberValuePair != null &&
                     memberValuePair.MemberKind == MemberKind.Property &&
                     memberValuePair.Value.Value is int &&
                     (int) memberValuePair.Value.Value != (int) MulticastInheritance.None
                    )
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTypeDerivedFromMulticastAttribute( TypeDefDeclaration typeDef )
        {
            object tag = typeDef.GetTag( attributeTypeDerivedFromMulticastGuid );

            if ( tag != null )
            {
                return tag == attributeTypeDerivedFromMulticastYes;
            }

            bool value;

            if ( typeDef.Name == typeof(MulticastAttribute).FullName )
            {
                value = true;
            }
            else if ( typeDef.BaseType == null )
            {
                value = false;
            }
            else
            {
                value = IsTypeDerivedFromMulticastAttribute( typeDef.BaseType.GetTypeDefinition() );
            }

            typeDef.SetTag( attributeTypeDerivedFromMulticastGuid,
                            value ? attributeTypeDerivedFromMulticastYes : attributeTypeDerivedFromMulticastNo );

            return value;
        }

        private bool AssemblyHasInheritedAttributes( AssemblyRefDeclaration assemblyRef )
        {
            bool value;
            if ( !assemblyHasInheritedAttributes.TryGetValue( assemblyRef, out value ) )
            {
                string assemblyName = assemblyRef.Name.ToLowerInvariant();
                if ( assemblyName == "mscorlib" || assemblyName.StartsWith( "system" ) )
                {
                    value = false;
                }
                else
                {
                    AssemblyEnvelope assemblyEnvelope = assemblyRef.GetAssemblyEnvelope();

                    if ( !assemblyHasInheritedAttributes.TryGetValue( assemblyEnvelope, out value ) )
                    {
                        value = assemblyEnvelope.ManifestModule.AssemblyManifest.CustomAttributes.Contains(
                            (IType)
                            assemblyEnvelope.ManifestModule.Cache.GetType( typeof(HasInheritedAttributeAttribute) )
                            );

                        assemblyHasInheritedAttributes.Add( assemblyEnvelope, value );
                    }
                }

                assemblyHasInheritedAttributes.Add( assemblyRef, value );
            }

            return value;
        }

        #endregion

        #region Instance class

        private class MulticastAttributeInstanceInfo : IComparable<MulticastAttributeInstanceInfo>
        {
            public readonly IAnnotationValue Annotation;
            public readonly Regex TargetTypesRegex;
            public readonly Regex TargetMembersRegex;
            public readonly Regex TargetAssembliesRegex;
            public readonly Regex TargetParametersRegex;
            public readonly int Priority;
            public MulticastAttributes TargetTypeAttributes;
            public MulticastAttributes TargetMemberAttributes;
            public MulticastAttributes TargetParameterAttributes;
            public MulticastTargets TargetElements;
            public readonly bool Exclude;
            public readonly MulticastAttributeUsageAttribute Usage;
            public readonly bool Replace;
            public MulticastInheritance Inheritance;
            public readonly bool IsInheritanceSpecified;
            public bool Inherited;
            public readonly MetadataDeclaration DeclaredOn;
            public readonly Set<MetadataDeclaration> AppliedOn = new Set<MetadataDeclaration>();
            public readonly long Id;

            private bool TryGetNamedValue<T>( string propertyName, out T value )
            {
                MemberValuePair memberValuePair = this.Annotation.NamedArguments[propertyName];

                if ( memberValuePair == null )
                {
                    value = default( T );
                    return false;
                }
                else
                {
                    value = (T) memberValuePair.Value.GetRuntimeValue();
                    return true;
                }
            }

            private T GetNamedValue<T>( string propertyName, T defaultValue )
            {
                MemberValuePair value = this.Annotation.NamedArguments[propertyName];

                if ( value == null )
                {
                    return defaultValue;
                }
                else
                {
                    return (T) value.Value.GetRuntimeValue();
                }
            }

            public MulticastAttributeInstanceInfo( MetadataDeclaration declaredOn, IAnnotationValue annotation,
                                                   MulticastAttributeUsageAttribute usage, long id )
            {
                this.DeclaredOn = declaredOn;

                InstancePriority declaringElement;

                switch ( this.DeclaredOn.GetTokenType() )
                {
                    case TokenType.Assembly:
                        declaringElement = InstancePriority.Assembly;
                        break;

                    case TokenType.Event:
                    case TokenType.Property:
                        declaringElement = InstancePriority.MethodSemanticCollection;
                        break;

                    case TokenType.MethodDef:
                        declaringElement = InstancePriority.Method;
                        break;

                    case TokenType.FieldDef:
                        declaringElement = InstancePriority.Field;
                        break;

                    case TokenType.TypeDef:
                        declaringElement = InstancePriority.Type;
                        break;

                    case TokenType.AssemblyRef:
                        declaringElement = InstancePriority.None;
                        break;

                    default:
                        declaringElement = InstancePriority.Other;
                        break;
                }


                this.Usage = usage;
                this.Annotation = annotation;
                this.TargetTypesRegex = GetRegex( this.GetNamedValue<string>( "AttributeTargetTypes", null ),
                                                  "AttributeTargetTypes" );
                this.TargetMembersRegex = GetRegex( this.GetNamedValue<string>( "AttributeTargetMembers", null ),
                                                    "AttributeTargetMembers" );
                this.TargetParametersRegex = GetRegex( this.GetNamedValue<string>( "AttributeTargetParameters", null ),
                                                       "AttributeTargetParameters" );
                this.TargetAssembliesRegex = GetRegex( this.GetNamedValue<string>( "AttributeTargetAssemblies", null ),
                                                       "AttributeTargetAssemblies" );
                this.Priority = this.GetNamedValue( "AttributePriority", 0 ) + (int) declaringElement;
                this.TargetTypeAttributes =
                    this.GetNamedValue( "AttributeTargetTypeAttributes", MulticastAttributes.Default );
                this.TargetMemberAttributes =
                    this.GetNamedValue( "AttributeTargetMemberAttributes", MulticastAttributes.Default );
                this.TargetParameterAttributes =
                    this.GetNamedValue( "AttributeTargetParameterAttributes", MulticastAttributes.Default );
                this.TargetElements = this.GetNamedValue( "AttributeTargetElements", MulticastTargets.Default );
                this.Exclude = this.GetNamedValue( "AttributeExclude", false );
                this.Replace = this.GetNamedValue( "AttributeReplace", false );
                this.IsInheritanceSpecified = this.TryGetNamedValue( "AttributeInheritance", out this.Inheritance );
                this.Id = this.GetNamedValue( "AttributeId", 0L );
                if ( this.Id == 0 ) this.Id = id;

            }

            private Regex GetRegex( string filter, string propertyName )
            {
                if ( filter == null )
                {
                    return null;
                }
                else if ( filter.StartsWith( "regex:", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    try
                    {
                        return new Regex( filter.Substring( 6 ) );
                    }
                    catch ( ArgumentException e )
                    {
                        // Invalid expression for property {0} of custom attributed {1} on {2} '{3}': {4}
                        CoreMessageSource.Instance.Write( SeverityType.Error, "PS0093",
                                                          new object[]
                                                              {
                                                                  propertyName,
                                                                  this.Annotation.Constructor.DeclaringType.ToString(),
                                                                  this.DeclaredOn.GetTokenType().ToString().
                                                                      ToLowerInvariant(),
                                                                  this.DeclaredOn.ToString(),
                                                                  e.Message
                                                              } );
                        return null;
                    }
                }
                {
                    return new Regex( "^" + Regex.Escape( filter ).Replace( "\\*", ".*" ) + "$", RegexOptions.IgnoreCase );
                }
            }

            #region IComparable<CustomAttributeInstance> Members

            public int CompareTo( MulticastAttributeInstanceInfo other )
            {
                return this.Priority.CompareTo( other.Priority );
            }

            #endregion

            public override string ToString()
            {
                return string.Format( "{0} @ {1}", CustomAttributeHelper.Render( this.Annotation ),
                                      this.DeclaredOn ?? (object) "?" );
            }

            public MulticastAttributeInstanceInfo Clone()
            {
                return (MulticastAttributeInstanceInfo) this.MemberwiseClone();
            }
        }

        #endregion

        private enum InstancePriority
        {
            None = 0,
            Assembly = 1 << 28,
            Type = 2 << 28,
            MethodSemanticCollection = 3 << 28,
            Method = 4 << 28,
            Field = 4 << 28,
            Other = 5 << 28,
            Inherited = 1 << 27
        }
    }
}