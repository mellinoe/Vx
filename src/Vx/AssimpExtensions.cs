namespace VdGfx
{
    internal static class AssimpExtensions
    {
        public static System.Numerics.Vector3 ToSystemVec3(this Assimp.Vector3D v)
        {
            return new System.Numerics.Vector3(v.X, v.Y, v.Z);
        }
    }
}
