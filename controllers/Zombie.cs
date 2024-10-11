using System.Numerics;
using static Raylib_cs.Raylib;
using Lurkers_revamped;
using Raylib_cs;

namespace uniray_Project
{
    /// <summary>Zombie action state system.</summary>
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

    /// <summary>Represents an instance of a <see cref="Zombie"/> object.</summary>
    public unsafe class Zombie
    {
        public const float SPEED = 5f;

        private int health;
        private string type;
        private float angle;
        private Matrix4x4 transform;
        private ModelAnimation currentAnimation;
        private Ray ray;
        private ZombieState state;

        /// <summary>The current health of the zombie.</summary>
        public int Frame;

        /// <summary>The current health of the zombie.</summary>
        public int Health { get { return health; } set { health = value; } }

        /// <summary>The rotation angle of the zombie.</summary>
        public float Angle { get { return angle; } set { angle = value; } }

        /// <summary>The type (model to use) of the zombie.</summary>
        public string Type { get { return type; } set { type = value; } }

        /// <summary>The <see cref="Matrix4x4"/> used for model transformations.</summary>
        public Matrix4x4 Transform { get { return transform; } set { transform = value; } }

        /// <summary>The action state of the zombie.</summary>
        public ZombieState State { get { return state; } set { state = value; } }

        /// <summary>The currently displayed animation of the zombie.</summary>
        public ModelAnimation CurrentAnimation { get { return currentAnimation; } set { currentAnimation = value; } }

        /// <summary>The zombie's player detection ray.</summary>
        public Ray VisionRay { get { return ray; } set { ray = value; } }

        /// <summary>The position of the zombie as a <see cref="Vector3"/>, extracted from a <see cref="Matrix4x4"/>.</summary>
        public Vector3 Position 
        { 
            get { return new Vector3(transform.M14, transform.M24, transform.M34); } 
            set { transform.M14 = value.X;transform.M24 = value.Y; transform.M34 = value.Z; } 
        }

        public float X
        {
            get { return transform.M14; }
            set { transform.M14 = value; }
        }

        public float Y
        {
            get { return transform.M24; }
            set { transform.M24 = value; }
        }

        public float Z
        {
            get { return transform.M34; }
            set { transform.M34 = value; }
        }

        /// <summary>Creates a zombie.</summary>
        /// <param name="position">Positon of the zombie.</param>
        /// <param name="type">Type of the zombie (what model to use).</param>
        /// <param name="anim">Starting animation to use for the zombie.</param>
        public Zombie(Vector3 position, string type, ModelAnimation anim)
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

        /// <summary>Updates the frame of current animation.</summary>
        public int UpdateFrame()
        {
            if (Frame == CurrentAnimation.FrameCount - 1)
            {
                Frame = 0;
            }
            else
            {
                Frame++;
            }
            return Frame;
        }

        /// <summary>Simulates a bullet collision on the zombie.</summary>
        /// <param name="ray">Bullet ray.</param>
        /// <param name="mesh">Mesh of the zombie</param>
        /// <returns><see langword="true"/> if collision succeeds. <see langword="false"/> otherwise.</returns>
        public bool Shoot(Ray ray, Mesh mesh)
        {
            // Calculates head bone position
            Vector3 bonePos = Program.RotateNormalizedBone(CurrentAnimation.FramePoses[Frame][5].Translation, Angle, Position);
            // Checks collision for the zombie's head
            RayCollision collisiion = GetRayCollisionSphere(ray, bonePos, 0.4f);
            if (collisiion.Hit)
            {
                // Start death animation (random)
                if (Random.Shared.Next(0, 2) == 1) State = ZombieState.Dying1;
                else State = ZombieState.Dying2;

                // Play headshot sounds
                AudioCenter.PlaySound("headshot");
                AudioCenter.StopSound("zombie_default");
                AudioCenter.PlaySound("zombie_kill");

                return true;
            }
            else return false;
        }
    }
}