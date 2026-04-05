using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Utile
{
    [ExecuteAlways]
    [RequireComponent(typeof(CanvasScaler))]
    public class DynamicCanvasScaler : MonoBehaviour
    {
        #region [Variables/Fields]

        [Tooltip("อัตราส่วนหน้าจอเริ่มต้นที่ใช้ออกแบบ UI (เช่น ยึด 1920x1080 เป็นหลัก)")]
        public Vector2 ReferenceResolution = new Vector2(1920, 1080);
        
        private CanvasScaler _canvasScaler;
        private float _lastScreenWidth = 0;
        private float _lastScreenHeight = 0;

        #endregion

        #region [Unity Methods]

        private void Awake()
        {
            _canvasScaler = GetComponent<CanvasScaler>();
            UpdateScaleMode();
        }

        private void Update()
        {
            // ทำงานเฉพาะเมื่อขนาดหน้าจอมีการเปลี่ยนแปลง เพื่อประหยัด Performance
            if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
            {
                UpdateScaleMode();
            }
        }

        #endregion

        #region [Private Methods]

        private void UpdateScaleMode()
        {
            if (_canvasScaler == null || _canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                return;

            _canvasScaler.referenceResolution = ReferenceResolution;

            float currentAspect = (float)Screen.width / Screen.height;
            float referenceAspect = ReferenceResolution.x / ReferenceResolution.y;

            // ถ้าจอปัจจุบัน 'กว้างกว่า' จอที่ออกแบบไว้ (เช่น จอ Ultrawide 21:9)
            // ให้ยึดความสูงเป็นแกนหลัก (Match = 1) เพื่อให้ภาพไม่โดนตัดบนล่าง
            if (currentAspect >= referenceAspect)
            {
                _canvasScaler.matchWidthOrHeight = 1f; // Match Height
            }
            // ถ้าจอปัจจุบัน 'แคบกว่า/สูงกว่า' จอที่ออกแบบไว้ (เช่นจอ 4:3 หรือ iPad 16:10)
            // ให้ยึดความกว้างเป็นแกนหลัก (Match = 0) เพื่อให้ภาพไม่โดนตัดซ้ายขวา
            else
            {
                _canvasScaler.matchWidthOrHeight = 0f; // Match Width
            }

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
        }

        #endregion
    }
}
