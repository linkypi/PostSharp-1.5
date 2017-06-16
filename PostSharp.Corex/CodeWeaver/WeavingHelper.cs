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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Collections;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;
using PostSharp.ModuleWriter;
using PostSharp.Reflection;

namespace PostSharp.CodeWeaver
{
    /// <summary>
    /// Provides methods that generate frequently used snippets of IL instructions.
    /// </summary>
    public sealed class WeavingHelper
    {
        private readonly ModuleDeclaration module;
        private IMethod type_GetField;
        private IMethod reflectionHelper_GetMethod;
        private IMethod console_WriteLine;
        private IMethod debuggerNonUserCodeAttributeConstructor;
        private IMethod compilerGeneratedAttributeConstructor;
        private bool compilerGeneratedAttributeConstructorCached;

        /// <summary>
        /// Initializes a new <see cref="WeavingHelper"/>.
        /// </summary>
        /// <param name="module">Module into which instructions shall be generated.</param>
        public WeavingHelper( ModuleDeclaration module )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( module, "module" );

            #endregion

            this.module = module;
        }

        #region Casting

        /// <summary>
        /// Emits MSIL instructions that cast an object of any type to an object
        /// (eventually boxed).
        /// </summary>
        /// <param name="originalType">Type to be converted.</param>
        /// <param name="writer"><see cref="InstructionEmitter"/> to which instructions
        /// have to be written.</param>
        public void ToObject( ITypeSignature originalType, InstructionEmitter writer )
        {
            ITypeSignature type = originalType.GetNakedType( TypeNakingOptions.IgnoreModifiers );
            PointerTypeSignature pointerType = type as PointerTypeSignature;

            IntrinsicTypeSignature intrinsicType = null;

            // If we got a managed pointer, dereference it.
            if ( pointerType != null )
            {
                type = pointerType.ElementType;
                writer.EmitInstructionLoadIndirect( type );
            }

            // Box the type if it is a value type.
            if ( type is GenericParameterDeclaration )
            {
                writer.EmitInstructionType( OpCodeNumber.Box, type );
            }
            else
            {
                GenericParameterTypeSignature genericParameterTypeSignature =
                    type as GenericParameterTypeSignature;

                if ( genericParameterTypeSignature != null )
                {
                    writer.EmitInstructionType( OpCodeNumber.Box, this.module.TypeSpecs.GetBySignature( type, true ) );
                }
                else if ( type.BelongsToClassification( TypeClassifications.ValueType ) )
                {
                    if ( intrinsicType == null )
                    {
                        intrinsicType = type as IntrinsicTypeSignature;
                    }

                    if ( intrinsicType != null )
                    {
                        type = this.module.Cache.GetIntrinsicBoxedType( intrinsicType.IntrinsicType );
                    }

                    writer.EmitInstructionType( OpCodeNumber.Box, type );
                }
            }
        }

        /// <summary>
        /// Emits MSIL instructions that cast an object (of type <see cref="object"/>)
        /// to a given type, performing unboxing when needed.
        /// </summary>
        /// <param name="targetType">Type into which the object has to be converted.</param>
        /// <param name="writer"><see cref="InstructionEmitter"/> into which instructions
        /// have to be written.</param>
        public void FromObject( ITypeSignature targetType, InstructionEmitter writer )
        {
            ITypeSignature type = targetType.GetNakedType( TypeNakingOptions.IgnoreModifiers );

            PointerTypeSignature pointerType = type as PointerTypeSignature;

            if ( pointerType != null )
            {
                type = pointerType.ElementType;
            }

            IntrinsicTypeSignature intrinsicType = type as IntrinsicTypeSignature;

            // Unbox the type if it is a value type.
            if ( type is GenericParameterTypeSignature ||
                 type is GenericParameterDeclaration )
            {
                writer.EmitInstructionType( OpCodeNumber.Unbox_Any, type );
            }
            else if ( type.BelongsToClassification( TypeClassifications.ValueType ) )
            {
                ITypeSignature boxedType;
                if ( intrinsicType != null )
                {
                    boxedType = this.module.Cache.GetIntrinsicBoxedType( intrinsicType.IntrinsicType );
                }
                else
                {
                    boxedType = type;
                }

                writer.EmitInstructionType( OpCodeNumber.Unbox, boxedType );
                writer.EmitInstructionType( OpCodeNumber.Ldobj, boxedType );
            }
            else if ( IntrinsicTypeSignature.Is( type, IntrinsicType.Object ) )
            {
                // Nothing to do.
            }
            else
            {
                if ( intrinsicType != null )
                {
                    writer.EmitInstructionType( OpCodeNumber.Castclass,
                                                this.module.Cache.GetIntrinsicBoxedType( intrinsicType.IntrinsicType ) );
                }
                else
                {
                    writer.EmitInstructionType( OpCodeNumber.Castclass, type );
                }
            }
        }

