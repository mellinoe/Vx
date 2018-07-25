using System;
using System.Collections.Generic;
using System.Text;

namespace VdGfx
{
    public class VxApplication
    {
        private readonly Action _render;

        public VxApplication(Action render)
        {
            _render = render;
        }

        public static void Run(Action render)
        {
            VxApplication app = new VxApplication(render);
            app.Run();
        }

        public void Run()
        {
            Vx.Initialize();

            while (Vx.IsRunning)
            {
                _render();
                Vx.EndFrame();
            }

            Vx.Terminate();
        }
    }
}
