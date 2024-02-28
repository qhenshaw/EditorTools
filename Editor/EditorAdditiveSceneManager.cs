using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EditorTools.Editor
{
    [InitializeOnLoad]
    public class EditorAdditiveSceneManager
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorSceneManager.sceneOpened += OnEditorSceneOpened;
        }

        private static void OnEditorSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (scene != SceneManager.GetActiveScene()) return;
            AdditiveSceneManager sceneManager = Object.FindAnyObjectByType<AdditiveSceneManager>();
            sceneManager?.LoadSceneList();
        }

        [MenuItem("GameObject/Additive Scene Manager", false, 0)]
        static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("= Additive Scene Manager");
            go.AddComponent<AdditiveSceneManager>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
    }
}