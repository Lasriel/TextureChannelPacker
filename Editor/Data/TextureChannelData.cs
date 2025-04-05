using System;
using UnityEngine;

namespace Lasriel.TextureChannelPacker.Editor {

    /// <summary>
    /// Texture data for a single output channel.
    /// </summary>
    [Serializable]
    public struct TextureChannelData {

        /// <summary>
        /// Input texture.
        /// </summary>
        public Texture2D Texture;

        /// <summary>
        /// Input color channel.
        /// </summary>
        public ColorChannels Channel;

        /// <summary>
        /// Invert the input color channel?
        /// </summary>
        public bool Invert;

        /// <summary>
        /// Input texture width.
        /// </summary>
        public int Width => Texture == null ? 0 : Texture.width;

        /// <summary>
        /// Input texture height.
        /// </summary>
        public int Height => Texture == null ? 0 : Texture.height;

    }

}
