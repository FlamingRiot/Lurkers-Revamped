using System.Numerics;
namespace Lurkers_revamped
{
    public class Weapon
    {
        // Weapon's different level colors
        public static readonly List<Vector4> Colors = new List<Vector4>() 
        {
            new Vector4(184, 181, 186, 255),
            new Vector4(61, 252, 3, 255),
            new Vector4(23, 214, 252, 255),
            new Vector4(148, 3, 252, 255),
            new Vector4(252, 186, 3, 255)
        };

        /// <summary>
        /// Level of the weapon
        /// </summary>
        private int level;
        /// <summary>
        /// The name of the weapon
        /// </summary>
        private string name;
        /// <summary>
        /// The ID for the used mesh of the weapon
        /// </summary>
        private string modelID;
        /// <summary>
        /// The current amount of ammo in the weapon
        /// </summary>
        private int ammos;
        /// <summary>
        /// The maximum ammos the weapon can hold
        /// </summary>
        private int maxAmmos;
        /// <summary>
        /// The shot bullets from the weapon
        /// </summary>
        public List<Bullet> bullets;
        /// <summary>
        /// The name of the weapon
        /// </summary>
        public string Name { get { return name; } set { name = value; } }
        /// <summary>
        /// Level of the weapon
        /// </summary>
        public int Level { get { return level; } set { level = value; } }
        /// <summary>
        /// The ID for the used mesh of the weapon
        /// </summary>
        public string ModelID { get { return modelID; } set { modelID = value; } }
        /// <summary>
        /// The current amount of ammo in the weapon
        /// </summary>
        public int Ammos { get { return ammos; } set { ammos = value; } }
        /// <summary>
        /// The maximum ammos the weapon can hold
        /// </summary>
        public int MaxAmmos { get { return maxAmmos; } set { maxAmmos = value; } }

        public Weapon()
        {
            name = "";
            modelID = "";
            bullets = new List<Bullet>();
        }

        /// <summary>
        /// Weapon constructor
        /// </summary>
        /// <param name="name">Weapon name</param>
        /// <param name="meshID">Weapon meshID</param>
        /// <param name="maxAmmos">Weapon maxAmmos</param>
        public Weapon(string name, string modelID, int maxAmmos, int level)
        {
            bullets = new List<Bullet>();
            this.name = name;
            this.modelID = modelID;
            this.maxAmmos = maxAmmos;
            this.level = level;
        }
        /// <summary>
        /// Shoot a bullet from the weapon
        /// </summary>
        /// <param name="origin">Camera view</param>
        /// <param name="direction">Weapon crosshair</param>
        public void ShootBullet(Vector3 origin, Vector3 direction)
        {
            bullets.Add(new Bullet(origin, direction));
        }
        /// <summary>
        /// Destroy the last shot bullet
        /// </summary>
        public void DestroyBullet()
        {
            bullets.RemoveAt(bullets.Count - 1);
        }
    }
}