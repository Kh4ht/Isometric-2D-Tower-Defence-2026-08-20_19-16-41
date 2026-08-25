using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Elasticated.SceneAutoSave
{
    /// <summary>
    /// Injects a small AutoSave icon into the editor status bar, just to the LEFT of Unity's native
    /// right-side icons — giving [save][mute][cache][checks] flush against the right edge.
    ///
    /// Unity paints those native icons with IMGUI inside a single full-width IMGUIContainer; they are
    /// not VisualElements, so there is no flex container to insert into. The icon is therefore
    /// absolute-positioned at a fixed offset from the right edge, with an opaque background so a long
    /// status message can't bleed through behind it. Clicking the icon opens the AutoSave settings.
    /// </summary>
    [InitializeOnLoad]
    public static class AutoSaveStatusBarIcon
    {
        private const float IconSize = 16f;

        /// <summary>
        /// Approximate width of Unity's native right-side icon cluster. The AutoSave icon's right edge
        /// is placed this far from the bar's right edge, so it sits flush-left of those icons. Nudge
        /// this if the icon overlaps the cluster or leaves a gap.
        /// </summary>
        private const float ClusterWidth = 82f;

        /// <summary>Kept in sync with AutoSaveNotificationWindow.StatusBarRightOffset.</summary>
        public const float RightOffset = ClusterWidth;

        private static Image _icon;
        private static VisualElement _statusBarRoot;
        private static double _nextProbeTime;
        private static double _nextRefreshTime;

        static AutoSaveStatusBarIcon()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            double now = EditorApplication.timeSinceStartup;

            if (_icon == null || _icon.panel == null)
            {
                if (now < _nextProbeTime) return;
                _nextProbeTime = now + 2.0;
                TryInject();
                return;
            }

            if (now < _nextRefreshTime) return;
            _nextRefreshTime = now + 0.5;
            RefreshVisuals();
        }

        private static void TryInject()
        {
            try
            {
                VisualElement statusBarRoot = FindStatusBarRoot();
                if (statusBarRoot == null) return;
                _statusBarRoot = statusBarRoot;

                Texture iconTexture = LoadIconTexture();
                if (iconTexture == null) return;

                var icon = new Image
                {
                    image = iconTexture,
                    scaleMode = ScaleMode.ScaleToFit,
                    pickingMode = PickingMode.Position,
                };

                // Absolute-positioned just left of the native (IMGUI-drawn) icon cluster.
                icon.style.position = Position.Absolute;
                icon.style.width = IconSize;
                icon.style.height = IconSize;
                icon.style.right = ClusterWidth;             // right edge flush against the cluster
                icon.style.bottom = 2f;                      // vertically centred in the ~20px bar

                // Opaque backing so a long status message can't show through behind the icon.
                icon.style.backgroundColor = EditorGUIUtility.isProSkin
                    ? new Color(0.22f, 0.22f, 0.22f, 1f)
                    : new Color(0.78f, 0.78f, 0.78f, 1f);

                statusBarRoot.Add(icon);

                icon.RegisterCallback<ClickEvent>(_ => AutoSaveWindow.ShowWindow());
                icon.RegisterCallback<MouseEnterEvent>(_ => icon.style.opacity = 1f);
                icon.RegisterCallback<MouseLeaveEvent>(_ => RefreshVisuals());

                _icon = icon;
                RefreshVisuals();
            }
            catch (Exception)
            {
                // Internal API changed — give up quietly; the menu item still works.
                _icon = null;
                EditorApplication.update -= Update;
            }
        }

        private static void RefreshVisuals()
        {
            if (_icon == null) return;

            switch (AutoSave.State)
            {
                case AutoSave.SaveState.Disabled:
                    _icon.tintColor = new Color(1f, 1f, 1f, 0.35f);
                    _icon.style.opacity = 0.8f;
                    _icon.tooltip = "AutoSave is disabled — click to open settings.";
                    break;

                case AutoSave.SaveState.CountingDown:
                    _icon.tintColor = new Color(1f, 0.35f, 0.3f, 1f);
                    _icon.style.opacity = 1f;
                    _icon.tooltip = "Auto-saving now — click to open settings.";
                    break;

                default:
                    _icon.tintColor = Color.white;
                    _icon.style.opacity = 0.85f;
                    int s = (int)AutoSave.SecondsUntilNextSave;
                    string last = AutoSave.LastSaveTime.HasValue
                        ? $"Last save {AutoSave.LastSaveTime.Value:HH:mm:ss}."
                        : "No save yet this session.";
                    _icon.tooltip = $"AutoSave: next save in {s / 60:00}:{s % 60:00}. {last} Click for settings.";
                    break;
            }
        }

        private static Texture LoadIconTexture()
        {
            string[] candidates =
            {
                "d_SaveAs@2x", "SaveAs@2x", "d_SaveAs", "SaveAs", "SaveActive", "d_Save", "Save",
            };
            foreach (string name in candidates)
            {
                try
                {
                    GUIContent content = EditorGUIUtility.IconContent(name);
                    if (content?.image != null) return content.image;
                }
                catch (Exception) { }
            }
            return null;
        }

        private static VisualElement FindStatusBarRoot()
        {
            Type statusBarType = typeof(Editor).Assembly.GetType("UnityEditor.AppStatusBar");
            if (statusBarType == null) return null;

            UnityEngine.Object[] views = Resources.FindObjectsOfTypeAll(statusBarType);
            if (views == null || views.Length == 0) return null;

            object view = views[0];
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            for (Type t = view.GetType(); t != null; t = t.BaseType)
            {
                PropertyInfo vtProp = t.GetProperty("visualTree", flags);
                if (vtProp?.GetValue(view) is VisualElement direct)
                    return direct;

                PropertyInfo backendProp = t.GetProperty("windowBackend", flags);
                object backend = backendProp?.GetValue(view);
                if (backend != null)
                {
                    PropertyInfo backendTree = backend.GetType().GetProperty("visualTree", flags);
                    if (backendTree?.GetValue(backend) is VisualElement fromBackend)
                        return fromBackend;
                }
            }
            return null;
        }
    }
}
