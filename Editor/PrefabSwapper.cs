using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorTools.Editor
{
    public class PrefabSwapper : EditorWindow
    {
        private GameObject _prefab;
        private bool _copyRotation = true;
        private bool _copyScale = true;

        [MenuItem("Tools/Prefab Swapper", false, 2000)]
        public static void ShowWindow()
        {
            GetWindow(typeof(PrefabSwapper));
        }

        private void OnGUI()
        {
            _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), allowSceneObjects: false);
            _copyRotation = EditorGUILayout.Toggle("Copy Rotation", _copyRotation);
            _copyScale = EditorGUILayout.Toggle("Copy Scale", _copyScale);

            if (GUILayout.Button("Swap All Selected"))
            {
                if (_prefab == null)
                {
                    Debug.LogError("Prefab field must have valid prefab selected!");
                    return;
                }

                Transform[] allSelected = Selection.GetTransforms(SelectionMode.Unfiltered);
                for (int i = 0; i < allSelected.Length; i++)
                {
                    Transform currentSelected = allSelected[i];
                    GameObject instantiated = (GameObject)PrefabUtility.InstantiatePrefab(_prefab);
                    instantiated.transform.parent = currentSelected.parent;
                    instantiated.transform.position = currentSelected.position;
                    if (_copyRotation) instantiated.transform.rotation = currentSelected.rotation;
                    if (_copyScale) instantiated.transform.localScale = currentSelected.localScale;
                    Undo.RegisterCreatedObjectUndo(instantiated, "Swap");
                    Undo.DestroyObjectImmediate(currentSelected.gameObject);
                }
            }
        }
    }
}