using System.IO;
using System.Text;
using PostSharp.CodeModel;
using PostSharp.ModuleReader;

namespace PostSharp.ModuleWriter
{
    internal static class SerializationUtil
    {
        public static void WriteSerString( string value, BinaryWriter writer )
        {
            if ( value == null )
            {
                writer.Write( (byte) 0xFF );
                return;
            }

            byte[] bytes = Encoding.UTF8.GetBytes( value );
            WriteCompressedInteger( (uint) bytes.Length, writer );
            writer.Write( bytes );
        }

        public static void WriteCompressedInteger( uint value, BinaryWriter writer )
        {
            if (value <= 0x7F)
            {
                writer.Write((byte) value);
            }
            else if (value <= 0x3FFF)
            {
                ushort v = (ushort)((1 << 15) | value);
                writer.Write((byte)(v >> 8));
                writer.Write((byte)(v & 0xFF));
            }
            else
            {
                uint v = (uint) ((3 << 30) | value);
                writer.Write((byte) (v >> 24));
                writer.Write((byte) ((v >> 16) & 0xFF));
                writer.Write((byte) ((v >> 8) & 0xFF));
                writer.Write((byte)(v & 0xFF));
            }
        }

        public static void SerializeCustomAttribute( CustomAttributeDeclaration customAttribute, BinaryWriter writer )
        {
            // Write the prolog
            writer.Write( (short) 1 );


            for ( int j = 0 ; j < customAttribute.ConstructorArguments.Count ; j++ )
            {
                customAttribute.ConstructorArguments[j].Value.WriteValue( writer );
            }

            // Write the optional arguments
            writer.Write( (short) customAttribute.NamedArguments.Count );

            for ( int j = 0 ; j < customAttribute.NamedArguments.Count ; j++ )
            {
                WriteSerializedNamedArgument( customAttribute.NamedArguments[j], writer );
            } // loop named arguments
        }


        public static void WriteSerializedNamedArgument( MemberValuePair pair, BinaryWriter writer )
        {
            // Write the member kind.
            writer.Write( pair.MemberKind == MemberKind.Field
                              ? (byte) CorSerializationType.TypeField
                              : (byte) CorSerializationType.TypeProperty );

            pair.Value.Type.WriteType( writer );
            WriteSerString( pair.MemberName, writer );
            pair.Value.WriteValue( writer );
        }
    }
}
