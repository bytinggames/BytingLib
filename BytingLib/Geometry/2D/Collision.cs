#pragma warning disable CS8629 // Nullable value type may be null.


namespace BytingLib
{
    public static class Collision
    {
        static readonly Type TShapeCollection = typeof(ShapeCollection);
        static readonly Type TVector2 = typeof(Vector2);
        static readonly Type TRect = typeof(Rect);
        static readonly Type TPolygon = typeof(Polygon);
        static readonly Type TCircle = typeof(Circle);
        static readonly Type TTextureShape = typeof(TextureShape);

        static readonly Type TPointF = typeof(PointF); // uses the same functions Vector2 can use

        static Collision()
        {
            // duplicate each collision function and reverse the parameters
            // in case the shapes are passed the other way around (a,b) -> (b,a)
            List<(Type, Type)> keys = collisionFunctions.Keys.ToList();
            foreach (var key in keys)
            {
                if (key.Item1 != key.Item2) // if they are the same, no need to swap them
                    collisionFunctions.Add((key.Item2, key.Item1), (a,b) => collisionFunctions[key](b,a));
            }
            // same for distance functions
            keys = distanceFunctions.Keys.ToList();
            foreach (var key in keys)
            {
                if (key.Item1 != key.Item2) // if they are the same, no need to swap them
                    distanceFunctions.Add((key.Item2, key.Item1), (a, b, dir) => distanceFunctions[key](b, a, -dir).InvertAxisAndReturn());
            }
            // same for distanceNoDir functions
            keys = distanceNoDirFunctions.Keys.ToList();
            foreach (var key in keys)
            {
                if (key.Item1 != key.Item2) // if they are the same, no need to swap them
                    distanceNoDirFunctions.Add((key.Item2, key.Item1), (a, b) => distanceNoDirFunctions[key](b, a).InvertAxisAndReturn());
            }

            keys = collisionFunctions.Keys.ToList();
            // also make sure point type calls can make use of vector functions
            foreach (var key in keys)
            {
                if (key.Item1 == TVector2)
                    collisionFunctions.Add((TPointF, key.Item2), (a, b) => collisionFunctions[key](((PointF)a).Pos, b));

                if (key.Item2 == TVector2)
                    collisionFunctions.Add((key.Item1, TPointF), (a, b) => collisionFunctions[key](a, ((PointF)b).Pos));
            }
            // same for distance functions
            keys = distanceFunctions.Keys.ToList();
            foreach (var key in keys)
            {
                if (key.Item1 == TVector2)
                    distanceFunctions.Add((TPointF, key.Item2), (a, b, dir) => distanceFunctions[key](((PointF)a).Pos, b, dir));

                if (key.Item2 == TVector2)
                    distanceFunctions.Add((key.Item1, TPointF), (a, b, dir) => distanceFunctions[key](a, ((PointF)b).Pos, dir));
            }
            // same for distance functions
            keys = distanceNoDirFunctions.Keys.ToList();
            foreach (var key in keys)
            {
                if (key.Item1 == TVector2)
                    distanceNoDirFunctions.Add((TPointF, key.Item2), (a, b) => distanceNoDirFunctions[key](((PointF)a).Pos, b));

                if (key.Item2 == TVector2)
                    distanceNoDirFunctions.Add((key.Item1, TPointF), (a, b) => distanceNoDirFunctions[key](a, ((PointF)b).Pos));
            }
        }

        static readonly Dictionary<(Type, Type), Func<object, object, bool>> collisionFunctions = new()
        {
            { (TShapeCollection, TVector2), (a, b) => ColShapeCollectionObject((ShapeCollection)a, b) },
            { (TShapeCollection, TRect), (a, b) => ColShapeCollectionObject((ShapeCollection)a, b) },
            { (TShapeCollection, TPolygon), (a, b) => ColShapeCollectionObject((ShapeCollection)a, b) },
            { (TShapeCollection, TCircle), (a, b) => ColShapeCollectionObject((ShapeCollection)a, b) },
            { (TShapeCollection, TTextureShape), (a, b) => ColShapeCollectionObject((ShapeCollection)a, b) },
            { (TShapeCollection, TShapeCollection), (a, b) => ColShapeCollectionObject((ShapeCollection)a, b) },

            { (TVector2, TVector2), (a, b) => ColVectorVector((Vector2)a, (Vector2)b) },
            { (TVector2, TRect), (a, b) => ColVectorRectangle((Vector2)a, (Rect)b) },
            { (TVector2, TPolygon), (a, b) => ColVectorPolygon((Vector2)a, (Polygon)b) },
            { (TVector2, TCircle), (a, b) => ColVectorCircle((Vector2)a, (Circle)b) },
            { (TVector2, TTextureShape), (a, b) => ColVectorTextureShape((Vector2)a, (TextureShape)b) },

            { (TRect, TRect), (a, b) => ColRectangleRectangle((Rect)a, (Rect)b) },
            { (TRect, TPolygon), (a, b) => ColRectanglePolygon((Rect)a, (Polygon)b) },
            { (TRect, TCircle), (a, b) => ColRectangleCircle((Rect)a, (Circle)b) },
            { (TRect, TTextureShape), (a, b) => ColRectangleTextureShape((Rect)a, (TextureShape)b) },


            { (TPolygon, TPolygon), (a, b) => ColPolygonPolygon((Polygon)a, (Polygon)b) },
            { (TPolygon, TCircle), (a, b) => ColPolygonCircle((Polygon)a, (Circle)b) },
            { (TPolygon, TTextureShape), (a, b) => ColPolygonTextureShape((Polygon)a, (TextureShape)b) },

            { (TCircle, TCircle), (a, b) => ColCircleCircle((Circle)a, (Circle)b) },
            { (TCircle, TTextureShape), (a, b) => ColCircleTextureShape((Circle)a, (TextureShape)b) },

            { (TTextureShape, TTextureShape), (a, b) => ColTextureShapeTextureShape((TextureShape)a, (TextureShape)b) },

            // special:
            { (TPointF, TPointF), (a, b) => ColVectorVector(((PointF)a).Pos, ((PointF)b).Pos) } // use (vector, vector) collision for (point, point)
        };

        static readonly Dictionary<(Type, Type), Func<object, object, Vector2, CollisionResult>> distanceFunctions = new()
        {
            { (TShapeCollection, TVector2), (a, b, dir) => DistShapeCollectionObject((ShapeCollection)a, b, dir) },
            { (TShapeCollection, TRect), (a, b, dir) => DistShapeCollectionObject((ShapeCollection)a, b, dir) },
            { (TShapeCollection, TPolygon), (a, b, dir) => DistShapeCollectionObject((ShapeCollection)a, b, dir) },
            { (TShapeCollection, TCircle), (a, b, dir) => DistShapeCollectionObject((ShapeCollection)a, b, dir) },
            { (TShapeCollection, TTextureShape), (a, b, dir) => DistShapeCollectionObject((ShapeCollection)a, b, dir) },
            { (TShapeCollection, TShapeCollection), (a, b, dir) => DistShapeCollectionObject((ShapeCollection)a, b, dir) },

            //{ (TVector2, TVector2), (a, b, dir) => DistVectorVector((Vector2)a, (Vector2)b, dir) },
            { (TVector2, TRect), (a, b, dir) => DistVectorRectangle((Vector2)a, (Rect)b, dir) },
            { (TVector2, TCircle), (a, b, dir) => DistVectorCircle((Vector2)a, (Circle)b, dir) },
            //{ (TVector2, TTextureShape), (a, b, dir) => DistVectorTextureShape((Vector2)a, (TextureShape)b, dir) },
            { (TVector2, TPolygon), (a, b, dir) => DistVectorPolygon((Vector2)a, (Polygon)b, dir) },

            { (TRect, TRect), (a, b, dir) => DistRectangleRectangle((Rect)a, (Rect)b, dir) },
            { (TRect, TCircle), (a, b, dir) => DistRectangleCircle((Rect)a, (Circle)b, dir) },
            //{ (TRect, TTextureShape), (a, b, dir) => DistRectangleTextureShape((Rect)a, (TextureShape)b, dir) },
            { (TRect, TPolygon), (a, b, dir) => DistRectanglePolygon((Rect)a, (Polygon)b, dir) },

            { (TPolygon, TPolygon), (a, b, dir) => DistPolygonPolygon((Polygon)a, (Polygon)b, dir) },
            { (TPolygon, TCircle), (a, b, dir) => DistPolygonCircle((Polygon)a, (Circle)b, dir) },
            //{ (TPolygon, TTextureShape), (a, b, dir) => DistPolygonTextureShape((Polygon)a, (TextureShape)b, dir) },

            { (TCircle, TCircle), (a, b, dir) => DistCircleCircle((Circle)a, (Circle)b, dir) },
            //{ (TCircle, TTextureShape), (a, b, dir) => DistCircleTextureShape((Circle)a, (TextureShape)b, dir) },

            //{ (TTextureShape, TTextureShape), (a, b, dir) => DistTextureShapeTextureShape((TextureShape)a, (TextureShape)b, dir) },
        };

