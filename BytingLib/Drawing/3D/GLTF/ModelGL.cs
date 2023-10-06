using System.Text.Json.Nodes;

namespace BytingLib
{

    public class ModelGL : IDisposable
    {
        public int SceneIndex { get; }

        public JsonArrayCache<SceneGL>? Scenes { get; }
        internal ArrayCacheNode? Nodes { get; }
        public JsonArrayCache<MeshGL>? Meshes { get; }
        public JsonArrayCache<MaterialGL>? Materials { get; }
        public JsonArrayCache<TextureGL>? Textures { get; }
        public JsonArrayCache<SamplerGL>? Samplers { get; }
        public JsonArrayCache<ImageGL>? Images { get; }
        public JsonArrayCache<SkinGL>? Skins { get; }
        public JsonArrayCache<AnimationGL>? Animations { get; }
        internal DictionaryCacheChannelTargets ChannelTargets { get; }
        internal DictionaryCacheKeyFrames? KeyFrames { get; }
        public SceneGL? CurrentScene => Scenes?.Get(SceneIndex);

        internal AnimationBlend AnimationBlend { get; } = new();

        private readonly Dictionary<string, VertexBuffer> vertexBuffers = new();
        private readonly Dictionary<int, IndexBuffer> indexBuffers = new();
        private readonly JsonArray? accessorsArr, bufferViewsArr, buffersArr;
        private readonly DisposableContainer disposables = new();
        private readonly IContentCollectorUse? contentCollector;
        private readonly string gltfDirRelativeToContent;
        private readonly string contentRootDirectory;
        private readonly GraphicsDevice? gDevice;


        private Dictionary<string, int>? AnimationNameToIndex;
        private readonly JsonArray? animationsJsonArray;

        public SamplerGL? DefaultSampler { get; internal set; }

        public ModelGL(string filePath, string contentRootDirectory, GraphicsDevice? gDevice, IContentCollectorUse? contentCollector)
            :this(File.ReadAllText(filePath), Path.GetDirectoryName(filePath) ?? "", contentRootDirectory, gDevice, contentCollector)
        {
        }

        public ModelGL(string json, string gltfDirectory, string contentRootDirectory, GraphicsDevice? gDevice, IContentCollectorUse? contentCollector)
        {
            this.contentRootDirectory = contentRootDirectory;
            this.gDevice = gDevice;
            this.contentCollector = contentCollector;
            ChannelTargets = new DictionaryCacheChannelTargets(this);

            gltfDirRelativeToContent = gltfDirectory.Substring(contentRootDirectory.Length);
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
                Meshes = new(n.AsArray(), n => new(this, n, gDevice != null));
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
            {
                animationsJsonArray = n.AsArray();
                Animations = new(animationsJsonArray, n => new(this, n));
            }
            if ((n = root["accessors"]) != null)
                KeyFrames = new(this);

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

            byte[] vertexData = GetVertexData(key, attributesObj, out VertexDeclaration vertexDeclaration, out int vertexCount);
            vertexBuffer = disposables.Use(new VertexBuffer(gDevice, vertexDeclaration, vertexCount, BufferUsage.None));
            vertexBuffer.SetData(vertexData);

            vertexBuffers.Add(key, vertexBuffer);
            return vertexBuffer;
        }

        internal byte[] GetVertexData(string key, JsonObject attributesObj, out VertexDeclaration vertexDeclaration, out int vertexCount)
        {
            List<VertexPartGL> vertexParts = new();
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
                    componentSize = BufferHelper.Convert16BitColorChannelTo8Bit(ref bufferBytes);
                }

                int elementSize = componentSize * componentCount;
                offset += elementSize;

                vertexParts.Add(new VertexPartGL(v, bufferBytes, elementSize));
            }

            int vertexStride = offset;
            vertexDeclaration = new VertexDeclaration(vertexStride, vertexParts.Select(f => f.VertexElement).ToArray());

            // merge vertex data
            byte[] vertexData = new byte[vertexParts.Sum(f => f.BufferBytes.Length)];
            vertexCount = vertexParts[0].BufferBytes.Length / vertexParts[0].VertexElementSize;
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

