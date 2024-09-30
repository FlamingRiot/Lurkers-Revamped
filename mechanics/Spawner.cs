using static Raylib_cs.Raylib;
using Raylib_cs;using System.Numerics;
using static UnirayEngine.UnirayEngine;

namespace uniray_Project.mechanics
{
    public class Spawner
    {
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
        public Zombie CreateZombie(Animation anim)
        {
            // Randomize position
            Vector3 position = Position + new Vector3((float)Random.Shared.NextDouble() * 2.5f, 0, (float)Random.Shared.NextDouble() * 2.5f);
            // Create zombie
            Zombie zombzomb = new Zombie(position, "cop", anim);
            // Return zombie
            return zombzomb;
        }

        /// <summary>Simulates a bullet collision on a spawner</summary>
        /// <param name="ray">Bullet ray</param>
        /// <param name="mesh">Mesh of the spawner</param>
        /// <returns></returns>
        public bool Shoot(Ray ray, Mesh mesh)
        {
            // Retrive collisiion informations
            RayCollision collision = GetRayCollisionMesh(ray, mesh, Transform);
            if (collision.Hit)
            {
                Health -= 100;
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