        static readonly Dictionary<(Type, Type), Func<object, object, CollisionResult>> distanceNoDirFunctions = new()
        {
            { (TShapeCollection, TVector2), (a, b) => DistShapeCollectionObject((ShapeCollection)a, b) },
            { (TShapeCollection, TRect), (a, b) => DistShapeCollectionObject((ShapeCollection)a, b) },
            { (TShapeCollection, TPolygon), (a, b) => DistShapeCollectionObject((ShapeCollection)a, b) },
            { (TShapeCollection, TCircle), (a, b) => DistShapeCollectionObject((ShapeCollection)a, b) },
            { (TShapeCollection, TTextureShape), (a, b) => DistShapeCollectionObject((ShapeCollection)a, b) },
            { (TShapeCollection, TShapeCollection), (a, b) => DistShapeCollectionObject((ShapeCollection)a, b) },

            //{ (TVector2, TVector2), (a, b) => DistVectorVector((Vector2)a, (Vector2)b) },
            { (TVector2, TRect), (a, b) => DistVectorRectangle((Vector2)a, (Rect)b) },
            { (TVector2, TCircle), (a, b) => DistVectorCircle((Vector2)a, (Circle)b) },
            //{ (TVector2, TTextureShape), (a, b) => DistVectorTextureShape((Vector2)a, (TextureShape)b) },
            { (TVector2, TPolygon), (a, b) => DistVectorPolygon((Vector2)a, (Polygon)b) },

            { (TRect, TRect), (a, b) => DistRectangleRectangle((Rect)a, (Rect)b) },
            { (TRect, TCircle), (a, b) => DistRectangleCircle((Rect)a, (Circle)b) },
            //{ (TRect, TTextureShape), (a, b) => DistRectangleTextureShape((Rect)a, (TextureShape)b) },
            { (TRect, TPolygon), (a, b) => DistRectanglePolygon((Rect)a, (Polygon)b) },

            { (TPolygon, TPolygon), (a, b) => DistPolygonPolygon((Polygon)a, (Polygon)b) },
            { (TPolygon, TCircle), (a, b) => DistPolygonCircle((Polygon)a, (Circle)b) },
            //{ (TPolygon, TTextureShape), (a, b) => DistPolygonTextureShape((Polygon)a, (TextureShape)b) },

            { (TCircle, TCircle), (a, b) => DistCircleCircle((Circle)a, (Circle)b) },
            //{ (TCircle, TTextureShape), (a, b) => DistCircleTextureShape((Circle)a, (TextureShape)b) },

            //{ (TTextureShape, TTextureShape), (a, b) => DistTextureShapeTextureShape((TextureShape)a, (TextureShape)b) },
        };

        public static bool GetCollision(object shape1, object shape2)
        {
            Type t1 = (shape1 is IShape s1) ? s1.GetCollisionType() : shape1.GetType();
            Type t2 = (shape2 is IShape s2) ? s2.GetCollisionType() : shape2.GetType();
            Func<object, object, bool>? func;
            if (!collisionFunctions.TryGetValue((t1, t2), out func))
            {
                //if (shape1 is ShapeCollection collection1)
                //{
                //    return collection1.CollidesWith(shape2);
                //}
                //else if (shape2 is ShapeCollection collection2)
                //{
                //    return collection2.CollidesWith(shape1);
                //}

                throw new NotImplementedException($"A collision check between {shape1.GetType()} and {shape2.GetType()} is not implemented yet.");
            }
            return func.Invoke(shape1, shape2);
        }

        public static CollisionResult GetDistance(object shape1, object shape2, Vector2 dir)
        {
            Type t1 = (shape1 is IShape s1) ? s1.GetCollisionType() : shape1.GetType();
            Type t2 = (shape2 is IShape s2) ? s2.GetCollisionType() : shape2.GetType();
            Func<object, object, Vector2, CollisionResult>? func;
            if (!distanceFunctions.TryGetValue((t1, t2), out func))
                throw new NotImplementedException($"A distance check between {shape1.GetType()} and {shape2.GetType()} is not implemented yet.");
            return func(shape1, shape2, dir);
        }

        public static CollisionResult GetDistance(object shape1, object shape2)
        {
            Type t1 = (shape1 is IShape s1) ? s1.GetCollisionType() : shape1.GetType();
            Type t2 = (shape2 is IShape s2) ? s2.GetCollisionType() : shape2.GetType();
            Func<object, object, CollisionResult>? func;
            if (!distanceNoDirFunctions.TryGetValue((t1, t2), out func))
                throw new NotImplementedException($"A distance check between {shape1.GetType()} and {shape2.GetType()} is not implemented yet.");
            return func(shape1, shape2);
        }

        #region ShapeCollection

        public static bool ColShapeCollectionObject(ShapeCollection collection, object obj)
        {
            return collection.Shapes.Any(shape => GetCollision(shape, obj));
        }

        public static CollisionResult DistShapeCollectionObject(ShapeCollection collection, object obj, Vector2 dir)
        {
            CollisionResult crTotal = new CollisionResult();
            foreach (var shape in collection.Shapes)
            {
                var cr = GetDistance(shape, obj, dir);
                crTotal.Add(cr);
            }
            return crTotal;
        }

        public static CollisionResult DistShapeCollectionObject(ShapeCollection collection, object obj)
        {
            CollisionResult crTotal = new CollisionResult();
            foreach (var shape in collection.Shapes)
            {
                var cr = GetDistance(shape, obj);
                crTotal.Add(cr);
            }
            return crTotal;
        }

        #endregion

        #region Vector

        public static bool ColVectorVector(Vector2 vec1, Vector2 vec2) => vec1 == vec2;

        public static bool ColVectorRectangle(Vector2 vec, Rect rect)
        {
            return (vec.X >= rect.X && vec.X < rect.X + rect.Size.X
                 && vec.Y >= rect.Y && vec.Y < rect.Y + rect.Size.Y);
        }

        public static CollisionResult DistVectorRectangle(Vector2 vec, Rect rect)
        {
            float distX = rect.X - vec.X;
            if (distX >= 0)
                return new CollisionResult();
            else if (distX + rect.Size.X < rect.Size.X / 2f)
            {
                distX = rect.Size.X + distX;
                if (distX <= 0)
                    return new CollisionResult();
            }

            float distY = rect.Y - vec.Y;
            if (distY >= 0)
                return new CollisionResult();
            else if (distY + rect.Size.Y < rect.Size.Y / 2f)
            {
                distY = rect.Size.Y + distY;
                if (distY <= 0)
                    return new CollisionResult();
            }

            if (Math.Abs(distX) <= Math.Abs(distY))
                return new CollisionResult() { Distance = Math.Abs(distX), AxisCol = new Vector2(Math.Sign(distX), 0) };
            else
                return new CollisionResult() { Distance = Math.Abs(distY), AxisCol = new Vector2(0, Math.Sign(distY)) };
        }

        public static CollisionResult DistVectorRectangle(Vector2 vec, Rect rect, Vector2 dir)
        {
            return DistPolygonPolygon(new Polygon(vec, new List<Vector2>() { Vector2.Zero }), rect.ToPolygon(), dir);
        }


        public static bool ColVectorPolygon(Vector2 vec, Polygon polygon)
        {
            List<Vector2> edges = polygon.GetEdges();
            List<Vector2> axes = new List<Vector2>();
            axes.AddRange(GetAxes(edges));

            if (axes.Count == 0)
            {
                if (polygon.Vertices.Count == 1)
                    return polygon.Pos + polygon.Vertices[0] == vec;
                return false;
            }

            for (int i = 0; i < axes.Count; i++)
            {
                float projection1 = vec.X * axes[i].X + vec.Y * axes[i].Y;
                float[] projection2 = GetProjection(axes[i], polygon.Pos, polygon.Vertices, edges);

                float dirDist1 = projection2[1] - projection1;
                float dirDist2 = projection2[0] - projection1;

                if (dirDist1 < dirDist2)
                    dirDist2 = projection2[0] - projection1;
                else
                {
                    dirDist1 = dirDist2;
                    dirDist2 = projection2[1] - projection1;
                }

                if (Math.Sign(dirDist1) == Math.Sign(dirDist2))
                    return false;
            }
            return true;
        }

        public static CollisionResult DistVectorPolygon(Vector2 vec, Polygon polygon)
        {
            CollisionResult cr = new CollisionResult();

            List<Vector2> edges = polygon.GetEdges();
            List<Vector2> axes = new List<Vector2>();
            axes.AddRange(GetAxes(edges));

            if (axes.Count == 0)
                return cr;

            for (int i = 0; i < axes.Count; i++)
            {
                float projection1 = vec.X * axes[i].X + vec.Y * axes[i].Y;
                float[] projection2 = GetProjection(axes[i], polygon.Pos, polygon.Vertices, edges);

                float dirDist1 = projection2[1] - projection1;
                float dirDist2 = projection2[0] - projection1;

                if (dirDist1 < dirDist2)
                    dirDist2 = projection2[0] - projection1;
                else
                {
                    dirDist1 = dirDist2;
                    dirDist2 = projection2[1] - projection1;
                }

                if (Math.Sign(dirDist1) == Math.Sign(dirDist2))
                    return new CollisionResult();

                if (!cr.Distance.HasValue || Math.Abs(dirDist1) < Math.Abs(cr.Distance.Value))
                {
                    cr.AxisCol = -axes[i];
                    cr.Distance = -dirDist1;
                }
                if (Math.Abs(dirDist2) < Math.Abs(cr.Distance.Value))
                {
                    cr.AxisCol = axes[i];
                    cr.Distance = dirDist2;
                }
            }

            return cr;
        }

