using System;
using System.Reflection;
using PostSharp.Reflection;

namespace PostSharp.Laos
{
    /// <summary>
    /// Allows to turn a custom attribute into an aspect, while implementing the aspect in
    /// a different class. This allows to develop complex aspects even using the Compact
    /// Framework or Silverlight, because the aspect implementation is linked to the
    /// full .NET Framework.
    /// </summary>
    /// <remarks>
    /// Classes implementing this interface should be annotated with the 
    /// <see cref="ExternalAspectConfigurationAttribute"/> custom attribute.
    /// </remarks>
    public interface IExternalAspect : ILaosAspect
    {
    }


    /// <summary>
    /// Specifies the type name of the implementation of external aspects (<see cref="IExternalAspect"/>).
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public sealed class ExternalAspectConfigurationAttribute : LaosAspectConfigurationAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="ExternalAspectConfigurationAttribute"/>.
        /// In user code, use the <see cref="ExternalAspectConfigurationAttribute(string)"/>
        /// constructor instead of this one.
        /// </summary>
        public ExternalAspectConfigurationAttribute()
        {
            
        }

        /// <summary>
        /// Initializes a new <see cref="ExternalAspectConfigurationAttribute"/>.
        /// </summary>
        /// <param name="implementationTypeName">Assembly-qualified name of the implementation
        /// type. This type should implement the <see cref="IExternalAspectImplementation"/>
        /// interface.</param>
        public ExternalAspectConfigurationAttribute( string implementationTypeName )
        {
            this.ImplementationTypeName = implementationTypeName;
        }

        /// <summary>
        /// Gets the assembly-qualified name of the implementation  type.
        /// </summary>
        public string ImplementationTypeName { get; set; }
    }

#if !SMALL
    /// <summary>
    /// Implementation of an <see cref="IExternalAspect"/>.
    /// </summary>
    public interface IExternalAspectImplementation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">Target (<see cref="Type"/>, <see cref="FieldInfo"/>, <see cref="MethodInfo"/>,
        /// <see cref="ConstructorInfo"/>, <see cref="ParameterInfo"/>, ...) on which the
        /// aspect is applied.</param>
        /// <param name="aspectConstruction">Data about the source aspect instance 
        /// (the one implementing <see cref="IExternalAspect"/>).</param>
        /// <param name="aspects">Collection to which weavable aspects can be added. Implementations
        /// will typically use the <see cref="LaosReflectionAspectCollection.AddAspectConstruction(Type,IObjectConstruction,ILaosAspectConfiguration)"/> method.</param>
        void ImplementAspect( object target, IObjectConstruction aspectConstruction,
                              LaosReflectionAspectCollection aspects );
    }
#endif
}