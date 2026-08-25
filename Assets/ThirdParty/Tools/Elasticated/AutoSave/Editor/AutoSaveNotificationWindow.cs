using System;
using UnityEditor;
using UnityEngine;

namespace Elasticated.SceneAutoSave
{
    /// <summary>
    /// Compact "Auto-saving in N…" toast shown just above the status-bar AutoSave icon, with a ✕
    /// button to cancel the pending save.
    ///
    /// Stuck-window protection (the old implementation could survive a domain reload as a dead,
    /// unclickable window):
    ///  1. <see cref="AutoSave"/> closes the toast before every assembly reload.
    ///  2. Any instance deserialized after a reload (callbacks lost) closes itself in OnEnable.
    ///  3. A hard deadline in Tick force-closes the window if it somehow outlives the countdown.
    /// </summary>
    public class AutoSaveNotificationWindow : EditorWindow
    {
        private const float Width = 252f;
        private const float Height = 64f;

        /// <summary>Keep in sync with <see cref="AutoSaveStatusBarIcon.RightOffset"/>.</summary>
        private const float StatusBarRightOffset = 118f;

        private double _countdownEnd;
        private double _countdownDuration;
        private Action _onConfirm;
        private Action _onCancel;
        private bool _resolved;

        private static AutoSaveNotificationWindow _instance;
        private static bool _creatingThisDomain;

        public static AutoSaveNotificationWindow Open(double countdownSeconds, Action onConfirm, Action onCancel)
        {
            if (_instance != null)
                _instance.CloseSilently();

            _creatingThisDomain = true;
            AutoSaveNotificationWindow window;
            try
            {
                window = CreateInstance<AutoSaveNotificationWindow>();
            }
            finally
            {
                _creatingThisDomain = false;
            }

            window._countdownDuration = Math.Max(0.01, countdownSeconds);
            window._countdownEnd = EditorApplication.timeSinceStartup + countdownSeconds;
            window._onConfirm = onConfirm;
            window._onCancel = onCancel;

            // Anchor just above the status bar, horizontally aligned with the AutoSave icon.
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            window.position = new Rect(
                main.xMax - Width - StatusBarRightOffset + 100f,
                main.yMax - Height - 30f,
                Width, Height);

            window.ShowPopup(); // borderless toast; intentionally not focused so it never steals input

            _instance = window;
            return window;
        }

        /// <summary>Closes without firing confirm/cancel callbacks.</summary>
        public void CloseSilently()
        {
            _resolved = true;
            Close();
        }

        /// <summary>Destroys toast instances that survived a domain reload (their callbacks are dead).</summary>
        public static void CloseStrayInstances()
        {
            foreach (var stray in Resources.FindObjectsOfTypeAll<AutoSaveNotificationWindow>())
            {
                if (stray != _instance)
                {
                    stray._resolved = true;
                    stray.Close();
                }
            }
        }

        private void OnEnable()
        {
            // Deserialized after a domain reload (not created via Open): the controller and the
            // callbacks no longer exist, so this window is a zombie — close it.
            if (!_creatingThisDomain)
            {
                _resolved = true;
                EditorApplication.delayCall += () =>
                {
                    if (this != null) Close();
                };
                return;
            }

            EditorApplication.update += Tick;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Tick;
            if (!_resolved)
            {
                _resolved = true;
                _onCancel?.Invoke();
            }
            if (_instance == this)
                _instance = null;
        }

        private void Tick()
        {
            if (_resolved) return;

            double now = EditorApplication.timeSinceStartup;

            // Hard safety: never outlive the countdown by more than a few seconds.
            if (now > _countdownEnd + 10)
            {
                CloseSilently();
                return;
            }

            if (now >= _countdownEnd)
            {
                _resolved = true;
                _onConfirm?.Invoke();
                Close();
                return;
            }

            Repaint();
        }

        private void Confirm()
        {
            if (_resolved) return;
            _resolved = true;
            _onConfirm?.Invoke();
            Close();
        }

        private void Cancel()
        {
            if (_resolved) return;
            _resolved = true;
            _onCancel?.Invoke();
            Close();
        }

        private void OnGUI()
        {
            float w = position.width;
            float h = position.height;

            // Background + accent strip.
            EditorGUI.DrawRect(new Rect(0, 0, w, h), new Color(0.125f, 0.125f, 0.145f, 0.98f));
            EditorGUI.DrawRect(new Rect(0, 0, 3, h), new Color(0.72f, 0.16f, 0.16f));

            double remaining = Math.Max(0, _countdownEnd - EditorApplication.timeSinceStartup);
            int seconds = Mathf.CeilToInt((float)remaining);
            float progress = 1f - (float)(remaining / _countdownDuration);

            // Title row.
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.92f, 0.92f, 0.92f) },
            };
            GUI.Label(new Rect(12, 7, w - 60, 18), $"Auto-saving in {seconds}…", titleStyle);

            // ✕ close (cancels this save).
            var closeStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                normal = { textColor = new Color(0.75f, 0.75f, 0.75f) },
                hover = { textColor = Color.white },
            };
            if (GUI.Button(new Rect(w - 24, 5, 18, 18), new GUIContent("✕", "Skip this auto-save"), closeStyle))
                Cancel();

            // Progress bar.
            var track = new Rect(12, 30, w - 24, 5);
            EditorGUI.DrawRect(track, new Color(0.22f, 0.22f, 0.25f));
            EditorGUI.DrawRect(new Rect(track.x, track.y, track.width * progress, track.height),
                new Color(0.72f, 0.16f, 0.16f));

            // Save-now shortcut.
            var saveNowStyle = new GUIStyle(EditorStyles.miniButton) { fontSize = 10 };
            if (GUI.Button(new Rect(w - 84, h - 24, 72, 18), "Save now", saveNowStyle))
                Confirm();

            var hintStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.55f, 0.55f, 0.58f) },
            };
            GUI.Label(new Rect(12, h - 23, w - 100, 16), "Saving all open scenes", hintStyle);
        }
    }
}