        public static CollisionResult DistVectorPolygon(Vector2 vec, Polygon polygon, Vector2 dir)
        {
            CollisionResult cr = new CollisionResult();

            if (dir == Vector2.Zero)
                return cr;

            List<Vector2> edges = polygon.GetEdges();
            List<Vector2> axes = new List<Vector2>();
            axes.AddRange(GetAxes(edges));

            float[][] dirDists = new float[axes.Count][];

            for (int i = 0; i < axes.Count; i++)
            {
                float projection1 = vec.X * axes[i].X + vec.Y * axes[i].Y;
                float[] projection2 = GetProjection(axes[i], polygon.Pos, polygon.Vertices, edges);

                float dotDir = dir.X * axes[i].X + dir.Y * axes[i].Y;

                if (dotDir == 0f)
                {
                    //no move direction on this axis, if no collision on this axis, then there is no distance on this direction
                    if (projection1 <= projection2[0] || projection1 >= projection2[1])
                        return new CollisionResult();
                }
                else
                {
                    float dirDist1 = (projection2[1] - projection1) / dotDir;
                    float dirDist2 = (projection2[0] - projection1) / dotDir;

                    if (dirDist1 < dirDist2)
                        dirDists[i] = (new float[] { dirDist1, (projection2[0] - projection1) / dotDir });
                    else
                        dirDists[i] = (new float[] { dirDist2, (projection2[1] - projection1) / dotDir });

                    if (!cr.Distance.HasValue || dirDists[i][0] > cr.Distance)
                    {
                        cr.AxisCol = axes[i];
                        cr.Distance = dirDists[i][0];
                    }

                    if (!cr.DistanceReversed.HasValue || dirDists[i][1] < cr.DistanceReversed)
                    {
                        cr.AxisColReversed = axes[i];
                        cr.DistanceReversed = dirDists[i][1];
                    }
                }
            }

            if (cr.Distance.HasValue)
            {
                for (int i = 0; i < dirDists.Length; i++)
                {
                    if (dirDists[i] != null && dirDists[i][1] <= cr.Distance)
                        return new CollisionResult();
                }

                if (Vector2.Dot(dir, cr.AxisCol) > 0)
                    cr.AxisCol = -cr.AxisCol;
                if (Vector2.Dot(dir, cr.AxisColReversed) < 0)
                    cr.AxisColReversed = -cr.AxisColReversed;
            }

            return cr;
        }


        public static bool ColVectorCircle(Vector2 vec, Circle circle)
        {
            Vector2 dist = circle.Pos - vec;
            return (dist.X * dist.X + dist.Y * dist.Y < circle.Radius * circle.Radius);
        }

        public static CollisionResult DistVectorCircle(Vector2 vec, Circle circle)
        {
            Vector2 dist = circle.Pos - vec;
            float distLength = (float)Math.Sqrt(dist.X * dist.X + dist.Y * dist.Y);

            if (distLength >= circle.Radius)
                return new CollisionResult();
            else
            {
                CollisionResult cr = new CollisionResult();
                cr.Distance = circle.Radius - distLength;
                cr.AxisCol = -Vector2.Normalize(dist);
                return cr;
            }
        }

        public static CollisionResult DistVectorCircle(Vector2 vec, Circle circle, Vector2 dir)
        {
            CollisionResult cr = new CollisionResult();

            Vector2 dist = circle.Pos - vec;

            float radius = circle.Radius;

            float?[] results = ABCFormula(dir.X * dir.X + dir.Y * dir.Y
                                         , -2 * (dist.X * dir.X + dist.Y * dir.Y)
                                         , dist.X * dist.X + dist.Y * dist.Y - radius * radius);

            if (results[0] != null)
            {
                cr.Distance = results[1].Value;
                cr.DistanceReversed = results[0].Value;

                Vector2 colPos = vec + dir * cr.Distance.Value;
                cr.AxisCol = Vector2.Normalize(colPos - circle.Pos);

                colPos = vec + dir * cr.DistanceReversed.Value;
                cr.AxisColReversed = Vector2.Normalize(colPos - circle.Pos);
            }

            return cr;
        }

        /// <summary>ONUSE: test</summary>
        public static bool ColVectorTextureShape(Vector2 vec, TextureShape sprite)
        {
            Rect rect = sprite.GetBoundingRect();

            if (ColVectorRectangle(vec, rect))
            {
                Vector2 transformedPos = Vector2.Transform(vec, Matrix.Invert(sprite.GetMatrix()));
                int x = (int)transformedPos.X;
                int y = (int)transformedPos.Y;
                if (x >= 0 && x < sprite.Size.X && y >= 0 && y < sprite.Size.Y)
                {
                    int index = y * sprite.Size.X + x;
                    //if (index >= 0 && index < sprite1.colorData.Length) //not necessary
                    return sprite.colorData[index].A != 0;
                }
            }
            return false;
        }

        #endregion

        #region Rectangle

        public static bool ColRectangleRectangle(Rect rect1, Rect rect2)
        {
            return (rect1.X + rect1.Size.X > rect2.X && rect1.X < rect2.X + rect2.Size.X
                 && rect1.Y + rect1.Size.Y > rect2.Y && rect1.Y < rect2.Y + rect2.Size.Y);

        }

        public static CollisionResult DistRectangleRectangle(Rect rect1, Rect rect2)
        {
            float distRight = rect2.Left - rect1.Right;
            if (distRight < 0)
            {
                float distLeft = rect1.Size.X + rect2.Size.X + distRight;
                if (distLeft > 0)
                {
                    float distDown = rect2.Top - rect1.Bottom;
                    if (distDown < 0)
                    {
                        float distUp = rect1.Size.Y + rect2.Size.Y + distDown;

                        if (distUp > 0)
                        {
                            distRight = MathExtension.MinAbs(distRight, distLeft); //distX
                            distDown = MathExtension.MinAbs(distDown, distUp); //distY

                            if (Math.Abs(distRight) <= Math.Abs(distDown))
                                return new CollisionResult() { Distance = Math.Abs(distRight), AxisCol = new Vector2(Math.Sign(distRight), 0) };
                            else
                                return new CollisionResult() { Distance = Math.Abs(distDown), AxisCol = new Vector2(0, Math.Sign(distDown)) };
                        }
                    }
                }
            }

            return new CollisionResult();
        }

        public static CollisionResult DistRectangleRectangle(Rect rect1, Rect rect2, Vector2 dir)
        {
            CollisionResult cr = new();

            // get distances on both axes forward and backward
            float[] distX = new float[] { (rect2.Left - rect1.Right) / dir.X, (rect2.Right - rect1.Left) / dir.X };
            float[] distY = new float[] { (rect2.Top - rect1.Bottom) / dir.Y, (rect2.Bottom - rect1.Top) / dir.Y };

            // move smaller number to index 0
            if (distX[0] > distX[1])
                CodeHelper.Swap(ref distX[0], ref distX[1]);
            if (distY[0] > distY[1])
                CodeHelper.Swap(ref distY[0], ref distY[1]);

            // check if no collision happens
            if (distX[1] < distY[0]
                || distY[1] < distX[0])
                return cr;

            // forward
            if (distX[0] < distY[0])
            {
                // y-axis-collision
                cr.AxisCol = new Vector2(0, dir.Y > 0 ? -1 : 1);
                cr.Distance = distY[0];
            }
            else
            {
                // x-axis-collision
                cr.AxisCol = new Vector2(dir.X > 0 ? -1 : 1, 0);
                cr.Distance = distX[0];
            }

            // reverse
            if (distX[1] > distY[1])
            {
                // y-axis-collision
                cr.AxisColReversed = new Vector2(0, dir.Y > 0 ? 1 : -1);
                cr.DistanceReversed = distY[1];
            }
            else
            {
                // x-axis-collision
                cr.AxisColReversed = new Vector2(dir.X > 0 ? 1 : -1, 0);
                cr.DistanceReversed = distX[1];
            }

            return cr;
        }


        public static bool ColRectanglePolygon(Rect rect, Polygon polygon)
        {
            return ColPolygonPolygon(rect.ToPolygon(), polygon);
        }

        public static CollisionResult DistRectanglePolygon(Rect rect, Polygon polygon)
        {
            return DistPolygonPolygon(rect.ToPolygon(), polygon);
        }

        public static CollisionResult DistRectanglePolygon(Rect rect, Polygon polygon, Vector2 dir)
        {
            return DistPolygonPolygon(rect.ToPolygon(), polygon, dir);
        }


        public static bool ColRectangleCircle(Rect rect, Circle circle)
        {
            Vector2 dist;
            dist.X = Math.Abs(circle.X - (rect.X + rect.Size.X / 2));
            dist.Y = Math.Abs(circle.Y - (rect.Y + rect.Size.Y / 2));

            //Check if bounding boxes collide
            if (dist.X >= circle.Radius + rect.Size.X / 2 || dist.Y >= circle.Radius + rect.Size.Y / 2)
                return false;

            //Check if Circle center in rectangle
            if (dist.X <= rect.Size.X / 2 || dist.Y <= rect.Size.Y / 2)
                return true;

            //Calc corner distance
            float cornerDistPow = (float)(Math.Pow(dist.X - rect.Size.X / 2, 2) + Math.Pow(dist.Y - rect.Size.Y / 2, 2));

            return cornerDistPow < Math.Pow(circle.Radius, 2);
        }

        public static CollisionResult DistRectangleCircle(Rect rect, Circle circle)
        {
            return DistPolygonCircle(rect.ToPolygon(), circle);
        }

        public static CollisionResult DistRectangleCircle(Rect rect, Circle circle, Vector2 dir)
        {
            return DistPolygonCircle(rect.ToPolygon(), circle, dir);
        }


