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

            //this.value = Raymath.Vector3DotProduct(target, face);
        }
    }
}