
namespace BytingLib
{
    public class Rectangle3 : Shape3Collection
    {
        Triangle3 Tri1 { get; set; }
        Triangle3 Tri2 { get; set; }

        public Rectangle3() : this(Vector3.Zero, Vector3.Zero, Vector3.Zero) { }

        public Rectangle3(Vector3 center, Vector3 width, Vector3 height) : base(center, new() { new Triangle3(), new Triangle3() })
        {
            Tri1 = (Shapes[0] as Triangle3)!;
            Tri2 = (Shapes[1] as Triangle3)!;

            width /= 2f;
            height /= 2f;

            Vector3 bl = -width - height;
            Vector3 tl = -width + height;
            Vector3 tr = width + height;
            Vector3 br = width - height;

            Tri1.Vertices[0] += tl;
            Tri1.Vertices[1] += tr;
            Tri1.Vertices[2] += bl;

            Tri2.Vertices[0] += tr;
            Tri2.Vertices[1] += br;
            Tri2.Vertices[2] += bl;
        }

        public void Reset(Vector3 width, Vector3 height)
        {
            width /= 2f;
            height /= 2f;
            Vector3 bl = Pos - width - height;
            Vector3 tl = Pos - width + height;
            Vector3 tr = Pos + width + height;
            Vector3 br = Pos + width - height;

            Tri1.Vertices[0] = tl;
            Tri1.Vertices[1] = tr;
            Tri1.Vertices[2] = bl;

            Tri2.Vertices[0] = tr;
            Tri2.Vertices[1] = br;
            Tri2.Vertices[2] = bl;
        }
    }
}
