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
using System.Reflection;
using System.Text;
using PostSharp.Reflection;

namespace PostSharp.CodeModel.ReflectionWrapper
{
    internal sealed class TypeWrapper : BaseTypeWrapper
    {
        private readonly IType type;

        public TypeWrapper( IType type ) :
                                base( type.GetTypeDefinition(), type.GetGenericContext( GenericContextOptions.None ),
                                      null, null)
        {
            this.type = type;
        }


        public override Type DeclaringType
        {
            get
            {
                ITypeSignature declaringType = this.type.DeclaringType;
                return declaringType == null
                           ? null
                           : declaringType.GetReflectionWrapper(
                                 this.genericTypeArguments, this.genericMethodArguments );
            }
        }

        public override bool ContainsGenericParameters { get { return this.type.ContainsGenericArguments(); } }

        public override int MetadataToken { get { return (int) this.type.MetadataToken.Value; } }

        public override bool IsGenericType { get { return this.type.IsGenericDefinition; } }

        public override bool IsGenericTypeDefinition { get { return this.type.IsGenericDefinition; } }

        public override Type[] GetGenericArguments()
        {
            return ReflectionWrapperUtil.GetGenericArguments( this.type, null, null );
        }

        public override Type GetGenericTypeDefinition()
        {
            if ( this.type.IsGenericInstance )
            {
                return this.type.GetTypeDefinition().GetReflectionWrapper( null, null );
            }
            else
            {
                return null;
            }
        }


        public override string FullName
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                this.type.WriteReflectionTypeName( builder, ReflectionNameOptions.IgnoreGenericTypeDefParameters );
                return builder.ToString();
            }
        }


        public override Type GetNestedType( string name, BindingFlags bindingAttr )
        {
            TypeDefDeclaration nestedType = this.typeDef.Types.GetByName( name );

            if ( nestedType != null )
            {
                return nestedType.GetReflectionWrapper( this.genericTypeArguments, this.genericMethodArguments );
            }

            return null;
        }

        public override Type[] GetNestedTypes( BindingFlags bindingAttr )
        {
            List<Type> types = new List<Type>( this.typeDef.Types.Count );

            foreach ( TypeDefDeclaration nestedType in this.typeDef.Types )
            {
                types.Add( nestedType.GetReflectionWrapper( this.genericTypeArguments, this.genericMethodArguments ) );
            }

            return types.ToArray();
        }


        public override Type UnderlyingSystemType { get { return this.typeDef.GetSystemType( this.genericTypeArguments, this.genericMethodArguments ); } }


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
    }
}
