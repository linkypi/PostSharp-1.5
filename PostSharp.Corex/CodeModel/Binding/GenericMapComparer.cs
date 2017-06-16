using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PostSharp.CodeModel.Binding
{
    public sealed class GenericMapComparer : IEqualityComparer<GenericMap>, IEqualityComparer
    {
        private readonly TypeComparer typeComparer = TypeComparer.GetInstance();
        private static readonly GenericMapComparer instance = new GenericMapComparer();

        public static GenericMapComparer GetInstance() 
        {
            return instance;
        }

        private GenericMapComparer()
        {
            
        }

        public bool Equals( GenericMap x, GenericMap y )
        {
            if ( x.GenericMethodParameterCount != y.GenericMethodParameterCount )
                return false;

            if ( x.GenericTypeParameterCount != y.GenericTypeParameterCount )
                return false;

            for ( int i = 0; i < x.GenericMethodParameterCount; i++ )
            {
                if ( !this.typeComparer.Equals( x.GetGenericMethodParameter( i ),
                                                y.GetGenericMethodParameter( i ) ) )
                    return false;
            }

            for ( int i = 0; i < x.GenericTypeParameterCount; i++ )
            {
                if ( !this.typeComparer.Equals( x.GetGenericTypeParameter( i ),
                    y.GetGenericTypeParameter( i )))
                    return false;
            }

            return true;
        }

        public int GetHashCode( GenericMap obj )
        {
            int hashCode = HashCodeHelper.CombineHashCodes( obj.GenericMethodParameterCount, obj.GenericTypeParameterCount );
            
            for ( int i = 0; i < obj.GenericMethodParameterCount; i++)
            {
                HashCodeHelper.CombineHashCodes( ref hashCode, this.typeComparer.GetHashCode( obj.GetGenericMethodParameter( i ) ) );
            }

            for (int i = 0; i < obj.GenericTypeParameterCount; i++)
            {
                HashCodeHelper.CombineHashCodes(ref hashCode, this.typeComparer.GetHashCode(obj.GetGenericTypeParameter(i)));
            }

            return hashCode;
        }

        public new bool Equals( object x, object y )
        {
            return this.Equals( (GenericMap) x, (GenericMap) y );
        }

        public int GetHashCode( object obj )
        {
            return this.GetHashCode( (GenericMap) obj );
        }
    }
}
