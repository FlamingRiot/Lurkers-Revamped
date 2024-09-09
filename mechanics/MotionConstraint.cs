using System.Numerics;
using Raylib_cs;

namespace uniray_Project
{
    public struct MotionConstraint
    {
        /// <summary>
        /// Intensity of the constraint
        /// </summary>
        private float value;

        /// <summary>
        /// Calculate the value of the constraint 
        /// </summary>
        public void Calculate(Vector3 target, BoundingBox box)
        {
            // Face Vector Z
            float vz = Raymath.Vector3Subtract(new Vector3(box.Max.X, target.Y, box.Min.Z), new Vector3(box.Max.X, target.Y, box.Max.Z)).Z;
            float vx = Raymath.Vector3Subtract(new Vector3(box.Min.X, target.Y, box.Max.Z), new Vector3(box.Max.X, target.Y, box.Max.Z)).X;

            // Object is facing Z axis
            if (vz > vx)
            {
                // Calculate constraint
                this.value = Raymath.Vector3DotProduct(target, Vector3.UnitX);
            }
            // Object is facing X axis
            else
            {
                // Calculate constraint
                this.value = Raymath.Vector3DotProduct(target, Vector3.UnitZ);
            }

            //this.value = Raymath.Vector3DotProduct(target, face);
        }
    }
}