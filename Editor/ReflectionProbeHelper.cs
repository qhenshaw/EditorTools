using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace EditorTools.Editor
{
    public class ReflectionProbeHelper
    {
        [MenuItem("GameObject/Light/Surround with Reflection Probe", false, priority = -100)]
        static void SurroundWithReflectionProbe(MenuCommand menuCommand)
        {
            if(BoundsUtils.TryGetSelectionBounds(out Bounds bounds, 2f))
            {
                Selection.objects = null;
                GameObject gameObject = new GameObject("Reflection Probe");
                Undo.RegisterCreatedObjectUndo(gameObject, "Create" + gameObject.name);
                ReflectionProbe volume = Undo.AddComponent(gameObject, typeof(ReflectionProbe)) as ReflectionProbe;
                volume.transform.position = bounds.center;
                volume.size = bounds.size;
            }
        }

        [MenuItem("GameObject/Light/Surround with Probe Volume", false, priority = -99)]
        static void SurroundWithProbeVolume(MenuCommand menuCommand)
        {
            if (BoundsUtils.TryGetSelectionBounds(out Bounds bounds, 2f))
            {
                Selection.objects = null;
                GameObject gameObject = new GameObject("Probe Volume");
                Undo.RegisterCreatedObjectUndo(gameObject, "Create" + gameObject.name);
                ProbeVolume volume = Undo.AddComponent(gameObject, typeof(ProbeVolume)) as ProbeVolume;
                volume.transform.position = bounds.center;
                volume.size = bounds.size;
            }
        }

        [MenuItem("GameObject/Light/Surround with Local Volume", false, priority = -98)]
        static void SurroundWithLocalVolume(MenuCommand menuCommand)
        {
            if (BoundsUtils.TryGetSelectionBounds(out Bounds bounds, 1f))
            {
                Selection.objects = null;
                GameObject gameObject = new GameObject("Local Volume");
                Undo.RegisterCreatedObjectUndo(gameObject, "Create" + gameObject.name);
                BoxCollider box = Undo.AddComponent(gameObject, typeof(BoxCollider)) as BoxCollider;
                Volume volume = Undo.AddComponent(gameObject, typeof(Volume)) as Volume;
                box.isTrigger = true;
                volume.isGlobal = false;
                volume.blendDistance = 1f;
                box.transform.position = bounds.center;
                box.size = bounds.size;
            }
        }
    }
}