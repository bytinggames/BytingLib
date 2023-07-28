namespace BytingLib
{
    static class BufferHelper
    {
        internal static int GetNumberOfComponents(string type)
        {
            return type switch
            {
                "SCALAR" => 1,
                "VEC2" => 2,
                "VEC3" => 3,
                "VEC4" => 4,
                "MAT2" => 4,
                "MAT3" => 9,
                "MAT4" => 16,
                _ => throw new NotImplementedException(),
            };
        }

        internal static VertexElementFormat GetVertexElementFormatFromAccessorType(string type, Type componentType)
        {
            switch (type)
            {
                case "SCALAR":
                    return VertexElementFormat.Single;
                case "VEC2":
                    if (componentType == typeof(float))
                        return VertexElementFormat.Vector2;
                    else if (componentType == typeof(short))
                        return VertexElementFormat.Short2;
                    break;
                case "VEC3":
                    if (componentType == typeof(float))
                        return VertexElementFormat.Vector3;
                    break;
                case "VEC4":
                    if (componentType == typeof(float))
                        return VertexElementFormat.Vector4;
                    else if (componentType == typeof(ushort))
                        return VertexElementFormat.Color;
                    else if (componentType == typeof(byte))
                        return VertexElementFormat.Byte4;
                    else if (componentType == typeof(short))
                        return VertexElementFormat.Short4;
                    break;
            }
            throw new NotImplementedException();
        }

        internal static VertexElementUsage GetVertexElementUsageFromAttributeName(string attributeName)
        {
            return attributeName switch
            {
                "POSITION" => VertexElementUsage.Position,
                "NORMAL" => VertexElementUsage.Normal,
                "TEXCOORD_0" or "TEXCOORD_1" => VertexElementUsage.TextureCoordinate,
                "COLOR_0" => VertexElementUsage.Color,
                "JOINTS_0" => VertexElementUsage.BlendIndices,
                "WEIGHTS_0" => VertexElementUsage.BlendWeight,
                "TANGENT" => VertexElementUsage.Tangent,
                _ => throw new NotImplementedException()
            };
        }

        internal static Type GetComponentType(int componentType)
        {
            return componentType switch
            {
                5120 => typeof(sbyte),
                5121 => typeof(byte),
                5122 => typeof(short),
                5123 => typeof(ushort),
                5125 => typeof(uint),
                5126 => typeof(float),
                _ => throw new NotImplementedException(),
            };
        }
        internal static int GetComponentSizeInBytes(int componentType)
        {
            return componentType switch
            {
                5120 => 1,
                5121 => 1,
                5122 => 2,
                5123 => 2,
                5125 => 4,
                5126 => 4,
                _ => throw new NotImplementedException(),
            };
        }

        internal static int Convert16BitColorChannelTo8Bit(ref byte[] bufferBytes)
        {
            int componentSize;
            byte[] newBufferBytes = new byte[bufferBytes.Length / 2];
            componentSize = 1;

            for (int i = 0; i < newBufferBytes.Length; i++)
            {
                newBufferBytes[i] = bufferBytes[i * 2 + 1]; // ushort to byte
            }

            bufferBytes = newBufferBytes;
            return componentSize;
        }

        const int MatrixStructSize = 16 * 4;
        internal static Matrix[] ByteArrayToMatrixArray(byte[] bytes)
            => ByteExtension.ByteArrayToStructArray<Matrix>(bytes, MatrixStructSize);
    }
}
