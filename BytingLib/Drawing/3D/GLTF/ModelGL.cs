using System.Text.Json.Nodes;
using BytingLib.DataTypes;

namespace BytingLib
{

    public class ModelGL : IDisposable
    {
        public readonly int SceneIndex;

        public readonly JsonDictionaryCache<SceneGL>? Scenes;
        public readonly DictionaryCacheNode? Nodes;
        public readonly JsonDictionaryCache<MeshGL>? Meshes;
        public readonly JsonDictionaryCache<MaterialGL>? Materials;
        public readonly JsonDictionaryCache<TextureGL>? Textures;
        public readonly JsonDictionaryCache<SamplerGL>? Samplers;
        public readonly JsonDictionaryCache<ImageGL>? Images;
        public readonly JsonDictionaryCache<SkinGL>? Skins;
        public readonly JsonDictionaryCache<AnimationGL>? Animations;
        internal readonly DictionaryCacheChannelTargets ChannelTargets;
        internal readonly DictionaryCacheKeyFrames KeyFrames;

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
            ChannelTargets = new DictionaryCacheChannelTargets(this);

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
                    componentSize = BufferHelper.Convert8BitColorTo4Bit(ref bufferBytes);
                }

                int elementSize = componentSize * componentCount;
                offset += elementSize;

                vertexParts.Add(new VertexPartGL(v, bufferBytes, elementSize));
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

        public void Draw(IShaderDefault shader)
        {
            Scenes?.Get(SceneIndex)?.Draw(shader);
        }

        public NodeGL? FindNode(string name)
        {
            return Scenes?.Get(SceneIndex)?.FindNode(name);
        }
    }
}
