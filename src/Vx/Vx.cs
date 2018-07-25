using System;
using System.Numerics;
using Veldrid;

namespace VdGfx
{
    public static class Vx
    {
        public static bool IsRunning => VxContext.IsRunning;

        public static void Initialize() => VxContext.Initialize();
        public static void Terminate() => VxContext.Terminate();

        public static void Camera() => VxContext.Instance.SetCameraActive();

        public static void Model(VxModel model) => VxContext.Instance.SetActiveModel(model);

        public static void Position(float x, float y, float z) => VxContext.Instance.SetPosition(new Vector3(x, y, z));
        public static void Position(Vector3 position) => VxContext.Instance.SetPosition(position);

        public static void Rotation(Quaternion rotation) => VxContext.Instance.SetRotation(rotation);

        public static void Draw() => VxContext.Instance.Draw();

        public static void Scale(float scale) => VxContext.Instance.SetScale(new Vector3(scale));
        public static void Scale(float xScale, float yScale, float zScale) => VxContext.Instance.SetScale(new Vector3(xScale, yScale, zScale));
        public static void Scale(Vector3 scale) => VxContext.Instance.SetScale(scale);

        public static void ColorMap(VxTexture tex) => VxContext.Instance.SetColorMap(tex);

        public static void SolidColor(float r, float g, float b) => VxContext.Instance.SetColor(new RgbaFloat(r, g, b, 1));
        public static void SolidColor(float r, float g, float b, float a) => VxContext.Instance.SetColor(new RgbaFloat(r, g, b, a));
        public static void SolidColor(RgbaFloat color) => VxContext.Instance.SetColor(color);

        public static void ClearColor(RgbaFloat color) => VxContext.Instance.ClearColor = color;

        public static void EndFrame() => VxContext.Instance.EndFrame();
        public static float FrameTime => VxContext.Instance.FrameTime;
    }
}
