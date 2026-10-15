using UnityEditor;
using UnityEngine;
using CoreSystem.UI;

[CustomEditor(typeof(CursorManager))]
public class CursorManagerEditor : Editor
{
    private SerializedProperty cursorStatesProp;
    private SerializedProperty attackableLayerProp;
    private SerializedProperty interactableLayerProp;

    private int previewIndex = -1;

    private void OnEnable()
    {
        cursorStatesProp = serializedObject.FindProperty("cursorStates");
        attackableLayerProp = serializedObject.FindProperty("attackableLayer");
        interactableLayerProp = serializedObject.FindProperty("interactableLayer");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(attackableLayerProp);
        EditorGUILayout.PropertyField(interactableLayerProp);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Cursor States Configuration", EditorStyles.boldLabel);
        
        for (int i = 0; i < cursorStatesProp.arraySize; i++)
        {
            SerializedProperty element = cursorStatesProp.GetArrayElementAtIndex(i);
            SerializedProperty typeProp = element.FindPropertyRelative("type");
            SerializedProperty spriteProp = element.FindPropertyRelative("cursorSprite");
            SerializedProperty hotSpotProp = element.FindPropertyRelative("hotSpot");

            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            EditorGUILayout.PropertyField(typeProp);
            EditorGUILayout.PropertyField(spriteProp);
            EditorGUILayout.PropertyField(hotSpotProp);

            // Toggle Preview Button
            if (GUILayout.Button(previewIndex == i ? "Close Preview" : "Preview & Edit Hot Spot"))
            {
                if (previewIndex == i) previewIndex = -1;
                else previewIndex = i;
            }

            // Draw Preview
            if (previewIndex == i && spriteProp.objectReferenceValue != null)
            {
                DrawHotSpotPreview((Sprite)spriteProp.objectReferenceValue, hotSpotProp);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        // Array Controls
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New Cursor State"))
        {
            cursorStatesProp.InsertArrayElementAtIndex(cursorStatesProp.arraySize);
        }
        if (GUILayout.Button("Remove Last State") && cursorStatesProp.arraySize > 0)
        {
            cursorStatesProp.DeleteArrayElementAtIndex(cursorStatesProp.arraySize - 1);
            if (previewIndex >= cursorStatesProp.arraySize) previewIndex = -1;
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHotSpotPreview(Sprite sprite, SerializedProperty hotSpotProp)
    {
        if (sprite.texture == null) return;

        Texture2D tex = sprite.texture;
        Rect texRect = sprite.textureRect;
        Vector2 hotSpot = hotSpotProp.vector2Value;

        // Calculate Preview Box Dimensions (Preserve Aspect Ratio)
        float previewSize = 150f;
        float aspect = texRect.width / texRect.height;
        float width = previewSize * (aspect > 1 ? 1 : aspect);
        float height = previewSize * (aspect < 1 ? 1 : (1 / aspect));

        // Create Rect and center it
        Rect rect = GUILayoutUtility.GetRect(width, height);
        rect.width = width;
        rect.height = height;
        rect.x += (EditorGUIUtility.currentViewWidth - width) / 2f - 20f;

        // Interactive Click to Set HotSpot
        Event e = Event.current;
        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            float relX = e.mousePosition.x - rect.x;
            float relY = e.mousePosition.y - rect.y;

            // Map UI click back to Sprite Pixels
            float newHotSpotX = (relX / width) * texRect.width;
            float newHotSpotY = (relY / height) * texRect.height;

            hotSpotProp.vector2Value = new Vector2(Mathf.Round(newHotSpotX), Mathf.Round(newHotSpotY));
            GUI.changed = true;
            e.Use();
        }

        // Draw Dark Background
        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f, 1f));

        // Draw Sprite Texture
        Rect coords = new Rect(
            texRect.x / tex.width,
            texRect.y / tex.height,
            texRect.width / tex.width,
            texRect.height / tex.height
        );
        GUI.DrawTextureWithTexCoords(rect, tex, coords, true);

        // Draw Crosshair at HotSpot
        float hotSpotX = (hotSpot.x / texRect.width) * width;
        float hotSpotY = (hotSpot.y / texRect.height) * height;

        Vector2 center = new Vector2(rect.x + hotSpotX, rect.y + hotSpotY);

        Handles.BeginGUI();
        Handles.color = Color.red;
        Handles.DrawLine(center - Vector2.right * 10, center + Vector2.right * 10);
        Handles.DrawLine(center - Vector2.up * 10, center + Vector2.up * 10);
        Handles.EndGUI();
        
        EditorGUILayout.HelpBox("Click directly on the image above to accurately set the Hot Spot position.", MessageType.Info);
    }
}
