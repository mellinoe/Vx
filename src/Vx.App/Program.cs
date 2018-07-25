using ImGuiNET;
using System;
using System.Numerics;
using VdGfx;
using Veldrid;

namespace VxDemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Vx.Initialize();

            VxModel cessnaModel = new VxModel("Models/cessna.obj");
            VxModel trumpetModel = new VxModel("Models/trumpet.obj");

            VxCamera camera = new VxCamera();

            while (Vx.IsRunning)
            {
                camera.Update();

                Vx.Model(cessnaModel);
                Vx.Position(0, 1, -1);
                Vx.Scale(1);
                Vx.SolidColor(RgbaFloat.Red);
                Vx.Draw();

                Vx.Model(trumpetModel);
                Vx.Position(0, 10, -1);
                Vx.Scale(0.05f);
                Vx.SolidColor(RgbaFloat.Yellow);
                Vx.Draw();

                Vx.Model(VxModel.Cube);
                Vx.Position(3, 5, -9);
                Vx.Scale(3);
                Vx.SolidColor(RgbaFloat.Blue);
                Vx.Draw();

                Vx.Position(-3, 2, -1);
                Vx.Scale(1.5f);
                Vx.SolidColor(RgbaFloat.Yellow);
                Vx.Draw();

                Vx.SolidColor(RgbaFloat.White);
                Vx.Scale(0.075f);
                for (int i = 0; i < 10; i++)
                {
                    Vx.Position(-4.15f + i * 0.175f, 3f, -1f);
                    Vx.Rotation(Quaternion.CreateFromAxisAngle(-Vector3.UnitZ, i * 0.2f));
                    Vx.Draw();
                }

                Vx.EndFrame();
            }

            Vx.Terminate();
        }
    }
}
