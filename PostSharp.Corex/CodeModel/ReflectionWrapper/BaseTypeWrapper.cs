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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using PostSharp.CodeModel.Binding;
using PostSharp.Collections;
using PostSharp.Reflection;

namespace PostSharp.CodeModel.ReflectionWrapper
{
    internal abstract class BaseTypeWrapper : Type, IReflectionWrapper<ITypeSignature>
    {
        protected TypeDefDeclaration typeDef;
        protected Type[] genericTypeArguments;
        protected Type[] genericMethodArguments;
        protected BaseTypeWrapper baseType;

        protected BaseTypeWrapper( TypeDefDeclaration typeDef,
                                GenericMap genericContext,
                                Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            this.typeDef = typeDef;

            ReflectionWrapperUtil.GetGenericMapWrapper( genericContext,
                                                        genericTypeArguments, genericMethodArguments,
                                                        out this.genericTypeArguments, 
                                                        out this.genericMethodArguments );
        }


        public override sealed Guid GUID { get { throw new NotSupportedException(); } }

        public override sealed Type GetElementType()
        {
            return null;
        }

        protected override sealed bool HasElementTypeImpl()
        {
            return false;
        }


        public override sealed object InvokeMember( string name, BindingFlags invokeAttr, Binder binder, object target,
                                                    object[] args, ParameterModifier[] modifiers, CultureInfo culture,
                                                    string[] namedParameters )
        {
            throw new NotSupportedException();
        }

        public override sealed Assembly Assembly { get { return this.typeDef.DeclaringAssembly.GetSystemAssembly(); } }

