using System;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using VeldridSandbox.Rendering;
using VeldridSandbox.Veldrid.Batches;
using VeldridSandbox.Veldrid.Buffers;

namespace VeldridSandbox
{
    public class VeldridRenderer
    {
        private const int aligned_vertices_size = 8;
        private const int vertices_number = instances * 6;
        private const int instances = 10000;

        private static readonly Vector2 Size = new(0.004f, 0.002f);

        public GraphicsDevice Device => device;
        public ResourceFactory Factory => device.ResourceFactory;
        public CommandList Commands => commands;

        public VeldridIndexData SharedQuadIndex { get; set; }
        private VeldridQuadBatch<Vector2> batch;
        private VeldridQuadBuffer<Vector2> buffer;

        private GraphicsDevice device = null!;
        private CommandList commands = null!;
        private DeviceBuffer vertexBuffer = null!;
        private Pipeline pipeline = null!;
        private unsafe Vector2* vertices;

        public unsafe void Initialise(int width, int height, SwapchainSource source)
        {
            SharedQuadIndex = new VeldridIndexData(this);

            var swapchain = new SwapchainDescription(source, (uint)width, (uint)height, PixelFormat.D32_Float_S8_UInt,
                true, false);
            var options = new GraphicsDeviceOptions(false, PixelFormat.D32_Float_S8_UInt, true);
            device = GraphicsDevice.CreateMetal(options, swapchain);

            commands = device.ResourceFactory.CreateCommandList();

            vertexBuffer = device.ResourceFactory.CreateBuffer(
                new BufferDescription(vertices_number * aligned_vertices_size,
                    BufferUsage.Dynamic | BufferUsage.VertexBuffer));

            var resource = device.Map(vertexBuffer, MapMode.Write);
            vertices = (Vector2*)resource.Data;

            var vertexLayout = new VertexLayoutDescription(new VertexElementDescription("position",
                VertexElementFormat.Float2, VertexElementSemantic.Position));

            var shaders = device.ResourceFactory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertex_texture), "main0"),
                new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragment_texture), "main0"));

            pipeline = device.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual with
                {
                    DepthComparison = ComparisonKind.Less
                },
                Outputs = device.SwapchainFramebuffer.OutputDescription,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                RasterizerState = RasterizerStateDescription.CullNone,
                ResourceBindingModel = ResourceBindingModel.Improved,
                ResourceLayouts = Array.Empty<ResourceLayout>(),
                ShaderSet = new ShaderSetDescription(new[] { vertexLayout }, shaders),
            });

            batch = new VeldridQuadBatch<Vector2>(this, instances);
            buffer = new VeldridQuadBuffer<Vector2>(this, instances);
        }

        public unsafe void Render()
        {
            device.WaitForNextFrameReady();

            commands.Begin();
            commands.SetFramebuffer(device.SwapchainFramebuffer);
            commands.ClearColorTarget(0, RgbaFloat.Black);
            commands.ClearDepthStencil(1f);

            commands.SetPipeline(pipeline);
            commands.SetVertexBuffer(0, vertexBuffer);
            commands.SetIndexBuffer(SharedQuadIndex.Buffer, IndexFormat.UInt16);

            // upload vertices.
            var row = 100;
            var spacing = new Vector2(Size.X + 0.005f, Size.Y + 0.002f);

            int index;

            ((IVertexBatch)batch).ResetCounters();

            for (int i = 0; i < instances; i++)
            {
                index = i * 4;
                float x = (float)(0.05 + (i % row) * spacing.X) * 2 - 1;
                // ReSharper disable once PossibleLossOfFraction
                float y = (float)-((0.1 + (i / row) * spacing.Y) * 2 - 1);
                var bottomLeft = new Vector2(x, y + Size.Y);
                var bottomRight = new Vector2(x + Size.X, y + Size.Y);
                var topRight = new Vector2(x + Size.X, y);
                var topLeft = new Vector2(x, y);
                // vertices[index + 0] = bottomLeft;
                // vertices[index + 1] = bottomRight;
                // vertices[index + 2] = topRight;
                // vertices[index + 3] = topLeft;
                // vertices[index + 0] = bottomLeft;
                // vertices[index + 1] = bottomRight;
                // vertices[index + 2] = topLeft;
                // vertices[index + 3] = topRight;
                // vertices[index + 4] = topLeft;
                // vertices[index + 5] = bottomRight;
                batch.Add(bottomLeft);
                batch.Add(bottomRight);
                batch.Add(topRight);
                batch.Add(topLeft);
                // buffer.SetVertex(index + 0, bottomLeft);
                // buffer.SetVertex(index + 1, bottomRight);
                // buffer.SetVertex(index + 2, topRight);
                // buffer.SetVertex(index + 3, topLeft);
            }

            // commands.DrawIndexed(instances * 6, 1, 0, 0, 0);
            batch.Draw();
            // buffer.DrawRange(0, instances * 4);
            // for (int i = 0; i < 1; i++)
            //     commands.Draw((instances * 6) / 1, 1, (uint)(((instances * 6) / 1) * i), 0);

            commands.End();
            device.SubmitCommands(commands);
            device.SwapBuffers();
        }

        private const string vertex_texture = @"
#version 330

layout(location = 0) in vec2 pos;

void main(void)
{
    gl_Position = vec4(pos, 0.0, 1.0);
}
";

        private const string fragment_texture = @"
#version 330

layout(location = 0) out vec4 colour;

void main(void)
{
    colour = vec4(1.0, 1.0, 1.0, 1.0);
}
";
    }
}