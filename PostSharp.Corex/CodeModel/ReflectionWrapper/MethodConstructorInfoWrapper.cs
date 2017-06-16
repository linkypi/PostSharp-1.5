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

namespace PostSharp.CodeModel.ReflectionWrapper
{
    internal sealed class MethodConstructorInfoWrapper : ConstructorInfo, IReflectionWrapper<MethodDefDeclaration>
    {
        private readonly IMethod method;
        private readonly MethodDefDeclaration methodDef;
        private readonly Type[] genericTypeArguments;
        private readonly Type[] genericMethodArguments;

        public MethodConstructorInfoWrapper( IMethod method, Type[] genericTypeArguments,
                                             Type[] genericMethodArguments )
        {
            this.method = method;
            this.methodDef = method.GetMethodDefinition();
            ReflectionWrapperUtil.GetGenericMapWrapper( method.GetGenericContext( GenericContextOptions.None ),
                                                        genericTypeArguments, genericMethodArguments,
                                                        out this.genericTypeArguments, out this.genericMethodArguments );
        }

        public override bool IsGenericMethodDefinition { get { return this.method.IsGenericDefinition; } }

        public override Type[] GetGenericArguments()
        {
            return this.genericMethodArguments;
        }


        public override int MetadataToken { get { return (int) this.method.MetadataToken.Value; } }


        public override MethodAttributes Attributes { get { return this.methodDef.Attributes; } }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return this.methodDef.ImplementationAttributes;
        }

        public override ParameterInfo[] GetParameters()
        {
            return
                ReflectionWrapperUtil.GetMethodParameters( this.methodDef, this.genericTypeArguments,
                                                           this.genericTypeArguments );
        }

        public override object Invoke( object obj, BindingFlags invokeAttr, Binder binder, object[] parameters,
                                       CultureInfo culture )
        {
            throw new NotSupportedException();
        }

        public override RuntimeMethodHandle MethodHandle { get { throw new NotSupportedException(); } }

        public override Type DeclaringType
        {
            get
            {
                TypeDefDeclaration internalDeclaringType = this.methodDef.DeclaringType;
                return internalDeclaringType != null ? internalDeclaringType.GetReflectionWrapper( null, null ) : null;
            }
        }

        public override object[] GetCustomAttributes( Type attributeType, bool inherit )
        {
            return ReflectionWrapperUtil.GetCustomAttributes(
                this.methodDef, attributeType );
        }

        public override object[] GetCustomAttributes( bool inherit )
        {
            return ReflectionWrapperUtil.GetCustomAttributes(
                this.methodDef );
        }

        public override bool IsDefined( Type attributeType, bool inherit )
        {
            return ReflectionWrapperUtil.IsCustomAttributeDefined(
                this.methodDef, attributeType );
        }

        public override string Name { get { return this.methodDef.Name; } }

        public override Type ReflectedType { get { throw new NotSupportedException(); } }

        public override object Invoke( BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture )
        {
            throw new NotSupportedException();
        }

        object IReflectionWrapper.UnderlyingSystemObject { get { return this.method.GetReflectionWrapper(this.genericTypeArguments, this.genericMethodArguments); } }

        public IAssemblyName DeclaringAssemblyName
        {
            get { return this.methodDef.DeclaringAssembly; }
        }

        #region IReflectionWrapper<MethodDefDeclaration> Members

        MethodDefDeclaration IReflectionWrapper<MethodDefDeclaration>.WrappedObject
        {
            get { return this.methodDef; }
        }

        #endregion
    }
}