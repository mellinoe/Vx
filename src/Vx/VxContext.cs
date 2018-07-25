using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;

namespace VdGfx
{
    internal class VxContext
    {
        private static VxContext s_instance;
        private static readonly object s_initializationLock = new object();

        private readonly Pipeline _modelPipeline;
        private readonly DeviceBuffer _viewProjectionBuffer;
        private readonly DeviceBuffer _sceneInfoBuffer;
        private readonly ResourceSet _viewProjectionSet;
        private readonly DeviceBuffer _worldBuffer;
        private readonly ResourceSet _worldSet;
        private readonly DeviceBuffer _modelParamsBuffer;
        private readonly ResourceSet _modelParamsSet;
        private readonly ImGuiRenderer _imguiRenderer;
        private readonly CommandList _cl;
        private readonly List<DrawSubmission> _drawSubmissions = new List<DrawSubmission>();
        private readonly Stopwatch _sw;
        private double _lastFrameTime;

        // Context state
        private bool _modelActive;
        private VxModel _model;
        private Vector3 _modelPosition;
        private Quaternion _modelRotation = Quaternion.Identity;
        private Vector3 _modelScale = Vector3.One;
        private Vector4 _modelColor = new Vector4(1, 1, 1, 1);

        private bool _cameraActive;
        private Vector3 _cameraPosition;
        private Quaternion _cameraRotation = Quaternion.Identity;

        internal GraphicsDevice Device { get; }
        internal Sdl2Window Window { get; }
        internal ResourceFactory Factory { get; }

        internal void SetCameraActive()
        {
            _cameraActive = true;
            _modelActive = false;
        }

        internal void SetPosition(Vector3 position)
        {
            if (_cameraActive)
            {
                _cameraPosition = position;
            }
            else
            {
                _modelPosition = position;
            }
        }

        internal void SetRotation(Quaternion rotation)
        {
            if (_cameraActive)
            {
                _cameraRotation = rotation;
            }
            else
            {
                _modelRotation = rotation;
            }
        }

        internal void SetScale(Vector3 scale)
        {
            _modelScale = scale;
        }

        internal void SetColorMap(VxTexture tex)
        {
            throw new NotImplementedException();
        }

        internal void SetColor(RgbaFloat color)
        {
            _modelColor = color.ToVector4();
        }

        internal void SetActiveModel(VxModel model)
        {
            _model = model;
            _cameraActive = false;
            _modelActive = true;
        }

        internal void Draw()
        {
            _drawSubmissions.Add(new DrawSubmission(
                _model,
                _modelPosition,
                _modelRotation,
                _modelScale,
                _modelColor));
        }

