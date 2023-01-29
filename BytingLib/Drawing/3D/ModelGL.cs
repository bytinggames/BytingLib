using System.Runtime.InteropServices;
using System.Text.Json.Nodes;

namespace BytingLib
{
    public interface IShaderGL : IShaderTexWorld
    {
        EffectParameterStack<Vector4> Color { get; }
    }
    public interface IShaderGLSkinned : IShaderGL
    {
        EffectParameterStack<Matrix[]> JointMatrices { get; }
    }

    public class DictCache<TValue>
    {
        private Dictionary<int, TValue> dict = new();
        private readonly JsonArray container;
        private Func<JsonNode, TValue> loadFromContainer;

        public DictCache(JsonArray container, Func<JsonNode, TValue> loadFromContainer)
        {
            this.container = container;
            this.loadFromContainer = loadFromContainer;
        }

        public TValue Get(int index)
        {
            if (dict.TryGetValue(index, out TValue? value))
                return value;

            var val = loadFromContainer(container[index]!);
            dict.Add(index, val);
            return val;
        }

        public void ForEach(Action<TValue> action)
        {
            foreach (var val in dict.Values)
            {
                action(val);
            }
        }

        public void Clear()
        {
            dict.Clear();
        }
    }

    public class DictCacheNode
    {
        private Dictionary<int, NodeGL> dict = new();
        private readonly JsonArray container;
        private Func<JsonNode, NodeGL?, NodeGL> loadFromContainer;

        public DictCacheNode(JsonArray container, Func<JsonNode, NodeGL?, NodeGL> loadFromContainer)
        {
            this.container = container;
            this.loadFromContainer = loadFromContainer;
        }

        public NodeGL Get(int index) => Get(index, null);

        public NodeGL Get(int index, NodeGL? parent)
        {
            if (dict.TryGetValue(index, out NodeGL? value))
            {
                value.SetParentIfNotHavingOne(parent);
                return value;
            }

            var val = loadFromContainer(container[index]!, parent);
            dict.Add(index, val);
            return val;
        }
    }

    public class ModelGL : IDisposable
    {
        public readonly int SceneIndex;

        public readonly DictCache<SceneGL>? Scenes;
        public readonly DictCacheNode? Nodes;
        public readonly DictCache<MeshGL>? Meshes;
        public readonly DictCache<AccessorGL>? Accessors;
        public readonly DictCache<BufferViewGL>? BufferViews;
        public readonly DictCache<BufferGL>? Buffers;
        public readonly DictCache<MaterialGL>? Materials;
        public readonly DictCache<TextureGL>? Textures;
        public readonly DictCache<SamplerGL>? Samplers;
        public readonly DictCache<ImageGL>? Images;
        public readonly DictCache<SkinGL>? Skins;
        public readonly DictCache<AnimationGL>? Animations;

        private readonly Dictionary<string, VertexBuffer> vertexBuffers = new();
        private readonly Dictionary<int, IndexBuffer> indexBuffers = new();

        private readonly JsonArray? accessorsArr, bufferViewsArr, buffersArr;

        private readonly DisposableContainer disposables = new();

        private readonly IContentCollectorUse contentCollector;
        private readonly string gltfDirRelativeToContent;
        private readonly GraphicsDevice gDevice;

