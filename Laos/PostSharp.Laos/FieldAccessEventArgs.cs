#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

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
using System.Diagnostics;
using System.Reflection;

namespace PostSharp.Laos
{
    /// <summary>
    /// Arguments of field-related join points. Used by
    /// <see cref="OnFieldAccessAspect"/>.
    /// </summary>
    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    public sealed class FieldAccessEventArgs : InstanceBoundLaosEventArgs
    {
        private readonly FieldInfo fieldInfo;
        private readonly Type declaringType;

        /// <summary>
        /// Initializes a new <see cref="FieldAccessEventArgs"/>.
        /// </summary>
        /// <param name="fieldInfo">Accessed field instance.</param>
        /// <param name="instance">Accessed instance, or <b>null</b> if the field is static.</param>
        /// <param name="exposedFieldValue">Exposed field value (<b>null</b> for get operations).</param>
        /// <param name="storedFieldValue">Value currently effectively stored in the field.</param>
        public FieldAccessEventArgs( object exposedFieldValue, object storedFieldValue, FieldInfo fieldInfo,
                                     object instance ) :
                                         base( instance )
        {
            this.fieldInfo = fieldInfo;
            this.ExposedFieldValue = exposedFieldValue;
            this.StoredFieldValue = storedFieldValue;
            if ( fieldInfo != null )
            {
                this.declaringType = fieldInfo.DeclaringType;
            }
        }

        /// <summary>
        /// Gets the field being accessed.
        /// </summary>
        /// <remarks>
        /// If the type declaring this field is a generic type, this property
        /// contains the concrete generic field instance.
        /// </remarks>
        public FieldInfo FieldInfo
        {
            get { return fieldInfo; }
        }

        /// <summary>
        /// Gets or sets the field value that is <i>exposed</i> to the external
        /// code. Do not confuse with <see cref="StoredFieldValue"/>, which is
        /// the value effectively stored.
        /// </summary>
        public object ExposedFieldValue { get; set; }

        /// <summary>
        /// Gets or sets the field value that is effectively <i>stored</i> in the
        /// field location. Do not confuse with <see cref="ExposedFieldValue"/>, which is
        /// the value that is 'seen' by the code.
        /// </summary>
        /// <remarks>
        /// Setting this property does not immediately set the underlying field.
        /// The field is typically set after the execution of advices located at the
        /// join point. This remark is important for multi-threaded applications.
        /// </remarks>
        public object StoredFieldValue { get; set; }


        /// <summary>
        /// Gets the type declaring the accessed field.
        /// </summary>
        /// <remarks>
        /// If the declaring type is generic, this property contains the concrete
        /// generic type instance being accessed.
        /// </remarks>
        public Type DeclaringType
        {
            get { return declaringType; }
        }
    }
}