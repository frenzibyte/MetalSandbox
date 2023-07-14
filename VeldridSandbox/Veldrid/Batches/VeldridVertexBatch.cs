// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using VeldridSandbox.Rendering;
using VeldridSandbox.Veldrid.Buffers;

namespace VeldridSandbox.Veldrid.Batches
{
    internal abstract class VeldridVertexBatch<T> : IVertexBatch<T>
        where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        /// Most documentation recommends that three buffers are used to avoid contention.
        ///
        /// Note that due to the higher level triple buffer, the actual number of buffers we are storing is three times as high as this constant.
        /// Maintaining this many buffers is a cause of concern from an memory alloc / GPU upload perspective.
        /// </summary>
        private const int vertex_buffer_count = 2;

        /// <summary>
        /// Multiple VBOs in a swap chain to try our best to avoid GPU contention.
        /// </summary>
        private readonly List<VeldridVertexBuffer<T>>[] vertexBuffers = new List<VeldridVertexBuffer<T>>[vertex_buffer_count];

        private List<VeldridVertexBuffer<T>> currentVertexBuffers => vertexBuffers[3 % (ulong)vertexBuffers.Length];

        /// <summary>
        /// The number of vertices in each VertexBuffer.
        /// </summary>
        public int Size { get; }

        private int changeBeginIndex = -1;
        private int changeEndIndex = -1;

        private int currentBufferIndex;
        private int currentVertexIndex;
        private int currentDrawIndex;

        private readonly VeldridRenderer renderer;

        protected VeldridVertexBatch(VeldridRenderer renderer, int bufferSize)
        {
            Size = bufferSize;
            this.renderer = renderer;

            AddAction = Add;

            for (int i = 0; i < vertexBuffers.Length; i++)
                vertexBuffers[i] = new List<VeldridVertexBuffer<T>>();
        }

        #region Disposal

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = 0; i < vertexBuffers.Length; i++)
                {
                    foreach (VeldridVertexBuffer<T> vbo in vertexBuffers[i])
                        vbo.Dispose();
                }
            }
        }

        #endregion

        void IVertexBatch.ResetCounters()
        {
            changeBeginIndex = -1;
            currentBufferIndex = 0;
            currentVertexIndex = 0;
            currentDrawIndex = 0;
        }

        protected abstract VeldridVertexBuffer<T> CreateVertexBuffer(VeldridRenderer renderer);

        /// <summary>
        /// Adds a vertex to this <see cref="VeldridVertexBatch{T}"/>.
        /// </summary>
        /// <param name="v">The vertex to add.</param>
        public void Add(T v)
        {
            var buffers = currentVertexBuffers;

            if (buffers.Count > 0 && currentVertexIndex >= buffers[currentBufferIndex].Size)
            {
                Draw();

                currentBufferIndex++;
                currentVertexIndex = 0;
                currentDrawIndex = 0;
            }

            // currentIndex will change after Draw() above, so this cannot be in an else-condition
            if (currentBufferIndex >= buffers.Count)
                buffers.Add(CreateVertexBuffer(renderer));

            if (buffers[currentBufferIndex].SetVertex(currentVertexIndex, v))
            {
                if (changeBeginIndex == -1)
                    changeBeginIndex = currentVertexIndex;

                changeEndIndex = currentVertexIndex + 1;
            }

            ++currentVertexIndex;
        }

        /// <summary>
        /// Adds a vertex to this <see cref="VeldridVertexBatch{T}"/>.
        /// This is a cached delegate of <see cref="Add"/> that should be used in memory-critical locations such as <see cref="DrawNode"/>s.
        /// </summary>
        public Action<T> AddAction { get; private set; }

        public int Draw()
        {
            if (currentVertexIndex == currentDrawIndex)
                return 0;

            var buffers = currentVertexBuffers;

            if (buffers.Count == 0)
                return 0;

            VeldridVertexBuffer<T> buffer = buffers[currentBufferIndex];

            if (changeBeginIndex >= 0)
                buffer.UpdateRange(changeBeginIndex, changeEndIndex);

            buffer.DrawRange(currentDrawIndex, currentVertexIndex);

            int count = currentVertexIndex - currentDrawIndex;

            // When using multiple buffers we advance to the next one with every draw to prevent contention on the same buffer with future vertex updates.
            currentDrawIndex = currentVertexIndex;
            changeBeginIndex = -1;

            return count;
        }
    }
}