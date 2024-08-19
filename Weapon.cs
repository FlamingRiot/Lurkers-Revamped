namespace uniray_Project
{
    public class Weapon
    { 
        /// <summary>
        /// The name of the weapon
        /// </summary>
        private string name;
        /// <summary>
        /// The ID for the used mesh of the weapon
        /// </summary>
        private int meshID;
        /// <summary>
        /// The current amount of ammo in the weapon
        /// </summary>
        private int ammos;
        /// <summary>
        /// The maximum ammos the weapon can hold
        /// </summary>
        private int maxAmmos;
        /// <summary>
        /// The name of the weapon
        /// </summary>
        public string Name { get { return name; } set { name = value; } }
        /// <summary>
        /// The ID for the used mesh of the weapon
        /// </summary>
        public int MeshID { get { return meshID; } set { meshID = value; } }
        /// <summary>
        /// The current amount of ammo in the weapon
        /// </summary>
        public int Ammos { get { return ammos; } set { ammos = value; } }
        /// <summary>
        /// The maximum ammos the weapon can hold
        /// </summary>
        public int MaxAmmos { get { return maxAmmos; } set { maxAmmos = value; } }
        /// <summary>
        /// Weapon constructor
        /// </summary>
        /// <param name="name">Weapon name</param>
        /// <param name="meshID">Weapon meshID</param>
        /// <param name="maxAmmos">Weapon maxAmmos</param>
        public Weapon(string name, int meshID, int maxAmmos)
        {
            this.name = name;
            this.meshID = meshID;
            this.maxAmmos = maxAmmos;
        }
    }
}