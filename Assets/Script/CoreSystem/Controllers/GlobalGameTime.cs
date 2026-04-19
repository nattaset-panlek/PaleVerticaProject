using UnityEngine;
using System;

namespace CoreSystem.Controllers
{
    public class GlobalGameTime : MonoBehaviour
    {
        #region [Variables/Fields]
        public static GlobalGameTime Instance { get; private set; }

        [Header("Time Simulation")]
        [Tooltip("How many real-world seconds equal one game minute.")]
        [SerializeField] private float realSecondsPerGameMinute = 1f;

        [Header("Starting Time")]
        [SerializeField][Range(0, 23)] private int startHour = 8;
        [SerializeField][Range(0, 59)] private int startMinute = 0;

        private int _currentHour;
        private int _currentMinute;
        private float _timeAccumulator = 0f;
        #endregion

        #region [Properties]
        public int CurrentHour => _currentHour;
        public int CurrentMinute => _currentMinute;
        #endregion

        #region [Events]
        /// <summary>
        /// Triggered whenever the game minute changes. Event signature: (int hour, int minute)
        /// </summary>
        public event Action<int, int> OnTimeChanged;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            _currentHour = startHour;
            _currentMinute = startMinute;
        }

        private void Update()
        {
            // Virtual Time Simulation
            _timeAccumulator += Time.deltaTime;
            if (_timeAccumulator >= realSecondsPerGameMinute)
            {
                int gameMinutesPassed = Mathf.FloorToInt(_timeAccumulator / realSecondsPerGameMinute);
                _timeAccumulator -= gameMinutesPassed * realSecondsPerGameMinute;

                AdvanceTime(gameMinutesPassed);
            }
        }
        #endregion

        #region [Public Methods]
        public void AdvanceTime(int minutes)
        {
            _currentMinute += minutes;

            while (_currentMinute >= 60)
            {
                _currentMinute -= 60;
                _currentHour++;
                
                if (_currentHour >= 24)
                {
                    _currentHour -= 24;
                    // Optional: Trigger OnDayPassed Here
                }
            }

            OnTimeChanged?.Invoke(_currentHour, _currentMinute);
        }

        public void SetTime(int hour, int minute)
        {
            _currentHour = Mathf.Clamp(hour, 0, 23);
            _currentMinute = Mathf.Clamp(minute, 0, 59);
            OnTimeChanged?.Invoke(_currentHour, _currentMinute);
        }
        #endregion
    }
}