        /// <summary>ONUSE: fix precision error (bottom of rectangle is 1 px smaller than given)</summary>
        public static bool ColRectangleTextureShape(Rect rect, TextureShape sprite)
        {
            Rect rect2 = sprite.GetBoundingRect();

            if (ColRectangleRectangle(rect2, rect))
            {
                Matrix transform1 = Matrix.Invert(sprite.GetMatrix());

                float xStart = Math.Max(0, rect2.X - rect.X);
                float xEnd = Math.Min(rect.Size.X, rect2.X + rect2.Size.X - rect.X);
                float yStart = Math.Max(0, rect2.Y - rect.Y);
                float yEnd = Math.Min(rect.Size.Y, rect2.Y + rect2.Size.Y - rect.Y);

                Vector2 pos1InY = Vector2.Transform(rect.Pos + new Vector2(xStart, yStart), transform1);
                Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transform1);
                Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transform1);
                for (float y2 = yStart; ;)
                {
                    Vector2 pos1 = pos1InY;
                    for (float x2 = xStart; ;)
                    {
                        if (pos1.X >= 0 && pos1.X < sprite.Size.X && pos1.Y >= 0 && pos1.Y < sprite.Size.Y)
                        {
                            if (sprite.colorData[(int)pos1.Y * sprite.Size.X + (int)pos1.X].A != 0)
                                return true;
                        }
                        pos1 += stepX;

                        x2++;
                        if (x2 == xEnd && pos1.X % 1 == 0)
                            break;
                        else if (x2 > xEnd)
                            break;
                    }
                    pos1InY += stepY;

                    y2++;
                    if (y2 == yEnd && pos1InY.Y % 1 == 0)
                        break;
                    else if (y2 > yEnd)
                        break;
                }
            }

