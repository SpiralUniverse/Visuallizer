#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec4 aColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec4 vColor;


void main() 
{
    gl_Position = vec4(aPosition.x, aPosition.y, aPosition.z, 1.0f)* model * view * projection;//Note : it works
    //gl_Position = projection * view * vec4(gridPlane[gl_VertexID].xyz, 1.0); //NOTE: doesnt work
    vColor = aColor;
}
