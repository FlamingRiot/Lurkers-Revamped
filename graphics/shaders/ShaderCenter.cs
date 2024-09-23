using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace uniray_Project
{
    public unsafe class ShaderCenter
    {
        public const int SHADOW_MAP_RESOLUTION = 4096;

        private int lightVPLoc;

        private int shadowMapLoc;

        /// <summary>
        /// The outline shader
        /// </summary>
        private Shader outlineShader;
        /// <summary>
        /// The material used for the outline shader
        /// </summary>
        private Material outlineMaterial;
        /// <summary>
        /// The texture-tiling shader for the terrain
        /// </summary>
        private Shader tilingShader;
        /// <summary>
        /// The skybox shader
        /// </summary>
        private Shader skyboxShader;
        /// <summary>
        /// The cubemap shader
        /// </summary>
        private Shader cubemapShader;

        /// <summary>Ambient lighting shader</summary>
        public Shader lightingShader;

        /// <summary>
        /// The skybox material
        /// </summary>
        private Material skyboxMaterial;
        /// <summary>
        /// The outline shader
        /// </summary>
        public Shader OutlineShader { get { return outlineShader; } }
        /// <summary>
        /// The texture-tiling shader for the terrain
        /// </summary>
        public Shader TilingShader { get { return tilingShader; } }
        /// <summary>
        /// The skybox shader
        /// </summary>
        public Shader SkyboxShader { get { return skyboxShader; } }
        /// <summary>
        /// The cubemap generation shader
        /// </summary>
        public Shader CubemapShader { get { return cubemapShader; } }
        /// <summary>
        /// The outline material
        /// </summary>
        public Material OutlineMaterial { get { return outlineMaterial; } }
        /// <summary>
        /// The skybox material
        /// </summary>
        public Material SkyboxMaterial { get { return skyboxMaterial; } }
        /// <summary>
        /// ShaderCenter constructor
        /// </summary>
        public ShaderCenter()
        {
            // Load shaders
            LoadShaders();
            // Load materials
            LoadMaterials();
        }

        public void LoadLighting(Vector3 lightDirection, Color lightColor)
        {
            // Load shader
            lightingShader = LoadShader("src/shaders/lighting.vs", "src/shaders/lighting.fs");
            lightingShader.Locs[(int)ShaderLocationIndex.VectorView] = GetShaderLocation(lightingShader, "viewPos");

            // Retrieve locations from shader code
            int lightDirLoc = GetShaderLocation(lightingShader, "lightDir");
            int lightColLoc = GetShaderLocation(lightingShader, "lightColor");
            int ambientLoc = GetShaderLocation(lightingShader, "ambient");
            lightVPLoc = GetShaderLocation(lightingShader, "lightVP");
            shadowMapLoc = GetShaderLocation(lightingShader, "shadowMap");
            int shadowMapResolutionLoc = GetShaderLocation(lightingShader, "shadowMapResolution");

            // Define values
            float[] ambient = new[] { 0.5f, 0.5f, 0.5f, 1.0f };
            int shadowMapResolution = SHADOW_MAP_RESOLUTION;
            Vector4 normalizedColor = ColorNormalize(lightColor);

            // Set values at shader locations
            SetShaderValue(lightingShader, lightDirLoc, &lightDirection, ShaderUniformDataType.Vec3);
            SetShaderValue(lightingShader, lightColLoc, &normalizedColor, ShaderUniformDataType.Vec4);
            SetShaderValue(lightingShader, ambientLoc, ambient, ShaderUniformDataType.Vec4);
            SetShaderValue(lightingShader, shadowMapResolutionLoc, &shadowMapResolution, ShaderUniformDataType.Int);
        }

        public void SetLightMatrix(Matrix4x4 mvp)
        {
            SetShaderValueMatrix(lightingShader, lightVPLoc, mvp);
        }

        public void UpdateShadowMap(ShadowMap shadow)
        {
            Rlgl.EnableShader(lightingShader.Id);
            int slot = 10;
            Rlgl.ActiveTextureSlot(10);
            Rlgl.EnableTexture(shadow.Map.Depth.Id);
            Rlgl.SetUniform(shadowMapLoc, &slot, 4, 1);
        }

        /// <summary>
        /// Load all the shaders
        /// </summary>
        private void LoadShaders()
        {
            // Load outline shader
            outlineShader = LoadShader("src/shaders/outline.vs", "src/shaders/outline.fs");
            // Load tiling shader
            tilingShader = LoadShader("src/shaders/tiling.vs", "src/shaders/tiling.fs");
            // Load skybox shader
            skyboxShader = LoadShader("src/shaders/skybox.vs", "src/shaders/skybox.fs");
            SetShaderValue(skyboxShader, GetShaderLocation(skyboxShader, "environmentMap"), (int)MaterialMapIndex.Cubemap, ShaderUniformDataType.Int);
            SetShaderValue(skyboxShader, GetShaderLocation(skyboxShader, "doGamma"), 1, ShaderUniformDataType.Int);
            SetShaderValue(skyboxShader, GetShaderLocation(skyboxShader, "vflipped"), 1, ShaderUniformDataType.Int);
            // Load cubemap shader
            cubemapShader = LoadShader("src/shaders/cubemap.vs", "src/shaders/cubemap.fs");
            SetShaderValue(cubemapShader, GetShaderLocation(cubemapShader, "equirectangularMap"), 0, ShaderUniformDataType.Int);
        }

        /// <summary>
        /// Load all the shader materials
        /// </summary>
        private void LoadMaterials()
        {
            // Init the outline material
            outlineMaterial = LoadMaterialDefault();
            outlineMaterial.Shader = outlineShader;

            // Init the skybox material
            skyboxMaterial = LoadMaterialDefault();
            skyboxMaterial.Shader = skyboxShader;
        }
        /// <summary>
        /// Unload all the shader center data
        /// </summary>
        public void UnloadShaderCenter()
        {
            UnloadMaterials();
            UnloadShaders();
        }
        /// <summary>
        /// Unload all the shaders
        /// </summary>
        private void UnloadShaders()
        {
            UnloadShader(SkyboxShader);
            UnloadShader(cubemapShader);
            UnloadShader(outlineShader);
        }
        /// <summary>
        /// Unload all the shader materials 
        /// </summary>
        private void UnloadMaterials()
        {
            UnloadMaterial(OutlineMaterial);
            UnloadMaterial(SkyboxMaterial);
        }
        /// <summary>
        /// Generate Cubemap according to the passed HDR texture
        /// </summary>
        /// <param name="panorama">Texture to use/param>
        /// <param name="size">FrameBuffer size</param>
        /// <param name="format">Format of the cubemap</param>
        /// <returns></returns>
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
            Rlgl.EnableShader(cubemapShader.Id);

            // Define projection matrix and send it to the shader
            Matrix4x4 matFboProjection = Raymath.MatrixPerspective(90.0f * DEG2RAD, 1.0f, Rlgl.CULL_DISTANCE_NEAR, Rlgl.CULL_DISTANCE_FAR);
            Rlgl.SetUniformMatrix(cubemapShader.Locs[(int)ShaderLocationIndex.MatrixProjection], matFboProjection);

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
                Rlgl.SetUniformMatrix(cubemapShader.Locs[(int)ShaderLocationIndex.MatrixView], fboViews[i]);

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
        /// <summary>
        /// Set the skybox material texture
        /// </summary>
        /// <param name="tex"></param>
        public void SetCubemap(Texture2D tex)
        {
            SetMaterialTexture(ref skyboxMaterial, MaterialMapIndex.Cubemap, tex);
        }
    }
}