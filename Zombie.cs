using System.Numerics;


namespace uniray_Project
{
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
        /// The 4x4 matrix used to render the zombie
        /// </summary>
        private Matrix4x4 transform;
        /// <summary>
        /// The current animation of the zombie
        /// </summary>
        private Animation currentAnimation;
        /// <summary>
        /// The current health of the zombie
        /// </summary>
        public int Health { get { return health; } set { health = value; } }
        /// <summary>
        /// The type of the zombie
        /// </summary>
        public string Type { get { return type; } set { type = value; } }
        /// <summary>
        /// The 4x4 matrix used to render the zombie
        /// </summary>
        public Matrix4x4 Transform { get { return transform; } set { transform = value; } }
        /// <summary>
        /// The current animations of the zombie
        /// </summary>
        public Animation CurrentAnimation { get { return currentAnimation; } set { currentAnimation = value; } }
        /// <summary>
        /// The position of the zombie extracted from the matrix
        /// </summary>
        public Vector3 Position { get { return new Vector3(transform.M14, transform.M24, transform.M34); } set { transform.M14 = value.X;transform.M24 = value.Y; transform.M34 = value.Z; } }
        public Zombie(Vector3 position, string type)
        {
            // Define position
            transform = Matrix4x4.Identity;
            transform.M14 = position.X;
            transform.M24 = position.Y;
            transform.M34 = position.Z;
            // Define type
            this.type = type;
            // Define health bar
            switch (type)
            {
                case "cop":
                    health = 100;
                    break;
                case "officer":
                    health = 120;
                    break;
            }
        }
    }
}