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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using PostSharp.CodeModel.SerializationTypes;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;
using PostSharp.ModuleReader;
using PostSharp.Utilities;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Caches some frequently-used declarations or constructions with lazy loading.
    /// </summary>
    /// <remarks>
    /// <para>This cache can serve three types of content: types, intrinsic types
    /// and arbitrary content served by providers. The last mechanism is used
    /// to cache methods (which are difficult to reference otherwise).
    /// </para>
    /// <para>
    /// The current class provides itself the contents whose key is declared here
    /// (see static fields of this class).
    /// </para>
    /// </remarks>
    public sealed class DeclarationCache
    {
        private const int maxGenericParameterNumber = 256;
        private readonly ModuleDeclaration module;
        private readonly Dictionary<object, object> cache = new Dictionary<object, object>();

        private readonly Dictionary<IntrinsicType, IntrinsicTypeSignature> intrinsicTypes =
            new Dictionary<IntrinsicType, IntrinsicTypeSignature>();

        private readonly Dictionary<IntrinsicType, INamedType> boxedIntrinsicTypes =
            new Dictionary<IntrinsicType, INamedType>();

        private readonly Dictionary<IntrinsicType, IntrinsicSerializationType> intrinsicSerializationTypes =
            new Dictionary<IntrinsicType, IntrinsicSerializationType>();

        private readonly Dictionary<int, GenericParameterTypeSignature> genericTypeParameters =
            new Dictionary<int, GenericParameterTypeSignature>(maxGenericParameterNumber);

        private readonly Dictionary<int, GenericParameterTypeSignature> genericMethodParameters =
            new Dictionary<int, GenericParameterTypeSignature>(maxGenericParameterNumber);

        /// <summary>
        /// Gets an instance of <see cref="TaggedObjectSerializationType"/> that is valid for the current module.
        /// </summary>
        public readonly TaggedObjectSerializationType TaggedObjectSerializationType;

        /// <summary>
        /// Gets an instance of <see cref="TypeSerializationType"/> that is valid for the current module.
        /// </summary>
        public readonly TypeSerializationType TypeSerializationType;

        /// <summary>
        /// Gets a <see cref="GenericMap"/> mapping generic parameters on themselves, valid for the
        /// current module.
        /// </summary>
        public readonly GenericMap IdentityGenericMap;


        internal DeclarationCache( ModuleDeclaration module )
        {
            this.module = module;
            this.AddIntrinsic( IntrinsicType.Boolean, true );
            this.AddIntrinsic( IntrinsicType.Byte, true );
            this.AddIntrinsic( IntrinsicType.Char, true );
            this.AddIntrinsic( IntrinsicType.Double, true );
            this.AddIntrinsic( IntrinsicType.Int16, true );
            this.AddIntrinsic( IntrinsicType.Int32, true );
            this.AddIntrinsic( IntrinsicType.Int64, true );
            this.AddIntrinsic( IntrinsicType.IntPtr, false );
            this.AddIntrinsic( IntrinsicType.NativeReal, false );
            this.AddIntrinsic( IntrinsicType.Object, false );
            this.AddIntrinsic( IntrinsicType.SByte, true );
            this.AddIntrinsic( IntrinsicType.Single, true );
            this.AddIntrinsic( IntrinsicType.String, true );
            this.AddIntrinsic( IntrinsicType.TypedReference, false );
            this.AddIntrinsic( IntrinsicType.UInt16, true );
            this.AddIntrinsic( IntrinsicType.UInt32, true );
            this.AddIntrinsic( IntrinsicType.UInt64, true );
            this.AddIntrinsic( IntrinsicType.UIntPtr, false );
            this.AddIntrinsic( IntrinsicType.Void, false );

            this.TaggedObjectSerializationType = TaggedObjectSerializationType.GetInstance( this.module );
            this.TypeSerializationType = TypeSerializationType.GetInstance( this.module );

            for (int i = 0; i < maxGenericParameterNumber; i++)
            {
                this.genericTypeParameters.Add( i,
                                                GenericParameterTypeSignature.GetInstance( this.module, i,
                                                                                           GenericParameterKind.Type ) );
                this.genericMethodParameters.Add( i,
                                                  GenericParameterTypeSignature.GetInstance( this.module, i,
                                                                                             GenericParameterKind.Method ) );
            }

            this.IdentityGenericMap = new GenericMap(
                this.GetGenericParameterArray(maxGenericParameterNumber, GenericParameterKind.Type),
                this.GetGenericParameterArray(maxGenericParameterNumber, GenericParameterKind.Method));

            MethodBaseGetMethodFromHandle
                =
                delegate
                    {
                        return this.module.FindMethod( "System.Reflection.MethodBase, mscorlib", "GetMethodFromHandle",
                                                       1 );
                    };

            MethodBaseGetMethodFromHandle2
                =
                delegate
                    {
                        return this.module.FindMethod( "System.Reflection.MethodBase, mscorlib", "GetMethodFromHandle",
                                                       2 );
                    };

            FieldInfoGetFieldFromHandle
                =
                delegate { return this.module.FindMethod( "System.Reflection.FieldInfo, mscorlib", "GetFieldFromHandle", 1 ); };

            FieldInfoGetFieldFromHandle2 = delegate
                                               {
                                                   return module.FindMethod( "System.Reflection.FieldInfo, mscorlib",
                                                                             "GetFieldFromHandle", 2 );
                                               };

            TypeGetTypeFromHandle
                = delegate { return this.module.FindMethod( "System.Type, mscorlib", "GetTypeFromHandle", 1 ); };
        }

        private void AddIntrinsic( IntrinsicType intrinsic, bool serializable )
        {
            IntrinsicTypeSignature typeSignature = IntrinsicTypeSignature.GetInstance( intrinsic, this.module );
            intrinsicTypes.Add( intrinsic, typeSignature );

            if ( serializable )
            {
                intrinsicSerializationTypes.Add( intrinsic,
                                                 IntrinsicSerializationType.GetInstance( this.module, intrinsic ) );
            }
        }

        /// <summary>
        /// Gets a <see cref="GenericParameterTypeSignature"/>.
        /// </summary>
        /// <param name="ordinal">Ordinal (index) of the generic parameter.</param>
        /// <param name="kind">Kind (<see cref="GenericParameterKind.Type"/> or <see cref="GenericParameterKind.Method"/>
        /// of this generic parameter.</param>
        /// <returns>A <see cref="GenericParameterTypeSignature"/> with the given <paramref name="ordinal"/>
        /// and <paramref name="kind"/>, valid in the current module.</returns>
        public GenericParameterTypeSignature GetGenericParameter( int ordinal, GenericParameterKind kind )
        {
            switch ( kind )
            {
                case GenericParameterKind.Method:
                    return this.genericMethodParameters[ordinal];

                case GenericParameterKind.Type:
                    return this.genericTypeParameters[ordinal];

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( kind, "kind" );
            }
        }

        /// <summary>
        /// Creates an array of <see cref="GenericParameterTypeSignature"/> with incremental
        /// ordinals.
        /// </summary>
        /// <param name="length">Array lenght.</param>
        /// <param name="kind">Kind of generic parameter.</param>
        /// <returns>An array of <see cref="ITypeSignature"/> that can be used to construct
        /// a generic type or method instance with generic parameters like 
        /// <c>&lt;!0, !1, ..., !<paramref name="length"/>&gt;</c>
        /// or <c>&lt;!!0, !!1, ..., !!<paramref name="length"/>&gt;</c>
        /// </returns>
        public IList<ITypeSignature> GetGenericParameterArray( int length, GenericParameterKind kind )
        {
            ITypeSignature[] array = new ITypeSignature[length];
            for ( int i = 0; i < length; i++ )
            {
                array[i] = this.GetGenericParameter( i, kind );
            }

            return array;
        }

        /// <summary>
        /// Gets a type (<see cref="ITypeSignature"/>) given a reflection type 
        /// (<see cref="Type"/>), with default binding options.
        /// </summary>
        /// <param name="type">The reflection type.</param>
        /// <returns>An <see cref="ITypeSignature"/>.</returns>
        /// <remarks>
        /// <para>
        /// You are required to use the second overload and to specify <see cref="BindingOptions"/>
        /// when you require a generic type.
        /// </para>
        /// <para>If <paramref name="type"/> is a primitive type, the method returns
        /// the class representing it, i.e. it does <i>not</i> return the intrinsic
        /// type. Intrinsic type substitution is allowed for type constructions.
        /// </para>
        /// </remarks>
        public ITypeSignature GetType( Type type )
        {
            return this.GetType( type, BindingOptions.Default );
        }

        /// <summary>
        /// Gets a type (<see cref="ITypeSignature"/>) given a reflection type 
        /// (<see cref="Type"/>), but specifies <see cref="BindingOptions"/>.
        /// </summary>
        /// <param name="type">The reflection type.</param>
        /// <param name="bindingOptions">Binding options. Specify <see cref="BindingOptions.RequireGenericInstance"/>
        /// or <see cref="BindingOptions.RequireGenericDefinition"/> when requesting a generic type.</param>
        /// <returns>An <see cref="ITypeSignature"/>.</returns>
        /// <remarks>
        /// If <paramref name="type"/> is a primitive type, the method returns
        /// the class representing it, i.e. it does <i>not</i> return the intrinsic
        /// type. Intrinsic type substitution is allowed for type constructions.
        /// </remarks>
        public ITypeSignature GetType( Type type, BindingOptions bindingOptions )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );

            #endregion

            Pair<Type, BindingOptions> cacheKey = new Pair<Type, BindingOptions>( type, bindingOptions );

            object value;
            if ( !this.cache.TryGetValue( cacheKey, out value ) )
            {
                value = this.module.FindType(type, bindingOptions);

//                if ( !type.HasElementType && !type.IsGenericType )
//                {
//                    value = this.module.FindType( type, bindingOptions | BindingOptions.DisallowIntrinsicSubstitution );
//                }
//                else
//                {
//                    value = this.module.FindType( type, bindingOptions );
//                }

                this.cache.Add( cacheKey, value );
            }

            return (ITypeSignature) value;
        }

        /// <summary>
        /// Gets a type (<see cref="ITypeSignature"/>) given its name.
        /// </summary>
        /// <param name="typeName">The reflection type.</param>
        /// <returns>An <see cref="ITypeSignature"/>.</returns>
        public ITypeSignature GetType( string typeName )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotEmptyOrNull( typeName, "typeName" );

            #endregion

            object value;
            if ( !this.cache.TryGetValue( typeName, out value ) )
            {
                value = this.module.FindType( typeName, BindingOptions.Default );


                this.cache.Add( typeName, value );
            }

            return (ITypeSignature) value;
        }

        /// <summary>
        /// Clear the cache content.
        /// </summary>
        /// <remarks>
        /// This obviously does not clear the list of providers.
        /// </remarks>
        public void Clear()
        {
            this.cache.Clear();
        }

        /// <summary>
        /// Gets a declaration served by a provider.
        /// </summary>
        /// <param name="getter">Method providing the cached item</param>
        /// <returns>The cached item.</returns>
        /// <remarks>
        /// The <paramref name="getter"/> parameter is used both as the item key
        /// (identification of the requested item) and in order to get the value in
        /// case it is not yet in cache. The item cache is not strictly the passed
        /// delegate, but only its <i>method</i>, i.e. the object instance is ignored.
        /// </remarks>
        public T GetItem<T>( Function<T> getter )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( getter, "getter" );

            #endregion

            object value;
            if ( !this.cache.TryGetValue( getter.Method, out value ) )
            {
                value = getter();
                this.cache.Add( getter.Method, value );
            }

            return (T) value;
        }

        /// <summary>
        /// Gets an <see cref="IntrinsicTypeSignature"/>, given an <see cref="IntrinsicType"/>.
        /// </summary>
        /// <param name="intrinsic">An intrinsic.</param>
        /// <returns>The <see cref="IntrinsicTypeSignature"/> corresponding to <paramref name="intrinsic"/>.</returns>
        public IntrinsicTypeSignature GetIntrinsic( IntrinsicType intrinsic )
        {
            return this.intrinsicTypes[intrinsic];
        }

        /// <summary>
        /// Gets an <see cref="IntrinsicTypeSignature"/>, given a reflection <see cref="Type"/>.
        /// </summary>
        /// <param name="reflectionType">A reflection <see cref="Type"/> corresponding to an intrinsic.</param>
        /// <returns>The <see cref="IntrinsicTypeSignature"/> corresponding to <paramref name="reflectionType"/>.</returns>
        public IntrinsicTypeSignature GetIntrinsic( Type reflectionType )
        {
            return this.intrinsicTypes[IntrinsicTypeSignature.MapReflectionType( reflectionType )];
        }


        /// <summary>
        /// Gets an <see cref="IntrinsicSerializationType"/>, given an <see cref="IntrinsicType"/>.
        /// </summary>
        /// <param name="intrinsic">An intrinsic.</param>
        /// <returns>The <see cref="IntrinsicSerializationType"/> corresponding to <paramref name="intrinsic"/>.</returns>
        public IntrinsicSerializationType GetIntrinsicSerializationType( IntrinsicType intrinsic )
        {
            return this.intrinsicSerializationTypes[intrinsic];
        }

        internal IntrinsicSerializationType GetIntrinsicSerializationType( CorElementType elementType )
        {
            return
                this.intrinsicSerializationTypes[IntrinsicTypeSignature.MapCorElementTypeToIntrinsicType( elementType )];
        }

        /// <summary>
        /// Gets the boxed type of an intrinsic.
        /// </summary>
        /// <param name="type">The instrinsic type.</param>
        /// <returns>The boxed type corresponding to the intrinsic type.</returns>
        /// <example><c>GetIntrinsicBoxedType(IntrinsicType.Object)</c> returns <see cref="System.Object"/>.</example>
        public INamedType GetIntrinsicBoxedType( IntrinsicType type )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );

            #endregion

            INamedType boxedType;
            if ( !this.boxedIntrinsicTypes.TryGetValue( type, out boxedType ) )
            {
                boxedType =
                    (INamedType)
                    module.FindType( this.GetIntrinsic( type ).GetSystemType( null, null ),
                                     BindingOptions.Default | BindingOptions.DisallowIntrinsicSubstitution );
                this.boxedIntrinsicTypes.Add( type, boxedType );
            }

            return boxedType;
        }


        /// <summary>
        /// Callback method for the <see cref="GetItem{IMethod}"/> method, retrieving the
        /// <see cref="Type"/>.<see cref="Type.GetTypeFromHandle(RuntimeTypeHandle)"/> method.
        /// </summary>
        [SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )] public readonly
            Function<IMethod> TypeGetTypeFromHandle;


        /// <summary>
        /// Callback method for the <see cref="GetItem{IMethod}"/> method, retrieving the
        /// <see cref="MethodBase"/>.<see cref="MethodBase.GetMethodFromHandle(RuntimeMethodHandle)"/> method.
        /// </summary>
        [SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )] public readonly
            Function<IMethod> MethodBaseGetMethodFromHandle;

        /// <summary>
        /// Callback method for the <see cref="GetItem{IMethod}"/> method, retrieving the
        /// <see cref="Type"/>.<see cref="MethodBase.GetMethodFromHandle(RuntimeMethodHandle,RuntimeTypeHandle)"/> method.
        /// </summary>
        [SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )] public readonly
            Function<IMethod> MethodBaseGetMethodFromHandle2;

        /// <summary>
        /// Callback method for the <see cref="GetItem{IMethod}"/> method, retrieving the
        /// <see cref="FieldInfo"/>.<see cref="FieldInfo.GetFieldFromHandle(RuntimeFieldHandle)"/> method.
        /// </summary>
        [SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )] public readonly
            Function<IMethod> FieldInfoGetFieldFromHandle;

        /// <summary>
        /// Callback method for the <see cref="GetItem{IMethod}"/> method, retrieving the
        /// <see cref="FieldInfo"/>.<see cref="FieldInfo.GetFieldFromHandle(RuntimeFieldHandle,RuntimeTypeHandle)"/> method.
        /// </summary>
        [SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )] public readonly
            Function<IMethod> FieldInfoGetFieldFromHandle2;
    }
}