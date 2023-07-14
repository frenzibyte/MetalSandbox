// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace VeldridSandbox.Rendering
{
    internal interface IVertexBuffer
    {
        ulong LastUseFrameIndex { get; }

        /// <summary>
        /// Whether this <see cref="IVertexBuffer"/> is currently in use.
        /// </summary>
        bool InUse { get; }

        /// <summary>
        /// Frees all resources allocated by this <see cref="IVertexBuffer"/>.
        /// </summary>
        void Free();
    }
}