﻿
using Microsoft.Xna.Framework;

namespace BytingLib
{
    public class Rect : IShape
    {
        private Vector2 pos;
        public Vector2 Size;

        public Rect()
        {
        }
        public Rect(Vector2 pos, Vector2 size)
        {
            Initialize(pos, size);
        }
        public Rect(float x, float y, float width, float height)
        {
            Initialize(new Vector2(x, y), new Vector2(width, height));
        }
        public Rect(Rectangle rect)
        {
            Initialize(new Vector2(rect.X, rect.Y), new Vector2(rect.Width, rect.Height));
        }
        public Rect(Rect rect)
        {
            Initialize(rect.pos, rect.Size);
        }
        public Rect(Vector2 pos, Vector2 size, Vector2 originNormalized, bool round = false)
        {
            pos -= size * originNormalized;
            if (round)
                pos = pos.GetRound();
            Initialize(pos, size);
        }
        public Rect(float x, float y, float width, float height, float originNormalizedX, float originNormalizedY, bool round = false)
        {
            Vector2 size = new Vector2(width, height);
            Vector2 pos = new Vector2(x, y) - size * new Vector2(originNormalizedX, originNormalizedY);
            if (round)
                pos = pos.GetRound();
            Initialize(pos, size);
        }

        public static Rect FromPoints(Vector2 a, Vector2 b)
        {
            return new Rect(a, b - a);
        }

        public Vector2 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }
        public float Width
        {
            get => Size.X;
            set => Size.X = value;
        }
        public float Height
        {
            get => Size.Y;
            set => Size.Y = value;
        }

        public float Top { get => pos.Y; set => Y = value; }
        public float Left { get => pos.X; set => X = value; }
        public float Right { get => pos.X + Size.X; set => X = value - Size.X; }
        public float Bottom { get => pos.Y + Size.Y; set => Y = value - Size.Y; }
        public Vector2 TopLeft { get => pos; set => pos = value; }
        public Vector2 TopRight { get => pos + new Vector2(Size.X, 0); set => pos = value - new Vector2(Size.X, 0); }
        public Vector2 BottomLeft { get => pos + new Vector2(0, Size.Y); set => pos = value - new Vector2(0, Size.Y); }
        public Vector2 BottomRight { get => pos + Size; set => pos = value - Size; }
        public Vector2 TopV { get => new Vector2(pos.X + Size.X / 2f, pos.Y); set => pos = value - new Vector2(Size.X / 2f, 0); }
        public Vector2 LeftV { get => new Vector2(pos.X, pos.Y + Size.Y / 2f); set => pos = value - new Vector2(0, Size.Y / 2f); }
        public Vector2 RightV { get => new Vector2(pos.X + Size.X, pos.Y + Size.Y / 2f); set => pos = value - new Vector2(Size.X, Size.Y / 2f); }
        public Vector2 BottomV { get => new Vector2(pos.X + Size.X / 2f, pos.Y + Size.Y); set => pos = value - new Vector2(Size.X / 2f, Size.Y); }

        public void Initialize(Vector2 pos, Vector2 size)
        {
            if (size.X < 0)
            {
                pos.X += size.X;
                size.X = -size.X;
            }
            if (size.Y < 0)
            {
                pos.Y += size.Y;
                size.Y = -size.Y;
            }

            this.pos = pos;
            Size = size;
        }

        public bool CollidesWith(IShape shape)
        {
            throw new NotImplementedException();
        }

        public CollisionResult DistanceTo(IShape shape, Vector2 dir)
        {
            throw new NotImplementedException();
        }

        public Rect GetBoundingRectangle() => new Rect(this);
        public object Clone() => new Rect(this);

        public override string ToString()
        {
            return pos.X + "; " + pos.Y + "; " + Size.X + "; " + Size.Y;
        }

        public static Rect operator *(Rect rect, float scale)
        {
            Rect clone = (Rect)rect.Clone();
            clone.pos *= scale;
            clone.Size *= scale;
            return clone;
        }
        public static Rect operator *(Rect rect, Vector2 scale)
        {
            Rect clone = (Rect)rect.Clone();
            clone.pos *= scale;
            clone.Size *= scale;
            return clone;
        }

        public void PushIntoRectangle(Rect bounds) //no center, if bounds is smaller then this rectangle!
        {
            if (X < bounds.X)
                X = bounds.X;
            else if (Right > bounds.Right)
                X = bounds.Right - Size.X;

            if (Y < bounds.Y)
                Y = bounds.Y;
            else if (Bottom > bounds.Bottom)
                Y = bounds.Bottom - Size.Y;
        }

        public Vector2 VectorPushInto(Vector2 pos) //no center, if bounds is smaller then this rectangle!
        {
            if (pos.X < X)
                pos.X = X;
            else if (pos.X > Right)
                pos.X = Right;

            if (pos.Y < Y)
                pos.Y = Y;
            else if (pos.Y > Bottom)
                pos.Y = Bottom;

            return pos;
        }

        public Rect Grow(float grow)
        {
            pos -= new Vector2(grow);
            Size += new Vector2(grow * 2);
            return this;
        }
        public Rect Grow(float growX, float growY) => Grow(new Vector2(growX, growY));
        public Rect Grow(Vector2 grow)
        {
            pos -= grow;
            Size += grow * 2;
            return this;
        }

        public Rect Expand(Vector2 expand)
        {
            return Expand(expand.X, expand.Y);
        }
        public Rect Expand(float x, float y)
        {
            if (x > 0)
                Size.X += x;
            else if (x < 0)
            {
                pos.X += x;
                Size.X -= x;
            }

            if (y > 0)
                Size.Y += y;
            else if (y < 0)
            {
                pos.Y += y;
                Size.Y -= y;
            }
            return this;
        }

        public Rect ApplyPadding(float left, float right, float top, float bottom)
        {
            Size.X -= left + right;
            Size.Y -= top + bottom;
            X += left;
            Y += top;
            return this;
        }

        public void SetCenter(Vector2 center)
        {
            pos = center - Size / 2f;
        }
        public void SetPosByOriginNormalized(Vector2 newPos, Vector2 origin)
        {
            pos = newPos - origin * Size;
        }

        public void SetToAnchor(Anchor anchor, bool round = true)
        {
            pos = anchor.pos - Size * anchor.origin;
            if (round)
                pos = pos.GetRound();
            Initialize(pos, Size);
        }
        public void SetToAnchor(Anchor anchor, Vector2 size, bool round = true)
        {
            pos = anchor.pos - size * anchor.origin;
            if (round)
                pos = pos.GetRound();
            Initialize(pos, size);
        }
        public Anchor GetCenterAnchor()
        {
            return new Anchor(GetCenter());
        }

        public Anchor GetAnchor(float anchorX, float anchorY)
        {
            return new Anchor(GetPos(anchorX, anchorY), new Vector2(anchorX, anchorY));
        }

        private Vector2 GetPos(float anchorX, float anchorY) => GetPos(new Vector2(anchorX, anchorY));
        private Vector2 GetPos(Vector2 anchor) => pos + Size * anchor;
        public Vector2 GetCenter() => pos + Size / 2f;

        public Anchor GetAnchor(Vector2 anchor)
        {
            return new Anchor(GetPos(anchor), anchor);
        }

        public Polygon ToPolygon()
        {
            return new Polygon(pos, new List<Vector2>() { Vector2.Zero, new Vector2(Size.X, 0), Size, new Vector2(0, Size.Y) });
        }
    }

}
