using System;

namespace Lasriel.TextureChannelPacker.Editor {

    /// <summary>
    /// Settings data defined in the packer GUI.
    /// </summary>
    [Serializable]
    public struct PackerSettings {

        /// <summary>
        /// Texture output format.
        /// </summary>
        public PackerFormats OutputFormat;

        /// <summary>
        /// Texture output resolution.
        /// </summary>
        public PackerResolutions OutputResolution;

    }

}
