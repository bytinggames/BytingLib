#pragma warning disable CS8629 // Nullable value type may be null.


namespace BytingLib
{
    public static class Collision3
    {
        static readonly Type TShapeCollection3 = typeof(IShape3Collection);
        static readonly Type TVector3 = typeof(Vector3);
        static readonly Type TSphere3 = typeof(Sphere3);
        static readonly Type TAxis3 = typeof(Axis3);
        static readonly Type TRay3 = typeof(Ray3);
        static readonly Type TLine3 = typeof(Line3);
        static readonly Type TPlane3 = typeof(Plane3);
        static readonly Type TTriangle3 = typeof(Triangle3);
        static readonly Type TAxisRadius3 = typeof(AxisRadius3);
        static readonly Type TCapsule3 = typeof(Capsule3);
        static readonly Type TAABB3 = typeof(AABB3);
        static readonly Type TBox3 = typeof(Box3);
        static readonly Type TCylinder3 = typeof(Cylinder3);

        static readonly Type TPoint3 = typeof(Point3); // uses the same functions Vector3 can use

        static Collision3()
        {
            // duplicate each collision function and reverse the parameters
            // in case the shapes are passed the other way around (a,b) -> (b,a)
            List<(Type, Type)> keys = collisionFunctions.Keys.ToList();
            foreach (var key in keys)
            {
                if (key.Item1 != key.Item2) // if they are the same, no need to swap them
                    collisionFunctions.Add((key.Item2, key.Item1), (a, b) => collisionFunctions[key](b, a));
            }
            // same for distance functions
            keys = distanceFunctions.Keys.ToList();
            foreach (var key in keys)
            {
                if (key.Item1 != key.Item2) // if they are the same, no need to swap them
                    distanceFunctions.Add((key.Item2, key.Item1), (a, b, dir) => distanceFunctions[key](b, a, -dir).GetAxisInvert());
            }

            keys = collisionFunctions.Keys.ToList();
            // also make sure point type calls can make use of vector functions
            foreach (var key in keys)
            {
                if (key.Item1 == TVector3)
                    collisionFunctions.Add((TPoint3, key.Item2), (a, b) => collisionFunctions[key](((Point3)a).Pos, b));

                if (key.Item2 == TVector3)
                    collisionFunctions.Add((key.Item1, TPoint3), (a, b) => collisionFunctions[key](a, ((Point3)b).Pos));
            }
            // same for distance functions
            keys = distanceFunctions.Keys.ToList();
            foreach (var key in keys)
            {
                if (key.Item1 == TVector3)
                    distanceFunctions.Add((TPoint3, key.Item2), (a, b, dir) => distanceFunctions[key](((Point3)a).Pos, b, dir));

                if (key.Item2 == TVector3)
                    distanceFunctions.Add((key.Item1, TPoint3), (a, b, dir) => distanceFunctions[key](a, ((Point3)b).Pos, dir));
            }
        }

        static readonly Dictionary<(Type, Type), Func<object, object, bool>> collisionFunctions = new()
        {
            { (TShapeCollection3, TVector3), (a, b) => ColShapeCollectionObject((IShape3Collection)a, b) },
            { (TShapeCollection3, TSphere3), (a, b) => ColShapeCollectionObject((IShape3Collection)a, b) },
            { (TShapeCollection3, TAxis3), (a, b) => ColShapeCollectionObject((IShape3Collection)a, b) },
            { (TShapeCollection3, TRay3), (a, b) => ColShapeCollectionObject((IShape3Collection)a, b) },
            { (TShapeCollection3, TLine3), (a, b) => ColShapeCollectionObject((IShape3Collection)a, b) },
            { (TShapeCollection3, TPlane3), (a, b) => ColShapeCollectionObject((IShape3Collection)a, b) },
            { (TShapeCollection3, TTriangle3), (a, b) => ColShapeCollectionObject((IShape3Collection)a, b) },
            { (TShapeCollection3, TAxisRadius3), (a, b) => ColShapeCollectionObject((IShape3Collection)a, b) },
            { (TShapeCollection3, TCapsule3), (a, b) => ColShapeCollectionObject((IShape3Collection)a, b) },
            { (TShapeCollection3, TAABB3), (a, b) => ColShapeCollectionObject((IShape3Collection)a, b) },
            { (TShapeCollection3, TBox3), (a, b) => ColShapeCollectionObject((IShape3Collection)a, b) },
            { (TShapeCollection3, TCylinder3), (a, b) => ColShapeCollectionObject((IShape3Collection)a, b) },
            { (TShapeCollection3, TShapeCollection3), (a, b) => ColShapeCollectionObject((IShape3Collection)a, b) },

            { (TVector3, TVector3), (a, b) => ColVectorVector((Vector3)a, (Vector3)b) },
            { (TVector3, TAABB3), (a, b) => ColVectorAABB((Vector3)a, (AABB3)b) },
            { (TVector3, TBox3), (a, b) => ColVectorBox((Vector3)a, (Box3)b) },
            { (TVector3, TCapsule3), (a, b) => ColAnyCapsule(new Point3((Vector3)a), (Capsule3)b) },

            { (TSphere3, TAABB3), (a, b) => ColSphereAABB((Sphere3)a, (AABB3)b) },
            { (TSphere3, TBox3), (a, b) => ColSphereBox((Sphere3)a, (Box3)b) },
            { (TSphere3, TCapsule3), (a, b) => ColSphereCapsule((Sphere3)a, (Capsule3)b) },
            { (TSphere3, TSphere3), (a, b) => ColSphereSphere((Sphere3)a, (Sphere3)b) },
            { (TSphere3, TTriangle3), (a, b) => ColSphereTriangle((Sphere3)a, (Triangle3)b) },

            { (TRay3, TPlane3), (a, b) => ColRayPlane((Ray3)a, (Plane3)b) },
            { (TRay3, TTriangle3), (a, b) => ColRayTriangle((Ray3)a, (Triangle3)b, out _, out _) },
            { (TRay3, TCapsule3), (a, b) => ColAnyCapsule((IShape3)a, (Capsule3)b) },

            { (TLine3, TPlane3), (a, b) => ColLinePlane((Line3)a, (Plane3)b) },
            { (TLine3, TTriangle3), (a, b) => ColLineTriangle((Line3)a, (Triangle3)b, out _, out _) },
            { (TLine3, TCapsule3), (a, b) => ColAnyCapsule((IShape3)a, (Capsule3)b) },

            { (TAxis3, TCapsule3), (a, b) => ColAnyCapsule((IShape3)a, (Capsule3)b) },

            { (TPlane3, TCapsule3), (a, b) => ColAnyCapsule((IShape3)a, (Capsule3)b) },

            { (TTriangle3, TAxisRadius3), (a, b) => ColTriangleAxisRadius((Triangle3)a, (AxisRadius3)b) },
            { (TTriangle3, TCylinder3), (a, b) => ColTriangleCylinder((Triangle3)a, (Cylinder3)b) },
            { (TTriangle3, TTriangle3), (a, b) => ColTriangleTriangle((Triangle3)a, (Triangle3)b, out _, out _) },
            { (TTriangle3, TCapsule3), (a, b) => ColAnyCapsule((IShape3)a, (Capsule3)b) },

            { (TAxisRadius3, TCapsule3), (a, b) => ColAnyCapsule((AxisRadius3)a, (Capsule3)b) },

            { (TCapsule3, TCapsule3), (a, b) => ColAnyCapsule((IShape3)a, (Capsule3)b) },
            
            { (TAABB3, TCapsule3), (a, b) => ColAnyCapsule((IShape3)a, (Capsule3)b) },
            { (TAABB3, TCylinder3), (a, b) => ColAABBCylinder((AABB3)a, (Cylinder3)b) },

            { (TBox3, TCapsule3), (a, b) => ColAnyCapsule((IShape3)a, (Capsule3)b) },
            { (TBox3, TCylinder3), (a, b) => ColBoxCylinder((Box3)a, (Cylinder3)b) },

            { (TCylinder3, TCapsule3), (a, b) => ColAnyCapsule((IShape3)a, (Capsule3)b) },

            // special:
            { (TPoint3, TPoint3), (a, b) => ColVectorVector(((Point3)a).Pos, ((Point3)b).Pos) } // use (vector, vector) collision for (point, point)
        };

