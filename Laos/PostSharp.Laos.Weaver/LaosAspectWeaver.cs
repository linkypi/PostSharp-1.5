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
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.Laos.Serializers;
using PostSharp.ModuleWriter;
using PostSharp.Reflection;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Base class that all weavers of Laos aspects (<see cref="ILaosAspect"/>)
    /// should implement.
    /// </summary>
    public abstract class LaosAspectWeaver : ILaosReflectionAspectProvider
    {
        private AspectSerializationData serializationData;
        private FieldDefDeclaration aspectRuntimeInstanceField;
        private List<ILaosAspectConfiguration> configurations;
        private readonly LaosAspectConfigurationAttribute defaultConfiguration;
        private int aspectPriority;

        /// <summary>
        /// Initializes a new <see cref="LaosAspectWeaver"/>.
        /// </summary>
        /// <param name="defaultConfiguration">Default weaver configuration.</param>
        protected LaosAspectWeaver( LaosAspectConfigurationAttribute defaultConfiguration )
        {
            this.defaultConfiguration = defaultConfiguration;
        }


        internal void SetProperties( LaosTask laosTask,
                                     string uniqueName,
                                     AspectTargetPair aspectTargetPair )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( laosTask, "laosTask" );
            ExceptionHelper.AssertArgumentNotEmptyOrNull( uniqueName, "uniqueName" );
            ExceptionHelper.AssertArgumentNotNull( aspectTargetPair, "aspectTargetPair" );

            #endregion

            this.Task = laosTask;
            this.TargetElement = aspectTargetPair.Target;
            this.AspectSpecification = aspectTargetPair.AspectSpecification;
            this.UniqueName = uniqueName;
        }

        #region Aspect configuration

        private void LoadAspectConfiguration()
        {
            this.configurations = new List<ILaosAspectConfiguration>( 3 );
            if ( this.defaultConfiguration != null )
                this.configurations.Add( defaultConfiguration );

            // If the aspect specification explicitely contains a configuration, return it.
            if ( this.AspectSpecification.AspectConfiguration != null )
            {
                this.configurations.Insert( 0, this.AspectSpecification.AspectConfiguration );
                return;
            }

            // If the aspect implements the configuration interface, return it.
            ILaosAspectConfiguration imperativeConfiguration = this.AspectSpecification.Aspect as ILaosAspectConfiguration;
            if ( imperativeConfiguration != null )
            {
                this.configurations.Insert( 0, imperativeConfiguration );
            }

            // We should look for a cutom attribute on the aspect type.
            if ( this.defaultConfiguration != null )
            {
                Domain domain = this.Task.Project.Module.Domain;
                TypeDefDeclaration aspectTypeDef = this.AspectSpecification.Aspect != null
                                                       ?
                                                           domain.FindTypeDefinition(
                                                               this.AspectSpecification.Aspect.GetType() )
                                                       :
                                                           domain.FindTypeDefinition(
                                                               this.AspectSpecification.AspectConstruction.TypeName );

                ILaosAspectConfiguration configurationAttribute = null;
                Set<string> setProperties = new Set<string>();

                TypeDefDeclaration cursorTypeDef = aspectTypeDef;
                while ( true )
                {
                    IType attributeType =
                        (IType)
                        cursorTypeDef.Module.GetTypeForFrameworkVariant( this.defaultConfiguration.GetType() );

                    if ( attributeType != null )
                    {
                        CustomAttributeDeclaration configurationData =
                            cursorTypeDef.CustomAttributes.GetOneByType( attributeType );

                        if ( configurationData != null )
                        {
                            // Create a new configuration attribute instance.
                            if ( configurationAttribute == null)
                            {
                                configurationAttribute =
                                    (ILaosAspectConfiguration)
                                    Activator.CreateInstance( this.defaultConfiguration.GetType() );
                                
                            }
                            // Set the propertes.
                            foreach ( MemberValuePair namedArgument in configurationData.NamedArguments )
                            {
                                if ( setProperties.AddIfAbsent( namedArgument.MemberName ) )
                                {
                                    object value = namedArgument.Value.GetRuntimeValue();
                                    PropertyInfo property =
                                        configurationAttribute.GetType().GetProperty( namedArgument.MemberName );
                                    property.SetValue( configurationAttribute, value, null );
                                }
                            }
                        }
                    }

                    if ( cursorTypeDef.BaseType == null )
                        break;
                    else
                        cursorTypeDef = cursorTypeDef.BaseType.GetTypeDefinition();
                }

                if (configurationAttribute != null)
                {
                    this.configurations.Insert( 0, configurationAttribute );
                }
            }
        }

        /// <summary>
        /// Gets an object (i.e., reference type) from the aspect configuration.
        /// </summary>
        /// <typeparam name="TConfiguration">Type of the aspect configuration.</typeparam>
        /// <typeparam name="TValue">Type of the retrieved aspect.</typeparam>
        /// <param name="func">Function (delegate) retrieving the value from a configuration element.</param>
        /// <returns>The configuration value, or <b>null</b> if no configuration element defined that property.</returns>
        protected TValue GetConfigurationObject<TConfiguration, TValue>(
            ConfigurationGetter<TConfiguration, TValue> func )
            where TConfiguration : class, ILaosAspectConfiguration
            where TValue : class
        {
            ExceptionHelper.AssertArgumentNotNull( func, "func" );

            for ( int i = 0; i < configurations.Count; i++ )
            {
                TConfiguration config = configurations[i] as TConfiguration;
                if (config != null)
                {
                    TValue value = func( (TConfiguration) configurations[i] );
                    if ( value != null ) return value;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a value (i.e., value type) from the aspect configuration.
        /// </summary>
        /// <typeparam name="TConfiguration">Type of the aspect configuration.</typeparam>
        /// <typeparam name="TValue">Type of the retrieved aspect.</typeparam>
        /// <param name="func">Function (delegate) retrieving the value from a configuration element.</param>
        /// <returns>The configuration value, or the default value of <typeparamref name="TValue"/> if no configuration element defined that property.</returns>
        protected TValue GetConfigurationValue<TConfiguration, TValue>(
            ConfigurationGetter<TConfiguration, TValue?> func )
            where TConfiguration : class, ILaosAspectConfiguration
            where TValue : struct
        {
            for (int i = 0; i < configurations.Count; i++)
            {
                TConfiguration config = configurations[i] as TConfiguration;
                if ( config != null )
                {
                    TValue? value = func( config );
                    if ( value.HasValue ) return value.Value;
                }
            }

            return default( TValue );
        }

        #endregion

        /// <summary>
        /// Determines whether the compile-time semantics of the current aspect
        /// require reflection wrappers.
        /// </summary>
        public virtual bool RequiresReflectionWrapper
        {
            get { return this.GetConfigurationValue<ILaosAspectConfiguration, bool>( c => c.RequiresReflectionWrapper ); }
        }


        internal AspectSerializationData SerializationData
        {
            get { return this.serializationData; }
            set { this.serializationData = value; }
        }

        /// <summary>
        /// Gets the field that contains, at runtime, the deserialized instance
        /// of the aspect.
        /// </summary>
        public FieldDefDeclaration AspectRuntimeInstanceField
        {
            get { return aspectRuntimeInstanceField; }
            internal set { this.aspectRuntimeInstanceField = value; }
        }

        /// <summary>
        /// Gets the <see cref="LaosTask"/> to which this weaver belong.
        /// </summary>
        public LaosTask Task { get; private set; }

        /// <summary>
        /// Gets the name of the current aspect, unique in the current module.
        /// </summary>
        /// <remarks>
        /// This name is useful to generate unique identifiers.
        /// </remarks>
        public string UniqueName { get; private set; }

        /// <summary>
        /// Gets the element to which the aspect is applied.
        /// </summary>
        public MetadataDeclaration TargetElement { get; private set; }

        /// <summary>
        /// Gets the compile-time instance of the aspect.
        /// </summary>
        /// <value>
        /// May be <b>null</b> if no runtime instance of the aspect is available,
        /// typically if the target assembly is linked against the Compact Framework
        /// or Silverlight. See <see cref="AspectSpecification"/> in this case.
        /// </value>
        public ILaosAspect Aspect
        {
            get { return this.AspectSpecification.Aspect; }
        }


        /// <summary>
        /// Gets the aspect priority.
        /// </summary>
        /// <remarks>
        /// Weavers are expected to set this property in their implementation of the
        /// <see cref="Initialize"/> method.
        /// </remarks>
        public virtual int AspectPriority
        {
            get { return aspectPriority; }
        }

        /// <summary>
        /// Gets the complete specification of the aspect.
        /// </summary>
        public AspectSpecification AspectSpecification { get; private set; }

        /// <summary>
        /// Gets the name of the aspect type.
        /// </summary>
        /// <returns>Full name of the type of the aspect.</returns>
        public string GetAspectTypeName()
        {
            if ( this.AspectSpecification.Aspect != null )
                return this.Aspect.GetType().AssemblyQualifiedName;
            else
            {
                return this.AspectSpecification.AspectConstruction.TypeName;
            }
        }

        /// <summary>
        /// Gets the aspect type.
        /// </summary>
        /// <returns>The aspect type.</returns>
        public ITypeSignature GetAspectType()
        {
            if ( this.AspectSpecification.Aspect != null )
                return this.Task.Project.Module.FindType( this.AspectSpecification.Aspect.GetType(),
                                                          BindingOptions.Default );
            else
                return this.Task.Project.Module.FindType( this.AspectSpecification.AspectConstruction.TypeName,
                                                          BindingOptions.Default );
        }

        /// <summary>
        /// Gets the target to which the <see cref="Implement"/> method redirected
        /// the implementation of the semantics.
        /// </summary>
        /// <value>
        /// The redirected target, or <b>null</b> if the target was unchanged.
        /// </value>
        /// <seealso cref="OnTargetAssigned"/>
        /// <see cref="SetOwnTargetRedirection"/>
        public MetadataDeclaration TargetRedirection { get; private set; }

        /// <summary>
        /// When implemented by a derived class, initializes fields that
        /// depend on the target to which this weaver is assigned.
        /// </summary>
        /// <param name="reassigned"><b>true</b> if the target is reassigned,
        /// i.e. it has been already assigned once. <b>false</b> if it is
        /// the first assignment.</param>
        /// <remarks>
        /// <para>
        /// Other weavers active on the same target, and before the current one
        /// in priority, may move the implementation of the original target
        /// (typically a method) into a new target. Such weavers invoke the
        /// method <see cref="SetOwnTargetRedirection"/>, which finally results
        /// in the method <see cref="OnTargetAssigned"/> to be invoked a second time
        /// for each weaver lower in the list of priority. 
        /// </para>
        /// <para>
        /// This method is invoked maximally twice.
        /// </para>
        /// </remarks>
        protected internal virtual void OnTargetAssigned( bool reassigned )
        {
        }

        /// <summary>
        /// Notify the weaving engine that the current weaver has moved the implementation
        /// of the original target (typically a method) into a new target.
        /// </summary>
        /// <param name="redirectedTarget">New target.</param>
        /// <remarks>
        /// <para>This method should be invoked from the <see cref="Implement"/> method.</para>
        /// <para>The effect of calling this method is to reset the target of other weavers
        /// on the same targets that are lower in the list of priority.</para>
        /// </remarks>
        /// <seealso cref="OnTargetAssigned"/>
        protected void SetOwnTargetRedirection( MetadataDeclaration redirectedTarget )
        {
            this.TargetRedirection = redirectedTarget;
        }

        internal void SetExternallyRedirectedTarget( MetadataDeclaration redirectedTarget )
        {
            this.TargetElement = redirectedTarget;
            this.OnTargetAssigned( true );
        }


        /// <summary>
        /// Validates the aspect use in itself, without regard to other aspects.
        /// </summary>
        /// <returns><b>true</b> if the aspect is used correctly, 
        /// otherwise <b>false</b>.</returns>
        /// <remarks>
        /// Implementations should write messages using a<see cref="MessageSource"/>,
        /// then return <b>true</b>. If <b>false</b> is returned, the aspect is
        /// not further processed (for instance, the <see cref="ILaosReflectionAspectProvider"/>
        /// interface is not used).
        /// </remarks>
        public virtual bool ValidateSelf()
        {
            return true;   
        }


        /// <summary>
        /// Validates whether the aspect use with regard to the declaring type (can the 
        /// aspect be applied on a field or a method of this type?). The default behavior
        /// is to require that this type itself is not a Laos aspect.
        /// </summary>
        /// <param name="type">The declaring type.</param>
        /// <returns><b>true</b> if the aspect is used correctly, 
        /// otherwise <b>false</b>.</returns>
        protected virtual bool ValidateDeclaringType( IType type )
        {
            if (
                type.IsAssignableTo( this.Task.Project.Module.
                                         GetTypeForFrameworkVariant(
                                         typeof(ILaosAspect) ),
                                     type.GetGenericContext(
                                         GenericContextOptions.None ) ) )
            {
                INamedType namedType = type as INamedType;
                string typeName = namedType != null ? namedType.Name : type.ToString();

                LaosMessageSource.Instance.Write(
                    SeverityType.Error, "LA0009",
                    new object[] {typeName} );

                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the aspect use with regards to other aspect uses.
        /// </summary>
        /// <param name="aspectsOnSameTarget">Array of all aspects present
        /// on the same declaration.</param>
        /// <remarks>
        /// Implementations should write messages using a<see cref="MessageSource"/>.
        /// </remarks>
        public virtual void ValidateInteractions( LaosAspectWeaver[] aspectsOnSameTarget )
        {
        }

        /// <summary>
        /// Emits instructions that initialize the aspect at runtime. These instructions
        /// will be injected in the static constructor of the PostSharp Laos implementation
        /// details object, <i>after</i> the field containing the runtime instance
        /// of the instance (<see cref="AspectRuntimeInstanceField"/>) has been 
        /// initialized.
        /// </summary>
        /// <param name="writer">The <see cref="InstructionWriter"/> into which
        /// instructions have to be written.</param>
        /// <remarks>
        /// It is expected that implementations generate only simple streams of instructions,
        /// without branching instructions. If more complexity is required, they should
        /// generate auxiliary methods.
        /// </remarks>
        public virtual void EmitRuntimeInitialization( InstructionEmitter writer )
        {
        }

        /// <summary>
        /// Emits instructions that build the custom attribute from <see cref="AspectSpecification"/>.
        /// </summary>
        /// <param name="writer">The <see cref="InstructionEmitter"/> with which
        /// instructions have to be written.</param>
        /// <remarks>
        /// This method is invoked by the Laos framework (<see cref="LaosTask"/>).
        /// </remarks>
        public virtual void EmitAspectConstruction( InstructionEmitter writer )
        {
            if ( this.serializationData == null )
            {
                IObjectConstruction objectConstruction = this.AspectSpecification.AspectConstruction;
                IAnnotationValue annotationValue = objectConstruction as IAnnotationValue;
                MethodDefDeclaration constructor = null;

                if ( annotationValue == null )
                {
                    // We have to wrap an IObjectConstruction into a CustomAttributeValue.

                    ModuleDeclaration module = this.Task.Project.Module;

                    // Find a constructor matching the constructor arguments.
                    TypeDefDeclaration typeDef =
                        module.Domain.FindTypeDefinition( this.AspectSpecification.AspectConstruction.TypeName );
                    foreach ( MethodDefDeclaration methodDef in typeDef.Methods.GetByName( ".ctor" ) )
                    {
                        if ( methodDef.Parameters.Count != objectConstruction.ConstructorArgumentCount )
                            continue;

                        bool match = true;

                        for ( int i = 0; i < methodDef.Parameters.Count; i++ )
                        {
                            object argumentValue = objectConstruction.GetConstructorArgument( i );
                            ITypeSignature parameterType =
                                methodDef.Parameters[i].ParameterType.GetNakedType( TypeNakingOptions.None );

                            // Does this parameter accept a null value?
                            if ( argumentValue == null )
                            {
                                if ( !parameterType.BelongsToClassification( TypeClassifications.ReferenceType ) )
                                {
                                    match = false;
                                    break;
                                }
                            }
                            else
                            {
                                // Check types.
                                ITypeSignature argumentType = module.FindType( argumentValue.GetType(),
                                                                               BindingOptions.Default );
                                if ( ! argumentType.IsAssignableTo( parameterType, GenericMap.Empty ) )
                                {
                                    match = false;
                                    break;
                                }
                            }
                        }

                        if ( !match )
                            continue;

                        constructor = methodDef;
                    }

                    // Fail if we did not find a constructor.
                    if ( constructor == null )
                    {
                        LaosMessageSource.Instance.Write(
                            SeverityType.Error, "LA0027",
                            new object[] {objectConstruction.TypeName} );
                        return;
                    }


                    // We can now construct our CustomAttributeValue.
                    annotationValue = new AnnotationValue( constructor.Translate( module ) );
                    for ( int i = 0; i < objectConstruction.ConstructorArgumentCount; i++ )
                    {
                        annotationValue.ConstructorArguments.Add(
                            new MemberValuePair(
                                MemberKind.Parameter,
                                i,
                                constructor.Parameters[i].Name,
                                new SerializedValue(
                                    SerializationType.GetSerializationType( constructor.Parameters[i].ParameterType ),
                                    objectConstruction.GetConstructorArgument( i ) ) ) );
                    }

                    string[] properties = objectConstruction.GetPropertyNames();
                    for ( int i = 0; i < properties.Length; i++ )
                    {
                        MemberKind memberKind;
                        ITypeSignature type;

                        PropertyDeclaration property = constructor.DeclaringType.FindProperty( properties[i] );
                        if ( property != null )
                        {
                            memberKind = MemberKind.Property;
                            type = property.PropertyType.Translate( module );
                        }
                        else
                        {
                            FieldDefDeclaration fieldDef = constructor.DeclaringType.FindField( properties[i] );

                            if ( fieldDef == null )
                            {
                                LaosMessageSource.Instance.Write( SeverityType.Error,
                                                                  "LA0028", new object[]
                                                                                {
                                                                                    constructor.DeclaringType.ToString()
                                                                                    ,
                                                                                    properties[i]
                                                                                } );
                                return;
                            }

                            type = fieldDef.FieldType.Translate( module );
                            memberKind = MemberKind.Field;
                        }


                        annotationValue.NamedArguments.Add(
                            new MemberValuePair(
                                memberKind,
                                i,
                                properties[i],
                                new SerializedValue(
                                    SerializationType.GetSerializationType( type ),
                                    objectConstruction.GetPropertyValue( properties[i] ) ) ) );
                    }
                }

                this.Task.WeavingHelper.EmitCustomAttributeConstruction( annotationValue, writer );
                writer.EmitInstructionField( OpCodeNumber.Stsfld, this.aspectRuntimeInstanceField );
            }
        }

        /// <summary>
        /// Initialize the current weaver and its aspect.
        /// </summary>
        /// <remarks>
        /// <para>Implementations should always call the base method.</para>
        /// <para><b>Warning.</b> Implementations should not initialize in this methods
        /// values that depend on the target declaration. Indeed, the target declaration
        /// may be reassigned after the <see cref="Initialize"/> method is called.
        /// Use the <see cref="OnTargetAssigned"/> method for this purpose.</para>
        /// </remarks>
        public virtual void Initialize()
        {
            this.LoadAspectConfiguration();

            if (  this.configurations.Count > 0 && this.configurations[0] is ILaosWeavableAspectConfiguration )
            {
                this.aspectPriority =
                    this.GetConfigurationValue<ILaosWeavableAspectConfiguration, int>( c => c.AspectPriority );
            }
        }

        /// <summary>
        /// Creates the instance tag in a type definition according to the configuration of
        /// the current aspect.
        /// </summary>
        /// <param name="typeDef">Type in which the instance tag has to be created.</param>
        /// <param name="forceStatic"><b>true</b> if only static tags are supported.</param>
        protected void InitializeInstanceTag( TypeDefDeclaration typeDef, bool forceStatic )
        {
            if (this.configurations.Count > 0 && this.configurations[0] is IInstanceBoundLaosAspectConfiguration)
            {
                InstanceTagRequest instanceTagRequest =
                    this.GetConfigurationObject<IInstanceBoundLaosAspectConfiguration, InstanceTagRequest>(
                        c => c.GetInstanceTagRequest() );

                if ( instanceTagRequest != null )
                {
                    this.InstanceTagField = this.Task.InstanceTagManager.GetInstanceTagField( typeDef,
                                                                                              instanceTagRequest,
                                                                                              forceStatic );
                }
            }
        }

        /// <summary>
        /// Gets the field containing the instance tag.
        /// </summary>
        protected FieldDefDeclaration InstanceTagField { get; private set; }

        /// <summary>
        /// Initializes the aspect instance, typically by invoking some <b>CompileTimeInitialize</b> method.
        /// </summary>
        public virtual void InitializeAspect()
        {
        }

        /// <summary>
        /// Implement the current aspect.
        /// </summary>
        /// <remarks>
        /// If implementations need to register an advice for the low-level code
        /// weaver, they can add them in the <see cref="LaosTask.MethodLevelAdvices"/>
        /// and <see cref="LaosTask.TypeLevelAdvices"/> collections of the
        /// parent <see cref="LaosTask"/>.
        /// </remarks>
        public abstract void Implement();

        /// <summary>
        /// Allows aspect weavers to provide other aspects.
        /// </summary>
        /// <param name="collection">Collection where new aspects should be added.</param>
        public virtual void ProvideAspects( LaosReflectionAspectCollection collection )
        {
            ILaosReflectionAspectProvider aspectProvider =
                this.AspectSpecification.Aspect as ILaosReflectionAspectProvider;
            if ( aspectProvider != null )
                aspectProvider.ProvideAspects( collection );
        }

        /// <summary>
        /// Determines whether the current aspect requires the <see cref="Implement"/> method to be invoked.
        /// </summary>
        public virtual bool RequiresImplementation
        {
            get { return true; }
        }

        /// <summary>
        /// Determines whether the current aspect requries a runtime instance.
        /// </summary>
        /// <remarks>
        /// If this property returns <b>true</b>, a static field will be created to store the
        /// aspect at runtime, and the method <see cref="EmitRuntimeInitialization"/>
        /// will be invoked, so that the weaver can emit code that will initialize the aspect
        /// at runtime.
        /// </remarks>
        public virtual bool RequiresRuntimeInstance
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the type of <see cref="LaosSerializer"/> used to serialize the current aspect.
        /// </summary>
        /// <returns>A <see cref="Type"/> derived from <see cref="LaosSerializer"/></returns>
        /// <remarks>
        /// The default implementation always returns <see cref="BinaryLaosSerializer"/>.
        /// </remarks>
        public virtual Type GetSerializerType()
        {
            if (this.Task.FrameworkVariant != FrameworkVariants.Full)
                return typeof(MsilLaosSerializer);

            Type serializerType =
                this.GetConfigurationObject<LaosWeavableAspectConfigurationAttribute, Type>(
                    configuration => configuration.SerializerType );

            return serializerType ?? typeof(BinaryLaosSerializer);
        }
    }

    /// <summary>
    /// Delegate definition for methods that retrieve the value of some property
    /// of a <see cref="ILaosAspectConfiguration"/>.
    /// </summary>
    /// <typeparam name="TConfiguration">Type of aspect configuration.</typeparam>
    /// <typeparam name="TValue">Type of the property value.</typeparam>
    /// <param name="configuration">Aspect configuration.</param>
    /// <returns>Typically an aspect configuration property.</returns>
    public delegate TValue ConfigurationGetter<TConfiguration, TValue>( TConfiguration configuration )
        where TConfiguration : ILaosAspectConfiguration;
}