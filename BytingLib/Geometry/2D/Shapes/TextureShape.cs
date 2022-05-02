using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytingLib
{
    public class TextureShape : IShape
    {
        public Vector2 Pos { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float X { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float Y { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Int2 Size { get; set; }
        public Color[] colorData;

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public bool CollidesWith(IShape shape)
        {
            throw new NotImplementedException();
        }

        public CollisionResult DistanceTo(IShape shape, Vector2 dir)
        {
            throw new NotImplementedException();
        }

        public Rect GetBoundingRectangle()
        {
            throw new NotImplementedException();
        }

        internal Matrix GetMatrix()
        {
            throw new NotImplementedException();
        }

        internal bool IsTransformed()
        {
            throw new NotImplementedException();
        }
    }
}