        #endregion

        #region Array of Arguments

        /// <summary>
        /// Emit instructions that create a new array of objects (object[]) and fills it with all input arguments,
        /// but allows the first arguments not to be included in this array.
        /// </summary>
        /// <param name="method">Method for which arguments have to be read.</param>
        /// <param name="writer">An <see cref="InstructionEmitter"/>.</param>
        /// <remarks>
        /// Stack transition: ... -> array, ... .
        /// </remarks>
        public void MakeArrayOfArguments( MethodDefDeclaration method, InstructionEmitter writer )
        {
            this.MakeArrayOfArguments( method, writer, 0 );
        }

        /// <summary>
        /// Emit instructions that create a new array of objects (object[]) and fills it with all input arguments,
        /// but allows the first arguments not to be included in this array,
        /// </summary>
        /// <param name="method">Method for which arguments have to be read.</param>
        /// <param name="writer">An <see cref="InstructionEmitter"/>.</param>
        /// <param name="firstArgument">Ordinal of the first argument to be loaded into the array.</param>
        /// <remarks>
        /// Stack transition: ... -> array, ... .
        /// </remarks>
        public void MakeArrayOfArguments( MethodDefDeclaration method, InstructionEmitter writer, int firstArgument )
        {
            this.MakeArrayOfArguments( method, writer, firstArgument, method.Parameters.Count );
        }

