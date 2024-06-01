﻿namespace BytingLib
{
    public class Anchor
    {
        public Vector2 Pos;
        public Vector2 Origin;

        public float X { get => Pos.X; set => Pos.X = value; }
        public float Y { get => Pos.Y; set => Pos.Y = value; }

        public float OX { get => Origin.X; set => Origin.X = value; }
        public float OY { get => Origin.Y; set => Origin.Y = value; }

        public Anchor(float x, float y, float originX, float originY)
        {
            this.Pos = new Vector2(x, y);
            this.Origin = new Vector2(originX, originY);
        }
        public Anchor(Vector2 pos, Vector2 origin)
        {
            this.Pos = pos;
            this.Origin = origin;
        }

        public Anchor(Vector2 pos)
        {
            this.Pos = pos;
            this.Origin = new Vector2(0.5f);
        }
        public Anchor(float x, float y)
        {
            this.Pos = new Vector2(x, y);
            this.Origin = new Vector2(0.5f);
        }
        public Anchor()
        {
            this.Pos = new Vector2(0, 0);
            this.Origin = new Vector2(0.5f);
        }

        public static Anchor Center(Vector2 v) => Center(v.X, v.Y);
        public static Anchor Center(float x, float y)
        {
            return new Anchor(x, y);
        }

        public static Anchor Bottom(Vector2 v) => Bottom(v.X, v.Y);
        public static Anchor Bottom(float x, float y)
        {
            return new Anchor(x, y, 0.5f, 1f);
        }
        public static Anchor Top(Vector2 v) => Top(v.X, v.Y);
        public static Anchor Top(float x, float y)
        {
            return new Anchor(x, y, 0.5f, 0f);
        }
        public static Anchor Left(Vector2 v) => Left(v.X, v.Y);
        public static Anchor Left(float x, float y)
        {
            return new Anchor(x, y, 0f, 0.5f);
        }
        public static Anchor Right(Vector2 v) => Right(v.X, v.Y);
        public static Anchor Right(float x, float y)
        {
            return new Anchor(x, y, 1f, 0.5f);
        }

        public static Anchor BottomLeft(Vector2 v) => BottomLeft(v.X, v.Y);
        public static Anchor BottomLeft(float x, float y)
        {
            return new Anchor(x, y, 0f, 1f);
        }
        public static Anchor BottomRight(Vector2 v) => BottomRight(v.X, v.Y);
        public static Anchor BottomRight(float x, float y)
        {
            return new Anchor(x, y, 1f, 1f);
        }
        public static Anchor TopLeft(Vector2 v) => TopLeft(v.X, v.Y);
        public static Anchor TopLeft(float x, float y)
        {
            return new Anchor(x, y, 0f, 0f);
        }
        public static Anchor TopRight(Vector2 v) => TopRight(v.X, v.Y);
        public static Anchor TopRight(float x, float y)
        {
            return new Anchor(x, y, 1f, 0f);
        }

        public Rect Rectangle(float sizeXY)
        {
            return new Rect(X, Y, sizeXY, sizeXY, OX, OY);
        }
        public Rect Rectangle(Vector2 size)
        {
            return new Rect(X, Y, size.X, size.Y, OX, OY);
        }

        public Rect Rectangle(float width, float height)
        {
            return new Rect(X, Y, width, height, OX, OY);
        }

        public Anchor Clone()
        {
            return (Anchor)this.MemberwiseClone();
        }
    }
}
