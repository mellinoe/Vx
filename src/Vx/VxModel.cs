using Assimp;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace VdGfx
{
    public class VxModel
    {
        internal DeviceBuffer VertexBuffer { get; }
        internal DeviceBuffer IndexBuffer { get; }
        internal List<(uint startIndex, int baseVertex, uint indexCount)> ModelRegions { get; }
            = new List<(uint startIndex, int baseVertex, uint indexCount)>();
        internal IndexFormat IndexFormat { get; }

        public VxModel(string path)
        {
            AssimpContext ctx = new AssimpContext();
            Scene scene = ctx.ImportFile(
                path,
                PostProcessSteps.FlipWindingOrder | PostProcessSteps.FlipUVs | PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals);

            List<VertexPositionNormal> positions = new List<VertexPositionNormal>();
            List<uint> indices = new List<uint>();

            foreach (Mesh mesh in scene.Meshes)
            {
                uint startIndex = (uint)indices.Count;
                int baseVertex = positions.Count;

                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    positions.Add(new VertexPositionNormal
                    {
                        Position = mesh.Vertices[i].ToSystemVec3(),
                        Normal = -mesh.Normals[i].ToSystemVec3()
                    });
                }

                foreach (Face face in mesh.Faces)
                {
                    if (face.IndexCount == 3)
                    {
                        indices.Add((uint)face.Indices[0]);
                        indices.Add((uint)face.Indices[1]);
                        indices.Add((uint)face.Indices[2]);
                    }
                }

                ModelRegions.Add((startIndex, baseVertex, (uint)(indices.Count - startIndex)));
            }

            VertexBuffer = VxContext.Instance.Factory.CreateBuffer(
                new BufferDescription((uint)(positions.Count * Unsafe.SizeOf<VertexPositionNormal>()), BufferUsage.VertexBuffer));
            VxContext.Instance.Device.UpdateBuffer(VertexBuffer, 0, positions.ToArray());

            IndexBuffer = VxContext.Instance.Factory.CreateBuffer(
                new BufferDescription((uint)indices.Count * sizeof(int), BufferUsage.IndexBuffer));
            VxContext.Instance.Device.UpdateBuffer(IndexBuffer, 0, indices.ToArray());
            IndexFormat = IndexFormat.UInt32;
        }

        internal VxModel(DeviceBuffer vertexBuffer, DeviceBuffer indexBuffer, uint indexCount, IndexFormat indexFormat)
        {
            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;
            ModelRegions.Add((0, 0, indexCount));
            IndexFormat = indexFormat;
        }

        public static VxModel Cube => s_cubeModel.Value;
        internal static Lazy<VxModel> s_cubeModel = new Lazy<VxModel>(CreateCube);

        internal static VxModel CreateCube()
        {
            VertexPositionNormal[] vertices =
            {
                // Front
                new VertexPositionNormal(new Vector3(-0.5f, -0.5f, 0.5f),   new Vector3(0, 0, 1)),
                new VertexPositionNormal(new Vector3(-0.5f, 0.5f, 0.5f),    new Vector3(0, 0, 1)),
                new VertexPositionNormal(new Vector3(0.5f, 0.5f, 0.5f),     new Vector3(0, 0, 1)),
                new VertexPositionNormal(new Vector3(0.5f, -0.5f, 0.5f),    new Vector3(0, 0, 1)),
                // Back
                new VertexPositionNormal(new Vector3(-0.5f, -0.5f, -0.5f),  new Vector3(0, 0, -1)),
                new VertexPositionNormal(new Vector3(-0.5f, 0.5f, -0.5f),   new Vector3(0, 0, -1)),
                new VertexPositionNormal(new Vector3(0.5f, 0.5f, -0.5f),    new Vector3(0, 0, -1)),
                new VertexPositionNormal(new Vector3(0.5f, -0.5f, -0.5f),   new Vector3(0, 0, -1)),
                // Left
                new VertexPositionNormal(new Vector3(-0.5f, -0.5f, 0.5f),   new Vector3(-1, 0, 0)),
                new VertexPositionNormal(new Vector3(-0.5f, 0.5f, 0.5f),    new Vector3(-1, 0, 0)),
                new VertexPositionNormal(new Vector3(-0.5f, 0.5f, -0.5f),   new Vector3(-1, 0, 0)),
                new VertexPositionNormal(new Vector3(-0.5f, -0.5f, -0.5f),  new Vector3(-1, 0, 0)),
                // Right
                new VertexPositionNormal(new Vector3(0.5f, -0.5f, 0.5f),   new Vector3(1, 0, 0)),
                new VertexPositionNormal(new Vector3(0.5f, 0.5f, 0.5f),    new Vector3(1, 0, 0)),
                new VertexPositionNormal(new Vector3(0.5f, 0.5f, -0.5f),   new Vector3(1, 0, 0)),
                new VertexPositionNormal(new Vector3(0.5f, -0.5f, -0.5f),  new Vector3(1, 0, 0)),
                // Bottom
                new VertexPositionNormal(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0, -1, 0)),
                new VertexPositionNormal(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0, -1, 0)),
                new VertexPositionNormal(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0, -1, 0)),
                new VertexPositionNormal(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, -1, 0)),
                // Top
                new VertexPositionNormal(new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0, 1, 0)),
                new VertexPositionNormal(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, 1, 0)),
                new VertexPositionNormal(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0, 1, 0)),
                new VertexPositionNormal(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0, 1, 0)),
            };

            ushort[] indices =
            {
                0, 1, 2, 0, 2, 3,
                4, 6, 5, 4, 7, 6,
                8, 10, 9, 8, 11, 10,
                12, 13, 14, 12, 14, 15,
                16, 17, 18, 16, 18, 19,
                20, 22, 21, 20, 23, 22,
            };

            DeviceBuffer vb = VxContext.Instance.Factory.CreateBuffer(
                new BufferDescription((uint)vertices.Length * (uint)Unsafe.SizeOf<VertexPositionNormal>(), BufferUsage.VertexBuffer));
            VxContext.Instance.Device.UpdateBuffer(vb, 0, vertices);

            DeviceBuffer ib = VxContext.Instance.Factory.CreateBuffer(
                new BufferDescription((uint)indices.Length * sizeof(ushort), BufferUsage.IndexBuffer));
            VxContext.Instance.Device.UpdateBuffer(ib, 0, indices);

            return new VxModel(vb, ib, (uint)indices.Length, IndexFormat.UInt16);
        }
    }

    struct VertexPositionNormal
    {
        public Vector3 Position;
        public Vector3 Normal;

        public VertexPositionNormal(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }
    }
}
