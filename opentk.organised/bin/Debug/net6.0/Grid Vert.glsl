﻿#version 430 core

out gl_PerVertex
{
    vec4 gl_Position;
};

layout (location = 0) in vec3 i_position;
layout (location = 1) in vec3 i_color;


uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec4 ps_vertex_color;
out vec4 ps_vertex_position;
out mat4 ps_mvp;

void main()
{
    ps_mvp = model * view * projection;
    ps_vertex_position = vec4(i_position, 1.0) * ps_mvp ;
    ps_vertex_color = vec4(i_color, 1.0);

    gl_Position = ps_vertex_position;
}