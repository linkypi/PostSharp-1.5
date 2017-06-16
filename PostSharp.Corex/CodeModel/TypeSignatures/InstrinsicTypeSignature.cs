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

#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using PostSharp.CodeModel.Helpers;
using PostSharp.ModuleReader;
using PostSharp.ModuleWriter;
using PostSharp.PlatformAbstraction;

#endregion

namespace PostSharp.CodeModel.TypeSignatures
{
    /// <summary>
    /// Represents an instrinsic type.
    /// </summary>
    public sealed class IntrinsicTypeSignature : TypeSignature
    {
        #region Fields

        /// <summary>
        /// Collection of instances that were already initialized.
        /// </summary>
        private static readonly Dictionary<int, IntrinsicTypeSignature> instancesByIntrinsicType =
            new Dictionary<int, IntrinsicTypeSignature>( 20 );


        /// <summary>
        /// Maps a runtime <see cref="Type"/> to an <see cref="IntrinsicType"/>.
        /// </summary>
        private static readonly Dictionary<Type, IntrinsicTypeSignature> instancesByReflectionType =
            new Dictionary<Type, IntrinsicTypeSignature>( 20 );

        /// <summary>
        /// Maps a reflection type name to an <see cref="IntrinsicType"/>.
        /// </summary>
        private static readonly Dictionary<string, IntrinsicTypeSignature> instancesByReflectionTypeName =
            new Dictionary<string, IntrinsicTypeSignature>( 20 );

        private ModuleDeclaration module;
        private IntrinsicType intrinsic;
        private IntrinsicType signAlternative;
        private Type reflectionType;
        private string name;
        private int size;

        #endregion

        #region Construction

        private IntrinsicTypeSignature()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="IntrinsicType"/>.
        /// </summary>
        /// <param name="name">MSIL name of this type.</param>
        /// <param name="intrinsic">The intrinsic type.</param>
        /// <param name="reflectionType">Corresponding reflection type.</param>
        /// <param name="signAlternative">Signed alternative of <paramref name="intrinsic"/> if it is unsigned, 
        /// or the unsigned alternative if <paramref name="intrinsic"/> is signed, or <paramref name="intrinsic"/>
        /// itself if sign does not apply.</param>
        private static void MakePrototype( IntrinsicType intrinsic, IntrinsicType signAlternative,
                                           string name,
                                           Type reflectionType )
        {
            IntrinsicTypeSignature instance = new IntrinsicTypeSignature
                                                  {
                                                      intrinsic = intrinsic,
                                                      name = name,
                                                      reflectionType = reflectionType,
                                                      signAlternative = signAlternative
                                                  };

            instancesByIntrinsicType.Add( (int) intrinsic, instance );

            if ( reflectionType != null )
            {
                instancesByReflectionType.Add( reflectionType, instance );
                instancesByReflectionTypeName.Add( reflectionType.FullName, instance );
            }
            switch ( intrinsic )
            {
                case IntrinsicType.Boolean:
                    instance.size = sizeof(bool);
                    break;

                case IntrinsicType.Byte:
                case IntrinsicType.SByte:
                    instance.size = sizeof(byte);
                    break;

                case IntrinsicType.Char:
                    instance.size = sizeof(char);
                    break;

                case IntrinsicType.Double:
                    instance.size = sizeof(double);
                    break;

                case IntrinsicType.Int16:
                case IntrinsicType.UInt16:
                    instance.size = sizeof(short);
                    break;

                case IntrinsicType.Int32:
                case IntrinsicType.UInt32:
                    instance.size = sizeof(int);
                    break;

                case IntrinsicType.Int64:
                case IntrinsicType.UInt64:
                    instance.size = sizeof(long);
                    break;

                case IntrinsicType.Single:
                    instance.size = sizeof(float);
                    break;

                case IntrinsicType.IntPtr:
                    instance.size = -2; /* platform-dependent */
                    break;

                case IntrinsicType.UIntPtr:
                    instance.size = -2; /* platform-dependent */
                    break;

                default:
                    instance.size = -1;
                    break;
            }

            return;
        }

        /// <summary>
        /// Creates an instance from the current prototype.
        /// </summary>
        /// <param name="module">Module to which the instance belong.</param>
        /// <returns>An <see cref="IntrinsicTypeSignature"/> belonging to the given module.</returns>
        private IntrinsicTypeSignature CreateInstance( ModuleDeclaration module )
        {
            IntrinsicTypeSignature instance = (IntrinsicTypeSignature) this.MemberwiseClone();
            instance.module = module;
            return instance;
        }

