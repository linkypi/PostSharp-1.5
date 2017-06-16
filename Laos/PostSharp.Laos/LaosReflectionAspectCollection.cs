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

#if !SMALL
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.Reflection;

namespace PostSharp.Laos
{
    /// <summary>
    /// Collection that types implementing the <see cref="ILaosReflectionAspectProvider"/> interface
    /// can use to provide new aspects.
    /// </summary>
    public sealed class LaosReflectionAspectCollection : IEnumerable<KeyValuePair<object, AspectSpecification>>
    {
        private readonly List<KeyValuePair<object, AspectSpecification>> collection =
            new List<KeyValuePair<object, AspectSpecification>>();

        internal void InternalAddAspect( object target, ILaosAspect aspect )
        {
            ILaosAspectBuildSemantics aspectBuildSemantics = aspect as ILaosAspectBuildSemantics;
            if ( aspectBuildSemantics == null || aspectBuildSemantics.CompileTimeValidate( target ) )
            {
                this.collection.Add( new KeyValuePair<object, AspectSpecification>( target,
                                                                                    new AspectSpecification( null,
                                                                                                             aspect ) ) );
            }
        }

        /// <summary>
        /// Adds an aspect to an element of unknown type.
        /// </summary>
        /// <param name="target">Target of the aspect.</param>
        /// <param name="aspect">Aspect.</param>
        public void AddAspect( object target, ILaosAspect aspect )
        {
            MethodBase targetMethod;
            FieldInfo targetField;
            Type targetType;
            PropertyInfo targetProperty;
            EventInfo targetEvent;
            ParameterInfo targetParameter;

            if ( ( targetMethod = target as MethodBase ) != null )
            {
                this.AddAspect( targetMethod, aspect );
            }
            else if ( ( targetField = target as FieldInfo ) != null )
            {
                this.AddAspect( targetField, aspect );
            }
            else if ( ( targetType = target as Type ) != null )
            {
                this.AddAspect( targetType, aspect );
            }
            else if ( ( targetProperty = target as PropertyInfo ) != null )
            {
                this.AddAspect( targetProperty, aspect );
            }
            else if ( ( targetEvent = target as EventInfo ) != null )
            {
                this.AddAspect( targetEvent, aspect );
            }
            else if ( ( targetParameter = target as ParameterInfo ) != null )
            {
                this.AddAspect( targetParameter, aspect );
            }
            else
            {
                throw new ArgumentException( string.Format( "Cannot apply an aspect to a {0}.", target.GetType() ),
                                             "target" );
            }
        }

        /// <summary>
        /// Add a new method-level aspect.
        /// </summary>
        /// <param name="targetMethod">Target method.</param>
        /// <param name="aspect">Aspect.</param>
        public void AddAspect( MethodBase targetMethod, ILaosAspect aspect )
        {
            #region Preconditions

            if ( targetMethod == null )
            {
                throw new ArgumentNullException( "targetMethod" );
            }
            if ( aspect == null )
            {
                throw new ArgumentNullException( "aspect" );
            }

            if ( ( targetMethod.IsGenericMethod && !targetMethod.IsGenericMethodDefinition ) ||
                 ( targetMethod.DeclaringType.IsGenericType && !targetMethod.DeclaringType.IsGenericTypeDefinition ) )
            {
                throw new ArgumentException(
                    "You cannot add an aspect to a generic method instance or to a method in a generic type instance. Add the aspect to the corresponding generic method definition." );
            }

            #endregion

            this.InternalAddAspect( targetMethod, aspect );
        }

        /// <summary>
        /// Add a new field-level aspect.
        /// </summary>
        /// <param name="targetField">Target field.</param>
        /// <param name="aspect">Aspect.</param>
        public void AddAspect( FieldInfo targetField, ILaosAspect aspect )
        {
            #region Preconditions

            if ( targetField == null )
            {
                throw new ArgumentNullException( "targetField" );
            }
            if ( aspect == null )
            {
                throw new ArgumentNullException( "aspect" );
            }
            if ( ( targetField.DeclaringType.IsGenericType && !targetField.DeclaringType.IsGenericTypeDefinition ) )
            {
                throw new ArgumentException(
                    "You cannot add an aspect to a field of a generic type instance. Add the aspect to the corresponding field in the generic type definition." );
            }

            #endregion

            this.InternalAddAspect( targetField, aspect );
        }

