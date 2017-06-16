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
using System.Globalization;
using System.IO;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.Extensibility.Tasks;
using PostSharp.Laos.Serializers;
using PostSharp.PlatformAbstraction;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Principal PostSharp Task processing Laos aspects.
    /// </summary>
// ReSharper disable ClassNeverInstantiated.Global
    public sealed class LaosTask : Task, IAdviceProvider
// ReSharper restore ClassNeverInstantiated.Global
    {
        private readonly Set<string> discoveredAwarenesses = new Set<string>(4, StringComparer.InvariantCultureIgnoreCase);
        private readonly Set<MetadataDeclaration> declarationsScannedForAwareness = new Set<MetadataDeclaration>();
        private TypeDefDeclaration implementationDetailsType;
        private FieldDefDeclaration implementationInitializedField;
        private readonly List<IMethodLevelAdvice> methodLevelAdvices = new List<IMethodLevelAdvice>();
        private readonly List<ITypeLevelAdvice> typeLevelAdvices = new List<ITypeLevelAdvice>();
        private readonly List<IFieldLevelAdvice> fieldLevelAdvices = new List<IFieldLevelAdvice>();
        private InstructionWriter instructionWriter;
        private WeavingHelper weavingHelper;
        private readonly TagCollection tags = new TagCollection();
        private EventArgsBuilders eventArgsBuilders;

        private readonly Queue<AspectTargetPair> aspects =
            new Queue<AspectTargetPair>();

        private readonly MultiDictionary<MetadataDeclaration, LaosAspectWeaver> aspectWeavers =
            new MultiDictionary<MetadataDeclaration, LaosAspectWeaver>();

        private InstanceTagManager instanceTagProvider;
        private readonly List<ILaosAspectWeaverFactory> aspectWeaverFactories = new List<ILaosAspectWeaverFactory>();
        private InstanceCredentialsManager instanceCredentialsManager;

        private readonly MultiDictionary<TypeDefDeclaration, LaosAspectWeaver> weaversOnTypes =
            new MultiDictionary<TypeDefDeclaration, LaosAspectWeaver>();

        private string frameworkVariant;

        private DelegateMapper delegateMapper;
        private InstanceInitializationManager instanceInitializationManager;
        private IType enableLaosAwarenessAttributeType;


        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            this.eventArgsBuilders = new EventArgsBuilders( this );
            this.instanceCredentialsManager = new InstanceCredentialsManager( this );
            this.instanceTagProvider = new InstanceTagManager( this );
            this.instanceInitializationManager = new InstanceInitializationManager(this);
        }

        /// <summary>
        /// Gets the <see cref="InstanceCredentialsManager"/>.
        /// </summary>
        public InstanceCredentialsManager InstanceCredentialsManager
        {
            get { return this.instanceCredentialsManager; }
        }

        /// <summary>
        /// Gets a collection of tags that weaver can use to share state.
        /// </summary>
        public TagCollection Tags
        {
            get { return this.tags; }
        }

        /// <summary>
        /// Gets a collection that can be used to add method-level advices
        /// of the <see cref="PostSharp.CodeWeaver"/> namespace (i.e. advices
        /// of the low-level weaver).
        /// </summary>
        public IList<IMethodLevelAdvice> MethodLevelAdvices
        {
            get { return this.methodLevelAdvices; }
        }

        /// <summary>
        /// Gets a collection that can be used to add method-level advices
        /// of the <see cref="PostSharp.CodeWeaver"/> namespace (i.e. advices
        /// of the low-level weaver).
        /// </summary>
        public IList<IFieldLevelAdvice> FieldLevelAdvices
        {
            get { return this.fieldLevelAdvices; }
        }

        /// <summary>
        /// Gets a collection that can be used to add type-level advices
        /// of the <see cref="PostSharp.CodeWeaver"/> namespace (i.e. advices
        /// of the low-level weaver).
        /// </summary>
        public IList<ITypeLevelAdvice> TypeLevelAdvices
        {
            get { return this.typeLevelAdvices; }
        }

        /// <summary>
        /// Gets the <see cref="DelegateMapper"/>, which provide mappings between
        /// arbitrary method signatures and ad hoc delegates.
        /// </summary>
        public DelegateMapper DelegateMapper
        {
            get { return this.delegateMapper; }
        }

        /// <summary>
        /// Gets a collection that can be used to add type-level advices
        /// of the <see cref="PostSharp.CodeWeaver"/> namespace (i.e. advices
        /// of the low-level weaver).
        /// </summary>
        public WeavingHelper WeavingHelper
        {
            get { return this.weavingHelper; }
        }

        /// <summary>
        /// Gets the <see cref="Laos.Weaver.InstanceInitializationManager"/>.
        /// </summary>
        public InstanceInitializationManager InstanceInitializationManager { get { return instanceInitializationManager;}}

        /// <summary>
        /// Gets the type where Laos implementation details (like deserialized aspects)
        /// are stored.
        /// </summary>
        public TypeDefDeclaration ImplementationDetailsType
        {
            get { return this.implementationDetailsType; }
        }

        /// <summary>
        /// Gets an <see cref="PostSharp.CodeModel.InstructionWriter"/> that weavers
        /// can use when writing third methods. Callers are responsible to release
        /// the instruction writer after use.
        /// </summary>
        public InstructionWriter InstructionWriter
        {
            get { return this.instructionWriter; }
        }

        /// <summary>
        /// Gets the <see cref="PostSharp.Laos.Weaver.EventArgsBuilders"/> helper
        /// for the current task.
        /// </summary>
        public EventArgsBuilders EventArgsBuilders
        {
            get { return this.eventArgsBuilders; }
        }

        /// <summary>
        /// Gets the <see cref="PostSharp.Laos.Weaver.InstanceTagManager"/> helper
        /// for the current task.
        /// </summary>
        public InstanceTagManager InstanceTagManager
        {
            get { return instanceTagProvider; }
        }

        /// <summary>
        /// Gets the current framework variant (<see cref="ModuleDeclaration.GetFrameworkVariant"/>).
        /// </summary>
        public string FrameworkVariant
        {
            get { return frameworkVariant; }
        }

        void DiscoverAwarenessesRecursive(IType type)
        {
            TypeDefDeclaration typeDef = type.GetTypeDefinition();
            this.DiscoverAwarenesses( typeDef );
            if ( typeDef.BaseType != null )
            {
                this.DiscoverAwarenessesRecursive( typeDef.BaseType );   
            }
        }

        void DiscoverAwarenesses(MetadataDeclaration metadataDeclaration)
        {
            if ( !this.declarationsScannedForAwareness.AddIfAbsent( metadataDeclaration ))
                return;

            IEnumerator<CustomAttributeDeclaration> customAttributes = 
                metadataDeclaration.CustomAttributes.GetByTypeEnumerator(this.enableLaosAwarenessAttributeType);

            while ( customAttributes.MoveNext() )
            {
                string plugInName = (string) customAttributes.Current.ConstructorArguments[0].Value.GetRuntimeValue();
                string taskName = (string) customAttributes.Current.ConstructorArguments[1].Value.GetRuntimeValue();
                string key = plugInName + ":" + taskName;

                if ( this.discoveredAwarenesses.AddIfAbsent( key ))
                {
                    if ( !this.Project.AddPlugIn( plugInName ) )
                    {
                        LaosMessageSource.Instance.Write(SeverityType.Error, "LA0046", new object[] { plugInName, taskName} );
                    }


                    if ( this.Project.Tasks[taskName] == null )
                    {
                        this.Project.Tasks.Add( taskName );
                    }
                }

            }
        }

        #region IAdviceProvider Members

        void IAdviceProvider.ProvideAdvices( CodeWeaver.Weaver codeWeaver )
        {
            // Provide type initializers.
            codeWeaver.AddTypeLevelAdvice(
                new AspectInitializeAdvice( this ),
                JoinPointKinds.BeforeStaticConstructor,
                this.weaversOnTypes.Keys );

            // Provide custom advices.
            foreach ( IMethodLevelAdvice advice in this.methodLevelAdvices )
            {
                IEnumerable<MethodDefDeclaration> methods = advice.Method == null
                                                                ? null
                                                                :
                                                                    new Singleton<MethodDefDeclaration>( advice.Method );

                IEnumerable<MetadataDeclaration> operands = advice.Operand == null
                                                                ? null
                                                                :
                                                                    new Singleton<MetadataDeclaration>( advice.Operand );

                codeWeaver.AddMethodLevelAdvice( advice, methods, advice.JoinPointKinds, operands );
            }

            foreach ( ITypeLevelAdvice advice in this.typeLevelAdvices )
            {
                codeWeaver.AddTypeLevelAdvice(
                    advice, advice.JoinPointKinds, new Singleton<TypeDefDeclaration>( advice.Type ) );
            }

            foreach ( IFieldLevelAdvice advice in this.fieldLevelAdvices )
            {
                codeWeaver.AddFieldLevelAdvice(
                    advice, advice.JoinPointKinds, new Singleton<IField>( advice.Field ) );

                if ( advice.ChangeToProperty )
                {
                    codeWeaver.GeneratePropertyAroundField( (FieldDefDeclaration) advice.Field );
                }
            }

            // Provide the GetInstanceCredentials advice
            IMethod getCredentialsMethod =
                this.Project.Module.FindMethod(
                    this.Project.Module.GetTypeForFrameworkVariant( typeof(LaosUtils) ),
                    "GetCurrentInstanceCredentials");

            if ( getCredentialsMethod != null )
            {
                codeWeaver.AddMethodLevelAdvice( new GetInstanceCredentialAdvice( this ),
                                                 null, JoinPointKinds.InsteadOfCall,
                                                 new Singleton<MetadataDeclaration>(
                                                     (MetadataDeclaration) getCredentialsMethod )
                    );
            }

            // Provide the InitializeCurrentAspects advice.
            IMethod initializeCurrentAspectsMethod =
                this.Project.Module.FindMethod(
                    this.Project.Module.GetTypeForFrameworkVariant(typeof(LaosUtils)),
                    "InitializeCurrentAspects");

            if (initializeCurrentAspectsMethod != null)
            {
                codeWeaver.AddMethodLevelAdvice(new InitializeCurrentAspectsAdvice(this),
                                                 null, JoinPointKinds.InsteadOfCall,
                                                 new Singleton<MetadataDeclaration>(
                                                     (MetadataDeclaration)initializeCurrentAspectsMethod)
                    );
            }
        }

        #endregion

        private void EnqueueAspects( ILaosReflectionAspectProvider provider )
        {
            ModuleDeclaration module = this.Project.Module;

            LaosReflectionAspectCollection newAspects = new LaosReflectionAspectCollection();
            provider.ProvideAspects( newAspects );
            foreach ( KeyValuePair<object, AspectSpecification> aspect in newAspects )
            {
                MetadataDeclaration target;
                MethodBase targetMethod;
                FieldInfo targetField;
                Type targetType;
                PropertyInfo targetProperty;
                EventInfo targetEvent;
                ParameterInfo targetParameter;

                if ( ( targetMethod = aspect.Key as MethodBase ) != null )
                {
                    target =
                        module.FindMethod( targetMethod, BindingOptions.RequireGenericDefinition ).
                            GetMethodDefinition();
                }
                else if ( ( targetField = aspect.Key as FieldInfo ) != null )
                {
                    target =
                        module.FindField( targetField, BindingOptions.RequireGenericDefinition ).
                            GetFieldDefinition();
                }
                else if ( ( targetType = aspect.Key as Type ) != null )
                {
                    target =
                        module.FindType( targetType, BindingOptions.RequireGenericDefinition ).
                            GetTypeDefinition();
                }
                else if ( ( targetProperty = aspect.Key as PropertyInfo ) != null )
                {
                    TypeDefDeclaration type =
                        module.FindType( targetProperty.DeclaringType,
                                         BindingOptions.RequireGenericDefinition | BindingOptions.OnlyDefinition |
                                         BindingOptions.DontThrowException )
                        as TypeDefDeclaration;

                    if ( type == null )
                    {
                        LaosMessageSource.Instance.Write( SeverityType.Error,
                                                          "LA0031", new object[]
                                                                        {
                                                                            targetProperty.DeclaringType.FullName,
                                                                            aspect.Value.AspectTypeName
                                                                        } );
                        continue;
                    }
                    else
                    {
                        PropertyDeclaration property = type.Properties.GetProperty( targetProperty.Name,
                                                                                    module.GetMethodSignature(
                                                                                        targetProperty.GetGetMethod(
                                                                                            true ),
                                                                                        BindingOptions.Default ) );
                        if ( property == null )
                        {
                            throw LaosExceptionHelper.Instance.CreateBindingException( "CannotFindEventOrProperty",
                                                                                       targetProperty.Name, type );
                        }

                        target = property;
                    }
                }
                else if ( ( targetEvent = aspect.Key as EventInfo ) != null )
                {
                    TypeDefDeclaration type =
                        module.FindType( targetEvent.DeclaringType,
                                         BindingOptions.RequireGenericDefinition | BindingOptions.OnlyDefinition |
                                         BindingOptions.DontThrowException )
                        as TypeDefDeclaration;

                    if ( type == null )
                    {
                        LaosMessageSource.Instance.Write( SeverityType.Error,
                                                          "LA0031", new object[]
                                                                        {
                                                                            targetEvent.DeclaringType.FullName,
                                                                            aspect.Value.AspectTypeName
                                                                        } );

                        continue;
                    }
                    else
                    {
                        EventDeclaration @event = type.Events.GetByName( targetEvent.Name );
                        if ( @event == null )
                        {
                            throw LaosExceptionHelper.Instance.CreateBindingException( "CannotFindEventOrProperty",
                                                                                       targetEvent.Name, type );
                        }

                        target = @event;
                    }
                }
                else if ( ( targetParameter = aspect.Key as ParameterInfo ) != null )
                {
                    MethodBase declaringMethodBase = targetParameter.Member as MethodBase;

                    if ( declaringMethodBase == null )
                    {
                        LaosMessageSource.Instance.Write( SeverityType.Error, "LA0032",
                                                          new object[]
                                                              {
                                                                  aspect.Value.AspectTypeName,
                                                                  targetParameter.Member.DeclaringType.FullName,
                                                                  targetParameter.Member.Name
                                                              } );
                        continue;
                    }
                    MethodDefDeclaration method = module.FindMethod( declaringMethodBase,
                                                                     BindingOptions.RequireGenericDefinition |
                                                                     BindingOptions.OnlyDefinition |
                                                                     BindingOptions.DontThrowException )
                                                  as MethodDefDeclaration;

                    if ( method == null )
                    {
                        LaosMessageSource.Instance.Write( SeverityType.Error, "LA0033",
                                                          new object[]
                                                              {
                                                                  aspect.Value.AspectTypeName,
                                                                  targetParameter.Member.DeclaringType.FullName,
                                                                  targetParameter.Member.Name
                                                              } );
                        continue;
                    }

                    if ( targetParameter.IsRetval || targetParameter.Position < 0 )
                    {
                        target = method.ReturnParameter;
                    }
                    else
                    {
                        target = method.Parameters[targetParameter.Position];
                    }

                    if ( target == null )
                        throw new AssertionFailedException(
                            string.Format( "Cannot find the parameter {0} in method {1}.",
                                           targetParameter.Position, method ) );
                }
                else
                {
                    LaosMessageSource.Instance.Write( SeverityType.Error, "LA0034",
                                                      new object[] {aspect.Key.GetType()} );
                    continue;
                }

                this.aspects.Enqueue( new AspectTargetPair( target, aspect.Value ) );
            }
        }

        private LaosAspectWeaver FindWeaver( AspectTargetPair aspectTargetPair )
        {
            foreach ( ILaosAspectWeaverFactory factory in this.aspectWeaverFactories )
            {
                LaosAspectWeaver weaver = factory.CreateAspectWeaver( aspectTargetPair );

                if ( weaver != null )
                    return weaver;
            }

            LaosMessageSource.Instance.Write( SeverityType.Error, "LA0003",
                                              new object[] {aspectTargetPair.AspectSpecification.AspectTypeName} );

            return null;
        }

        /// <inheritdoc />
        public override bool Execute()
        {
            ModuleDeclaration module = this.Project.Module;

            // Determine whether we run the compact framework.
            this.frameworkVariant = module.GetFrameworkVariant();

            if ( this.frameworkVariant != FrameworkVariants.Full )
            {
                LaosTrace.Trace.WriteLine( "Compact Framework / Silverlight detected." );
            }

            this.enableLaosAwarenessAttributeType = (IType) module.GetTypeForFrameworkVariant( typeof(EnableLaosAwarenessAttribute) );

            // Discover awarenesses on the assembly manifest.
            this.DiscoverAwarenesses( module.AssemblyManifest );

            // Get the weaver factories.
            IEnumerator<ILaosAspectWeaverFactory> factoryEnumerator =
                this.Project.Tasks.GetInterfaces<ILaosAspectWeaverFactory>();
            while ( factoryEnumerator.MoveNext() )
            {
                LaosTrace.Trace.WriteLine( "Adding weaver factory {{{0}}}.",
                                           factoryEnumerator.Current.GetType().FullName );
                this.aspectWeaverFactories.Add( factoryEnumerator.Current );
            }

            // Process custom attributes.
            AnnotationRepositoryTask annotationRepository =
                AnnotationRepositoryTask.GetTask( this.Project );

            TypeDefDeclaration laosAspectTypeDef =
                module.GetTypeForFrameworkVariant( typeof(ILaosAspect) ).GetTypeDefinition();

            IEnumerator<IAnnotationInstance> customAttributeEnumerator =
                annotationRepository.GetAnnotationsOfType( laosAspectTypeDef, true );

            if ( !customAttributeEnumerator.MoveNext() )
                return true;


            customAttributeEnumerator.Dispose();
            customAttributeEnumerator =
                annotationRepository.GetAnnotationsOfType( laosAspectTypeDef, true );


            // Go on.
            using ( this.instructionWriter = new InstructionWriter() )
            using ( InstructionWriter constructorInstructionWriter = new InstructionWriter() )
            {
#if DEBUG
                this.instructionWriter.CheckEnabled = true;
                constructorInstructionWriter.CheckEnabled = true;
#endif

                this.weavingHelper = new WeavingHelper( this.Project.Module );

                // Create the implementation type.
                string implementationDetailsTypeName;
                int i = 0;
                do
                {
                    i++;
                    implementationDetailsTypeName = string.Format( "<>AspectsImplementationDetails_{0}", i );
                } while ( this.Project.Module.Types.GetByName( implementationDetailsTypeName ) != null );

                this.implementationDetailsType = new TypeDefDeclaration
                                                     {
                                                         Name =
                                                             Platform.Current.NormalizeCilIdentifier(
                                                             implementationDetailsTypeName),
                                                         Attributes =
                                                             ( TypeAttributes.NotPublic | TypeAttributes.Sealed ),
                                                         BaseType =
                                                             ( (IType)
                                                               this.Project.Module.Cache.GetType(
                                                                   "System.Object, mscorlib" ) )
                                                     };
                this.Project.Module.Types.Add( this.implementationDetailsType );
                this.weavingHelper.AddCompilerGeneratedAttribute( this.implementationDetailsType.CustomAttributes );
                ReflectionWrapperUtil.HideTypeFromAssembly( this.implementationDetailsType );

                // Create the delegate mapper.
                this.delegateMapper = new DelegateMapper( this.implementationDetailsType );

                // Create the field "implementation is initialized".
                this.implementationInitializedField = new FieldDefDeclaration
                                                          {
                                                              Name = "initialized",
                                                              FieldType =
                                                                  module.Cache.GetIntrinsic( IntrinsicType.Boolean ),
                                                              Attributes =
                                                                  ( FieldAttributes.Assembly | FieldAttributes.Static )
                                                          };
                this.implementationDetailsType.Fields.Add( this.implementationInitializedField );

                // Create the static constructor
                MethodDefDeclaration implementationConstructor = new MethodDefDeclaration {Name = ".cctor"};
                this.implementationDetailsType.Methods.Add( implementationConstructor );
                implementationConstructor.Attributes = MethodAttributes.Private | MethodAttributes.Static |
                                                       MethodAttributes.RTSpecialName | MethodAttributes.SpecialName |
                                                       MethodAttributes.HideBySig;
                implementationConstructor.ReturnParameter = new ParameterDeclaration
                                                                {
                                                                    Attributes = ParameterAttributes.Retval,
                                                                    ParameterType =
                                                                        module.Cache.GetIntrinsic( IntrinsicType.Void )
                                                                };

                MethodBodyDeclaration implementationConstructorBody = new MethodBodyDeclaration
                                                                          {InitLocalVariables = true};
                implementationConstructor.MethodBody = implementationConstructorBody;
                InstructionBlock implementationConstructorRootBlock =
                    implementationConstructorBody.CreateInstructionBlock();
                implementationConstructorBody.RootInstructionBlock = implementationConstructorRootBlock;

                InstructionBlock catchBlock = implementationConstructorBody.CreateInstructionBlock();
                implementationConstructorRootBlock.AddChildBlock( catchBlock, NodePosition.After, null );
                InstructionSequence returnSequence = implementationConstructorBody.CreateInstructionSequence();
                InstructionBlock returnBlock = implementationConstructorBody.CreateInstructionBlock();
                implementationConstructorRootBlock.AddChildBlock( returnBlock, NodePosition.After, null );
                returnBlock.AddInstructionSequence( returnSequence, NodePosition.After, null );
                InstructionSequence implementationConstructorSequence =
                    implementationConstructorBody.CreateInstructionSequence();
                catchBlock.AddInstructionSequence( implementationConstructorSequence,
                                                   NodePosition.Before, null );


                constructorInstructionWriter.AttachInstructionSequence( implementationConstructorSequence );


#if DEBUG
                constructorInstructionWriter.CheckEnabled = true;
#endif

                #region Enqueue aspects coming from custom attributes

                while ( customAttributeEnumerator.MoveNext() )
                {
                    // Create an instance of the custom attribute.

                    IAnnotationInstance instance = customAttributeEnumerator.Current;

                    LaosTrace.Trace.WriteLine( "Processing attribute {{{0}}} on {{{1}}}.",
                                               instance.Value.Constructor.DeclaringType, instance.TargetElement );

                    // Check whether this aspect should be skipped. Sometimes we have to ignore a custom
                    // attribute if it is not applied on user code, but on compiler-generated code.
                    if ( CompilerWorkarounds.IsCompilerGenerated( instance.TargetElement ))
                        continue;

                    AspectSpecification aspectSpecification;
                    if ( module.Domain.ReflectionDisabled )
                    {
                        aspectSpecification = new AspectSpecification( customAttributeEnumerator.Current.Value,
                                                                       (ILaosAspectConfiguration) null );
                        
                    }
                    else
                    {
                        ILaosAspect aspect;

                        try
                        {
                            aspect = (ILaosAspect)
                                     CustomAttributeHelper.ConstructRuntimeObject( instance.Value, this.Project.Module );
                        }
                        catch ( CustomAttributeConstructorException e )
                        {
                            LaosMessageSource.Instance.Write( SeverityType.Error, "LA0014", new object[] {e.Message} );
                            continue;
                        }

                        aspectSpecification = new AspectSpecification( customAttributeEnumerator.Current.Value,
                                                                       aspect );
                    }

                    this.aspects.Enqueue(new AspectTargetPair(instance.TargetElement, aspectSpecification));
                }

                #endregion

                // Will store per-serializer data.
                Dictionary<Type, SerializerSensitiveData> serializerSensitiveDatas =
                    new Dictionary<Type, SerializerSensitiveData>();

                #region Validate aspects, get additional aspects, group aspects per serializers, and initializes serializers

                i = 0;
                while ( this.aspects.Count > 0 )
                {
                    i++;

                    AspectTargetPair aspectTargetPair = this.aspects.Dequeue();

                    LaosAspectWeaver aspectWeaver = this.FindWeaver( aspectTargetPair );
                    if ( aspectWeaver == null )
                        continue;

                    // Initialize the aspect weaver.
                    aspectWeaver.SetProperties(
                        this,
                        i.ToString( CultureInfo.InvariantCulture ),
                        aspectTargetPair
                        );
                    aspectWeaver.Initialize();
                    aspectWeaver.InitializeAspect();
                    aspectWeaver.OnTargetAssigned( false );


                    // Validate the attribute. Skip it if invalid.
                    if ( !aspectWeaver.ValidateSelf() )
                        continue;

                    // Enqueue more aspects if we have an aspect provider.
                    this.EnqueueAspects( aspectWeaver );

                    // Load awarenesses defined on the aspect.
                    this.DiscoverAwarenessesRecursive( (IType) aspectWeaver.GetAspectType() );


                    // Stop here is the aspect is not weavable.
                    if ( !aspectWeaver.RequiresImplementation )
                        continue;

                    // Add an aspect weaver.
                    this.aspectWeavers.Add( aspectWeaver.TargetElement, aspectWeaver );

                    // Determine which serializer takes care of the current aspect.


                    Type serializerType = aspectWeaver.GetSerializerType();

                    if ( serializerType == typeof(MsilLaosSerializer) || serializerType == null ) continue;

                    // Initialize serializer-sensitive stuff.
                    SerializerSensitiveData serializerSensitiveData;
                    if ( !serializerSensitiveDatas.TryGetValue( serializerType, out serializerSensitiveData ) )
                    {
                        serializerSensitiveData = new SerializerSensitiveData();
                        serializerSensitiveDatas.Add( serializerType, serializerSensitiveData );

                        if ( !typeof(LaosSerializer).IsAssignableFrom( serializerType ) )
                        {
                            // The type {0} cannot be used as an argument of the [LaosSerializer]
                            // custom attribute for aspect {1}, because it is not derived from class 
                            // PostSharp.Laos.LaosSerializer.
                            LaosMessageSource.Instance.Write( SeverityType.Error, "LA0021",
                                                              new object[]
                                                                  {
                                                                      serializerType,
                                                                      aspectTargetPair.AspectSpecification.GetType()
                                                                  } );
                        }

                        ConstructorInfo serializerConstructor = serializerType.GetConstructor( Type.EmptyTypes );
                        if ( serializerConstructor == null )
                        {
                            // No public parameterless constructor.
                            LaosMessageSource.Instance.Write( SeverityType.Error, "LA0022",
                                                              new object[]
                                                                  {
                                                                      serializerType,
                                                                      aspectTargetPair.AspectSpecification.GetType()
                                                                  } );
                        }
                        else
                        {
                            serializerSensitiveData.Serializer = (LaosSerializer) serializerConstructor.Invoke( null );
                        }


                        // Emit IL instructions to initialize this serializer.

                        serializerSensitiveData.CustomAttributeArrayLocal =
                            implementationConstructorRootBlock.DefineLocalVariable(
                                new ArrayTypeSignature( module.Cache.GetIntrinsic( IntrinsicType.Object ) ),
                                string.Format( "customAttributeArray~{0}", serializerSensitiveDatas.Count - 1 ) );
                        serializerSensitiveData.ResourceName = serializerType.FullName + ".bin";

                        constructorInstructionWriter.EmitInstructionMethod( OpCodeNumber.Newobj,
                                                                            this.Project.Module.FindMethod(
                                                                                serializerConstructor,
                                                                                BindingOptions.RequireGenericDefinition ) );
                        this.weavingHelper.GetRuntimeType( implementationDetailsType, constructorInstructionWriter );
                        constructorInstructionWriter.EmitInstructionMethod( OpCodeNumber.Callvirt,
                                                                            this.Project.Module.FindMethod(
                                                                                "System.Type, mscorlib",
                                                                                "get_Assembly" ) );
                        constructorInstructionWriter.EmitInstructionString( OpCodeNumber.Ldstr,
                                                                            serializerSensitiveData.ResourceName );
                        constructorInstructionWriter.EmitInstructionMethod( OpCodeNumber.Call,
                                                                            this.Project.Module.FindMethod(
                                                                                module.GetTypeForFrameworkVariant(
                                                                                    typeof(LaosSerializer) ),
                                                                                "Deserialize", 2 ) );
                        constructorInstructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Stloc,
                                                                                   serializerSensitiveData.
                                                                                       CustomAttributeArrayLocal );
                    }

                    aspectWeaver.SerializationData =
                        new AspectSerializationData( serializerSensitiveData.AspectInstances.Count,
                                                     serializerSensitiveData.CustomAttributeArrayLocal );
                    serializerSensitiveData.AspectInstances.Add(
                        (ILaosWeavableAspect) aspectTargetPair.AspectSpecification.Aspect );
                }

                if ( Messenger.Current.ErrorCount > 0 )
                {
                    LaosMessageSource.Instance.Write( SeverityType.Fatal, "LA0006", null );
                }

                #endregion

                // Load all awarenesses.
                List<ILaosAwareness> awarenesses = new List<ILaosAwareness>();
                IEnumerator<ILaosAwareness> awarenessEnumerator = this.Project.Tasks.GetInterfaces<ILaosAwareness>();
                while ( awarenessEnumerator.MoveNext())
                {
                    awarenessEnumerator.Current.Initialize( this );
                    awarenesses.Add(awarenessEnumerator.Current);
                }
                

                #region Order aspects on the same declaration and validate interactions.

                Dictionary<MetadataDeclaration, LaosAspectWeaver[]> orderedAspectWeavers =
                    new Dictionary<MetadataDeclaration, LaosAspectWeaver[]>( this.aspectWeavers.Keys.Count );

                foreach ( MetadataDeclaration target in this.aspectWeavers.Keys )
                {
                    IEnumerator<LaosAspectWeaver> weaverEnumerator = this.aspectWeavers[target].GetEnumerator();
                    weaverEnumerator.MoveNext();
                    LaosAspectWeaver firstWeaver = weaverEnumerator.Current;

                    LaosAspectWeaver[] orderedWeaversOnThisTarget;

                    if ( weaverEnumerator.MoveNext() )
                    {
                        // There are many aspects on the same declaration.
                        List<LaosAspectWeaver> list = new List<LaosAspectWeaver>( 4 )
                                                          {
                                                              firstWeaver,
                                                              weaverEnumerator.Current
                                                          };

                        while ( weaverEnumerator.MoveNext() )
                        {
                            list.Add( weaverEnumerator.Current );
                        }

                        list.Sort( ( x, y ) => x.AspectPriority - y.AspectPriority );

                        orderedWeaversOnThisTarget = list.ToArray();
                    }
                    else
                    {
                        orderedWeaversOnThisTarget = new[] {firstWeaver};
                    }

                    orderedAspectWeavers.Add( target, orderedWeaversOnThisTarget );

                    foreach ( LaosAspectWeaver weaver in orderedWeaversOnThisTarget )
                    {
                        weaver.ValidateInteractions( orderedWeaversOnThisTarget );
                    }

                    // Invoke awarenesses.
                    foreach ( ILaosAwareness awareness in awarenesses )
                    {
                        awareness.ValidateAspects( target, orderedWeaversOnThisTarget );
                    }
                }

                if ( Messenger.Current.ErrorCount > 0 )
                {
                    LaosMessageSource.Instance.Write( SeverityType.Fatal, "LA0006", null );
                }

                #endregion

                #region Implement aspects

                foreach ( KeyValuePair<MetadataDeclaration, LaosAspectWeaver[]> pair in orderedAspectWeavers )
                {
                    MetadataDeclaration originalTarget = pair.Key;
                    MetadataDeclaration redirectedTarget = originalTarget;

                    // Invoke awarenesses.
                    foreach (ILaosAwareness awareness in awarenesses)
                    {
                        awareness.BeforeImplementAspects(originalTarget, pair.Value);
                    }

                    // Invoke weavers.
                    foreach ( LaosAspectWeaver aspectWeaver in pair.Value )
                    {
                        // Notify if a preceding aspect has redirected the target.
                        if ( redirectedTarget != originalTarget )
                        {
                            aspectWeaver.SetExternallyRedirectedTarget( redirectedTarget );
                        }


                        ITypeSignature aspectType = aspectWeaver.GetAspectType();
                        TypeDefDeclaration aspectTypeDef = aspectType.GetTypeDefinition();


                        // Create and initialize a field that will contain the runtime instance of the aspect.
                        if ( aspectWeaver.RequiresRuntimeInstance )
                        {
                            FieldDefDeclaration aspectRuntimeInstanceField = new FieldDefDeclaration
                                                                                 {
                                                                                     Name =
                                                                                         Platform.Current.
                                                                                         NormalizeCilIdentifier(
                                                                                         aspectTypeDef.Name + "~" +
                                                                                         aspectWeaver.UniqueName ),
                                                                                     Attributes =
                                                                                         ( FieldAttributes.Assembly |
                                                                                           FieldAttributes.Static |
                                                                                           FieldAttributes.InitOnly ),
                                                                                     FieldType = aspectType
                                                                                 };

                            if ( aspectTypeDef.Visibility == Visibility.Private ||
                                 aspectTypeDef.Visibility == Visibility.Family )
                            {
                                LaosMessageSource.Instance.Write( SeverityType.Error, "LA0013",
                                                                  new object[]
                                                                      {
                                                                          aspectType.ToString(),
                                                                          aspectTypeDef.Visibility.ToString().
                                                                              ToLowerInvariant()
                                                                      } );
                                continue;
                            }
                            else
                            {
                                implementationDetailsType.Fields.Add( aspectRuntimeInstanceField );
                            }

                            aspectWeaver.AspectRuntimeInstanceField = aspectRuntimeInstanceField;

                            AspectSerializationData aspectSerializationData = aspectWeaver.SerializationData;

                            if ( aspectSerializationData != null )
                            {
                                // The field is deserialized.

                                constructorInstructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc,
                                                                                           aspectSerializationData.
                                                                                               ArrayVariable );
                                constructorInstructionWriter.EmitInstructionInt32( OpCodeNumber.Ldc_I4,
                                                                                   aspectSerializationData.Index );
                                constructorInstructionWriter.EmitInstructionType( OpCodeNumber.Ldelem,
                                                                                  module.Cache.GetIntrinsic(
                                                                                      IntrinsicType.Object ) );
                                constructorInstructionWriter.EmitInstructionType( OpCodeNumber.Castclass,
                                                                                  aspectRuntimeInstanceField.FieldType );
                                constructorInstructionWriter.EmitInstructionField( OpCodeNumber.Stsfld,
                                                                                   aspectRuntimeInstanceField );
                            }
                            else if ( aspectWeaver.AspectSpecification.AspectConstruction != null )
                            {
                                aspectWeaver.EmitAspectConstruction( constructorInstructionWriter );
                            }
                            else
                            {
                                LaosMessageSource.Instance.Write( SeverityType.Error, "LA0035",
                                                                  new object[]
                                                                      {
                                                                          aspectWeaver.AspectSpecification.
                                                                              AspectTypeName,
                                                                          aspectWeaver.TargetElement
                                                                      } );
                                continue;
                            }

                            // Let the weaver implement its initialization.
                            aspectWeaver.EmitRuntimeInitialization( constructorInstructionWriter );
                        }

                        aspectWeaver.Implement();

                        // Check for target redirections due to that weaver.
                        if ( aspectWeaver.TargetRedirection != null )
                            redirectedTarget = aspectWeaver.TargetRedirection;

                        i++;
                    }

                    // Invoke awarenesses.
                    foreach (ILaosAwareness awareness in awarenesses)
                    {
                        awareness.AfterImplementAspects(originalTarget, pair.Value);
                    }
                }

                #endregion

               
                // Implement aspect initialization.
                this.instanceInitializationManager.Implement();

                // Invoke awarenesses.
                foreach (ILaosAwareness awareness in awarenesses)
                {
                    awareness.AfterImplementAllAspects();
                }

                // Do not continue if there are errors.
                if ( Messenger.Current.ErrorCount > 0 )
                {
                    LaosMessageSource.Instance.Write( SeverityType.Fatal, "LA0006", null );
                }

                

                constructorInstructionWriter.EmitInstruction( OpCodeNumber.Ldc_I4_1 );
                constructorInstructionWriter.EmitInstructionField( OpCodeNumber.Stsfld,
                                                                   this.implementationInitializedField );
                if ( this.frameworkVariant != FrameworkVariants.Full )
                {
                    constructorInstructionWriter.EmitInstruction( OpCodeNumber.Ret );
                }
                else
                {
                    constructorInstructionWriter.EmitBranchingInstruction( OpCodeNumber.Leave, returnSequence );
                }
                constructorInstructionWriter.DetachInstructionSequence();

                if ( this.frameworkVariant == FrameworkVariants.Full )
                {
                    ITypeSignature exceptionType = module.Cache.GetType( "System.Exception, mscorlib" );
                    InstructionBlock faultBlock = implementationConstructorBody.CreateInstructionBlock();
                    catchBlock.AddExceptionHandlerCatch( exceptionType, faultBlock,
                                                         NodePosition.After, null );
                    InstructionSequence faultSequence = implementationConstructorBody.CreateInstructionSequence();
                    faultBlock.AddInstructionSequence( faultSequence, NodePosition.After, null );

                    constructorInstructionWriter.AttachInstructionSequence( faultSequence );
                    LocalVariableSymbol exceptionVariable = faultBlock.DefineLocalVariable( exceptionType, "e" );
                    constructorInstructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Stloc, exceptionVariable );
                    constructorInstructionWriter.EmitInstructionInt32( OpCodeNumber.Ldc_I4, 30 );
                    constructorInstructionWriter.EmitInstructionString( OpCodeNumber.Ldstr, "PostSharp" );
                    constructorInstructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, exceptionVariable );
                    constructorInstructionWriter.EmitInstructionMethod( OpCodeNumber.Callvirt,
                                                                        module.FindMethod(
                                                                            typeof(Exception).GetProperty( "Message" )
                                                                                .GetGetMethod(),
                                                                            BindingOptions.Default ) );
                    constructorInstructionWriter.EmitInstructionMethod( OpCodeNumber.Call,
                                                                        module.FindMethod(
                                                                            "System.Diagnostics.Debugger, mscorlib",
                                                                            "Log" ) );
                    constructorInstructionWriter.EmitInstruction( OpCodeNumber.Rethrow );
                    constructorInstructionWriter.DetachInstructionSequence();

                    constructorInstructionWriter.AttachInstructionSequence( returnSequence );
                    constructorInstructionWriter.EmitInstruction( OpCodeNumber.Ret );
                    constructorInstructionWriter.DetachInstructionSequence();
                }

                // Define new resources in the assembly and serialize the initial values into it.
                foreach ( SerializerSensitiveData serializerSensitiveData in serializerSensitiveDatas.Values )
                {
                    ManifestResourceDeclaration resource = new ManifestResourceDeclaration
                                                               {
                                                                   Name = serializerSensitiveData.ResourceName,
                                                                   IsPublic = true,
                                                                   ContentStream = new MemoryStreamIgnoringClose()
                                                               };
                    try
                    {
                        ILaosAspect[] array = serializerSensitiveData.AspectInstances.ToArray();
                        serializerSensitiveData.Serializer.Serialize( array, resource.ContentStream );
                    }
                    catch ( Exception e )
                    {
                        LaosMessageSource.Instance.Write( SeverityType.Fatal, "LA0001",
                                                          new object[] {e.Message} );
                    }
                    resource.ContentStream.Seek( 0, SeekOrigin.Begin );
                    this.Project.Module.AssemblyManifest.Resources.Add( resource );
                }
            }

            this.instructionWriter = null;


            return true;
        }

        private class SerializerSensitiveData
        {
            public LocalVariableSymbol CustomAttributeArrayLocal;
            public readonly List<ILaosWeavableAspect> AspectInstances = new List<ILaosWeavableAspect>();
            public LaosSerializer Serializer;
            public string ResourceName;
        }

        private class MemoryStreamIgnoringClose : MemoryStream
        {
            public override void Close()
            {
            }
        }


        private class AspectInitializeAdvice : IBeforeStaticConstructorAdvice
        {
            private readonly LaosTask task;
            private readonly IMethod throwLaosNotInitializedExceptionMethod;

            public AspectInitializeAdvice( LaosTask task )
            {
                this.task = task;
                this.throwLaosNotInitializedExceptionMethod =
                    this.task.Project.Module.FindMethod(
                        this.task.Project.Module.GetTypeForFrameworkVariant( typeof(LaosNotInitializedException) ),
                        "Throw" );
            }

            #region IAdvice Members

            public int Priority
            {
                get { return int.MinValue; }
            }

            public bool IsBeforeFieldInitSupported
            {
                get { return true; }
            }

            public bool RequiresWeave( WeavingContext context )
            {
                return true;
            }

            public void Weave( WeavingContext context, InstructionBlock block )
            {
                InstructionSequence initializeSequence = context.Method.MethodBody.CreateInstructionSequence();
                InstructionSequence checkSequence = context.Method.MethodBody.CreateInstructionSequence();
                block.AddInstructionSequence( checkSequence, NodePosition.After, null );
                block.AddInstructionSequence( initializeSequence, NodePosition.After, null );

                context.InstructionWriter.AttachInstructionSequence( checkSequence );
                context.InstructionWriter.EmitInstructionField( OpCodeNumber.Ldsfld,
                                                                this.task.implementationInitializedField );
                context.InstructionWriter.EmitBranchingInstruction( OpCodeNumber.Brtrue, initializeSequence );
                context.InstructionWriter.EmitInstructionMethod( OpCodeNumber.Call,
                                                                 this.throwLaosNotInitializedExceptionMethod );
                context.InstructionWriter.DetachInstructionSequence();

                context.InstructionWriter.AttachInstructionSequence( initializeSequence );

                TypeDefDeclaration typeDef = context.Method.DeclaringType;
                foreach ( LaosAspectWeaver weaver in this.task.weaversOnTypes[typeDef] )
                {
                    weaver.EmitRuntimeInitialization( context.InstructionWriter );
                }
                context.InstructionWriter.DetachInstructionSequence();
            }

            #endregion
        }
    }
}