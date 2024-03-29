﻿using System.Text.Json.Nodes;

namespace BytingLib
{
    public class ImageGL : IDisposable
    {
        public Ref<Texture2D> Tex2D { get; set; }

        public ImageGL(ModelGL model, JsonNode n)
        {
            var imageUri = n["uri"]!.GetValue<string>();
            imageUri = imageUri.Replace("%20", " ");
            Tex2D = model.GetTexture(imageUri)!;
        }

        public void Dispose()
        {
            Tex2D.Dispose();
        }
    }
}
