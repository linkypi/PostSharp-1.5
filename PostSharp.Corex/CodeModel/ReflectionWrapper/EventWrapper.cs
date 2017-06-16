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
    internal sealed class EventWrapper : EventInfo, IReflectionWrapper<EventDeclaration>
    {
        private readonly EventDeclaration eventDecl;
        private readonly Type[] genericTypeArguments;
        private readonly Type[] genericMethodArguments;

        public EventWrapper( EventDeclaration eventDecl,
                             Type[] genericTypeArguments,
                             Type[] genericMethodArguments )
        {
            this.eventDecl = eventDecl;
            this.genericTypeArguments = genericTypeArguments;
            this.genericMethodArguments = genericMethodArguments;
        }

        public override EventAttributes Attributes { get { return this.eventDecl.Attributes; } }

        public override MethodInfo GetAddMethod( bool nonPublic )
        {
            return ReflectionWrapperUtil.GetMethodSemantic( this.eventDecl,
                                                            MethodSemantics.AddOn, nonPublic,
                                                            this.genericTypeArguments, this.genericMethodArguments );
        }

        public override MethodInfo GetRaiseMethod( bool nonPublic )
        {
            return ReflectionWrapperUtil.GetMethodSemantic( this.eventDecl,
                                                            MethodSemantics.Fire, nonPublic,
                                                            this.genericTypeArguments, this.genericMethodArguments );
        }

        public override MethodInfo GetRemoveMethod( bool nonPublic )
        {
            return ReflectionWrapperUtil.GetMethodSemantic( this.eventDecl,
                                                            MethodSemantics.RemoveOn, nonPublic,
                                                            this.genericTypeArguments, this.genericMethodArguments );
        }

        public override Type DeclaringType
        {
            get
            {
                return this.eventDecl.DeclaringType.GetReflectionWrapper(
                    this.genericTypeArguments, this.genericMethodArguments );
            }
        }

        public override object[] GetCustomAttributes( Type attributeType, bool inherit )
        {
            return ReflectionWrapperUtil.GetCustomAttributes( this.eventDecl );
        }

        public override object[] GetCustomAttributes( bool inherit )
        {
            return ReflectionWrapperUtil.GetCustomAttributes( this.eventDecl );
        }

        public override bool IsDefined( Type attributeType, bool inherit )
        {
            return ReflectionWrapperUtil.IsCustomAttributeDefined( this.eventDecl,
                                                                   attributeType );
        }

        public override string Name { get { return this.eventDecl.Name; } }

        public override Type ReflectedType { get { throw new NotSupportedException(); } }

        public override MethodInfo[] GetOtherMethods( bool nonPublic )
        {
            return ReflectionWrapperUtil.GetOtherMethodSemantics( this.eventDecl, nonPublic,
                                                                  this.genericTypeArguments, this.genericMethodArguments );
        }

        public override int MetadataToken { get { return (int) this.eventDecl.MetadataToken.Value; } }

        object IReflectionWrapper.UnderlyingSystemObject { get { throw new NotImplementedException(); } }
        public IAssemblyName DeclaringAssemblyName
        {
            get { return this.eventDecl.DeclaringAssembly; }
        }

        #region IReflectionWrapper<EventDeclaration> Members

        EventDeclaration IReflectionWrapper<EventDeclaration>.WrappedObject
        {
            get { return this.eventDecl; }
        }

        #endregion
    }
}