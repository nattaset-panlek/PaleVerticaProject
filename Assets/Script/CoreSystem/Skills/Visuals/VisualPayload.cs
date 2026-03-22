using UnityEngine;

namespace CoreSystem.Skills.Visuals
{
    [CreateAssetMenu(fileName = "NewVisualPayload", menuName = "Skill System/Visual Payload")]
    public class VisualPayload : ScriptableObject
    {
        public VisualCategory Category;
        public GameObject Prefab;
    }
}
