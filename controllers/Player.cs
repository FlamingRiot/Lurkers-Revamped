using System.Numerics;
using Raylib_cs;
using System.Diagnostics;

namespace Lurkers_revamped
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

        /// <summary>Player amount of life</summary>
        private int life;

        /// <summary>Time delay for player action.</summary>
        public double ActionDelay;

        /// <summary>Player name.</summary>
        public string Name;

        /// <summary>Current animation frame.</summary>
        public int Frame;

        /// <summary>Y position of the player.</summary>
        public float YPos;

        public bool BLOODY;

        public Stopwatch Watch;
        
        /// <summary>Ray of the player.</summary>
        public Ray Ray;

        /// <summary>Collision ray of the player.</summary>
        public RayCollision RayCollision;

        /// <summary>Player bounding box.</summary>
        public BoundingBox Box;

        /// <summary>Player motion constraint.</summary>
        public MotionConstraint MotionConstraint;

        /// <summary>Player inventory.</summary>
        public List<Weapon> Inventory;

        /// <summary>Inventory index.</summary>
        public int InventoryIndex;

        /// <summary>Currently held weapon.</summary>
        public Weapon CurrentWeapon;

        /// <summary>Currently displayed animation.</summary>
        public ModelAnimation CurrentAnimation;

        /// <summary>Player current weapon state.</summary>
        public PlayerWeaponState WeaponState;

        /// <summary>Player current move state.</summary>
        public PlayerMoveState MoveState;

        /// <summary>Player jump velocity.</summary>
        public float VJump;

        /// <summary>Current amount of life.</summary>
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

        /// <summary>Inventory size starting from 0</summary>
        public int InventorySize { get { return Inventory.Count - 1; } }

        /// <summary>
        /// Player constructor
        /// </summary>
        /// <param name="name">Player name</param>
        public Player(string name, Weapon baseWeapon, ModelAnimation currentAnimation)
        {
            Name = name;
            // Init the inventory
            Inventory = new List<Weapon>();
            // Set player life
            life = 100;
            // Set the weapon to non when the player spawns
            CurrentWeapon = baseWeapon;
            Inventory.Add(CurrentWeapon);
            InventoryIndex = 0;
            // Set the player state
            WeaponState = PlayerWeaponState.Idle;
            // Set player current animation
            CurrentAnimation = currentAnimation;
            // Set player ray direction
            Ray.Direction = -Vector3.UnitY;

            // Set the player's default Bounding box dimensions
            Box.Max = new Vector3(0.5f, 0, 0.5f);
            Box.Min = new Vector3(-0.5f, -3, -0.5f);

            // Set default motion constraint
            MotionConstraint.Value = 1;
            MotionConstraint.Constraint = new Vector3(1, 0, 1);

            // Init watch
            Watch = new Stopwatch();
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
        }

        /// <summary>
        /// Add a weapon to the inventory of the player
        /// </summary>
        /// <param name="weapon">Weapon of the player</param>
        public void AddWeapon(Weapon weapon)
        {
            Inventory.Add(weapon);
        }
        /// <summary>
        /// Set the current weapon of the player
        /// </summary>
        /// <param name="index"></param>
        public void SetCurrentWeapon(int index)
        {
            InventoryIndex = index;
            CurrentWeapon = Inventory[InventoryIndex];
        }
        /// <summary>
        /// Reset the box's dimensions
        /// </summary>
        public void ResetBox()
        {
            // Set the player's default Bounding box dimensions
            Box.Max = new Vector3(0.5f, 0, 0.5f);
            Box.Min = new Vector3(-0.5f, -3, -0.5f);
        }
        /// <summary>
        /// Calculate motion constraint of the player
        /// </summary>
        /// <param name="target">Target of the player</param>
        /// <param name="box">Bounding box to calculate with</param>
        public void CalculateMotionConstraint(Vector3 target, BoundingBox box, Vector3 position)
        {
            MotionConstraint.Calculate(target, box, position);
        }
    }
}