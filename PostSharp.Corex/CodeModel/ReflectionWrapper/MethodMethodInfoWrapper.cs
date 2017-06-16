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
using System.Globalization;
using System.Reflection;

namespace PostSharp.CodeModel.ReflectionWrapper
{
    internal sealed class MethodMethodInfoWrapper : MethodInfo, IReflectionWrapper<MethodDefDeclaration>
    {
        private readonly IMethod method;
        private readonly MethodDefDeclaration methodDef;
        private readonly Type[] genericTypeArguments;
        private readonly Type[] genericMethodArguments;

        public MethodMethodInfoWrapper(IMethod method, Type[] genericTypeArguments,
                                       Type[] genericMethodArguments)
        {
            this.method = method;
            this.methodDef = method.GetMethodDefinition();
            ReflectionWrapperUtil.GetGenericMapWrapper(method.GetGenericContext(GenericContextOptions.None),
                                                       genericTypeArguments, genericMethodArguments,
                                                       out this.genericTypeArguments, out this.genericMethodArguments);
        }

        public override bool IsGenericMethodDefinition
        {
            get { return this.method.IsGenericDefinition; }
        }

        public override Type[] GetGenericArguments()
        {
            return ReflectionWrapperUtil.GetGenericArguments(this.method,
                                                             this.genericTypeArguments, this.genericTypeArguments);
        }

        public override MethodInfo GetGenericMethodDefinition()
        {
            if (this.method.IsGenericInstance)
            {
                return (MethodInfo) this.methodDef.GetReflectionWrapper(null, null);
            }
            else
            {
                return null;
            }
        }

        public override ParameterInfo ReturnParameter
        {
            get
            {
                return
                    this.methodDef.ReturnParameter.GetReflectionWrapper(this.genericTypeArguments,
                                                                        this.genericMethodArguments);
            }
        }

        public override int MetadataToken
        {
            get { return (int) this.method.MetadataToken.Value; }
        }

        public override Type ReturnType
        {
            get
            {
                return
                    this.methodDef.ReturnParameter.ParameterType.GetReflectionWrapper(this.genericTypeArguments,
                                                                                      this.genericMethodArguments);
            }
        }

        public override MethodInfo GetBaseDefinition()
        {
            return this;
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get { return new CustomAttributeProviderWrapper(this.methodDef.ReturnParameter); }
        }

        public override MethodAttributes Attributes
        {
            get { return this.methodDef.Attributes; }
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return this.methodDef.ImplementationAttributes;
        }

        public override ParameterInfo[] GetParameters()
        {
            return
                ReflectionWrapperUtil.GetMethodParameters(this.methodDef, this.genericTypeArguments,
                                                          this.genericTypeArguments);
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters,
                                      CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get { throw new NotSupportedException(); }
        }

        public override Type DeclaringType
        {
            get
            {
                TypeDefDeclaration internalDeclaringType = this.methodDef.DeclaringType;
                return internalDeclaringType != null ? internalDeclaringType.GetReflectionWrapper(null, null) : null;
            }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            MethodDefDeclaration cursor = this.methodDef;

            ArrayList arrayList = new ArrayList();

            while (cursor != null)
            {
                MethodDefDeclaration current = cursor;
                cursor = inherit ? cursor.GetParentDefinition() : null;

                IType internalAttributeType;
                if ( attributeType == null )
                    internalAttributeType = null;
                else
                {
                    internalAttributeType = (IType)current.Module.FindType(
                                                   attributeType, BindingOptions.OnlyExisting | BindingOptions.DisallowIntrinsicSubstitution);    
                    if ( internalAttributeType == null )
                        continue;
                }

                
                current.CustomAttributes.ConstructRuntimeObjects( internalAttributeType, arrayList );
               
            }

            return (object[]) arrayList.ToArray(attributeType ?? typeof(Attribute));
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return ReflectionWrapperUtil.GetCustomAttributes(
                this.methodDef);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            #region Preconditions
            ExceptionHelper.AssertArgumentNotNull(attributeType, "attributeType");
            #endregion

         
            MethodDefDeclaration cursor = this.methodDef;

            while ( cursor != null )
            {
                IType internalAttributeType = (IType)cursor.Module.FindType(
                                               attributeType, BindingOptions.OnlyExisting | BindingOptions.DontThrowException);

                if (internalAttributeType != null &&
                    cursor.CustomAttributes.Contains(internalAttributeType))
                {
                    return true;
                }

                cursor = inherit ? cursor.GetParentDefinition() : null;
            }

            return false;
            
        }

        public override string Name
        {
            get { return this.methodDef.Name; }
        }

        public override Type ReflectedType
        {
            get { throw new NotSupportedException(); }
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