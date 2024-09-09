﻿using System.Numerics;
using Raylib_cs;

namespace uniray_Project
{
    public struct MotionConstraint
    {
        // Default motion constraint object
        public static readonly MotionConstraint Default = new MotionConstraint()
        {
            Value = 1,
            Constraint = new Vector3(1f, 0f, 1f)
        };
        /// <summary>
        /// Intensity of the constraint
        /// </summary>
        public float Value { get; set;  }
        /// <summary>
        /// Constraint vector of the object
        /// </summary>
        public Vector3 Constraint {  get; set; }
        /// <summary>
        /// Calculate the value of the constraint 
        /// <param name="target">Target vector of the player</param>
        /// <param name="box">Bounding box to calculate with</param>
        /// </summary>
        public void Calculate(Vector3 target, BoundingBox box)
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
                Constraint = Vector3.UnitX;
            }
            // Object is facing X axis
            else
            {
                // Calculate constraint
                Value = Math.Abs(Raymath.Vector3DotProduct(target, Vector3.UnitZ));
                Constraint = Vector3.UnitZ;
            }
        }
    }
}