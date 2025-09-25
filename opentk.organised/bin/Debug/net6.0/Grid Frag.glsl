#version 430 core

out vec4 o_frag_color;

in vec4 ps_vertex_color;
in vec4 ps_vertex_position;
in mat4 ps_mvp;

void main()
{
    // Calculate distance from camera for fog effect
    float distance = length(ps_vertex_position.xyz);

    // Enhanced fog system for professional look
    float fog_start = 50;
    float fog_end = 250;

    float fog_factor = (distance - fog_start) / (fog_end - fog_start);
    fog_factor = clamp(fog_factor, 0, 1);
    
    // Smooth fog transition with professional dark background
    fog_factor = smoothstep(0.0, 1.0, fog_factor);
    
    // Professional background color matching the clear color
    vec4 fogColor = vec4(0.08, 0.08, 0.12, 1.0);
    
    // Apply atmospheric perspective
    vec4 finalColor = mix(ps_vertex_color, fogColor, fog_factor);
    
    // Add subtle brightness based on distance for depth perception
    float brightness = 1.0 - fog_factor * 0.3;
    finalColor.rgb *= brightness;
    
    // Anti-flickering: ensure minimum alpha for close grid lines
    if (distance < fog_start * 1.5) {
        finalColor.a = max(finalColor.a, 0.3);
    }
    
    // Depth-based fade to prevent z-fighting
    float depthFade = 1.0 - pow(fog_factor, 2.0);
    finalColor.a *= depthFade;
    
    o_frag_color = finalColor;
} 