#if !SMALL
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PostSharp.Laos.Serializers
{
    /// <summary>
    /// When used as a value of <see cref="LaosWeavableAspectConfigurationAttribute"/>.<see cref="LaosWeavableAspectConfigurationAttribute.SerializerType"/>
    /// property, specifies that the aspect should not be serialized but should instead be constructed at runtime using MSIL instructions.
    /// </summary>
    /// <remarks>
    /// This class is <b>not</b> a serializer. When you use MSIL aspect construction, the aspect is built at runtime
    /// just as normal custom attributes, and any change made at build time is lost.
    /// </remarks>
    public sealed class MsilLaosSerializer : LaosSerializer
    {
        /// <inheritdoc />
        public override void Serialize( ILaosAspect[] aspects, Stream stream )
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override ILaosAspect[] Deserialize( Stream stream )
        {
            throw new NotSupportedException();
        }
    }
}
#endif