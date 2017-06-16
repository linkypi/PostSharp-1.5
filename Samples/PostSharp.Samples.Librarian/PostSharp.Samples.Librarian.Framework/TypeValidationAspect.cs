#region Released to Public Domain by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of samples of PostSharp.                                *
 *                                                                             *
 *   This sample is free software: you have an unlimited right to              *
 *   redistribute it and/or modify it.                                         *
 *                                                                             *
 *   This sample is distributed in the hope that it will be useful,            *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.                      *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Laos;

namespace PostSharp.Samples.Librarian.Framework
{
    /// <summary>
    /// Aspect applied on types that have fields decorated with a <see cref="FieldValidationAttribute"/>
    /// custom attribute. This aspect implements the <see cref="Aspectable.AutoGeneratedValidate"/>
    /// method. You should not use this class directly in your code.
    /// </summary>
    [Serializable]
    public class TypeValidationAspect : ILaosTypeLevelAspect
    {
        /// <summary>
        /// Used at compile-time only, maintains the association between types (the key is the type full name)
        /// and an instance of the current class. This is used by the <see cref="RegisterFieldValidator"/> method.
        /// </summary>
        private static readonly Dictionary<string, TypeValidationAspect> instances =
            new Dictionary<string, TypeValidationAspect>();

        /// <summary>
        /// List of custom attributes found on fields of the type to which the current
        /// custom attribute is applied.
        /// </summary>
        private readonly List<FieldValidationAttribute> validators = new List<FieldValidationAttribute>();


        /// <summary>
        /// Initializes a new <see cref="TypeValidationAspect"/>.
        /// </summary>
        internal TypeValidationAspect()
        {
        }

        /// <summary>
        /// Gets the collection of validators on the type on which the current aspect
        /// is applied.
        /// </summary>
        internal ICollection<FieldValidationAttribute> Validators { get { return this.validators; } }


        /// <summary>
        /// Called by the <b>ProvideAspects</b> method of <see cref="FieldValidationAttribute"/>.
        /// Since a type can have many validated fields but needs a single instance of the current type-level aspect,
        /// this method ensures that each validated type has a single <see cref="TypeValidationAspect"/>
        /// and registers all validated fields in this aspect.
        /// </summary>
        /// <param name="field">Validated field.</param>
        /// <param name="validator">Field validator.</param>
        /// <param name="aspects">Collection to which the new aspect should (eventually) be added.</param>
        internal static void RegisterFieldValidator( FieldInfo field, FieldValidationAttribute validator,
                                                     LaosReflectionAspectCollection aspects )
        {
            TypeValidationAspect instance;

            // Do I have already an aspect for this type?
            if ( !instances.TryGetValue( field.DeclaringType.FullName, out instance ) )
            {
                // No? Create one.
                instance = new TypeValidationAspect();
                instances.Add( field.DeclaringType.FullName, instance );
                aspects.AddAspect( field.DeclaringType, instance );
            }

            // Index the validator in the new aspect instance.
            instance.validators.Add( validator );
        }


        /// <summary>
        /// Method called by generated code (in user assemblies) to validate all validated fields.
        /// </summary>
        /// <param name="fieldValues">An array containing the value of all validated fields, such that
        /// the <i>i</i>-th element of <paramref name="fieldValues"/> is the value of the field of the validator
        /// at the <i>i</i>-th position of the <see cref="Validators"/> collection.</param>
        public void Validate( object[] fieldValues )
        {
            if ( fieldValues == null )
                throw new ArgumentNullException( "fieldValues" );
            if ( fieldValues.Length != this.validators.Count )
            {
                throw new ArgumentException( "Wrong number of field values.", "fieldValues" );
            }

            for ( int i = 0 ; i < fieldValues.Length ; i++ )
            {
                this.validators[i].ValidateFieldValue( fieldValues[i] );
            }
        }

        #region ILaosTypeLevelAspect Members

        public void CompileTimeInitialize( Type type )
        {
        }

        public bool CompileTimeValidate( object type )
        {
            return true;
        }

        public void RuntimeInitialize( Type type )
        {
        }

        #endregion

        #region ILaosWeavableAspect Members

        public int? AspectPriority { get { return 0; } }
        public Type SerializerType
        {
            get { return null; }
        }

        #endregion

        #region Implementation of ILaosAspectConfiguration

        public bool? RequiresReflectionWrapper
        {
            get { return false; }
        }

        #endregion

    
    }
}