        public ModelGL(string filePath, string contentRootDirectory, GraphicsDevice gDevice, IContentCollectorUse contentCollector)
        {
            this.gDevice = gDevice;
            this.contentCollector = contentCollector;

            string json = File.ReadAllText(filePath);
            string gltfDir = Path.GetDirectoryName(filePath)!;
            gltfDirRelativeToContent = gltfDir.Substring(contentRootDirectory.Length);
            gltfDirRelativeToContent = gltfDirRelativeToContent.Replace('\\', '/');
            if (gltfDirRelativeToContent.StartsWith('/'))
                gltfDirRelativeToContent = gltfDirRelativeToContent.Substring(1);

            var root = JsonNode.Parse(json)!;

            JsonNode? n;
            if ((n = root["scenes"]) != null)
                Scenes = new(n.AsArray(), n => new(this, n));
            if ((n = root["nodes"]) != null)
                Nodes = new(n.AsArray(), (n, parent) => new(this, n, parent));
            if ((n = root["meshes"]) != null)
                Meshes = new(n.AsArray(), n => new(this, n));
            //if ((n = root["accessors"]) != null)
            //     Accessors = new(n.AsArray(), n => new(this, n));
            //if ((n = root["bufferViews"]) != null)
            //    BufferViews = new(n.AsArray(), n => new(this, n));
            //if ((n = root["buffers"]) != null)
            //    Buffers = new(n.AsArray(), n => new(this, n));
            if ((n = root["materials"]) != null)
                Materials = new(n.AsArray(), n => new(this, n));
            if ((n = root["textures"]) != null)
                Textures = new(n.AsArray(), n => new(this, n));
            if ((n = root["samplers"]) != null)
                Samplers = new(n.AsArray(), n => new(n));
            if ((n = root["images"]) != null)
                Images = new(n.AsArray(), n => new(this, n));
            if ((n = root["skins"]) != null)
                Skins = new(n.AsArray(), n => new(this, n));
            if ((n = root["animations"]) != null)
                Animations = new(n.AsArray(), n => new(this, n));


            accessorsArr = root["accessors"]?.AsArray();
            bufferViewsArr = root["bufferViews"]?.AsArray();
            buffersArr = root["buffers"]?.AsArray();

            SceneIndex = root["scene"]?.GetValue<int>() ?? 0;
        }

        internal VertexBuffer GetVertexBuffer(string key, JsonObject attributesObj)
        {
            VertexBuffer? vertexBuffer;
            if (vertexBuffers.TryGetValue(key, out vertexBuffer))
                return vertexBuffer;


            List<VertexPart> vertexParts = new();
            int[] vertexElementUsages = new int[Enum.GetNames(typeof(VertexElementUsage)).Length];
            int offset = 0;
            foreach (var attribute in attributesObj)
            {
                // setup vertex declaration
                int accessorIndex = attribute.Value!.GetValue<int>();
                var accessor = accessorsArr![accessorIndex]!;
                string type = accessor["type"]!.GetValue<string>();
                var usage = BufferHelper.GetVertexElementUsageFromAttributeName(attribute.Key);
                int componentTypeInt = accessor["componentType"]!.GetValue<int>();
                Type componentType = BufferHelper.GetComponentType(componentTypeInt);

                VertexElement v = new(offset,
                    BufferHelper.GetVertexElementFormatFromAccessorType(type, componentType),
                    usage,
                    vertexElementUsages[(int)usage]);

                vertexElementUsages[(int)usage]++;

                int componentSize = BufferHelper.GetComponentSizeInBytes(componentTypeInt);
                int componentCount = BufferHelper.GetNumberOfComponents(type);

                byte[] bufferBytes = GetBytesFromBuffer(accessorIndex);

                // when using color, the component size should be 1. 1 byte for each color channel.
                // if it is 2, convert it!
                if (usage == VertexElementUsage.Color
                    && componentSize == 2)
                {
                    componentSize = BufferHelper.Convert8BitColorTo4Bit(ref bufferBytes);
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

        public IndexBuffer GetIndexBuffer(int id)
        {
            IndexBuffer? indexBuffer;
            if (indexBuffers.TryGetValue(id, out indexBuffer))
                return indexBuffer;

            // set index buffer
            var indicesAccessor = accessorsArr![id]!;
            byte[] indicesData = GetBytesFromBuffer(id);
            int indicesCount = indicesAccessor["count"]!.GetValue<int>();
            IndexElementSize indexElementSize = indicesData.Length / indicesCount != 2 ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits;

            indexBuffer = disposables.Use(new IndexBuffer(gDevice, indexElementSize, indicesCount, BufferUsage.WriteOnly));
            indexBuffer.SetData(indicesData);

            indexBuffers.Add(id, indexBuffer);
            return indexBuffer;
        }


        public byte[] GetBytesFromBuffer(int accessorIndex)
        {
#nullable disable
            var bufferView = bufferViewsArr[accessorsArr[accessorIndex]["bufferView"].GetValue<int>()];
            int bufferByteLength = bufferView["byteLength"].GetValue<int>();
            int bufferByteOffset = bufferView["byteOffset"].GetValue<int>();
            var buffer = buffersArr[bufferView["buffer"].GetValue<int>()];
            var bufferUri = buffer["uri"].GetValue<string>();
#nullable restore

            Ref<byte[]> wholeBuffer = disposables.Use(contentCollector.Use<byte[]>(ContentHelper.UriToContentFileWithExtension(bufferUri, gltfDirRelativeToContent)));
            byte[] bufferBytes = new byte[bufferByteLength];
            System.Buffer.BlockCopy(wholeBuffer.Value, bufferByteOffset, bufferBytes, 0, bufferByteLength);
            //using (Stream stream = File.OpenRead(bufferUri)) // TODO: only read each file once
            //{
            //    stream.Position = bufferByteOffset;
            //    stream.Read(bufferBytes, 0, bufferByteLength);
            //}

            return bufferBytes;
        }

        public void Dispose()
        {
            disposables.Dispose();

            if (Images != null)
            {
                Images.ForEach(f => f.Dispose());
                Images.Clear();
            }
        }

        internal Ref<Texture2D> GetTexture(string imageUri)
        {
            return contentCollector.Use<Texture2D>(ContentHelper.UriToContentFile(imageUri, gltfDirRelativeToContent));
        }

        public void Draw(IShaderGLSkinned shader)
        {
            Scenes!.Get(SceneIndex).Draw(shader);
        }

        public NodeGL? FindNode(string name)
        {
            return Scenes?.Get(SceneIndex).FindNode(name);
        }
    }

    public class SceneGL
    {
        List<NodeGL> nodes = new();

        public SceneGL(ModelGL model, JsonNode n)
        {
            var nodesArr = n["nodes"]!.AsArray();
            for (int i = 0; i < nodesArr.Count; i++)
            {
                int nodeId = nodesArr[i]!.GetValue<int>();
                nodes.Add(model.Nodes!.Get(nodeId));
            }
        }

        internal void Draw(IShaderGLSkinned shader)
        {
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].Draw(shader);
        }

        internal NodeGL? FindNode(string name)
        {
            NodeGL? node;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Name == name)
                    return nodes[i];
                node = nodes[i].FindNode(name);
                if (node != null)
                    return node;
            }
            return null;
        }
    }

