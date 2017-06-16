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
    internal sealed class FieldWrapper : FieldInfo, IReflectionWrapper<FieldDefDeclaration>
    {
        private readonly IField field;
        private readonly FieldDefDeclaration fieldDef;
        private readonly Type[] genericTypeArguments;
        private readonly Type[] genericMethodArguments;

        public FieldWrapper( IField field, Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            this.field = field;
            this.fieldDef = field.GetFieldDefinition();
            ReflectionWrapperUtil.GetGenericMapWrapper( field.GetGenericContext( GenericContextOptions.None ),
                                                        genericTypeArguments, genericMethodArguments,
                                                        out this.genericTypeArguments, out this.genericMethodArguments );
        }

        public override int MetadataToken { get { return (int) this.field.MetadataToken.Value; } }

        public override FieldAttributes Attributes { get { return this.fieldDef.Attributes; } }

        public override RuntimeFieldHandle FieldHandle { get { throw new NotSupportedException(); } }

        public override Type FieldType
        {
            get
            {
                return
                    this.fieldDef.FieldType.GetReflectionWrapper( this.genericTypeArguments, this.genericMethodArguments );
            }
        }

        public override object GetValue( object obj )
        {
            throw new NotSupportedException();
        }

        public override void SetValue( object obj, object value, BindingFlags invokeAttr, Binder binder,
                                       CultureInfo culture )
        {
            throw new NotSupportedException();
        }

        public override Type DeclaringType
        {
            get
            {
                return
                    this.fieldDef.DeclaringType.GetReflectionWrapper( this.genericTypeArguments,
                                                                      this.genericMethodArguments );
            }
        }

        public override object[] GetCustomAttributes( Type attributeType, bool inherit )
        {
            return ReflectionWrapperUtil.GetCustomAttributes(
                this.fieldDef, attributeType );
        }

        public override object[] GetCustomAttributes( bool inherit )
        {
            return ReflectionWrapperUtil.GetCustomAttributes(
                this.fieldDef );
        }

        public override bool IsDefined( Type attributeType, bool inherit )
        {
            return ReflectionWrapperUtil.IsCustomAttributeDefined(
                this.fieldDef, attributeType );
        }

        public override string Name { get { return this.fieldDef.Name; } }

        public override Type ReflectedType { get { throw new NotSupportedException(); } }

        public override object GetRawConstantValue()
        {
            SerializedValue serializedValue = this.fieldDef.LiteralValue;
            if ( serializedValue != null )
            {
                return serializedValue.GetRuntimeValue();
            }
            else
            {
                if ( this.FieldType.IsValueType )
                {
                    return Activator.CreateInstance( this.FieldType );
                }
                else
                {
                    return null;
                }
            }
        }

        object IReflectionWrapper.UnderlyingSystemObject { get { return this.field.GetSystemField(this.genericTypeArguments, this.genericMethodArguments); } }
        public IAssemblyName DeclaringAssemblyName
        {
            get { return this.fieldDef.DeclaringAssembly; }
        }

        #region IReflectionWrapper<FieldDefDeclaration> Members

        FieldDefDeclaration IReflectionWrapper<FieldDefDeclaration>.WrappedObject
        {
            get { return this.fieldDef; }
        }

        #endregion
    }
}