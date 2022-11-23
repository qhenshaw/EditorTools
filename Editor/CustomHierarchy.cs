using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EditorTools.Editor
{
    [InitializeOnLoad]
    public class CustomHierarchy : MonoBehaviour
    {
        private static Vector2 _offset = new Vector2(0, 2);
        private static Color _backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        private static Color _selectedBackgroundColor = new Color(0.175f, 0.35f, 0.5f);
        private static Color _normalTextColor = Color.white;
        private static Color _selectedTextColor = Color.white;

        static CustomHierarchy()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
        }

        private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            Color fontColor = _normalTextColor;
            Color backgroundColor = _backgroundColor;

            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj != null && obj.name.Length >= 2)
            {
                if (obj.name.Substring(0, 2).Equals("= "))
                {
                    if (Selection.instanceIDs.Contains(instanceID))
                    {
                        fontColor = _selectedTextColor;
                        backgroundColor = _selectedBackgroundColor;
                    }

                    Rect offsetRect = new Rect(selectionRect.position + _offset, selectionRect.size);
                    EditorGUI.DrawRect(selectionRect, backgroundColor);
                    string name = " " + obj.name.Substring(2, obj.name.Length - 2);
                    EditorGUI.LabelField(offsetRect, name, new GUIStyle()
                    {
                        normal = new GUIStyleState() { textColor = fontColor },
                        fontStyle = FontStyle.Bold
                    }
                    );
                }
            }
        }
    }
}