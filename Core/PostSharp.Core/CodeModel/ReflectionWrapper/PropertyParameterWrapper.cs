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
    internal sealed class PropertyParameterWrapper : ParameterWrapper
    {
        private readonly PropertyWrapper propertyWrapper;

        public PropertyParameterWrapper(
            PropertyWrapper propertyWrapper,
            ParameterDeclaration param,
            Type[] genericTypeArguments,
            Type[] genericMethodArguments )
            : base( param, genericTypeArguments, genericMethodArguments )
        {
            this.propertyWrapper = propertyWrapper;
        }

        public override MemberInfo Member { get { return this.propertyWrapper; } }


    }
}