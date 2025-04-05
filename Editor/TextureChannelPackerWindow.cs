using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace Lasriel.TextureChannelPacker.Editor {

    /// <summary>
    /// Editor window for texture packer.
    /// </summary>
    public class TextureChannelPackerWindow : EditorWindow, IDisposable {

        private const string k_WindowUxmlPath = "PackerWindow";
        private const string k_WindowIcon = "d_PreTextureRGB";
        private const string k_WindowName = "Texture Channel Packer";

        private const string k_SaveWindowTitle = "Select packed texture save path.";
        private const string k_DefaultTextureSaveName = "PackedTexture";
        private const string k_SaveWindowMessage = "Please enter a file name to save the texture to.";
        private const string k_DefaultSavePath = "Assets";

        private const int k_PreviewSize = 256;

        private static VisualTreeAsset m_WindowUxml;
        private static VisualTreeAsset WindowUxml {
            get {
                if (m_WindowUxml == null) {
                    m_WindowUxml = Resources.Load<VisualTreeAsset>(k_WindowUxmlPath);
                }
                return m_WindowUxml;
            }
            set => m_WindowUxml = value;
        }

        [SerializeField] private string m_LastSavePath = "";

        [SerializeField] private TextureChannelData m_Red;
        [SerializeField] private TextureChannelData m_Green;
        [SerializeField] private TextureChannelData m_Blue;
        [SerializeField] private TextureChannelData m_Alpha;

        [SerializeField]
        private PackerSettings m_Settings = new PackerSettings() {
            OutputFormat = PackerFormats.PNG,
            OutputResolution = PackerResolutions.m_1024x1024
        };

        [SerializeField] private bool m_ShowTransparency = false;
        [SerializeField] private PreviewMode m_PreviewMode = PreviewMode.RGB;

        private RenderTexture m_PreviewRenderTexture;
        private RenderTexture PreviewRenderTexture {
            get {
                if (m_PreviewRenderTexture == null) {
                    m_PreviewRenderTexture = new RenderTexture(k_PreviewSize, k_PreviewSize, 0) {
                        graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm,
                        filterMode = FilterMode.Bilinear,
                        wrapMode = TextureWrapMode.Clamp
                    };
                }
                return m_PreviewRenderTexture;
            }
            set => m_PreviewRenderTexture = value;
        }

        private Texture2D m_PreviewTexture;
        private Texture2D PreviewTexture {
            get {
                if (m_PreviewTexture == null) {
                    m_PreviewTexture = new Texture2D(
                        width: k_PreviewSize,
                        height: k_PreviewSize,
                        textureFormat: TextureFormat.RGBA32,
                        mipChain: false
                    );
                }
                return m_PreviewTexture;
            }
            set => m_PreviewTexture = value;
        }

        private PreviewModeToggleGroup m_ToggleGroup;
        private IMGUIContainer m_PreviewGui;
        private VisualElement m_PreviewTextureContainer;

        [MenuItem("Window/Lasriel/Texture Channel Packer")]
        public static void Open() {
            // Setup editor window
            EditorWindow window = GetWindow(typeof(TextureChannelPackerWindow));
            GUIContent content = EditorGUIUtility.IconContent(k_WindowIcon);
            content.text = k_WindowName;
            content.tooltip = "";
            window.titleContent = content;
        }

        public void CreateGUI() {
            Initialize();
        }

        private void OnDestroy() {
            Dispose();
        }

        /// <summary>
        /// Creates the GUI.
        /// </summary>
        private void Initialize() {
            VisualElement root = WindowUxml.Instantiate();
            rootVisualElement.Add(root);

            // Find elements in hierarchy
            ObjectField textureFieldR = (ObjectField)root.Q(name: "TextureR");
            ObjectField textureFieldG = (ObjectField)root.Q(name: "TextureG");
            ObjectField textureFieldB = (ObjectField)root.Q(name: "TextureB");
            ObjectField textureFieldA = (ObjectField)root.Q(name: "TextureA");
            EnumField channelEnumR = (EnumField)root.Q(name: "ChannelR");
            EnumField channelEnumG = (EnumField)root.Q(name: "ChannelG");
            EnumField channelEnumB = (EnumField)root.Q(name: "ChannelB");
            EnumField channelEnumA = (EnumField)root.Q(name: "ChannelA");
            Toggle invertToggleR = (Toggle)root.Q(name: "InvertR");
            Toggle invertToggleG = (Toggle)root.Q(name: "InvertG");
            Toggle invertToggleB = (Toggle)root.Q(name: "InvertB");
            Toggle invertToggleA = (Toggle)root.Q(name: "InvertA");
            EnumField formatEnumField = (EnumField)root.Q(name: "SettingFormat");
            EnumField resolutionEnumField = (EnumField)root.Q(name: "SettingResolution");
            Button packButton = (Button)root.Q(name: "GenerateButton");
            m_PreviewTextureContainer = root.Q(name: "PreviewTextureContainer");
            Toolbar previewToolbar = (Toolbar)root.Q(name: "PreviewModeToolbar");
            Toggle previewTransparencyToggle = (Toggle)root.Q(name: "TransparencyToggle");


            // Set serialized values
            textureFieldR.value = m_Red.Texture;
            textureFieldG.value = m_Green.Texture;
            textureFieldB.value = m_Blue.Texture;
            textureFieldA.value = m_Alpha.Texture;
            channelEnumR.value = m_Red.Channel;
            channelEnumG.value = m_Green.Channel;
            channelEnumB.value = m_Blue.Channel;
            channelEnumA.value = m_Alpha.Channel;
            invertToggleR.value = m_Red.Invert;
            invertToggleG.value = m_Green.Invert;
            invertToggleB.value = m_Blue.Invert;
            invertToggleA.value = m_Alpha.Invert;
            formatEnumField.value = m_Settings.OutputFormat;
            resolutionEnumField.value = m_Settings.OutputResolution;
            previewTransparencyToggle.value = m_ShowTransparency;

            // Listen for changes in fields
            textureFieldR.RegisterValueChangedCallback((evt) => {
                m_Red.Texture = (Texture2D)evt.newValue;
                UpdatePreview();
            });
            textureFieldG.RegisterValueChangedCallback((evt) => {
                m_Green.Texture = (Texture2D)evt.newValue;
                UpdatePreview();
            });
            textureFieldB.RegisterValueChangedCallback((evt) => {
                m_Blue.Texture = (Texture2D)evt.newValue;
                UpdatePreview();
            });
            textureFieldA.RegisterValueChangedCallback((evt) => {
                m_Alpha.Texture = (Texture2D)evt.newValue;
                UpdatePreview();
            });

            channelEnumR.RegisterValueChangedCallback((evt) => {
                m_Red.Channel = (ColorChannels)evt.newValue;
                UpdatePreview();
            });
            channelEnumG.RegisterValueChangedCallback((evt) => {
                m_Green.Channel = (ColorChannels)evt.newValue;
                UpdatePreview();
            });
            channelEnumB.RegisterValueChangedCallback((evt) => {
                m_Blue.Channel = (ColorChannels)evt.newValue;
                UpdatePreview();
            });
            channelEnumA.RegisterValueChangedCallback((evt) => {
                m_Alpha.Channel = (ColorChannels)evt.newValue;
                UpdatePreview();
            });

            invertToggleR.RegisterValueChangedCallback((evt) => {
                m_Red.Invert = evt.newValue;
                UpdatePreview();
            });
            invertToggleG.RegisterValueChangedCallback((evt) => {
                m_Green.Invert = evt.newValue;
                UpdatePreview();
            });
            invertToggleB.RegisterValueChangedCallback((evt) => {
                m_Blue.Invert = evt.newValue;
                UpdatePreview();
            });
            invertToggleA.RegisterValueChangedCallback((evt) => {
                m_Alpha.Invert = evt.newValue;
                UpdatePreview();
            });

            formatEnumField.RegisterValueChangedCallback((evt) => m_Settings.OutputFormat = (PackerFormats)evt.newValue);
            resolutionEnumField.RegisterValueChangedCallback((evt) => m_Settings.OutputResolution = (PackerResolutions)evt.newValue);

            packButton.clicked += OnPackClicked;

            m_ToggleGroup = new PreviewModeToggleGroup(previewToolbar, m_PreviewMode, (mode) => {
                m_PreviewMode = mode;
            });

            previewTransparencyToggle.RegisterValueChangedCallback((evt) => {
                m_ShowTransparency = evt.newValue;
                UpdatePreview();
            });

            if (m_PreviewGui != null) {
                m_PreviewGui.Dispose();
            }
            m_PreviewGui = new IMGUIContainer(UpdatePreviewGui);
            m_PreviewGui.style.height = k_PreviewSize;
            m_PreviewTextureContainer.Add(m_PreviewGui);

            UpdatePreview();
        }

        /// <summary>
        /// Packs channel data onto a texture and prompts user to save it.
        /// </summary>
        private void OnPackClicked() {
            try {
                string path;
                if (string.IsNullOrEmpty(m_LastSavePath)) {
                    path = k_DefaultSavePath;
                } else {
                    // If last save path exists use it instead
                    path = AssetDatabase.IsValidFolder(m_LastSavePath) ? m_LastSavePath : k_DefaultSavePath;
                }

                // Open file save window, get path or cancel
                string filePath = EditorUtility.SaveFilePanelInProject(
                    k_SaveWindowTitle,
                    k_DefaultTextureSaveName,
                    m_Settings.OutputFormat.ToString().ToLower(),
                    k_SaveWindowMessage,
                    path
                );

                // Cancelled
                if (string.IsNullOrEmpty(filePath)) return;
                m_LastSavePath = Path.GetDirectoryName(filePath);

                EditorUtility.DisplayProgressBar("Saving", "Packing texture...", 0.0f);

                // Pack input textures
                Texture2D output = TexturePacker.Pack(m_Settings, m_Red, m_Green, m_Blue, m_Alpha);

                EditorUtility.DisplayProgressBar("Saving", "Saving texture...", 0.5f);

                // Encode raw texture data
                byte[] buffer;
                switch (m_Settings.OutputFormat) {
                    case PackerFormats.PNG:
                        buffer = output.EncodeToPNG();
                        break;
                    case PackerFormats.TGA:
                        buffer = output.EncodeToTGA();
                        break;
                    default:
                        throw new InvalidDataException("[TextureChannelPackerWindow]: Invalid texture format.");
                }

                // Write encoded texture to disk
                File.WriteAllBytes(filePath, buffer);

                // Cleanup
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            } catch (Exception error) {
                EditorUtility.ClearProgressBar();
                Debug.LogError("[TextureChannelPackerWindow]: " + error);
                return;
            }

        }

        /// <summary>
        /// Creates a new preview texture from current channel data.
        /// </summary>
        private void UpdatePreview() {
            Texture2D texture = PreviewTexture;
            texture.alphaIsTransparency = m_ShowTransparency;
            TexturePacker.GetPreview(PreviewRenderTexture, texture, m_Red, m_Green, m_Blue, m_Alpha);
        }

        /// <summary>
        /// IMGUI code for drawing the texture preview.
        /// </summary>
        private void UpdatePreviewGui() {
            if (m_PreviewTexture == null) {
                UpdatePreview();
            }

            Rect textureRect = m_PreviewTextureContainer.contentRect;

            Vector2 ratio;
            ratio.x = textureRect.width / k_PreviewSize;
            ratio.y = textureRect.height / k_PreviewSize;
            float minRatio = Mathf.Min(ratio.x, ratio.y);

            Vector2 center = textureRect.center;
            textureRect.width = k_PreviewSize * minRatio;
            textureRect.height = k_PreviewSize * minRatio;
            textureRect.center = center;

            ColorWriteMask colorWriteMask = ColorWriteMask.All;
            switch (m_PreviewMode) {
                case PreviewMode.R:
                    colorWriteMask = ColorWriteMask.Red | ColorWriteMask.Alpha;
                    break;
                case PreviewMode.G:
                    colorWriteMask = ColorWriteMask.Green | ColorWriteMask.Alpha;
                    break;
                case PreviewMode.B:
                    colorWriteMask = ColorWriteMask.Blue | ColorWriteMask.Alpha;
                    break;
            }

            if (m_PreviewMode == PreviewMode.A) {
                EditorGUI.DrawTextureAlpha(textureRect, m_PreviewTexture, ScaleMode.StretchToFill, 0, 0);
            } else {
                if (m_PreviewTexture.alphaIsTransparency) {
                    EditorGUI.DrawTextureTransparent(textureRect, m_PreviewTexture, ScaleMode.StretchToFill, 0, 0, colorWriteMask);
                } else {
                    EditorGUI.DrawPreviewTexture(textureRect, m_PreviewTexture, null, ScaleMode.StretchToFill, 0, 0, colorWriteMask);
                }
            }

        }

        public void Dispose() {
            if (m_PreviewRenderTexture != null) {
                m_PreviewRenderTexture.Release();
            }
            if (m_PreviewGui != null) {
                m_PreviewGui.Dispose();
            }
        }
    }

}