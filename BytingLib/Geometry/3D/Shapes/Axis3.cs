﻿
using BytingLib.DataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class Axis3 : IShape3
    {
        private Vector3 pos;
        /// <summary>This vector should never be set to a non-normalized vector.</summary>
        public Vector3 Dir;

        /// <summary>Be sure to pass a normalized direction.</summary>
        public Axis3(Vector3 position, Vector3 normalizedDirection)
        {
            Pos = position;
            Dir = normalizedDirection;
        }

        public Vector3 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }
        public float Z { get => pos.Z; set => pos.Z = value; }

        public Type GetCollisionType() => typeof(Axis3);

        public virtual object Clone() => MemberwiseClone();

        public BoundingBox GetBoundingBox()
        {
            Vector3 boundingSize = Vector3.Zero;

            if (Dir.X != 0)
                boundingSize.X = float.PositiveInfinity;
            if (Dir.Y != 0)
                boundingSize.Y = float.PositiveInfinity;
            if (Dir.Z != 0)
                boundingSize.Z = float.PositiveInfinity;

            return new BoundingBox(Pos - boundingSize, Pos + boundingSize);
        }

        public void Render(PrimitiveBatcher batcher, Color color)
        {
            var b = batcher.LineBatcher;

            // draw two lines originating from the origin
            b.EnsureAdditionalArrayCapacity(3, 4);

            // indices
            // 0 - 1 1 - 2
            b.indices[b.indicesIndex++] = b.verticesIndex + 0;
            b.indices[b.indicesIndex++] = b.verticesIndex + 1;
            b.indices[b.indicesIndex++] = b.verticesIndex + 1;
            b.indices[b.indicesIndex++] = b.verticesIndex + 2;

            // line: start - pos - end
            b.vertices[b.verticesIndex++] = new(pos - Dir * 1000f, color);
            b.vertices[b.verticesIndex++] = new(pos, color);
            b.vertices[b.verticesIndex++] = new(pos + Dir * 1000f, color);
        }
    }
}
