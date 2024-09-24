using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace uniray_Project
{
    public unsafe class ShaderCenter
    {
        /// <summary>Light View-Projection matrix location in lighting shader</summary>
        private int lightVPLoc;

        /// <summary>Shadow map location in lighting shader</summary>
        private int shadowMapLoc;

        /// <summary>Location of the previous frame render texture in motion blur shader</summary>
        private int prevTextureLoc;

        /// <summary>Location of the terrain boolean value in shader</summary>
        private int isTerrainLoc;

        // ################### Shaders ###################

        /// <summary>Outline shader</summary>
        public Shader OutlineShader;
        
        /// <summary>The texture-tiling shader for the terrain</summary>
        public Shader TilingShader;

        /// <summary>Skybox shader</summary>
        public Shader SkyboxShader;
        
        /// <summary>Cubemap shader</summary>
        public Shader CubemapShader;

        /// <summary>Ambient lighting shader</summary>
        public Shader LightingShader;

        /// <summary>Motion blur shader</summary>
        public Shader MotionBlurShader;

        // ################### Materials ###################

        /// <summary>Skybox material</summary>
        public Material SkyboxMaterial;

        /// <summary>The material used for the outline shader</summary>
        public Material OutlineMaterial;

        /// <summary>ShaderCenter constructor</summary>
        public ShaderCenter()
        {
            // Load shaders
            LoadShaders();
            // Load materials
            LoadMaterials();
        }

        /// <summary>Loads lighting shader and assigns required values</summary>
        /// <param name="lightDirection">Direction of lighting</param>
        /// <param name="lightColor">General coloration of lighting</param>
        public void LoadLighting(Vector3 lightDirection, Color lightColor)
        {
            // Load shader
            LightingShader = LoadShader("src/shaders/lighting.vs", "src/shaders/lighting.fs");
            LightingShader.Locs[(int)ShaderLocationIndex.VectorView] = GetShaderLocation(LightingShader, "viewPos");

            // Retrieve locations from shader code
            int lightDirLoc = GetShaderLocation(LightingShader, "lightDir");
            int lightColLoc = GetShaderLocation(LightingShader, "lightColor");
            int ambientLoc = GetShaderLocation(LightingShader, "ambient");
            lightVPLoc = GetShaderLocation(LightingShader, "lightVP");
            shadowMapLoc = GetShaderLocation(LightingShader, "shadowMap");
            isTerrainLoc = GetShaderLocation(LightingShader, "isTerrain");
            int shadowMapResolutionLoc = GetShaderLocation(LightingShader, "shadowMapResolution");

            // Define values
            float[] ambient = new[] { 0.5f, 0.5f, 0.5f, 1.0f };
            int shadowMapResolution = ShadowMap.SHADOW_MAP_RESOLUTION;
            Vector4 normalizedColor = ColorNormalize(lightColor);

            // Set values at shader locations
            SetShaderValue(LightingShader, lightDirLoc, &lightDirection, ShaderUniformDataType.Vec3);
            SetShaderValue(LightingShader, lightColLoc, &normalizedColor, ShaderUniformDataType.Vec4);
            SetShaderValue(LightingShader, ambientLoc, ambient, ShaderUniformDataType.Vec4);
            SetShaderValue(LightingShader, shadowMapResolutionLoc, &shadowMapResolution, ShaderUniformDataType.Int);
        }

        /// <summary>Updates light view-project matrix</summary>
        /// <param name="mvp">Matrix View-Projection</param>
        public void UpdateLightMatrix(Matrix4x4 mvp)
        {
            SetShaderValueMatrix(LightingShader, lightVPLoc, mvp);
        }

        public void UpdateTiling(bool enabled)
        {
            SetShaderValue(LightingShader, isTerrainLoc, &enabled, ShaderUniformDataType.Int);
        }

        /// <summary>Updates shadow map to the shader</summary>
        /// <param name="shadow">Shadow map to update</param>
        public void UpdateShadowMap(ShadowMap shadow)
        {
            Rlgl.EnableShader(LightingShader.Id);
            int slot = 10;
            Rlgl.ActiveTextureSlot(10);
            Rlgl.EnableTexture(shadow.Map.Depth.Id);
            Rlgl.SetUniform(shadowMapLoc, &slot, 4, 1);
        }

        /// <summary>Loads every basic shader in shader center</summary>
        private void LoadShaders()
        {
            // Load outline shader
            OutlineShader = LoadShader("src/shaders/outline.vs", "src/shaders/outline.fs");
            // Load tiling shader
            TilingShader = LoadShader("src/shaders/tiling.vs", "src/shaders/tiling.fs");
            // Load skybox shader
            SkyboxShader = LoadShader("src/shaders/skybox.vs", "src/shaders/skybox.fs");
            SetShaderValue(SkyboxShader, GetShaderLocation(SkyboxShader, "environmentMap"), (int)MaterialMapIndex.Cubemap, ShaderUniformDataType.Int);
            SetShaderValue(SkyboxShader, GetShaderLocation(SkyboxShader, "doGamma"), 1, ShaderUniformDataType.Int);
            SetShaderValue(SkyboxShader, GetShaderLocation(SkyboxShader, "vflipped"), 1, ShaderUniformDataType.Int);
            // Load cubemap shader
            CubemapShader = LoadShader("src/shaders/cubemap.vs", "src/shaders/cubemap.fs");
            SetShaderValue(CubemapShader, GetShaderLocation(CubemapShader, "equirectangularMap"), 0, ShaderUniformDataType.Int);

            // Load motion blur shader
            MotionBlurShader = LoadShader("", "src/shaders/blur.fs");
            prevTextureLoc = GetShaderLocation(MotionBlurShader, "prevFrame");
            int blurAmountLoc = GetShaderLocation(MotionBlurShader, "blurAmount");
            float blurAmount = 1f;
            SetShaderValue(MotionBlurShader, blurAmountLoc, &blurAmount, ShaderUniformDataType.Float);

            int chromaticAmountLoc = GetShaderLocation(MotionBlurShader, "chromaticAmount");
            float chromaticAmount = 0.2f;
            SetShaderValue(MotionBlurShader, chromaticAmountLoc, &chromaticAmount, ShaderUniformDataType.Float);
        }

        /// <summary>Loads every basic shader's material in shader center</summary>
        private void LoadMaterials()
        {
            // Init the outline material
            OutlineMaterial = LoadMaterialDefault();
            OutlineMaterial.Shader = OutlineShader;

            // Init the skybox material
            SkyboxMaterial = LoadMaterialDefault();
            SkyboxMaterial.Shader = SkyboxShader;
        }

        /// <summary>Generates cubemap according to the passed HDR texture</summary>
        /// <param name="panorama">Texture to use/param>
        /// <param name="size">FrameBuffer size</param>
        /// <param name="format">Format of the cubemap</param>
        /// <returns>Cubemap texture</returns>
        public Texture2D GenTexureCubemap(Texture2D panorama, int size, PixelFormat format)
        {
            Texture2D cubemap;

            // Disable Backface culling to render inside the cube
            Rlgl.DisableBackfaceCulling();

            // Setup frame buffer
            uint rbo = Rlgl.LoadTextureDepth(size, size, true);
            cubemap.Id = Rlgl.LoadTextureCubemap(null, size, format);

            uint fbo = Rlgl.LoadFramebuffer(size, size);
            Rlgl.FramebufferAttach(fbo, rbo, FramebufferAttachType.Depth, FramebufferAttachTextureType.Renderbuffer, 0);
            Rlgl.FramebufferAttach(fbo, cubemap.Id, FramebufferAttachType.ColorChannel0, FramebufferAttachTextureType.CubemapPositiveX, 0);

            // Check if framebuffer is valid

            if (Rlgl.FramebufferComplete(fbo))
            {
                Console.WriteLine($"FBO: [ID {fbo}] Framebuffer object created successfully");
            }

            // Draw to framebuffer
            Rlgl.EnableShader(CubemapShader.Id);

            // Define projection matrix and send it to the shader
            Matrix4x4 matFboProjection = Raymath.MatrixPerspective(90.0f * DEG2RAD, 1.0f, Rlgl.CULL_DISTANCE_NEAR, Rlgl.CULL_DISTANCE_FAR);
            Rlgl.SetUniformMatrix(CubemapShader.Locs[(int)ShaderLocationIndex.MatrixProjection], matFboProjection);

            // Define view matrix for every side of the cube
            Matrix4x4[] fboViews = new Matrix4x4[]
            {
                Raymath.MatrixLookAt(Vector3.Zero, new Vector3(-1.0f,  0.0f,  0.0f), new Vector3( 0.0f, -1.0f,  0.0f)),
                Raymath.MatrixLookAt(Vector3.Zero, new Vector3( 1.0f,  0.0f,  0.0f), new Vector3( 0.0f, -1.0f,  0.0f)),
                Raymath.MatrixLookAt(Vector3.Zero, new Vector3( 0.0f,  1.0f,  0.0f), new Vector3( 0.0f,  0.0f, -1.0f)),
                Raymath.MatrixLookAt(Vector3.Zero, new Vector3( 0.0f, -1.0f,  0.0f), new Vector3( 0.0f,  0.0f, 1.0f)),
                Raymath.MatrixLookAt(Vector3.Zero, new Vector3( 0.0f,  0.0f, -1.0f), new Vector3( 0.0f, -1.0f,  0.0f)),
                Raymath.MatrixLookAt(Vector3.Zero, new Vector3( 0.0f,  0.0f,  1.0f), new Vector3( 0.0f, -1.0f,  0.0f)),
            };

            // Set viewport to current fbo dimensions
            Rlgl.Viewport(0, 0, size, size);

            // Activate and enable texture for drawing to cubemap faces
            Rlgl.ActiveTextureSlot(0);
            Rlgl.EnableTexture(panorama.Id);

            for (int i = 0; i < 6; i++)
            {
                // Set the view matrix for current face
                Rlgl.SetUniformMatrix(CubemapShader.Locs[(int)ShaderLocationIndex.MatrixView], fboViews[i]);

                // Select the current cubemap face attachment for the fbo
                Rlgl.FramebufferAttach(fbo, cubemap.Id, FramebufferAttachType.ColorChannel0, FramebufferAttachTextureType.CubemapPositiveX + i, 0);
                Rlgl.EnableFramebuffer(fbo);

                Rlgl.ClearScreenBuffers();
                Rlgl.LoadDrawCube();
            }

            // Unload framebuffer and reset state
            Rlgl.DisableShader();
            Rlgl.DisableTexture();
            Rlgl.DisableFramebuffer();

            Rlgl.UnloadFramebuffer(fbo);

            Rlgl.Viewport(0, 0, Rlgl.GetFramebufferWidth(), Rlgl.GetFramebufferHeight());
            Rlgl.EnableBackfaceCulling();

            cubemap.Width = size;
            cubemap.Height = size;
            cubemap.Mipmaps = 1;
            cubemap.Format = format;

            return cubemap;
        }

        /// <summary>Sets cubemap texture to the skybox material</summary>
        /// <param name="tex">Cubemap texture to use</param>
        public void SetCubemap(Texture2D tex)
        {
            SetMaterialTexture(ref SkyboxMaterial, MaterialMapIndex.Cubemap, tex);
        }

        /// <summary>Set the previous frame texture to the motion blur shader</summary>
        /// <param name="prevTexture">Previous frame texture to set</param>
        public void SetBlurTexture(RenderTexture2D prevTexture)
        {
            SetShaderValueTexture(MotionBlurShader, prevTextureLoc, prevTexture.Texture);
        }

        /// <summary>Unloads all data storred in shader center</summary>
        public void UnloadShaderCenter()
        {
            UnloadMaterials();
            UnloadShaders();
        }

        /// <summary>Unloads every shader storred in shader center</summary>
        private void UnloadShaders()
        {
            UnloadShader(SkyboxShader);
            UnloadShader(CubemapShader);
            UnloadShader(OutlineShader);
            UnloadShader(TilingShader);
            UnloadShader(LightingShader);
        }

        /// <summary>Unloads every material storred in shader center</summary>
        private void UnloadMaterials()
        {
            UnloadMaterial(OutlineMaterial);
            UnloadMaterial(SkyboxMaterial);
        }
    }
}