            return vertexData;
        }

        public IndexBuffer GetIndexBuffer(int id)
        {
            IndexBuffer? indexBuffer;
            if (indexBuffers.TryGetValue(id, out indexBuffer))
                return indexBuffer;

            byte[] indicesData = GetIndexData(id, out IndexElementSize indexElementSize, out int indicesCount);

            indexBuffer = disposables.Use(new IndexBuffer(gDevice, indexElementSize, indicesCount, BufferUsage.None));
            indexBuffer.SetData(indicesData);

            indexBuffers.Add(id, indexBuffer);
            return indexBuffer;
        }

        public byte[] GetIndexData(int id, out IndexElementSize indexElementSize, out int indexCount)
        {
            // set index buffer
            var indicesAccessor = accessorsArr![id]!;
            byte[] indexData = GetBytesFromBuffer(id);
            indexCount = indicesAccessor["count"]!.GetValue<int>();
            indexElementSize = indexData.Length / indexCount != 2 ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits;
            return indexData;
        }


        public byte[] GetBytesFromBuffer(int accessorIndex)
        {
            var bufferView = bufferViewsArr![accessorsArr![accessorIndex]!["bufferView"]!.GetValue<int>()];
            int bufferByteLength = bufferView!["byteLength"]!.GetValue<int>();
            var buffer = buffersArr![bufferView["buffer"]!.GetValue<int>()];
            var bufferUri = buffer!["uri"]!.GetValue<string>();

            int bufferByteOffset = 0;
            JsonNode? n = bufferView["byteOffset"];
            if (n != null)
                bufferByteOffset = n.GetValue<int>();

            if (contentCollector != null)
            {
                Ref<byte[]> wholeBuffer = disposables.Use(contentCollector.Use<byte[]>(ContentHelper.UriToContentFileWithExtension(bufferUri, gltfDirRelativeToContent)));
                byte[] bufferBytes = new byte[bufferByteLength];
                Buffer.BlockCopy(wholeBuffer.Value, bufferByteOffset, bufferBytes, 0, bufferByteLength);

                return bufferBytes;
            }
            else
            {
                string filePath = Path.GetFullPath(Path.Combine(contentRootDirectory, gltfDirRelativeToContent, bufferUri));
                byte[] bufferBytes = new byte[bufferByteLength];
                using (var stream = File.OpenRead(filePath))
                {
                    stream.Position = bufferByteOffset;
                    stream.Read(bufferBytes, 0, bufferByteLength);
                }
                return bufferBytes;
            }
        }

        public void Dispose()
        {
            disposables.Dispose();

            if (Images != null)
            {
                Images.ForEachLoaded(f => f.Dispose());
                Images.Clear();
            }
        }

        internal Ref<Texture2D> GetTexture(string imageUri)
        {
            if (contentCollector == null)
                return new Ref<Texture2D>(new Pointer<Texture2D>(), null);

            return contentCollector.Use<Texture2D>(ContentHelper.UriToContentFile(imageUri, gltfDirRelativeToContent));
        }

        public void Draw(IShaderWorld shader, IShaderMaterial? shaderMaterial, IShaderSkin? shaderSkin)
        {
            CurrentScene?.Draw(shader, shaderMaterial, shaderSkin);
        }

        public void Draw(IShaderWorld shader, IShaderMaterial? shaderMaterial, IShaderSkin? shaderSkin, Predicate<NodeGL> goDown)
        {
            CurrentScene?.Draw(shader, shaderMaterial, shaderSkin, goDown);
        }

        public void DrawSelect(IShaderWorld shader, IShaderMaterial? shaderMaterial, IShaderSkin? shaderSkin, Predicate<NodeGL> select, Matrix transform)
        {
            CurrentScene?.DrawSelect(shader, shaderMaterial, shaderSkin, select, transform);
        }

        public IEnumerable<NodeGL>? GetNodes()
        {
            return Scenes?.Get(SceneIndex)?.GetNodes();
        }
        public IEnumerable<NodeGL>? GetNodes(Func<NodeGL, bool> goDeeper)
        {
            return Scenes?.Get(SceneIndex)?.GetNodes(goDeeper);
        }
        public NodeGL? FindNode(string name)
        {
            return GetNodes()?.FirstOrDefault(f => f.Name == name);
        }

        public int? GetAnimationIndex(string name)
        {
            if (animationsJsonArray == null) // no animations in json?
                return null;

            if (AnimationNameToIndex == null)
            {
                InitAnimationNameToIndex();
            }
            int index;
            if (AnimationNameToIndex!.TryGetValue(name, out index))
                return index;
            return -1;
        }

        private void InitAnimationNameToIndex()
        {
            if (animationsJsonArray == null) // no animations in json?
                return;

            AnimationNameToIndex = new();

            for (int i = 0; i < animationsJsonArray!.Count; i++)
            {
                string name = animationsJsonArray[i]!["name"]!.GetValue<string>();
                AnimationNameToIndex.Add(name, i);
            }
        }
    }
}
