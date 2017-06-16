using System;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Type classifications are predicate that applies on types.
    /// </summary>
    [Flags]
    public enum TypeClassifications
    {
        /// <summary>
        /// No classification.
        /// </summary>
        None = 0,

        /// <summary>
        /// Interface.
        /// </summary>
        Interface = 1,

        /// <summary>
        /// Class.
        /// </summary>
        Class = 2,

        /// <summary>
        /// Value type (other than <see cref="Enum"/>).
        /// </summary>
        Struct = 4,

        /// <summary>
        /// Enumeration.
        /// </summary>
        Enum = 8,

        /// <summary>
        /// The type representing the module.
        /// </summary>
        Module = 16,

        /// <summary>
        /// Delegate.
        /// </summary>
        Delegate = 32,

        /// <summary>
        /// Pointer (managed or unmanaged).
        /// </summary>
        Pointer = 64,

        /// <summary>
        /// Pointer to method.
        /// </summary>
        MethodPointer = 128,

        /// <summary>
        /// Array.
        /// </summary>
        Array = 256,

        /// <summary>
        /// Boxed value type.
        /// </summary>
        Boxed = 512,

        /// <summary>
        /// Generic type instance.
        /// </summary>
        GenericTypeInstance = 1024,

        /// <summary>
        /// Generic parameter.
        /// </summary>
        GenericParameter = 2048,

        /// <summary>
        /// Intrinsic.
        /// </summary>
        /// <remarks>
        /// An intrinsic type is defined by the VRE. Other are defined by libraries.
        /// </remarks>
        Intrinsic = 4096,

        /// <summary>
        /// Any type represented by a signature (not directly a type definition or type reference).
        /// </summary>
        Signature = Pointer | MethodPointer | Array | Boxed | GenericTypeInstance | GenericParameter | Intrinsic,

        /// <summary>
        /// Reference (heap) type.
        /// </summary>
        ReferenceType = Interface | Class | Array | Boxed | Delegate,

        /// <summary>
        /// Value (stack) type.
        /// </summary>
        ValueType = Struct | Enum | Pointer | MethodPointer,

        /// <summary>
        /// Any type.
        /// </summary>
        Any = ReferenceType | ValueType
    }
}