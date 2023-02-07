using System.Text.Json.Nodes;

namespace BytingLib
{
    public class NodeGL
    {
        private readonly MeshGL? mesh;
        private readonly SkinGL? skin;
        private Matrix localTransform;

        private int globalTransformCalculationId;

        public JointTransform? JointTransform { get; private set; }
        public Matrix GlobalJointTransform { get; private set; } // needs to be updated according to LocalTransform and all local transforms of the parents
        public string? Name { get; set; }
        public List<NodeGL> Children { get; } = new();
        public NodeGL? Parent { get; private set; }

        public NodeGL(ModelGL model, JsonNode n, NodeGL? parent)
        {
            if (Parent == null)
                Parent = parent;

            localTransform = GetTransform(n);
            //initialTransform = LocalTransform;

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

        public override string ToString() => "Node: " + Name;

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

        public void Draw(IShaderGL shader) => Draw(shader, Matrix.Identity);

        private void Draw(IShaderGL shader, Matrix GlobalNodeTransform)
        {
            GlobalNodeTransform = localTransform * GlobalNodeTransform;
            using (skin?.Use(shader, GlobalNodeTransform))
            {
                if (mesh != null)
                {
                    using (shader.World.Use(f => GlobalNodeTransform * f))
                        mesh.Draw(shader);
                }

                for (int i = 0; i < Children.Count; i++)
                    Children[i].Draw(shader, GlobalNodeTransform);
            }
        }


        internal void CalculateGlobalTransform(int globalTransformCalculationId)
        {
            if (this.globalTransformCalculationId == globalTransformCalculationId)
                return;

            if (JointTransform != null && JointTransform.Dirty)
            {
                localTransform = JointTransform.GetTransform();
                JointTransform.Dirty = false;
            }

            if (Parent == null)
            {
                GlobalJointTransform = localTransform;
            }
            else
            {
                Parent.CalculateGlobalTransform(globalTransformCalculationId);
                GlobalJointTransform = localTransform * Parent.GlobalJointTransform;
            }
            this.globalTransformCalculationId = globalTransformCalculationId;
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

        internal void SetParentIfNotHavingOne(NodeGL? parent)
        {
            Parent ??= parent;
        }

        internal void InitializeForAnimation()
        {
            JointTransform ??= new JointTransform(localTransform);
        }
    }
}
