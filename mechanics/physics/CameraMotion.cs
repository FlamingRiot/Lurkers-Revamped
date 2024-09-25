/* 
    Author: Evan Comtesse
    Last modified: 25.09.2024
    Description: Represents a handful of camera motion data relative to the game
 */

using System.Numerics;
using Raylib_cs;

namespace Lurkers_revamped 
{ 
    /// <summary>Represents a handful of camera motion data relative to the game</summary> 
    public struct CameraMotion
    {
        /// <summary>Yaw angle of the camera</summary> 
        public float Yaw;

        /// <summary>Pitch angle of the camera</summary> 
        public float Pitch;

        /// <summary>Camera SideShake movement</summary> 
        public float SideShake;

        /// <summary>Camera shake amplitude</summary> 
        public float Amplitude;

        /// <summary>Camera shake frequency</summary> 
        public float Frequency;

        /// <summary>Camera shake Toggle Speed</summary> 
        public float ToggleSpeed;

        /// <summary>Camera shake starting point</summary> 
        public Vector3 ShakeStart;

        /// <summary>Creates a <see cref="CameraMotion"/> object to use with a <see cref="Raylib_cs.Camera3D"/></summary> 
        /// <param name="amplitude">Camera shake amplitude</param>
        /// <param name="frequency">Camera shake frequency</param>
        /// <param name="toggleSpeed">Camera shake Toggle speed</param>
        public CameraMotion(float amplitude, float frequency, float toggleSpeed)
        {
            Yaw = 0;
            Pitch = 0;
            SideShake = 0;

            // Camera movement shake
            Amplitude = amplitude;
            Frequency = frequency;
            ToggleSpeed = toggleSpeed;
            ShakeStart = Vector3.Zero;
        }

        /// <summary>Updates the camera shake</summary>
        /// <param name="position">Actual position of the camera</param>
        /// <param name="speed">Player speed</param>
        /// <returns>The shaken camera position</returns>
        public Vector3 Update(Vector3 position, float speed)
        {
            position += CheckMotion(speed);
            return ResetPosition(position);
        }

        /// <summary>Moves camera around</summary>
        /// <returns>The moved position</returns>
        private Vector3 FootStepMotion()
        {
            Vector3 pos = Vector3.Zero;
            pos.Y += MathF.Sin((float)Raylib.GetTime() * Frequency) * Amplitude;
            pos.X += MathF.Sin((float)Raylib.GetTime() * Frequency / 2) * Amplitude * 2;
            pos.Z += MathF.Cos((float)Raylib.GetTime() * Frequency / 2) * Amplitude * 2;
            return pos;
        }

        /// <summary>Resets the position of the camera to start a new cycle</summary>
        /// <param name="position">Actual position of the camera</param>
        /// <returns>The new position after reset started</returns>
        private Vector3 ResetPosition(Vector3 position)
        {
            if (position == ShakeStart) return position;
            return Raymath.Vector3Lerp(position, ShakeStart, 1 * Raylib.GetFrameTime());
        }

        /// <summary>Checks if player is moving to perform camera shake</summary>
        /// <param name="speed">Player actual speed</param>
        /// <returns>The shaken camera position</returns>
        private Vector3 CheckMotion(float speed)
        {
            if (speed < ToggleSpeed) return Vector3.Zero;

            return FootStepMotion();
        }
    }
}