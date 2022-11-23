using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorTools.Editor
{
    public class ModelCollisionImporter : AssetPostprocessor
    {
        private static string _collisionPrefix = "ucx";

        private void OnPostprocessModel(GameObject gameObject)
        {
            Transform[] children = gameObject.GetComponentsInChildren<Transform>();
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                MeshFilter meshFilter = child.GetComponent<MeshFilter>();
                if (meshFilter == null) continue;
                if (!child.name.ToLower().Contains(_collisionPrefix)) continue;
                Mesh mesh = meshFilter.sharedMesh;
                if (mesh == null) continue;
                MeshCollider collider = child.GetComponent<MeshCollider>();
                if (collider == null) collider = child.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh;
                if (meshFilter != null) Object.DestroyImmediate(meshFilter);
                MeshRenderer meshRenderer = collider.GetComponent<MeshRenderer>();
                if (meshRenderer != null) Object.DestroyImmediate(meshRenderer);
            }
        }
    }
}