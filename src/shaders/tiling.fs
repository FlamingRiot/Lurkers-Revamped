#version 330

in vec3 fragPosition;
in vec3 fragNormal;
in vec2 uv;

uniform sampler2D texture0;
uniform vec4 colDiffuse;

out vec4 finalColor;

void main()
{
    float tx = fract(uv.x);
    float ty = fract(uv.y);

    vec4 texelColor = texture(texture0, vec2(tx, ty));

    finalColor = texelColor*colDiffuse;
}