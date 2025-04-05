using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Lasriel.TextureChannelPacker.Editor {

    /// <summary>
    /// Packs color channel data into textures.
    /// </summary>
    public static class TexturePacker {

        private const string k_BlitShaderName = "Hidden/BlitCopy";
        private const int k_SmallestTextureSize = 8;

        /// <summary>
        /// Packs color channels from multiple textures into one.
        /// </summary>
        /// <param name="settings"> Packing settings. </param>
        /// <param name="r"> Red channel texture data. </param>
        /// <param name="g"> Green channel texture data. </param>
        /// <param name="b"> Blue channel texture data. </param>
        /// <param name="a"> Alpha channel texture data. </param>
        /// <returns> Packed texture data. </returns>
        /// <exception cref="NullReferenceException"> When blit shader could not be found. </exception>
        public static Texture2D Pack(
            PackerSettings settings,
            TextureChannelData r = default,
            TextureChannelData g = default,
            TextureChannelData b = default,
            TextureChannelData a = default
            ) {

            bool autoSize = settings.OutputResolution == PackerResolutions.Auto;
            int size = autoSize ? CalculateAutoSize(r, g, b, a) : (int)settings.OutputResolution;

            // Create a temp texture where color data will be copied to
            Texture2D output = new Texture2D(
                width: size,
                height: size,
                textureFormat: TextureFormat.RGBA32,
                mipChain: false
            );

            Material material = CreateBlitMaterial(r, g, b, a);

            // Blit input textures to black render texture using a custom shader
            RenderTexture renderTexture = RenderTexture.GetTemporary(size, size);

            Graphics.Blit(Texture2D.blackTexture, renderTexture, material);

            // Copy render texture color data to the output
            RenderTexture.active = renderTexture;
            output.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

            // Cleanup resources
            RenderTexture.ReleaseTemporary(renderTexture);
            RenderTexture.active = null;
            Object.DestroyImmediate(material);

            return output;
        }

        /// <summary>
        /// Creates a preview texture from channel data.
        /// </summary>
        /// <param name="renderTexture"> Render texture to use for the data copying. </param>
        /// <param name="target"> Target texture where color data will get copied to. </param>
        /// <param name="r"> Red channel texture data. </param>
        /// <param name="g"> Green channel texture data. </param>
        /// <param name="b"> Blue channel texture data. </param>
        /// <param name="a"> Alpha channel texture data. </param>
        public static void GetPreview(
            RenderTexture renderTexture,
            Texture2D target,
            TextureChannelData r = default,
            TextureChannelData g = default,
            TextureChannelData b = default,
            TextureChannelData a = default
            ) {

            Material material = CreateBlitMaterial(r, g, b, a);

            // Perform blit to render texture
            Graphics.Blit(Texture2D.blackTexture, renderTexture, material);

            // Copy pixel data to the target texture
            RenderTexture.active = renderTexture;
            target.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            target.Apply();
            RenderTexture.active = null;

            // Cleanup
            Object.DestroyImmediate(material);
        }

        /// <summary>
        /// Creates a material for blitting and sets it's properties.
        /// </summary>
        /// <param name="r"> Red channel texture data. </param>
        /// <param name="g"> Green channel texture data. </param>
        /// <param name="b"> Blue channel texture data. </param>
        /// <param name="a"> Alpha channel texture data. </param>
        /// <returns> Created blit material. </returns>
        /// <exception cref="NullReferenceException"> When blit shader could not be found. </exception>
        private static Material CreateBlitMaterial(
            TextureChannelData r,
            TextureChannelData g,
            TextureChannelData b,
            TextureChannelData a
            ) {

            // Create material from custom blit shader
            Shader shader = Shader.Find(k_BlitShaderName);
            if (shader == null) {
                throw new NullReferenceException("[TexturePacker]: Blit copy shader was not found!");
            }
            Material material = new Material(shader) {
                hideFlags = HideFlags.HideAndDontSave
            };

            // Input textures
            material.SetTexture("_ToRedTexture", r.Texture);
            material.SetTexture("_ToGreenTexture", g.Texture);
            material.SetTexture("_ToBlueTexture", b.Texture);
            material.SetTexture("_ToAlphaTexture", a.Texture);

            // Which input texture color channel to copy in the shader
            // 0 = Red
            // 1 = Green
            // 2 = Blue
            // 3 = Alpha
            Vector4 channels = Vector4.zero;
            channels[0] = (int)r.Channel;
            channels[1] = (int)g.Channel;
            channels[2] = (int)b.Channel;
            channels[3] = (int)a.Channel;

            // Does the color channel get inverted in the shader
            // 0f = no invert
            // 1f = invert
            Vector4 inverts = Vector4.zero;
            inverts[0] = r.Invert ? 1f : 0f;
            inverts[1] = g.Invert ? 1f : 0f;
            inverts[2] = b.Invert ? 1f : 0f;
            inverts[3] = a.Invert ? 1f : 0f;

            // Set shader properties
            material.SetVector("_InputChannels", channels);
            material.SetVector("_Inverts", inverts);

            return material;
        }

        /// <summary>
        /// Calculates the largest texture size from input and clamps it to closest power of two.
        /// </summary>
        /// <param name="inputs"> Input texture data. </param>
        /// <returns> Largest size, if inputs are too small returns default value. </returns>
        private static int CalculateAutoSize(params TextureChannelData[] inputs) {
            // Find largest size among the textures
            int largestSize = 0;
            foreach (TextureChannelData input in inputs) {
                Texture2D texture = input.Texture;
                int width = texture.width;
                int height = texture.height;
                int size = Math.Max(width, height);
                if (size > largestSize) {
                    largestSize = size;
                }
            }

            if (largestSize < k_SmallestTextureSize) {
                largestSize = k_SmallestTextureSize;
            }

            // Make sure the texture size is power of two
            return Mathf.ClosestPowerOfTwo(largestSize);
        }

    }

}
