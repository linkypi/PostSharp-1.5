#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
#if !MF

#endif

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Custom attribute that can be applied to multiple elements
    /// using wildcards.
    /// </summary>
    /// <remarks>
    /// <para>Each class derived from <see cref="MulticastAttribute"/>
    /// should be decorated with an instance of <see cref="MulticastAttributeUsageAttribute"/>.
    /// </para>
    /// <para>
    /// Multicasting is performed by the <b>MulticastAttributeTask</b>, which should be
    /// included in the project. After multicasting, custom attribute instances are
    /// available on the <b>CustomAttributeDictionaryTask</b> class.
    /// </para>
    /// </remarks>
#if !SMALL
    [Serializable]
#endif
    public abstract class MulticastAttribute : Attribute
    {
        private class Fields
        {
            /// <summary>
            /// Set of element kinds to which this attribute applies.
            /// </summary>
            public MulticastTargets Targets = MulticastTargets.All;

            /// <summary>
            /// Wildcard or regular expression specifying to which types
            /// this instance applies, or <b>null</b> this instance
            /// applies either to all types, either to the module
            /// or the assembly.
            /// </summary>
            public string TargetTypes;

            /// <summary>
            /// Filters the visibilities of types to which this attribute applies.
            /// Ignored if the target of this attribute is the module or the assembly.
            /// </summary>
            public MulticastAttributes TargetTypeAttributes = MulticastAttributes.All;

            /// <summary>
            /// Wildcard or regular expression specifying to which members
            /// this instance applies, or <b>null</b> if this instance
            /// applies either to all members whose kind is given in <see cref="AttributeTargetElements"/>, 
            /// either to the module or the assembly.
            /// </summary>
            public string TargetMembers;


            /// <summary>
            /// Filters the visibilities of members to which this attribute applies.
            /// Ignored if the target of this attribute does not include type members.
            /// </summary>
            public MulticastAttributes TargetMemberAttributes;

            public string TargetParameters;

            public MulticastAttributes TargetParameterAttributes;

            /// <summary>
            /// Wildcard or regular expression specifying to which assemblies
            /// this instance applies, or <b>null</b> if this instance applies
            /// only to elements of the current assembly.
            /// </summary>
            public string TargetAssemblies;


            /// <summary>
            /// If true, indicates that this attribute <i>removes</i> all other instances of the
            /// same attribute type from the set of elements defined by the current instance.
            /// </summary>
            public bool Exclude;

            /// <summary>
            /// Priority of the current attribute in case that multiple instances are defined
            /// on the same element.
            /// </summary>
            public int Priority;

            public bool Replace;

            public MulticastInheritance Inheritance;

            public long Id;
        }

#if !SL
        [NonSerialized]
#endif
            private readonly Fields fields = new Fields();

        private Fields GetFields()
        {
            if ( this.fields == null )
                throw new NotSupportedException( "Members of MulticastAttribute are not available." );

            return this.fields;
        }


        /// <summary>
        /// Gets or sets the kind of elements to which this custom attributes applies.
        /// </summary>
#if !MF
        [XmlIgnore]
#endif
            public MulticastTargets AttributeTargetElements
        {
            get { return this.GetFields().Targets; }
            set { this.GetFields().Targets = value; }
        }

        /// <summary>
        /// Gets or sets the assemblies to which the current attribute apply.
        /// </summary>
        /// <value>
        /// Wildcard or regular expression specifying to which assemblies
        /// this instance applies, or <b>null</b> if this instance applies
        /// only to elements of the current assembly. Wildcard expressions should
        /// start with the <code>regex:</code> prefix.
        /// </value>
        /// <remarks>
        /// When this property is not specified or is <b>null</b>, the current
        /// attribute is multicasted only in the current assembly. Otherwise, it
        /// is multicasted also to external assemblies, i.e. to declarations that
        /// are <i>referenced</i> by the current assembly.
        /// </remarks>
#if !MF
        [XmlIgnore]
#endif
            public string AttributeTargetAssemblies
        {
            get { return this.GetFields().TargetAssemblies; }
            set { this.GetFields().TargetAssemblies = value; }
        }


        /// <summary>
        /// Gets or sets the expression specifying to which types
        /// this instance applies.
        /// </summary>
        /// <value>
        /// A wildcard or regular expression specifying to which types
        /// this instance applies, or <b>null</b> this instance
        /// applies either to all types. Wildcard expressions should
        /// start with the <code>regex:</code> prefix.
        /// </value>
        /// <remarks>
        /// Ignored if the <see cref="AttributeTargetElements"/> are only the module and/or the assembly.
        /// </remarks>
#if !MF
        [XmlIgnore]
#endif
            public string AttributeTargetTypes
        {
            get { return this.GetFields().TargetTypes; }
            set { this.GetFields().TargetTypes = value; }
        }

        /// <summary>
        /// Gets or sets the visibilities of types to which this attribute applies.
        /// </summary>
        /// <remarks>
        /// On type-level, the only meaningfull enumeration values are related to visibility.
        /// </remarks>
#if !MF
        [XmlIgnore]
#endif
            public MulticastAttributes AttributeTargetTypeAttributes
        {
            get { return this.GetFields().TargetTypeAttributes; }
            set { this.GetFields().TargetTypeAttributes = value; }
        }

        /// <summary>
        /// Gets or sets the expression specifying to which members 
        /// this instance applies.
        /// </summary>
        /// <value>
        /// A wildcard or regular expression specifying to which members
        /// this instance applies, or <b>null</b> this instance
        /// applies either to all members whose kind is given in <see cref="AttributeTargetElements"/>.
        /// Wildcard expressions should
        /// start with the <code>regex:</code> prefix.
        /// </value>
        /// <remarks>
        /// <para>Ignored if the only <see cref="AttributeTargetElements"/> are only types.
        /// </para>
        /// </remarks>
#if !MF
        [XmlIgnore]
#endif
            public string AttributeTargetMembers
        {
            get { return this.GetFields().TargetMembers; }
            set { this.GetFields().TargetMembers = value; }
        }

        /// <summary>
        /// Gets or sets the visibilities, scopes, virtualities, and implementation
        ///  of members to which this attribute applies.
        /// </summary>
        /// <remarks>
        /// <para>Ignored if the <see cref="AttributeTargetElements"/> are only the module, the assembly,
        /// and/or types.
        /// </para>
        /// <para>
        /// The <see cref="MulticastAttributes"/> enumeration is a multi-part flag: there is one
        /// part for visibility, one for scope, one for virtuality, and one for implementation.
        /// If you specify one part, it will override the values defined on the custom attribute definition.
        /// If you do not specify it, the values defined on the custom attribute definition will be inherited.
        /// Note that custom attributes may apply restrictions on these attributes. For instance, 
        /// a custom attribute may not be valid on abstract methods. You are obviously not allowed
        /// to 'enlarge' the set of possible targets.
        /// </para>
        /// </remarks>
#if !MF
        [XmlIgnore]
#endif
            public MulticastAttributes AttributeTargetMemberAttributes
        {
            get { return this.GetFields().TargetMemberAttributes; }
            set { this.GetFields().TargetMemberAttributes = value; }
        }


        /// <summary>
        /// Gets or sets the expression specifying to which parameters 
        /// this instance applies.
        /// </summary>
        /// <value>
        /// A wildcard or regular expression specifying to which parameters
        /// this instance applies, or <b>null</b> this instance
        /// applies either to all members whose kind is given in <see cref="AttributeTargetElements"/>.
        /// Wildcard expressions should
        /// start with the <code>regex:</code> prefix.
        /// </value>
        /// <remarks>
        /// <para>Ignored if the only <see cref="AttributeTargetElements"/> are only types.
        /// </para>
        /// </remarks>
#if !MF
        [XmlIgnore]
#endif
            public string AttributeTargetParameters
        {
            get { return this.GetFields().TargetParameters; }
            set { this.GetFields().TargetParameters = value; }
        }

        /// <summary>
        /// Gets or sets the passing style (by value, <b>out</b> or <b>ref</b>)
        ///  of parameters to which this attribute applies.
        /// </summary>
        /// <remarks>
        /// <para>Ignored if the <see cref="AttributeTargetElements"/> do not include parameters.
        /// </para>
        /// </remarks>
#if !MF
        [XmlIgnore]
#endif
            public MulticastAttributes AttributeTargetParameterAttributes
        {
            get { return this.GetFields().TargetParameterAttributes; }
            set { this.GetFields().TargetParameterAttributes = value; }
        }


        /// <summary>
        /// If true, indicates that this attribute <i>removes</i> all other instances of the
        /// same attribute type from the set of elements defined by the current instance.
        /// </summary>
#if !MF
        [XmlIgnore]
#endif
            public bool AttributeExclude
        {
            get { return this.GetFields().Exclude; }
            set { this.GetFields().Exclude = value; }
        }

        /// <summary>
        /// Gets or sets the priority of the current attribute in case that multiple 
        /// instances are defined on the same element (lower values are processed before).
        /// </summary>
        /// <remarks>
        /// You should use only 16-bit values in user code. Top 16 bits are reserved for the system.
        /// </remarks>
#if !MF
        [XmlIgnore]
#endif
            public int AttributePriority
        {
            get { return this.GetFields().Priority; }
            set { this.GetFields().Priority = value; }
        }

        /// <summary>
        /// Determines whether this attribute replaces other attributes found on the
        /// target declarations.
        /// </summary>
        /// <value>
        /// <b>true</b> if the current instance will replace previous ones, or <b>false</b>
        /// if it will be added to previous instances.
        /// </value>
#if !MF
        [XmlIgnore]
#endif
            public bool AttributeReplace
        {
            get { return this.GetFields().Replace; }
            set { this.GetFields().Replace = value; }
        }

        /// <summary>
        /// Determines whether this attribute is inherited
        /// </summary>
        /// <remarks>
        /// <para>If this property is <b>true</b>, a copy of this attribute will be propagated along
        /// the lines of inheritance of the target element:</para>
        /// <list type="bullet">
        /// <item>On <b>classes</b>: all classed derived from that class.</item>
        /// <item>On <b>interfaces</b>: all classes implementing this interface.</item>
        /// <item>On <b>virtual, abstract or interface methods</b>: all methods overriding 
        /// or implementing this method.</item>
        /// <item>On <b>parameters</b> or <b>return value</b> of virtual, abstract or interface methods:
        /// corresponding parameter or return value on all methods or overriding or implementing the
        /// parent method of the target parameter or return value.</item>
        /// </list>
        /// </remarks>
#if !MF
        [XmlIgnore]
#endif
            public MulticastInheritance AttributeInheritance
        {
            get { return this.GetFields().Inheritance; }
            set { this.GetFields().Inheritance = value; }
        }

        /// <summary>
        /// <b>Internal Only.</b> Identifier of the current instance of the custom attribute.
        /// </summary>
        /// <remarks>
        /// This property is used internally by PostSharp to uniquely identify instances of custom
        /// attributes and avoid adding duplicates. Do not use this property in customer code.
        /// </remarks>
        [Obsolete( "Do not use this property in customer code.", true )]
        public long AttributeId
        {
            get { return this.GetFields().Id; }
            set { this.GetFields().Id = value; }
        }
    }

    /// <summary>
    /// Custom attribute that determines the usage of a <see cref="MulticastAttribute"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = true )]
    public sealed class MulticastAttributeUsageAttribute : Attribute
    {
        /// <summary>
        /// Kinds of targets that instances of the related <see cref="MulticastAttribute"/>
        /// apply to.
        /// </summary>
        private MulticastTargets validOn;

        private BoolWithDefault allowExternalAssemblies;
        private BoolWithDefault allowMultiple;


        private MulticastInheritance inheritance;
        private bool isInheritanceSpecified;

        private BoolWithDefault persistMetaData;

        private MulticastAttributes targetMemberAttributes = MulticastAttributes.All;
        private bool isTargetMemberAttributesSpecified;

        private MulticastAttributes targetParameterAttributes = MulticastAttributes.All;
        private bool isTargetParameterAttributesSpecified;


        private MulticastAttributes targetTypeAttributes = MulticastAttributes.All;
        private bool isTargetTypeAttributesSpecified;


        /// <summary>
        /// Initializes a new <see cref="MulticastAttributeUsageAttribute"/>.
        /// </summary>
        /// <param name="validOn">Kinds of targets that instances of the related <see cref="MulticastAttribute"/>
        /// apply to.</param>
        public MulticastAttributeUsageAttribute( MulticastTargets validOn )
        {
            this.validOn = validOn;
        }

        /// <summary>
        /// Gets the kinds of targets that instances of the related <see cref="MulticastAttribute"/>
        /// apply to.
        /// </summary>
        public MulticastTargets ValidOn
        {
            get { return this.validOn; }
            set { this.validOn = value; }
        }

        /// <summary>
        /// Determines wether many instances of the custom attribute are allowed on a single declaration.
        /// </summary>
        public bool AllowMultiple
        {
            get { return allowMultiple == BoolWithDefault.True; }
            set { allowMultiple = value ? BoolWithDefault.True : BoolWithDefault.False; }
        }

        public bool IsAllowMultipleSpecified
        {
            get { return allowMultiple != BoolWithDefault.Default; }
        }

        /// <summary>
        /// Determines wether the custom attribute in inherited along the lines of inheritance
        /// of the target element.
        /// </summary>
        /// <seealso cref="MulticastAttribute.AttributeInheritance"/>
        public MulticastInheritance Inheritance
        {
            get { return inheritance; }
            set
            {
                this.inheritance = value;
                this.isInheritanceSpecified = true;
            }
        }

        public bool IsInheritanceSpecified
        {
            get { return isInheritanceSpecified; }
        }

        /// <summary>
        /// Determines whether this attribute can be applied to declaration of external assemblies
        /// (i.e. to other assemblies than the one in which the custom attribute is instanciated).
        /// </summary>
        public bool AllowExternalAssemblies
        {
            get { return allowExternalAssemblies == BoolWithDefault.True; }
            set { allowExternalAssemblies = value ? BoolWithDefault.True : BoolWithDefault.False; }
        }

        public bool IsAllowExternalAssembliesSpecified
        {
            get { return allowExternalAssemblies != BoolWithDefault.Default; }
        }

        /// <summary>
        /// Determines whether the custom attribute should be persisted in metadata, so that
        /// it would be available for <b>System.Reflection</b>.
        /// </summary>
        public bool PersistMetaData
        {
            get { return persistMetaData == BoolWithDefault.True; }
            set { persistMetaData = value ? BoolWithDefault.True : BoolWithDefault.False; }
        }

        public bool IsPersistMetaDataSpecified
        {
            get { return persistMetaData != BoolWithDefault.Default; }
        }

        /// <summary>
        /// Gets or sets the attributes of the members (fields or methods) to which
        /// the custom attribute can be applied.
        /// </summary>
        public MulticastAttributes TargetMemberAttributes
        {
            get { return this.targetMemberAttributes; }
            set
            {
                this.targetMemberAttributes = value;
                this.isTargetMemberAttributesSpecified = true;
            }
        }

        public bool IsTargetMemberAttributesSpecified
        {
            get { return this.isTargetMemberAttributesSpecified; }
        }

        /// <summary>
        /// Gets or sets the attributes of the parameter to which
        /// the custom attribute can be applied.
        /// </summary>
        public MulticastAttributes TargetParameterAttributes
        {
            get { return this.targetMemberAttributes; }
            set
            {
                this.targetParameterAttributes = value;
                this.isTargetParameterAttributesSpecified = true;
            }
        }

        public bool IsTargetParameterAttributesSpecified
        {
            get { return this.isTargetParameterAttributesSpecified; }
        }

        /// <summary>
        /// Gets or sets the attributes of the types to which
        /// the custom attribute can be applied. If the custom attribute relates to
        /// fields or methods, this property specifies which attributes
        /// of the declaring type are acceptable.
        /// </summary>
        public MulticastAttributes TargetTypeAttributes
        {
            get { return this.targetTypeAttributes; }
            set
            {
                this.targetTypeAttributes = value;
                this.isTargetTypeAttributesSpecified = true;
            }
        }

        public bool IsTargetTypeAttributesSpecified
        {
            get { return this.isTargetTypeAttributesSpecified; }
        }


        internal MulticastAttributeUsageAttribute Clone()
        {
            MulticastAttributeUsageAttribute clone = new MulticastAttributeUsageAttribute( this.validOn )
                                                         {
                                                             persistMetaData = this.persistMetaData,
                                                             allowExternalAssemblies = this.allowExternalAssemblies,
                                                             allowMultiple = this.allowMultiple,
                                                             targetMemberAttributes = this.targetMemberAttributes,
                                                             isTargetMemberAttributesSpecified =
                                                                 this.isTargetMemberAttributesSpecified,
                                                             targetParameterAttributes = this.targetParameterAttributes,
                                                             isTargetParameterAttributesSpecified =
                                                                 this.isTargetParameterAttributesSpecified
                                                         };
            return clone;
        }

        public static MulticastAttributeUsageAttribute GetMaximumValue()
        {
            MulticastAttributeUsageAttribute attribute = new MulticastAttributeUsageAttribute( MulticastTargets.All )
                                                             {
                                                                 AllowExternalAssemblies = true,
                                                                 AllowMultiple = true,
                                                                 TargetMemberAttributes = MulticastAttributes.All,
                                                                 TargetTypeAttributes = MulticastAttributes.All
                                                             };

            return attribute;
        }
    }

    /// <summary>
    /// Kinds of targets to which multicast custom attributes (<see cref="MulticastAttribute"/>)
    /// can apply.
    /// </summary>
    [Flags]
