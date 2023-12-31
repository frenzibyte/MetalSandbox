// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace VeldridSandbox.Rendering
{
    public interface IVertexBatch : IDisposable
    {
        /// <summary>
        /// The number of vertices in each VertexBuffer.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Draw any newly added indices in this vertex batch.
        /// </summary>
        /// <returns>The number of indices drawn, if any.</returns>
        int Draw();

        internal void ResetCounters();
    }

    public interface IVertexBatch<in TVertex> : IVertexBatch
        where TVertex : unmanaged, IEquatable<TVertex>
    {
        Action<TVertex> AddAction { get; }

        void Add(TVertex vertex);
    }
}