/* 
    Author: Evan Comtesse
    Last modified: 16.09.2024
    Description: Represents a handful of camera motion data relative to the game
 */

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

        /// <summary>Creates a <see cref="CameraMotion"/> object to use with a <see cref="Raylib_cs.Camera3D"/></summary> 
        public CameraMotion()
        {
            Yaw = 0;
            Pitch = 0;
            SideShake = 0;
        }
    }
}
