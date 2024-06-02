using System.Text.Json.Nodes;

namespace BytingLib
{
    public class CameraGL
    {
        public string? Name { get; }
        public float AspectRatio { get; }
        public float FovY { get; }
        public float Near { get; }
        public float Far { get; }

        public CameraGL(JsonNode n)
        {
            Name = n["name"]?.GetValue<string>();
            var perspective = n["perspective"];
            if (perspective != null)
            {
                AspectRatio = perspective["aspectRatio"]?.GetValue<float>() ?? throw new BytingException();
                FovY = perspective["yfov"]?.GetValue<float>() ?? throw new BytingException();
                Near = perspective["zfar"]?.GetValue<float>() ?? throw new BytingException();
                Far = perspective["znear"]?.GetValue<float>() ?? throw new BytingException();
            }
        }

        public override string ToString() => "Camera: " + Name;
    }
}
