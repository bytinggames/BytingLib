using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BytingLib
{
    public class MeshBatch
    {
        struct TriangleBatchKey
        {
            public RenderSettingsOverride Settings;
            public Type Type;

            public TriangleBatchKey(RenderSettingsOverride settings, Type type)
            {
                Settings = settings;
                Type = type;
            }
        }

        Dictionary<TriangleBatchKey, MeshBatcher> batches = new();

        RenderSettings? defaultRenderSettings;

        public void Render<V>(V[] vertices, int[] indices, RenderSettingsOverride settings) where V : struct, IVertexType
        {
            MeshBatcher? batch = GetBatcher<V>(settings);
            ((MeshBatcher<V>)batch!).Add(vertices, indices);
        }
        public void Render<V>(V[] vertices, RenderSettingsOverride settings) where V : struct, IVertexType
        {
            MeshBatcher? batch = GetBatcher<V>(settings);
            ((MeshBatcher<V>)batch!).Add(vertices);
        }
        public void Render<V>(V[] vertices) where V : struct, IVertexType
        {
            MeshBatcher? batch = GetBatcher<V>(default(RenderSettingsOverride));
            ((MeshBatcher<V>)batch!).Add(vertices);
        }
        public void Render<V>(V[] vertices, int[] indices) where V : struct, IVertexType
        {
            MeshBatcher? batch = GetBatcher<V>(default(RenderSettingsOverride));
            ((MeshBatcher<V>)batch!).Add(vertices, indices);
        }

        public MeshBatcher<V> GetBatcher<V>(RenderSettingsOverride settings) where V : struct, IVertexType
        {
            Type vertexType = typeof(V);
            if (!batches.TryGetValue(new(settings, vertexType), out MeshBatcher? batch))
            {
                Type genericType = typeof(MeshBatcher<>).MakeGenericType(typeof(V));
                batch = (MeshBatcher)Activator.CreateInstance(genericType)!;
                batches.Add(new(settings, vertexType), batch);
            }

            return (MeshBatcher<V>)batch;
        }

        public void Begin(RenderSettings defaultRenderSettings)
        {
            if (this.defaultRenderSettings != null)
                throw new Exception("End() must be called, before Begin() can be called again.");

            this.defaultRenderSettings = defaultRenderSettings;

            foreach (var (_, value) in batches)
            {
                value.Begin();
            }
        }

        public void End()
        {
            if (defaultRenderSettings == null)
                throw new Exception("Begin() must be called, before End() can be called.");

            List<TriangleBatchKey> emptyBatches = new();
            foreach (var (key, value) in batches)
            {
                RenderSettings currentSettings = defaultRenderSettings.Clone();
                currentSettings.Override(key.Settings);
                if (!value.End(currentSettings))
                    emptyBatches.Add(key);
            }

            for (int i = 0; i < emptyBatches.Count; i++)
            {
                batches.Remove(emptyBatches[i]);
            }
            emptyBatches.Clear();

            defaultRenderSettings = null;
        }
    }
}
