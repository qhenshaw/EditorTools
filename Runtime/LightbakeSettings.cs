using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EditorTools
{
    public class LightbakeSettings : MonoBehaviour
    {
#if UNITY_EDITOR
        private enum LightBakePreset
        {
            FullStatic,
            StaticNoShadows,
            FullDynamic,
            DynamicNoShadows,
            DynamicBakeReflections
        }

        [SerializeField, InlineButton(nameof(ApplyPreset), "Apply")] private LightBakePreset _preset;
        [SerializeField]
        private StaticEditorFlags _flags = StaticEditorFlags.ContributeGI |
                                           StaticEditorFlags.BatchingStatic |
                                           StaticEditorFlags.ReflectionProbeStatic |
                                           StaticEditorFlags.OccluderStatic |
                                           StaticEditorFlags.OccludeeStatic;
        [SerializeField] private ReceiveGI _bakeMode = ReceiveGI.LightProbes;
        [SerializeField] private ShadowCastingMode _shadowCastingMode = ShadowCastingMode.On;
        [SerializeField] private bool _staticShadowCaster = true;

        private void ApplyPreset()
        {
            switch (_preset)
            {
                case LightBakePreset.FullStatic:
                    _flags = StaticEditorFlags.ContributeGI |
                             StaticEditorFlags.BatchingStatic |
                             StaticEditorFlags.ReflectionProbeStatic |
                             StaticEditorFlags.OccluderStatic |
                             StaticEditorFlags.OccludeeStatic;
                    _bakeMode = ReceiveGI.LightProbes;
                    _shadowCastingMode = ShadowCastingMode.On;
                    _staticShadowCaster = true;
                    break;
                case LightBakePreset.FullDynamic:
                    _flags = 0;
                    _bakeMode = ReceiveGI.LightProbes;
                    _shadowCastingMode = ShadowCastingMode.On;
                    _staticShadowCaster = false;
                    break;
                case LightBakePreset.StaticNoShadows:
                    _flags = StaticEditorFlags.ContributeGI |
                             StaticEditorFlags.BatchingStatic |
                             StaticEditorFlags.ReflectionProbeStatic |
                             StaticEditorFlags.OccluderStatic |
                             StaticEditorFlags.OccludeeStatic;
                    _bakeMode = ReceiveGI.LightProbes;
                    _shadowCastingMode = ShadowCastingMode.Off;
                    _staticShadowCaster = false;
                    break;
                case LightBakePreset.DynamicNoShadows:
                    _flags = 0;
                    _bakeMode = ReceiveGI.LightProbes;
                    _shadowCastingMode = ShadowCastingMode.Off;
                    _staticShadowCaster = false;
                    break;
                case LightBakePreset.DynamicBakeReflections:
                    _flags = StaticEditorFlags.ReflectionProbeStatic;
                    _bakeMode = ReceiveGI.LightProbes;
                    _shadowCastingMode = ShadowCastingMode.On;
                    _staticShadowCaster = false;
                    break;
            }

            ApplyCurrentSettings();
        }

        [Button]
        public void ApplyCurrentSettings()
        {
            Debug.Log($"Lighting settings applied under object: {gameObject.name}", gameObject);
            Transform[] transforms = GetComponentsInChildren<Transform>();
            foreach (Transform t in transforms)
            {
                if (t.TryGetComponent(out MeshRenderer renderer))
                {
                    t.gameObject.isStatic = true;
                    GameObjectUtility.SetStaticEditorFlags(t.gameObject, _flags);
                    renderer.receiveGI = _bakeMode;
                    renderer.shadowCastingMode = _shadowCastingMode;
                    renderer.staticShadowCaster = _staticShadowCaster;
                }
                else
                {
                    t.gameObject.isStatic = false;
                }
            }

            LightbakeSettings[] childSettings = GetComponentsInChildren<LightbakeSettings>();
            foreach (LightbakeSettings child in childSettings)
            {
                if (child == this) continue;
                child.ApplyCurrentSettings();
            }
        }
#endif
    }
}