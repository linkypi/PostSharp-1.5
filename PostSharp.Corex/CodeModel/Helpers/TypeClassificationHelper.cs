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

namespace PostSharp.CodeModel.Helpers
{
    /// <summary>
    /// Helps to work with type classifications (<see cref="TypeClassifications"/>).
    /// </summary>
    public static class TypeClassificationHelper
    {
        internal static TypeClassifications GetRootTypeClassification( string name )
        {
            switch ( name )
            {
                case "System.Delegate":
                case "System.MulticastDelegate":
                    return TypeClassifications.Delegate;

                case "System.ValueType":
                    return TypeClassifications.Struct;

                case "System.Enum":
                    return TypeClassifications.Enum;

                case "System.Object":
                    return TypeClassifications.Class;

                default:
                    return TypeClassifications.Any;
            }
        }

        

        /// <summary>
        /// Determines how two type classifications intersect.
        /// </summary>
        /// <param name="actualTypeClassification">Actual classification.</param>
        /// <param name="requestedTypeSpecification">Requested classification.</param>
        /// <returns><see cref="NullableBool.True"/> if actual classifications are <i>included</i> in requested classification,
        /// <see cref="NullableBool.False"/> if both are <i>disjoint</i> and <see cref="NullableBool.Null"/> otherwise.</returns>
        public static NullableBool BelongsToClassification( TypeClassifications actualTypeClassification,
                                                            TypeClassifications requestedTypeSpecification )
        {
            if ( requestedTypeSpecification == actualTypeClassification ||
                 requestedTypeSpecification == TypeClassifications.Any )
            {
                return NullableBool.True;
            }
            else if ( actualTypeClassification == TypeClassifications.Any )
            {
                return NullableBool.Null;
            }
            else if ( requestedTypeSpecification == TypeClassifications.Module )
            {
                return requestedTypeSpecification == TypeClassifications.Module;
            }
            else if ( ( requestedTypeSpecification & TypeClassifications.ValueType ) != 0 )
            {
                if ( requestedTypeSpecification == TypeClassifications.ValueType )
                {
                    return ( actualTypeClassification & TypeClassifications.ValueType ) != 0;
                }
                else
                {
                    if ( actualTypeClassification == TypeClassifications.ValueType )
                    {
                        return NullableBool.Null;
                    }
                    else
                    {
                        return NullableBool.False;
                    }
                }
            }
            else if ( ( requestedTypeSpecification & TypeClassifications.ReferenceType ) != 0 )
            {
                if ( requestedTypeSpecification == TypeClassifications.ReferenceType )
                {
                    return ( actualTypeClassification & TypeClassifications.ReferenceType ) != 0;
                }
                else
                {
                    if ( actualTypeClassification == TypeClassifications.ReferenceType )
                    {
                        return NullableBool.Null;
                    }
                    else
                    {
                        return NullableBool.False;
                    }
                }
            }
            else if ( ( requestedTypeSpecification & TypeClassifications.Signature ) != 0 )
            {
                if ( requestedTypeSpecification == TypeClassifications.Signature )
                {
                    return ( actualTypeClassification & TypeClassifications.Signature ) != 0;
                }
                else
                {
                    if ( actualTypeClassification == TypeClassifications.Signature )
                    {
                        return NullableBool.Null;
                    }
                    else
                    {
                        return NullableBool.False;
                    }
                }
            }
            else
            {
                throw ExceptionHelper.CreateInvalidEnumerationValueException( requestedTypeSpecification,
                                                                              "requestedTypeSpecification" );
            }
        }
    }
}
