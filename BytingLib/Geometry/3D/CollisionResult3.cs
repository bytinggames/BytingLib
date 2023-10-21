namespace BytingLib
{
    public class CollisionResult3 : AnyCollisionResult
    {
        public Vector3 AxisCol, AxisColReversed;
        /// <summary>
        /// is calculated by letting one shape move towards another shape, that does not move.
        /// </summary>
        public Vector3? ColPoint;

        /// <summary>
        /// -1: none
        /// 0,1,2: vertex.
        /// 3,4,5: edge.
        /// 6: face</summary>
        public int ColTriangleIndex = -1;

        public void AxisInvert()
        {
            AxisCol = -AxisCol;
            AxisColReversed = -AxisColReversed;
            ColPoint = null; // use AxisInvert(Vector3) for keeping colPoint
        }

        public void AxisInvert(Vector3 newDir)
        {
            // set colPoint to the collision of the other shape
            if (Distance.HasValue)
            {
                if (ColPoint.HasValue)
                {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                    ColPoint += newDir * Distance.Value;
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                }
            }
            else
            {
                ColPoint = null;
            }

            AxisCol = -AxisCol;
            AxisColReversed = -AxisColReversed;
        }

        public CollisionResult3 GetAxisInvert()
        {
            AxisInvert();
            return this;
        }

        public CollisionResult3 GetAxisInvert(Vector3 newDir)
        {
            AxisInvert(newDir);
            return this;
        }

        public override string ToString()
        {
            return 
$@"Distance: {Distance}
DistanceReversed: {DistanceReversed}
AxisCol: {AxisCol}
AxisColReversed: {AxisColReversed}
ColPoint: {ColPoint}
ColTriangleIndex: {ColTriangleIndex}";
        }

        /// <summary>Applies the values of cr to this instance, that result in a larger collision shape.</summary>
        public bool MinResult(CollisionResult3 cr)
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

        /// <summary>Applies the values of cr to this instance, that result in a larger collision shape.</summary>
        public bool MaxResult(CollisionResult3 cr)
        {
            if (cr.DistanceReversed.HasValue &&
                (!DistanceReversed.HasValue || cr.DistanceReversed < DistanceReversed))
            {
                CopyBackwardValues(cr);
            }

            if (cr.Distance.HasValue &&
                (!Distance.HasValue || cr.Distance > Distance))
            {
                CopyForwardValues(cr);

                return true;
            }
            return false;
        }

        /// <summary>Used for triangle distance checking</summary>
        internal void ApplyUnionTakeNormalAsReversed(CollisionResult3 cr)
        {
            if (cr.Distance.HasValue)
            {
                if (!Distance.HasValue || cr.Distance.Value < Distance.Value)
                {
                    CopyForwardValues(cr);
                }

                if (!DistanceReversed.HasValue || cr.Distance.Value > DistanceReversed.Value)
                {
                    DistanceReversed = cr.Distance;
                    AxisColReversed = cr.AxisCol;
                }
            }
        }

        private void CopyForwardValues(CollisionResult3 cr)
        {
            Distance = cr.Distance;
            AxisCol = cr.AxisCol;
            ColPoint = cr.ColPoint;
            ColTriangleIndex = cr.ColTriangleIndex;
        }

        private void CopyBackwardValues(CollisionResult3 cr)
        {
            DistanceReversed = cr.DistanceReversed;
            AxisColReversed = cr.AxisColReversed;
        }

        public bool MinResultIfCollisionInPresentOrFuture(CollisionResult3 cr)
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
