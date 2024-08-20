namespace uniray_Project
{
    public enum PlayerState
    {
        Idle,
        Running,
        Shooting,
        Reloading,
        Taking,
        Hiding,
        Crouch,
        GetUp
    }
    public class Player
    {
        /// <summary>
        /// The time delay for the player's actions
        /// </summary>
        private double actionDelay;
        /// <summary>
        /// Player name
        /// </summary>
        private string name;
        /// <summary>
        /// The currently held weapon of the player
        /// </summary>
        private Weapon? currentWeapon;
        /// <summary>
        /// The currently displayed animation
        /// </summary>
        private Animation currentAnimation;
        /// <summary>
        /// The amount of life the player
        /// </summary>
        private int life;
        /// <summary>
        /// The current state of the player
        /// </summary>
        private PlayerState state;
        /// <summary>
        /// The current state of the player
        /// </summary>
        public PlayerState State { get { return state; } set { state = value; } }
        /// <summary>
        /// The current amount of life the player has
        /// </summary>
        public int Life { get { return life; } set { life = value; } }
        /// <summary>
        /// The time delay for the player's actions
        /// </summary>
        public double ActionDelay { get { return actionDelay; } set { actionDelay = value; } }
        /// <summary>
        /// The currently held weapon of the player
        /// </summary>
        public Weapon? CurrentWeapon { get { return currentWeapon; } set { currentWeapon = value; } }
        /// <summary>
        /// The currently displayed animation
        /// </summary>
        public Animation CurrentAnimation { get { return currentAnimation; } set { currentAnimation = value; } }
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
            // Set the player state
            State = PlayerState.Running;
        }
    }
}