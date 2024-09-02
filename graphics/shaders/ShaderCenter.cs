using Raylib_cs;
using static Raylib_cs.Raylib;

namespace uniray_Project
{
    public class ShaderCenter
    {
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
        /// The outline shader
        /// </summary>
        public Shader OutlineShader { get { return outlineShader; } }
        /// <summary>
        /// The texture-tiling shader for the terrain
        /// </summary>
        public Shader TilingShader { get { return tilingShader; } }
        /// <summary>
        /// The outline material
        /// </summary>
        public Material OutlineMaterial { get { return outlineMaterial; } }
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
        /// <summary>
        /// Load all the shaders
        /// </summary>
        private void LoadShaders()
        {
            // Load outline shader
            outlineShader = LoadShader("src/shaders/outline.vs", "src/shaders/outline.fs");
            // Load tiling shader
            tilingShader = LoadShader("src/shaders/tiling.vs", "src/shaders/tiling.fs");
        }
        /// <summary>
        /// Load all the shader materials
        /// </summary>
        private void LoadMaterials()
        {
            // Init the outline material
            outlineMaterial = LoadMaterialDefault();
            outlineMaterial.Shader = outlineShader;
        }
    }
}