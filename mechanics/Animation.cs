using Raylib_cs;
namespace uniray_Project
{
    public unsafe class Animation
    {
        /// <summary>
        /// Model animation
        /// </summary>
        private ModelAnimation anim;
        /// <summary>
        /// The current frame of the animation
        /// </summary>
        private int frame;
        /// <summary>
        /// The current frame of the animation
        /// </summary>
        public int Frame 
        { 
            get 
            {
                return frame;
            } 
            set 
            { 
                frame = value;
            } 
        }
        /// <summary>
        /// Model animation
        /// </summary>
        public ModelAnimation Anim { get { return anim; } set { anim = value; } }
        /// <summary>
        /// Animation constructor
        /// </summary>
        /// <param name="animation"></param>
        public Animation(ModelAnimation anim)
        {
            this.anim = anim;
        }
        /// <summary>
        /// Return and update the current frame 
        /// </summary>
        /// <returns>The updated frame</returns>
        public int UpdateFrame()
        {
            if (frame < anim.FrameCount - 1)
            {
                frame++;
            }
            else frame = 0;
            return frame;
        }
        /// <summary>
        /// Get the count of frame in the animation
        /// </summary>
        /// <returns>Int count</returns>
        public int FrameCount()
        {
            return anim.FrameCount;
        }
        /// <summary>
        /// Get the bones of the animation
        /// </summary>
        /// <returns>Int count</returns>
        public BoneInfo* Bones()
        {
            return anim.Bones;
        }
        /// <summary>
        /// Get the bone count of the animation
        /// </summary>
        /// <returns></returns>
        public int BoneCount()
        {
            
            return anim.BoneCount;
        }
    }
}