using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using Raylib_cs;

namespace uniray_Project
{
    /// <summary>
    /// The state of the zombie
    /// </summary>
    public enum ZombieState
    {
        Idle,
        Running,
        Attacking,
        Dying1,
        Dying2,
        Crawling,
        Killing
    }

    public class Zombie
    {
        /// <summary>
        /// The current health of the zombie
        /// </summary>
        private int health;
        /// <summary>
        /// The type of the zombie
        /// </summary>
        private string type;
        /// <summary>
        /// The angle rotation of the model
        /// </summary>
        private float angle;
        /// <summary>
        /// The 4x4 matrix used to render the zombie
        /// </summary>
        private Matrix4x4 transform;
        /// <summary>
        /// The current animation of the zombie
        /// </summary>
        private Animation currentAnimation;
        /// <summary>
        /// The zombie's player detection ray
        /// </summary>
        private Ray ray;
        /// <summary>
        /// The state of the zombie
        /// </summary>
        private ZombieState state;
        /// <summary>
        /// The current health of the zombie
        /// </summary>
        public int Health { get { return health; } set { health = value; } }
        /// <summary>
        /// The angle rotation of the model
        /// </summary>
        public float Angle { get { return angle; } set { angle = value; } }
        /// <summary>
        /// The type of the zombie
        /// </summary>
        public string Type { get { return type; } set { type = value; } }
        /// <summary>
        /// The 4x4 matrix used to render the zombie
        /// </summary>
        public Matrix4x4 Transform { get { return transform; } set { transform = value; } }
        /// <summary>
        /// The state of the zombie
        /// </summary>
        public ZombieState State { get { return state; } set { state = value; } }
        /// <summary>
        /// The current animations of the zombie
        /// </summary>
        public Animation CurrentAnimation { get { return currentAnimation; } set { currentAnimation = value; } }
        /// <summary>
        /// The zombie's player detection ray
        /// </summary>
        public Ray VisionRay { get { return ray; } set { ray = value; } }
        /// <summary>
        /// The position of the zombie extracted from the matrix
        /// </summary>
        public Vector3 Position { get { return new Vector3(transform.M14, transform.M24, transform.M34); } set { transform.M14 = value.X;transform.M24 = value.Y; transform.M34 = value.Z; } }
        
        /// <summary>Creates a zombie.</summary>
        /// <param name="position">Positon of the zombie.</param>
        /// <param name="type">Type of the zombie (what model to use).</param>
        /// <param name="anim">Starting animation to use for the zombie.</param>
        public Zombie(Vector3 position, string type, Animation anim)
        {
            // Define position
            transform = Matrix4x4.Identity;
            transform.M14 = position.X;
            transform.M24 = position.Y;
            transform.M34 = position.Z;
            // Define rotation
            Angle = 0;
            // Define type
            this.type = type;
            // Define health bar according to the type of the zombie
            switch (type)
            {
                case "cop":
                    health = 100;
                    break;
                case "officer":
                    health = 120;
                    break;
            }

            // Set default state
            state = ZombieState.Running;

            // Set current animation of the zombie
            currentAnimation = anim;
        }

        public void Shoot()
        {

        }
    }
}