#if !MF
    [SuppressMessage( "Microsoft.Usage", "CA2217:DoNotMarkEnumsWithFlags" )]
#endif
    public enum MulticastTargets
    {
        /// <summary>
        /// Specifies that the set of target elements is inherited from
        /// the parent custom attribute.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Class.
        /// </summary>
        Class = 1,

        /// <summary>
        /// Structure.
        /// </summary>
        Struct = 2,

        /// <summary>
        /// Enumeration.
        /// </summary>
        Enum = 4,

        /// <summary>
        /// Delegate.
        /// </summary>
        Delegate = 8,

        /// <summary>
        /// Interface.
        /// </summary>
        Interface = 16,

        /// <summary>
        /// Any type (<see cref="Class"/>, <see cref="Struct"/>, <see cref="Enum"/>,
        /// <see cref="Delegate"/> or <see cref="Interface"/>).
        /// </summary>
        AnyType = Class | Struct | Enum | Delegate | Interface,

        /// <summary>
        /// Field.
        /// </summary>
        Field = 32,

        /// <summary>
        /// Method (but not constructor).
        /// </summary>
        Method = 64,

        /// <summary>
        /// Constructor.
        /// </summary>
        [Obsolete( "Use InstanceConstructor or StaticConstructor." )] Constructor =
            InstanceConstructor | StaticConstructor,

        /// <summary>
        /// Instance constructor.
        /// </summary>
        InstanceConstructor = 128,

        /// <summary>
        /// Static constructor.
        /// </summary>
        StaticConstructor = 256,

        /// <summary>
        /// Property (but not methods inside the property).
        /// </summary>
        Property = 512,

        /// <summary>
        /// Event (but not methods inside the event).
        /// </summary>
        Event = 1024,

        /// <summary>
        /// Any member (<see cref="Field"/>, <see cref="Method"/>, <see cref="InstanceConstructor"/>,
        /// <see cref="StaticConstructor"/>,
        /// <see cref="Property"/>, <see cref="Event"/>).
        /// </summary>
        AnyMember = Field | Method | InstanceConstructor | StaticConstructor | Property | Event,

        /// <summary>
        /// Assembly.
        /// </summary>
        Assembly = 2048,

        /// <summary>
        /// Method or property parameter.
        /// </summary>
        Parameter = 4096,

        /// <summary>
        /// Method or property return value.
        /// </summary>
        ReturnValue = 8192,

        /// <summary>
        /// All element kinds.
        /// </summary>
        All = Assembly | AnyMember | AnyType | Parameter | ReturnValue
    }

    /// <summary>
    /// Attributes of elements to which multicast custom attributes (<see cref="MulticastAttribute"/>)
    /// apply.
    /// </summary>
    /// <remarks>
    /// When specifying attributes of target members or types, do not forget to provide
    /// <i>all</i> categories of flags, not only the category on which you want to put
    /// a restriction. If you want to limit, for instance, a custom attribute to be
    /// applied on public members, a good pratice is to set 
    /// <c>TargetMemberAttributes = MulticastAttributes.Public | ~MulticastAttributes.AnyVisibility</c>,
    /// unless the parent is more restrictive, in which case you should specify attributes
    /// explicitely.
    /// </remarks>
    [Flags]