    public class NodeGL
    {
        MeshGL? mesh;
        SkinGL? skin;
        Matrix LocalTransform;
        Matrix JointTransform = Matrix.Identity;
        public Matrix GlobalJointTransform; // needs to be updated according to LocalTransform and all local transforms of the parents
        int GlobalTransformCalculationId;
        public readonly string? Name;
        public override string ToString() => "Node: " + Name;
        public readonly List<NodeGL> Children = new();
        public NodeGL? Parent { get; private set; }

        public NodeGL(ModelGL model, JsonNode n, NodeGL? parent)
        {
            if (Parent == null)
                Parent = parent;

            LocalTransform = GetTransform(n);

            Name = n["name"]?.GetValue<string>();

            JsonNode? t;
            if ((t = n["mesh"]) != null)
            {
                mesh = model.Meshes?.Get(t.GetValue<int>());
                skin = (t = n["skin"]) == null ? null : model.Skins?.Get(t.GetValue<int>());
            }
            if ((t = n["children"]) != null)
            {
                var childrenArr = t.AsArray();
                for (int i = 0; i < childrenArr.Count; i++)
                {
                    int childId = childrenArr[i]!.GetValue<int>();
                    NodeGL child = model.Nodes!.Get(childId, this);
                    Children.Add(child);
                }
            }
        }

        static Matrix GetTransform(JsonNode n)
        {
            Matrix transform = Matrix.Identity;
            // get transform
            var scale = n["scale"];
            if (scale != null)
            {
                Vector3 s = new Vector3(
                    scale[0]!.GetValue<float>(),
                    scale[1]!.GetValue<float>(),
                    scale[2]!.GetValue<float>());
                transform = Matrix.CreateScale(s);
            }
            var rotation = n["rotation"];
            if (rotation != null)
            {
                Quaternion q = new Quaternion(
                    rotation[0]!.GetValue<float>(),
                    rotation[1]!.GetValue<float>(),
                    rotation[2]!.GetValue<float>(),
                    rotation[3]!.GetValue<float>());
                transform *= Matrix.CreateFromQuaternion(q);
            }
            var translation = n["translation"];
            if (translation != null)
            {
                Vector3 v = new Vector3(
                    translation[0]!.GetValue<float>(),
                    translation[1]!.GetValue<float>(),
                    translation[2]!.GetValue<float>());
                transform *= Matrix.CreateTranslation(v);
            }

            return transform;
        }