        static readonly Dictionary<(Type, Type), Func<object, object, Vector3, CollisionResult3>> distanceFunctions = new()
        {
            { (TShapeCollection3, TVector3), (a, b, dir) => DistShapeCollectionObject((IShape3Collection)a, b, dir) },
            { (TShapeCollection3, TSphere3), (a, b, dir) => DistShapeCollectionObject((IShape3Collection)a, b, dir) },
            { (TShapeCollection3, TAxis3), (a, b, dir) => DistShapeCollectionObject((IShape3Collection)a, b, dir) },
            { (TShapeCollection3, TRay3), (a, b, dir) => DistShapeCollectionObject((IShape3Collection)a, b, dir) },
            { (TShapeCollection3, TLine3), (a, b, dir) => DistShapeCollectionObject((IShape3Collection)a, b, dir) },
            { (TShapeCollection3, TPlane3), (a, b, dir) => DistShapeCollectionObject((IShape3Collection)a, b, dir) },
            { (TShapeCollection3, TTriangle3), (a, b, dir) => DistShapeCollectionObject((IShape3Collection)a, b, dir) },
            { (TShapeCollection3, TAxisRadius3), (a, b, dir) => DistShapeCollectionObject((IShape3Collection)a, b, dir) },
            { (TShapeCollection3, TCapsule3), (a, b, dir) => DistShapeCollectionObject((IShape3Collection)a, b, dir) },
            { (TShapeCollection3, TAABB3), (a, b, dir) => DistShapeCollectionObject((IShape3Collection)a, b, dir) },
            { (TShapeCollection3, TBox3), (a, b, dir) => DistShapeCollectionObject((IShape3Collection)a, b, dir) },
            { (TShapeCollection3, TCylinder3), (a, b, dir) => DistShapeCollectionObject((IShape3Collection)a, b, dir) },
            { (TShapeCollection3, TShapeCollection3), (a, b, dir) => DistShapeCollectionObject((IShape3Collection)a, b, dir) },

            { (TVector3, TPlane3), (a, b, dir) => DistVectorPlane((Vector3)a, (Plane3)b, dir) },
            { (TVector3, TSphere3), (a, b, dir) => DistVectorSphere((Vector3)a, (Sphere3)b, dir) },
            { (TVector3, TTriangle3), (a, b, dir) => DistVectorTriangle((Vector3)a, (Triangle3)b, dir) },
            { (TVector3, TCapsule3), (a, b, dir) => DistAnyCapsule(new Point3((Vector3)a), (Capsule3)b, dir) },

            { (TSphere3, TAxis3), (a, b, dir) => DistSphereAxis((Sphere3)a, (Axis3)b, dir) },
            { (TSphere3, TLine3), (a, b, dir) => DistSphereLine((Sphere3)a, (Line3)b, dir) },
            { (TSphere3, TPlane3), (a, b, dir) => DistSpherePlane((Sphere3)a, (Plane3)b, dir) },
            { (TSphere3, TTriangle3), (a, b, dir) => DistSphereTriangle((Sphere3)a, (Triangle3)b, dir) },
            { (TSphere3, TCapsule3), (a, b, dir) => DistAnyCapsule((IShape3)a, (Capsule3)b, dir) },

            { (TAxis3, TAxis3), (a, b, dir) => DistAxisAxis((Axis3)a, (Axis3)b, dir) },
            { (TAxis3, TCapsule3), (a, b, dir) => DistAnyCapsule((IShape3)a, (Capsule3)b, dir) },

            { (TRay3, TCapsule3), (a, b, dir) => DistAnyCapsule((IShape3)a, (Capsule3)b, dir) },

            { (TLine3, TLine3), (a, b, dir) => DistLineLine((Line3)a, (Line3)b, dir) },
            { (TLine3, TPlane3), (a, b, dir) => DistLinePlane((Line3)a, (Plane3)b, dir) },
            { (TLine3, TCapsule3), (a, b, dir) => DistAnyCapsule((IShape3)a, (Capsule3)b, dir) },

            { (TPlane3, TCapsule3), (a, b, dir) => DistAnyCapsule((IShape3)a, (Capsule3)b, dir) },

            { (TTriangle3, TAxisRadius3), (a, b, dir) => DistTriangleAxisRadius((Triangle3)a, (AxisRadius3)b, dir) },
            { (TTriangle3, TTriangle3), (a, b, dir) => DistTriangleTriangle((Triangle3)a, (Triangle3)b, dir) },
            { (TTriangle3, TCapsule3), (a, b, dir) => DistAnyCapsule((IShape3)a, (Capsule3)b, dir) },

            { (TAxisRadius3, TCapsule3), (a, b, dir) => DistAnyCapsule((IShape3)a, (Capsule3)b, dir) },

            { (TCapsule3, TCapsule3), (a, b, dir) => DistAnyCapsule((IShape3)a, (Capsule3)b, dir) },

            { (TAABB3, TCapsule3), (a, b, dir) => DistAnyCapsule((IShape3)a, (Capsule3)b, dir) },

            { (TBox3, TCapsule3), (a, b, dir) => DistAnyCapsule((IShape3)a, (Capsule3)b, dir) },

            { (TCylinder3, TCapsule3), (a, b, dir) => DistAnyCapsule((IShape3)a, (Capsule3)b, dir) },
        };

