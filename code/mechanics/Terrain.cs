using System.Numerics;

/* 
    Author: Evan Comtesse
    Last modified: 15.09.2024
    Description: Represents a terrain object for the game
 */

namespace Raylib_cs
{
    /// <summary>Represents a terrain object for the game</summary>
    public struct Terrain
    {
        /// <summary>Terrain mesh</summary>
        public Mesh Mesh;
        
        /// <summary>Terrain material</summary>
        public Material Material;
        
        /// <summary>Terrain transform</summary>
        public Matrix4x4 Transform;

        /// <summary>Creates a terrain based based on a specific mesh and material</summary>
        /// <param name="mesh">The <see cref="Raylib_cs.Mesh"/> to use for terrain rendering</param>
        /// <param name="material">The <see cref="Raylib_cs.Material"/> to use for rendering</param>
        public Terrain(Mesh mesh, Material material)
        {
            this.Mesh = mesh;
            this.Material = material;
            this.Transform = Matrix4x4.Identity;
        }

        /// <summary>Creates a terrain based based on a specific mesh, material and transform</summary>
        /// <param name="mesh">The <see cref="Raylib_cs.Mesh"/> to use for terrain rendering</param>
        /// <param name="material">The <see cref="Raylib_cs.Material"/> to use for rendering</param>
        /// <param name="transform">The transform (<see cref="Matrix4x4"/>) to use for terrain rendering</param>
        public Terrain(Mesh mesh, Material material, Matrix4x4 transform)
        {
            this.Mesh = mesh;
            this.Material = material;
            this.Transform = transform;
        }

        /// <summary>Sets the shader to use for terrain rendering</summary>
        /// <param name="shader"></param>
        public void SetShader(Shader shader)
        {
            this.Material.Shader = shader;
        }
    }
}