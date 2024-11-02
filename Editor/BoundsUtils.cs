using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorTools.Editor
{
    public static class BoundsUtils
    {
        public static bool TryGetSelectionBounds(out Bounds bounds, float padding = 2f)
        {
            bounds = new Bounds();
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0) return false;

            GameObject[] selection = Selection.gameObjects;
            List<MeshRenderer> renderers = new List<MeshRenderer>();
            foreach (GameObject selected in selection)
            {
                renderers.AddRange(selected.GetComponentsInChildren<MeshRenderer>());
            }
            if (renderers.Count == 0) return false;

            bounds = renderers[0].bounds;
            foreach (MeshRenderer meshRenderer in renderers)
            {
                bounds.Encapsulate(meshRenderer.bounds);
            }

            bounds.size += Vector3.one * padding;

            return true;
        }
    }
}