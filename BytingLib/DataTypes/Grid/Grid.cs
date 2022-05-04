using Microsoft.Xna.Framework;

namespace BytingLib
{
    public class Grid<T> where T : IBoundingRectangle
    {
        public float FieldSize { get; private set; }
        public Dictionary<Int2, List<T>> Lists { get; private set; } = new Dictionary<Int2, List<T>>();

        public bool BoundsSet { get; private set; } = false;

        private int minX, minY, maxX, maxY;

        public int MinX => minX;
        public int MinY => minY;
        public int MaxX => maxX;
        public int MaxY => maxY;

        private Int2 Min => new Int2(minX, minY);
        private Int2 Max => new Int2(maxX, maxY);

        public Grid(float fieldSize)
        {
            FieldSize = fieldSize;
        }

        public virtual void Add(T entity)
        {
            AddToGrid(entity);
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

        public IEnumerable<Int2> GetCoords(T entity)
        {
            return GetCoords(entity.GetBoundingRectangle());
        }

        public IEnumerable<T> GetEntities(IEnumerable<Int2> coords)
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

        public IEnumerable<T> GetEntities(IEnumerable<Int2> coords, Func<Int2, bool> continueCondition)
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

        public IEnumerable<T> GetEntities(T searchingEntity, Vector2 movement)
        {
            return GetEntities(searchingEntity.GetBoundingRectangle(), movement);
        }
        public IEnumerable<T> GetEntities(Rect boundingBox, Vector2 movement)
        {
            return GetEntities(new Rect(boundingBox).Expand(movement));
        }
        public IEnumerable<T> GetEntities(Rect boundingBox)
        {
            return GetEntities(GetCoords(boundingBox));
        }

        public IEnumerable<Int2> GetCoords(Rect boundingBox)
        {
            int x1 = (int)Math.Floor(boundingBox.Left / FieldSize);
            int y1 = (int)Math.Floor(boundingBox.Top / FieldSize);
            int x2 = (int)Math.Ceiling(boundingBox.Right / FieldSize);
            int y2 = (int)Math.Ceiling(boundingBox.Bottom / FieldSize);

            Int2 c = new Int2(x1, y1);
            for (; c.Y < y2; c.Y++)
            {
                for (c.X = x1; c.X < x2; c.X++)
                {
                    yield return c;
                }
            }
        }

        private void AddToCoord(Int2 c, T entity)
        {
            if (Lists.TryGetValue(c, out List<T> list))
            {
                list.Add(entity);
            }
            else
            {
                Lists.Add(c, new List<T>() { entity });

                if (!BoundsSet)
                {
                    minX = maxX = c.X;
                    minY = maxY = c.Y;
                    BoundsSet = true;
                }
                else
                {
                    if (c.X < minX)
                        minX = c.X;
                    else if (c.X > maxX)
                        maxX = c.X;

                    if (c.Y < minY)
                        minY = c.Y;
                    else if (c.Y > maxY)
                        maxY = c.Y;
                }
            }
        }
        private bool RemoveFromCoord(Int2 c, T entity)
        {
            if (Lists.TryGetValue(c, out List<T> list))
            {
                list.Remove(entity);

                // if list is empty, remove entry from dictionary
                if (list.Count == 0)
                    Lists.Remove(c);

                return true;
            }
            return false;
        }

        public IEnumerable<T> this[Int2 c] => GetEntities(c);
        public IEnumerable<T> this[int x, int y] => GetEntities(x, y);
        public IEnumerable<T> GetEntities(int x, int y) => GetEntities(new Int2(x, y));
        public IEnumerable<T> GetEntities(Int2 c)
        {
            if (Lists.TryGetValue(c, out List<T> list))
                return list.AsEnumerable();
            else
                return Enumerable.Empty<T>();
        }

        public bool CoordContainsAny(Int2 c) => Lists.ContainsKey(c);

        public IEnumerable<Int2> GetCoords(Vector2 rayOrigin, Vector2 rayDirection)
        {
            // TODO: get level bounds

            float x = rayOrigin.X / FieldSize;
            float y = rayOrigin.Y / FieldSize;
            float dx = rayDirection.X;
            float dy = rayDirection.Y;
            float dxAbs = Math.Abs(rayDirection.X);
            float dyAbs = Math.Abs(rayDirection.Y);

            int cx = (int)Math.Floor(x);
            int cy = (int)Math.Floor(y);

            while (x >= minX && y >= minY && x < maxX + 1 && y < maxY + 1)
            {
                // parts inside the tile
                float x1 = x - cx;
                float y1 = y - cy;

                yield return new Int2(cx, cy);

                float remainingX = dx >= 0 ? 1f - x1 : x1;
                float remainingY = dy >= 0 ? 1f - y1 : y1;

                float timeToX = remainingX / dxAbs;
                float timeToY = remainingY / dyAbs;

                if (timeToY < timeToX)
                {
                    // move to Y
                    x += timeToY * dx;
                    y += timeToY * dy;
                    y = MathF.Round(y);
                    cy += Math.Sign(dy);
                }
                else
                {
                    // move to X
                    x += timeToX * dx;
                    y += timeToX * dy;
                    x = MathF.Round(x);
                    cx += Math.Sign(dx);
                }

            }
        }

        public virtual IEnumerable<T> GetEntities()
        {
            return GetEntities(GetUsedCoords());
        }

        public IEnumerable<T> GetEntities(Vector2 pos, Vector2 movement)
        {
            return GetEntities(GetCoords(pos, movement, movement.Length()));
        }
        /// <summary>rayDirection must not be normalized.</summary>
        public IEnumerable<Int2> GetCoords(Vector2 rayOrigin, Vector2 rayDirection, float rayLength = float.MaxValue)
        {
            float x = rayOrigin.X / FieldSize;
            float y = rayOrigin.Y / FieldSize;
            float dx = rayDirection.X / FieldSize;
            float dy = rayDirection.Y / FieldSize;
            float dxAbs = Math.Abs(dx);
            float dyAbs = Math.Abs(dy);

            int cx = (int)Math.Floor(x);
            int cy = (int)Math.Floor(y);

            Int2 maxEnd = Max + new Int2(1);



            while ((dx > 0 || x >= Min.X) && (dy > 0 || y >= Min.Y)
                && (dx < 0 || x < maxEnd.X) && (dy < 0 || y < maxEnd.Y))
            {
                // parts inside the tile
                float x1 = x - cx;
                float y1 = y - cy;

                yield return new Int2(cx, cy);

                if (rayLength <= 0)
                    yield break;

                float remainingX = dx >= 0 ? 1f - x1 : x1;
                float remainingY = dy >= 0 ? 1f - y1 : y1;

                float timeToX = remainingX / dxAbs;
                float timeToY = remainingY / dyAbs;

                if (timeToY < timeToX)
                    MoveToY();
                else
                    MoveToX();

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

                void Move(float time)
                {
                    x += time * dx;
                    y += time * dy;

                    rayLength -= time;
                }
            }
        }

        public IEnumerable<Int2> GetUsedCoords()
        {
            return Lists.Keys;
        }

        public IEnumerable<(T, T)> GetNearPairs()
        {
            HashSet<(T, T)> done = new HashSet<(T, T)>();

            foreach (Int2 c in GetUsedCoords())
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

            foreach (Int2 c in GetUsedCoords())
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
