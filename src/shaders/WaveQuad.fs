#version 330

uniform vec4 colDiffuse;
uniform sampler2D texture0;  // Texture de la scène actuelle
uniform float time;

in vec3 fragPosition;
in vec3 fragNormal;
in vec2 fragTexCoord;

out vec4 finalColor;

void main()
{
    vec4 texelColor = vec4(normalize(vec3(245, 66, 230)), 1.0);
    

    finalColor = texelColor;
}
