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
using System.Collections.ObjectModel;
using System.Reflection;

namespace PostSharp.Reflection
{
    /// <summary>
    /// Specifies how an object should be constructed, i.e. specifies the constructor to be
    /// used, the arguments to be passed to this constructor, and the fields or properties to
    /// be set.
    /// </summary>
    public sealed class ObjectConstruction : IObjectConstruction
    {
        private readonly string typeName;
        private readonly ConstructorInfo constructor;
        private readonly ReadOnlyCollection<object> constructorArguments;
        private readonly NamedArgumentsCollection namedArguments;

        
        /// <summary>
        /// Initializes a new type-unsafe <see cref="ObjectConstruction"/>.
        /// </summary>
        /// <param name="typeName">Name of the object type.</param>
        /// <param name="constructorArguments">Arguments passed to the constructor.</param>
        public ObjectConstruction( string typeName, params object[] constructorArguments )
        {
            if ( string.IsNullOrEmpty( typeName ) ) throw new ArgumentNullException( "typeName" );
            this.typeName = typeName;
            this.constructorArguments = new ReadOnlyCollection<object>( constructorArguments ?? new object[0] );
            this.namedArguments = new NamedArgumentsCollection( this );
        }

        /// <summary>
        /// Initializes a new type-safe <see cref="ObjectConstruction"/> from a <see cref="ConstructorInfo"/>.
        /// </summary>
        /// <param name="constructor">Constructor.</param>
        /// <param name="constructorArguments">Arguments passed to the constructor.</param>
        public ObjectConstruction( ConstructorInfo constructor, params object[] constructorArguments )
        {
            // Trivial checks.
            if ( constructor == null ) throw new ArgumentNullException( "constructor" );
            this.constructor = constructor;
            this.typeName = this.constructor.DeclaringType.FullName;
            this.constructorArguments = new ReadOnlyCollection<object>( constructorArguments ?? new object[0] );
            this.namedArguments = new NamedArgumentsCollection( this );

            // Check compatibility of constructor arguments.
            ParameterInfo[] parameters = constructor.GetParameters();
            if ( parameters.Length != this.constructorArguments.Count )
            {
                throw new ArgumentException( "Invalid number of constructor arguments." );
            }

            for ( int i = 0; i < parameters.Length; i++ )
            {
                VerifyValue( string.Format( "constructorArguments[{0}]", i ), parameters[i].ParameterType,
                             constructorArguments[i] );
            }
        }

        private static void VerifyValue( string name, Type type, object value )
        {
            if (value == null)
            {
                if ( type.IsValueType )
                    throw new ArgumentNullException( name );
            }
            else 
            {
                Type expectedType = type;
                Type valueType = value.GetType();

                // If we have enumerations, we are tolerant with conversions between the enumeration
                // and the intrinsic type.
                if (expectedType.IsEnum) expectedType = Enum.GetUnderlyingType(expectedType);
                if (valueType.IsEnum) valueType = Enum.GetUnderlyingType(valueType);


                if (!expectedType.IsAssignableFrom(valueType))
                {
                

                    throw new ArgumentOutOfRangeException( name, string.Format( "Expected value of type '{0}', got '{1}.",
                                                                                type, value.GetType() ) );
                }
            }
        }

        /// <summary>
        /// Gets the custom attribute constructor.
        /// </summary>
        public ConstructorInfo Constructor
        {
            get { return constructor; }
        }

        /// <summary>
        /// Gets the arguments passed to the custom attribute constructor.
        /// </summary>
        public IList<object> ConstructorArguments
        {
            get { return constructorArguments; }
        }

        /// <summary>
        /// Gets the collection of named arguments.
        /// </summary>
        /// <remarks>
        /// This collection is a dictionary associating the name of public a field or property of the
        /// custom attributes to the value that should be assigned to it.
        /// </remarks>
        public IDictionary<string, object> NamedArguments
        {
            get { return namedArguments; }
        }

        private class NamedArgumentsCollection : IDictionary<string, object>
        {
            private readonly ObjectConstruction parent;
            private readonly Dictionary<string, object> dictionary = new Dictionary<string, object>();

            private void VerifyNamedValue( string name, object value )
            {
                if ( this.parent.constructor == null )
                    return;

                Type type;
                PropertyInfo property = this.parent.constructor.DeclaringType.GetProperty( name,
                                                                                           BindingFlags.Public |
                                                                                           BindingFlags.Instance );
                if ( property != null )
                {
                    type = property.PropertyType;
                }
                else
                {
                    FieldInfo field = this.parent.constructor.DeclaringType.GetField( name,
                                                                                      BindingFlags.Public |
                                                                                      BindingFlags.Instance );
                    if ( field != null )
                    {
                        type = field.FieldType;
                    }
                    else
                    {
                        throw new ArgumentException( string.Format( "Cannot find a field or property named {0}.", name ) );
                    }
                }

                VerifyValue( "value", type, value );
            }

            public NamedArgumentsCollection( ObjectConstruction parent )
            {
                this.parent = parent;
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                return this.dictionary.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add( KeyValuePair<string, object> item )
            {
                this.Add( item.Key, item.Value );
            }

            public void Clear()
            {
                this.dictionary.Clear();
            }

            public bool Contains( KeyValuePair<string, object> item )
            {
                return this.dictionary.ContainsKey( item.Key );
            }

            public void CopyTo( KeyValuePair<string, object>[] array, int arrayIndex )
            {
                throw new NotImplementedException();
            }

            public bool Remove( KeyValuePair<string, object> item )
            {
                return this.dictionary.Remove( item.Key );
            }

            public int Count
            {
                get { return this.dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool ContainsKey( string key )
            {
                return this.dictionary.ContainsKey( key );
            }

            public void Add( string key, object value )
            {
                VerifyNamedValue( key, value );
                this.dictionary.Add( key, value );
            }

            public bool Remove( string key )
            {
                return this.dictionary.ContainsKey( key );
            }

            public bool TryGetValue( string key, out object value )
            {
                return this.dictionary.TryGetValue( key, out value );
            }

            public object this[ string key ]
            {
                get { return this.dictionary[key]; }
                set
                {
                    VerifyNamedValue( key, value );
                    this.dictionary[key] = value;
                }
            }

            public ICollection<string> Keys
            {
                get { return this.dictionary.Keys; }
            }

            public ICollection<object> Values
            {
                get { return this.Values; }
            }
        }

        #region Implementation of IObjectConstruction

        /// <inheritdoc />
        public string TypeName
        {
            get { return this.typeName; }
        }

        /// <inheritdoc />
        int IObjectConstruction.ConstructorArgumentCount
        {
            get { return this.constructorArguments.Count; }
        }

        /// <inheritdoc />
        object IObjectConstruction.GetConstructorArgument( int index )
        {
            return this.constructorArguments[index];
        }

        /// <inheritdoc />
        string[] IObjectConstruction.GetPropertyNames()
        {
            string[] names = new string[this.namedArguments.Count];
            this.namedArguments.Keys.CopyTo( names, 0 );
            return names;
        }

        /// <inheritdoc />
        object IObjectConstruction.GetPropertyValue( string name )
        {
            return this.namedArguments[name];
        }

        #endregion
    }
}

#endif