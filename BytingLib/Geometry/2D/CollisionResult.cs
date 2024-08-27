namespace BytingLib
{
    public class CollisionResult : AnyCollisionResult
    {
        public Vector2 AxisCol;
        public Vector2 AxisColReversed;
        public int ColCornerPoly;
        public int ColCornerIndex;

        public override string ToString()
        {
            return
$@"Distance: {Distance}
DistanceReversed: {DistanceReversed}
AxisCol: {AxisCol}
AxisColReversed: {AxisColReversed}
ColCornerPoly: {ColCornerPoly}
ColCornerIndex: {ColCornerIndex}";
        }

        public void AxisInvert()
        {
            AxisCol = -AxisCol;
            AxisColReversed = -AxisColReversed;
        }

        public CollisionResult InvertAxisAndReturn()
        {
            AxisInvert();
            return this;
        }

        /// <summary>Combines the collision result into this one.</summary>
        public void Add(CollisionResult cr)
        {
            if (cr.DistanceReversed.HasValue
                && (!DistanceReversed.HasValue || cr.DistanceReversed > DistanceReversed))
            {
                CopyBackwardValues(cr);
            }

            if (cr.Distance.HasValue
                && (!Distance.HasValue || cr.Distance < Distance))
            {
                CopyForwardValues(cr);
            }
        }

        private void CopyBackwardValues(CollisionResult cr)
        {
            DistanceReversed = cr.DistanceReversed;
            AxisColReversed = cr.AxisColReversed;
        }

        private void CopyForwardValues(CollisionResult cr)
        {
            Distance = cr.Distance;
            AxisCol = cr.AxisCol;
            ColCornerPoly = cr.ColCornerPoly;
            ColCornerIndex = cr.ColCornerIndex;
        }

        /// <summary>Applies the values of cr to this instance, that result in a larger collision shape.</summary>
        public bool MinResult(CollisionResult cr)
        {
            if (cr.DistanceReversed.HasValue &&
                (!DistanceReversed.HasValue || cr.DistanceReversed > DistanceReversed))
            {
                CopyBackwardValues(cr);
            }

            if (cr.Distance.HasValue &&
                (!Distance.HasValue || cr.Distance < Distance))
            {
                CopyForwardValues(cr);

                return true;
            }
            return false;
        }


        public bool MinResultIfCollisionInPresentOrFuture(CollisionResult cr)
        {
            if (cr.DistanceReversed.HasValue)
            {
                // is collision in positive distance?
                // or is a collision happening right now?
                if (cr.DistanceReversed >= 0)
                {
                    return MinResult(cr);
                }
            }
            return false;
        }
    }
}
