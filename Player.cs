namespace uniray_Project
{
    public enum PlayerWeaponState
    {
        Idle,
        Shooting,
        Reloading,
        Taking,
        Hiding,
        GetUp,
        Aiming
    }
    public enum PlayerMoveState
    {
        Running,
        Jumping,
        Crouch
    }
    public class Player
    {
        /// <summary>
        /// Player's jump force
        /// </summary>
        public const float JUMP_FORCE = 0.5f; 

        /// <summary>
        /// The time delay for the player's actions
        /// </summary>
        private double actionDelay;
        /// <summary>
        /// Player name
        /// </summary>
        private string name;
        /// <summary>
        /// The inventory of the player
        /// </summary>
        private List<Weapon> inventory;
        /// <summary>
        /// The currently held weapon of the player
        /// </summary>
        private Weapon currentWeapon;
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
        private PlayerWeaponState weaponState;
        /// <summary>
        /// The secondary state of the player
        /// </summary>
        private PlayerMoveState moveState;
        /// <summary>
        /// The velocity of the player's jump
        /// </summary>
        private float vJump;
        /// <summary>
        /// The current state of the player
        /// </summary>
        public PlayerWeaponState WeaponState { get { return weaponState; } set { weaponState = value; } }
        /// <summary>
        /// The secondary state of the player
        /// </summary>
        public PlayerMoveState MoveState { get { return moveState; } set { moveState = value; } }
        /// <summary>
        /// The current amount of life the player has
        /// </summary>
        public int Life { get { return life; } set { life = value; } }
        /// <summary>
        /// The time delay for the player's actions
        /// </summary>
        public double ActionDelay { get { return actionDelay; } set { actionDelay = value; } }
        /// <summary>
        /// The velocity of the player's jump
        /// </summary>
        public float VJump { get { return vJump; } set { vJump = value; } }
        /// <summary>
        /// The currently held weapon of the player
        /// </summary>
        public Weapon CurrentWeapon { get { return currentWeapon; } set { currentWeapon = value; } }
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
        public Player(string name, Weapon baseWeapon, Animation currentAnimation)
        {
            this.name = name;
            // Init the inventory
            inventory = new List<Weapon>();
            // Set player life
            life = 100;
            // Set the weapon to non when the player spawns
            currentWeapon = baseWeapon;
            inventory.Add(currentWeapon);
            // Set the player state
            WeaponState = PlayerWeaponState.Idle;
            // Set player current animation
            this.currentAnimation = currentAnimation;
        }
        /// <summary>
        /// Add a weapon to the inventory of the player
        /// </summary>
        /// <param name="weapon">Weapon of the player</param>
        public void AddWeapon(Weapon weapon)
        {
            inventory.Add(weapon);
        }
        /// <summary>
        /// Set the current weapon of the player
        /// </summary>
        /// <param name="index"></param>
        public void SetCurrentWeapon(int index)
        {
            CurrentWeapon = inventory[index];
        }
    }
}