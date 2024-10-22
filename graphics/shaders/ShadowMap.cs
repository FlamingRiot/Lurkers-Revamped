using Raylib_cs;
using System.Numerics;

namespace Lurkers_revamped
{
    /// <summary>Represents a Shadow Map object</summary>
    public class ShadowMap
    {
        /// <summary>Shadow map resolution</summary>
        public static readonly int SHADOW_MAP_RESOLUTION = 4096;

        /// <summary>Maximum of rendered lights for shadowmap</summary>
        public const int MAX_LIGHTS = 4;

        /// <summary>Shadow map camera view</summary>
        public Camera3D CameraView;

        /// <summary>Shadow map render texture</summary>
        public RenderTexture2D Map;

        /// <summary>Creates a shadow map object</summary>
        /// <param name="shader">Lighting shader to use</param>
        public ShadowMap(Vector3 position, Vector3 target)
        {
            // Load camera view
            CameraView = new Camera3D() {
                Position = position,
                Target = Raymath.Vector3Normalize(target),
                Projection = CameraProjection.Orthographic,
                Up = new Vector3(0f, 1f, 0f),
                FovY = 60f
            };

            // Load shadow map
            Map = LoadShadowMapRenderTexture(SHADOW_MAP_RESOLUTION, SHADOW_MAP_RESOLUTION);
        }

        /// <summary>Loads a shadow map render texture based on the passed dimensions</summary>
        /// <param name="width">Map width</param>
        /// <param name="height">Map height</param>
        /// <returns>Shadow map render texture</returns>
        private RenderTexture2D LoadShadowMapRenderTexture(int width, int height)
        {
            RenderTexture2D target = new RenderTexture2D();

            target.Id = Rlgl.LoadFramebuffer(width, height);
            target.Texture.Width = width;
            target.Texture.Height = height;

            if (target.Id > 0)
            {
                Rlgl.EnableFramebuffer(target.Id);

                target.Depth.Id = Rlgl.LoadTextureDepth(width, height, false);
                target.Depth.Width = width;
                target.Depth.Height = height;
                target.Depth.Format = PixelFormat.CompressedPvrtRgba;
                target.Depth.Mipmaps = 1;

                Rlgl.FramebufferAttach(target.Id, target.Depth.Id, FramebufferAttachType.Depth, FramebufferAttachTextureType.Texture2D, 0);

                if (Rlgl.FramebufferComplete(target.Id)) Raylib.TraceLog(TraceLogLevel.Info, "FBO: [ID %i] Framebuffer object created successfully");

                Rlgl.DisableFramebuffer();
            }
            else
            {
                Raylib.TraceLog(TraceLogLevel.Info, "FBO: [ID %i] Framebuffer object can not be created");
            }

            return target;
        }
    }
}
