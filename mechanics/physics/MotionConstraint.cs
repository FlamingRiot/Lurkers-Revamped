using System.Numerics;
using Raylib_cs;

namespace Lurkers_revamped
{
    public struct MotionConstraint
    {
        /// <summary>
        /// Default MotionConstraint object
        /// </summary>
        public static readonly MotionConstraint Default = new()
        {
            Value = 1,
            Constraint = new Vector3(1f, 0f, 1f)
        };
        /// <summary>
        /// Intensity of the constraint
        /// </summary>
        public float Value { get; set; }
        /// <summary>
        /// Constraint vector of the object
        /// </summary>
        public Vector3 Constraint {  get; set; }
        /// <summary>
        /// Calculate the value of the constraint 
        /// <param name="target">Target vector of the player</param>
        /// <param name="box">Bounding box to calculate with</param>
        /// </summary>
        public void Calculate(Vector3 target, BoundingBox box, Vector3 position)
        {
            // Face Value Z
            float vz = Raymath.Vector3Subtract(new Vector3(box.Max.X, target.Y, box.Min.Z), new Vector3(box.Max.X, target.Y, box.Max.Z)).Z;
            // Face Value X
            float vx = Raymath.Vector3Subtract(new Vector3(box.Min.X, target.Y, box.Max.Z), new Vector3(box.Max.X, target.Y, box.Max.Z)).X;

            // Object is facing Z axis
            if (vz > vx)
            {
                // Calculate constraint
                Value = Math.Abs(Raymath.Vector3DotProduct(target, Vector3.UnitX));
                if (position.Z < ((box.Min + box.Max) / 2).Z)
                {
                    Constraint = Vector3.UnitX;
                }
                else
                {
                    Constraint = -Vector3.UnitX;
                }
            }
            // Object is facing X axis
            else
            {
                // Calculate constraint
                Value = Math.Abs(Raymath.Vector3DotProduct(target, Vector3.UnitZ));
                if (position.X < ((box.Min + box.Max) / 2).X)
                {
                    Constraint = Vector3.UnitZ;
                }
                else
                {
                    Constraint = -Vector3.UnitZ;
                }
            }
        }
    }
}