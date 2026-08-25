using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elasticated.SceneAutoSave
{
    /// <summary>
    /// Core auto-save controller. Runs a repeating timer; when it elapses it shows a small
    /// countdown toast (<see cref="AutoSaveNotificationWindow"/>) above the status bar, then saves
    /// all open scenes. Settings live in EditorPrefs and are edited via <see cref="AutoSaveWindow"/>.
    ///
    /// Reload safety: the countdown is cancelled (and the toast closed) before any domain reload or
    /// while the editor is compiling/importing, and stray toast windows that survived a reload are
    /// destroyed on init — this prevents the orphaned "stuck at 0" notification.
    /// </summary>
    [InitializeOnLoad]
    public static class AutoSave
    {
        public enum SaveState
        {
            Disabled,
            Waiting,
            CountingDown,
        }

        // EditorPrefs keys (kept stable for backwards compatibility with older settings).
        public const string KeyEnabled = "AutoSaveEnabled";
        public const string KeyInterval = "AutoSaveInterval";
        public const string KeyOnlyIfEdited = "SaveOnlyIfEdited";
        public const string KeyBeforePlayMode = "SaveBeforePlayMode";
        public const string KeyCountdown = "AutoSaveCountdown";
        public const string KeyVerboseLogging = "AutoSaveVerboseLogging";

        private const string SessionKeyLastSave = "AutoSave.LastSaveTime";

        private static bool _enabled;
        private static double _interval;
        private static bool _onlyIfEdited;
        private static bool _beforePlayMode;
        private static double _countdownDuration;
        private static bool _verboseLogging;

        private static double _nextSaveTime;
        private static bool _isCountingDown;
        private static AutoSaveNotificationWindow _toast;

        public static bool Enabled => _enabled;
        public static double Interval => _interval;
        public static bool OnlyIfEdited => _onlyIfEdited;
        public static bool BeforePlayMode => _beforePlayMode;
        public static double CountdownDuration => _countdownDuration;
        public static bool VerboseLogging => _verboseLogging;

        public static SaveState State
        {
            get
            {
                if (!_enabled) return SaveState.Disabled;
                return _isCountingDown ? SaveState.CountingDown : SaveState.Waiting;
            }
        }

        /// <summary>Seconds until the next save attempt (0 while counting down).</summary>
        public static double SecondsUntilNextSave =>
            _isCountingDown ? 0 : System.Math.Max(0, _nextSaveTime - EditorApplication.timeSinceStartup);

        /// <summary>Last successful auto/manual save this editor session, or null.</summary>
        public static System.DateTime? LastSaveTime
        {
            get
            {
                string raw = SessionState.GetString(SessionKeyLastSave, string.Empty);
                if (string.IsNullOrEmpty(raw)) return null;
                return System.DateTime.TryParse(raw, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var t) ? t : null;
            }
        }

        static AutoSave()
        {
            LoadSettings();
            _nextSaveTime = EditorApplication.timeSinceStartup + _interval;

            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            // Close the toast before scripts recompile so it can never get orphaned by the reload.
            AssemblyReloadEvents.beforeAssemblyReload += CloseToast;

            // Destroy any toast window that survived the previous domain reload (serialized window
            // with dead callbacks) — this is the fix for the notification getting stuck on screen.
            EditorApplication.delayCall += AutoSaveNotificationWindow.CloseStrayInstances;
        }

        private static void LoadSettings()
        {
            _enabled = EditorPrefs.GetBool(KeyEnabled, true);
            _interval = System.Math.Max(10, EditorPrefs.GetFloat(KeyInterval, 60f));
            _onlyIfEdited = EditorPrefs.GetBool(KeyOnlyIfEdited, true);
            _beforePlayMode = EditorPrefs.GetBool(KeyBeforePlayMode, true);
            _countdownDuration = System.Math.Max(0, EditorPrefs.GetFloat(KeyCountdown, 5f));
            _verboseLogging = EditorPrefs.GetBool(KeyVerboseLogging, false);
        }

        /// <summary>Re-reads settings from EditorPrefs and restarts the timer.</summary>
        public static void ReloadSettings()
        {
            LoadSettings();
            CancelCountdown();
            _nextSaveTime = EditorApplication.timeSinceStartup + _interval;
        }

        /// <summary>Saves all open scenes immediately and restarts the timer.</summary>
        public static void SaveNow()
        {
            CancelCountdown();
            SaveScenes();
            _nextSaveTime = EditorApplication.timeSinceStartup + _interval;
        }

        /// <summary>Dismisses the countdown without saving and restarts the timer.</summary>
        public static void CancelCountdown()
        {
            _isCountingDown = false;
            CloseToast();
            _nextSaveTime = EditorApplication.timeSinceStartup + _interval;
        }

        /// <summary>Starts the save countdown right now (used by the settings window preview).
        /// Bypasses the scene-dirty guard so the toast always appears regardless of saved state.</summary>
        public static void TriggerCountdownNow()
        {
            if (_isCountingDown || _countdownDuration <= 0) return;

            _isCountingDown = true;
            _toast = AutoSaveNotificationWindow.Open(
                _countdownDuration,
                onConfirm: () =>
                {
                    _isCountingDown = false;
                    _toast = null;
                    SaveScenes();
                    _nextSaveTime = EditorApplication.timeSinceStartup + _interval;
                },
                onCancel: () =>
                {
                    _isCountingDown = false;
                    _toast = null;
                    _nextSaveTime = EditorApplication.timeSinceStartup + _interval;
                });
        }

        private static void Update()
        {
            if (!_enabled) return;

            // While the editor compiles or imports, never run (or keep running) a countdown — the
            // imminent domain reload would orphan the toast and the save itself can be deferred.
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                if (_isCountingDown)
                    CancelCountdown();
                else
                    _nextSaveTime = System.Math.Max(_nextSaveTime, EditorApplication.timeSinceStartup + 5);
                return;
            }

            // Safety: toast was closed externally without resolving.
            if (_isCountingDown && _toast == null)
            {
                _isCountingDown = false;
                _nextSaveTime = EditorApplication.timeSinceStartup + _interval;
            }

            if (_isCountingDown) return;
            if (EditorApplication.timeSinceStartup < _nextSaveTime) return;

            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                _nextSaveTime = EditorApplication.timeSinceStartup + _interval;
                return;
            }

            if (_onlyIfEdited && !AnyOpenSceneDirty())
            {
                _nextSaveTime = EditorApplication.timeSinceStartup + _interval;
                return;
            }

            if (_countdownDuration > 0)
            {
                _isCountingDown = true;
                _toast = AutoSaveNotificationWindow.Open(
                    _countdownDuration,
                    onConfirm: () =>
                    {
                        _isCountingDown = false;
                        _toast = null;
                        SaveScenes();
                        _nextSaveTime = EditorApplication.timeSinceStartup + _interval;
                    },
                    onCancel: () =>
                    {
                        _isCountingDown = false;
                        _toast = null;
                        _nextSaveTime = EditorApplication.timeSinceStartup + _interval;
                    });
            }
            else
            {
                SaveScenes();
                _nextSaveTime = EditorApplication.timeSinceStartup + _interval;
            }
        }

        private static bool AnyOpenSceneDirty()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).isDirty)
                    return true;
            }
            return false;
        }

        private static void SaveScenes()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            if (EditorSceneManager.SaveOpenScenes())
            {
                SessionState.SetString(SessionKeyLastSave, System.DateTime.Now.ToString("o"));
                Log("Auto-saved all open scenes at " + System.DateTime.Now);
            }
        }

        private static void CloseToast()
        {
            if (_toast != null)
            {
                _toast.CloseSilently();
                _toast = null;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                // Entering play mode invalidates any pending countdown.
                CancelCountdown();

                if (_beforePlayMode)
                {
                    if (EditorSceneManager.SaveOpenScenes())
                    {
                        SessionState.SetString(SessionKeyLastSave, System.DateTime.Now.ToString("o"));
                        Log("Auto-saved all open scenes before entering play mode at " + System.DateTime.Now);
                    }
                }
            }
        }

        /// <summary>Console output, off by default so a save every interval never spams the log.</summary>
        private static void Log(string message)
        {
            if (_verboseLogging)
                Debug.Log("[AutoSave] " + message);
        }
    }
}