        internal void Draw(IShaderGLSkinned shader)
        {
            using (shader.World.Use(f => LocalTransform * f))
            using (skin?.Use(shader, shader.World.GetValue()))
            {
                mesh?.Draw(shader);
                for (int i = 0; i < Children.Count; i++)
                    Children[i].Draw(shader);
            }
        }


        internal void CalculateGlobalTransform(int globalTransformCalculationId)
        {
            if (GlobalTransformCalculationId == globalTransformCalculationId)
                return;

            if (Parent == null)
            {
                GlobalJointTransform = JointTransform * LocalTransform;
            }
            else
            {
                Parent.CalculateGlobalTransform(globalTransformCalculationId);
                GlobalJointTransform = JointTransform * LocalTransform * Parent.GlobalJointTransform;
            }
            GlobalTransformCalculationId = globalTransformCalculationId;
        }

        public NodeGL? FindNode(string name)
        {
            NodeGL? node;
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i].Name == name)
                    return Children[i];
                node = Children[i].FindNode(name);
                if (node != null)
                    return node;
            }
            return null;
        }

        public void SetRotation(Quaternion rotation)
        {
            // TODO: only set rotation
            JointTransform = Matrix.CreateFromQuaternion(rotation);
        }

        internal void SetParentIfNotHavingOne(NodeGL? parent)
        {
            Parent ??= parent;
        }

        // FOR LATER MAYBE:
        //Lazy<Mesh?> mesh;
        //Lazy<Skin?> skin;

        //public Node(model model, JsonNode n)
        //{
        //    skin = new(() =>
        //    {
        //        JsonNode? t;
        //        return (t = n["skin"]) == null ? null : model.Skins?.Get(t.GetValue<int>());
        //    });
        //    mesh = new(() =>
        //    {
        //        JsonNode? t;
        //        return (t = n["mesh"]) == null ? null : model.Meshes?.Get(t.GetValue<int>());
        //    });
        //}
    }

    public class MeshGL
    {
        public readonly string? Name;
        public override string ToString() => "Mesh: " + Name;
        List<Primitive> Primitives = new();

        public MeshGL(ModelGL model, JsonNode n)
        {
            Name = n["name"]?.GetValue<string>();

            JsonNode? t;
            if ((t = n["primitives"]) != null)
            {
                JsonArray primitivesArr = t.AsArray();
                for (int i = 0; i < primitivesArr.Count; i++)
                {
                    Primitives.Add(new Primitive(model, primitivesArr[i]!));
                }
            }
        }

        public void Draw(IShaderGLSkinned shader)
        {
            for (int i = 0; i < Primitives.Count; i++)
            {
                Primitives[i].Draw(shader);
            }
        }
    }

    public class Primitive
    {
        public VertexBuffer VertexBuffer;
        public IndexBuffer? IndexBuffer;
        public MaterialGL? Material;

        public Primitive(ModelGL model, JsonNode n)
        {
            var attributesObj = n["attributes"]!.AsObject();

            string key = string.Concat(attributesObj.Select(f => f.Key + f.Value));

            VertexBuffer = model.GetVertexBuffer(key, attributesObj);

            JsonNode? t;
            if ((t = n["indices"]) != null)
            {
                int indicesAccessorIndex = t.GetValue<int>();
                IndexBuffer = model.GetIndexBuffer(indicesAccessorIndex);
            }

            if ((t = n["material"]) != null)
            {
                int materialId = t.GetValue<int>();
                Material = model.Materials!.Get(materialId);
            }
        }

        public void Draw(IShaderGLSkinned shader)
        {
            string techniqueName = Shader.GetTechniqueName(VertexBuffer.VertexDeclaration);

            using (shader.UseTechnique(techniqueName))
            {
                using (Material?.Use(shader))
                {
                    if (IndexBuffer == null)
                        shader.Draw(VertexBuffer);
                    else
                        shader.Draw(VertexBuffer, IndexBuffer);
                }
            }
        }
    }

    public class AccessorGL
    {

    }

    public class BufferViewGL
    {

    }

    public class BufferGL
    {

    }

    public class MaterialGL
    {
        public readonly string? Name;
        public override string ToString() => "Material: " + Name;

        public PbrMetallicRoughness? PbrMetallicRoughness;
        public RasterizerState? RasterizerState;

        public MaterialGL(ModelGL model, JsonNode n)
        {
            Name = n["name"]?.GetValue<string>();

            JsonNode? t;
            if ((t = n["pbrMetallicRoughness"]) != null)
            {
                // get texture
                var baseColorTexture = t["baseColorTexture"];
                if (baseColorTexture != null)
                {
                    int texIndex = baseColorTexture["index"]!.GetValue<int>();
                    TextureGL textureSampler = model.Textures!.Get(texIndex);

                    PbrMetallicRoughness ??= new PbrMetallicRoughness();
                    PbrMetallicRoughness.BaseColorTexture = textureSampler;
                }

                // get base color
                var baseColorFactor = t["baseColorFactor"];
                if (baseColorFactor != null)
                {
                    float[] c = baseColorFactor.AsArray().Select(f => f!.GetValue<float>()).ToArray();
                    PbrMetallicRoughness ??= new PbrMetallicRoughness();
                    PbrMetallicRoughness.BaseColor = new Vector4(c[0], c[1], c[2], c[3]);
                }
            }

            var extras = n["extras"];
            if (extras != null)
            {
                // get extra base color
                var baseColorFactor = extras["baseColorFactor"];
                if (baseColorFactor != null)
                {
                    float[] c = baseColorFactor.AsArray().Select(f => f!.GetValue<float>()).ToArray();
                    PbrMetallicRoughness ??= new PbrMetallicRoughness();
                    PbrMetallicRoughness.BaseColor = new Vector4(c[0], c[1], c[2], c.Length > 3 ? c[3] : 1f);
                }

                var culling = extras["culling"];
                if (culling != null)
                {
                    int cull = culling.GetValue<int>();
                    if (cull > 0)
                        RasterizerState = RasterizerState.CullClockwise;
                    else if (cull < 0)
                        RasterizerState = RasterizerState.CullCounterClockwise;
                    else
                        RasterizerState = RasterizerState.CullNone;
                }
            }
        }

        internal IDisposable Use(IShaderGLSkinned shader)
        {
            DisposableContainer disposables = new();
            if (PbrMetallicRoughness != null)
                disposables.Use(PbrMetallicRoughness.Use(shader));
            if (RasterizerState != null)
                disposables.Use(shader.UseRasterizer(RasterizerState));
            return disposables;
        }
    }

    public class PbrMetallicRoughness
    {
        public Vector4? BaseColor;
        public TextureGL? BaseColorTexture;
        public float MetallicFactor;
        public float RoughnessFactor;

        public IDisposable Use(IShaderGL shader)
        {
            DisposableContainer toDispose = new();
            if (BaseColor != null)
                toDispose.UseCheckNull(shader.Color.Use(BaseColor.Value));
            if (BaseColorTexture != null)
            {
                toDispose.Use(shader.UseSampler(BaseColorTexture.Sampler.SamplerState));
                toDispose.UseCheckNull(shader.ColorTex.Use(BaseColorTexture.Image.Tex2D.Value));
            }
            return toDispose;
        }
    }

    public class TextureGL
    {
        public ImageGL Image;
        public SamplerGL Sampler;

        public TextureGL(ModelGL model, JsonNode n)
        {
            int samplerId = n["sampler"]!.GetValue<int>();
            Sampler = model.Samplers!.Get(samplerId);
            int sourceId = n["source"]!.GetValue<int>();
            Image = model.Images!.Get(sourceId);
        }
    }

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

    public class ImageGL : IDisposable
    {
        public Ref<Texture2D> Tex2D;

        public ImageGL(ModelGL model, JsonNode n)
        {
            var imageUri = n["uri"]!.GetValue<string>();

            Tex2D = model.GetTexture(imageUri)!;
        }

        public void Dispose()
        {
            Tex2D.Dispose();
        }
    }

    public class SkinGL
    {
        static int globalTransformCalculationId;

        public readonly Matrix[] InverseBindMatrices;
        public readonly NodeGL[] Joints;

        private Matrix[] jointMatrices;

        public readonly string? Name;
        public override string ToString() => "Skin: " + Name;

        public SkinGL(ModelGL model, JsonNode n)
        {
            Name = n["name"]?.GetValue<string>();


            int inverseBindMatricesId = n["inverseBindMatrices"]!.GetValue<int>();
            int[] joints = n["joints"]!.AsArray().Select(f => f!.GetValue<int>()).ToArray();


            Joints = new NodeGL[joints.Length];
            for (int i = 0; i < joints.Length; i++)
            {
                Joints[i] = model.Nodes!.Get(joints[i], null);
            }

            byte[] inverseBindAccessorData = model.GetBytesFromBuffer(inverseBindMatricesId);
            InverseBindMatrices = BufferHelper.ByteArrayToMatrixArray(inverseBindAccessorData);


            jointMatrices = new Matrix[Joints.Length * 2];
        }

        public void ComputeJointMatrices(Matrix globalTransform)
        {
            globalTransformCalculationId++;
            for (int i = 0; i < Joints.Length; i++)
            {
                Joints[i].CalculateGlobalTransform(globalTransformCalculationId);
            }

            Matrix globalTransformInverse = Matrix.Invert(globalTransform);
            for (int i = 0; i < Joints.Length; i++)
            {
                jointMatrices[i] = InverseBindMatrices[i]
                    * Joints[i].GlobalJointTransform
                    * globalTransformInverse;

                // inverse transpose matrices
                jointMatrices[i + Joints.Length] = jointMatrices[i].GetInverseTranspose3x3();
            }
        }

        internal IDisposable? Use(IShaderGLSkinned shader, Matrix globalTransform)
        {
            ComputeJointMatrices(globalTransform);
            return shader.JointMatrices.Use(jointMatrices);
        }
    }

    public class AnimationGL
    {
        public readonly string? Name;
        public override string ToString() => "Animation: " + Name;

        public Channel[] channels;
        public Sampler[] samplers;

        public AnimationGL(ModelGL model, JsonNode n)
        {
            Name = n["name"]?.GetValue<string>();
            //samplers = n["samplers"]!.AsArray().Select(f => new Sampler(f!)).ToArray();
            //channels = n["channels"]!.AsArray().Select(f => new Channel(f!)).ToArray();
        }

        public class Channel
        {
            public enum TargetPath
            {
                Translation,
                Rotation,
                Scale,
                Weights
            }

            public Sampler sampler;
            public NodeGL TargetNode;
            public TargetPath _TargetPath;
        }

        public class Sampler
        {
            public enum Interpolation
            {
                Linear,
                Step,
                CubicSpline
            }
            public KeyFrames keyFrames; // refers to an accessor with floats, which are the times of the key frames of the animation
            public SamplerOutput output; // refers to an accessor that contains the values for the animated property at the respective key frames
            public Interpolation interpolation;

            //public Sampler(JsonNode n, Dictionary<int, Accessor> accessors)
            //{
            //    int input = n["input"]!.GetValue<int>();
            //    int output = n["output"]!.GetValue<int>();

            //    var interpolationStr = n["interpolation"]?.GetValue<string>();
            //    interpolation = interpolationStr == "STEP" ? Interpolation.Step
            //        : interpolationStr == "CUBICSPLINE" ? Interpolation.CubicSpline
            //        : Interpolation.Linear;


            //}
        }
    }
    public class KeyFrames
    {
        public float[] seconds; // in seconds
    }

    public class SamplerOutput
    {
        public Quaternion[] values; // TODO: support Vector3[] too
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
                "TEXCOORD_0" => VertexElementUsage.TextureCoordinate,
                "COLOR_0" => VertexElementUsage.Color,
                "JOINTS_0" => VertexElementUsage.BlendIndices,
                "WEIGHTS_0" => VertexElementUsage.BlendWeight,
                _ => throw new NotImplementedException(),
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

        internal static int Convert8BitColorTo4Bit(ref byte[] bufferBytes)
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


        internal static Matrix[] ByteArrayToMatrixArray(byte[] inverseBindAccessorData)
        {
            Matrix[] m = new Matrix[inverseBindAccessorData.Length / 4 / 16];
            IntPtr mPtr = Marshal.UnsafeAddrOfPinnedArrayElement(m, 0);
            Marshal.Copy(inverseBindAccessorData, 0, mPtr, inverseBindAccessorData.Length);
            return m;
        }

    }

    class ContentHelper
    {
        internal static string UriToContentFile(string uri, string modelDirRelativeToContent)
        {
            string ext = Path.GetExtension(uri);
            return UriToContentFileWithExtension(uri.Remove(uri.Length - ext.Length), modelDirRelativeToContent);
        }
        internal static string UriToContentFileWithExtension(string uri, string modelDirRelativeToContent)
        {
            string fullPath = Path.GetFullPath(Path.Combine(modelDirRelativeToContent, uri));
            fullPath = fullPath.Substring(Environment.CurrentDirectory.Length + 1);
            return fullPath.Replace('\\', '/');
        }

    }
}
