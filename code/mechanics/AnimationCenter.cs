using Raylib_cs;

namespace Lurkers_revamped
{
    /// <summary>Represents an instance of a <see cref="AnimationCenter"/> object.</summary>
    public static class AnimationCenter
    {
        public static List<ModelAnimation> PlayerAnimations = new List<ModelAnimation>();
        public static List<ModelAnimation> ZombieAnimations = new List<ModelAnimation>();

        /// <summary>Inits the animation center by loading all the animations lists.</summary>
        public static void Init()
        {
            // Load animation lists
            PlayerAnimations = RLoading.LoadAnimationList("src/animations/rifle.m3d");
            ZombieAnimations = RLoading.LoadAnimationList("src/animations/walker.m3d");
        }

        /// <summary>Closes the animation center by unloading all the animations lists.</summary>
        public static void Close()
        {
            PlayerAnimations.ForEach(anim => Raylib.UnloadModelAnimation(anim));
            ZombieAnimations.ForEach(anim => Raylib.UnloadModelAnimation(anim));
        }
    }
}