using UnityEditor;
using UnityEngine;

namespace Elasticated.SceneAutoSave
{
    /// <summary>
    /// AutoSave settings window. Changes apply immediately (no separate "save settings" step).
    /// Opened from Window ▸ AutoSave ▸ Settings or by clicking the status-bar AutoSave icon.
    /// </summary>
    public class AutoSaveWindow : EditorWindow
    {
        private bool _enabled;
        private float _interval;
        private bool _onlyIfEdited;
        private bool _beforePlayMode;
        private float _countdown;
        private bool _verboseLogging;

        private static readonly Color Accent = new(0.72f, 0.16f, 0.16f);

        [MenuItem("Window/AutoSave/Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<AutoSaveWindow>();
            window.titleContent = new GUIContent("AutoSave", EditorGUIUtility.IconContent("d_SaveAs").image);
            window.minSize = new Vector2(340, 380);
            window.Show();
        }

        private void OnEnable()
        {
            _enabled = EditorPrefs.GetBool(AutoSave.KeyEnabled, true);
            _interval = EditorPrefs.GetFloat(AutoSave.KeyInterval, 60f);
            _onlyIfEdited = EditorPrefs.GetBool(AutoSave.KeyOnlyIfEdited, true);
            _beforePlayMode = EditorPrefs.GetBool(AutoSave.KeyBeforePlayMode, true);
            _countdown = EditorPrefs.GetFloat(AutoSave.KeyCountdown, 5f);
            _verboseLogging = EditorPrefs.GetBool(AutoSave.KeyVerboseLogging, false);
        }

        private void Update()
        {
            // Keep the live status (next-save countdown) ticking.
            Repaint();
        }

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(8);
            DrawStatusCard();
            EditorGUILayout.Space(8);
            DrawSettings();
            EditorGUILayout.Space(10);
            DrawActions();
        }

        // ── Sections ────────────────────────────────────────────────────────────

        private void DrawHeader()
        {
            Rect band = EditorGUILayout.GetControlRect(false, 44);
            band.x = 0;
            band.width = position.width;
            EditorGUI.DrawRect(band, new Color(0.16f, 0.16f, 0.18f));
            EditorGUI.DrawRect(new Rect(band.x, band.yMax - 2, band.width, 2), Accent);

            var icon = EditorGUIUtility.IconContent("d_SaveAs@2x").image;
            if (icon != null)
                GUI.DrawTexture(new Rect(12, band.y + 8, 28, 28), icon, ScaleMode.ScaleToFit);

            var titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 };
            GUI.Label(new Rect(50, band.y + 5, band.width - 60, 20), "AutoSave", titleStyle);

            var subStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.62f, 0.62f, 0.65f) },
            };
            GUI.Label(new Rect(50, band.y + 25, band.width - 60, 14),
                "Periodically saves all open scenes while you work", subStyle);
        }

        private void DrawStatusCard()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string stateText;
                Color stateColor;
                switch (AutoSave.State)
                {
                    case AutoSave.SaveState.Disabled:
                        stateText = "Disabled";
                        stateColor = new Color(0.6f, 0.6f, 0.6f);
                        break;

                    case AutoSave.SaveState.CountingDown:
                        stateText = "Saving…";
                        stateColor = new Color(0.95f, 0.45f, 0.35f);
                        break;

                    default:
                        int s = (int)AutoSave.SecondsUntilNextSave;
                        stateText = $"Next save in {s / 60:00}:{s % 60:00}";
                        stateColor = new Color(0.45f, 0.8f, 0.5f);
                        break;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    var dotStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = stateColor } };
                    EditorGUILayout.LabelField("●", dotStyle, GUILayout.Width(16));
                    EditorGUILayout.LabelField(stateText, EditorStyles.boldLabel);
                }

                string last = AutoSave.LastSaveTime.HasValue
                    ? AutoSave.LastSaveTime.Value.ToString("HH:mm:ss")
                    : "—";
                EditorGUILayout.LabelField("Last save (this session)", last);
            }
        }

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            _enabled = EditorGUILayout.ToggleLeft(
                new GUIContent("  Enable AutoSave", "Master switch for the automatic scene saving."),
                _enabled);

            using (new EditorGUI.DisabledScope(!_enabled))
            {
                EditorGUILayout.Space(4);

                _interval = EditorGUILayout.IntSlider(
                    new GUIContent("Save interval (s)", "How often the auto-save triggers."),
                    (int)_interval, 10, 600);

                _countdown = EditorGUILayout.IntSlider(
                    new GUIContent("Countdown (s)",
                        "Warning toast duration before the save. 0 saves instantly with no toast."),
                    (int)_countdown, 0, 30);

                EditorGUILayout.Space(4);

                _onlyIfEdited = EditorGUILayout.ToggleLeft(
                    new GUIContent("  Save only when a scene has unsaved changes"),
                    _onlyIfEdited);

                _beforePlayMode = EditorGUILayout.ToggleLeft(
                    new GUIContent("  Save before entering Play Mode"),
                    _beforePlayMode);

                _verboseLogging = EditorGUILayout.ToggleLeft(
                    new GUIContent("  Log each save to the Console",
                        "Prints a line to the Console every time AutoSave saves your scenes."),
                    _verboseLogging);
            }

            if (EditorGUI.EndChangeCheck())
                ApplySettings();
        }

        private void DrawActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Save Now", "Save all open scenes immediately."),
                        GUILayout.Height(26)))
                {
                    AutoSave.SaveNow();
                }

                using (new EditorGUI.DisabledScope(!_enabled || _countdown <= 0))
                {
                    if (GUILayout.Button(new GUIContent("Preview Notification",
                            "Shows the countdown toast right now so you can see how it looks."),
                            GUILayout.Height(26)))
                    {
                        AutoSave.TriggerCountdownNow();
                    }
                }
            }

            if (GUILayout.Button("Reset to Defaults", GUILayout.Height(20)))
            {
                _enabled = true;
                _interval = 60f;
                _onlyIfEdited = true;
                _beforePlayMode = true;
                _countdown = 5f;
                _verboseLogging = false;
                ApplySettings();
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox(
                "Changes apply immediately. The AutoSave icon in the status bar (bottom right) " +
                "shows the current state — click it to reopen this window.",
                MessageType.Info);
        }

        private void ApplySettings()
        {
            EditorPrefs.SetBool(AutoSave.KeyEnabled, _enabled);
            EditorPrefs.SetFloat(AutoSave.KeyInterval, _interval);
            EditorPrefs.SetBool(AutoSave.KeyOnlyIfEdited, _onlyIfEdited);
            EditorPrefs.SetBool(AutoSave.KeyBeforePlayMode, _beforePlayMode);
            EditorPrefs.SetFloat(AutoSave.KeyCountdown, _countdown);
            EditorPrefs.SetBool(AutoSave.KeyVerboseLogging, _verboseLogging);
            AutoSave.ReloadSettings();
        }
    }
}
