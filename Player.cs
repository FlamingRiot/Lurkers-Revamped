namespace uniray_Project
{
    public class Player
    {
        /// <summary>
        /// Player name
        /// </summary>
        private string name;
        /// <summary>
        /// The currently held weapon of the player
        /// </summary>
        private Weapon? currentWeapon;
        /// <summary>
        /// The amount of life the player
        /// </summary>
        private int life;
        /// <summary>
        /// The current amount of life the player has
        /// </summary>
        public int Life { get { return life; } set { life = value; } }
        /// <summary>
        /// The currently held weapon of the player
        /// </summary>
        public Weapon? CurrentWeapon { get { return currentWeapon; } set { currentWeapon = value; } }
        /// <summary>
        /// Player name
        /// </summary>
        public string Name { get { return name; } set { name = value; } }
        /// <summary>
        /// Player constructor
        /// </summary>
        /// <param name="name">Player name</param>
        public Player(string name)
        {
            this.name = name;
            life = 100;
            // Set the weapon to non when the player spawns
            currentWeapon = null;
        }
    }
}