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
using System.Text;
using PostSharp.CodeModel.TypeSignatures;

namespace PostSharp.CodeModel.ReflectionWrapper
{
    internal sealed class GenericTypeInstanceTypeSignatureWrapper : BaseTypeWrapper,
         IReflectionWrapper<ITypeSignature>
    {
        private readonly GenericTypeInstanceTypeSignature type;

        public GenericTypeInstanceTypeSignatureWrapper( GenericTypeInstanceTypeSignature type,
                                                        Type[] genericTypeArguments, Type[] genericMethodArguments ) :
                                                            base(
                                                            type.GenericDefinition.GetTypeDefinition(),
                                                            type.GetGenericContext( GenericContextOptions.None ),
                                                            genericTypeArguments, genericMethodArguments )
        {
            this.type = type;
        }


        public override string FullName { get { return null; } }

        public override Type GetNestedType( string name, BindingFlags bindingAttr )
        {
            return null;
        }

        public override Type[] GetNestedTypes( BindingFlags bindingAttr )
        {
            return EmptyTypes;
        }


        public override Type UnderlyingSystemType { get { return this.type.GetSystemType( this.genericTypeArguments, this.genericMethodArguments ); } }

        public override Type[] GetGenericArguments()
        {
            return this.genericTypeArguments;
        }

        object IReflectionWrapper.UnderlyingSystemObject { get { return this.UnderlyingSystemType; } }


        public override string ToString()
        {
            // TODO: Resolve generic parameters. Unfortunately this cannot be done in the current design.
            StringBuilder stringBuilder = new StringBuilder();
            this.type.WriteReflectionTypeName( stringBuilder, ReflectionNameOptions.UseBracketsForGenerics );
            return stringBuilder.ToString();
        }

        public override bool IsGenericType
        {
            get
            {
                return true;
            }
        }

        public override Type GetGenericTypeDefinition()
        {
            return this.typeDef.GetReflectionWrapper(null, null);
        }

        #region IReflectionWrapper<ITypeSignature> Members

        ITypeSignature IReflectionWrapper<ITypeSignature>.WrappedObject
        {
            get { return this.type; }
        }

        #endregion

    }
}