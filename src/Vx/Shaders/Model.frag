#version 450

layout(set = 2, binding = 0) uniform ModelParams
{
    vec4 _tintColor;
};

layout(set = 0, binding = 1) uniform SceneInfo
{
    vec4 _lightDir;
    vec4 _lightColor;
};

layout(location = 0) in vec3 fsin_Normal;
layout(location = 0) out vec4 fsout_Color0;

void main()
{
    float lightEffect = dot(fsin_Normal, -_lightDir.xyz);
    fsout_Color0 = vec4(lightEffect, lightEffect, lightEffect, 1);
    return;
    vec4 surfaceColor = _tintColor;
    vec4 litColor = vec4(0, 0, 0, 1);
    if (lightEffect > 0)
    {
        vec4 litColor = lightEffect * surfaceColor;
    }

    vec4 ambientColor = surfaceColor * vec4(0.2, 0.2, 0.2, 1);
    fsout_Color0 = litColor + ambientColor;
}
