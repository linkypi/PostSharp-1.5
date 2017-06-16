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
using System.Globalization;
using System.Reflection;
using PostSharp.Collections;

namespace PostSharp.CodeModel.ReflectionWrapper
{
    internal sealed class PropertyWrapper : PropertyInfo, IReflectionWrapper<PropertyDeclaration>
    {
        private readonly PropertyDeclaration property;
        private readonly Type[] genericTypeArguments;
        private readonly Type[] genericMethodArguments;

        public PropertyWrapper( PropertyDeclaration property,
                                Type[] genericTypeArguments,
                                Type[] genericMethodArguments )
        {
            this.property = property;
            this.genericTypeArguments = genericTypeArguments;
            this.genericMethodArguments = genericMethodArguments;
        }

        public override PropertyAttributes Attributes { get { return this.property.Attributes; } }

        public override bool CanRead { get { return this.property.CanRead; } }

        public override bool CanWrite { get { return this.property.CanWrite; } }

        public override MethodInfo[] GetAccessors( bool nonPublic )
        {
            List<MethodInfo> methodInfo = new List<MethodInfo>(2);
            foreach ( MethodSemanticDeclaration member in this.property.Members )
            {
                  if (!nonPublic && member.Method.Visibility != Visibility.Public)
                    continue;

                methodInfo.Add( (MethodInfo) member.Method.GetReflectionWrapper( genericTypeArguments, genericMethodArguments ) );
            }

            return methodInfo.ToArray();
        }

        public override MethodInfo GetGetMethod( bool nonPublic )
        {
            return ReflectionWrapperUtil.GetMethodSemantic( this.property,
                                                            MethodSemantics.Getter, nonPublic,
                                                            this.genericTypeArguments, this.genericMethodArguments );
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            if ( this.CanRead )
            {
                MethodDefDeclaration method = this.property.Members.GetBySemantic( MethodSemantics.Getter ).Method;

                ParameterInfo[] parameters = new ParameterInfo[method.Parameters.Count];

                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i] = new PropertyParameterWrapper( this, method.Parameters[i], this.genericTypeArguments, this.genericMethodArguments);
                }

                return parameters;

            }
            else
            {
                return EmptyArray<ParameterInfo>.GetInstance();
            }
        }

        public override MethodInfo GetSetMethod( bool nonPublic )
        {
            return ReflectionWrapperUtil.GetMethodSemantic( this.property,
                                                            MethodSemantics.Setter, nonPublic,
                                                            this.genericTypeArguments, this.genericMethodArguments );
        }

        public override object GetValue( object obj, BindingFlags invokeAttr, Binder binder, object[] index,
                                         CultureInfo culture )
        {
            throw new NotSupportedException();
        }

        public override Type PropertyType
        {
            get
            {
                return
                    this.property.PropertyType.GetReflectionWrapper( this.genericTypeArguments,
                                                                     this.genericMethodArguments );
            }
        }

        public override void SetValue( object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index,
                                       CultureInfo culture )
        {
            throw new NotSupportedException();
        }

        public override Type DeclaringType
        {
            get
            {
                return
                    this.property.DeclaringType.GetReflectionWrapper( this.genericTypeArguments,
                                                                      this.genericMethodArguments );
            }
        }

        public override object[] GetCustomAttributes( Type attributeType, bool inherit )
        {
            return ReflectionWrapperUtil.GetCustomAttributes( this.property, attributeType );
        }

        public override object[] GetCustomAttributes( bool inherit )
        {
            return ReflectionWrapperUtil.GetCustomAttributes( this.property );
        }

        public override bool IsDefined( Type attributeType, bool inherit )
        {
            return ReflectionWrapperUtil.IsCustomAttributeDefined( this.property, attributeType );
        }

        public override string Name { get { return this.property.Name; } }

        public override Type ReflectedType { get { throw new NotSupportedException(); } }

        object IReflectionWrapper.UnderlyingSystemObject { get { return this.property.GetReflectionWrapper(this.genericTypeArguments, this.genericMethodArguments); } }
        public IAssemblyName DeclaringAssemblyName
        {
            get { return this.property.DeclaringAssembly; }
        }

        #region IReflectionWrapper<PropertyDeclaration> Members

        PropertyDeclaration IReflectionWrapper<PropertyDeclaration>.WrappedObject
        {
            get { return this.property; }
        }

        #endregion
    }
}