using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Lurkers_revamped
{
    /// <summary>Represents a PBR Material</summary>
    public struct PBR
    {
        /// <summary>Raylib material for the PBR</summary>
        public Material Material;

        /// <summary>Returns a PBR Material with all the textures loaded</summary>
        /// <param name="dir">Relative path to the folder containg the texture maps</param>
        /// <param name="shader">The PBR Material processing shader to use for render</param>
        public PBR(string dir, Shader shader)
        {
            Material = LoadMaterialDefault();

            // Texture maps
            SetMaterialTexture(ref Material, MaterialMapIndex.Albedo, LoadTexture(dir + "albedo.png"));
            SetMaterialTexture(ref Material, MaterialMapIndex.Metalness, LoadTexture(dir + "metalness.png"));
            SetMaterialTexture(ref Material, MaterialMapIndex.Normal, LoadTexture(dir + "normal.png"));
            SetMaterialTexture(ref Material, MaterialMapIndex.Roughness, LoadTexture(dir + "roughness.png"));
            SetMaterialTexture(ref Material, MaterialMapIndex.Occlusion, LoadTexture(dir + "occlusion.png"));
            SetMaterialTexture(ref Material, MaterialMapIndex.Height, LoadTexture(dir + "height.png"));

            Material.Shader = shader;
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            hash.Add(Material);

            return hash.ToHashCode();   
        }
    }
}
