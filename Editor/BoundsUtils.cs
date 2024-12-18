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
            bool boundsInit = false;

            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0) return false;

            GameObject[] selection = Selection.gameObjects;
            List<MeshRenderer> renderers = new List<MeshRenderer>();
            List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
            List<Terrain> terrains = new List<Terrain>();

            foreach (GameObject selected in selection)
            {
                renderers.AddRange(selected.GetComponentsInChildren<MeshRenderer>());
                skinnedMeshRenderers.AddRange(selected.GetComponentsInChildren<SkinnedMeshRenderer>());
                terrains.AddRange(selected.GetComponentsInChildren<Terrain>());
            }

            if (renderers.Count == 0 && skinnedMeshRenderers.Count == 0 && terrains.Count == 0)
            {
                Debug.LogWarning("No renderers or terrains found.");
                return false;
            }    

            if (renderers.Count > 0)
            {
                bounds = renderers[0].bounds;
                boundsInit = true;
                foreach (MeshRenderer meshRenderer in renderers)
                {
                    bounds.Encapsulate(meshRenderer.bounds);
                }
            }

            if(skinnedMeshRenderers.Count > 0)
            {
                if (!boundsInit) bounds = skinnedMeshRenderers[0].bounds;
                boundsInit = true;
                foreach (SkinnedMeshRenderer smr in skinnedMeshRenderers)
                {
                    bounds.Encapsulate(smr.bounds);
                }
            }

            if(terrains.Count > 0)
            {
                if (!boundsInit) bounds = GetTerrainWorldBoundS(terrains[0]);
                boundsInit = true;
                foreach(Terrain terrain in terrains)
                {
                    bounds.Encapsulate(GetTerrainWorldBoundS(terrain));
                }
            }

            bounds.size += Vector3.one * padding;

            return true;
        }

        private static Bounds GetTerrainWorldBoundS(Terrain terrain)
        {
            Bounds bounds = terrain.terrainData.bounds;
            bounds.center += terrain.transform.position;
            return bounds;
        }
    }
}