        public override sealed string AssemblyQualifiedName
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                this.typeDef.WriteReflectionTypeName( builder, ReflectionNameOptions.UseAssemblyName | ReflectionNameOptions.IgnoreGenericTypeDefParameters);
                return builder.ToString();
            }
        }



        public override sealed Type BaseType
        {
            get
            {
                if ( this.baseType == null )
                {
                    if ( this.typeDef.BaseType == null )
                    {
                        return null;
                    }

                    this.baseType = (BaseTypeWrapper)
                                    this.typeDef.BaseType.GetReflectionWrapper( this.genericTypeArguments,
                                                                                this.genericMethodArguments );
                }

                return this.baseType;
            }
        }

        public override RuntimeTypeHandle TypeHandle { get { throw new NotSupportedException(); } }

        public override bool ContainsGenericParameters
        {
            get
            {
                return this.typeDef.ContainsGenericArguments();
            }
        }

        protected override sealed TypeAttributes GetAttributeFlagsImpl()
        {
            return this.typeDef.Attributes;
        }

        protected override sealed ConstructorInfo GetConstructorImpl( BindingFlags bindingAttr, Binder binder,
                                                                      CallingConventions callConvention, Type[] types,
                                                                      ParameterModifier[] modifiers )
        {
            foreach (MethodDefDeclaration method in this.typeDef.Methods.GetByName(".ctor"))
            {
                if (BindingHelper.BindingFlagsMatch(bindingAttr, method.IsStatic, method.Visibility) &&
                    BindingHelper.SignatureMatches(method, callConvention, types))
                    return (ConstructorInfo)method.GetReflectionWrapper(this.genericTypeArguments, this.genericMethodArguments);
            }

            return null; 
        }

        public override sealed ConstructorInfo[] GetConstructors( BindingFlags bindingAttr )
        {
            List<ConstructorInfo> constructors = new List<ConstructorInfo>();

            if ( ( bindingAttr & BindingFlags.Instance ) != 0 )
            {
                foreach ( MethodDefDeclaration method in this.typeDef.Methods.GetByName( ".ctor" ) )
                {
                    constructors.Add(
                        (ConstructorInfo)
                        method.GetReflectionWrapper( this.genericTypeArguments, this.genericMethodArguments ) );
                }
            }

            if ( ( bindingAttr & BindingFlags.Static ) != 0 )
            {
                foreach ( MethodDefDeclaration method in this.typeDef.Methods.GetByName( ".cctor" ) )
                {
                    constructors.Add(
                        (ConstructorInfo)
                        method.GetReflectionWrapper( this.genericTypeArguments, this.genericMethodArguments ) );
                }
            }

            return constructors.ToArray();
        }

        public override Type[] GetGenericArguments()
        {
            return EmptyTypes;
        }


        public override sealed Type GetInterface( string name, bool ignoreCase )
        {
            throw new NotImplementedException();
        }


        protected override sealed bool IsArrayImpl()
        {
            return false;
        }

        protected override sealed bool IsByRefImpl()
        {
            return false;
        }

        protected override sealed bool IsCOMObjectImpl()
        {
            return false;
        }

        protected override sealed bool IsPointerImpl()
        {
            return false;
        }

        protected override sealed bool IsPrimitiveImpl()
        {
            return false;
        }

        public override sealed Module Module { get { return this.typeDef.Module.GetSystemModule(); } }

        public override sealed string Namespace
        {
            get
            {
                string ns, name;
                this.SplitName( out ns, out name );
                return ns;
            }
        }

        private void SplitName( out string ns, out string name )
        {
            int dot = this.typeDef.Name.LastIndexOf( '.' );
            if ( dot > 0 )
            {
                ns = this.typeDef.Name.Substring( 0, dot );
                name = this.typeDef.Name.Substring( dot + 1 );
            }
            else
            {
                ns = null;
                name = this.typeDef.Name;
            }
        }


        public override sealed EventInfo[] GetEvents( BindingFlags bindingAttr )
        {
            List<EventInfo> events = new List<EventInfo>();
            this.PopulateEvents( bindingAttr, events );
            return events.ToArray();
        }

        protected EventInfo GetEventImpl( string name, BindingFlags bindingAttr )
        {
            EventDeclaration @event = this.typeDef.Events.GetByName( name );
            if ( @event != null && BindingHelper.BindingFlagsMatch( bindingAttr, @event ))
            {
                return @event.GetReflectionWrapper( this.genericTypeArguments, this.genericMethodArguments );
            }
            else
            {
                return null;
            }
        }

        public override sealed EventInfo GetEvent( string name, BindingFlags bindingAttr )
        {
            EventInfo eventInfo = this.GetEventImpl( name, bindingAttr );

            if ( eventInfo != null )
            {
                return eventInfo;
            }

            BaseTypeWrapper cursor = (BaseTypeWrapper) this.BaseType;
            while ( cursor != null )
            {
                eventInfo = cursor.GetEventImpl( name, bindingAttr );
                if ( eventInfo != null )
                {
                    return eventInfo;
                }

                cursor = (BaseTypeWrapper) cursor.BaseType;
            }

            return null;
        }

        public override sealed FieldInfo[] GetFields( BindingFlags bindingAttr )
        {
            List<FieldInfo> fields = new List<FieldInfo>();
            this.PopulateFields( bindingAttr, fields );
            return fields.ToArray();
        }

        public override sealed FieldInfo GetField( string name, BindingFlags bindingAttr )
        {
            FieldInfo fieldInfo = this.GetFieldImpl( name, bindingAttr );

            if ( fieldInfo != null )
            {
                return fieldInfo;
            }

            BaseTypeWrapper cursor = (BaseTypeWrapper) this.BaseType;
            while ( cursor != null )
            {
                fieldInfo = cursor.GetFieldImpl( name, bindingAttr );
                if ( fieldInfo != null )
                {
                    return fieldInfo;
                }

                cursor = (BaseTypeWrapper) cursor.BaseType;
            }

            return null;
        }

        public override sealed Type[] GetInterfaces()
        {
            List<Type> types = new List<Type>();
            this.PopulateInterfaces( types );
            return types.ToArray();
        }

        public override sealed MemberInfo[] GetMembers( BindingFlags bindingAttr )
        {
            List<MemberInfo> members = new List<MemberInfo>();
            this.PopulateFields( bindingAttr, members );
            this.PopulateMethods( bindingAttr, members );
            return members.ToArray();
        }

        public override sealed MethodInfo[] GetMethods( BindingFlags bindingAttr )
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            this.PopulateMethods( bindingAttr, methods );
            return methods.ToArray();
        }

        protected override sealed MethodInfo GetMethodImpl( string name, BindingFlags bindingAttr, Binder binder,
                                                            CallingConventions callConvention, Type[] types,
                                                            ParameterModifier[] modifiers )
        {
            MethodInfo methodInfo =
                this.GetMethodImplImpl( name, bindingAttr, binder, callConvention, types, modifiers );

            if ( methodInfo != null )
            {
                return methodInfo;
            }

            BaseTypeWrapper cursor = (BaseTypeWrapper) this.BaseType;
            while ( cursor != null )
            {
                methodInfo = cursor.GetMethodImplImpl( name, bindingAttr, binder, callConvention, types, modifiers );
                if ( methodInfo != null )
                {
                    return methodInfo;
                }

                cursor = (BaseTypeWrapper) cursor.BaseType;
            }

            return null;
        }

        private void PopulateProperties( BindingFlags bindingAttr, IList properties )
        {
           foreach ( PropertyDeclaration property in this.typeDef.Properties )
            {
               if (BindingHelper.BindingFlagsMatch( bindingAttr, property ))
               {
                    properties.Add(property.GetReflectionWrapper(this.genericTypeArguments, this.genericMethodArguments));
               }
            }
        }

        public override sealed PropertyInfo[] GetProperties( BindingFlags bindingAttr )
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();
            this.PopulateProperties( bindingAttr, properties );
            return properties.ToArray();
        }


        protected override sealed PropertyInfo GetPropertyImpl( string name, BindingFlags bindingAttr, Binder binder,
                                                                Type returnType, Type[] types,
                                                                ParameterModifier[] modifiers )
        {
            PropertyInfo propertyInfo =
                this.GetPropertyImplImpl( name, bindingAttr, binder, returnType, types, modifiers );

            if ( propertyInfo != null )
            {
                return propertyInfo;
            }

            BaseTypeWrapper cursor = (BaseTypeWrapper) this.BaseType;
            while ( cursor != null )
            {
                propertyInfo = cursor.GetPropertyImplImpl( name, bindingAttr, binder, returnType, types, modifiers );
                if ( propertyInfo != null )
                {
                    return propertyInfo;
                }

                cursor = (BaseTypeWrapper) cursor.BaseType;
            }

            return null;
        }

        public override sealed object[] GetCustomAttributes( Type attributeType, bool inherit )
        {
          
            ArrayList objects = new ArrayList();

            TypeDefDeclaration cursor = this.typeDef;
            while (cursor != null)
            {
                TypeDefDeclaration current = cursor;
                cursor = inherit && cursor.BaseType != null ? cursor.BaseType.GetTypeDefinition() : null;

                IType internalAttributeType;
                
                if ( attributeType == null )
                {
                    internalAttributeType = null;
                }
                else
                {
                    internalAttributeType = (IType)current.Module.FindType(attributeType, BindingOptions.OnlyExisting | BindingOptions.DontThrowException);
                    if ( internalAttributeType == null )
                        continue;
                    
                }
                
                current.CustomAttributes.ConstructRuntimeObjects(internalAttributeType, objects);
                
            }

            return (object[]) objects.ToArray(attributeType ?? typeof(Attribute));
        }

        public override sealed object[] GetCustomAttributes( bool inherit )
        {
            return this.GetCustomAttributes( null, inherit );
        }


        public override sealed bool IsDefined( Type attributeType, bool inherit )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( attributeType, "attributeType" );

            #endregion

            TypeDefDeclaration cursor = this.typeDef;
            while (cursor != null)
            {
                IType internalAttributeType = (IType)
                                         this.typeDef.Module.FindType(attributeType, BindingOptions.OnlyExisting | BindingOptions.DontThrowException);


                if (internalAttributeType != null && cursor.CustomAttributes.Contains(internalAttributeType))
                {
                    return true;
                }

                cursor = inherit ? cursor.BaseType.GetTypeDefinition() : null;
            }

            return false;
        }

        public override sealed string Name
        {
            get
            {
                string ns, name;
                this.SplitName( out ns, out name );
                return name;
            }
        }

        private void PopulateEvents( BindingFlags bindingAttr, IList events )
        {
            foreach ( EventDeclaration @event in this.typeDef.Events )
            {
                if (BindingHelper.BindingFlagsMatch(bindingAttr, @event))
                {
                    events.Add( @event.GetReflectionWrapper( this.genericTypeArguments, this.genericMethodArguments ) );
                }
            }
        }

        private void PopulateFields( BindingFlags bindingAttr, IList fields )
        {
            foreach ( FieldDefDeclaration field in this.typeDef.Fields )
            {
                if (BindingHelper.BindingFlagsMatch(bindingAttr, field.IsStatic, field.Visibility))
                {
                    fields.Add( field.GetReflectionWrapper( this.genericTypeArguments, this.genericMethodArguments ) );
                }
            }
        }

        private FieldInfo GetFieldImpl( string name, BindingFlags bindingAttr )
        {
            FieldDefDeclaration field = this.typeDef.FindField( name );
            if ( field == null || !BindingHelper.BindingFlagsMatch( bindingAttr, field.IsStatic, field.Visibility )) return null;

            return field.GetReflectionWrapper( this.genericTypeArguments, this.genericMethodArguments );
        }

        private void PopulateInterfaces( IList<Type> interfaces )
        {
            Set<ITypeSignature> _interfaces = this.typeDef.GetInterfacesRecursive();

            foreach ( ITypeSignature type in _interfaces )
            {
                interfaces.Add( type.GetReflectionWrapper( this.genericTypeArguments, this.genericMethodArguments ) );
            }
        }

        private void PopulateMethods( BindingFlags bindingAttr, IList methods )
        {
            foreach ( MethodDefDeclaration method in this.typeDef.Methods )
            {
                if (method.Name != ".ctor" && method.Name != ".cctor" &&
                    BindingHelper.BindingFlagsMatch(bindingAttr, method.IsStatic, method.Visibility))
                {
                    methods.Add( method.GetReflectionWrapper( this.genericTypeArguments, this.genericMethodArguments ) );
                }
            }
        }

        protected MethodInfo GetMethodImplImpl( string name, BindingFlags bindingAttr, Binder binder,
                                                CallingConventions callConvention, Type[] types,
                                                ParameterModifier[] modifiers )
        {
            foreach ( MethodDefDeclaration method in this.typeDef.Methods.GetByName( name ) )
            {
                if (BindingHelper.BindingFlagsMatch(bindingAttr, method.IsStatic, method.Visibility) &&
                    BindingHelper.SignatureMatches(method, callConvention, types))
                    return (MethodInfo) method.GetReflectionWrapper( this.genericTypeArguments, this.genericMethodArguments );
            }

            return null;

        }

        protected PropertyInfo GetPropertyImplImpl( string name, BindingFlags bindingAttr, Binder binder,
                                                    Type returnType,
                                                    Type[] types, ParameterModifier[] modifiers )
        {
            foreach (PropertyDeclaration property in this.typeDef.Properties.GetByName(name))
            {
                MethodDefDeclaration getter = property.Getter;
                if (getter == null) continue;

                if (BindingHelper.BindingFlagsMatch(bindingAttr, getter.IsStatic, getter.Visibility) &&
                    BindingHelper.SignatureMatches(getter, CallingConventions.Any, types))
                    return property.GetReflectionWrapper(this.genericTypeArguments, this.genericMethodArguments);
            }

            return null;
        }

        public override int GetHashCode()
        {
            return ReflectionTypeComparer.GetInstance().GetHashCode( this );
        }

        public override bool Equals( object o )
        {
            return ReflectionTypeComparer.GetInstance().Equals( this, (Type) o );
        }

        object IReflectionWrapper.UnderlyingSystemObject { get { return this.UnderlyingSystemType; } }
        public IAssemblyName DeclaringAssemblyName
        {
            get { return this.typeDef.DeclaringAssembly; }
        }

        public override bool IsInstanceOfType(object o)
        {
            if (o == null)
                return false;

            return this.IsAssignableFrom(o.GetType());
        }

        #region IReflectionWrapper<ITypeSignature> Members

        ITypeSignature IReflectionWrapper<ITypeSignature>.WrappedObject
        {
            get { return this.typeDef; }
        }

        #endregion
    }
}
