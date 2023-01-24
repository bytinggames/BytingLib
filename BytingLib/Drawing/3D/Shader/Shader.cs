using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public abstract class Shader : IShaderTexWorld, IDisposable
    {
        protected readonly Ref<Effect> effect;
        protected readonly GraphicsDevice gDevice;
        protected Matrix view, projection;

        protected List<IEffectParameterStack> parameters = new();

        protected void Add(IEffectParameterStack parameter) => parameters.Add(parameter);

        public abstract EffectParameterStack<Matrix> World { get; }
        public abstract EffectParameterStack<Texture2D> ColorTex { get; }

        public Effect Effect => effect.Value;

        public Shader(Ref<Effect> effect)
        {
            this.effect = effect;
            gDevice = effect.Value.GraphicsDevice;
        }

        public void ApplyParameters()
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i].Apply();
            }
        }

        public void DrawFull(Matrix view, Matrix projection, Action draw)
        {
            this.view = view;
            this.projection = projection;

            DrawFullChild(draw);
        }

        protected virtual void DrawFullChild(Action draw)
        {
            draw();
        }

        public void Draw(Model model)
        {
            var e = effect.Value;

            foreach (var mesh in model.Meshes)
            {
                using (World.Use(f => mesh.ParentBone.Transform * f))
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        switch (part.VertexBuffer.VertexDeclaration.VertexStride)
                        {
                            case 24:
                                e.CurrentTechnique = e.Techniques["RenderNormal"];
                                break;
                            case 28:
                                e.CurrentTechnique = e.Techniques["RenderColorNormal"];
                                break;
                            case 36:
                                e.CurrentTechnique = e.Techniques["RenderColorNormalTexture"];
                                break;
                            default: // 32
                                e.CurrentTechnique = e.Techniques["RenderNormalTexture"];
                                break;
                        }

                        gDevice.SetVertexBuffer(part.VertexBuffer);
                        gDevice.Indices = part.IndexBuffer;

                        var basicEffect = (part.Effect as BasicEffect)!;
                        var texture = basicEffect.Texture;
                        using (ColorTex.Use(texture))
                        {
                            ApplyParameters();
                            foreach (var pass in e.CurrentTechnique.Passes)
                            {
                                pass.Apply();
                                gDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                    part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                            }
                        }
                    }
                }
            }
        }

        public void DrawTriangles<V>(V[] vertices) where V : struct, IVertexType
        {
            var e = effect.Value;

            string vertexName = typeof(V).Name;
            string technique = "Render" + vertexName.Substring("VertexPosition".Length);

            e.CurrentTechnique = e.Techniques[technique];

            ApplyParameters();
            foreach (var pass in e.CurrentTechnique.Passes)
            {
                pass.Apply();
                gDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
            }
        }

        public IDisposable UseRasterizer(RasterizerState rasterizerState)
        {
            var storeVal = gDevice.RasterizerState;
            gDevice.RasterizerState = rasterizerState;

            return new OnDispose(() => gDevice.RasterizerState = storeVal);
        }
        public IDisposable UseSampler(SamplerState samplerState, int index = 0)
        {
            var storeVal = gDevice.SamplerStates[index];
            gDevice.SamplerStates[index] = samplerState;

            return new OnDispose(() => gDevice.SamplerStates[index] = storeVal);
        }
        public IDisposable UseBlend(BlendState blendState)
        {
            var storeVal = gDevice.BlendState;
            gDevice.BlendState = blendState;

            return new OnDispose(() => gDevice.BlendState = storeVal);
        }
        public IDisposable UseDepthStencil(DepthStencilState depthStencilState)
        {
            var storeVal = gDevice.DepthStencilState;
            gDevice.DepthStencilState = depthStencilState;

            return new OnDispose(() => gDevice.DepthStencilState = storeVal);
        }
        public IDisposable UseScissorsRectangle(Rectangle scissorsRectangle)
        {
            var storeVal = gDevice.ScissorRectangle;
            gDevice.ScissorRectangle = scissorsRectangle;

            return new OnDispose(() => gDevice.ScissorRectangle = storeVal);
        }

        public void Dispose()
        {
            foreach (var p in parameters)
                p.Dispose();
        }
    }
}
