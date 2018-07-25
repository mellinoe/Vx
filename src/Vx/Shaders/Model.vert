#version 450

layout(set = 0, binding = 0) uniform ViewProjection
{
    mat4 _viewProjection;
};

layout(set = 1, binding = 0) uniform WorldAndInverse
{
    mat4 _world;
    mat4 _inverseWorld;
};

layout(constant_id = 100) const bool ClipSpaceInvertedY = true;
layout(constant_id = 101) const bool TextureCoordinatesInvertedY = false;
layout(constant_id = 102) const bool ReverseDepthRange = true;

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;

layout(location = 0) out vec3 fsin_Normal;

void main()
{
    gl_Position = _viewProjection * _world * vec4(Position, 1);
    fsin_Normal = normalize((_inverseWorld * vec4(Normal, 1)).xyz);
}
