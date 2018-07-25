using System.Numerics;

namespace VdGfx
{
    internal readonly struct DrawSubmission
    {
        public readonly VxModel Model;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Scale;
        public readonly Vector4 Color;

        public DrawSubmission(VxModel model, Vector3 position, Quaternion rotation, Vector3 scale, Vector4 color)
        {
            Model = model;
            Position = position;
            Rotation = rotation;
            Scale = scale;
            Color = color;
        }
    }
}
