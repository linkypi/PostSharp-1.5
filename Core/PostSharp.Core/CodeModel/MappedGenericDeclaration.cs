using System;
using System.Collections.Generic;
using System.Text;

namespace PostSharp.CodeModel
{
    public struct MappedGenericDeclaration<T>
        where T : IGenericDefinition
    {
        private readonly T declaration;
        private readonly GenericMap genericMap;

        public MappedGenericDeclaration(T declaration, GenericMap genericMap)
        {
            this.declaration = declaration;
            this.genericMap = genericMap;
        }

        public T Declaration { get { return this.declaration;} }
        public GenericMap GenericMap { get { return this.genericMap; } }

        public MappedGenericDeclaration<T> Apply(GenericMap genericMap)
        {
            return new MappedGenericDeclaration<T>(this.declaration, this.genericMap.Apply( genericMap ));
        }

        public override string ToString()
        {
            return string.Format( "{{{0}; {1}}}", this.declaration, this.genericMap );
        }
    }
}
