#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace ScenarioTesting
{
    public sealed class ScenarioInput
    {
        #region [Variables/Fields]
        private const string KeyboardName = "ScenarioKeyboard";
        private const string MouseName = "ScenarioMouse";
        private const float DragKickPixels = 24f;
        private const int DragKickFrames = 3;

        private readonly HashSet<Key> _held = new HashSet<Key>();
        private Keyboard _keyboard;
        private Mouse _mouse;
        private Vector2 _pointer;
        private MouseButton? _pressed;
        private InputSettings.BackgroundBehavior _savedBackground;
        private InputSettings.EditorInputBehaviorInPlayMode _savedEditorBehaviour;
        #endregion

        #region [Properties]
        public bool IsEnabled => _keyboard != null;
        public Vector2 Pointer => _pointer;
        public bool OwnsPointer => _mouse != null && Mouse.current == _mouse;
        #endregion

        #region [Public Methods]
        public void Enable()
        {
            if (IsEnabled) return;
            _savedBackground = InputSystem.settings.backgroundBehavior;
            _savedEditorBehaviour = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            _keyboard = InputSystem.AddDevice<Keyboard>(KeyboardName);
            _mouse = InputSystem.AddDevice<Mouse>(MouseName);
            _pointer = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Send();
        }

        public void Disable()
        {
            if (!IsEnabled) return;
            _held.Clear();
            _pressed = null;
            InputSystem.RemoveDevice(_keyboard);
            InputSystem.RemoveDevice(_mouse);
            _keyboard = null;
            _mouse = null;
            InputSystem.settings.backgroundBehavior = _savedBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = _savedEditorBehaviour;
        }

        public void KeepCurrent()
        {
            if (!IsEnabled) return;
            _keyboard.MakeCurrent();
            _mouse.MakeCurrent();
        }

        public void Hold(params Key[] keys)
        {
            _held.Clear();
            foreach (Key key in keys) if (key != Key.None) _held.Add(key);
            Send();
        }

        public void ReleaseAll()
        {
            _held.Clear();
            _pressed = null;
            Send();
        }

        public IEnumerator Tap(Key key, int frames = 2)
        {
            var before = new List<Key>(_held);
            _held.Add(key);
            Send();
            for (int i = 0; i < frames; i++) yield return null;
            _held.Clear();
            foreach (Key held in before) _held.Add(held);
            Send();
            yield return null;
        }

        public void Point(Vector2 screen)
        {
            _pointer = screen;
            Send();
        }

        public IEnumerator Click(Vector2 screen, MouseButton button = MouseButton.Left)
        {
            Point(screen);
            yield return null;
            _pressed = button;
            Send();
            yield return null;
            yield return null;
            _pressed = null;
            Send();
            yield return null;
        }

        public IEnumerator DoubleClick(Vector2 screen, MouseButton button = MouseButton.Left)
        {
            Point(screen);
            yield return null;
            for (int i = 0; i < 2; i++)
            {
                _pressed = button;
                Send();
                yield return null;
                _pressed = null;
                Send();
                yield return null;
            }
            yield return null;
        }

        public IEnumerator Drag(Vector2 from, Vector2 to, int frames = 16)
        {
            Point(from);
            yield return null;
            _pressed = MouseButton.Left;
            Send();
            yield return null;
            yield return null;

            Vector2 start = from + new Vector2(DragKickPixels, 0f);
            for (int i = 1; i <= DragKickFrames; i++)
            {
                Point(Vector2.Lerp(from, start, (float)i / DragKickFrames));
                yield return null;
            }
            for (int i = 1; i <= frames; i++)
            {
                Point(Vector2.Lerp(start, to, (float)i / frames));
                yield return null;
            }
            yield return null;
            _pressed = null;
            Send();
            yield return null;
            yield return null;
        }

        public IEnumerator ClickWorld(Vector3 world, MouseButton button = MouseButton.Left)
        {
            if (!TryScreenOf(world, out Vector2 screen)) yield break;
            yield return Click(screen, button);
        }

        public IEnumerator ClickButton(Button button)
        {
            if (button == null) yield break;
            yield return Click(ScreenOf(button.transform as RectTransform));
        }

        public static bool TryScreenOf(Vector3 world, out Vector2 screen)
        {
            screen = Vector2.zero;
            Camera camera = Camera.main;
            if (camera == null) return false;
            Vector3 point = camera.WorldToScreenPoint(world);
            screen = point;
            return point.z >= 0f && point.x >= 0f && point.y >= 0f && point.x <= Screen.width && point.y <= Screen.height;
        }

        public static Vector2 ScreenOf(RectTransform rect)
        {
            if (rect == null) return Vector2.zero;
            Canvas canvas = rect.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            return RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
        }

        public static Button FindButton(string label)
        {
            if (string.IsNullOrEmpty(label)) return null;
            Button partial = null;
            foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
            {
                if (!button.isActiveAndEnabled || !button.interactable) continue;
                string text = LabelOf(button);
                if (string.Equals(text, label, System.StringComparison.OrdinalIgnoreCase)) return button;
                if (partial == null && text.IndexOf(label, System.StringComparison.OrdinalIgnoreCase) >= 0) partial = button;
            }
            return partial;
        }

        public static List<string> UIUnder(Vector2 screen)
        {
            var names = new List<string>();
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null) return names;
            var hits = new List<RaycastResult>();
            eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = screen }, hits);
            foreach (RaycastResult hit in hits) if (hit.gameObject != null) names.Add(hit.gameObject.name);
            return names;
        }

        public static bool IsInside(Vector2 screen, float margin)
        {
            float x = Screen.width * margin;
            float y = Screen.height * margin;
            return screen.x >= x && screen.y >= y && screen.x <= Screen.width - x && screen.y <= Screen.height - y;
        }

        public static List<string> VisibleButtonLabels()
        {
            var labels = new List<string>();
            foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
            {
                if (!button.isActiveAndEnabled) continue;
                string text = LabelOf(button);
                if (text.Length > 0) labels.Add(text);
            }
            return labels;
        }

        public static string LabelOf(Button button)
        {
            TMP_Text tmp = button.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null) return tmp.text.Trim();
            Text legacy = button.GetComponentInChildren<Text>(true);
            return legacy != null ? legacy.text.Trim() : button.name;
        }
        #endregion

        #region [Private Methods]
        private void Send()
        {
            if (!IsEnabled) return;
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(new List<Key>(_held).ToArray()));
            var state = new MouseState { position = _pointer };
            if (_pressed.HasValue) state = state.WithButton(_pressed.Value);
            InputSystem.QueueStateEvent(_mouse, state);
            KeepCurrent();
        }
        #endregion
    }
}
#endif
