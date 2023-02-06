using System.Text.Json.Nodes;

namespace BytingLib
{
    public class SamplerGL
    {
        public SamplerState SamplerState;

        public SamplerGL(JsonNode n)
        {
            int? magFilter = n["magFilter"]?.GetValue<int>();
            int? minFilter = n["minFilter"]?.GetValue<int>();
            TextureFilter textureFilter = GetTextureFilter(magFilter, minFilter);

            SamplerState = new SamplerState()
            {
                Filter = textureFilter,

            };

            var wrap = n["wrapS"];
            if (wrap != null)
            {
                int wrapInt = wrap.GetValue<int>();
                SamplerState.AddressU = GetAddressMode(wrapInt);
            }
            wrap = n["wrapT"];
            if (wrap != null)
            {
                int wrapInt = wrap.GetValue<int>();
                SamplerState.AddressV = GetAddressMode(wrapInt);
            }
        }

        public SamplerGL(SamplerState samplerState)
        {
            SamplerState = samplerState;
        }

        static TextureFilter GetTextureFilter(int? magFilter, int? minFilter)
        {
            TextureFilter textureFilter;
            switch (magFilter)
            {
                case null:
                case 9728: // NEAREST
                    switch (minFilter)
                    {
                        case null:
                        case 9728: // NEAREST
                            textureFilter = TextureFilter.Point;
                            break;
                        case 9984: // NEAREST_MIPMAP_NEAREST
                            textureFilter = TextureFilter.Point;
                            break;
                        case 9985: // LINEAR_MIPMAP_NEAREST
                            textureFilter = TextureFilter.MinLinearMagPointMipPoint;
                            break;
                        case 9987: // LINEAR_MIPMAP_LINEAR
                            textureFilter = TextureFilter.MinLinearMagPointMipLinear; ;
                            break;
                        case 9986: // NEAREST_MIPMAP_LINEAR
                        case 9729: // LINEAR
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                case 9729: // LINEAR
                    switch (minFilter)
                    {
                        case null:
                        case 9729: // LINEAR
                            textureFilter = TextureFilter.Linear;
                            break;
                        case 9986: // NEAREST_MIPMAP_LINEAR
                            textureFilter = TextureFilter.MinPointMagLinearMipLinear;
                            break;
                        case 9987: // LINEAR_MIPMAP_LINEAR
                            textureFilter = TextureFilter.Linear;
                            break;
                        case 9728: // NEAREST
                        case 9984: // NEAREST_MIPMAP_NEAREST
                        case 9985: // LINEAR_MIPMAP_NEAREST
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return textureFilter;
        }
        static TextureAddressMode GetAddressMode(int wrapInt)
        {
            return wrapInt switch
            {
                33071 => TextureAddressMode.Clamp,
                33648 => TextureAddressMode.Mirror,
                10497 => TextureAddressMode.Wrap,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
