// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using Veldrid;
using PrimitiveTopology = Veldrid.PrimitiveTopology;

namespace VeldridSandbox.Veldrid.Buffers
{
    internal class VeldridQuadBuffer<T> : VeldridVertexBuffer<T>
        where T : unmanaged, IEquatable<T>
    {
        private readonly VeldridRenderer renderer;
        private readonly int amountIndices;

        /// <summary>
        /// The maximum number of quads supported by this buffer.
        /// </summary>
        public const int MAX_QUADS = ushort.MaxValue / 6;

        internal VeldridQuadBuffer(VeldridRenderer renderer, int amountQuads)
            : base(renderer, amountQuads * 4)
        {
            this.renderer = renderer;
            amountIndices = amountQuads * 6;
            Debug.Assert(amountIndices <= ushort.MaxValue);
        }

        public override void Initialise()
        {
            base.Initialise();

            if (amountIndices > renderer.SharedQuadIndex.Capacity)
            {
                renderer.SharedQuadIndex.Capacity = amountIndices;

                ushort[] indices = new ushort[amountIndices];

                for (ushort i = 0, j = 0; j < amountIndices; i += 4, j += 6)
                {
                    indices[j] = i;
                    indices[j + 1] = (ushort)(i + 1);
                    indices[j + 2] = (ushort)(i + 3);
                    indices[j + 3] = (ushort)(i + 2);
                    indices[j + 4] = (ushort)(i + 3);
                    indices[j + 5] = (ushort)(i + 1);
                }

                // These pathways are faster on respective platforms.
                // Test using TestSceneVertexUploadPerformance.
                renderer.Device.UpdateBuffer(renderer.SharedQuadIndex.Buffer, 0, indices);
            }
        }

        public override void Bind()
        {
            base.Bind();
            renderer.Commands.SetIndexBuffer(renderer.SharedQuadIndex.Buffer, IndexFormat.UInt16);
        }

        protected override int ToElements(int vertices) => 3 * vertices / 2;

        protected override int ToElementIndex(int vertexIndex) => 3 * vertexIndex / 2;

        protected override PrimitiveTopology Type => PrimitiveTopology.TriangleList;
    }
}