        /// <summary>
        /// Add a new property-level aspect.
        /// </summary>
        /// <param name="targetProperty">Target property.</param>
        /// <param name="aspect">Aspect.</param>
        public void AddAspect( PropertyInfo targetProperty, ILaosAspect aspect )
        {
            #region Preconditions

            if ( targetProperty == null )
            {
                throw new ArgumentNullException( "targetProperty" );
            }
            if ( aspect == null )
            {
                throw new ArgumentNullException( "aspect" );
            }

            MethodInfo getter = targetProperty.GetGetMethod( true );

            if ( getter == null )
                throw new ArgumentException( "You cannot add an aspect to a property without getter." );

            if ( ( getter.IsGenericMethod && !getter.IsGenericMethodDefinition ) ||
                 ( getter.DeclaringType.IsGenericType && !getter.DeclaringType.IsGenericTypeDefinition ) )
            {
                throw new ArgumentException(
                    "You cannot add an aspect to a generic property instance or to a property in a generic type instance. Add the aspect to the corresponding generic property definition." );
            }

            #endregion

            this.InternalAddAspect( targetProperty, aspect );
        }

        /// <summary>
        /// Add a new event-level aspect.
        /// </summary>
        /// <param name="targetEvent">Target event.</param>
        /// <param name="aspect">Aspect.</param>
        public void AddAspect( EventInfo targetEvent, ILaosAspect aspect )
        {
            #region Preconditions

            if ( targetEvent == null )
            {
                throw new ArgumentNullException( "targetEvent" );
            }
            if ( aspect == null )
            {
                throw new ArgumentNullException( "aspect" );
            }

            MethodInfo adder = targetEvent.GetAddMethod( true );

            if ( adder == null )
                throw new ArgumentException( "You cannot add an aspect to an event without adder." );

            if ( ( adder.IsGenericMethod && !adder.IsGenericMethodDefinition ) ||
                 ( adder.DeclaringType.IsGenericType && !adder.DeclaringType.IsGenericTypeDefinition ) )
            {
                throw new ArgumentException(
                    "You cannot add an aspect to a generic event instance or to an event in a generic type instance. Add the aspect to the corresponding generic event definition." );
            }

            #endregion

            this.InternalAddAspect( targetEvent, aspect );
        }

        /// <summary>
        /// Add a new parameter-level aspect.
        /// </summary>
        /// <param name="targetParameter">Target parameter.</param>
        /// <param name="aspect">Aspect.</param>
        public void AddAspect( ParameterInfo targetParameter, ILaosAspect aspect )
        {
            #region Preconditions

            if ( targetParameter == null )
            {
                throw new ArgumentNullException( "targetParameter" );
            }
            if ( aspect == null )
            {
                throw new ArgumentNullException( "aspect" );
            }

            MethodInfo declaringMethod = targetParameter.Member as MethodInfo;

            if ( declaringMethod == null )
            {
                throw new ArgumentException( "Cannot add an aspect to a parameter of something else than a method." );
            }


            if ( ( declaringMethod.IsGenericMethod && !declaringMethod.IsGenericMethodDefinition ) ||
                 ( declaringMethod.DeclaringType.IsGenericType && !declaringMethod.DeclaringType.IsGenericTypeDefinition ) )
            {
                throw new ArgumentException(
                    "You cannot add an aspect to a generic method instance or to a method in a generic type instance. Add the aspect to the corresponding generic method definition." );
            }

            #endregion

            this.InternalAddAspect( targetParameter, aspect );
        }

        /// <summary>
        /// Add a new type-level aspect.
        /// </summary>
        /// <param name="targetType">Target type.</param>
        /// <param name="aspect">aspect.</param>
        public void AddAspect( Type targetType, ILaosAspect aspect )
        {
            #region Preconditions

            if ( targetType == null )
            {
                throw new ArgumentNullException( "targetType" );
            }
            if ( aspect == null )
            {
                throw new ArgumentNullException( "aspect" );
            }
            if ( ( targetType.IsGenericType && !targetType.IsGenericTypeDefinition ) )
            {
                throw new ArgumentException(
                    "You cannot add an aspect to a generic type instance. Add the aspect to the corresponding generic type definition." );
            }

            #endregion

            this.InternalAddAspect( targetType, aspect );
        }


