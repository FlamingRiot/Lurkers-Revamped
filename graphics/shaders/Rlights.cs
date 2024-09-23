using static Raylib_cs.Raylib;
using System.Numerics;
using Raylib_cs;

namespace uniray_Project
{
    public struct Light
    {
        public bool Enabled;
        public LightType Type;
        public Vector3 Position;
        public Vector3 Target;
        public Color Color;

        public int EnabledLoc;
        public int TypeLoc;
        public int PosLoc;
        public int TargetLoc;
        public int ColorLoc;
    }

    public enum LightType
    {
        Directional,
        Point
    }

    public static class Rlights
    {
        /// <summary>Creates a light object</summary>
        /// <param name="index">Light index in the shader's light array</param>
        /// <param name="type">Light behavior</param>
        /// <param name="pos">Light world position</param>
        /// <param name="target">Light target position</param>
        /// <param name="color">Light emission color</param>
        /// <param name="shader">Shader to use for creation</param>
        /// <returns></returns>
        public static Light CreateLight(
            int index,
            LightType type,
            Vector3 pos,
            Vector3 target,
            Color color,
            Shader shader
        )
        {
            Light light = new();

            // Set values
            light.Enabled = true;
            light.Type = type;
            light.Position = pos;
            light.Target = target;
            light.Color = color;

            // Set array corresponding index name
            string enabledName = "lights[" + index + "].enabled";
            string typeName = "lights[" + index + "].type";
            string posName = "lights[" + index + "].position";
            string targetName = "lights[" + index + "].target";
            string colorName = "lights[" + index + "].color";

            // Pass values to shader
            light.EnabledLoc = GetShaderLocation(shader, enabledName);
            light.TypeLoc = GetShaderLocation(shader, typeName);
            light.PosLoc = GetShaderLocation(shader, posName);
            light.TargetLoc = GetShaderLocation(shader, targetName);
            light.ColorLoc = GetShaderLocation(shader, colorName);
            
            // Initially update light values for shader
            UpdateLightValues(shader, light);

            return light;
        }

        /// <summary>Updates light values in the lighting shader</summary>
        /// <param name="shader">Shader to update</param>
        /// <param name="light">Light to update</param>
        public static void UpdateLightValues(Shader shader, Light light)
        {
            // Send to shader light enabled state and type
            SetShaderValue(
                shader,
                light.EnabledLoc,
                light.Enabled ? 1 : 0,
                ShaderUniformDataType.Int
            );
            SetShaderValue(shader, light.TypeLoc, (int)light.Type, ShaderUniformDataType.Int);

            // Send to shader light target position values
            SetShaderValue(shader, light.PosLoc, light.Position, ShaderUniformDataType.Vec3);

            // Send to shader light target position values
            SetShaderValue(shader, light.TargetLoc, light.Target, ShaderUniformDataType.Vec3);

            // Send to shader light color values
            float[] color = new[]
            {
                (float)light.Color.R/ (float)255,
                (float)light.Color.G / (float)255,
                (float)light.Color.B / (float)255,
                (float)light.Color.A / (float)255
            };
            SetShaderValue(shader, light.ColorLoc, color, ShaderUniformDataType.Vec4);
        }
    }
}
