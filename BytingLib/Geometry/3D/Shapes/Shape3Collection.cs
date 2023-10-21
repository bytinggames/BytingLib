namespace BytingLib
{
    public class Shape3Collection : IShape3Collection
    {
        private Vector3 pos;
        public List<IShape3> Shapes { get; set; }
        public List<bool>? ShapesEnabled { get; set; } = null;

        public Shape3Collection(Vector3 pos, List<IShape3> shapes)
        {
            Shapes = shapes;
            Pos = pos;
        }

        public Vector3 Pos
        {
            get => pos;
            set
            {
                // LATER: it would be better to make it more like a scene graph. That means, that the child shapes positions are only locally
                // but... would it be as performant?
                Vector3 move = value - pos;
                for (int i = 0; i < Shapes.Count; i++)
                {
                    Shapes[i].Pos += move;
                }

                pos = value;
            }
        }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }
        public float Z { get => pos.Z; set => pos.Z = value; }

        public IEnumerable<IShape3> ShapesEnumerable
        {
            get
            {
                for (int i = 0; i < Shapes.Count; i++)
                {
                    if (IsShapeEnabled(i))
                    {
                        yield return Shapes[i];
                    }
                }
            }
        }

        public bool IsShapeEnabled(int index)
        {
            if (ShapesEnabled == null || ShapesEnabled.Count <= index)
            {
                return true;
            }

            return ShapesEnabled[index];
        }

        public void EnableShape(int index, bool enable)
        {
            if (ShapesEnabled == null)
            {
                ShapesEnabled = new List<bool>();
            }

            while (ShapesEnabled.Count <= index)
            {
                ShapesEnabled.Add(true);
            }

            ShapesEnabled[index] = enable;
        }

        public Type GetCollisionType() => typeof(IShape3Collection);

        public virtual object Clone()
        {
            Shape3Collection clone = (Shape3Collection)MemberwiseClone();

            clone.Shapes = new List<IShape3>();
            for (int i = 0; i < Shapes.Count; i++)
            {
                clone.Shapes.Add((IShape3)Shapes[i].Clone());
            }
            if (ShapesEnabled != null)
            {
                clone.ShapesEnabled = new List<bool>(ShapesEnabled);
            }

            return clone;
        }

        public BoundingBox GetBoundingBox()
        {
            if (Shapes.Count == 0)
            {
                return default;
            }

            BoundingBox box = default;// = new BoundingBox(Vector3.In Shapes[0].GetBoundingBox();
            bool isset = false;
            for (int i = 0; i < Shapes.Count; i++)
            {
                if (IsShapeEnabled(i))
                {
                    if (!isset)
                    {
                        isset = true;
                        box = Shapes[0].GetBoundingBox();
                    }
                    else
                    {
                        BoundingBox cBox = Shapes[i].GetBoundingBox();
                        box.Min = Vector3.Min(box.Min, cBox.Min);
                        box.Max = Vector3.Max(box.Max, cBox.Max);
                    }
                }
            }

            if (!isset)
            {
                throw new Exception("empty shape collection cannot return a BoundingBox");
            }

            return box;
        }
    }
}
