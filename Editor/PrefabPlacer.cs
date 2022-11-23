#if ODIN_INSPECTOR

using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace EditorTools.Editor
{
    public class PrefabPlacer : OdinEditorWindow
    {
        private enum RotationMode
        {
            Random,
            Identity,
            Prefab,
            DragDirection,
            CameraFlattened
        }

        [System.Serializable]
        private class RandomRotationRange
        {
            [MinMaxSlider(-180f, 180f, true), LabelWidth(15)] public Vector2 X = new Vector2(0f, 0f);
            [MinMaxSlider(-180f, 180f, true), LabelWidth(15)] public Vector2 Y = new Vector2(-180f, 180f);
            [MinMaxSlider(-180f, 180f, true), LabelWidth(15)] public Vector2 Z = new Vector2(0f, 0f);
        }

        [System.Serializable]
        private class RandomScaleRange
        {
            [MinMaxSlider(0f, 10f, true), LabelWidth(15)] public Vector2 X = new Vector2(1f, 1f);
            [MinMaxSlider(0f, 10f, true), LabelWidth(15)] public Vector2 Y = new Vector2(1f, 1f);
            [MinMaxSlider(0f, 10f, true), LabelWidth(15)] public Vector2 Z = new Vector2(1f, 1f);
        }

        [BoxGroup("Prefabs"), SerializeField] private string _filter;
        [BoxGroup("Prefabs"), SerializeField, AssetList(CustomFilterMethod = nameof(AssetNameMatch)), InlineEditor] private List<GameObject> _prefabs;
        [BoxGroup("Position"), SerializeField] private Vector3 _positionOffset;
        [BoxGroup("Position"), SerializeField] private float _spreadRadius = 0.5f;
        [BoxGroup("Position"), SerializeField] private AnimationCurve _spreadDistribution = new AnimationCurve(new Keyframe(0f, 1f, 0f, 0f), new Keyframe(1f, 0f, -2f, 2f));
        [BoxGroup("Rotation"), SerializeField] private RotationMode _rotationMode;
        [BoxGroup("Rotation"), ShowIf("_rotationMode", RotationMode.Random), InlineProperty, SerializeField] private RandomRotationRange _randomRotationRange;
        [BoxGroup("Rotation"), SerializeField] private Vector3 _rotationOffset;
        [BoxGroup("Scale"), InlineProperty, SerializeField] private RandomScaleRange _randomScaleRange;
        [BoxGroup("Placement"), SerializeField, SceneObjectsOnly] private Transform _parent;
        [BoxGroup("Placement"), SerializeField] private LayerMask _layerMask = 1 << 0;
        [BoxGroup("Placement"), SerializeField] private bool _staticOnly = false;
        [BoxGroup("Placement"), SerializeField] private float _dragDistance = 2f;

        private bool _controlDown;
        private bool _shiftDown;
        private Vector3 _lastSpawnPosition;
        private int _undoID;

        [MenuItem("Tools/Prefab Placer", false, 2000)]
        private static void OpenWindow()
        {
            GetWindow<PrefabPlacer>().Show();
        }

        protected override void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
        protected override void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

        private bool AssetNameMatch(GameObject go)
        {
            if (string.IsNullOrEmpty(_filter)) return true;
            string[] strings = _filter.ToLower().Replace(" ", "").Split(',');
            string name = go.name.ToLower();
            for (int i = 0; i < strings.Length; i++)
            {
                if (!name.Contains(strings[i])) return false;
            }

            return true;
        }

        [BoxGroup("Prefabs"), Button("Clear")]
        private void ClearPrefabs()
        {
            _prefabs.Clear();
        }

        private void OnSceneGUI(SceneView view)
        {
            if (TestKeyEventType(KeyCode.LeftControl) == EventType.KeyDown) _controlDown = true;
            else if (TestKeyEventType(KeyCode.LeftControl) == EventType.KeyUp) _controlDown = false;

            if (TestKeyEventType(KeyCode.LeftShift) == EventType.KeyDown) _shiftDown = true;
            else if (TestKeyEventType(KeyCode.LeftShift) == EventType.KeyUp) _shiftDown = false;

            if (!_controlDown || !_shiftDown) return;

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));

            if (GetMousePosition(out Vector3 mousePosition))
            {
                Handles.color = Color.white;
                Handles.DrawWireDisc(mousePosition, Vector3.up, _spreadRadius);
                Handles.Label(mousePosition + Vector3.back * _spreadRadius, _spreadRadius.ToString());
                int innerRadiusSteps = 50;
                for (int i = 0; i < innerRadiusSteps; i++)
                {
                    float progress = (float)i / innerRadiusSteps;
                    float value = _spreadDistribution.Evaluate(progress);
                    if (value <= 0.5f)
                    {
                        Handles.DrawWireDisc(mousePosition, Vector3.up, progress * _spreadRadius);
                        break;
                    }
                }

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    Undo.IncrementCurrentGroup();
                    _undoID = Undo.GetCurrentGroup();
                    Spawn(mousePosition);
                    Event.current.Use();
                }

                if (Event.current.type == EventType.MouseDrag && Event.current.button == 0 && Vector3.Distance(_lastSpawnPosition, mousePosition) > _dragDistance)
                {
                    Spawn(mousePosition);
                    Event.current.Use();
                }

                if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    Undo.CollapseUndoOperations(_undoID);
                    Event.current.Use();
                }
            }
        }

        private EventType TestKeyEventType(KeyCode key)
        {
            Event e = Event.current;
            if (e.keyCode != key) return EventType.Ignore;
            return e.type;
        }

        private void Spawn(Vector3 position)
        {
            if (_prefabs == null || _prefabs.Count < 1) return;

            GameObject prefab = _prefabs[Random.Range(0, _prefabs.Count)];
            if (prefab == null) return;

            Undo.IncrementCurrentGroup();

            Quaternion rotation = Quaternion.identity;
            switch (_rotationMode)
            {
                case RotationMode.Random:
                    float x = Random.Range(_randomRotationRange.X.x, _randomRotationRange.X.y);
                    float y = Random.Range(_randomRotationRange.Y.x, _randomRotationRange.Y.y);
                    float z = Random.Range(_randomRotationRange.Z.x, _randomRotationRange.Z.y);
                    rotation = Quaternion.Euler(x, y, z);
                    break;
                case RotationMode.Identity:
                    rotation = Quaternion.identity;
                    break;
                case RotationMode.Prefab:
                    rotation = prefab.transform.rotation;
                    break;
                case RotationMode.DragDirection:
                    Vector3 dragDir = (position - _lastSpawnPosition).normalized;
                    rotation = Quaternion.LookRotation(dragDir);
                    break;
                case RotationMode.CameraFlattened:
                    Camera[] sceneCameras = SceneView.GetAllSceneCameras();
                    Vector3 cameraDir = sceneCameras[0].transform.forward;
                    cameraDir.y = 0f;
                    rotation = Quaternion.LookRotation(cameraDir.normalized);
                    break;
            }
            rotation *= Quaternion.Euler(_rotationOffset);
            GameObject instantiated = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            float distribution = _spreadDistribution.Evaluate(Random.value);
            Vector3 spreadOffset = Vector3.ClampMagnitude(new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)), 1f) * _spreadRadius * distribution;
            instantiated.transform.SetPositionAndRotation(position + _positionOffset + spreadOffset, rotation);
            Vector3 scale = new Vector3(Random.Range(_randomScaleRange.X.x, _randomScaleRange.X.y) * instantiated.transform.localScale.x,
                                        Random.Range(_randomScaleRange.Y.x, _randomScaleRange.Y.y) * instantiated.transform.localScale.y,
                                        Random.Range(_randomScaleRange.Z.x, _randomScaleRange.Z.y)) * instantiated.transform.localScale.z;
            instantiated.transform.localScale = scale;
            instantiated.transform.SetParent(_parent);

            Undo.RegisterCreatedObjectUndo(instantiated, "Place prefab");

            _lastSpawnPosition = position;
        }

        private bool GetMousePosition(out Vector3 position)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMask))
            {
                position = hit.point;
                if (_staticOnly && !GameObjectUtility.AreStaticEditorFlagsSet(hit.transform.gameObject, StaticEditorFlags.BatchingStatic)) return false;
                return true;
            }

            position = Vector3.zero;
            return false;
        }
    }
}

#endif