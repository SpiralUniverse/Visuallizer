#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec4 aColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec4 vColor;

float f(float x, float y)
{
    return /*return function*/;
}

float rand(vec2 co){
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

void main() 
{ 
    float fx = f(aPosition.x, aPosition.y);
    gl_Position = vec4(aPosition.x, fx, aPosition.y, 1.0f) * model * view * projection;//Note : it works
    vec2 _co = vec2(aPosition.x, aPosition.y);
    vColor = vec4(rand(vec2(aPosition.x, aPosition.y)), rand(vec2(aPosition.y, aPosition.z)), rand(vec2(aPosition.x, aPosition.z)), 1);
}
