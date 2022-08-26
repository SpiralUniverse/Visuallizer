#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec4 aColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec4 vColor;

float f(float x, float y)
{
    /*if(pow((x), 2) + pow((y), 2) - 5 > sin(x * 4) + cos(y * 4))
    {
        return 2 + sin(x * 4) + cos(y * 4);
    }
    return log(x) * sign(x);*/
    
    return /*return function*/;
}


void main() 
{ 
    float fx = f(aPosition.x, aPosition.y);
    gl_Position = vec4(aPosition.x, fx, aPosition.y, 1.0f) * model * view * projection;//Note : it works
    vColor = vec4(1 - fx + sin(aPosition.y), log(fx), fx, 1.0f);
}
