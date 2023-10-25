using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorTools.Editor
{
    public class LDHotkeys : MonoBehaviour
    {
        [MenuItem("Tools/LD Shortcuts/Snap to Floor _End")]
        private static void SnapToFloor()
        {
            if (Selection.activeGameObject == null) return;

            GameObject selected = Selection.activeGameObject;
            Bounds combinedBounds = new Bounds();
            if(selected.TryGetComponent(out Renderer renderer)) combinedBounds = renderer.bounds;
            Renderer[] renderers = selected.GetComponentsInChildren<Renderer>();
            foreach (Renderer child in renderers)
            {
                if(combinedBounds.size.x <= 0f)
                {
                    combinedBounds = child.bounds;
                }
                else
                {
                    combinedBounds.Encapsulate(child.bounds);
                }
            }

            float lowestPoint = combinedBounds.min.y;
            Vector3 center = combinedBounds.center;
            Vector3 lowestCenter = new Vector3(center.x, lowestPoint, center.z);
            if (combinedBounds.size.x <= 0f) lowestCenter = selected.transform.position;

            Ray ray = new Ray(lowestCenter, Vector3.down);
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
            float shortest = Mathf.Infinity;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform.IsChildOf(selected.transform)) continue;

                float diff = lowestCenter.y - hit.point.y;
                if(diff < shortest) shortest = diff;
                
            }

            if(shortest < Mathf.Infinity)
            {
                Undo.RegisterFullObjectHierarchyUndo(selected, "Snap to Floor");
                selected.transform.position += Vector3.down * shortest;
            }
        }
    }
}