using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Components
{
    [System.Serializable]
    public class Tab
    {
        public Button ActionButtonTabs;
        public GameObject Default;
        public GameObject Active;
        public GameObject Page;
        public UnityEvent OnActive;
    }
}
