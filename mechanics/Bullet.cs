using Raylib_cs;
using System.Numerics;

namespace uniray_Project
{
    public class Bullet
    {
        /// <summary>
        /// The trajectory ray of the bullet
        /// </summary>
        private Ray ray;
        /// <summary>
        /// The collision informations of the bullet
        /// </summary>
        private RayCollision collision;
        /// <summary>
        /// The trajectory ray of the bullet
        /// </summary>
        public Ray Ray { get { return ray; } set { ray = value; } }
        /// <summary>
        /// The collision informations of the bullet
        /// </summary>
        public RayCollision Collision { get { return collision; } set { collision = value; } }
        /// <summary>
        /// Bullet constructor
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        public Bullet(Vector3 origin, Vector3 direction)
        {
            ray = new Ray(origin, direction);
            collision = new RayCollision();
        }
        /// <summary>
        /// Reset the collision of the bullet
        /// </summary>
        public void ResetCollision()
        {
            collision.Hit = false;
        }
    }
}