        /// <summary>
        /// Emit instructions that create a new array of objects (object[]) and fills it with all input arguments,
        /// but allows the first arguments and last arguments not to be included in this array,
        /// </summary>
        /// <param name="method">Method for which arguments have to be read.</param>
        /// <param name="writer">An <see cref="InstructionEmitter"/>.</param>
        /// <param name="firstArgument">Ordinal of the first argument to be loaded into the array.</param>
        /// <param name="argumentCount">Number of arguments to be loaded into the array.</param>
        /// <remarks>
        /// Stack transition: ... -> array, ... .
        /// </remarks>
        public void MakeArrayOfArguments( MethodDefDeclaration method, InstructionEmitter writer, int firstArgument,
                                          int argumentCount )
        {
            int thisShift = ( method.Attributes & MethodAttributes.Static ) != 0 ? 0 : 1;

            if ( argumentCount > 0 )
            {
                // Create the array
                writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, argumentCount - firstArgument );
                writer.EmitInstructionType( OpCodeNumber.Newarr,
                                            this.module.Cache.GetIntrinsicBoxedType( IntrinsicType.Object ) );

                // Set into the array all arguments that have an input value.
                for ( int i = firstArgument; i < argumentCount; i++ )
                {
                    ParameterDeclaration parameter = method.Parameters[i];

                    if ( !parameter.ParameterType.BelongsToClassification( TypeClassifications.Pointer )
                         || ( parameter.Attributes & (ParameterAttributes) 3 ) != ParameterAttributes.Out )
                    {
                        writer.EmitInstruction( OpCodeNumber.Dup );
                        writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, i - firstArgument );
                        writer.EmitInstructionInt16( OpCodeNumber.Ldarg, (short) ( i + thisShift ) );

                        this.ToObject( parameter.ParameterType, writer );

                        // Next stack transition: ..., array, index, value --> ...,
                        writer.EmitInstruction( OpCodeNumber.Stelem_Ref );
                    }
                }
            }
            else
            {
                writer.EmitInstruction( OpCodeNumber.Ldnull );
            }
        }

        /// <summary>
        /// Emit instructions that create a new array of objects (object[]) and fills it with all input arguments.
        /// </summary>
        /// <param name="method">Method for which arguments have to be read.</param>
        /// <param name="writer">An <see cref="InstructionEmitter"/>.</param>
        /// <param name="arrayVariable">Local variable of type <c>object[]</c> that contains the array of arguments.</param>
        /// <remarks>
        /// Stack transition: ... -> array, ... .
        /// </remarks>
        public void CopyArgumentsFromArray( LocalVariableSymbol arrayVariable,
                                            MethodDefDeclaration method, InstructionEmitter writer )
        {
            this.CopyArgumentsFromArray( arrayVariable, method, writer, 0 );
        }

        /// <summary>
        /// Emits instruction that write the output arguments from the value of an array of arguments (object[]).
        /// </summary>
        /// <param name="method">Method for which arguments have to be written.</param>
        /// <param name="writer">An <see cref="InstructionEmitter"/>.</param>
        /// <param name="arrayVariable">Local variable of type <c>object[]</c> that contains the array of arguments.</param>
        /// <param name="firstArgument">Ordinal of the argument corresponding to the first (0) position in the array.</param>
        /// <remarks>
        /// Stack transition: ..., array -> ... .
        /// </remarks>
        public void CopyArgumentsFromArray( LocalVariableSymbol arrayVariable,
                                            MethodDefDeclaration method, InstructionEmitter writer, int firstArgument )
        {
            int n = method.Parameters.Count;
            int thisShift = ( method.Attributes & MethodAttributes.Static ) != 0 ? 0 : 1;

            if ( n > 0 )
            {
                // Set into the array all arguments that have an input value.
                for ( int i = firstArgument; i < method.Parameters.Count; i++ )
                {
                    ParameterDeclaration parameter = method.Parameters[i];
                    if ( parameter.ParameterType.BelongsToClassification( TypeClassifications.Pointer ) )
                    {
                        // Put the address on the stack.
                        writer.EmitInstructionInt16( OpCodeNumber.Ldarg, (short) ( i + thisShift ) );

                        // Load the array element.
                        writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, arrayVariable );
                        writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, i - firstArgument );
                        writer.EmitInstruction( OpCodeNumber.Ldelem_Ref );

                        PointerTypeSignature pointerType = (PointerTypeSignature) parameter.ParameterType;
                        this.FromObject( pointerType, writer );
                        writer.EmitInstructionStoreIndirect( pointerType.ElementType );
                    }
                }
            }
        }

        /// <summary>
        /// Emits instructions that create a new array of types (Type[]) and fills it with all generic arguments of a given method.
        /// </summary>
        /// <param name="method">Method for which generic arguments have to be read.</param>
        /// <param name="writer"><see cref="InstructionEmitter"/>.</param>
        /// <remarks>
        /// Stack transition: ... -> array, ... .
        /// </remarks>
        public void MakeArrayOfGenericMethodArguments( MethodDefDeclaration method, InstructionEmitter writer )
        {
            int n = method.GenericParameters.Count;
            if ( n == 0 )
            {
                writer.EmitInstruction( OpCodeNumber.Ldnull );
            }
            else
            {
                writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, n );
                writer.EmitInstructionType( OpCodeNumber.Newarr, this.module.Cache.GetType( "System.Type, mscorlib" ) );

                for ( int i = 0; i < n; i++ )
                {
                    writer.EmitInstruction( OpCodeNumber.Dup );
                    writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, i );
                    writer.EmitInstructionType( OpCodeNumber.Ldtoken, method.GenericParameters[i] );
                    writer.EmitInstructionMethod( OpCodeNumber.Call,
                                                  this.module.Cache.GetItem( this.module.Cache.TypeGetTypeFromHandle ) );
                    writer.EmitInstruction( OpCodeNumber.Stelem_Ref );
                }
            }
        }

        /// <summary>
        /// Emit instructions that create a new array of types (Type[]) and fills it with all generic arguments of a given type.
        /// </summary>
        /// <param name="type">Type for which generic arguments have to be read.</param>
        /// <param name="writer">An <see cref="InstructionEmitter"/>.</param>
        /// <remarks>
        /// Stack transition: ... -> array, ... .
        /// </remarks>
        public void MakeArrayOfGenericTypeArguments( TypeDefDeclaration type, InstructionEmitter writer )
        {
            int n = type.GenericParameters.Count;
            if ( n == 0 )
            {
                writer.EmitInstruction( OpCodeNumber.Ldnull );
            }
            else
            {
                writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, n );
                writer.EmitInstructionType( OpCodeNumber.Newarr, this.module.Cache.GetType( "System.Type, mscorlib" ) );

                for ( int i = 0; i < n; i++ )
                {
                    writer.EmitInstruction( OpCodeNumber.Dup );
                    writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, i );
                    writer.EmitInstructionType( OpCodeNumber.Ldtoken, type.GenericParameters[i] );
                    writer.EmitInstructionMethod( OpCodeNumber.Call,
                                                  this.module.Cache.GetItem( this.module.Cache.TypeGetTypeFromHandle ) );
                    writer.EmitInstruction( OpCodeNumber.Stelem_Ref );
                }
            }
        }

        #endregion

        #region Custom Attributes

        /// <summary>
        /// Emits instructions that construct a custom attribute.
        /// </summary>
        /// <param name="attribute">The custom attribute to construct.</param>
        /// <param name="writer">An <see cref="InstructionEmitter"/>.</param>
        /// <remarks>
        /// Stack transition: ... -> Ref(Of <c>attribute.Constructor.DeclaringType</c>), ... .
        /// </remarks>
        public void EmitCustomAttributeConstruction( IAnnotationValue attribute, InstructionEmitter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( attribute, "attribute" );
            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );

            #endregion

            TypeDefDeclaration attributeTypeDef = attribute.Constructor.DeclaringType.GetTypeDefinition();

            foreach ( MemberValuePair pair in attribute.ConstructorArguments )
            {
                pair.Value.EmitLoadValue( writer );
            }

            writer.EmitInstructionMethod( OpCodeNumber.Newobj, attribute.Constructor );

            foreach ( MemberValuePair pair in attribute.NamedArguments )
            {
                writer.EmitInstruction( OpCodeNumber.Dup );
                pair.Value.EmitLoadValue( writer );

                switch ( pair.MemberKind )
                {
                    case MemberKind.Field:
                        {
                            FieldDefDeclaration field = attributeTypeDef.FindField( pair.MemberName );
                            if ( field == null )
                                throw new ArgumentException( string.Format( "Cannot find field {0} in type {1}.",
                                                                            pair.MemberName, attributeTypeDef ) );

                            writer.EmitInstructionField( OpCodeNumber.Stfld, field.Translate( this.module ) );
                        }
                        break;

                    case MemberKind.Property:
                        {
                            PropertyDeclaration property = attributeTypeDef.FindProperty( pair.MemberName );

                            if ( property == null )
                                throw new ArgumentException( string.Format( "Cannot find property {0} in type {1}.",
                                                                            pair.MemberName, attributeTypeDef ) );

                            MethodDefDeclaration setter = property.Setter;

                            if ( setter == null )
                                throw new ArgumentException( string.Format( "The property {0} is read only.", property ) );

                            writer.EmitInstructionMethod(
                                setter.IsVirtual ? OpCodeNumber.Callvirt : OpCodeNumber.Call,
                                setter.Translate( this.module ) );
                        }
                        break;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException( pair.MemberKind, "pair.MemberKind" );
                }
            }
        }

        /// <summary>
        /// Builds a <see cref="CustomAttributeDeclaration"/> representing
        /// the <see cref="System.Diagnostics.DebuggerNonUserCodeAttribute"/> custom attribute.
        /// </summary>
        /// <returns>A new instance of <see cref="CustomAttributeDeclaration"/> representing
        /// the <see cref="System.Diagnostics.DebuggerNonUserCodeAttribute"/> custom attribute.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        public CustomAttributeDeclaration GetDebuggerNonUserCodeAttribute()
        {
            if ( this.debuggerNonUserCodeAttributeConstructor == null )
            {
                this.debuggerNonUserCodeAttributeConstructor =
                    this.module.FindMethod( "System.Diagnostics.DebuggerNonUserCodeAttribute, mscorlib", ".ctor", 0 );
            }

            return new CustomAttributeDeclaration( this.debuggerNonUserCodeAttributeConstructor );
        }

        /// <summary>
        /// Builds a <see cref="CustomAttributeDeclaration"/> representing
        /// the <see cref="System.Diagnostics.DebuggerNonUserCodeAttribute"/> custom attribute
        /// and adds it to a <see cref="CustomAttributeDeclarationCollection"/>.
        /// </summary>
        public void AddCompilerGeneratedAttribute( CustomAttributeDeclarationCollection collection )
        {
            CustomAttributeDeclaration attribute = this.GetCompilerGeneratedAttribute();
            if ( attribute != null )
                collection.Add( attribute );
        }

        /// <summary>
        /// Builds a <see cref="CustomAttributeDeclaration"/> representing
        /// the <see cref="System.Diagnostics.DebuggerNonUserCodeAttribute"/> custom attribute.
        /// </summary>
        /// <returns>A new instance of <see cref="CustomAttributeDeclaration"/> representing
        /// the <see cref="System.Diagnostics.DebuggerNonUserCodeAttribute"/> custom attribute.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        public CustomAttributeDeclaration GetCompilerGeneratedAttribute()
        {
            if ( !this.compilerGeneratedAttributeConstructorCached )
            {
                try
                {
                    this.compilerGeneratedAttributeConstructorCached = true;

                    this.compilerGeneratedAttributeConstructor =
                        this.module.FindMethod( "System.Runtime.CompilerServices.CompilerGeneratedAttribute, mscorlib",
                                                ".ctor", 0 );
                }
                catch ( BindingException )
                {
                    return null;
                }
            }

            // This will be null if the target
            if ( this.compilerGeneratedAttributeConstructor == null )
                return null;

            return new CustomAttributeDeclaration( this.compilerGeneratedAttributeConstructor );
        }

        #endregion

        #region Get Runtime Elements

        /// <summary>
        /// Emits instruction that load a runtime type (<see cref="Type"/>) on the stack.
        /// </summary>
        /// <param name="type">The type to load on the stack.</param>
        /// <param name="writer">An <see cref="InstructionEmitter"/></param>
        /// <remarks>
        /// Stack transition: ... -> Ref(Of <see cref="Type"/>), ... .
        /// </remarks>
        [SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters" )]
        public void GetRuntimeType( ITypeSignature type, InstructionEmitter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );
            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );

            #endregion

            writer.EmitInstructionType( OpCodeNumber.Ldtoken, type );
            writer.EmitInstructionMethod( OpCodeNumber.Call,
                                          this.module.Cache.GetItem( this.module.Cache.TypeGetTypeFromHandle ) );
        }


        /// <summary>
        /// Emits instruction that load a runtime field (<see cref="FieldInfo"/>) on the stack.
        /// </summary>
        /// <param name="field">The field to load on the stack.</param>
        /// <param name="writer">An <see cref="InstructionEmitter"/></param>
        /// <remarks>
        /// Stack transition: ... -> Ref(Of <see cref="FieldInfo"/>), ... .
        /// </remarks>
        public void GetRuntimeField( IField field, InstructionEmitter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( field, "field" );
            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );

            #endregion

            if ( field.DeclaringType.IsGenericDefinition )
            {
                /*
                writer.EmitInstructionField(OpCodeNumber.Ldtoken, field);
                writer.EmitInstructionType(OpCodeNumber.Ldtoken, field.DeclaringType);
                writer.EmitInstructionMethod(OpCodeNumber.Call,
                                              (IMethod)
                                             this.module.Cache.GetItem(DeclarationCache.FieldInfoGetFieldFromHandle2));
                */

                if ( this.type_GetField == null )
                {
                    this.type_GetField = this.module.FindMethod( "System.Type, mscorlib", "GetField", 2 );
                }

                // We will call Type.GetField (String, BindingFlags).
                this.GetRuntimeType( field.DeclaringType, writer );
                writer.EmitInstructionString( OpCodeNumber.Ldstr, field.Name );
                writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4,
                                             (int)
                                             ( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                               BindingFlags.Static ) );
                writer.EmitInstructionMethod( OpCodeNumber.Callvirt, this.type_GetField );
            }
            else
            {
                writer.EmitInstructionField( OpCodeNumber.Ldtoken, field );
                writer.EmitInstructionMethod( OpCodeNumber.Call,
                                              this.module.Cache.GetItem( this.module.Cache.FieldInfoGetFieldFromHandle ) );
            }
        }

        /// <summary>
        /// Emits instruction that load a runtime method (<see cref="MethodBase"/>) on the stack.
        /// </summary>
        /// <param name="method">The method to load on the stack.</param>
        /// <param name="writer">An <see cref="InstructionEmitter"/></param>
        /// <remarks>
        /// Stack transition: ... -> Ref(Of <see cref="MethodBase"/>), ... .
        /// </remarks>
        public void GetRuntimeMethod( IMethod method, InstructionEmitter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( method, "method" );
            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );

            #endregion

            if ( !method.IsGenericDefinition &&
                 !method.DeclaringType.IsGenericDefinition )
            {
                writer.EmitInstructionMethod( OpCodeNumber.Ldtoken, method );
                writer.EmitInstructionMethod( OpCodeNumber.Call,
                                              this.module.Cache.GetItem( this.module.Cache.MethodBaseGetMethodFromHandle ) );
            }
            else
            {
                /*
                writer.EmitInstructionMethod(OpCodeNumber.Ldtoken, method);
                writer.EmitInstructionType(OpCodeNumber.Ldtoken, method.DeclaringType);
                writer.EmitInstructionMethod(OpCodeNumber.Call,
                                              (IMethod)
                                              this.module.Cache.GetItem(DeclarationCache.MethodBaseGetMethodFromHandle2));
                */

                /* We want to call:
                 * 
                    public static MethodBase ReflectionHelper.GetMethod(Type type, string methodName, string methodSignature)
                 */


                if ( this.reflectionHelper_GetMethod == null )
                {
                    this.reflectionHelper_GetMethod =
                        this.module.FindMethod(
                            this.module.GetTypeForFrameworkVariant( typeof(ReflectionHelper) ), "GetMethod" );
                }


                this.GetRuntimeType( method.DeclaringType, writer );
                writer.EmitInstructionString( OpCodeNumber.Ldstr, method.Name );

                StringBuilder stringBuilder = new StringBuilder();
                method.WriteReflectionMethodName( stringBuilder, ReflectionNameOptions.UseBracketsForGenerics );
                string methodToString = stringBuilder.ToString();

                writer.EmitInstructionString( OpCodeNumber.Ldstr, methodToString );
                writer.EmitInstructionMethod( OpCodeNumber.Call, this.reflectionHelper_GetMethod );
            }
        }

        #endregion

        #region Miscenaleous

        /// <summary>
        /// Emits instructions that write a line using <see cref="Console.WriteLine(string)"/>.
        /// </summary>
        /// <param name="format">Formatting string.</param>
        /// <param name="writer">An <see cref="InstructionEmitter"/>.</param>
        /// <param name="parameters">Arguments of the formatting string.</param>
        public void WriteLine( string format, InstructionEmitter writer, params object[] parameters )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( format, "format" );

            #endregion

            if ( this.console_WriteLine == null )
            {
                this.console_WriteLine =
                    this.module.FindMethod(
                        "System.Console, mscorlib", "WriteLine", "System.String, mscorlib" );
            }

            writer.EmitInstructionString( OpCodeNumber.Ldstr,
                                          string.Format( CultureInfo.InvariantCulture, format, parameters ) );
            writer.EmitInstructionMethod( OpCodeNumber.Call, this.console_WriteLine );
        }

        #endregion

        /// <summary>
        /// Transforms an <see cref="InstructionBlock"/> so that it can be protected
        /// and decorated with exception handlers. This method does not add the
        /// handlers in themselves.
        /// </summary>
        /// <param name="reader">An <see cref="InstructionReader"/> assigned to
        /// the method of the current <see cref="InstructionBlock"/>.</param>
        /// <param name="writer">An available <see cref="InstructionWriter"/>.</param>
        /// <param name="block">An <see cref="InstructionBlock"/> attached to
        /// a method.</param>
        /// <param name="returnSequence">An <see cref="PostSharp.CodeModel.InstructionSequence"/> located
        /// outside <paramref name="block"/>, containing the instructions <c>ldloc
        /// <paramref name="returnValueVariable"/>; ret</c>.
        /// <param name="useLeave"><b>true</b> if <b>leave</b> instructions should be
        /// used instead of <b>ret</b>, <b>false</b> to use <b>br</b> instructions.</param>
        /// </param>
        /// <param name="returnValueVariable">A <see cref="LocalVariableSymbol"/> where
        /// the return value will be stored before the control will be directed
        /// to <paramref name="returnSequence"/>.</param>
        /// <remarks>
        /// <para>
        /// The only branching instruction allowed to leave a protected block
        /// is the <b>leave</b> instruction. This method should change all branching
        /// instructions to <b>leave</b>. 
        /// </para>
        /// <para>
        /// The current implementation of the current method is only able to transform
        /// <b>ret</b> instructions, i.e. it assumes that all branch targets
        /// are internal to the block to protect.
        /// </para>
        /// </remarks>
        public void RedirectReturnInstructions(
            InstructionReader reader,
            InstructionWriter writer,
            InstructionBlock block,
            InstructionSequence returnSequence,
            LocalVariableSymbol returnValueVariable,
            bool useLeave )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( block, "block" );
            ExceptionHelper.AssertArgumentNotNull( reader, "reader" );
            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );
            ExceptionHelper.AssertArgumentNotNull( returnSequence, "returnSequence" );

            #endregion

            // Recursively process children blocks.
            if ( block.HasChildrenBlocks )
            {
                InstructionBlock child = block.FirstChildBlock;
                while ( child != null )
                {
                    if ( !child.HasExceptionHandlers )
                    {
                        this.RedirectReturnInstructions( reader, writer, child, returnSequence, returnValueVariable,
                                                         useLeave );
                    }

                    child = child.NextSiblingBlock;
                }
            }

            reader.JumpToInstructionBlock( block );

            // Process instructions.
            if ( block.HasInstructionSequences )
            {
                InstructionSequence sequence = block.FirstInstructionSequence;
                while ( sequence != null )
                {
                    bool changed = false;
                    writer.AttachInstructionSequence( sequence );
                    reader.EnterInstructionSequence( sequence );

                    while ( reader.ReadInstruction() )
                    {
                        // TODO: Process branching instructions that jump
                        // out of the processed block (or natural control flow
                        // flowing out of the block).

                        if ( reader.CurrentInstruction.OpCodeNumber == OpCodeNumber.Ret )
                        {
                            changed = true;
                            if ( returnValueVariable != null )
                            {
                                writer.EmitInstructionLocalVariable( OpCodeNumber.Stloc, returnValueVariable );
                            }

                            if ( reader.CurrentInstruction.SymbolSequencePoint != null )
                            {
                                writer.EmitSymbolSequencePoint( reader.CurrentInstruction.SymbolSequencePoint );
                            }
                            writer.EmitBranchingInstruction( useLeave ? OpCodeNumber.Leave : OpCodeNumber.Br,
                                                             returnSequence );
                        }
                        else
                        {
                            reader.CurrentInstruction.Write( writer );
                        }
                    }

                    reader.LeaveInstructionSequence();
                    writer.DetachInstructionSequence( changed );
                    sequence = sequence.NextSiblingSequence;
                }
            }

            reader.LeaveInstructionBlock();
        }

        /// <summary>
        /// Adds exception handlers to an <see cref="InstructionBlock"/>.
        /// </summary>
        /// <param name="writer">An available <see cref="InstructionWriter"/>.</param>
        /// <param name="block">The <see cref="InstructionBlock"/> to which handlers have to be added. This block should be safe
        /// for being nested in an exception handler (see the <see cref="RedirectReturnInstructions"/> method).</param>
        /// <param name="leaveSequence">The branching target that should be used to jump out of the protected block
        /// (typically a sequence located just after this block).</param>
        /// <param name="catchExceptionTypes">An array of types for which typed <b>catch</b> handlers should be generated,
        /// or <b>null</b> or an empty array if no <b>catch</b> handler should be generated.</param>
        /// <param name="includeFinallyBlock"><b>true</b> whether a <b>finally</b> handler should be added, otherwise <b>false</b>.</param>
        /// <param name="protectedBlock">At output, an <see cref="InstructionBlock"/> containing the initial block and its handlers.</param>
        /// <param name="catchBlocks">At output, an array of blocks where the instructions of <b>catch</b> handler can be emitted. The <b>leave</b>
        /// instruction is already emitted, so user code should not do it a second time. <b>null</b> if no <b>catch</b> handler was requested.</param>
        /// <param name="finallyBlock">At output, an <see cref="InstructionBlock"/> where the instruction of the <b>finally</b> handler
        /// can be emitted. The <b>endfinally</b> instruction is already emitted, so user code should not do it a second time. 
        /// <b>null</b> if no <b>finally</b> handler was requested.</param>
        /// <remarks>
        /// <para>The method body is modified in such a way that the <b>finally</b> handler protects also the <b>catch</b> handlers.
        /// That is, if an exception is thrown from a <b>catch</b> handler, the <b>finally</b> handler will be invoked.
        /// </para>
        /// </remarks>
        public void AddExceptionHandlers(
            InstructionWriter writer,
            InstructionBlock block,
            InstructionSequence leaveSequence,
            ITypeSignature[] catchExceptionTypes,
            bool includeFinallyBlock,
            out InstructionBlock protectedBlock,
            out InstructionBlock[] catchBlocks,
            out InstructionBlock finallyBlock )
        {
            MethodBodyDeclaration methodBody = block.MethodBody;

            // Append the "leave" instruction to the block.
            InstructionBlock blockWithLeave = block.Nest();
            blockWithLeave.Comment = "Block with Leave";
            InstructionBlock leaveTryBlock = methodBody.CreateInstructionBlock();
            blockWithLeave.AddChildBlock( leaveTryBlock, NodePosition.After, null );

            /*InstructionSequence leaveTrySequence = block.MethodBody.CreateInstructionSequence();
            leaveTryBlock.AddInstructionSequence(leaveTrySequence, NodePosition.After, null);
            writer.AttachInstructionSequence(leaveTrySequence);
            writer.EmitBranchingInstruction(OpCodeNumber.Leave, leaveSequence);
            writer.DetachInstructionSequence();*/

            // Build a new parent group that will contain everything.
            protectedBlock = blockWithLeave.Nest();
            protectedBlock.Comment = "Protected Group";

            // Build the 'finally' handler.
            if ( includeFinallyBlock )
            {
                // Create the block that will be protected by the "finally" handler.
                InstructionBlock tryFinallyBlock = methodBody.CreateInstructionBlock();
                tryFinallyBlock.Comment = "Try-Finally Block";
                block.ParentBlock.AddChildBlock( tryFinallyBlock, NodePosition.After, block );
                block.Detach();
                tryFinallyBlock.AddChildBlock( block, NodePosition.Before, null );


                // Create the 'finally' exception handler.
                InstructionBlock systemFinallyBlock = methodBody.CreateInstructionBlock();
                systemFinallyBlock.Comment = "System Finally Block";
                tryFinallyBlock.AddExceptionHandlerFinally( systemFinallyBlock, NodePosition.After, null );

                // Create the finally handler (the user block, then the 'endfinally' instruction).
                finallyBlock = methodBody.CreateInstructionBlock();
                finallyBlock.Comment = "User Finally Block";
                InstructionBlock endFinallyBlock = methodBody.CreateInstructionBlock();
                endFinallyBlock.Comment = "End Finally Block";
                systemFinallyBlock.AddChildBlock( finallyBlock, NodePosition.Before, null );
                systemFinallyBlock.AddChildBlock( endFinallyBlock, NodePosition.After, null );

                InstructionSequence endFinallySequence = block.MethodBody.CreateInstructionSequence();
                endFinallyBlock.AddInstructionSequence( endFinallySequence, NodePosition.After, null );
                writer.AttachInstructionSequence( endFinallySequence );
                writer.EmitSymbolSequencePoint( SymbolSequencePoint.Hidden );
                writer.EmitInstruction( OpCodeNumber.Endfinally );
                writer.DetachInstructionSequence();
            }
            else
            {
                //tryFinallyBlock = null;
                finallyBlock = null;
            }

            // Build the 'catch' handlers.
            if ( catchExceptionTypes != null && catchExceptionTypes.Length > 0 )
            {
                catchBlocks = new InstructionBlock[catchExceptionTypes.Length];

                for ( int i = 0; i < catchExceptionTypes.Length; i++ )
                {
                    // Add catch and finally handlers.
                    InstructionBlock systemCatchBlock = methodBody.CreateInstructionBlock();
                    systemCatchBlock.Comment = string.Format(
                        CultureInfo.InvariantCulture,
                        "Catch Block #{0}", i );
                    block.AddExceptionHandlerCatch( catchExceptionTypes[i], systemCatchBlock, NodePosition.After, null );
                    catchBlocks[i] = methodBody.CreateInstructionBlock();
                    catchBlocks[i].Comment = string.Format(
                        CultureInfo.InvariantCulture,
                        "User Catch Block #{0}", i );
                    systemCatchBlock.AddChildBlock( catchBlocks[i], NodePosition.After, null );
                    InstructionBlock leaveBlock = methodBody.CreateInstructionBlock();
                    leaveBlock.Comment = "Leave Block";
                    systemCatchBlock.AddChildBlock( leaveBlock, NodePosition.After, null );
                    InstructionSequence leaveCatchSequence = block.MethodBody.CreateInstructionSequence();
                    leaveBlock.AddInstructionSequence( leaveCatchSequence, NodePosition.After, null );
                    writer.AttachInstructionSequence( leaveCatchSequence );
                    writer.EmitSymbolSequencePoint( SymbolSequencePoint.Hidden );
                    writer.EmitBranchingInstruction( OpCodeNumber.Leave, leaveSequence );
                    writer.DetachInstructionSequence();
                }
            }
            else
            {
                catchBlocks = null;
            }
        }
    }
}