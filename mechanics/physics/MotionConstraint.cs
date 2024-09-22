using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using Raylib_cs;

namespace Lurkers_revamped
{
    /// <summary>Represents a constraint object used to influence controllers' movement</summary>
    public struct MotionConstraint : IEquatable<MotionConstraint>
    {
        /// <summary>Default motion constraint object</summary>
        public static readonly MotionConstraint Default = new()
        {
            Id = -1,
            Value = 1,
            Constraint = new Vector3(1f, 0f, 1f)
        };

        /// <summary>Id of the current instance</summary>
        public int Id;

        /// <summary>Intensity of the constraint</summary>
        public float Value;

        /// <summary>Constraint vector of the object</summary>
        public Vector3 Constraint;

        /// <summary>Returns a basic motion constraint object without a valid ID</summary>
        public MotionConstraint()
        {
            this.Id = -1;
            this.Value = 1f;
            this.Constraint = new Vector3(1f, 0f, 1f);
        }

        /// <summary>Calculates the value of a constraint</summary>
        /// <param name="target">Target vector of the player</param>
        /// <param name="box">Bounding box to calculate with</param>
        public void Calculate(Vector3 target, BoundingBox box, Vector3 position)
        {
            if (position.Z < box.Max.Z && position.Z > box.Min.Z)
            {
                Value = Math.Abs(Raymath.Vector3DotProduct(target, Vector3.UnitZ));

                if (position.X < box.Min.X)
                {
                    Constraint = Vector3.UnitZ;
                }
                else
                {
                    Constraint = -Vector3.UnitZ;
                }
            }
            else if (position.X < box.Max.X && position.X > box.Min.X)
            {
                Value = Math.Abs(Raymath.Vector3DotProduct(target, Vector3.UnitX));

                if (position.Z < box.Min.Z)
                {
                    Constraint = Vector3.UnitX;
                }
                else
                {
                    Constraint = -Vector3.UnitX;
                }
            }
        }

        /// <summary>Resets the motion constraint to default value</summary>
        public void Reset()
        {
            this = Default;
        }

        /// <summary>Returns a value that indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true" /> if the current instance and <paramref name="obj" /> are equal; otherwise, <see langword="false" />. If <paramref name="obj" /> is <see langword="null" />, the method returns <see langword="false" />.</returns>
        /// <remarks>The current instance and <paramref name="obj" /> are equal if <paramref name="obj" /> is a <see cref="MotionConstraint" /> object and the corresponding elements of each constraint are equal.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly bool Equals([NotNullWhen(true)] object? obj)
        {
            return (obj is MotionConstraint other) && Equals(other);
        }

        /// <summary>Returns a value that indicates whether this instance and another 4x4 matrix are equal.</summary>
        /// <param name="other">The other matrix.</param>
        /// <returns><see langword="true" /> if the two matrices are equal; otherwise, <see langword="false" />.</returns>
        public readonly bool Equals(MotionConstraint other)
        {
            return this == other;
        }

        /// <summary>Returns a value that indicates whether the constraints are equal.</summary>
        /// <param name="left">First constraint to compare</param>
        /// <param name="right">Second constraint to compare</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> equals <paramref name="right"/>. Otherwise, <see langword="false"/>.</returns>
        /// <remarks>Two motion constraints are equal if their value and vector constraint are the same</remarks>
        public static bool operator ==(MotionConstraint left, MotionConstraint right) 
        {
            return left.Value == right.Value && left.Constraint == right.Constraint;
        }

        /// <summary>Returns a value that indicates whether the constraints are not equal.</summary>
        /// <param name="left">First constraint to compare</param>
        /// <param name="right">Second constraint to compare</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/>are not equal. Otherwise, <see langword="false"/>.</returns>
        /// <remarks>Two motion constraints are equal if their value and vector constraint are the same</remarks>
        public static bool operator !=(MotionConstraint left, MotionConstraint right) 
        { 
            return left.Value != right.Value || left.Constraint != right.Constraint;
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            hash.Add(Value);
            hash.Add(Constraint);

            return hash.ToHashCode();
        }
    }
}