using System;
using System.Collections.Generic;
using System.IO;
using Veldrid;
using Veldrid.SPIRV;

namespace VdGfx
{
    public static class ShaderHelpers
    {
        public static Shader[] LoadSet(GraphicsDevice gd, ResourceFactory factory, string name)
        {
            ShaderDescription vsDesc = new ShaderDescription(
                ShaderStages.Vertex,
                LoadBytes(name, ShaderStages.Vertex),
                "main");
            ShaderDescription fsDesc = new ShaderDescription(
                ShaderStages.Fragment,
                LoadBytes(name, ShaderStages.Fragment),
                "main");
            CrossCompileOptions options = GetCompileOptions(gd);

            return factory.CreateFromSpirv(vsDesc, fsDesc, options);
        }

        private static byte[] LoadBytes(string name, ShaderStages stage)
        {
            string extension = string.Empty;
            switch (stage)
            {
                case ShaderStages.Vertex:
                    extension = ".vert.spv";
                    break;
                case ShaderStages.Fragment:
                    extension = ".frag.spv";
                    break;
                case ShaderStages.Compute:
                    extension = ".comp.spv";
                    break;
                default:
                    throw new VxException("Invalid stage: " + stage);
            }
            string fullPath = Path.Combine(AppContext.BaseDirectory, "Shaders", name + extension);
            return File.ReadAllBytes(fullPath);
        }

        private static CrossCompileOptions GetCompileOptions(GraphicsDevice gd)
        {
            bool fixClipZ = false;
            bool invertY = false;
            List<SpecializationConstant> specializations = new List<SpecializationConstant>();
            specializations.Add(new SpecializationConstant(102, gd.IsDepthRangeZeroToOne));
            switch (gd.BackendType)
            {
                case GraphicsBackend.Direct3D11:
                case GraphicsBackend.Metal:
                    specializations.Add(new SpecializationConstant(100, false));
                    break;
                case GraphicsBackend.Vulkan:
                    specializations.Add(new SpecializationConstant(100, true));
                    break;
                case GraphicsBackend.OpenGL:
                case GraphicsBackend.OpenGLES:
                    specializations.Add(new SpecializationConstant(100, false));
                    specializations.Add(new SpecializationConstant(101, true));
                    fixClipZ = !gd.IsDepthRangeZeroToOne;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return new CrossCompileOptions(fixClipZ, invertY, specializations.ToArray());
        }
    }
}
