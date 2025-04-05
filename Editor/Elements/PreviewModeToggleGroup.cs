using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Lasriel.TextureChannelPacker.Editor {

    /// <summary>
    /// Hardcoded preview toggle group for changing texture preview mode.
    /// </summary>
    public class PreviewModeToggleGroup {

        /// <summary>
        /// Preview toggle, relationship between toggle and mode enum.
        /// </summary>
        private struct PreviewModeToggle {
            public ToolbarToggle Toggle;
            public PreviewMode Mode;

            public PreviewModeToggle(ToolbarToggle toggle, PreviewMode mode) {
                Toggle = toggle;
                Mode = mode;
            }
        }

        /// <summary>
        /// Currently selected preview mode.
        /// </summary>
        public PreviewMode CurrentPreviewMode => m_CurrentToggle.Mode;

        private List<PreviewModeToggle> m_Toggles;
        private PreviewModeToggle m_CurrentToggle;
        private Action<PreviewMode> m_OnChangeAction;

        /// <summary>
        /// Creates a new toggle group.
        /// </summary>
        /// <param name="toolbar"> Toolbar element to get toggles from. </param>
        /// <param name="initialMode"> Initial preview mode. </param>
        /// <param name="onToggleChanged"> Event to invoke when toggle selection changes. </param>
        public PreviewModeToggleGroup(Toolbar toolbar, PreviewMode initialMode, Action<PreviewMode> onToggleChanged) {
            m_Toggles = new List<PreviewModeToggle>();
            m_OnChangeAction = onToggleChanged;

            ToolbarToggle toggleRGB = (ToolbarToggle)toolbar.Q(name: "PreviewModeRGB");
            ToolbarToggle toggleR = (ToolbarToggle)toolbar.Q(name: "PreviewModeR");
            ToolbarToggle toggleG = (ToolbarToggle)toolbar.Q(name: "PreviewModeG");
            ToolbarToggle toggleB = (ToolbarToggle)toolbar.Q(name: "PreviewModeB");
            ToolbarToggle toggleA = (ToolbarToggle)toolbar.Q(name: "PreviewModeA");

            PreviewModeToggle rgb = new PreviewModeToggle(toggleRGB, PreviewMode.RGB);
            PreviewModeToggle r = new PreviewModeToggle(toggleR, PreviewMode.R);
            PreviewModeToggle g = new PreviewModeToggle(toggleG, PreviewMode.G);
            PreviewModeToggle b = new PreviewModeToggle(toggleB, PreviewMode.B);
            PreviewModeToggle a = new PreviewModeToggle(toggleA, PreviewMode.A);

            AddToggle(rgb);
            AddToggle(r);
            AddToggle(g);
            AddToggle(b);
            AddToggle(a);

            foreach (PreviewModeToggle toggle in m_Toggles) {
                if (toggle.Mode != initialMode) continue;
                toggle.Toggle.SetValueWithoutNotify(true);
                HandleToggleChange(toggle);
            }
        }

        /// <summary>
        /// Adds toggle to the toggle group and registers value change event for it.
        /// </summary>
        /// <param name="toggle"></param>
        private void AddToggle(PreviewModeToggle toggle) {
            m_Toggles.Add(toggle);
            toggle.Toggle.RegisterValueChangedCallback((evt) => {
                ToolbarToggle target = (ToolbarToggle)evt.target;
                if (evt.newValue == false) {
                    target.SetValueWithoutNotify(true);
                    return;
                }
                HandleToggleChange(toggle);
            });
        }

        /// <summary>
        /// Handles toggle state changes and invoking events when value changes.
        /// </summary>
        /// <param name="toggled"></param>
        private void HandleToggleChange(PreviewModeToggle toggled) {
            m_CurrentToggle = toggled;

            foreach (PreviewModeToggle toggle in m_Toggles) {
                if (toggle.Mode == toggled.Mode) continue;
                toggle.Toggle.SetValueWithoutNotify(false);
            }
            m_OnChangeAction.Invoke(toggled.Mode);
        }

    }

}
