using System.Text.Json.Nodes;

namespace BytingLib
{
    public class NodeGL : INodeContainer
    {
        public MeshGL? Mesh { get; }
        public SkinGL? Skin { get; }
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
                Mesh = model.Meshes?.Get(t.GetValue<int>());
                Skin = (t = n["skin"]) == null ? null : model.Skins?.Get(t.GetValue<int>());
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

        public void Draw(IShaderWorld shader, IShaderMaterial? shaderMaterial, IShaderSkin? shaderSkin, Predicate<NodeGL>? goDown = null) => Draw(shader, shaderMaterial, shaderSkin, Matrix.Identity, goDown);

        private void Draw(IShaderWorld shader, IShaderMaterial? shaderMaterial, IShaderSkin? shaderSkin, Matrix GlobalNodeTransform, Predicate<NodeGL>? goDown)
        {
            GlobalNodeTransform = localTransform * GlobalNodeTransform;
            using (shaderSkin == null ? null : Skin?.Use(shaderSkin, GlobalNodeTransform))
            {
                if (Mesh != null)
                {
                    using (shader.World.Use(f => GlobalNodeTransform * f))
                        Mesh.Draw(shader, shaderMaterial);
                }

                if (goDown == null)
                {
                    for (int i = 0; i < Children.Count; i++)
                        Children[i].Draw(shader, shaderMaterial, shaderSkin, GlobalNodeTransform, null);
                }
                else
                    for (int i = 0; i < Children.Count; i++)
                    {
                        if (goDown(Children[i]))
                            Children[i].Draw(shader, shaderMaterial, shaderSkin, GlobalNodeTransform, goDown);
                    }
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

        internal void SetParentIfNotHavingOne(NodeGL? parent)
        {
            Parent ??= parent;
        }

        internal void InitializeForAnimation()
        {
            JointTransform ??= new JointTransform(localTransform);
        }

        public Matrix GetGlobalTransform()
        {
            if (Parent == null)
                return localTransform;
            return localTransform * Parent.GetGlobalTransform();
        }

        /// <summary>Not the most performance efficient method.</summary>
        public IEnumerable<NodeGL> GetParents()
        {
            if (Parent == null)
                yield break;
            yield return Parent;
            foreach (var p in Parent.GetParents())
            {
                yield return p;
            }
        }
    }
}