            return false;
        }

        #endregion

        #region Polygon

        public static bool ColPolygonPolygon(Polygon poly1, Polygon poly2)
        {
            if (poly1.Vertices.Count < 2
                || poly2.Vertices.Count < 2)
                return false;

            List<Vector2> edges1 = poly1.GetEdges();
            List<Vector2> edges2 = poly2.GetEdges();
            List<Vector2> axes = new List<Vector2>();
            axes.AddRange(GetAxes(edges1));
            axes.AddRange(GetAxes(edges2));

            if (axes.Count == 0)
            {
                if (poly1.Vertices.Count == 1 && poly2.Vertices.Count == 1)
                    return poly1.Pos + poly1.Vertices[0] == poly2.Pos + poly2.Vertices[0];
                return false;
            }

            for (int i = 0; i < axes.Count; i++)
            {
                float[] projection1 = GetProjection(axes[i], poly1.Pos, poly1.Vertices, edges1);
                float[] projection2 = GetProjection(axes[i], poly2.Pos, poly2.Vertices, edges2);

                if (projection1[0] >= projection2[1] || projection1[1] <= projection2[0])
                    return false;

                ////minDist inclusion
                //if (projection1[0] > projection2[1] || projection1[1] < projection2[0])
                //    return false;
            }
            return true;
        }

        public static CollisionResult DistPolygonPolygon(Polygon poly1, Polygon poly2)
        {
            CollisionResult cr = new CollisionResult();

            List<Vector2> edges1 = poly1.GetEdges();
            List<Vector2> edges2 = poly2.GetEdges();
            List<Vector2> axes = new List<Vector2>();
            axes.AddRange(GetAxes(edges1));
            axes.AddRange(GetAxes(edges2));

            if (axes.Count == 0)
                return cr;

            for (int i = 0; i < axes.Count; i++)
            {
                float[] projection1 = GetProjection(axes[i], poly1.Pos, poly1.Vertices, edges1);
                float[] projection2 = GetProjection(axes[i], poly2.Pos, poly2.Vertices, edges2);

                float dirDist1 = projection2[1] - projection1[0];
                float dirDist2 = projection2[0] - projection1[1];

                if (dirDist1 < dirDist2)
                    dirDist2 = projection2[0] - projection1[1];
                else
                {
                    dirDist1 = dirDist2;
                    dirDist2 = projection2[1] - projection1[0];
                }

                if (Math.Sign(dirDist1) == Math.Sign(dirDist2))
                    return new CollisionResult();

                if (!cr.Distance.HasValue || Math.Abs(dirDist1) < Math.Abs(cr.Distance.Value))
                {
                    cr.AxisCol = -axes[i];
                    cr.Distance = -dirDist1;
                }
                if (Math.Abs(dirDist2) < Math.Abs(cr.Distance.Value))
                {
                    cr.AxisCol = axes[i];
                    cr.Distance = dirDist2;
                }
            }

            return cr;
        }

        public static CollisionResult DistPolygonPolygon(Polygon poly1, Polygon poly2, Vector2 dir)
        {
            if (poly1.LastEdgeClosed && poly2.LastEdgeClosed)
                return DistPolygonPolygonClosed(poly1, poly2, dir);
            else
                return DistPolygonPolygonOpened(poly1, poly2, dir);
        }

        public static CollisionResult DistPolygonPolygonClosed(Polygon poly1, Polygon poly2, Vector2 dir)
        {
            CollisionResult cr = new CollisionResult();

            if (dir == Vector2.Zero)
                return cr;

            List<Vector2> edges1 = poly1.GetClosedEdges();
            List<Vector2> edges2 = poly2.GetClosedEdges();

            //List<Axis> axes = new List<Axis>();

            int polyAxisCol = 0;
            int jNearest = -1;
            int jNearestReversed = -1;

            for (int i = 0; i < edges1.Count; i++)
            {
                int j = 0;
                int jFinal = 0;
                //axes.Add(new Axis(edges1[i]));
                Axis axis = new Axis(edges1[i]);
                axis.a1 = Vector2.Dot(poly1.Pos + poly1.Vertices[i], axis.axis);

                axis.b2 = Vector2.Dot(poly2.Pos + poly2.Vertices[j], axis.axis);

                for (j = 1; j < poly2.Vertices.Count; j++)
                {
                    float dot = Vector2.Dot(poly2.Pos + poly2.Vertices[j], axis.axis);
                    if (dot > axis.b2)
                    {
                        axis.b2 = dot;
                        jFinal = j;
                    }
                }

                float dotDir = Vector2.Dot(dir, axis.axis);
                float dist = (axis.b2 - axis.a1) / dotDir;

                if (dotDir < 0)
                {
                    if (!cr.Distance.HasValue || dist > cr.Distance)
                    {
                        cr.Distance = dist;
                        cr.AxisCol = axis.axis;
                        jNearest = jFinal;
                    }
                }
                else if (dotDir > 0)
                {
                    if (!cr.DistanceReversed.HasValue || dist < cr.DistanceReversed)
                    {
                        cr.DistanceReversed = dist;
                        cr.AxisColReversed = axis.axis;
                        jNearestReversed = jFinal;
                    }
                }
                else if (axis.b2 - axis.a1 <= 0) // ONTROUBLESHOOT: CHECK: <= before was < (2017.08.14)
                    return new CollisionResult();

            }
            for (int i = 0; i < edges2.Count; i++)
            {
                int j = 0;
                int jFinal = 0;
                //axes.Add(new Axis(edges2[i]));
                Axis axis = new Axis(edges2[i]);
                axis.a1 = Vector2.Dot(poly2.Pos + poly2.Vertices[i], axis.axis);

                axis.b2 = Vector2.Dot(poly1.Pos + poly1.Vertices[j], axis.axis);

                for (j = 1; j < poly1.Vertices.Count; j++)
                {
                    float dot = Vector2.Dot(poly1.Pos + poly1.Vertices[j], axis.axis);
                    if (dot > axis.b2)
                    {
                        axis.b2 = dot;
                        jFinal = j;
                    }
                }

                float dotDir = -Vector2.Dot(dir, axis.axis);
                float dist = (axis.b2 - axis.a1) / dotDir;

                if (dotDir < 0)
                {
                    if (!cr.Distance.HasValue || dist > cr.Distance)
                    {
                        cr.Distance = dist;
                        cr.AxisCol = -axis.axis;
                        jNearest = jFinal;
                        polyAxisCol = 1;
                    }
                }
                else if (dotDir > 0)
                {
                    if (!cr.DistanceReversed.HasValue || dist < cr.DistanceReversed)
                    {
                        cr.DistanceReversed = dist;
                        cr.AxisColReversed = -axis.axis;
                        jNearestReversed = jFinal;
                    }
                }
                else if (axis.b2 - axis.a1 <= 0) // ONTROUBLESHOOT: CHECK: <= before was < (2017.08.14)
                    return new CollisionResult();
            }

            if (cr.Distance.HasValue && cr.DistanceReversed <= cr.Distance)
                return new CollisionResult();

            cr.ColCornerPoly = polyAxisCol + 1;
            cr.ColCornerIndex = jNearest;

            return cr;
        }

        public static CollisionResult DistPolygonPolygonOpened(Polygon poly1, Polygon poly2, Vector2 dir)
        {
            bool open = false, openReversed = false;

            CollisionResult cr = new CollisionResult();

            if (dir == Vector2.Zero)
                return cr;

            List<Vector2> edges1 = poly1.GetClosedEdges();
            List<Vector2> edges2 = poly2.GetClosedEdges();

            for (int i = 0; i < edges1.Count; i++)
            {
                int endVerticeCol = -1; //if the collision happens to be on an open vertice save the index of this vertice here
                Axis axis = new Axis(edges1[i]);
                axis.a1 = Vector2.Dot(poly1.Pos + poly1.Vertices[i], axis.axis);

                axis.b2 = Vector2.Dot(poly2.Pos + poly2.Vertices[0], axis.axis);
                endVerticeCol = !poly2.LastEdgeClosed ? 0 : -1;

                for (int j = 1; j < poly2.Vertices.Count; j++)
                {
                    float dot = Vector2.Dot(poly2.Pos + poly2.Vertices[j], axis.axis);
                    if (dot > axis.b2)
                    {
                        axis.b2 = dot;
                        endVerticeCol = (!poly2.LastEdgeClosed && j == poly2.Vertices.Count - 1) ? poly2.Vertices.Count - 1 : -1;
                    }
                }

                float dotDir = Vector2.Dot(dir, axis.axis);
                float dist = (axis.b2 - axis.a1) / dotDir;

                if (dotDir < 0)
                {
                    if (!cr.Distance.HasValue || dist > cr.Distance)
                    {
                        cr.Distance = dist;
                        cr.AxisCol = axis.axis;

                        if (!poly1.LastEdgeClosed && i == edges1.Count - 1)
                            open = true;
                        else
                        {
                            open = false;
                            if (endVerticeCol == 0)
                            {
                                if (!poly2.StartCorner || Vector2.Dot(new Vector2(-edges2[0].Y, edges2[0].X), cr.AxisCol) > 0)
                                    open = true;
                            }
                            else if (endVerticeCol != -1)
                            {
                                if (!poly2.EndCorner || Vector2.Dot(new Vector2(-edges2[endVerticeCol - 1].Y, edges2[endVerticeCol - 1].X), cr.AxisCol) > 0)
                                    open = true;
                            }
                        }
                    }
                }
                else if (dotDir > 0)
                {
                    if (!cr.DistanceReversed.HasValue || dist < cr.DistanceReversed)
                    {
                        cr.DistanceReversed = dist;
                        cr.AxisColReversed = axis.axis;

                        if (!poly1.LastEdgeClosed && i == edges1.Count - 1)
                            openReversed = true;
                        else
                        {
                            openReversed = false;
                            if (endVerticeCol == 0)
                            {
                                if (!poly2.StartCorner || Vector2.Dot(new Vector2(-edges2[0].Y, edges2[0].X), cr.AxisColReversed) > 0)
                                    openReversed = true;
                            }
                            else if (endVerticeCol != -1)
                            {
                                if (!poly2.EndCorner || Vector2.Dot(new Vector2(-edges2[endVerticeCol - 1].Y, edges2[endVerticeCol - 1].X), cr.AxisColReversed) > 0)
                                    openReversed = true;
                            }
                        }
                    }
                }
                else if (axis.b2 - axis.a1 < 0)
                    return new CollisionResult();

            }
            for (int i = 0; i < edges2.Count; i++)
            {
                int endVerticeCol = -1; //if the collision happens to be on an open vertice save the index of this vertice here
                Axis axis = new Axis(edges2[i]);
                axis.a1 = Vector2.Dot(poly2.Pos + poly2.Vertices[i], axis.axis);

                axis.b2 = Vector2.Dot(poly1.Pos + poly1.Vertices[0], axis.axis);
                endVerticeCol = !poly1.LastEdgeClosed ? 0 : -1;

                for (int j = 1; j < poly1.Vertices.Count; j++)
                {
                    float dot = Vector2.Dot(poly1.Pos + poly1.Vertices[j], axis.axis);
                    if (dot > axis.b2)
                    {
                        axis.b2 = dot;
                        endVerticeCol = (!poly1.LastEdgeClosed && j == poly1.Vertices.Count - 1) ? poly1.Vertices.Count - 1 : -1;
                    }
                }

                float dotDir = -Vector2.Dot(dir, axis.axis);
                float dist = (axis.b2 - axis.a1) / dotDir;

                if (dotDir < 0)
                {
                    if (!cr.Distance.HasValue || dist > cr.Distance)
                    {
                        cr.Distance = dist;
                        cr.AxisCol = -axis.axis;

                        if (!poly2.LastEdgeClosed && i == edges2.Count - 1)
                            open = true;
                        else
                        {
                            open = false;
                            if (endVerticeCol == 0)
                            {
                                if (!poly1.StartCorner || Vector2.Dot(new Vector2(-edges1[0].Y, edges1[0].X), cr.AxisCol) < 0)
                                    open = true;
                            }
                            else if (endVerticeCol != -1)
                            {
                                if (!poly1.EndCorner || Vector2.Dot(new Vector2(-edges1[endVerticeCol - 1].Y, edges1[endVerticeCol - 1].X), cr.AxisCol) < 0)
                                    open = true;
                            }
                        }
                    }
                }
                else if (dotDir > 0)
                {
                    if (!cr.DistanceReversed.HasValue || dist < cr.DistanceReversed)
                    {
                        cr.DistanceReversed = dist;
                        cr.AxisColReversed = -axis.axis;

                        if (!poly2.LastEdgeClosed && i == edges2.Count - 1)
                            openReversed = true;
                        else
                        {
                            openReversed = false;
                            if (endVerticeCol == 0)
                            {
                                if (!poly1.StartCorner || Vector2.Dot(new Vector2(-edges1[0].Y, edges1[0].X), cr.AxisColReversed) < 0)
                                    openReversed = true;
                            }
                            else if (endVerticeCol != -1)
                            {
                                if (!poly1.EndCorner || Vector2.Dot(new Vector2(-edges1[endVerticeCol - 1].Y, edges1[endVerticeCol - 1].X), cr.AxisColReversed) < 0)
                                    openReversed = true;
                            }
                        }
                    }
                }
                else if (axis.b2 - axis.a1 < 0)
                    return new CollisionResult();
            }

            if (cr.Distance.HasValue && cr.DistanceReversed <= cr.Distance)
                return new CollisionResult();

            if (open)
            {
                cr.Distance = null;
                cr.AxisCol = Vector2.Zero;
            }
            if (openReversed)
            {
                cr.DistanceReversed = null;
                cr.AxisColReversed = Vector2.Zero;
            }

            return cr;
        }

        public static CollisionResult DistPolygonPolygon2(Polygon poly1, Polygon poly2, Vector2 dir)
        {
            CollisionResult cr = new CollisionResult();

            if (dir == Vector2.Zero)
                return cr;

            List<Vector2> edges1 = poly1.GetEdges();
            List<Vector2> edges2 = poly2.GetEdges();
            List<Vector2> axes = new List<Vector2>();
            axes.AddRange(GetAxes(edges1));
            axes.AddRange(GetAxes(edges2));

            float[][] dirDists = new float[axes.Count][];

            for (int i = 0; i < axes.Count; i++)
            {
                float[] projection1 = GetProjection(axes[i], poly1.Pos, poly1.Vertices, edges1);
                float[] projection2 = GetProjection(axes[i], poly2.Pos, poly2.Vertices, edges2);

                float dotDir = dir.X * axes[i].X + dir.Y * axes[i].Y;

                if (dotDir == 0f)
                {
                    //no move direction on this axis, if no collision on this axis, then there is no distance on this direction
                    if (projection1[1] <= projection2[0] || projection1[0] >= projection2[1])
                        return new CollisionResult();
                }
                else
                {
                    float dirDist1 = (projection2[1] - projection1[0] * Math.Sign(dotDir)) / dotDir;
                    float dirDist2 = (projection2[0] - projection1[1] * Math.Sign(dotDir)) / dotDir;

                    if (dirDist1 < dirDist2)
                        dirDists[i] = (new float[] { dirDist1, (projection2[0] - projection1[1] * Math.Sign(dotDir)) / dotDir });
                    else
                        dirDists[i] = (new float[] { dirDist2, (projection2[1] - projection1[0] * Math.Sign(dotDir)) / dotDir });


                    if (!cr.Distance.HasValue || dirDists[i][0] > cr.Distance)
                    {
                        cr.AxisCol = axes[i];
                        cr.Distance = dirDists[i][0];
                    }

                    if (!cr.DistanceReversed.HasValue || dirDists[i][1] < cr.DistanceReversed)
                    {
                        cr.AxisColReversed = axes[i];
                        cr.DistanceReversed = dirDists[i][1];
                    }
                }
            }

            //if (cr.Distance.HasValue)
            {
                for (int i = 0; i < dirDists.Length; i++)
                {
                    if (dirDists[i] != null && dirDists[i][1] <= cr.Distance)
                        return new CollisionResult();
                }

                if (Vector2.Dot(dir, cr.AxisCol) > 0)
                    cr.AxisCol = -cr.AxisCol;
                if (Vector2.Dot(dir, cr.AxisColReversed) < 0)
                    cr.AxisColReversed = -cr.AxisColReversed;
            }

            return cr;
        }


        public static bool ColPolygonCircle(Polygon polygon, Circle circle)
        {
            if (polygon.Vertices.Count == 0)
                return false;

            List<Vector2> edges1 = polygon.GetEdges();
            List<Vector2> axes = new List<Vector2>();
            axes.AddRange(GetAxes(edges1));

            if (axes.Count == 0)
            {
                if (polygon.Vertices.Count == 1)
                    return ColVectorCircle(polygon.Pos + polygon.Vertices[0], circle);
                return false;
            }

            //add axis between circle and nearest vertice
            int nearestI = -1;
            float nearestDist = 0;
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                float cDist = (float)(Math.Pow(circle.X - polygon.X - polygon.Vertices[i].X, 2) + Math.Pow(circle.Y - polygon.Y - polygon.Vertices[i].Y, 2));
                if (i == 0 || cDist < nearestDist)
                {
                    nearestDist = cDist;
                    nearestI = i;
                }
            }

            Vector2 cornerAxis = circle.Pos - polygon.Pos - polygon.Vertices[nearestI];
            if (cornerAxis != Vector2.Zero)
                axes.Add(Vector2.Normalize(cornerAxis).XPositive());


            for (int i = 0; i < axes.Count; i++)
            {
                float[] projection1 = GetProjection(axes[i], polygon.Pos, polygon.Vertices, edges1);
                float circle_axis = Vector2.Dot(axes[i], circle.Pos);

                if (projection1[0] > circle_axis + circle.Radius || projection1[1] < circle_axis - circle.Radius)
                    return false;
            }
            return true;
        }

        public static CollisionResult DistPolygonCircle(Polygon polygon, Circle circle)
        {
            CollisionResult cr = new CollisionResult();

            List<Vector2> edges1 = polygon.GetEdges();
            List<Vector2> axes = new List<Vector2>();
            axes.AddRange(GetAxes(edges1));


            if (axes.Count == 0)
                return cr;

            //add axis between circle and nearest vertice
            int nearestI = -1;
            float nearestDist = 0;
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                float cDist = (float)(Math.Pow(circle.X - polygon.X - polygon.Vertices[i].X, 2) + Math.Pow(circle.Y - polygon.Y - polygon.Vertices[i].Y, 2));
                if (i == 0 || cDist < nearestDist)
                {
                    nearestDist = cDist;
                    nearestI = i;
                }
            }

            Vector2 cornerAxis = circle.Pos - polygon.Pos - polygon.Vertices[nearestI];
            if (cornerAxis != Vector2.Zero)
                axes.Add(Vector2.Normalize(cornerAxis).XPositive());


            for (int i = 0; i < axes.Count; i++)
            {
                float[] projection1 = GetProjection(axes[i], polygon.Pos, polygon.Vertices, edges1);
                float circle_axis = Vector2.Dot(axes[i], circle.Pos);
                float[] projection2 = new float[] { circle_axis - circle.Radius, circle_axis + circle.Radius };

                float dirDist1 = projection2[1] - projection1[0];
                float dirDist2 = projection2[0] - projection1[1];

                if (dirDist1 < dirDist2)
                    dirDist2 = projection2[0] - projection1[1];
                else
                {
                    dirDist1 = dirDist2;
                    dirDist2 = projection2[1] - projection1[0];
                }

                if (Math.Sign(dirDist1) == Math.Sign(dirDist2))
                    return new CollisionResult();

                if (!cr.Distance.HasValue || Math.Abs(dirDist1) < Math.Abs(cr.Distance.Value))
                {
                    cr.AxisCol = -axes[i];
                    cr.Distance = -dirDist1;
                }
                if (Math.Abs(dirDist2) < Math.Abs(cr.Distance.Value))
                {
                    cr.AxisCol = axes[i];
                    cr.Distance = dirDist2;
                }
            }

            return cr;
        }

        public static CollisionResult DistPolygonCircle(Polygon polygon, Circle circle, Vector2 dir)
        {
            dir = -dir; //- for polygon, circle order change

            CollisionResult cr = new CollisionResult();

            if (dir == Vector2.Zero)
                return cr;

            if (polygon.Vertices.Count == 0)
                return cr;

            float? cDist = null;

            //find start on edge
            List<Vector2> edges2 = polygon.GetEdges();

            //Init for [0]
            Vector2 e = Vector2.Normalize(edges2[0]);
            Vector2 p = circle.Pos - polygon.Pos - polygon.Vertices[0];

            Vector2 pDist;
            float pDist_e;

            int i = 0;
            bool noEdge = false;

            float radius = circle.Radius;

            while (true)
            {
                //edge check
                if (!noEdge)
                {
                    Vector2 ne = new Vector2(e.Y, -e.X); //rotate counter-clockwise for the normal pointing out of the polygon
                    float p_ne = Vector2.Dot(p, ne);

                    float dir_ne = Vector2.Dot(dir, ne);

                    if (dir_ne != 0)
                    {
                        bool forward = dir_ne < 0;
                        Vector2 cDir = dir;
                        if (!forward)
                            cDir *= -1f;

                        //if (dir_ne < 0) //dir against ne? (pointing to the top side of the edge)
                        //{
                        //if (forward)
                        cDist = (radius - ne.X * p.X - ne.Y * p.Y) / (ne.X * cDir.X + ne.Y * cDir.Y);
                        //else
                        //    cDist = (-circle.Radius - ne.X * p.X - ne.Y * p.Y) / (-ne.X * cDir.X - ne.Y * cDir.Y);

                        pDist = p + cDir * (float)cDist;
                        pDist_e = Vector2.Dot(pDist, e);

                        if (pDist_e >= 0 && pDist_e <= edges2[i].Length())
                        {
                            if (forward)
                            {
                                //cDist -= minDist / cDir.Length();
                            }
                            if (!forward)
                            {
                                cDist *= -1f;
                                //cDist += minDist / cDir.Length();
                            }

                            if (forward)
                            {
                                if (!cr.Distance.HasValue)
                                {
                                    cr.AxisCol = -ne; //- for polygon, circle order change
                                    cr.Distance = cDist;
                                }
                            }
                            else if (!cr.DistanceReversed.HasValue)
                            {
                                cr.DistanceReversed = cDist;
                                cr.AxisColReversed = -ne;
                            }

                            if (cr.Distance.HasValue && cr.DistanceReversed.HasValue)
                                return cr;
                        }
                        //}
                    }
                }

                //next corner check
                noEdge = false;
                int j = i + 1;
                if (j == polygon.Vertices.Count)
                    j = 0;
                else if (j == edges2.Count)
                {
                    if (!polygon.LastEdgeClosed)//???????is this necessary?
                        noEdge = true;
                    //else
                    //    j = 0;
                }
                //cDist = GetDistCircle(circle.X, circle.Y, polygon.vertices[i].X + polygon.X, polygon.vertices[i].Y + polygon.Y, dir.X, dir.Y, circle.Radius);

                p = circle.Pos - polygon.Pos - polygon.Vertices[j];

                Vector2 oldE = e;
                if (!noEdge)
                    e = Vector2.Normalize(edges2[j]);
                else
                    e = new Vector2(-oldE.Y, oldE.X);//Vector2.Zero;

                if ((polygon.EndCorner || j != polygon.Vertices.Count - 1) && (polygon.StartCorner || j != 0))
                {
                    float?[] cDists = ABCFormula(dir.X * dir.X + dir.Y * dir.Y, 2 * (p.X * dir.X + p.Y * dir.Y), p.X * p.X + p.Y * p.Y - radius * radius);

                    for (int k = 0; k < cDists.Length; k++)
                    {
                        //Vector2 cDir = dir;
                        //if (k == 0)
                        //    cDir *= -1f;

                        if (cDists[k].HasValue)
                        {
                            //check if the distancePoint is not in the prev voroni region
                            pDist = circle.Pos + dir * (float)cDists[k] - polygon.Pos - polygon.Vertices[j];
                            if (j == 0 && !polygon.LastEdgeClosed)
                                pDist_e = Vector2.Dot(pDist, new Vector2(e.Y, -e.X));
                            else
                                pDist_e = Vector2.Dot(pDist, oldE);

                            if (pDist_e > 0) //is pos right (clockwise) from the previous edge?
                            {

                                pDist = p + dir * (float)cDists[k];
                                pDist_e = Vector2.Dot(pDist, e);

                                //if (pDist_e <= 0) //TODO: check if replaced one works -v
                                if (pDist_e < 0) //is pos left (counter-clockwise) from the next edge?
                                {

                                    if (k == 0)
                                    {
                                        //cDists[k] += minDist / dir.Length();
                                        if (!cr.DistanceReversed.HasValue)
                                        {
                                            cr.DistanceReversed = cDists[k];
                                            cr.AxisColReversed = Vector2.Normalize(polygon.Pos + polygon.Vertices[j] - cr.DistanceReversed.Value * dir - circle.Pos);
                                        }
                                    }
                                    else if (!cr.Distance.HasValue)
                                    {
                                        //cDists[k] -= minDist / dir.Length();
                                        cr.Distance = cDists[k];
                                        cr.AxisCol = Vector2.Normalize(polygon.Pos + polygon.Vertices[j] - cr.Distance.Value * dir - circle.Pos);
                                    }

                                    if (cr.Distance.HasValue && cr.DistanceReversed.HasValue)
                                        return cr;
                                }
                            }
                        }
                    }
                }

                i = j;

                if (i == 0)
                    break;
            }
            return cr;
        }

        public static CollisionResultPolygonExtended DistPolygonCircleExtended(Polygon polygon, Circle circle, Vector2 dir)
        {
            dir = -dir; //- for polygon, circle order change

            CollisionResultPolygonExtended cr = new CollisionResultPolygonExtended();

            if (dir == Vector2.Zero)
                return cr;

            if (polygon.Vertices.Count <= 1) // TODO: check point vs circle, when polygon has 1 vertex?
                return cr;

            float? cDist = null;

            //find start on edge
            List<Vector2> edges2 = polygon.GetEdges();

            //Init for [0]
            Vector2 e = Vector2.Normalize(edges2[0]);
            Vector2 p = circle.Pos - polygon.Pos - polygon.Vertices[0];

            Vector2 pDist;
            float pDist_e;

            int i = 0;
            bool noEdge = false;

            float radius = circle.Radius;

            while (true)
            {
                //edge check
                if (!noEdge)
                {
                    Vector2 ne = new Vector2(e.Y, -e.X); //rotate counter-clockwise for the normal pointing out of the polygon
                    float p_ne = Vector2.Dot(p, ne);

                    float dir_ne = Vector2.Dot(dir, ne);

                    if (dir_ne != 0)
                    {
                        bool forward = dir_ne < 0;
                        Vector2 cDir = dir;
                        if (!forward)
                            cDir *= -1f;

                        //if (dir_ne < 0) //dir against ne? (pointing to the top side of the edge)
                        //{
                        //if (forward)
                        cDist = (radius - ne.X * p.X - ne.Y * p.Y) / (ne.X * cDir.X + ne.Y * cDir.Y);
                        //else
                        //    cDist = (-circle.Radius - ne.X * p.X - ne.Y * p.Y) / (-ne.X * cDir.X - ne.Y * cDir.Y);

                        pDist = p + cDir * (float)cDist;
                        pDist_e = Vector2.Dot(pDist, e);

                        if (pDist_e >= 0 && pDist_e <= edges2[i].Length())
                        {
                            if (forward)
                            {
                                //cDist -= minDist / cDir.Length();
                            }
                            if (!forward)
                            {
                                cDist *= -1f;
                                //cDist += minDist / cDir.Length();
                            }

                            if (forward)
                            {
                                if (!cr.Distance.HasValue)
                                {
                                    cr.AxisCol = -ne; //- for polygon, circle order change
                                    cr.Distance = cDist;
                                    cr.ColVertexIndex = i + pDist_e / edges2[i].Length();
                                }
                            }
                            else if (!cr.DistanceReversed.HasValue)
                            {
                                cr.DistanceReversed = cDist;
                                cr.AxisColReversed = -ne;
                            }

                            if (cr.Distance.HasValue && cr.DistanceReversed.HasValue)
                                return cr;
                        }
                        //}
                    }
                }

                //next corner check
                noEdge = false;
                int j = i + 1;
                if (j == polygon.Vertices.Count)
                    j = 0;
                else if (j == edges2.Count)
                {
                    if (!polygon.LastEdgeClosed)//???????is this necessary?
                        noEdge = true;
                    //else
                    //    j = 0;
                }
                //cDist = GetDistCircle(circle.X, circle.Y, polygon.vertices[i].X + polygon.X, polygon.vertices[i].Y + polygon.Y, dir.X, dir.Y, circle.Radius);

                p = circle.Pos - polygon.Pos - polygon.Vertices[j];

                Vector2 oldE = e;
                if (!noEdge)
                    e = Vector2.Normalize(edges2[j]);
                else
                    e = new Vector2(-oldE.Y, oldE.X);//Vector2.Zero;

                if ((polygon.EndCorner || j != polygon.Vertices.Count - 1) && (polygon.StartCorner || j != 0))
                {
                    float?[] cDists = ABCFormula(dir.X * dir.X + dir.Y * dir.Y, 2 * (p.X * dir.X + p.Y * dir.Y), p.X * p.X + p.Y * p.Y - radius * radius);

                    for (int k = 0; k < cDists.Length; k++)
                    {
                        //Vector2 cDir = dir;
                        //if (k == 0)
                        //    cDir *= -1f;

                        if (cDists[k].HasValue)
                        {
                            //check if the distancePoint is not in the prev voroni region
                            pDist = circle.Pos + dir * (float)cDists[k] - polygon.Pos - polygon.Vertices[j];
                            if (j == 0 && !polygon.LastEdgeClosed)
                                pDist_e = Vector2.Dot(pDist, new Vector2(e.Y, -e.X));
                            else
                                pDist_e = Vector2.Dot(pDist, oldE);

                            if (pDist_e > 0) //is pos right (clockwise) from the previous edge?
                            {

                                pDist = p + dir * (float)cDists[k];
                                pDist_e = Vector2.Dot(pDist, e);

                                //if (pDist_e <= 0) //TODO: check if replaced one works -v
                                if (pDist_e < 0) //is pos left (counter-clockwise) from the next edge?
                                {

                                    if (k == 0)
                                    {
                                        //cDists[k] += minDist / dir.Length();
                                        if (!cr.DistanceReversed.HasValue)
                                        {
                                            cr.DistanceReversed = cDists[k];
                                            cr.AxisColReversed = Vector2.Normalize(polygon.Pos + polygon.Vertices[j] - cr.DistanceReversed.Value * dir - circle.Pos);
                                        }
                                    }
                                    else if (!cr.Distance.HasValue)
                                    {
                                        //cDists[k] -= minDist / dir.Length();
                                        cr.Distance = cDists[k];
                                        cr.AxisCol = Vector2.Normalize(polygon.Pos + polygon.Vertices[j] - cr.Distance.Value * dir - circle.Pos);
                                        cr.ColVertexIndex = j;
                                    }

                                    if (cr.Distance.HasValue && cr.DistanceReversed.HasValue)
                                        return cr;
                                }
                            }
                        }
                    }
                }

                i = j;

                if (i == 0)
                    break;
            }
            return cr;
        }


        public static bool ColPolygonTextureShape(Polygon polygon, TextureShape sprite)
        {
            throw new NotImplementedException();
        }

        #region old


        //public static CollisionResult DistPolygonPolygonOpened(Polygon poly1, Polygon poly2, Vector2 dir)
        //{
        //    bool open = false, openReversed = false;

        //    CollisionResult cr = new CollisionResult();

        //    if (dir == Vector2.Zero)
        //    {
        //        cr.Collision = ColPolygonPolygon(poly1, poly2);
        //        return cr;
        //    }

        //    List<Vector2> edges1 = poly1.GetClosedEdges();
        //    List<Vector2> edges2 = poly2.GetClosedEdges();

        //    if (!Jau1(ref cr, dir, edges1, edges2, poly1, poly2, ref open, ref openReversed, 1))
        //        return new CollisionResult();
        //    if (!Jau1(ref cr, -dir, edges2, edges1, poly2, poly1, ref open, ref openReversed, -1))
        //        return new CollisionResult();

        //    if (cr.Distance.HasValue && cr.DistanceReversed <= cr.Distance)
        //        return new CollisionResult();


        //    cr.Distance -= minDist / Math.Abs(Vector2.Dot(cr.AxisCol, dir));
        //    cr.DistanceReversed += minDist / Math.Abs(Vector2.Dot(cr.AxisColReversed, dir));

        //    if (open)
        //    {
        //        cr.Distance = null;
        //        cr.AxisCol = Vector2.Zero;
        //    }
        //    if (openReversed)
        //    {
        //        cr.DistanceReversed = null;
        //        cr.AxisColReversed = Vector2.Zero;
        //    }


        //    if (cr.Distance.HasValue && cr.DistanceReversed.HasValue)
        //        cr.Collision = cr.Distance < 0 && cr.DistanceReversed > 0;

        //    return cr;
        //}

        //static bool Jau1(ref CollisionResult cr, Vector2 dir, List<Vector2> edges1, List<Vector2> edges2, Polygon poly1, Polygon poly2, ref bool open, ref bool openReversed, int reversed)
        //{
        //    for (int i = 0; i < edges1.Count; i++)
        //    {
        //        int endVerticeCol = -1; //if the collision happens to be on an open vertice save the index of this vertice here
        //        Axis axis = new Axis(edges1[i]);
        //        axis.a1 = Vector2.Dot(poly1.Pos + poly1.vertices[i], axis.axis);

        //        axis.b2 = Vector2.Dot(poly2.Pos + poly2.vertices[0], axis.axis);
        //        endVerticeCol = !poly2.closed ? 0 : -1;

        //        for (int j = 1; j < poly2.vertices.Count; j++)
        //        {
        //            float dot = Vector2.Dot(poly2.Pos + poly2.vertices[j], axis.axis);
        //            if (dot > axis.b2)
        //            {
        //                axis.b2 = dot;
        //                endVerticeCol = (!poly2.closed && j == poly2.vertices.Count - 1) ? poly2.vertices.Count - 1 : -1;
        //            }
        //        }

        //        float dotDir = Vector2.Dot(dir, axis.axis);
        //        float dist = (axis.b2 - axis.a1) / dotDir;

        //        if (dotDir < 0)
        //        {
        //            if (!cr.Distance.HasValue || dist > cr.Distance)
        //                Jau2(ref cr.Distance, ref cr.AxisCol, axis, reversed, poly1, poly2, i == edges1.Count - 1, edges2, ref open, dist, endVerticeCol);
        //        }
        //        else if (dotDir > 0)
        //        {
        //            if (!cr.DistanceReversed.HasValue || dist < cr.DistanceReversed)
        //                Jau2(ref cr.DistanceReversed, ref cr.AxisColReversed, axis, reversed, poly1, poly2, i == edges1.Count - 1, edges2, ref openReversed, dist, endVerticeCol);
        //        }
        //        else if (axis.b2 - axis.a1 < 0)
        //            return false;

        //    }

        //    return true;
        //}

        //static void Jau2(ref float? distance, ref Vector2 axisCol, Axis axis, int reversed, Polygon poly1, Polygon poly2, bool lastIndex, List<Vector2> edges2, ref bool open, float dist, int endVerticeCol)
        //{
        //    distance = dist;
        //    axisCol = axis.axis * reversed;

        //    if (!poly1.closed && lastIndex)
        //        open = true;
        //    else
        //    {
        //        open = false;
        //        if (endVerticeCol == 0)
        //        {
        //            if (!poly2.startCorner || Vector2.Dot(new Vector2(-edges2[0].Y, edges2[0].X), axis.axis) > 0)
        //                open = true;
        //        }
        //        else if (endVerticeCol != -1)
        //        {
        //            if (!poly2.endCorner || Vector2.Dot(new Vector2(-edges2[endVerticeCol].Y, edges2[endVerticeCol].X), axis.axis) < 0)
        //                open = true;
        //        }
        //    }
        //}


        #endregion


        #endregion

        #region Circle

        public static bool ColCircleCircle(Circle circle1, Circle circle2)
        {
            float radius = circle1.Radius + circle2.Radius;
            Vector2 dist = circle2.Pos - circle1.Pos;
            return (dist.X * dist.X + dist.Y * dist.Y < radius * radius);
        }

        public static CollisionResult DistCircleCircle(Circle circle1, Circle circle2)
        {
            Vector2 dist = circle2.Pos - circle1.Pos;
            float distLength = (float)Math.Sqrt(dist.X * dist.X + dist.Y * dist.Y);

            if (distLength >= circle1.Radius + circle2.Radius)
                return new CollisionResult();
            else
            {
                CollisionResult cr = new CollisionResult();
                cr.Distance = circle1.Radius + circle2.Radius - distLength;
                cr.AxisCol = -Vector2.Normalize(dist);
                return cr;
            }
        }

        public static CollisionResult DistCircleCircle(Circle circle1, Circle circle2, Vector2 dir)
        {
            CollisionResult cr = new CollisionResult();

            float radius = circle1.Radius + circle2.Radius;

            Vector2 dist = circle2.Pos - circle1.Pos;

            float?[] results = ABCFormula(dir.X * dir.X + dir.Y * dir.Y
                                         , -2 * (dist.X * dir.X + dist.Y * dir.Y)
                                         , dist.X * dist.X + dist.Y * dist.Y - radius * radius);

            if (results[0] != null)
            {
                if (!float.IsNaN(results[0].Value) && !float.IsNaN(results[1].Value))
                {
                    cr.Distance = results[1].Value;
                    cr.DistanceReversed = results[0].Value;

                    Vector2 colPos = circle1.Pos + dir * cr.Distance.Value;
                    cr.AxisCol = Vector2.Normalize(colPos - circle2.Pos);

                    colPos = circle1.Pos + dir * cr.DistanceReversed.Value;
                    cr.AxisColReversed = Vector2.Normalize(colPos - circle2.Pos);
                }
                else
                {
                    // ONTROUBLESHOOT: check if this isn't causing any bugs
                }
            }

            return cr;
        }

        public static bool ColCircleTextureShape(Circle circle, TextureShape sprite)
        {
            Rect rect = sprite.GetBoundingRect();

            if (ColRectangleCircle(rect, circle))
            {
                Matrix transform1 = sprite.GetMatrix();

                Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transform1);
                Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transform1);

                Vector2 yPosIn2 = Vector2.Transform(Vector2.Zero, transform1);

                float radiusSquared = MathF.Pow(circle.Radius, 2);

                for (int y1 = 0; y1 < sprite.Size.Y; y1++)
                {
                    Vector2 posIn2 = yPosIn2;

                    for (int x1 = 0; x1 < sprite.Size.X; x1++)
                    {
                        float xB = posIn2.X - circle.X;
                        float yB = posIn2.Y - circle.Y;

                        //calc dist from nearest pixel corner (get the nearest)
                        float minDist = (float)(Math.Pow(xB, 2) + Math.Pow(yB, 2));
                        float newDist = (float)(Math.Pow(xB + stepX.X, 2) + Math.Pow(yB + stepX.Y, 2));
                        if (newDist < minDist)
                        {
                            minDist = newDist;
                            newDist = (float)(Math.Pow(xB + stepX.X + stepY.X, 2) + Math.Pow(yB + stepX.Y + stepY.Y, 2));
                        }
                        else
                        {
                            newDist = (float)(Math.Pow(xB + stepY.X, 2) + Math.Pow(yB + stepY.Y, 2));
                            if (newDist < minDist)
                            {
                                minDist = newDist;
                                newDist = (float)(Math.Pow(xB + stepX.X + stepY.X, 2) + Math.Pow(yB + stepX.Y + stepY.Y, 2));
                            }
                        }

                        if (newDist < minDist)
                            minDist = newDist;

                        if (minDist < radiusSquared)
                        {
                            Color color1 = sprite.colorData[x1 + y1 * sprite.Size.X];

                            if (color1.A != 0)
                            {
                                return true;
                            }
                        }

                        posIn2 += stepX;
                    }

                    yPosIn2 += stepY;
                }
            }

            return false;
        }

        #endregion

        #region TextureShape

        public static bool ColTextureShapeTextureShape(TextureShape sprite1, TextureShape sprite2)
        {
            if (sprite1.IsTransformed() || sprite2.IsTransformed())
                return ColTextureShapeTextureShapeTransformed(sprite1, sprite2);
            else
                return ColTextureShapeTextureShapeIdentity(sprite1, sprite2);
        }

        public static bool ColTextureShapeTextureShapeIdentity(TextureShape sprite1, TextureShape sprite2)
        {
            Rect rect1 = sprite1.GetBoundingRect();
            Rect rect2 = sprite2.GetBoundingRect();
            if (ColRectangleRectangle(rect1, rect2))
            {
                int xdiff = (int)(rect2.X - rect1.X);
                int ydiff = (int)(rect2.Y - rect1.Y);

                int left = Math.Max(0, (int)rect2.X - (int)rect1.X);
                int right = Math.Min((int)rect1.Size.X, (int)(rect2.X + rect2.Size.X - rect1.X));
                int top = Math.Max(0, (int)rect2.Y - (int)rect1.Y);
                int bottom = Math.Min((int)rect1.Size.Y, (int)(rect2.Y + rect2.Size.Y - rect1.Y));
                for (int y = Math.Max(top, ydiff); y < bottom; y++)
                {
                    for (int x = Math.Max(left, xdiff); x < right; x++)
                    {
                        Color color1 = sprite1.colorData[y * sprite1.Size.X + x];

                        if (color1.A != 0)
                        {
                            Color color2 = sprite2.colorData[(y - ydiff) * sprite2.Size.X + (x - xdiff)];
                            if (color2.A != 0)
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool ColTextureShapeTextureShapeTransformed(TextureShape sprite1, TextureShape sprite2)
        {
            Rect rect1 = sprite1.GetBoundingRect();
            Rect rect2 = sprite2.GetBoundingRect();

            if (ColRectangleRectangle(rect1, rect2))
            {
                Matrix transform1 = sprite1.GetMatrix();
                Matrix transform2 = sprite2.GetMatrix();

                Matrix transform1To2 = transform1 * Matrix.Invert(transform2);

                Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transform1To2);
                Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transform1To2);

                Vector2 yPosIn2 = Vector2.Transform(Vector2.Zero, transform1To2) + (stepX + stepY) * 0.5f;

                for (int y1 = 0; y1 < sprite1.Size.Y; y1++)
                {
                    Vector2 posIn2 = yPosIn2;

                    for (int x1 = 0; x1 < sprite1.Size.X; x1++)
                    {
                        int xB = (int)(posIn2.X);
                        int yB = (int)(posIn2.Y);

                        if (xB >= 0 && xB < sprite2.Size.X &&
                            yB >= 0 && yB < sprite2.Size.Y)
                        {
                            Color color1 = sprite1.colorData[x1 + y1 * sprite1.Size.X];
                            Color color2 = sprite2.colorData[xB + yB * sprite2.Size.X];

                            if (color1.A != 0 && color2.A != 0)
                            {
                                return true;
                            }
                        }

                        posIn2 += stepX;
                    }

                    yPosIn2 += stepY;
                }
            }

            return false;
        }


        #endregion

        public static List<Vector2> GetAxes(IList<Vector2> edges)
        {
            List<Vector2> axes = new List<Vector2>();

            for (int i = 0; i < edges.Count; i++)
            {
                Vector2 axis = Vector2.Normalize(new Vector2(-edges[i].Y, edges[i].X));
                if (axis.X < 0)
                    axis *= -1f;
                else if (axis.X == 0 && axis.Y < 0)
                    axis.Y *= -1f;

                if (!axes.Contains(axis))
                    axes.Add(axis);
            }

            //if only one axis is available (line) make it two dimensional (+normal axis)
            if (axes.Count <= 2 && axes.Count > 0)
            {
                Vector2 axis = new Vector2(-axes[0].Y, axes[0].X);
                if (axis.X < 0)
                    axis *= -1f;
                else if (axis.X == 0 && axis.Y < 0)
                    axis.Y *= -1f;
                axes.Add(axis);
            }

            return axes;
        }

        public static float[] GetProjection(Vector2 axis, Vector2 pos, IList<Vector2> vertices, IList<Vector2> edges)
        {
            float[] projection = new float[2];
            bool hasValue = false;

            if (edges.Count == 0)
            {
                float dotPos = ((vertices[0].X + pos.X) * axis.X + (vertices[0].Y + pos.Y) * axis.Y);

                projection[0] = projection[1] = dotPos;

                return new float[0];
            }
            else
            {
                for (int j = 0; j < edges.Count; j++)
                {
                    float dotLength = (edges[j].X * axis.X + edges[j].Y * axis.Y);


                    float dotPos = ((vertices[j].X + pos.X) * axis.X + (vertices[j].Y + pos.Y) * axis.Y);

                    float min, max;
                    if (dotLength >= 0)
                    {
                        min = dotPos;
                        max = dotPos + dotLength;
                    }
                    else
                    {
                        min = dotPos + dotLength;
                        max = dotPos;
                    }

                    if (!hasValue)
                    {
                        projection[0] = min;
                        projection[1] = max;
                        hasValue = true;
                    }
                    else
                    {
                        if (min < projection[0])
                            projection[0] = min;
                        if (max > projection[1])
                            projection[1] = max;
                    }
                }
            }

            return projection;
        }

        public static float?[] ABCFormula(float a, float b, float c)
        {
            float discriminant = b * b - 4f * a * c;
            if (discriminant < 0)
                return new float?[] { null, null };
            else
            {
                float sqrt = MathF.Sqrt(discriminant);
                return new float?[] { (-b + sqrt) / (2f * a), (-b - sqrt) / (2f * a) };
            }
        }

        class Axis
        {
            public Vector2 axis;
            public float a1, b2; //a1 <> b2   a2 <> b1

            public Axis(Vector2 edge)
            {
                axis = Vector2.Normalize(new Vector2(-edge.Y, edge.X));
            }
        }

        public static float GetCollisionOf2Axes(Vector2 pos1, Vector2 dir1, Vector2 pos2, Vector2 dir2)
        {
            return (-pos1.Y * dir2.X + pos1.X * dir2.Y + pos2.Y * dir2.X - pos2.X * dir2.Y) / (dir2.X * dir1.Y - dir2.Y * dir1.X);
        }
    }
}

#pragma warning restore CS8629 // Nullable value type may be null.