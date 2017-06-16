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
using System.Reflection;

namespace PostSharp.CodeModel.ReflectionWrapper
{
    internal class ParameterWrapper : ParameterInfo, IReflectionWrapper<ParameterDeclaration>
    {
        private readonly ParameterDeclaration param;
        private readonly Type[] genericTypeArguments;
        private readonly Type[] genericMethodArguments;

        public ParameterWrapper( ParameterDeclaration param, Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            this.param = param;
            this.genericTypeArguments = genericTypeArguments;
            this.genericMethodArguments = genericMethodArguments;
        }


        public override ParameterAttributes Attributes { get { return this.param.Attributes; } }

        public override string Name { get { return this.param.Name; } }

        public override MemberInfo Member
        {
            get
            {
                MethodDefDeclaration methodDef = this.param.DeclaringMethod;
                
                if ( methodDef.Name == ".ctor")
                {
                    return new MethodConstructorInfoWrapper( methodDef, this.genericTypeArguments, null );
                }
                else
                {
                    return
                        new MethodMethodInfoWrapper( methodDef, this.genericTypeArguments, this.genericMethodArguments );
                }
            }
        }

        public override Type ParameterType
        {
            get
            {
                return
                    this.param.ParameterType.GetReflectionWrapper( this.genericTypeArguments,
                                                                   this.genericMethodArguments );
            }
        }

        public override int Position { get { return this.param.Ordinal; } }

        public override object[] GetCustomAttributes( bool inherit )
        {
            return ReflectionWrapperUtil.GetCustomAttributes( this.param );
        }

        public override object[] GetCustomAttributes( Type attributeType, bool inherit )
        {
            return ReflectionWrapperUtil.GetCustomAttributes( this.param, attributeType );
        }

        public override bool IsDefined( Type attributeType, bool inherit )
        {
            return ReflectionWrapperUtil.IsCustomAttributeDefined( this.param, attributeType );
        }

        object IReflectionWrapper.UnderlyingSystemObject { get { return this.param.GetReflectionWrapper(this.genericTypeArguments, this.genericMethodArguments); } }
        public IAssemblyName DeclaringAssemblyName
        {
            get { return this.param.DeclaringAssembly; }
        }

        #region IReflectionWrapper<ParameterDeclaration> Members

        ParameterDeclaration IReflectionWrapper<ParameterDeclaration>.WrappedObject
        {
            get { return this.param; }
        }

        #endregion
    }
}