        /// <summary>
        /// Gets the <see cref="IntrinsicType"/> corresponding to a <see cref="Type"/>.
        /// </summary>
        /// <param name="reflectionType">The <see cref="Type"/> to be
        /// mapped to an <see cref="IntrinsicType"/>.</param>
        /// <returns>The <see cref="IntrinsicType"/> corresponding to <paramref name="reflectionType"/>.</returns>
        public static IntrinsicType MapReflectionType( Type reflectionType )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( reflectionType, "reflectionType" );

            #endregion

            IntrinsicTypeSignature instance;
            if ( instancesByReflectionType.TryGetValue( reflectionType, out instance ) )
            {
                return instance.intrinsic;
            }
            else
            {
                throw new ArgumentOutOfRangeException( "reflectionType" );
            }
        }

        /// <summary>
        /// Determines whether a <see cref="Type"/> is intrinsic.
        /// </summary>
        /// <param name="reflectionType"></param>
        /// <returns></returns>
        public static bool IsIntrinsic( Type reflectionType )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( reflectionType, "reflectionType" );

            #endregion

            return instancesByReflectionType.ContainsKey( reflectionType );
        }

        internal static IntrinsicType MapCorElementTypeToIntrinsicType( CorElementType elementType )
        {
            switch ( elementType )
            {
                case CorElementType.Boolean:
                    return IntrinsicType.Boolean;

                case CorElementType.Char:
                    return IntrinsicType.Char;

                case CorElementType.SByte:
                    return IntrinsicType.SByte;

                case CorElementType.Int16:
                    return IntrinsicType.Int16;

                case CorElementType.Int32:
                    return IntrinsicType.Int32;

                case CorElementType.Int64:
                    return IntrinsicType.Int64;

                case CorElementType.IntPtr:
                    return IntrinsicType.IntPtr;

                case CorElementType.Byte:
                    return IntrinsicType.Byte;

                case CorElementType.UInt16:
                    return IntrinsicType.UInt16;

                case CorElementType.UInt32:
                    return IntrinsicType.UInt32;

                case CorElementType.UInt64:
                    return IntrinsicType.UInt64;

                case CorElementType.UIntPtr:
                    return IntrinsicType.UIntPtr;

                case CorElementType.Single:
                    return IntrinsicType.Single;

                case CorElementType.Double:
                    return IntrinsicType.Double;

                case CorElementType.String:
                    return IntrinsicType.String;

                case CorElementType.Object:
                    return IntrinsicType.Object;

                case CorElementType.Void:
                    return IntrinsicType.Void;

                case CorElementType.TypedByRef:
                    return IntrinsicType.TypedReference;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( elementType,
                                                                                  "element type in type signature" );
            }
        }

        internal static CorElementType MapIntrinsicTypeToCorElementType( IntrinsicType intrinsicType )
        {
            switch ( intrinsicType )
            {
                case IntrinsicType.Boolean:
                    return CorElementType.Boolean;

                case IntrinsicType.Char:
                    return CorElementType.Char;

                case IntrinsicType.SByte:
                    return CorElementType.SByte;

                case IntrinsicType.Int16:
                    return CorElementType.Int16;

                case IntrinsicType.Int32:
                    return CorElementType.Int32;

                case IntrinsicType.Int64:
                    return CorElementType.Int64;

                case IntrinsicType.IntPtr:
                    return CorElementType.IntPtr;

                case IntrinsicType.Byte:
                    return CorElementType.Byte;

                case IntrinsicType.UInt16:
                    return CorElementType.UInt16;

                case IntrinsicType.UInt32:
                    return CorElementType.UInt32;

                case IntrinsicType.UInt64:
                    return CorElementType.UInt64;

                case IntrinsicType.UIntPtr:
                    return CorElementType.UIntPtr;

                case IntrinsicType.Single:
                    return CorElementType.Single;

                case IntrinsicType.Double:
                    return CorElementType.Double;

                case IntrinsicType.String:
                    return CorElementType.String;

                case IntrinsicType.Object:
                    return CorElementType.Object;

                case IntrinsicType.Void:
                    return CorElementType.Void;

                case IntrinsicType.TypedReference:
                    return CorElementType.TypedByRef;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( intrinsicType,
                                                                                  "element type in type signature" );
            }
        }

        /// <summary>
        /// Initializes the <see cref="IntrinsicTypeSignature"/> type.
        /// </summary>
        [SuppressMessage( "Microsoft.Performance", "CA1810",
            Justification = "The type cannot be initialized at declaration." )]
        static IntrinsicTypeSignature()
        {
            MakePrototype( IntrinsicType.TypedReference, IntrinsicType.TypedReference, "typedref",
                           typeof(TypedReference) );
            MakePrototype( IntrinsicType.Char, IntrinsicType.Char, "char", typeof(char) );
            MakePrototype( IntrinsicType.Void, IntrinsicType.Void, "void", typeof(void) );
            MakePrototype( IntrinsicType.Boolean, IntrinsicType.Void, "bool", typeof(bool) );
            MakePrototype( IntrinsicType.SByte, IntrinsicType.Byte, "int8", typeof(sbyte) );
            MakePrototype( IntrinsicType.Int16, IntrinsicType.UInt16, "int16", typeof(short) );
            MakePrototype( IntrinsicType.Int32, IntrinsicType.UInt32, "int32", typeof(int) );
            MakePrototype( IntrinsicType.Int64, IntrinsicType.UInt64, "int64", typeof(long) );
            MakePrototype( IntrinsicType.Single, IntrinsicType.Single, "float32", typeof(float) );
            MakePrototype( IntrinsicType.Double, IntrinsicType.Double, "float64", typeof(double) );
            MakePrototype( IntrinsicType.Byte, IntrinsicType.SByte, "uint8", typeof(byte) );
            MakePrototype( IntrinsicType.UInt16, IntrinsicType.Int16, "uint16", typeof(ushort) );
            MakePrototype( IntrinsicType.UInt32, IntrinsicType.Int32, "uint32", typeof(uint) );
            MakePrototype( IntrinsicType.UInt64, IntrinsicType.Int64, "uint64", typeof(ulong) );
            MakePrototype( IntrinsicType.IntPtr, IntrinsicType.UIntPtr, "native int", typeof(IntPtr) );
            MakePrototype( IntrinsicType.UIntPtr, IntrinsicType.IntPtr, "native uint", typeof(UIntPtr) );
            MakePrototype( IntrinsicType.NativeReal, IntrinsicType.NativeReal, "native float", null );
            MakePrototype( IntrinsicType.Object, IntrinsicType.Object, "object", typeof(object) );
            MakePrototype( IntrinsicType.String, IntrinsicType.String, "string", typeof(string) );
        }


        /// <summary>
        /// Gets an <see cref="IntrinsicTypeSignature"/> instance given a reflection <see cref="Type"/>.
        /// </summary>
        /// <param name="reflectionType">An instrinsic runtime <see cref="Type"/>.</param>
        /// <param name="module">Module for which the instance should be valid.</param>
        /// <returns>The <see cref="IntrinsicTypeSignature"/> corresponding to <paramref name="reflectionType"/>.</returns>
        /// <remarks>
        /// User code should use the method <see cref="DeclarationCache"/>.<see cref="DeclarationCache.GetIntrinsic(Type)"/>
        /// instead of this one.
        /// </remarks>
        public static IntrinsicTypeSignature GetInstance( Type reflectionType, ModuleDeclaration module )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( reflectionType, "reflectionType" );
            ExceptionHelper.AssertArgumentNotNull( module, "module" );

            #endregion

            return instancesByReflectionType[reflectionType].CreateInstance( module );
        }


        /// <summary>
        /// Gets an instance of a <see cref="IntrinsicTypeSignature"/> for a given module.
        /// </summary>
        /// <param name="intrinsic"><see cref="IntrinsicType"/> for which the type signature is required.</param>
        /// <param name="module">Module in which the instance should be valid.</param>
        /// <returns>An <see cref="IntrinsicTypeSignature"/>.</returns>
        public static IntrinsicTypeSignature GetInstance( IntrinsicType intrinsic, ModuleDeclaration module )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( module, "module" );

            #endregion

            return GetPrototype( intrinsic ).CreateInstance( module );
        }

        private static IntrinsicTypeSignature GetPrototype( IntrinsicType intrinsic )
        {
            return instancesByIntrinsicType[(int) intrinsic];
        }

        internal static IntrinsicTypeSignature GetInstance( CorElementType elementType, ModuleDeclaration module )
        {
            return GetInstance( MapCorElementTypeToIntrinsicType( elementType ), module );
        }

        #endregion

        /// <summary>
        /// Gets the instrinsic type.
        /// </summary>
        public IntrinsicType IntrinsicType
        {
            get { return this.intrinsic; }
        }

        #region TypeSignature implementation

        internal override ITypeSignature GetNakedType( TypeNakingOptions options )
        {
            return this;
        }

        internal override bool InternalIsAssignableTo( ITypeSignature signature, GenericMap genericMap,
                                                       IsAssignableToOptions options )
        {
            IntrinsicTypeSignature intrinsicTypeSignature = signature as IntrinsicTypeSignature;

            if ( intrinsicTypeSignature != null )
            {
                if ( this.intrinsic == intrinsicTypeSignature.intrinsic )
                {
                    return true;
                }

                if ( this.signAlternative == intrinsicTypeSignature.intrinsic &&
                     Platform.Current.IntrinsicOfOppositeSignAssignable )
                {
                    return true;
                }

                if ( intrinsicTypeSignature.intrinsic == IntrinsicType.Object &&
                     this.intrinsic == IntrinsicType.String )
                {
                    return true;
                }
            }

            if ( signature.BelongsToClassification( TypeClassifications.Enum ) )
            {
                return this.InternalIsAssignableTo( EnumHelper.GetUnderlyingType( (IType) signature ), genericMap,
                                                    options );
            }

            return false;
        }


        /// <inheritdoc />
        public override NullableBool BelongsToClassification( TypeClassifications typeClassification )
        {
            switch ( typeClassification )
            {
                case TypeClassifications.Any:
                case TypeClassifications.Intrinsic:
                case TypeClassifications.Signature:
                    return true;

                case TypeClassifications.ReferenceType:
                    return this.intrinsic == IntrinsicType.Object ||
                           this.intrinsic == IntrinsicType.String;

                case TypeClassifications.ValueType:
                    return !( this.intrinsic == IntrinsicType.Object ||
                              this.intrinsic == IntrinsicType.String );

                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public override bool ContainsGenericArguments()
        {
            return false;
        }

        /// <inheritdoc />
        public override ITypeSignature MapGenericArguments( GenericMap genericMap )
        {
            return this;
        }

        /// <inheritdoc />
        public override ITypeSignature ElementType
        {
            get { return null; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.name;
        }

        /// <inheritdoc />
        public override int GetValueSize( PlatformInfo platform )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( platform, "platform" );

            #endregion

            return this.size != -2 ? this.size : platform.NativePointerSize;
        }

        /// <inheritdoc />
        internal override void InternalWriteILReference( ILWriter writer, GenericMap genericMap,
                                                         WriteTypeReferenceOptions options )
        {
            writer.WriteKeyword( this.name );
        }


        /// <inheritdoc />
        public override Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.reflectionType;
        }

        /// <inheritdoc />
        public override Type GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.reflectionType;
        }

        /// <inheritdoc />
        public override void WriteReflectionTypeName( StringBuilder stringBuilder, ReflectionNameOptions options )
        {
            ReflectionNameOptions encoding = options & ReflectionNameOptions.EncodingMask;

            /*
             *  (encoding == ReflectionNameOptions.NormalEncoding||
                 ( ( this.intrinsic == IntrinsicType.Object ||
                     this.intrinsic == IntrinsicType.String ) &&
                   (encoding & ReflectionNameOptions.EncodingMask) != ReflectionNameOptions.GenericArgumentEncoding))
                && (encoding & ReflectionNameOptions.SkipNamespace) == 0
             */

            if ( ( options & ReflectionNameOptions.SkipNamespace ) != 0 ||
                 ( encoding == ReflectionNameOptions.MethodParameterEncoding && this.intrinsic != IntrinsicType.Object &&
                   this.intrinsic != IntrinsicType.String ) ||
                 encoding == ReflectionNameOptions.GenericArgumentEncoding )
            {
                stringBuilder.Append( this.reflectionType.Name );
            }
            else
            {
                stringBuilder.Append( this.reflectionType.FullName );
            }


            if ( ( options & ReflectionNameOptions.UseAssemblyName ) != 0 )
            {
                stringBuilder.Append( ", " );
                stringBuilder.Append( this.module.FindMscorlib().FullName );
            }
        }

        /// <inheritdoc />
        public override ModuleDeclaration Module
        {
            get { return this.module; }
        }

        /// <inheritdoc />
        public override ITypeSignature Translate( ModuleDeclaration targetModule )
        {
            if ( targetModule == this.module )
            {
                return this;
            }
            else
            {
                return this.CreateInstance( targetModule );
            }
        }

        #endregion

        /// <summary>
        /// Determines whether a type signature is a given intrinsic.
        /// </summary>
        /// <param name="typeSignature">A type signature.</param>
        /// <param name="intrinsic">An intrinsic.</param>
        /// <returns><b>true</b> if <paramref name="typeSignature"/> is the given intrinsic,
        /// otherwise <b>false</b>.</returns>
        public static bool Is( ITypeSignature typeSignature, IntrinsicType intrinsic )
        {
            TypeSpecDeclaration typeSpec = typeSignature as TypeSpecDeclaration;
            if ( typeSpec != null ) typeSignature = typeSpec.Signature;

            IntrinsicTypeSignature intrinsicTypeSignature = typeSignature as IntrinsicTypeSignature;
            return intrinsicTypeSignature != null && intrinsicTypeSignature.intrinsic == intrinsic;
        }

        internal override bool InternalEquals( ITypeSignature reference, bool isReference )
        {
            IntrinsicTypeSignature referenceAsIntrinsic = reference as IntrinsicTypeSignature;
            if ( referenceAsIntrinsic == null )
                return false;
            return this.intrinsic == referenceAsIntrinsic.intrinsic;
        }

        /// <inheritdoc />
        public override int GetCanonicalHashCode()
        {
            return (int) this.intrinsic;
        }
    }
}