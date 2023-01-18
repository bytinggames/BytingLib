using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public struct RenderSettingsOverride
    {
        public Color? Color = null;
        public Texture2D? Texture = null;
        public RasterizerState? Rasterizer = null;
        public SamplerState? SamplerState = null;
        public BlendState? BlendState = null;
        public DepthStencilState? DepthStencilState = null;
        public Effect? Effect = null;
        public PrimitiveType? PrimitiveType = null;

        public RenderSettingsOverride() { }
    }

    public class RenderSettings
    {
        public Color Color = Color.White;
        public Texture2D? Texture = null;
        public RasterizerState Rasterizer = RasterizerState.CullCounterClockwise;
        public SamplerState SamplerState = SamplerState.LinearWrap;
        public BlendState BlendState = BlendState.AlphaBlend;
        public DepthStencilState DepthStencilState = DepthStencilState.Default;
        public Effect Effect;
        public PrimitiveType PrimitiveType = PrimitiveType.TriangleList;

        public RenderSettings(Effect effect)
        {
            this.Effect = effect;
        }

        public void Override(RenderSettingsOverride settings)
        {
            Color = settings.Color ?? Color;
            Texture = settings.Texture ?? Texture;
            Rasterizer = settings.Rasterizer ?? Rasterizer;
            SamplerState = settings.SamplerState ?? SamplerState;
            BlendState = settings.BlendState ?? BlendState;
            DepthStencilState = settings.DepthStencilState ?? DepthStencilState;
            Effect = settings.Effect ?? Effect;
            PrimitiveType = settings.PrimitiveType ?? PrimitiveType;
        }

        public void Render<V>(V[] vertices, int vertexCount, int[] indices, int indexCount) 
            where V : struct, IVertexType
        {
            ApplySettings<V>();
            Effect.Render(vertices, vertexCount, indices, indexCount, PrimitiveType, Texture);
        }

        public void ApplySettingsToGraphicsDevice()
        {
            Effect.GraphicsDevice.Textures[0] = Texture;
            Effect.GraphicsDevice.RasterizerState = Rasterizer;
            Effect.GraphicsDevice.SamplerStates[0] = SamplerState;
            Effect.GraphicsDevice.BlendState = BlendState;
            Effect.GraphicsDevice.DepthStencilState = DepthStencilState;
        }

        public void ApplySettings<V>() where V : struct, IVertexType
        {
            ApplySettingsToGraphicsDevice();
            Effect.Parameters["Color"].SetValue(Color.ToVector4());
            Effect.CurrentTechnique = Effect.Techniques["Render" + typeof(V).Name.Substring("VertexPosition".Length)];
        }

        public RenderSettings Clone()
        {
            return (RenderSettings)MemberwiseClone();
        }
    }
}
