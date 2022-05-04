using Microsoft.Xna.Framework;

namespace BytingLib
{
    public class CollisionResult
    {
        public bool Collision { get; set; } // TODO: probably remove this variable, cause it's redundant cause of distance and distance reversed
        public float? Distance { get; set; }
        public float? DistanceReversed { get; set; }
        public Vector2 AxisCol { get; set; }
        public Vector2 AxisColReversed { get; set; }
        public int ColCornerPoly { get; set; }
        public int ColCornerIndex { get; set; }

        public override string ToString()
        {
            return "collision: " + Collision
                 + "\ndistance: " + Distance
                 + "\ndistanceReversed: " + DistanceReversed
                 + "\naxisCol: " + AxisCol
                 + "\naxisColReversed: " + AxisColReversed;
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

        public void SetCollisionFromDistance()
        {
            Collision = Math.Sign(Distance!.Value * DistanceReversed!.Value) == -1;
        }

        /// <summary>Combines the collision result into this one.</summary>
        public void Add(CollisionResult cr)
        {
            if (cr.Collision)
                Collision = true;

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
    }
}