        /// <summary>
        /// Allows to add an aspect to a <see cref="Type"/> when it is not possible to create an instance of this aspect,
        /// typically because the transformed assembly is linked against the Compact Framework
        /// or Silverlight.
        /// </summary>
        /// <param name="targetType">Target <see cref="Type"/> of the aspect.</param>
        /// <param name="aspectConstruction">Aspect construction.</param>
        /// <param name="aspectConfiguration">Aspect configuration (optional).</param>
        public void AddAspectConstruction( Type targetType, IObjectConstruction aspectConstruction,
                                           ILaosAspectConfiguration aspectConfiguration )
        {
            #region Preconditions

            if ( targetType == null )
            {
                throw new ArgumentNullException( "targetType" );
            }

            if ( aspectConstruction == null )
            {
                throw new ArgumentNullException( "aspectConstruction" );
            }

            #endregion

            this.InternalAddAspectConstruction( targetType, aspectConstruction, aspectConfiguration );
        }

        /// <summary>
        /// Allows to add an aspect to a method, constructor, property, field or even, when it is not possible to create an instance of this aspect,
        /// typically because the transformed assembly is linked against the Compact Framework
        /// or Silverlight.
        /// </summary>
        /// <param name="targetMember">Target <see cref="MemberInfo"/> of the aspect.</param>
        /// <param name="aspectConstruction">Aspect construction.</param>
        /// <param name="aspectConfiguration">Aspect configuration (optional).</param>
        public void AddAspectConstruction( MemberInfo targetMember, IObjectConstruction aspectConstruction,
                                           ILaosAspectConfiguration aspectConfiguration )
        {
            #region Preconditions

            if ( targetMember == null )
            {
                throw new ArgumentNullException( "targetMember" );
            }

            if ( aspectConstruction == null )
            {
                throw new ArgumentNullException( "aspectConstruction" );
            }

            #endregion

            this.InternalAddAspectConstruction( targetMember, aspectConstruction, aspectConfiguration );
        }

        /// <summary>
        /// Allows to add an aspect to a parameter when it is not possible to create an instance of this aspect,
        /// typically because the transformed assembly is linked against the Compact Framework
        /// or Silverlight.
        /// </summary>
        /// <param name="targetParameter">Target <see cref="MemberInfo"/> of the aspect.</param>
        /// <param name="aspectConstruction">Aspect construction.</param>
        /// <param name="aspectConfiguration">Aspect configuration (optional).</param>
        public void AddAspectConstruction( ParameterInfo targetParameter, IObjectConstruction aspectConstruction,
                                           ILaosAspectConfiguration aspectConfiguration )
        {
            #region Preconditions

            if ( targetParameter == null )
            {
                throw new ArgumentNullException( "targetParameter" );
            }

            if ( aspectConstruction == null )
            {
                throw new ArgumentNullException( "aspectConstruction" );
            }

            #endregion

            this.InternalAddAspectConstruction( targetParameter, aspectConstruction, aspectConfiguration );
        }

        /// <summary>
        /// Allows to add an aspect to an assembly when it is not possible to create an instance of this aspect,
        /// typically because the transformed assembly is linked against the Compact Framework
        /// or Silverlight.
        /// </summary>
        /// <param name="targetAssembly">Target <see cref="MemberInfo"/> of the aspect.</param>
        /// <param name="aspectConstruction">Aspect construction.</param>
        /// <param name="aspectConfiguration">Aspect configuration (optional).</param>
        public void AddAspectConstruction( IAssemblyName targetAssembly, IObjectConstruction aspectConstruction,
                                           ILaosAspectConfiguration aspectConfiguration )
        {
            #region Preconditions

            if ( targetAssembly == null )
            {
                throw new ArgumentNullException( "targetAssembly" );
            }

            if ( aspectConstruction == null )
            {
                throw new ArgumentNullException( "aspectConstruction" );
            }

            #endregion

            this.InternalAddAspectConstruction( targetAssembly, aspectConstruction, aspectConfiguration );
        }

        private void InternalAddAspectConstruction( object target, IObjectConstruction aspectConstruction,
                                                    ILaosAspectConfiguration aspectConfiguration )
        {
            this.collection.Add( new KeyValuePair<object, AspectSpecification>( target,
                                                                                new AspectSpecification(
                                                                                    aspectConstruction,
                                                                                    aspectConfiguration ) ) );
        }

        #region IEnumerable<KeyValuePair<object,ILaosAspect>> Members

        /// <summary>
        /// Enumerates the content of this collection.
        /// </summary>
        /// <returns>An enumerator of <see cref="KeyValuePair{K,V}"/>
        /// where the key is the target element (<see cref="MethodBase"/>, <see cref="FieldInfo"/>
        /// or <see cref="Type"/>) and the value is the aspect.</returns>
        public IEnumerator<KeyValuePair<object, AspectSpecification>> GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }


        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}

#endif