        public static bool GetCollision(object shape1, object shape2)
        {
            Type t1 = (shape1 is IShape3 s1) ? s1.GetCollisionType() : shape1.GetType();
            Type t2 = (shape2 is IShape3 s2) ? s2.GetCollisionType() : shape2.GetType();
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

        public static CollisionResult3 GetDistance(object shape1, object shape2, Vector3 dir)
        {
            Type t1 = (shape1 is IShape3 s1) ? s1.GetCollisionType() : shape1.GetType();
            Type t2 = (shape2 is IShape3 s2) ? s2.GetCollisionType() : shape2.GetType();
            Func<object, object, Vector3, CollisionResult3>? func;
            if (!distanceFunctions.TryGetValue((t1, t2), out func))
                throw new NotImplementedException($"A distance check between {shape1.GetType()} and {shape2.GetType()} is not implemented yet.");
            return func(shape1, shape2, dir);

        }


        #region Any

        public static CollisionResult3 DistAnyCapsule(IShape3 shape, Capsule3 capsule, Vector3 dir)
        {
            // check if the capsule is actually a sphere, then we can only do any x sphere collisions
            if (capsule.SphereDistance == Vector3.Zero)
                return shape.DistanceTo(capsule.Spheres[0], dir);

            CollisionResult3 cr;

            // calculate distance to cylinder
            cr = shape.DistanceTo(capsule.AxisRadius, dir);
            // if collision is between both sphere origins, return it
            if (cr.ColPoint.HasValue)
            {
                float dot = Vector3.Dot(cr.ColPoint.Value - capsule.Pos, Vector3.Normalize(capsule.SphereDistance));
                if (dot >= 0f && dot * dot <= capsule.SphereDistance.LengthSquared())
                    return cr;
            }

            // calculate distance to bottom sphere
            cr = shape.DistanceTo(capsule.Spheres[0], dir);

            // calculate distance to top sphere
            cr.MinResult(shape.DistanceTo(capsule.Spheres[1], dir));

            return cr;
        }

        public static bool ColAnyCapsule(IShape3 shape, Capsule3 capsule)
        {
            if (shape.CollidesWith(capsule.Sphere0))
                return true;
            if (capsule.SphereDistance == Vector3.Zero)
                return false;
            if (shape.CollidesWith(capsule.Sphere1))
                return true;

            Cylinder3 cylinder = new Cylinder3(capsule.AxisRadius.Pos, capsule.SphereDistance, capsule.Radius);
            if (cylinder.CollidesWith(shape))
                return true;

            return false;
        }

        #endregion

        #region ShapeCollection

        public static bool ColShapeCollectionObject(IShape3Collection collection, object obj)
        {
            return collection.ShapesEnumerable.Any(shape => GetCollision(shape, obj));
        }

        public static CollisionResult3 DistShapeCollectionObject(IShape3Collection collection, object obj, Vector3 dir)
        {
            CollisionResult3 crTotal = new CollisionResult3();
            foreach (var shape in collection.ShapesEnumerable)
            {
                var cr = GetDistance(shape, obj, dir);
                crTotal.MinResult(cr);
            }
            return crTotal;
        }

        #endregion

        #region Vector

        public static bool ColVectorVector(Vector3 vec1, Vector3 vec2) => vec1 == vec2;

        public static bool ColVectorAABB(Vector3 vec, AABB3 box)
        {
            return vec.X >= box.Min.X
                && vec.Y >= box.Min.Y
                && vec.Z >= box.Min.Z
                && vec.X < box.Max.X
                && vec.Y < box.Max.Y
                && vec.Z < box.Max.Z;
        }

        /// <summary>Not tested yet.</summary>
        public static bool ColVectorBox(Vector3 vec, Box3 box)
        {
            vec = Vector3.Transform(vec, box.TransformInverse);

            return vec.X >= -1
                && vec.Y >= -1
                && vec.Z >= -1
                && vec.X < 1
                && vec.Y < 1
                && vec.Z < 1;
        }

        public static CollisionResult3 DistVectorSphere(Vector3 vec, Sphere3 sphere, Vector3 dir)
        {
            Vector3 dist = vec - sphere.Pos;
            float[] t = ABCFormulaOrSomethingSimilar(dist.X, dir.X, dist.Y, dir.Y, dist.Z, dir.Z, sphere.Radius);

            if (float.IsNaN(t[1]))
                return new CollisionResult3();

            CollisionResult3 cr = new CollisionResult3();
            cr.Distance = t[1];
            cr.DistanceReversed = t[0];

            // calculate axis col
            Vector3 spherePosOnCol = sphere.Pos + dir * cr.Distance.Value;
            cr.AxisCol = Vector3.Normalize(vec - spherePosOnCol);

            return cr;
        }

        public static CollisionResult3 DistVectorPlane(Vector3 vec, Plane3 plane, Vector3 dir)
        {
            Vector3 normal = plane.Normal;

            Vector3 dist = plane.Pos - vec;
            float distProjectionOnNormal = Vector3.Dot(normal, dist);
            float rayDirectionToPlane = Vector3.Dot(normal, dir);

            // does ray start inside plane?
            if (distProjectionOnNormal == 0)
            {
                return new CollisionResult3()
                {
                    Collision = true,
                    Distance = 0f,
                    AxisCol = rayDirectionToPlane <= 0 ? plane.Normal : -plane.Normal
                };
            }

            // is parallel to plane?
            if (rayDirectionToPlane == 0)
                return new CollisionResult3();

            return new CollisionResult3()
            {
                Distance = distProjectionOnNormal / rayDirectionToPlane,
                AxisCol = rayDirectionToPlane <= 0 ? plane.Normal : -plane.Normal
            };
        }

        public static CollisionResult3 DistVectorTriangle(Vector3 vec, Triangle3 triangle, Vector3 dir)
        {
            CollisionResult3 cr = DistVectorPlane(vec, triangle.ToPlane(), dir);

            if (!cr.Distance.HasValue)
                return cr;

            cr.ColPoint = vec + dir * cr.Distance.Value;
            if (CheckIfPointOnPlaneIsAlsoOnTriangle(cr.ColPoint.Value, triangle))
                return cr;
            return new CollisionResult3();
        }

        private static float[] ABCFormulaOrSomethingSimilar(double a, double b, double c, double d, double e, double f, double r)
        {
            double bdfPow = b * b + d * d + f * f;
            double sqrt = Math.Sqrt
                    (
                        Math.Pow(2 * a * b + 2 * c * d + 2 * e * f, 2)
                        - 4 * bdfPow * (a * a + c * c + e * e - r * r)
                    );
            double partNextToSqrt = -2 * a * b - 2 * c * d - 2 * e * f;

            return new float[] { (float)((sqrt + partNextToSqrt) / (2 * bdfPow)), (float)((-sqrt + partNextToSqrt) / (2 * bdfPow)) };
        }

        #endregion

        #region Sphere

        public static bool ColSphereSphere(Sphere3 s1, Sphere3 s2)
        {
            float radiusSumSq = s1.Radius + s2.Radius;
            radiusSumSq *= radiusSumSq;
            float distSq = (s2.Pos - s1.Pos).LengthSquared();
            return distSq < radiusSumSq;

        }

        public static bool ColSphereTriangle(Sphere3 sphere, Triangle3 tri)
        {
            // check sphere vs triangle face
            Vector3 dir = -tri.N; // negative, cause I swapped the normal of the tri
            CollisionResult3 cr = DistSpherePlane(sphere, tri.ToPlane(), dir);

            if (!cr.Distance.HasValue)
                throw new Exception("there should always be a distance, because the sphere is moved towards the triangle plane");

            if (cr.Distance.Value > 0 || cr.DistanceReversed.Value < 0)
                return false;

            Vector3 spherePosOnCol = sphere.Pos + dir * cr.Distance.Value;

            float collisionRadius = sphere.Radius + cr.Distance.Value;
            if (collisionRadius < 0)
                collisionRadius = -collisionRadius;

            float radiusOnTriPlane = MathF.Sqrt(sphere.Radius * sphere.Radius - collisionRadius * collisionRadius);

            Vector3 x = Vector3.Normalize(Vector3.Cross(dir, dir.GetNonParallelVector()));
            Vector3 y = Vector3.Normalize(Vector3.Cross(dir, x));

            Polygon tri2D = new Polygon(Vector2.Zero, new List<Vector2>()
            {
                To2D(tri.Pos, x, y),
                To2D(tri.PosA, x, y),
                To2D(tri.PosB, x, y),
            });
            Circle sphere2D = new Circle(To2D(spherePosOnCol, x, y), radiusOnTriPlane);

            return Collision.ColPolygonCircle(tri2D, sphere2D);

            static Vector2 To2D(Vector3 pos, Vector3 x, Vector3 y)
            {
                return new Vector2(Vector3.Dot(x, pos), Vector3.Dot(y, pos));
            }
        }

        public static bool ColSphereCapsule(Sphere3 sphere, Capsule3 capsule)
        {
            if (ColSphereSphere(sphere, capsule.Sphere0))
                return true;
            if (capsule.SphereDistance == Vector3.Zero)
                return false;
            if (ColSphereSphere(sphere, capsule.Sphere1))
                return true;

            Vector3 dist = sphere.Pos - capsule.Pos;
            float dot = Vector3.Dot(dist, capsule.DistanceN);
            if (dot > 0 && dot * dot < capsule.SphereDistance.LengthSquared())
            {
                // sphere is next to the cylinder of the capsule
                dist -= dot * capsule.DistanceN; // make distance orthogonal to capsules cylinder axis
                float radiusSumSq = capsule.Radius + sphere.Radius;
                radiusSumSq *= radiusSumSq;
                if (dist.LengthSquared() < radiusSumSq)
                    return true;
            }
            return false;
        }

        public static bool ColSphereAABB(Sphere3 sphere, AABB3 box)
        {
            Vector3 nearestInBox;
            nearestInBox = box.MoveVectorInside(sphere.Pos);

            Vector3 dist = nearestInBox - sphere.Pos;
            if (dist == Vector3.Zero)
                return true;
            else if (dist.LengthSquared() < sphere.Radius * sphere.Radius)
                return true;
            return false;
        }

        public static bool ColSphereBox(Sphere3 sphere, Box3 box)
        {
            // clone sphere so we can rotate the sphere by the box matrix
            sphere = (Sphere3)sphere.Clone();

            // not sure, which method is more precise, but the latter is faster:

            //Matrix t = box.Transform;
            //box.Transform.Decompose(out Vector3 scale, out _, out _);
            //t = Matrix.CreateScale(1f / scale.X, 1f / scale.Y, 1f / scale.Z) * t;
            //t = Matrix.Invert(t);
            //sphere.Pos = Vector3.Transform(sphere.Pos, t);

            sphere.Pos = Vector3.Transform(sphere.Pos, box.TransformInverse);
            box.Transform.Decompose(out Vector3 scale, out _, out _);
            sphere.Pos *= scale; // scale position, so we don't have to scale the sphere

            AABB3 aabb = box.GetAABBWithoutRotationAndTranslation();

            return ColSphereAABB(sphere, aabb);
        }

        public static CollisionResult3 DistSphereAxis(Sphere3 sphere, Axis3 axis, Vector3 dir)
        {
            // imagine a plane, that is spanned by the axis and the cross vector of (axis x dir)
            Vector3 cross = Vector3.Cross(dir, axis.Dir);
            // parallel?
            if (cross == Vector3.Zero)
                return new CollisionResult3();
            cross.Normalize();

            Vector3 dist = axis.Pos - sphere.Pos;
            float axesDistance = Vector3.Dot(dist, cross);

            if (Math.Abs(axesDistance) > sphere.Radius)
                return new CollisionResult3();

            cross = Vector3.Normalize(Vector3.Cross(axis.Dir, cross));
            Plane3 plane = new Plane3(axis.Pos, cross);

            CollisionResult3 cr = DistVectorPlane(sphere.Pos, plane, dir);
            if (!cr.Distance.HasValue)
                return new CollisionResult3();

            float shiftOnDir = (float)Math.Sqrt(sphere.Radius * sphere.Radius - axesDistance * axesDistance);
            float dirLength = dir.Length();
            shiftOnDir /= dirLength;


            // until here it works fine, when both lines are orthogonal to each other, but when they get more parallel, we need this code
            Vector3 dirNormalized = dir / dirLength;
            Vector3 axisDirN = axis.Dir;
            float angle = (float)Math.Acos(Vector3.Dot(dirNormalized, axisDirN));
            if (angle > MathHelper.PiOver2)
                angle = MathHelper.Pi - angle;

            // second check for parallelism...
            if (angle <= 0)
                return new CollisionResult3();

            float shiftOnDirWithRespectToMoreParallelDir = shiftOnDir / (float)Math.Sin(angle);
            shiftOnDir = shiftOnDirWithRespectToMoreParallelDir;

            cr.DistanceReversed = cr.Distance + shiftOnDir;
            cr.Distance -= shiftOnDir;

            // calculate axis col
            Vector3 spherePosOnCol = sphere.Pos + dir * cr.Distance.Value;
            float onLine = Vector3.Dot(spherePosOnCol - axis.Pos, axisDirN);
            Vector3 axisPosOnCol = axis.Pos + axisDirN * onLine;
            cr.AxisCol = Vector3.Normalize(spherePosOnCol - axisPosOnCol);
            return cr;
        }

        public static CollisionResult3 DistSphereLine(Sphere3 sphere, Line3 line, Vector3 dir)
        {
            CollisionResult3 cr = DistSphereAxis(sphere, new Axis3(line.Pos, line.DirN), dir);

            if (!cr.Distance.HasValue)
                return new CollisionResult3();

            Vector3 spherePosAxisCol = sphere.Pos + cr.Distance.Value * dir;
            Vector3 spherePosAxisColRelativeToLineOrigin = spherePosAxisCol - line.Pos;
            float onLine = Vector3.Dot(spherePosAxisColRelativeToLineOrigin, line.DirN);
            if (onLine < 0)
            {
                cr = DistVectorSphere(line.Pos, sphere, -dir).GetAxisInvert();
                cr.ColTriangleIndex = 0; // in this context means first vertex
                return cr;
            }
            if (onLine * onLine > line.Dir.LengthSquared())
            {
                cr = DistVectorSphere(line.Pos2, sphere, -dir).GetAxisInvert();
                cr.ColTriangleIndex = 1; // in this context means second vertex
                return cr;
            }
            cr.ColTriangleIndex = 2; // in this context means edge
            return cr;
        }

        public static CollisionResult3 DistSpherePlane(Sphere3 sphere, Plane3 plane, Vector3 dir)
        {
            Vector3 nearestPointOnSphereToPlane;
            Vector3 pole = plane.Normal * sphere.Radius;
            if (Vector3.Dot(plane.Normal, dir) > 0)
                pole = -pole;
            nearestPointOnSphereToPlane = sphere.Pos - pole;

            CollisionResult3 cr = DistVectorPlane(nearestPointOnSphereToPlane, plane, dir);
            if (!cr.Distance.HasValue)
                return new CollisionResult3();

            float dirOnNormal = Vector3.Dot(dir, plane.Normal);
            float penetrationDuration = (2 * sphere.Radius) / Math.Abs(dirOnNormal);
            cr.DistanceReversed = cr.Distance + penetrationDuration;

            return cr;
        }

        public static CollisionResult3 DistSphereTriangle(Sphere3 sphere, Triangle3 tri, Vector3 dir)
        {
            // TODO: optimization: for triangles that only have one face (-> most)
            //if (Vector3.Dot(dir, -tri.N) > 0f)
            //    return new CollisionResult3();

            // check sphere vs triangle face
            CollisionResult3 cr = DistSpherePlane(sphere, tri.ToPlane(), dir);
            // when not parallel, check if col point is on the plane
            int side = -2; // parallel -> unknown side
            if (cr.Distance.HasValue)
            {
                Vector3 spherePosOnCol = sphere.Pos + dir * cr.Distance.Value;
                side = CheckIfPointOnPlaneIsNextToTriangle(spherePosOnCol, tri);
                if (side == -1) // point on plane
                {
                    cr.ColTriangleIndex = 6;
                    return cr;
                }
            }

            // check sphere vs triangle line
            if (side == -2)
            {
                // check all lines
                cr = new CollisionResult3();
                for (int i = 0; i < 3; i++)
                {
                    int j = (i + 1) % 3;
                    CollisionResult3 cr2 = DistSphereLine(sphere, Line3.FromTwoPoints(tri[i], tri[j]), dir);
                    if (cr.MinResult(cr2))
                    {
                        if (cr.ColTriangleIndex == 2)
                            cr.ColTriangleIndex = 3 + i;
                        else
                            cr.ColTriangleIndex += i;
                    }
                }
                return cr;
            }
            else
            {
                // check only one line
                int i = (side + 1) % 3;
                int j = (i + 1) % 3;
                cr = DistSphereLine(sphere, Line3.FromTwoPoints(tri[i], tri[j]), dir);
                if (cr.ColTriangleIndex == 2)
                    cr.ColTriangleIndex = 3 + i;
                else if (cr.ColTriangleIndex != -1)
                    cr.ColTriangleIndex += i;

                return cr;
            }
        }

        #endregion

        #region Axis

        public static CollisionResult3 DistAxisAxis(Axis3 axis1, Axis3 axis2, Vector3 dir)
        {
            // parallel?
            if (Vector3.Cross(axis1.Dir, axis2.Dir) == Vector3.Zero)
            {
                throw new NotImplementedException();
            }

            Plane3 plane1 = new Plane3(axis1.Pos, Vector3.Normalize(Vector3.Cross(axis1.Dir, dir)));
            float? depth = PenetrateAxisPlane(axis2.Pos, axis2.Dir, plane1, out Vector3 colNormal);

            // are parallel? (TODO: again?)
            if (!depth.HasValue)
                return new CollisionResult3();

            CollisionResult3 cr = new CollisionResult3();
            cr.ColPoint = axis2.Pos + axis2.Dir * depth.Value;
            cr.AxisCol = colNormal;
            cr.Distance = Vector3.Dot(cr.ColPoint.Value - axis1.Pos, dir);

            if (cr.Distance == 0)
                cr.Collision = true;

            return cr;
        }

        public static float? PenetrateAxisPlane(Vector3 pos, Vector3 dir, Plane3 plane) => PenetrateAxisPlane(pos, dir, plane, out _);
        public static float? PenetrateAxisPlane(Vector3 pos, Vector3 dir, Plane3 plane, out Vector3 colNormal)
        {
            float depth = 0f;

            Vector3 normal = plane.Normal;

            Vector3 dist = plane.Pos - pos;
            float distProjectionOnNormal = Vector3.Dot(normal, dist);
            float rayProjectionOnNormal = Vector3.Dot(normal, dir);

            // does axis start inside plane?
            if (distProjectionOnNormal == 0)
            {
                colNormal = rayProjectionOnNormal <= 0f ? normal : -normal;
                return depth;
            }

            // is axis parallel to plane?
            if (rayProjectionOnNormal == 0)
            {
                colNormal = Vector3.Zero;
                return null;
            }

            colNormal = rayProjectionOnNormal <= 0f ? normal : -normal;
            return distProjectionOnNormal / rayProjectionOnNormal;
        }

        #endregion

        #region Ray

        public static bool ColRayPlane(Ray3 ray, Plane3 plane)
        {
            Vector3 normal = plane.Normal;

            Vector3 dist = plane.Pos - ray.Pos;
            float distProjectionOnNormal = Vector3.Dot(normal, dist);

            // does ray start inside plane?
            if (distProjectionOnNormal == 0)
                return true;

            // is ray facing towards plane?
            float rayDirectionToPlane = Vector3.Dot(normal, ray.Dir);
            if (Math.Sign(rayDirectionToPlane) == Math.Sign(distProjectionOnNormal))
                return true;
            return false;
        }

        public static float? PenetrateRayPlane(Ray3 ray, Plane3 plane) => PenetrateRayPlane(ray, plane, out _);
        public static float? PenetrateRayPlane(Ray3 ray, Plane3 plane, out Vector3 colNormal)
        {
            float depth = 0f;

            Vector3 normal = plane.Normal;

            Vector3 dist = plane.Pos - ray.Pos;
            float distProjectionOnNormal = Vector3.Dot(normal, dist);
            float rayProjectionOnNormal = Vector3.Dot(normal, ray.Dir);

            // does ray start inside plane?
            if (distProjectionOnNormal == 0)
            {
                colNormal = rayProjectionOnNormal <= 0f ? normal : -normal;
                return depth;
            }

            // is ray facing towards plane?
            if (Math.Sign(rayProjectionOnNormal) == Math.Sign(distProjectionOnNormal))
            {
                colNormal = rayProjectionOnNormal <= 0f ? normal : -normal;
                return distProjectionOnNormal / rayProjectionOnNormal;
            }
            colNormal = Vector3.Zero;
            return null;
        }

        public static bool ColRayTriangle(Ray3 ray, Triangle3 triangle, out Vector3 colNormal, out Vector3 colPoint)
        {
            Plane3 plane = triangle.ToPlane();

            float? depth = PenetrateRayPlane(ray, plane, out colNormal);
            if (!depth.HasValue)
            {
                colPoint = Vector3.Zero;
                return false;
            }

            colPoint = ray.Pos + ray.Dir * depth.Value;

            return CheckIfPointOnPlaneIsAlsoOnTriangle(colPoint, triangle);
        }

        #endregion

        #region Line

        public static bool ColLinePlane(Line3 line, Plane3 plane)
        {
            Vector3 normal = plane.Normal;

            Vector3 dist = plane.Pos - line.Pos;
            float distProjectionOnNormal = Vector3.Dot(normal, dist);

            // does line start inside plane?
            if (distProjectionOnNormal == 0)
                return true;

            // is line facing towards plane?
            float lineProjectionOnNormal = Vector3.Dot(normal, line.Dir);
            if (Math.Sign(lineProjectionOnNormal) == Math.Sign(distProjectionOnNormal))
            {
                // does the end of the line end after the plane?
                if (Math.Abs(lineProjectionOnNormal) >= Math.Abs(distProjectionOnNormal))
                    return true;
            }
            return false;
        }

        public static float? PenetrateLinePlane(Line3 line, Plane3 plane) => PenetrateLinePlane(line, plane, out _);
        public static float? PenetrateLinePlane(Line3 line, Plane3 plane, out Vector3 colNormal)
        {
            float depth = 0f;

            Vector3 normal = plane.Normal;

            Vector3 dist = plane.Pos - line.Pos;
            float distProjectionOnNormal = Vector3.Dot(normal, dist);
            float lineProjectionOnNormal = Vector3.Dot(normal, line.Dir);

            // does line start inside plane?
            if (distProjectionOnNormal == 0)
            {
                colNormal = lineProjectionOnNormal <= 0f ? normal : -normal;
                return depth;
            }

            // is line facing towards plane?
            if (Math.Sign(lineProjectionOnNormal) == Math.Sign(distProjectionOnNormal))
            {
                // does the end of the line end after the plane?
                if (Math.Abs(lineProjectionOnNormal) >= Math.Abs(distProjectionOnNormal))
                {
                    colNormal = lineProjectionOnNormal <= 0f ? normal : -normal;
                    return distProjectionOnNormal / lineProjectionOnNormal;
                }
            }
            colNormal = Vector3.Zero;
            return null;
        }

        public static bool ColLineTriangle(Line3 line, Triangle3 triangle, out Vector3 colNormal, out Vector3 colPoint)
        {
            Plane3 plane = triangle.ToPlane();

            float? depth = PenetrateLinePlane(line, plane, out colNormal);
            if (!depth.HasValue)
            {
                colPoint = Vector3.Zero;
                return false;
            }

            colPoint = line.Pos + line.Dir * depth.Value;

            return CheckIfPointOnPlaneIsAlsoOnTriangle(colPoint, triangle);
        }

        private static bool CheckIfPointOnPlaneIsAlsoOnTriangle(Vector3 colPoint, Triangle3 triangle)
        {
            return CheckIfPointOnPlaneIsNextToTriangle(colPoint, triangle) == -1;
        }
        private static int CheckIfPointOnPlaneIsNextToTriangle(Vector3 colPoint, Triangle3 triangle)
        {
            for (int a = 0; a < 3; a++)
            {
                int b = (a + 1) % 3;
                int c = (a + 2) % 3;
                Vector3 bc = triangle[c] - triangle[b];
                Vector3 a_bc = Vector3.Normalize(Vector3.Cross(triangle.N, bc));
                float d = Vector3.Dot(a_bc, triangle[b] - triangle[a]);

                float col_a = Vector3.Dot(colPoint - triangle[a], a_bc);

                if (col_a > d)
                    return a;
            }
            return -1;
        }

        public static CollisionResult3 DistLinePlane(Line3 line, Plane3 plane, Vector3 dir)
        {
            CollisionResult3 cr1 = DistVectorPlane(line.Pos, plane, dir);
            CollisionResult3 cr2 = DistVectorPlane(line.Pos2, plane, dir);

            if (cr1.Distance.HasValue)
            {
                if (cr1.Distance.Value < cr2.Distance.Value)
                {
                    cr1.ColPoint = line.Pos + dir * cr1.Distance.Value;
                    return cr1;
                }
                else
                {
                    cr2.ColPoint = line.Pos2 + dir * cr2.Distance.Value;
                    return cr2;
                }
            }

            return new CollisionResult3();
        }

        public static CollisionResult3 DistLineLine(Line3 line1, Line3 line2, Vector3 dir)
        {
            // parallel?
            if (Vector3.Cross(line1.Dir, line2.Dir).Length() == 0)
            {
                // TODO
                return new CollisionResult3();
                throw new NotImplementedException();
            }

            Vector3 normal = Vector3.Cross(line1.Dir, dir);

            if (normal == Vector3.Zero)
            {
                // line1 has same dir (or negative) as dir -> dart-like
                // make ray check instead
                return new CollisionResult3(); // TODO: make ray check instead
            }
            normal.Normalize();
            Plane3 plane1 = new Plane3(line1.Pos, normal);
            float? depth = PenetrateAxisPlane(line2.Pos, line2.Dir, plane1, out _);

            // are parallel? (TODO: again?)
            if (!depth.HasValue)
                return new CollisionResult3();

            // line2 is too short?
            if (depth.Value < 0 || depth.Value > 1f)
                return new CollisionResult3();

            CollisionResult3 cr = new CollisionResult3();
            cr.ColPoint = line2.Pos + line2.Dir * depth.Value;

            // line1 is too short?
            Vector3 n = Vector3.Cross(dir, plane1.Normal); // this is the orthogonal vector to dir, which still lies on the plane
            float colOnLine1 = Vector3.Dot(cr.ColPoint.Value, n);
            float line1Begin = Vector3.Dot(line1.Pos, n);
            if (colOnLine1 < line1Begin)
                return new CollisionResult3();
            float line1End = Vector3.Dot(line1.Pos2, n);
            if (colOnLine1 > line1End)
                return new CollisionResult3();

            float lerpOnLine1 = (colOnLine1 - line1Begin) / (line1End - line1Begin);

            Vector3 colPointOnLine1 = line1.Pos + line1.Dir * lerpOnLine1;

            cr.Distance = Vector3.Dot(cr.ColPoint.Value - colPointOnLine1, dir) / dir.LengthSquared();

            // calculate axisCol, by crossing both line dirs
            cr.AxisCol = Vector3.Normalize(Vector3.Cross(line1.Dir, line2.Dir));
            if (Vector3.Dot(cr.AxisCol, dir) > 0) // axisCol must face to the reverse of dir
                cr.AxisCol = -cr.AxisCol;

            if (cr.Distance == 0)
                cr.Collision = true;

            return cr;
        }

        #endregion

        #region Plane

        #endregion

        #region Triangle

        public static bool ColTriangleTriangle(Triangle3 tri1, Triangle3 tri2, out Vector3 colNormal, out Vector3 colPoint)
        {
            for (int i = 0; i < 3; i++)
            {
                Line3 line = new Line3(tri1[i], tri1[(i + 1) % 3] - tri1[i]);
                if (ColLineTriangle(line, tri2, out colNormal, out colPoint))
                    return true;
            }
            for (int i = 0; i < 3; i++)
            {
                Line3 line = new Line3(tri2[i], tri2[(i + 1) % 3] - tri2[i]);
                if (ColLineTriangle(line, tri1, out colNormal, out colPoint))
                    return true;
            }

            colNormal = Vector3.Zero;
            colPoint = Vector3.Zero;
            return false;
        }

        public static bool ColTriangleAxisRadius(Triangle3 tri, AxisRadius3 axisRadius)
        {
            throw new NotImplementedException();
        }

        public static bool ColTriangleCylinder(Triangle3 tri, Cylinder3 cylinder)
        {
            // clip triangle on top and base slab of cylinder. generate a 2d polygon this way (projected in the direction of the cylinder)
            List<Vector2> vertices = ClipTriangle2DUnit(tri, cylinder);


            // do SAT polygon vs circle
            Circle circle = new Circle(Vector2.Zero, 1f);

            Polygon poly = new Polygon(Vector2.Zero, vertices);

            return Collision.ColPolygonCircle(poly, circle);
        }

        public static List<Vector2> ClipTriangle2DUnit(Triangle3 tri, Cylinder3 cylinder)
        {
            List<Vector3> v = tri.Vertices.ToList();
            Matrix cylinderToUnit; // pos at 0; pos2 at 0,1,0; radius at 1

            float dirLength = cylinder.Length.Length();
            Matrix world = Matrix.CreateScale(cylinder.Radius, dirLength, cylinder.Radius)
                * MatrixExtension.CreateMatrixRotationFromTo(Vector3.Up, cylinder.Length / dirLength) // normalizing here, so no NaN matrix results on parallel vectors
                * Matrix.CreateTranslation(cylinder.Pos);
            cylinderToUnit = Matrix.Invert(world);

            for (int i = 0; i < v.Count; i++)
            {
                v[i] = Vector3.Transform(v[i], cylinderToUnit);
            }

            v.Add(v[0]); // closed polygon

            int lastVertexStatus = GetVertexStatus(v[0]);

            List<Vector2> clipped = new List<Vector2>();

            if (lastVertexStatus == 0)
                clipped.Add(v[0].XZ());

            for (int i = 1; i < v.Count; i++)
            {
                int status = GetVertexStatus(v[i]);

                // check if edge collides with the clip face
                if (status != lastVertexStatus)
                {
                    if (lastVertexStatus == -1)
                    {
                        // bottom face collision (getting inside from the bottom)
                        BottomCol(v, clipped, i);
                        status = 0;
                    }
                    else if (lastVertexStatus == 1)
                    {
                        // top face collision (getting inside from the top)
                        TopCol(v, clipped, i);
                        status = 0;
                    }
                    else // lastVertexStatus == 0
                    {
                        if (status == -1)
                        {
                            // bottom face collision (getting outside to the bottom)
                            BottomCol(v, clipped, i);
                        }
                        else if (status == 1)
                        {
                            // top face collision (getting outside to the top)
                            TopCol(v, clipped, i);
                        }
                    }
                }
                else if (status == 0)
                {
                    if (i != v.Count - 1) // ignore last vertex, cause it only closes the polygon
                        clipped.Add(v[i].XZ());
                }

                lastVertexStatus = status;
            }

            return clipped;

            static int GetVertexStatus(Vector3 v)
            {
                int lastVertexStatus;
                if (v.Y < 0f)
                    lastVertexStatus = -1;
                else if (v.Y > 1f)
                    lastVertexStatus = 1;
                else
                    lastVertexStatus = 0;
                return lastVertexStatus;
            }

            static void BottomCol(List<Vector3> v, List<Vector2> clipped, int i)
            {
                Vector3 dir = v[i] - v[i - 1];
                float lerp = -v[i - 1].Y / dir.Y;
                Vector3 clip = v[i - 1] + dir * lerp;
                //clip.Y = 0f;
                clipped.Add(clip.XZ());

                v.Insert(i, clip);
            }

            static void TopCol(List<Vector3> v, List<Vector2> clipped, int i)
            {
                Vector3 dir = v[i] - v[i - 1];
                float yDist = v[i - 1].Y - 1;
                float lerp = -yDist / dir.Y;
                Vector3 clip = v[i - 1] + dir * lerp;
                //clip.Y = 1f;
                clipped.Add(clip.XZ());
                v.Insert(i, clip);
            }
        }

        public static List<Vector3> ClipTriangle(Triangle3 tri, Cylinder3 cylinder)
        {
            List<Vector3> v = tri.Vertices.ToList();
            Matrix cylinderToUnit; // pos at 0; pos2 at 0,1,0; radius at 1

            float dirLength = cylinder.Length.Length();
            Matrix world = Matrix.CreateScale(cylinder.Radius, dirLength, cylinder.Radius)
                * MatrixExtension.CreateMatrixRotationFromTo(Vector3.Up, cylinder.Length / dirLength) // normalizing here, so no NaN matrix results on parallel vectors
                * Matrix.CreateTranslation(cylinder.Pos);
            cylinderToUnit = Matrix.Invert(world);

            for (int i = 0; i < v.Count; i++)
            {
                v[i] = Vector3.Transform(v[i], cylinderToUnit);
            }

            v.Add(v[0]); // closed polygon

            int lastVertexStatus = GetVertexStatus(v[0]);

            List<Vector3> clipped = new List<Vector3>();

            if (lastVertexStatus == 0)
                clipped.Add(v[0]);

            for (int i = 1; i < v.Count; i++)
            {
                int status = GetVertexStatus(v[i]);

                // check if edge collides with the clip face
                if (status != lastVertexStatus)
                {
                    if (lastVertexStatus == -1)
                    {
                        // bottom face collision (getting inside from the bottom)
                        BottomCol(v, clipped, i);
                        status = 0;
                    }
                    else if (lastVertexStatus == 1)
                    {
                        // top face collision (getting inside from the top)
                        TopCol(v, clipped, i);
                        status = 0;
                    }
                    else // lastVertexStatus == 0
                    {
                        if (status == -1)
                        {
                            // bottom face collision (getting outside to the bottom)
                            BottomCol(v, clipped, i);
                        }
                        else if (status == 1)
                        {
                            // top face collision (getting outside to the top)
                            TopCol(v, clipped, i);
                        }
                    }
                }
                else if (status == 0)
                {
                    if (i != v.Count - 1) // ignore last vertex, cause it only closes the polygon
                        clipped.Add(v[i]);
                }

                lastVertexStatus = status;
            }

            for (int i = 0; i < clipped.Count; i++)
            {
                clipped[i] = Vector3.Transform(clipped[i], world);
            }

            return clipped;

            static int GetVertexStatus(Vector3 v)
            {
                int lastVertexStatus;
                if (v.Y < 0f)
                    lastVertexStatus = -1;
                else if (v.Y > 1f)
                    lastVertexStatus = 1;
                else
                    lastVertexStatus = 0;
                return lastVertexStatus;
            }

            static void BottomCol(List<Vector3> v, List<Vector3> clipped, int i)
            {
                Vector3 dir = v[i] - v[i - 1];
                float lerp = -v[i - 1].Y / dir.Y;
                Vector3 clip = v[i - 1] + dir * lerp;
                //clip.Y = 0f;
                clipped.Add(clip);

                v.Insert(i, clip);
            }

            static void TopCol(List<Vector3> v, List<Vector3> clipped, int i)
            {
                Vector3 dir = v[i] - v[i - 1];
                float yDist = v[i - 1].Y - 1;
                float lerp = -yDist / dir.Y;
                Vector3 clip = v[i - 1] + dir * lerp;
                //clip.Y = 1f;
                clipped.Add(clip);
                v.Insert(i, clip);
            }
        }

        public static CollisionResult3 DistTriangleTriangle(Triangle3 tri1, Triangle3 tri2, Vector3 dir)
        {
            // check distance for each line with each (3x3)
            CollisionResult3 cr = new CollisionResult3();
            for (int i = 0; i < 3; i++)
            { // i==2 && j==1
                int i2 = (i + 1) % 3;
                for (int j = 0; j < 3; j++)
                {
                    int j2 = (j + 1) % 3;
                    CollisionResult3 cr1 = DistLineLine(new Line3(tri1[i], tri1[i2] - tri1[i]),
                        new Line3(tri2[j], tri2[j2] - tri2[j]), dir);

                    cr.MinResultTakeNormalAsReversed(cr1);
                }
            }

            // check distance for each corner of triangle1 with the triangle2 face
            for (int i = 0; i < 3; i++)
            {
                CollisionResult3 cr1 = DistVectorTriangle(tri1[i], tri2, dir);
                cr.MinResultTakeNormalAsReversed(cr1);
            }


            // check distance for each corner of triangle2 with the triangle1 face
            for (int i = 0; i < 3; i++)
            {
                CollisionResult3 cr1 = DistVectorTriangle(tri2[i], tri1, -dir).GetAxisInvert(); // invert dir, cause we check the other way around
                cr.MinResultTakeNormalAsReversed(cr1);
            }

            return cr;
        }

        public static CollisionResult3 DistTriangleAxisRadius(Triangle3 tri, AxisRadius3 axisRadius3, Vector3 dir)
        {
            // TODO: LATER (although probably introduce a new triangle that has only one face)
            //// stop if dir faces toward the backface
            //if (Vector3.Dot(dir, tri.N) > 0)
            //    return new CollisionResult3();

            //Matrix toAxisOrthPlane = Matrix.

            Vector3 zAxisDir = axisRadius3.Dir;
            Vector3 xAxisDir = Vector3.Normalize(Vector3.Cross(zAxisDir.GetNonParallelVector(), zAxisDir));
            Vector3 yAxisDir = Vector3.Normalize(Vector3.Cross(zAxisDir, xAxisDir));
            // convert triangle to 2d polygon (along axisRadius3 orthogonal plane)

            Vector2 To2D(Vector3 v)
            {
                return new Vector2(Vector3.Dot(xAxisDir, v), Vector3.Dot(yAxisDir, v));
            }
            Vector3 To3D(Vector2 v)
            {
                return xAxisDir * v.X + yAxisDir * v.Y;
            }

            Vector2 dir2D = To2D(dir);
            Circle cylinder2D = new Circle(Vector2.Zero, axisRadius3.Radius);
            Polygon tri2D = new Polygon(Vector2.Zero, new List<Vector2>()
            {
                To2D(tri[0] - axisRadius3.Pos),
                To2D(tri[1] - axisRadius3.Pos),
                To2D(tri[2] - axisRadius3.Pos),
            });

            // to later determine where the collision happened on the triangle
            List<int> triToPolyIndex = new List<int>() { 0, 1, 2 };

            float dot = Vector3.Dot(-tri.N, axisRadius3.Dir);
            if (dot < 0)
            {
                // if the triangle faces into the other direction of the axis, swap vertices so that the triangle is still clockwisely defined
                Vector2 save = tri2D.Vertices[1];
                tri2D.Vertices[1] = tri2D.Vertices[2];
                tri2D.Vertices[2] = save;

                // also swap the indexing
                triToPolyIndex[1] = 2;
                triToPolyIndex[2] = 1;
            }
            else if (dot == 0)
            {
                // check for overlapping vertices. remove them
                for (int i = tri2D.Vertices.Count - 1; i >= 0; i--)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (tri2D.Vertices[i] == tri2D.Vertices[j])
                        {
                            tri2D.Vertices.RemoveAt(i);
                            triToPolyIndex.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            // calculate collision using M_Polygon x M_Circle
            CollisionResultPolygonExtended cr2D = tri2D.DistToCircleExtended(cylinder2D, dir2D);

            CollisionResult3 cr = new CollisionResult3();
            cr.Distance = cr2D.Distance;
            cr.DistanceReversed = cr2D.DistanceReversed;
            cr.AxisCol = To3D(cr2D.AxisCol);
            cr.AxisColReversed = To3D(cr2D.AxisColReversed);

            if (cr2D.ColVertexIndex != -1f)
            {
                int floor = (int)Math.Floor(cr2D.ColVertexIndex);
                if (floor < 0)
                    floor = triToPolyIndex.Count - 1;
                if (floor == triToPolyIndex.Count)
                    floor = 0;
                int ceil = (int)Math.Ceiling(cr2D.ColVertexIndex);
                if (ceil >= triToPolyIndex.Count)
                    ceil = 0;

                float lerpAmount = cr2D.ColVertexIndex % 1;
                if (lerpAmount < 0)
                    lerpAmount += 1;

                cr.ColPoint = Vector3.LerpPrecise(tri[triToPolyIndex[floor]], tri[triToPolyIndex[ceil]], lerpAmount)
                            + dir * cr2D.Distance.Value;

                if (cr2D.ColVertexIndex % 1 == 0)
                    cr.ColTriangleIndex = floor;
                else
                    cr.ColTriangleIndex = 3 + floor;
            }
            return cr;
        }

        #endregion

        #region AxisRadius

        #endregion

        #region Capsule

        #endregion

        #region AABB

        public static bool ColAABBCylinder(AABB3 aabb, Cylinder3 cylinder)
        {
            return ColBoxCylinder(aabb.ToBox(), cylinder);
        }

        #endregion

        #region Box

        public static bool ColBoxCylinder(Box3 box, Cylinder3 cylinder)
        {
            // check if cylinder origin is inside the box
            if (box.CollidesWith(cylinder.Center))
                return true;

            // triangulate the box and check if any triangle collides with the cylinder
            foreach (var tri in box.Triangulate())
            {
                if (ColTriangleCylinder(tri, cylinder))
                    return true;
            }
            return false;
        }

        #endregion

        #region Cylinder

        #endregion
    }
}

#pragma warning restore CS8629 // Nullable value type may be null.