#if !MF
    [SuppressMessage( "Microsoft.Usage", "CA2217:DoNotMarkEnumsWithFlags" )]
#endif
    public enum MulticastAttributes
    {
        /// <summary>
        /// Specifies that the set of target attributes is inherited from
        /// the parent custom attribute.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Private (visible inside the current type).
        /// </summary>
        Private = 1 << 1,

        /// <summary>
        /// Protected (visible inside derived types).
        /// </summary>
        Protected = 1 << 2,

        /// <summary>
        /// Internal (visible inside the current assembly).
        /// </summary>
        Internal = 1 << 3,

        /// <summary>
        /// Internal <i>and</i> protected (visible inside derived types that are defined in the current assembly).
        /// </summary>
        InternalAndProtected = 1 << 4,

        /// <summary>
        /// Internal <i>or</i> protected (visible inside all derived types and in the current assembly).
        /// </summary>
        InternalOrProtected = 1 << 5,

        /// <summary>
        /// Public (visible everywhere).
        /// </summary>
        Public = 1 << 6,

        /// <summary>
        /// Any visibility.
        /// </summary>
        AnyVisibility = Private | Protected | Internal | InternalAndProtected | InternalOrProtected | Public,

        /// <summary>
        /// Static scope.
        /// </summary>
        Static = 1 << 7,

        /// <summary>
        /// Instance scope.
        /// </summary>
        Instance = 1 << 8,

        /// <summary>
        /// Any scope (<see cref="Static"/> | <see cref="Instance"/>).
        /// </summary>
        AnyScope = Static | Instance,

        /// <summary>
        /// Abstract methods.
        /// </summary>
        Abstract = 1 << 9,

        /// <summary>
        /// Concrete (non-abstract) methods. 
        /// </summary>
        NonAbstract = 1 << 10,

        /// <summary>
        /// Any abstraction (<see cref="Abstract"/> | <see cref="NonAbstract"/>).
        /// </summary>
        AnyAbstraction = Abstract | NonAbstract,

        /// <summary>
        /// Virtual methods. 
        /// </summary>
        Virtual = 1 << 11,

        /// <summary>
        /// Non-virtual methods.
        /// </summary>
        NonVirtual = 1 << 12,

        /// <summary>
        /// Any virtuality (<see cref="Virtual"/> | <see cref="NonVirtual"/>).
        /// </summary>
        AnyVirtuality = NonVirtual | Virtual,


        /// <summary>
        /// Managed code implemetation.
        /// </summary>
        Managed = 1 << 13,

        /// <summary>
        /// Non-managed code implementation (external or system).
        /// </summary>
        NonManaged = 1 << 14,

        /// <summary>
        /// Any implementation (<see cref="Managed"/> | <see cref="NonManaged"/>).
        /// </summary>
        AnyImplementation = Managed | NonManaged,

        /// <summary>
        /// Literal fields.
        /// </summary>
        Literal = 1 << 15,

        /// <summary>
        /// Non-literal fields.
        /// </summary>
        NonLiteral = 1 << 16,


        /// <summary>
        /// Any field literality (<see cref="Literal"/> | <see cref="NonLiteral"/>).
        /// </summary>
        AnyLiterality = Literal | NonLiteral,

        /// <summary>
        /// Input parameters.
        /// </summary>
        InParameter = 1 << 17,

        /// <summary>
        /// Compiler-generated code (for instance closure types of anonymous method, iterator type, ...).
        /// </summary>
        CompilerGenerated = 1 << 18,

        /// <summary>
        /// User-generated code (anything expected <see cref="CompilerGenerated"/>).
        /// </summary>
        UserGenerated = 1 << 19,

        /// <summary>
        /// Any code generation (<see cref="CompilerGenerated"/> | <see cref="UserGenerated"/>)l
        /// </summary>
        AnyGeneration = CompilerGenerated | UserGenerated,

        /// <summary>
        /// Output (<b>out</b> in C#) parameters.
        /// </summary>
        OutParameter = 1 << 18,

        /// <summary>
        /// Input/Output (<b>ref</b> in C#) parameters.
        /// </summary>
        RefParameter = 1 << 19,

        /// <summary>
        /// Any kind of parameter passing (<see cref="InParameter"/> | <see cref="OutParameter"/> | <see cref="RefParameter"/>).
        /// </summary>
        AnyParameter = InParameter | OutParameter | RefParameter,

        /// <summary>
        /// All members.
        /// </summary>
        All =
            AnyVisibility | AnyVirtuality | AnyScope | AnyImplementation | AnyLiterality | AnyAbstraction |
            AnyGeneration | AnyParameter
    }

    /// <summary>
    /// Kind of inheritance of <see cref="MulticastAttribute"/>.
    /// </summary>
    public enum MulticastInheritance
    {
        /// <summary>
        /// No inheritance.
        /// </summary>
        None,

        /// <summary>
        /// The instance is inherited to children of the original element,
        /// but multicasting is not applied to members of children.
        /// </summary>
        Strict,

        /// <summary>
        /// The instance is inherited to children of the original element
        /// and multicasting is applied to members of children.
        /// </summary>
        Multicast
    }
}