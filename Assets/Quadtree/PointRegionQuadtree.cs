﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Quadtree {

    /// <summary>
    /// Implements a region quadtree.
    /// </summary>
    public class PointRegionQuadtree<T> : IQuadtree<T> {

        private readonly uint depth;
        private readonly uint maximumDepth;
        private readonly uint bucketSize;
        private readonly QRegion region;
        private readonly PointRegionQuadtree<T>[] children = new PointRegionQuadtree<T>[4];
        private readonly Dictionary<QVector2D, T> data = new Dictionary<QVector2D, T> ();

        /// <summary>
        /// Initializes a new instance of the RegionQuadtree class.
        /// </summary>
        /// <param name="maximumDepth">Maximum depth the quadtree may reach.</param>
        /// <param name="bucketSize">Bucket size of the quadtree.</param>
        /// <param name="region">Initial region of the quadtree.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public PointRegionQuadtree (uint maximumDepth, uint bucketSize, QRegion region) : this (0, maximumDepth, bucketSize, region) { }

        /// <summary>
        /// Initializes a new instance of the RegionQuadtree class.
        /// </summary>
        /// <param name="depth">Depth of this node.</param>
        /// <param name="maximumDepth">Maximum depth the quadtree may reach.</param>
        /// <param name="bucketSize">Bucket size of the quadtree.</param>
        /// <param name="region">Region of the quadtree.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private PointRegionQuadtree (uint depth, uint maximumDepth, uint bucketSize, QRegion region) {
            this.depth = depth;
            this.maximumDepth = maximumDepth;
            this.bucketSize = bucketSize;
            this.region = region;
        }

        public bool InsertPoint (QVector2D point, T pointData) {
            if (IsLeaf) {
                if (!region.ContainsPoint (point) || data.ContainsKey (point)) {
                    return false;
                }

                data.Add (point, pointData);
                if (data.Count == bucketSize && depth != maximumDepth) {
                    Subdivide ();
                }
                return true;
            }

            return InsertInChild (point, pointData);
        }

        /// <summary>
        /// Inserts the given point in the corresponding child.
        /// </summary>
        /// <returns>True, if the point was inserted, False otherwise.</returns>
        /// <param name="point">The point to insert.</param>
        /// <param name="pointData">The data associated with this point.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private bool InsertInChild (QVector2D point, T pointData) {
            if (children[0].region.ContainsPoint (point)) {
                return children[0].InsertPoint (point, pointData);
            }
            if (children[1].region.ContainsPoint (point)) {
                return children[1].InsertPoint (point, pointData);
            }
            if (children[2].region.ContainsPoint (point)) {
                return children[2].InsertPoint (point, pointData);
            }
            if (children[3].region.ContainsPoint (point)) {
                return children[3].InsertPoint (point, pointData);
            }

            return false;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool ContainsPoint (QVector2D point) {
            if (IsLeaf) {
                return data.ContainsKey (point);
            }

            if (children[0].ContainsPoint (point)) {
                return true;
            }
            if (children[1].ContainsPoint (point)) {
                return true;
            }
            if (children[2].ContainsPoint (point)) {
                return true;
            }
            if (children[3].ContainsPoint (point)) {
                return true;
            }

            return false;
        }

        public void Subdivide () {
            children[0] = new PointRegionQuadtree<T> (
                depth + 1,
                maximumDepth,
                bucketSize,
                CalculateChildRegion ((QQuadrant) 0)
            );
            children[1] = new PointRegionQuadtree<T> (
                depth + 1,
                maximumDepth,
                bucketSize,
                CalculateChildRegion ((QQuadrant) 1)
            );
            children[2] = new PointRegionQuadtree<T> (
                depth + 1,
                maximumDepth,
                bucketSize,
                CalculateChildRegion ((QQuadrant) 2)
            );
            children[3] = new PointRegionQuadtree<T> (
                depth + 1,
                maximumDepth,
                bucketSize,
                CalculateChildRegion ((QQuadrant) 3)
            );

            foreach (KeyValuePair<QVector2D, T> entry in data) {
                InsertInChild (entry.Key, entry.Value);
            }
            data.Clear ();
        }

        /// <summary>
        /// Calculates the region of the child associated to the given quadrant.
        /// </summary>
        /// <returns>The child region.</returns>
        /// <param name="quadrant">The quadrant of the child.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private QRegion CalculateChildRegion (QQuadrant quadrant) {
            QVector2D childHalfRegion = .5f * region.halfRegionSize;

            return new QRegion (
                CalculateChildCenter (quadrant, childHalfRegion),
                childHalfRegion
            );
        }

        /// <summary>
        /// Calculates the center of the child's region.
        /// </summary>
        /// <returns>The child's region center.</returns>
        /// <param name="quadrant">The quadrant of the child.</param>
        /// <param name="childHalfRegion">Child's region's half size.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private QVector2D CalculateChildCenter (QQuadrant quadrant, QVector2D childHalfRegion) {
            float xSign = (quadrant.XComponentIsPositive ()) ? 1 : -1;
            float ySign = (quadrant.YComponentIsPositive ()) ? 1 : -1;
            return new QVector2D (
                region.center.x + xSign * childHalfRegion.x,
                region.center.y + ySign * childHalfRegion.y
            );
        }

        public IQuadtree<T> GetChild (QQuadrant quadrant) {
            if (quadrant == QQuadrant.NumberOfQuadrants) {
                throw new System.Exception ("Argument cannot be \"NumberOfRegions\"");
            }

            if (IsLeaf) {
                throw new System.Exception ("Quadtree is leaf");
            }

            return children[(int) quadrant];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void GetLeafNodes (List<IQuadtree<T>> outputList) {
            if (IsLeaf) {
                outputList.Add (this);
            } else {
                children[0].GetLeafNodes (outputList);
                children[1].GetLeafNodes (outputList);
                children[2].GetLeafNodes (outputList);
                children[3].GetLeafNodes (outputList);
            }
        }

        public override bool Equals (object obj) {
            if (obj == null) {
                return false;
            }

            if (!GetType ().Equals (obj.GetType ())) {
                return false;
            }

            PointRegionQuadtree<T> otherQuadtree = (PointRegionQuadtree<T>) obj;
            return bucketSize == otherQuadtree.bucketSize &&
                data.Equals (otherQuadtree.data) &&
                depth == otherQuadtree.depth &&
                region.Equals (otherQuadtree.region);
        }

        public override int GetHashCode () {
            return (bucketSize, data, depth, IsLeaf, region).GetHashCode ();
        }

        public override string ToString () {
            string tabs = PrintTabs ();
            return string.Format ("{0}PRQ. Depth: {1}. Region: [{2}].\n{3}Data: {4}.\n{5}Children: {6}\n",
                tabs, depth, region, tabs, PrintData (), tabs, PrintChildren ());
        }

        private string PrintTabs () {
            StringBuilder tabs = new StringBuilder ();
            for (int i = 0; i < depth; ++i) {
                tabs.Append ('\t');
            }

            return tabs.ToString ();
        }

        private string PrintData () {
            StringBuilder dataString = new StringBuilder ("{ ");
            if (IsLeaf) {
                foreach (KeyValuePair<QVector2D, T> entry in data) {
                    dataString.AppendFormat ("[{0} => {1}]; ", entry.Key, entry.Value);
                }
            }
            dataString.Append (" }");

            return dataString.ToString ();
        }

        private string PrintChildren () {
            if (!IsLeaf) {
                StringBuilder childrenString = new StringBuilder ("[");
                foreach (PointRegionQuadtree<T> child in children) {
                    childrenString.AppendFormat ("\n{0}", child);
                }
                childrenString.Append (PrintTabs ());
                childrenString.Append ("]");
                return childrenString.ToString ();
            }

            return "No children";
        }

        /*
        Properties
        */

        public bool IsLeaf {
            get => children[0] == null;
        }

        public uint Depth {
            get => depth;
        }

        public uint MaximumDepth {
            get => maximumDepth;
        }

        public uint BucketSize {
            get => bucketSize;
        }

        public QRegion Region {
            get => region;
        }

        public ReadOnlyDictionary<QVector2D, T> Data {
            get => new ReadOnlyDictionary<QVector2D, T> (data);
        }
    }
}