﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class PointF : IShape
    {
        private Vector2 pos;

        public Vector2 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }

        public PointF(Vector2 pos)
        {
            this.pos = pos;
        }
        public PointF(float x, float y)
        {
            pos = new Vector2(x,y);
        }
        public PointF(Point point)
        {
            pos = point.ToVector2();
        }

        public object Clone()
        {
            return new PointF(pos);
        }

        public Rect GetBoundingRectangle()
        {
            return new Rect(pos, Vector2.Zero);
        }

        public void Draw(SpriteBatch spriteBatch, Color color, float depth = 0f)
        {
            spriteBatch.DrawCross(pos, 10f, 2f, color, depth);
        }
    }
}
