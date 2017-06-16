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
using System.Globalization;
using System.Reflection;
using System.Text;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;
using PostSharp.Reflection;

namespace PostSharp.CodeModel.ReflectionWrapper
{
    [Serializable]
    internal sealed class ArrayTypeSignatureWrapper : Type, IReflectionWrapper<ITypeSignature>
    {
        private readonly ArrayTypeSignature type;
        private readonly Type[] genericTypeArguments;
        private readonly Type[] genericMethodArguments;


        public ArrayTypeSignatureWrapper( ArrayTypeSignature type, Type[] genericTypeArguments,
                                          Type[] genericMethodArguments )
        {
            this.type = type;
            this.genericTypeArguments = genericTypeArguments;
            this.genericMethodArguments = genericMethodArguments;
        }

        public override bool ContainsGenericParameters { get { return this.type.ContainsGenericArguments(); } }

        public override int GetArrayRank()
        {
            return this.type.Rank;
        }

        public override RuntimeTypeHandle TypeHandle { get { throw new NotSupportedException(); } }

        public override Assembly Assembly { get { return null; } }

        public override string AssemblyQualifiedName { get { return null; } }

        public override Type BaseType { get { return null; } }

        public override string FullName { get { return null; } }

        public override Guid GUID { get { return Guid.Empty; } }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return 0;
        }

        protected override ConstructorInfo GetConstructorImpl( BindingFlags bindingAttr, Binder binder,
                                                               CallingConventions callConvention, Type[] types,
                                                               ParameterModifier[] modifiers )
        {
            return null;
        }

        public override ConstructorInfo[] GetConstructors( BindingFlags bindingAttr )
        {
            return EmptyArray<ConstructorInfo>.GetInstance();
        }

        public override Type GetElementType()
        {
            return this.type.ElementType.GetReflectionWrapper(
                this.genericTypeArguments, this.genericMethodArguments );
        }

        public override EventInfo GetEvent( string name, BindingFlags bindingAttr )
        {
            return null;
        }

        public override EventInfo[] GetEvents( BindingFlags bindingAttr )
        {
            return EmptyArray<EventInfo>.GetInstance();
        }

        public override FieldInfo GetField( string name, BindingFlags bindingAttr )
        {
            return null;
        }

        public override FieldInfo[] GetFields( BindingFlags bindingAttr )
        {
            return EmptyArray<FieldInfo>.GetInstance();
        }

        public override Type GetInterface( string name, bool ignoreCase )
        {
            return null;
        }

        public override Type[] GetInterfaces()
        {
            return EmptyArray<Type>.GetInstance();
        }

        public override MemberInfo[] GetMembers( BindingFlags bindingAttr )
        {
            return EmptyArray<MemberInfo>.GetInstance();
        }

        protected override MethodInfo GetMethodImpl( string name, BindingFlags bindingAttr, Binder binder,
                                                     CallingConventions callConvention, Type[] types,
                                                     ParameterModifier[] modifiers )
        {
            return null;
        }

        public override MethodInfo[] GetMethods( BindingFlags bindingAttr )
        {
            return EmptyArray<MethodInfo>.GetInstance();
        }

        public override Type GetNestedType( string name, BindingFlags bindingAttr )
        {
            return null;
        }

        public override Type[] GetNestedTypes( BindingFlags bindingAttr )
        {
            return EmptyArray<Type>.GetInstance();
        }

        public override PropertyInfo[] GetProperties( BindingFlags bindingAttr )
        {
            return EmptyArray<PropertyInfo>.GetInstance();
        }

        protected override PropertyInfo GetPropertyImpl( string name, BindingFlags bindingAttr, Binder binder,
                                                         Type returnType, Type[] types, ParameterModifier[] modifiers )
        {
            return null;
        }

        protected override bool HasElementTypeImpl()
        {
            return true;
        }

        public override object InvokeMember( string name, BindingFlags invokeAttr, Binder binder, object target,
                                             object[] args, ParameterModifier[] modifiers, CultureInfo culture,
                                             string[] namedParameters )
        {
            throw new NotSupportedException();
        }

        protected override bool IsArrayImpl()
        {
            return true;
        }

        protected override bool IsByRefImpl()
        {
            return false;
        }

        protected override bool IsCOMObjectImpl()
        {
            return false;
        }

        protected override bool IsPointerImpl()
        {
            return false;
        }

        protected override bool IsPrimitiveImpl()
        {
            return false;
        }

        public override Module Module { get { return null; } }

        public override string Namespace { get { return null; } }

        public override Type UnderlyingSystemType { get { return this.type.GetSystemType( this.genericTypeArguments, this.genericMethodArguments ); } }

        object IReflectionWrapper.UnderlyingSystemObject { get { return this.UnderlyingSystemType; } }
        
        public IAssemblyName DeclaringAssemblyName
        {
            get { return null; }
        }

        public override object[] GetCustomAttributes( Type attributeType, bool inherit )
        {
            return null;
        }

        public override object[] GetCustomAttributes( bool inherit )
        {
            return EmptyArray<object>.GetInstance();
        }

        public override bool IsDefined( Type attributeType, bool inherit )
        {
            return false;
        }

        public override string Name { get { return null; } }

        public override int GetHashCode()
        {
            return ReflectionTypeComparer.GetInstance().GetHashCode( this );
        }

        public override bool Equals( object o )
        {
            return ReflectionTypeComparer.GetInstance().Equals( this, (Type) o );
        }

        public override string ToString()
        {
            // TODO: Resolve generic parameters. Unfortunately this cannot be done in the current design.
            StringBuilder stringBuilder = new StringBuilder();
            this.type.WriteReflectionTypeName( stringBuilder, ReflectionNameOptions.UseBracketsForGenerics );
            return stringBuilder.ToString();
        }

        #region IReflectionWrapper<ITypeSignature> Members

        ITypeSignature IReflectionWrapper<ITypeSignature>.WrappedObject
        {
            get { return this.type; }
        }

        #endregion

    }
}
