using System.Collections.Generic;

namespace BytingLib.Test.BytingSerializer
{
    public class Data
    {
        [BytingProp(0)]
        public List<object> Objects { get; set; }

        public override string ToString()
        {
            string str = "Data(";
            for (int i = 0; i < Objects.Count; i++)
            {
                if (Objects[i] == this)
                    return "circular reference, ";
                else
                    str += Objects[i].ToString() + ", ";
            }
            return str + ")";
        }

        public IEnumerable<object> GetAllObjects()
        {
            for (int i = 0; i < Objects.Count; i++)
            {
                if (Objects[i] is Data data)
                {
                    if (data != this)
                    {
                        foreach (var item in data.GetAllObjects())
                        {
                            yield return item;
                        }
                    }
                }
                else
                    yield return Objects[i];
            }
        }
    }

    public class Point
    {
        [BytingProp(0)]
        public Vector2 Pos { get; set; }

        public override string ToString()
        {
            return $"Point({Pos.X},{Pos.Y})";
        }
    }

    public class Line
    {
        [BytingProp(0)]
        public Vector2 Pos1 { get; set; }
        [BytingProp(1)]
        public Vector2 Pos2 { get; set; }

        public override string ToString()
        {
            return $"Line({Pos1.X},{Pos1.Y},{Pos2.X},{Pos2.Y})";
        }
    }

    public class LineThick : Line
    {
        [BytingProp(0)]
        public float Thickness { get; set; }
    }

    public class PrivateProp
    {
        [BytingProp(0)]
        private float Prop { get; set; }

        public PrivateProp()
        {

        }

        public PrivateProp(float prop)
        {
            Prop = prop;
        }

        public override string ToString()
        {
            return $"PrivateProp({Prop})";
        }
    }

    public class PublicField
    {
        [BytingProp(0)]
        public float Field;

        public override string ToString()
        {
            return $"PublicField({Field})";
        }
    }
    public class PrivateField
    {
        [BytingProp(0)]
        private float Field;

        public PrivateField()
        {

        }

        public PrivateField(float field)
        {
            Field = field;
        }

        public override string ToString()
        {
            return $"PublicField({Field})";
        }
    }
}
