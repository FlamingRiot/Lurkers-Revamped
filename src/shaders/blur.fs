#version 330

uniform sampler2D texture0;  // Texture de la sc�ne actuelle
uniform sampler2D prevFrame; // Texture de la frame pr�c�dente

uniform float blurAmount;    // Intensit� du flou
uniform float chromaticAmount;  // Intensit� de l'aberration chromatique
uniform float time;

in vec2 fragTexCoord;
out vec4 finalColor;

void main()
{
    vec2 uv = fragTexCoord;

    // Create horizontal line offset
    float offset = sin(uv.y * 50.0 + time) * 0.0001;
    uv.y += offset;

    // Calculate noise
    float noise = fract(sin(dot(uv.xy, vec2(12.9898, 78.233))) * 43758.5453);

    // R�cup�rer la couleur de la sc�ne actuelle et de la frame pr�c�dente
    vec4 currentColor = texture(texture0, uv);
    vec4 prevColor = texture(prevFrame, uv);
    
    // Calculer la combinaison de flou
    float weightCurrent = 1.0 / (blurAmount + 1.0);
    float weightPrev = blurAmount / (blurAmount + 1.0);
    vec4 blurredColor = (weightCurrent * currentColor) + (weightPrev * prevColor);
    
    // Ajouter l'effet d'aberration chromatique
    vec2 chromOffset = vec2(chromaticAmount * 0.005);

    // D�calage pour les canaux de couleur
    float r = texture(texture0, uv + chromOffset).r;
    float g = texture(texture0, uv).g;
    float b = texture(texture0, uv - chromOffset).b;

    // Combinaison des canaux RGB dans une couleur aberr�e
    vec4 aberrationColor = vec4(r, g, b, 1.0);

    // Combiner la couleur aberr�e avec la couleur flout�e, mais �viter la multiplication directe
    finalColor = mix(blurredColor, aberrationColor, 0.5); // M�lange �quilibr� entre flou et aberration

    // Noise addition
    finalColor.rgb += noise * 0.15;

    // Gamma correction
    finalColor = pow(finalColor, vec4(1.2));
}
