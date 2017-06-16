#region #region Copyright (c) 2004-2010 by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

using System;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// <b>Internal Only.</b> Custom attribute used internally by PostSharp to mark
    /// elements having inherited custom attributes. This custom attribute should not
    /// be used in custom code, otherwise PostSharp may not work properly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Interface |
                    AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue,
        AllowMultiple = false, Inherited = true)]
    [CLSCompliant(false)]
    public sealed class HasInheritedAttributeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="HasInheritedAttributeAttribute"/>.
        /// </summary>
        /// <param name="ids">List of pooled inherited instances present on the target element
        /// of the current <see cref="HasInheritedAttributeAttribute"/> instance.</param>
        [Obsolete("Do not use this custom attribute in user code.", true)]
        public HasInheritedAttributeAttribute(long[] ids)
        {
            
        }
        
    }
}