using System.Text.Json.Nodes;

namespace BytingLib
{
    public class NodeGL
    {
        MeshGL? mesh;
        SkinGL? skin;
        Matrix LocalTransform;
        //Matrix initialTransform;
        JointTransform? jointTransform;
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

        public void Draw(IShaderDefault shader) => Draw(shader, Matrix.Identity);

        private void Draw(IShaderDefault shader, Matrix GlobalNodeTransform)
        {
            GlobalNodeTransform = LocalTransform * GlobalNodeTransform;
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
            if (GlobalTransformCalculationId == globalTransformCalculationId)
                return;

            if (jointTransform != null && jointTransform.Dirty)
            {
                LocalTransform = jointTransform.GetTransform();
                jointTransform.Dirty = false;
            }

            if (Parent == null)
            {
                GlobalJointTransform = LocalTransform;
            }
            else
            {
                Parent.CalculateGlobalTransform(globalTransformCalculationId);
                GlobalJointTransform = LocalTransform * Parent.GlobalJointTransform;
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
            jointTransform ??= new JointTransform();
            jointTransform.Rotation = rotation;
            jointTransform.Dirty = true;
        }

        public void SetTranslation(Vector3 translation)
        {
            jointTransform ??= new JointTransform();
            jointTransform.Translation = translation;
            jointTransform.Dirty = true;
        }

        public void SetScale(Vector3 scale)
        {
            jointTransform ??= new JointTransform();
            jointTransform.Scale = scale;
            jointTransform.Dirty = true;
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
}
