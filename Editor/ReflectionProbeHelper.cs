using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorTools.Editor
{
    public class ReflectionProbeHelper
    {
        [MenuItem("GameObject/Light/Surround with Reflection Probe", false, 0)]
        static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            if (Selection.activeGameObject == null) return;

            GameObject go = new GameObject("Reflection Probe");
            ReflectionProbe probe = go.AddComponent<ReflectionProbe>();
            Undo.RegisterCreatedObjectUndo(go, "Create" + go.name);

            Bounds bounds = new Bounds();
            MeshRenderer[] renderers = Selection.activeGameObject.GetComponentsInChildren<MeshRenderer>();
            bounds.Encapsulate(Selection.activeGameObject.transform.position);
            foreach (MeshRenderer meshRenderer in renderers)
            {
                bounds.Encapsulate(meshRenderer.bounds);
            }
            probe.transform.position = bounds.center;
            probe.size = bounds.size + Vector3.one * 2f;

            Selection.activeObject = go;
        }
    }
}