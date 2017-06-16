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
using PostSharp.Extensibility;
using PostSharp.Laos;

namespace PostSharp.Samples.Librarian.Framework
{
    /// <summary>
    /// Custom attribute that needs to be applied on assemblies that define entities (<see cref="Entity"/>)
    /// or other classes requiring post-compilation code generation (<see cref="Aspectable"/>). This
    /// custom attribute precisely causes this code to be generated and performs additional
    /// compile-time validation.
    /// </summary>
    [MulticastAttributeUsage( MulticastTargets.Assembly, AllowMultiple=false )]
    [RequirePostSharp("PostSharp.Samples.Librarian", "PostSharp.Samples.Librarian.Weaver.LibrarianAspectWeaverFactory")]
    public sealed class AspectedAssemblyAttribute : CompoundAspect
    {
        // List of discovered types (in the target assembly) derived from Aspectable.
        private readonly List<Type> aspectableTypes = new List<Type>();

        // List of discovered types (in the target assembly) derived from Entity.
        private readonly List<Type> entityTypes = new List<Type>();


        /// <summary>
        /// Discovers types derived from <see cref="Aspectable"/> or <see cref="Entity"/>,
        /// with a recursion on nested types.
        /// </summary>
        /// <param name="type">Type to consider.</param>
        private void DiscoverTypeRecursive( Type type )
        {
            // Derived from Aspectable?
            if ( typeof(Aspectable).IsAssignableFrom( type ) )
            {
                this.aspectableTypes.Add( type );
            }

            // Derived from Entity?
            if ( typeof(Entity).IsAssignableFrom( type ) )
            {
                this.entityTypes.Add( type );
            }

            // Recursion on nested types.
            foreach ( Type nestedType in type.GetNestedTypes( BindingFlags.Public | BindingFlags.NonPublic ) )
            {
                this.DiscoverTypeRecursive( nestedType );
            }
        }

        /// <summary>
        /// Validates a field of an entity class (the field itself, not its value, since 
        /// we are at compile-time!).
        /// </summary>
        /// <param name="field">The field to be validated.</param>
        /// <returns><b>true</b> if this field is valid, otherwise <b>false</b>.</returns>
        private static bool ValidateEntityField( FieldInfo field )
        {
            // Check that the field type is not an entity in itself (should use an EntityRef).
            if ( typeof(Entity).IsAssignableFrom( field.FieldType ) )
            {
                LibrarianMessageSource.Instance.Write( SeverityType.Error, "LF0001",
                                                       new object[]
                                                           {field.DeclaringType.Name, field.Name, field.FieldType.Name} );
                return false;
            }

            // Check that the field type is either a value type either is cloneable.
            if ( !field.FieldType.IsValueType &&
                 !typeof(ICloneable).IsAssignableFrom( field.FieldType ) &&
                 field.FieldType != typeof(string) )
            {
                LibrarianMessageSource.Instance.Write( SeverityType.Error, "LF0002",
                                                       new object[]
                                                           {field.DeclaringType.Name, field.Name, field.FieldType.Name} );
                return false;
            }

            // Check that the field type is serializable.
            if ( !field.FieldType.IsSerializable )
            {
                LibrarianMessageSource.Instance.Write( SeverityType.Error, "LF0003",
                                                       new object[]
                                                           {field.DeclaringType.Name, field.Name, field.FieldType.Name} );
                return false;
            }

            // The field is correct.
            return true;
        }

        /// <summary>
        /// Validates a type derived from <see cref="Entity"/>.
        /// </summary>
        /// <param name="type">A type derived from <see cref="Entity"/>.</param>
        /// <returns><b>true</b> if the type is correct, otherwise <b>false</b>.</returns>
        private static bool ValidateEntityType( Type type )
        {
            bool ok = true;

            // Ensure that the type is serializable.
            if ( !type.IsSerializable )
            {
                LibrarianMessageSource.Instance.Write( SeverityType.Error, "LF0004",
                                                       new object[] {type.Name} );
                ok = false;
            }

            // Check fields.
            foreach (
                FieldInfo field in
                    type.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) )
            {
                if ( field.DeclaringType == type )
                {
                    ok &= ValidateEntityField( field );
                }
            }

            return ok;
        }

        /// <summary>
        /// Validates the assembly to which the current custom attribute is applied. What
        /// we want to do is to ensure that all entity classes respect some rules. This
        /// method is called by the post-compiler.
        /// </summary>
        /// <param name="element">The <see cref="Assembly"/> to which the current
        /// custom attribute is applied.</param>
        /// <returns><b>true</b> if all rules are respected, otherwise <b>false</b>.</returns>
        public override bool CompileTimeValidate( object element )
        {
            bool ok = true;

            Assembly assembly = (Assembly)element;

            // Discover types.
            foreach ( Type type in assembly.GetTypes() )
            {
                this.DiscoverTypeRecursive( type );
            }

            // Validate entity types.
            foreach ( Type type in this.entityTypes )
            {
                ok &= ValidateEntityType( type );
            }

            return ok;
        }

        /// <summary>
        /// Called by PostSharp to provide aspects (or code generators) required on the
        /// target assembly.
        /// </summary>
        /// <param name="element"><see cref="Assembly"/> on which the current custom
        /// attribute is applied.</param>
        /// <param name="collection">Collection in which required aspects should be added.</param>
        /// <remarks>
        /// We need to add an aspect to implement the <see cref="ICloneable"/> interface.
        /// Other aspects are detected by their custom attribute.
        /// </remarks>
        public override void ProvideAspects( object element, LaosReflectionAspectCollection collection )
        {
            // Detect the types that need explicyt implementation of Cloneable.CopyTo.
            foreach ( Type type in this.aspectableTypes )
            {
                bool requiredExplicitCopyToImpl = false;

                // Does at least one field needs to be cloned explicitely?
                foreach (
                    FieldInfo field in
                        type.GetFields( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ) )
                {
                    if ( field.DeclaringType == type &&
                         typeof(ICloneable).IsAssignableFrom( field.FieldType ) )
                    {
                        requiredExplicitCopyToImpl = true;
                        break;
                    }
                }

                if ( requiredExplicitCopyToImpl )
                {
                    collection.AddAspect( type, new ImplementCloneableAspect() );
                }
            }
        }

       
        #region ImplementCloneableAspect

        /// <summary>
        /// Runtime information for the Cloneable aspect. There are no runtime
        /// information, but this object should be present to trigger the
        /// generation of the compile-time weaver.
        /// </summary>
        [Serializable]
        internal class ImplementCloneableAspect : ILaosTypeLevelAspect
        {
            public ImplementCloneableAspect()
            {
            }



            public void RuntimeInitialize( Type type )
            {
            }

           
        }

        #endregion
    }
}