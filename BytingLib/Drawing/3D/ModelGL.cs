using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace BytingLib
{
    public interface IShaderGL : IShaderTexWorld
    {
        EffectParameterStack<Vector4> Color { get; }
    }


    public interface IDrawShaderGL
    {
        void Draw(IShaderGL shader);
    }

    class Node
    {
        public Mesh? Mesh;
        public Matrix Transform;
        public string? Name;
        public List<Node> Children = new();

        public Node(Matrix transform)
        {
            Transform = transform;
        }

        internal void Draw(IShaderGL shader)
        {
            using (shader.World.Use(f => Transform * f))
            {
                Mesh?.Draw(shader);
                for (int i = 0; i < Children.Count; i++)
                    Children[i].Draw(shader);
            }
        }

        public override string? ToString()
        {
            if (Name == null)
                return base.ToString();
            return "Node " + Name;
        }

        internal void ApplyTransform(Matrix matrix)
        {
            Transform *= matrix;
        }
    }

    class Mesh
    {
        public string? Name;
        public List<Primitive> Primitives = new();

        public void Draw(IShaderGL shader)
        {
            for (int i = 0; i < Primitives.Count; i++)
            {
                Primitives[i].Draw(shader);
            }
        }

        public override string? ToString()
        {
            if (Name == null)
                return base.ToString();
            return "Mesh " + Name;
        }
    }

    class Primitive
    {
        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;
        public Material? Material;

        public Primitive(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, Material? material)
        {
            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;
            Material = material;
        }

        public void Draw(IShaderGL shader)
        {
            string techniqueName = Shader.GetTechniqueName(VertexBuffer.VertexDeclaration);

            using (shader.UseTechnique(techniqueName))
            {
                using (Material?.Use(shader))
                    shader.Draw(VertexBuffer, IndexBuffer);
            }
        }
    }

    
    class Material
    {
        public string? Name;
        public PbrMetallicRoughness? PbrMetallicRoughness;
        public RasterizerState? RasterizerState;

        public IDisposable? Use(IShaderGL shader)
        {
            DisposableContainer disposables = new();
            if (PbrMetallicRoughness != null)
                disposables.Use(PbrMetallicRoughness.Use(shader));
            if (RasterizerState != null)
                disposables.Use(shader.UseRasterizer(RasterizerState));
            return disposables;
        }

        public override string? ToString()
        {
            if (Name == null)
                return base.ToString();
            return "Material " + Name;
        }
    }

    class PbrMetallicRoughness
    {
        public Vector4? BaseColor;
        public TextureGL BaseColorTexSampler;
        public float MetallicFactor;
        public float RoughnessFactor;

        public IDisposable Use(IShaderGL shader)
        {
            DisposableContainer toDispose = new();
            if (BaseColor != null)
                toDispose.UseCheckNull(shader.Color.Use(BaseColor.Value));
            if (BaseColorTexSampler != null)
            {
                toDispose.Use(shader.UseSampler(BaseColorTexSampler.SamplerState));
                toDispose.UseCheckNull(shader.ColorTex.Use(BaseColorTexSampler.Texture.Value));
            }
            return toDispose;
        }
    }

    class TextureGL
    {
        public Ref<Texture2D> Texture;
        public SamplerState SamplerState;

        public TextureGL(Ref<Texture2D> texture, SamplerState samplerState)
        {
            Texture = texture;
            SamplerState = samplerState;
        }
    }

    public class ModelGL : IDrawShaderGL, IDisposable
    {
        List<Node> nodes = new();

        DisposableContainer disposables = new();

        int GetNumberOfComponents(string type)
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

        VertexElementFormat GetVertexElementFormatFromAccessorType(string type, Type componentType)
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

        private int GetVertexElementSizeFromFormat(VertexElementFormat vertexElementFormat, Type componentType)
        {
            return GetIndexCount(vertexElementFormat);
        }

        private static int GetIndexCount(VertexElementFormat vertexElementFormat)
        {
            return vertexElementFormat switch
            {
                VertexElementFormat.Single => 4,
                VertexElementFormat.Vector2 => 8,
                VertexElementFormat.Vector3 => 12,
                VertexElementFormat.Vector4 => 16,
                VertexElementFormat.Color => 4,
                VertexElementFormat.Byte4 => 4,
                VertexElementFormat.Short2 => 4,
                VertexElementFormat.Short4 => 8,
                VertexElementFormat.NormalizedShort2 => 4,
                VertexElementFormat.NormalizedShort4 => 8,
                VertexElementFormat.HalfVector2 => 4,
                VertexElementFormat.HalfVector4 => 8,
                _ => throw new NotImplementedException(),
            };

            /*
                VertexElementFormat.Single => 4,
                VertexElementFormat.Vector2 => 8,
                VertexElementFormat.Vector3 => 12,
                VertexElementFormat.Vector4 => 16,
                VertexElementFormat.Color => 4,
                VertexElementFormat.Byte4 => 4,
                VertexElementFormat.Short2 => 4,
                VertexElementFormat.Short4 => 8,
                VertexElementFormat.NormalizedShort2 => 4,
                VertexElementFormat.NormalizedShort4 => 8,
                VertexElementFormat.HalfVector2 => 4,
                VertexElementFormat.HalfVector4 => 8,*/
        }

        VertexElementUsage GetVertexElementUsageFromAttributeName(string attributeName)
        {
            return attributeName switch
            {
                "POSITION" => VertexElementUsage.Position,
                "NORMAL" => VertexElementUsage.Normal,
                "TEXCOORD_0" => VertexElementUsage.TextureCoordinate,
                "COLOR_0" => VertexElementUsage.Color,
                "JOINTS_0" => VertexElementUsage.BlendIndices,
                "WEIGHTS_0" => VertexElementUsage.BlendWeight,
                _ => throw new NotImplementedException(),
            };
        }

        Type GetComponentType(int componentType)
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
        int GetComponentSizeInBytes(int componentType)
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

        class VertexPart
        {
            public VertexPart(VertexElement vertexElement, byte[] bufferBytes, int vertexElementSize)
            {
                VertexElement = vertexElement;
                BufferBytes = bufferBytes;
                VertexElementSize = vertexElementSize;
            }

            public VertexElement VertexElement { get; }
            public byte[] BufferBytes { get; }
            public int VertexElementSize { get; }
        }


        public ModelGL(string filePath, string contentRootDirectory, GraphicsDevice gDevice, IContentCollectorUse contentCollector)
        {
            Dictionary<int, Node> nodes = new();
            Dictionary<int, Material> materials = new();
            Dictionary<int, Mesh> meshes = new();
            Dictionary<int, IndexBuffer> indexBuffers = new();
            Dictionary<string, VertexBuffer> vertexBuffers = new();
            Dictionary<int, TextureGL> textures = new();
            Dictionary<int, SamplerState> samplers = new();

            string json = File.ReadAllText(filePath);
            string gltfDir = Path.GetDirectoryName(filePath)!;
            string gltfDirRelativeToContent = gltfDir.Substring(contentRootDirectory.Length);
            gltfDirRelativeToContent = gltfDirRelativeToContent.Replace('\\', '/');
            if (gltfDirRelativeToContent.StartsWith('/'))
                gltfDirRelativeToContent = gltfDirRelativeToContent.Substring(1);

#nullable disable
            var gltf = JsonNode.Parse(json);
            int sceneId = gltf["scene"].GetValue<int>();
            var scene = gltf["scenes"][sceneId];
            var sceneNodesArr = scene["nodes"].AsArray();
            var nodesArr = gltf["nodes"].AsArray();
            var bufferViewsArr = gltf["bufferViews"].AsArray();
            var buffersArr = gltf["buffers"].AsArray();
            var accessorsArr = gltf["accessors"].AsArray();
            var materialsArr = gltf["materials"]?.AsArray();
            var texturesArr = gltf["textures"]?.AsArray();
            var samplersArr = gltf["samplers"]?.AsArray();
            var imagesArr = gltf["images"]?.AsArray();
            foreach (var n in sceneNodesArr)
            {
                int nodeId = n.GetValue<int>();

                this.nodes.Add(GetNode(nodeId));

                Node GetNode(int id)
                {
                    Node _node;
                    if (nodes.TryGetValue(id, out _node))
                        return _node;

                    var node = nodesArr[id];
                    Matrix transform = GetTransform(node);
                    string name = node["name"].GetValue<string>();

                    _node = new Node(transform);
                    _node.Name = name;

                    var mesh = node["mesh"];
                    if (mesh != null)
                    {
                        int meshId = node["mesh"].GetValue<int>();
                        _node.Mesh = GetMesh(meshId);
                    }

                    var children = node["children"];
                    if (children != null)
                    {
                        var childrenArr = children.AsArray();
                        for (int i = 0; i < childrenArr.Count; i++)
                        {
                            int childId = childrenArr[i].GetValue<int>();
                            _node.Children.Add(GetNode(childId));
                        }
                    }

                    nodes.Add(id, _node);
                    return _node;
                }

                Mesh GetMesh(int id)
                {
                    Mesh mesh;
                    if (meshes.TryGetValue(id, out mesh))
                        return mesh;


                    var meshNode = gltf["meshes"][id];

                    mesh = new Mesh();
                    mesh.Name = meshNode["name"].GetValue<string>();
                    var primitivesArr = meshNode["primitives"].AsArray();
                    for (int i = 0; i < primitivesArr.Count; i++)
                    {
                        mesh.Primitives.Add(GetPrimitive(primitivesArr[i]));
                    }

                    meshes.Add(id, mesh);
                    return mesh;
                }

                Primitive GetPrimitive(JsonNode primitiveNode)
                {
                    var attributesObj = primitiveNode["attributes"].AsObject();

                    string key = string.Concat(attributesObj.Select(f => f.Key + f.Value));

                    VertexBuffer vertexBuffer = GetVertexBuffer(key, attributesObj);

                    int indicesAccessorIndex = primitiveNode["indices"].GetValue<int>();
                    IndexBuffer indexBuffer = GetIndexBuffer(indicesAccessorIndex);

                    Material material = null;
                    var mat = primitiveNode["material"];
                    if (mat != null)
                    {
                        int materialId = mat.GetValue<int>();
                        material = GetMaterial(materialId);
                    }
                    return new Primitive(vertexBuffer, indexBuffer, material);
                }

                Material GetMaterial(int id)
                {
                    Material material;
                    if (materials.TryGetValue(id, out material))
                        return material;


                    material = new Material();

                    // get name
                    var materialNode = materialsArr[id];
                    material.Name = materialNode["name"].GetValue<string>();

                    var pbrMetallicRoughness = materialNode["pbrMetallicRoughness"];
                    if (pbrMetallicRoughness != null)
                    {
                        // get texture
                        var baseColorTexture = pbrMetallicRoughness["baseColorTexture"];
                        if (baseColorTexture != null)
                        {
                            int texIndex = baseColorTexture["index"].GetValue<int>();
                            TextureGL textureSampler = GetTexture(texIndex);

                            material.PbrMetallicRoughness ??= new PbrMetallicRoughness();
                            material.PbrMetallicRoughness.BaseColorTexSampler = textureSampler;
                        }

                        // get base color
                        var baseColorFactor = pbrMetallicRoughness["baseColorFactor"];
                        if (baseColorFactor != null)
                        {
                            float[] c = baseColorFactor.AsArray().Select(f => f.GetValue<float>()).ToArray();
                            material.PbrMetallicRoughness ??= new PbrMetallicRoughness();
                            material.PbrMetallicRoughness.BaseColor = new Vector4(c[0], c[1], c[2], c[3]);
                        }
                    }

                    var extras = materialNode["extras"];
                    if (extras != null)
                    {
                        // get extra base color
                        var baseColorFactor = extras["baseColorFactor"];
                        if (baseColorFactor != null)
                        {
                            float[] c = baseColorFactor.AsArray().Select(f => f.GetValue<float>()).ToArray();
                            material.PbrMetallicRoughness ??= new PbrMetallicRoughness();
                            material.PbrMetallicRoughness.BaseColor = new Vector4(c[0], c[1], c[2], c.Length > 3 ? c[3] : 1f);
                        }

                        var culling = extras["culling"];
                        if (culling != null)
                        {
                            int cull = culling.GetValue<int>();
                            if (cull > 0)
                                material.RasterizerState = RasterizerState.CullClockwise;
                            else if (cull < 0)
                                material.RasterizerState = RasterizerState.CullCounterClockwise;
                            else
                                material.RasterizerState = RasterizerState.CullNone;
                        }
                    }

                    materials.Add(id, material);
                    return material;
                }

                IndexBuffer GetIndexBuffer(int id)
                {
                    IndexBuffer indexBuffer;
                    if (indexBuffers.TryGetValue(id, out indexBuffer))
                        return indexBuffer;

                    // set index buffer
                    var indicesAccessor = accessorsArr[id];
                    byte[] indicesData = GetBytesFromBuffer(bufferViewsArr, buffersArr, indicesAccessor, contentCollector, gltfDirRelativeToContent);
                    int indicesCount = indicesAccessor["count"].GetValue<int>();
                    IndexElementSize indexElementSize = indicesData.Length / indicesCount != 2 ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits;

                    indexBuffer = disposables.Use(new IndexBuffer(gDevice, indexElementSize, indicesCount, BufferUsage.WriteOnly));
                    indexBuffer.SetData(indicesData);

                    indexBuffers.Add(id, indexBuffer);
                    return indexBuffer;
                }

                VertexBuffer GetVertexBuffer(string key, JsonObject attributesObj)
                {
                    VertexBuffer vertexBuffer;
                    if (vertexBuffers.TryGetValue(key, out vertexBuffer))
                        return vertexBuffer;



                    List<VertexPart> vertexParts = new();
                    int[] vertexElementUsages = new int[Enum.GetNames(typeof(VertexElementUsage)).Length];
                    int offset = 0;
                    foreach (var attribute in attributesObj)
                    {
                        // setup vertex declaration
                        int accessorIndex = attribute.Value.GetValue<int>();
                        var accessor = accessorsArr[accessorIndex];
                        string type = accessor["type"].GetValue<string>();
                        var usage = GetVertexElementUsageFromAttributeName(attribute.Key);
                        int componentTypeInt = accessor["componentType"].GetValue<int>();
                        Type componentType = GetComponentType(componentTypeInt);

                        VertexElement v = new(offset,
                            GetVertexElementFormatFromAccessorType(type, componentType),
                            usage,
                            vertexElementUsages[(int)usage]);

                        vertexElementUsages[(int)usage]++;

                        int componentSize = GetComponentSizeInBytes(componentTypeInt);
                        int componentCount = GetNumberOfComponents(type);

                        byte[] bufferBytes = GetBytesFromBuffer(bufferViewsArr, buffersArr, accessor, contentCollector, gltfDirRelativeToContent);

                        // when using color, the component size should be 1. 1 byte for each color channel.
                        // if it is 2, convert it!
                        if (usage == VertexElementUsage.Color
                            && componentSize == 2)
                        {
                            componentSize = Convert8BitColorTo4Bit(ref bufferBytes);
                        }

                        int elementSize = componentSize * componentCount;
                        offset += elementSize;

                        vertexParts.Add(new VertexPart(v, bufferBytes, elementSize));
                    }

                    int vertexStride = offset;
                    var vertexDeclaration = new VertexDeclaration(vertexStride, vertexParts.Select(f => f.VertexElement).ToArray());

                    // merge vertex data
                    byte[] vertexData = new byte[vertexParts.Sum(f => f.BufferBytes.Length)];
                    int vertexCount = vertexParts[0].BufferBytes.Length / vertexParts[0].VertexElementSize;
                    foreach (var vertexPart in vertexParts)
                    {
                        unsafe
                        {
                            int vertexElementSize = vertexPart.VertexElementSize;
                            fixed (byte* srcFixed = vertexPart.BufferBytes)
                            fixed (byte* dstFixed = vertexData)
                            {
                                byte* src = srcFixed;
                                byte* dst = dstFixed + vertexPart.VertexElement.Offset;

                                for (int vertex = 0; vertex < vertexCount; vertex++)
                                {
                                    System.Buffer.MemoryCopy(src, dst, vertexElementSize, vertexElementSize);
                                    src += vertexElementSize;
                                    dst += vertexStride;
                                }
                            }
                        }
                    }

                    vertexBuffer = disposables.Use(new VertexBuffer(gDevice, vertexDeclaration, vertexCount, BufferUsage.WriteOnly));
                    vertexBuffer.SetData(vertexData);

                    vertexBuffers.Add(key, vertexBuffer);
                    return vertexBuffer;
                }

                TextureGL GetTexture(int key)
                {
                    TextureGL _texture;
                    if (textures.TryGetValue(key, out _texture))
                        return _texture;

                    var texture = texturesArr[key];
                    int samplerId = texture["sampler"].GetValue<int>();
                    var sampler = GetSampler(samplerId);
                    var image = imagesArr[texture["source"].GetValue<int>()];
                    var imageUri = image["uri"].GetValue<string>();

                    Ref<Texture2D> tex = disposables.Use(contentCollector.Use<Texture2D>(UriToContentFile(imageUri, gltfDirRelativeToContent)));

                    _texture = new TextureGL(tex, sampler);
                    textures.Add(key, _texture);
                    return _texture;
                }

                SamplerState GetSampler(int key)
                {
                    SamplerState _sampler;
                    if (samplers.TryGetValue(key, out _sampler))
                        return _sampler;

                    var sampler = samplersArr[key];
                    int magFilter = sampler["magFilter"].GetValue<int>();
                    int minFilter = sampler["minFilter"].GetValue<int>();
                    TextureFilter textureFilter = GetTextureFilter(magFilter, minFilter);

                    _sampler = new SamplerState()
                    {
                        Filter = textureFilter,
                          
                    };

                    var wrap = sampler["wrapS"];
                    if (wrap != null)
                    {
                        int wrapInt = wrap.GetValue<int>();
                        _sampler.AddressU = GetAddressMode(wrapInt);
                    }
                    wrap = sampler["wrapT"];
                    if (wrap != null)
                    {
                        int wrapInt = wrap.GetValue<int>();
                        _sampler.AddressV = GetAddressMode(wrapInt);
                    }


                    samplers.Add(key, _sampler);
                    return _sampler;

                }
                static int Convert8BitColorTo4Bit(ref byte[] bufferBytes)
                {
                    int componentSize;
                    byte[] newBufferBytes = new byte[bufferBytes.Length / 2];
                    componentSize = 1;

                    for (int i = 0; i < newBufferBytes.Length; i++)
                    {
                        newBufferBytes[i] = (byte)(BitConverter.ToUInt16(bufferBytes, i * 2) / (byte.MaxValue + 1));
                    }

                    bufferBytes = newBufferBytes;
                    return componentSize;
                }

                static Matrix GetTransform(JsonNode node)
                {
                    Matrix transform = Matrix.Identity;
                    // get transform
                    var scale = node["scale"];
                    if (scale != null)
                    {
                        Vector3 s = new Vector3(
                            scale[0].GetValue<float>(),
                            scale[1].GetValue<float>(),
                            scale[2].GetValue<float>());
                        transform = Matrix.CreateScale(s);
                    }
                    var rotation = node["rotation"];
                    if (rotation != null)
                    {
                        Quaternion q = new Quaternion(
                            rotation[0].GetValue<float>(),
                            rotation[1].GetValue<float>(),
                            rotation[2].GetValue<float>(),
                            rotation[3].GetValue<float>());
                        transform *= Matrix.CreateFromQuaternion(q);
                    }
                    var translation = node["translation"];
                    if (translation != null)
                    {
                        Vector3 v = new Vector3(
                            translation[0].GetValue<float>(),
                            translation[1].GetValue<float>(),
                            translation[2].GetValue<float>());
                        transform *= Matrix.CreateTranslation(v);
                    }

                    return transform;
                }
                static TextureFilter GetTextureFilter(int magFilter, int minFilter)
                {
                    TextureFilter textureFilter;
                    switch (magFilter)
                    {
                        case 9728: // NEAREST
                            switch (minFilter)
                            {
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
#nullable restore
        }

        private string UriToContentFile(string uri, string gltfDirRelativeToContent)
        {
            string ext = Path.GetExtension(uri);
            return UriToContentFileWithExtension(uri.Remove(uri.Length - ext.Length), gltfDirRelativeToContent);
        }
        private string UriToContentFileWithExtension(string uri, string gltfDirRelativeToContent)
        {
            string fullPath = Path.GetFullPath(Path.Combine(gltfDirRelativeToContent, uri));
            fullPath = fullPath.Substring(Environment.CurrentDirectory.Length + 1);
            return fullPath.Replace('\\', '/');
        }

        private byte[] GetBytesFromBuffer(JsonArray bufferViewsArr, JsonArray buffersArr, JsonNode? accessor, IContentCollectorUse contentCollector, string gltfDirRelativeToContent)
        {
#nullable disable
            var bufferView = bufferViewsArr[accessor["bufferView"].GetValue<int>()];
            int bufferByteLength = bufferView["byteLength"].GetValue<int>();
            int bufferByteOffset = bufferView["byteOffset"].GetValue<int>();
            var buffer = buffersArr[bufferView["buffer"].GetValue<int>()];
            var bufferUri = buffer["uri"].GetValue<string>();
#nullable restore

            Ref<byte[]> wholeBuffer = disposables.Use(contentCollector.Use<byte[]>(UriToContentFileWithExtension(bufferUri, gltfDirRelativeToContent)));
            byte[] bufferBytes = new byte[bufferByteLength];
            Buffer.BlockCopy(wholeBuffer.Value, bufferByteOffset, bufferBytes, 0, bufferByteLength);
            //using (Stream stream = File.OpenRead(bufferUri)) // TODO: only read each file once
            //{
            //    stream.Position = bufferByteOffset;
            //    stream.Read(bufferBytes, 0, bufferByteLength);
            //}

            return bufferBytes;
        }

        public void Draw(IShaderGL shader)
        {
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].Draw(shader);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
