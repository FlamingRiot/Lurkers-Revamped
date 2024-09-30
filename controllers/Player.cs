using System.Numerics;
using Raylib_cs;
using Lurkers_revamped;

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
        Crouch,
        Dying
    }
    public class Player
    {
        /// <summary>Player's max jump force</summary>
        public static readonly float JUMP_FORCE = 0.3f;

        /// <summary>Player's base movement speed</summary>
        public float SPEED = 0.1f;

        /// <summary>
        /// The time delay for the player's actions
        /// </summary>
        private double actionDelay;
        /// <summary>
        /// Player name
        /// </summary>
        private string name;

        public int Frame;
        /// <summary>
        /// The y position of the player
        /// </summary>
        private float yPos;
        /// <summary>
        /// The y collision ray of the player
        /// </summary>
        private Ray ray;
        /// <summary>
        /// The ray collision of the player
        /// </summary>
        private RayCollision rayCollision;
        /// <summary>
        /// Bouding box of the player
        /// </summary>
        private BoundingBox box;
        /// <summary>
        /// Motion constraint of the player
        /// </summary>
        private MotionConstraint motionConstraint;
        /// <summary>
        /// The inventory of the player
        /// </summary>
        private List<Weapon> inventory;
        /// <summary>
        /// The index in the inventory of the active weapon
        /// </summary>
        private int inventoryIndex;
        /// <summary>
        /// The currently held weapon of the player
        /// </summary>
        private Weapon currentWeapon;
        /// <summary>
        /// The currently displayed animation
        /// </summary>
        private ModelAnimation currentAnimation;
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
        /// The collision of the player ray
        /// </summary>
        public RayCollision RayCollision { get { return rayCollision; } set { rayCollision = value; } }
        /// <summary>
        /// Motion constraint of the player
        /// </summary>
        public MotionConstraint MotionConstraint { get { return motionConstraint; } set { motionConstraint = value; } }
        /// <summary>
        /// The hit attribute of the rayCollision object
        /// </summary>
        public bool CollisionHit { get { return rayCollision.Hit; } set { rayCollision.Hit = value; } }
        /// <summary>
        /// The ray of the player
        /// </summary>
        public Ray Ray { get { return ray; } set { ray = value; } }
        /// <summary>
        /// The direction of the ray
        /// </summary>
        public Vector3 RayDirection { get { return ray.Direction; } set { ray.Direction = value; } }
        /// <summary>
        /// The position of the ray
        /// </summary>
        public Vector3 RayPosition { get { return ray.Position; } set { ray.Position = value; } }
        /// <summary>
        /// Min value of the player's bounding box
        /// </summary>
        public Vector3 MinBox { get { return box.Min; } set { box.Min = value; } }
        /// <summary>
        /// Max value of the player's bounding box
        /// </summary>
        public Vector3 MaxBox { get { return box.Max; } set { box.Max = value; } }
        /// <summary>
        /// The player's bounding box
        /// </summary>
        public BoundingBox Box { get { return box; } }
        /// <summary>
        /// The secondary state of the player
        /// </summary>
        public PlayerMoveState MoveState { get { return moveState; } set { moveState = value; } }

        /// <summary>The current amount of life of the player</summary>
        public int Life { get { return life; } 
            set 
            { 
                life = value; 
                if (life <= 0)
                {
                    Kill();
                }
            } 
        }

        /// <summary>
        /// The y position of the player
        /// </summary>
        public float YPos { get { return yPos; } set { yPos = value; } }
        /// <summary>
        /// The inventory index of the active weapon
        /// </summary>
        public int InventoryIndex { get { return inventoryIndex; } set { inventoryIndex = value; } }
        /// <summary>
        /// The size of the inventory (starting from 0)
        /// </summary>
        public int InventorySize { get { return inventory.Count - 1; } }
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
        public ModelAnimation CurrentAnimation { get { return currentAnimation; } set { currentAnimation = value; } }
        /// <summary>
        /// Player name
        /// </summary>
        public string Name { get { return name; } set { name = value; } }
        /// <summary>
        /// Player constructor
        /// </summary>
        /// <param name="name">Player name</param>
        public Player(string name, Weapon baseWeapon, ModelAnimation currentAnimation)
        {
            this.name = name;
            // Init the inventory
            inventory = new List<Weapon>();
            // Set player life
            life = 100;
            // Set the weapon to non when the player spawns
            currentWeapon = baseWeapon;
            inventory.Add(currentWeapon);
            InventoryIndex = 0;
            // Set the player state
            WeaponState = PlayerWeaponState.Idle;
            // Set player current animation
            this.currentAnimation = currentAnimation;
            // Set player ray direction
            RayDirection = -Vector3.UnitY;

            // Set the player's default Bounding box dimensions
            MaxBox = new Vector3(0.5f, 0, 0.5f);
            MinBox = new Vector3(-0.5f, -3, -0.5f);

            // Set default motion constraint
            this.motionConstraint.Value = 1;
            this.motionConstraint.Constraint = new Vector3(1, 0, 1);
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

        /// <summary>Starts the dying animation of the player</summary>
        public void Kill()
        {
            MoveState = PlayerMoveState.Dying;
            Console.WriteLine("Player has been killed!");
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
            InventoryIndex = index;
            CurrentWeapon = inventory[InventoryIndex];
        }
        /// <summary>
        /// Reset the box's dimensions
        /// </summary>
        public void ResetBox()
        {
            // Set the player's default Bounding box dimensions
            MaxBox = new Vector3(0.5f, 0, 0.5f);
            MinBox = new Vector3(-0.5f, -3, -0.5f);
        }
        /// <summary>
        /// Calculate motion constraint of the player
        /// </summary>
        /// <param name="target">Target of the player</param>
        /// <param name="box">Bounding box to calculate with</param>
        public void CalculateMotionConstraint(Vector3 target, BoundingBox box, Vector3 position)
        {
            this.motionConstraint.Calculate(target, box, position);
        }
    }
}