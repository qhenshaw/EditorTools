using EditorTools.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace EditorTools.Editor
{
    [CustomEditor(typeof(LightProbeVolume))]
    public class LightProbeVolumeEditor : UnityEditor.Editor
    {
        private BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        [MenuItem("GameObject/Light/Light Probe Volume", false, 99)]
        private static void Create(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Light Probe Volume");
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            go.AddComponent<LightProbeVolume>();
            Selection.activeGameObject = go;
        }

        private void OnSceneGUI()
        {
            LightProbeVolume lightProbeVolume = (LightProbeVolume)target;

            // copy the target object's data to the handle
            m_BoundsHandle.center = lightProbeVolume.Bounds.center;
            m_BoundsHandle.size = lightProbeVolume.Bounds.size;

            // draw the handle
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                // record the target object before setting new values so changes can be undone/redone
                Undo.RecordObject(lightProbeVolume, "Change Bounds");

                // copy the handle's updated data back to the target object
                Bounds newBounds = new Bounds();
                newBounds.center = m_BoundsHandle.center;
                newBounds.size = m_BoundsHandle.size;
                lightProbeVolume.Bounds = newBounds;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Recalculate"))
            {
                LightProbeVolume lightProbeVolume = (LightProbeVolume)target;
                lightProbeVolume.Recalculate();
            }
        }
    }
}