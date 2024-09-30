using static Raylib_cs.Raylib;
using Raylib_cs;using System.Numerics;
using static UnirayEngine.UnirayEngine;

namespace uniray_Project.mechanics
{
    public class Spawner
    {
        /// <summary>Quad used to render the wave</summary>
        public static Mesh WaveQuad = GenMeshPlane(10, 10, 1, 1);

        private int RessourceIndex;

        /// <summary>Position of the spawner.</summary>
        public Vector3 Position;

        /// <summary>Transform of the spawner.</summary>
        public Matrix4x4 Transform;

        /// <summary>Spawn rate of the spawner.</summary>
        public float SpawnRate;

        /// <summary>Radius of action of the spawner.</summary>
        public float Radius;

        /// <summary>Health of the spawner.</summary>
        public int Health;

        /// <summary>Creates a spawner object.</summary>
        /// <param name="position">Position of the spawner.</param>
        /// <param name="spawnRate">Spawn rate of the spawner</param>
        /// <param name="radius">Radius of action of the spawner.</param>
        public Spawner(Vector3 position, Matrix4x4 transform, float spawnRate, float radius, int index)
        {
            Transform = transform;
            Position = position;
            SpawnRate = spawnRate;
            Radius = radius;
            Health = 500;

            RessourceIndex = index;
        }
        
        /// <summary>Creates a zombie and returns it.</summary>
        /// <param name="anim">Zombie animation.</param>
        /// <returns>Created zombie</returns>
        public Zombie CreateZombie(ModelAnimation anim, Vector3 playerPosition)
        {
            // Randomize position
            Vector3 diff = Raymath.Vector3Normalize(Raymath.Vector3Subtract(playerPosition, Position));
            Vector3 position = new Vector3(Position.X + diff.X * 10, 0, Position.Z + diff.Z * 10);
            // Create zombie
            Zombie zombzomb = new Zombie(position, "cop", anim);
            // Return zombie
            return zombzomb;
        }

        /// <summary>Simulates a bullet collision on a spawner</summary>
        /// <param name="ray">Bullet ray</param>
        /// <param name="mesh">Mesh of the spawner</param>
        /// <returns><see langword="true"/> if collision succeeds. <see langword="false"/> otherwise.</returns>
        public bool Shoot(Ray ray, Mesh mesh)
        {
            // Retrive collisiion informations
            RayCollision collision = GetRayCollisionMesh(ray, mesh, Transform);
            if (collision.Hit)
            {
                Health -= 25;
                //CurrentScene.GameObjects[RessourceIndex].Position -= Vector3.UnitY * 0.5f;
                // Destroy crystal if health = 0
                if (Health <= 0)
                {
                    Destroy();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>Destroys a spawner with a beautiful firework (hopefully).</summary>
        public void Destroy()
        {
            CurrentScene.GameObjects.RemoveAt(RessourceIndex);
        }
    }
}