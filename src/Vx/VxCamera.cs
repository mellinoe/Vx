using System;
using System.Numerics;
using Veldrid;

namespace VdGfx
{
    public class VxCamera
    {
        private float _yaw;
        private float _pitch;
        private Vector2 _lastMousePos;
        private float _mouseSensitivity = .01f;
        private float _moveSpeed = 5f;

        public Vector3 Position { get; set; }

        public void Update()
        {
            Vector2 newMousePos = VxInput.MousePosition;
            Vector2 mouseDelta = newMousePos - _lastMousePos;
            _lastMousePos = newMousePos;
            _yaw = _yaw + -mouseDelta.X * _mouseSensitivity;
            _pitch = MathUtil.Clamp(_pitch - mouseDelta.Y * _mouseSensitivity, -1.5f, 1.5f);

            Quaternion camRot = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0f);
            Vector3 camForward = Vector3.Transform(-Vector3.UnitZ, camRot);
            Vector3 camRight = Vector3.Cross(camForward, Vector3.UnitY);
            Vector3 camUp = Vector3.Cross(camRight, camForward);

            Vector3 moveDir = Vector3.Zero;
            if (VxInput.GetKey(Key.W)) { moveDir += camForward; }
            if (VxInput.GetKey(Key.S)) { moveDir += -camForward; }
            if (VxInput.GetKey(Key.A)) { moveDir += -camRight; }
            if (VxInput.GetKey(Key.D)) { moveDir += camRight; }
            if (VxInput.GetKey(Key.Q)) { moveDir += -camUp; }
            if (VxInput.GetKey(Key.E)) { moveDir += camUp; }
            if (moveDir != Vector3.Zero)
            {
                float turboFactor = VxInput.GetKey(Key.ControlLeft) ? 0.125f : VxInput.GetKey(Key.ShiftLeft) ? 2f : 1f;
                Position += Vector3.Normalize(moveDir) * _moveSpeed * Vx.FrameTime * turboFactor;
            }

            Vx.Camera();
            Vx.Position(Position);
            Vx.Rotation(camRot);
        }
    }
}
