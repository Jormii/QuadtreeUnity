﻿namespace Quadtree {

    public struct QuadtreeRegion {
        public readonly Vector2D center;
        public readonly Vector2D halfRegionSize;
        public readonly Vector2D leftUpperCorner;
        public readonly Vector2D rightLowerCorner;

        public QuadtreeRegion (Vector2D center, Vector2D halfRegionSize) {
            QuadtreeTest.AddDebugMessage ("QuadtreeRegion::QuadtreeRegion");
            this.center = center;
            this.halfRegionSize = halfRegionSize;

            this.leftUpperCorner = new Vector2D (
                center.x - halfRegionSize.x,
                center.y + halfRegionSize.y
            );

            this.rightLowerCorner = new Vector2D (
                center.x + halfRegionSize.x,
                center.y - halfRegionSize.y
            );
        }

        public bool ContainsPoint (Vector2D point) {
            QuadtreeTest.AddDebugMessage ("QuadtreeRegion::ContainsPoint");
            bool containsXComponent = point.x >= leftUpperCorner.x && point.x <= leftUpperCorner.x;
            bool containsYComponent = point.y <= leftUpperCorner.y && point.y >= leftUpperCorner.y;

            return containsXComponent && containsYComponent;
        }

        public override bool Equals (object obj) {
            QuadtreeTest.AddDebugMessage ("QuadtreeRegion::Equals");
            if (obj == null) {
                return false;
            }

            if (!GetType ().Equals (obj.GetType ())) {
                return false;
            }

            QuadtreeRegion otherRegion = (QuadtreeRegion) obj;
            return center.Equals (otherRegion.center) && halfRegionSize.Equals (otherRegion.halfRegionSize);
        }

        public override int GetHashCode () {
            QuadtreeTest.AddDebugMessage ("QuadtreeRegion::GetHashCode");
            return (center, halfRegionSize, leftUpperCorner, leftUpperCorner).GetHashCode ();
        }

        public override string ToString () {
            return string.Format ("O: {0}, HS: {1}", center.ToString (), halfRegionSize.ToString ());
        }

    }
}