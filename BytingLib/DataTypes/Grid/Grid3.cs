namespace BytingLib
{
    public class Grid3<T> where T : IBoundingBox
    {
        public Vector3 FieldSize { get; private set; }
        public Dictionary<Int3, List<T>> Lists { get; private set; }

        public bool BoundsSet { get; private set; } = false;
        private Int3 min;
        private Int3 max;

        public Int3 Min => min;
        public Int3 Max => max;

        public Grid3(float fieldSize)
        {
            FieldSize = new Vector3(fieldSize);
            Lists = new Dictionary<Int3, List<T>>();
        }

        public Grid3(Vector3 fieldSize)
        {
            FieldSize = fieldSize;
            Lists = new Dictionary<Int3, List<T>>();
        }

        public Grid3(Vector3 fieldSize, Dictionary<Int3, List<T>> lists)
        {
            FieldSize = fieldSize;
            Lists = lists;
        }

        public virtual void Add(T entity)
        {
            AddToGrid(entity);
        }

        /// <summary>
        /// Used for when coords are pre-calculated.
        /// Warning: Coord is not checked for validity (if it actually fits the entities).
        /// </summary>
        public virtual void AddMultipleToSingleCoord(Int3 coord, IEnumerable<T> entities)
        {
            var list = GetOrCreateList(coord);
            list.AddRange(entities);
        }

        /// <summary>Does not add to Entities list.</summary>
        private void AddToGrid(T entity)
        {
            foreach (var c in GetCoords(entity))
                AddToCoord(c, entity);
        }

        public void ChangePos(T entity, Action<T> changePosAction)
        {
            if (!RemoveFromGrid(entity))
                throw new Exception("Error in map storage: Couldn't find entity in some coordinations where it should have been.");

            changePosAction(entity);

            AddToGrid(entity);
        }

        public virtual bool Remove(T entity)
        {
            return !RemoveFromGrid(entity);
        }

        /// <summary>Does not remove from Entities list.</summary>
        private bool RemoveFromGrid(T entity)
        {
            bool anyFail = false;
            foreach (var c in GetCoords(entity))
            {
                if (!RemoveFromCoord(c, entity))
                    anyFail = true;
            }
            return !anyFail;
        }

        public void Clear()
        {
            Lists.Clear();
            BoundsSet = false;
            min = default;
            max = default;
        }

        public Int3 GetCoord(Vector3 pos)
        {
            return new Int3(
             (int)Math.Floor(pos.X / FieldSize.X),
             (int)Math.Floor(pos.Y / FieldSize.Y),
             (int)Math.Floor(pos.Z / FieldSize.Z));
        }

        public IEnumerable<Int3> GetCoords(Vector3 pos)
        {
            yield return GetCoord(pos);
        }
        public IEnumerable<Int3> GetCoords(IBoundingBox boundingBoxObject)
        {
            return GetCoords(boundingBoxObject.GetBoundingBox());
        }


        public IEnumerable<Int3> GetCoords(BoundingBox boundingBox, Vector3 movement)
        {
            return GetCoords(boundingBox.Expand(movement));
        }

        public IEnumerable<Int3> GetCoords(BoundingBox boundingBox)
        {
            if (!float.IsFinite(boundingBox.Min.X)
                || !float.IsFinite(boundingBox.Min.Y)
                || !float.IsFinite(boundingBox.Min.Z)
                || !float.IsFinite(boundingBox.Max.X)
                || !float.IsFinite(boundingBox.Max.Y)
                || !float.IsFinite(boundingBox.Max.Z))
                yield break;


            int x1 = (int)Math.Floor(boundingBox.Min.X / FieldSize.X);
            int y1 = (int)Math.Floor(boundingBox.Min.Y / FieldSize.Y);
            int z1 = (int)Math.Floor(boundingBox.Min.Z / FieldSize.Z);
            int x2 = (int)Math.Floor(boundingBox.Max.X / FieldSize.X) + 1;
            int y2 = (int)Math.Floor(boundingBox.Max.Y / FieldSize.Y) + 1;
            int z2 = (int)Math.Floor(boundingBox.Max.Z / FieldSize.Z) + 1;

            Int3 c = new Int3(x1, y1, z1);
            for (; c.Z < z2; c.Z++)
            {
                for (c.Y = y1; c.Y < y2; c.Y++)
                {
                    for (c.X = x1; c.X < x2; c.X++)
                    {
                        yield return c;
                    }
                }
            }
        }

        private List<T> GetOrCreateList(Int3 c)
        {
            if (Lists.TryGetValue(c, out List<T>? list))
            {
                return list;
            }
            else
            {
                var list2 = new List<T>();
                Lists.Add(c, list2);

                if (!BoundsSet)
                {
                    min = max = c;
                    BoundsSet = true;
                }
                else
                {
                    if (c.X < min.X)
                        min.X = c.X;
                    else if (c.X > max.X)
                        max.X = c.X;

                    if (c.Y < min.Y)
                        min.Y = c.Y;
                    else if (c.Y > max.Y)
                        max.Y = c.Y;

                    if (c.Z < min.Z)
                        min.Z = c.Z;
                    else if (c.Z > max.Z)
                        max.Z = c.Z;
                }

                return list2;
            }
        }

        private void AddToCoord(Int3 c, T entity)
        {
            var list = GetOrCreateList(c);
            list.Add(entity);
        }
        private bool RemoveFromCoord(Int3 c, T entity)
        {
            if (Lists.TryGetValue(c, out List<T>? list))
            {
                list.Remove(entity);

                // if list is empty, remove entry from dictionary
                if (list.Count == 0)
                    Lists.Remove(c);

                return true;
            }
            return false;
        }

        public IEnumerable<T> this[Int3 c] => GetEntities(c);
        public IEnumerable<T> this[int x, int y, int z] => GetEntities(x, y, z);
        public IEnumerable<T> GetEntities(Int3 c)
        {
            if (Lists.TryGetValue(c, out List<T>? list))
                return list.AsEnumerable();
            else
                return Enumerable.Empty<T>();
        }

        public IEnumerable<T> GetEntities(int x, int y, int z) => GetEntities(new Int3(x, y, z));

        public virtual IEnumerable<T> GetEntities()
        {
            return GetEntities(GetUsedCoords());
        }

        public bool CoordContainsAny(Int3 c) => Lists.ContainsKey(c);

        public IEnumerable<T> GetEntities(BoundingBox boundingBox)
        {
            return GetEntities(GetCoords(boundingBox));
        }

        public IEnumerable<T> GetEntities(Vector3 pos)
        {
            return GetEntities(GetCoords(pos));
        }

        public IEnumerable<T> GetEntities(Vector3 pos, Vector3 movement)
        {
            return GetEntities(GetCoords(pos, movement, movement.Length()));
        }

        /// <summary>Not as efficient as <see cref="GetEntities(Vector3, Vector3)"/>.</summary>
        public IEnumerable<T> GetEntities(BoundingBox boundingBox, Vector3 movement)
        {
            return GetEntities(boundingBox.Expand(movement));
        }

        /// <summary>Not as efficient as <see cref="GetEntities(Vector3, Vector3)"/>.</summary>
        public IEnumerable<T> GetEntities(IBoundingBox boundingBoxObject, Vector3 movement)
        {
            var box = boundingBoxObject.GetBoundingBox();

            // if it's a dot, call a faster method
            if (box.Min == box.Max)
                return GetEntities(box.Min, movement);

            return GetEntities(box.Expand(movement));
        }

        public IEnumerable<T> GetEntities(IEnumerable<Int3> coords)
        {
            HashSet<T> done = new HashSet<T>();
            foreach (var c in coords)
            {
                foreach (var e in GetEntities(c))
                {
                    if (!done.Contains(e))
                    {
                        done.Add(e);
                        yield return e;
                    }
                }
            }
        }

        public IEnumerable<T> GetEntities(IEnumerable<Int3> coords, Func<Int3, bool> continueCondition)
        {
            HashSet<T> done = new HashSet<T>();
            foreach (var c in coords)
            {
                if (!continueCondition(c))
                    yield break;

                foreach (var e in GetEntities(c))
                {
                    if (!done.Contains(e))
                    {
                        done.Add(e);
                        yield return e;
                    }
                }
            }
        }

        /// <summary>rayDirection must not be normalized.</summary>
        public IEnumerable<Int3> GetCoords(Vector3 rayOrigin, Vector3 rayDirection, float rayLength = float.MaxValue)
        {
            float x = rayOrigin.X / FieldSize.X;
            float y = rayOrigin.Y / FieldSize.Y;
            float z = rayOrigin.Z / FieldSize.Z;
            float dx = rayDirection.X / FieldSize.X;
            float dy = rayDirection.Y / FieldSize.Y;
            float dz = rayDirection.Z / FieldSize.Z;
            float dxAbs = Math.Abs(dx);
            float dyAbs = Math.Abs(dy);
            float dzAbs = Math.Abs(dz);

            int cx = (int)Math.Floor(x);
            int cy = (int)Math.Floor(y);
            int cz = (int)Math.Floor(z);

            Int3 maxEnd = max + new Int3(1);



            while ((dx > 0 || x >= min.X) && (dy > 0 || y >= min.Y) && (dz > 0 || z >= min.Z)
                && (dx < 0 || x < maxEnd.X) && (dy < 0 || y < maxEnd.Y) && (dz < 0 || z < maxEnd.Z))
            {
                // parts inside the tile
                float x1 = x - cx;
                float y1 = y - cy;
                float z1 = z - cz;

                yield return new Int3(cx, cy, cz);

                if (rayLength <= 0)
                    yield break;

                float remainingX = dx >= 0 ? 1f - x1 : x1;
                float remainingY = dy >= 0 ? 1f - y1 : y1;
                float remainingZ = dz >= 0 ? 1f - z1 : z1;

                float timeToX = remainingX / dxAbs;
                float timeToY = remainingY / dyAbs;
                float timeToZ = remainingZ / dzAbs;

                if (timeToY < timeToX)
                {
                    if (timeToY < timeToZ)
                        MoveToY();
                    else
                        MoveToZ();
                }
                else
                {
                    if (timeToX < timeToZ)
                        MoveToX();
                    else
                        MoveToZ();
                }

                void MoveToX()
                {
                    Move(timeToX);
                    x = MathF.Round(x);
                    cx += Math.Sign(dx);
                }

                void MoveToY()
                {
                    Move(timeToY);
                    y = MathF.Round(y);
                    cy += Math.Sign(dy);
                }

                void MoveToZ()
                {
                    Move(timeToZ);
                    z = MathF.Round(z);
                    cz += Math.Sign(dz);
                }

                void Move(float time)
                {
                    x += time * dx;
                    y += time * dy;
                    z += time * dz;

                    rayLength -= time;
                }
            }
        }

        public IEnumerable<Int3> GetUsedCoords()
        {
            return Lists.Keys;
        }

        public IEnumerable<(T, T)> GetNearPairs()
        {
            HashSet<(T, T)> done = new HashSet<(T, T)>();

            foreach (Int3 c in GetUsedCoords())
            {
                foreach (var triA in this[c])
                {
                    foreach (var triB in this[c])
                    {
                        if (EqualityComparer<T>.Default.Equals(triA, triB)) // same triangle
                            continue;
                        if (!done.Contains((triB, triA)) && done.Add((triA, triB)))
                        {
                            yield return (triA, triB);
                        }
                    }
                }
            }
        }

        public IEnumerable<(T, T)> GetNearPairs(Func<T, T, bool> isPairable)
        {
            HashSet<(T, T)> done = new HashSet<(T, T)>();

            foreach (Int3 c in GetUsedCoords())
            {
                foreach (var triA in this[c])
                {
                    foreach (var triB in this[c])
                    {
                        if (EqualityComparer<T>.Default.Equals(triA, triB)) // same triangle
                            continue;
                        if (!isPairable(triA, triB))
                            continue;
                        if (!done.Contains((triB, triA)) && done.Add((triA, triB)))
                        {
                            yield return (triA, triB);
                        }
                    }
                }
            }
        }
    }
}