        private VxContext(GraphicsDevice gd, Sdl2Window window)
        {
            Device = gd;
            Window = window;
            Factory = new DisposeCollectorResourceFactory(gd.ResourceFactory);
            _cl = Factory.CreateCommandList();

            Window.Resized += () =>
            {
                Device.MainSwapchain.Resize((uint)Window.Width, (uint)Window.Height);
                _imguiRenderer.WindowResized(Window.Width, Window.Height);
            };

            ShaderSetDescription meshShaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3))
                },
                ShaderHelpers.LoadSet(Device, Factory, "Model"));

            ResourceLayout viewProjLayout = Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ViewProjection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("SceneInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment)));
            ResourceLayout worldLayout = Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("World", ResourceKind.UniformBuffer, ShaderStages.Vertex)));
            ResourceLayout modelParamsLayout = Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ModelParams", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

            GraphicsPipelineDescription meshPipelineDescription = new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyGreaterEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                meshShaderSet,
                new[] { viewProjLayout, worldLayout, modelParamsLayout },
                Device.MainSwapchain.Framebuffer.OutputDescription);

            _modelPipeline = Factory.CreateGraphicsPipeline(meshPipelineDescription);

            _viewProjectionBuffer = Factory.CreateBufferFor<Matrix4x4>(BufferUsage.Dynamic | BufferUsage.UniformBuffer);
            _sceneInfoBuffer = Factory.CreateBufferFor<SceneInfo>(BufferUsage.Dynamic | BufferUsage.UniformBuffer);
            _viewProjectionSet = Factory.CreateResourceSet(new ResourceSetDescription(viewProjLayout, _viewProjectionBuffer, _sceneInfoBuffer));

            _worldBuffer = Factory.CreateBuffer(new BufferDescription(128, BufferUsage.Dynamic | BufferUsage.UniformBuffer));
            _worldSet = Factory.CreateResourceSet(new ResourceSetDescription(worldLayout, _worldBuffer));

            _modelParamsBuffer = Factory.CreateBufferFor<Vector4>(BufferUsage.Dynamic | BufferUsage.UniformBuffer);
            _modelParamsSet = Factory.CreateResourceSet(new ResourceSetDescription(modelParamsLayout, _modelParamsBuffer));

            _imguiRenderer = new ImGuiRenderer(Device, Device.MainSwapchain.Framebuffer.OutputDescription, Window.Width, Window.Height);

            _sw = Stopwatch.StartNew();
        }

        internal void EndFrame()
        {
            double newFrameTime = _sw.Elapsed.TotalSeconds;
            FrameTime = (float)(newFrameTime - _lastFrameTime);
            _lastFrameTime = newFrameTime;
            FlushFrame();
            Reset();
        }

        private void Reset()
        {
            _modelRotation = Quaternion.Identity;
            _modelPosition = Vector3.Zero;
            _modelScale = Vector3.One;
            _modelColor = RgbaFloat.White.ToVector4();
        }

        private void FlushFrame()
        {
            InputSnapshot input = Window.PumpEvents();
            VxInput.UpdateFrameInput(input);

            if (!Window.Exists) { return; }
            _cl.Begin();
            _cl.SetFramebuffer(Device.MainSwapchain.Framebuffer);
            _cl.ClearDepthStencil(Device.IsDepthRangeZeroToOne ? 0f : 1f);
            _cl.ClearColorTarget(0, ClearColor);

            Vector3 cameraLookDir = Vector3.Transform(-Vector3.UnitZ, _cameraRotation);
            Matrix4x4 view = Matrix4x4.CreateLookAt(_cameraPosition, _cameraPosition + cameraLookDir, Vector3.UnitY);
            Matrix4x4 projection = MathUtil.CreatePerspective(
                Device, Device.IsDepthRangeZeroToOne,
                1f, (float)Window.Width / Window.Height,
                0.5f, 1000f);
            _cl.UpdateBuffer(_viewProjectionBuffer, 0, view * projection);

            SceneInfo sceneInfo = new SceneInfo
            {
                LightDir = Vector4.Normalize(new Vector4(0.2f, -0.6f, -1f, 0)),
                LightColor = new Vector4(1, 1, 1, 1)
            };
            _cl.UpdateBuffer(_sceneInfoBuffer, 0, sceneInfo);

            _cl.SetPipeline(_modelPipeline);
            _cl.SetGraphicsResourceSet(0, _viewProjectionSet);
            _cl.SetGraphicsResourceSet(1, _worldSet);
            _cl.SetGraphicsResourceSet(2, _modelParamsSet);

            foreach (DrawSubmission submission in _drawSubmissions)
            {
                SubmitDraw(submission);
            }
            _drawSubmissions.Clear();
            _imguiRenderer.Render(Device, _cl);
            _imguiRenderer.Update(Vx.FrameTime, input);

            _cl.End();
            Device.SubmitCommands(_cl);
            Device.SwapBuffers(Device.MainSwapchain);
            Device.WaitForIdle();
        }

        private void SubmitDraw(DrawSubmission submission)
        {
            WorldAndInverseTranspose matrices;
            matrices.World = Matrix4x4.CreateScale(submission.Scale)
                * Matrix4x4.CreateFromQuaternion(submission.Rotation)
                * Matrix4x4.CreateTranslation(submission.Position);
            bool inverted = Matrix4x4.Invert(matrices.World, out Matrix4x4 inverse);
            Debug.Assert(inverted);
            matrices.InverseTranspose = Matrix4x4.Transpose(inverse);
            _cl.UpdateBuffer(_worldBuffer, 0, ref matrices);
            _cl.UpdateBuffer(_modelParamsBuffer, 0, submission.Color);
            _cl.SetVertexBuffer(0, submission.Model.VertexBuffer);
            _cl.SetIndexBuffer(submission.Model.IndexBuffer, submission.Model.IndexFormat);

            foreach ((uint startIndex, int baseVertex, uint indexCount) in submission.Model.ModelRegions)
            {
                _cl.DrawIndexed(indexCount, 1, startIndex, baseVertex, 0);
            }
        }


        public static VxContext Instance
        {
            get
            {
                lock (s_initializationLock)
                {
                    if (s_instance == null)
                    {
                        throw new VxException("Vx has not been initialized. Call VxContext.Initialize before anything else.");
                    }
                    return s_instance;
                }
            }
        }

        public RgbaFloat ClearColor { get; internal set; } = new RgbaFloat(0f, 0, 0.2f, 1f);

        public static bool IsRunning
        {
            get
            {
                lock (s_initializationLock)
                {
                    if (s_instance == null)
                    {
                        return false;
                    }

                    return s_instance.Window.Exists;
                }
            }
        }

        public float FrameTime { get; private set; }

        public static void Initialize()
        {
            lock (s_initializationLock)
            {
                if (s_instance != null)
                {
                    throw new VxException("VxContext has already been initialized.");
                }

                WindowCreateInfo wci = new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, Assembly.GetEntryAssembly().GetName().Name);
                GraphicsDeviceOptions options = new GraphicsDeviceOptions(false, PixelFormat.R16_UNorm, true, ResourceBindingModel.Improved, true, true);
                VeldridStartup.CreateWindowAndGraphicsDevice(wci, options, out Sdl2Window window, out GraphicsDevice gd);
                s_instance = new VxContext(gd, window);
            }
        }

        public static void Terminate()
        {
            lock (s_initializationLock)
            {
                if (s_instance == null)
                {
                    throw new VxException("VxContext has not been initialized.");
                }

                ((DisposeCollectorResourceFactory)s_instance.Factory).DisposeCollector.DisposeAll();
                s_instance.Device.Dispose();
                s_instance.Window.Close();
            }
        }
    }

    struct WorldAndInverseTranspose
    {
        public Matrix4x4 World;
        public Matrix4x4 InverseTranspose;
    }
}
