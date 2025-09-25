#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec4 aColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float minHeight;
uniform float maxHeight;

out vec4 vColor;

float f(float x, float y)
{
    return /*return function*/;
}

vec4 heightToHeatMapColor(float height, float minH, float maxH) {
    float normalizedHeight = (height - minH) / (maxH - minH);
    normalizedHeight = clamp(normalizedHeight, 0.0, 1.0);
    
    vec4 color;
    if (normalizedHeight < 0.2) {
        // Deep blue to teal
        float t = normalizedHeight / 0.2;
        color = mix(vec4(0.0, 0.1, 0.4, 1.0), vec4(0.0, 0.5, 0.5, 1.0), t);
    } else if (normalizedHeight < 0.4) {
        // Teal to green
        float t = (normalizedHeight - 0.2) / 0.2;
        color = mix(vec4(0.0, 0.5, 0.5, 1.0), vec4(0.0, 0.8, 0.2, 1.0), t);
    } else if (normalizedHeight < 0.6) {
        // Green to yellow
        float t = (normalizedHeight - 0.4) / 0.2;
        color = mix(vec4(0.0, 0.8, 0.2, 1.0), vec4(0.9, 0.9, 0.1, 1.0), t);
    } else if (normalizedHeight < 0.8) {
        // Yellow to orange
        float t = (normalizedHeight - 0.6) / 0.2;
        color = mix(vec4(0.9, 0.9, 0.1, 1.0), vec4(1.0, 0.5, 0.0, 1.0), t);
    } else {
        // Orange to red
        float t = (normalizedHeight - 0.8) / 0.2;
        color = mix(vec4(1.0, 0.5, 0.0, 1.0), vec4(1.0, 0.0, 0.0, 1.0), t);
    }
    
    return color;
}

void main() 
{ 
    float fx = f(aPosition.x, aPosition.y);
    gl_Position = vec4(aPosition.x, fx, aPosition.y, 1.0f) * model * view * projection;
    vColor = heightToHeatMapColor(fx, minHeight, maxHeight);
}
