#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor.SceneManagement;
#endif

namespace EditorTools.Editor
{
    [ExecuteAlways]
    public class PrefabPlacer : OdinEditorWindow
    {
        private enum RotationMode
        {
            Random,
            Identity,
            Prefab,
            DragDirection,
            CameraFlattened,
            SurfaceNormal
        }

        private enum CoordinateSpace
        {
            World,
            Local
        }

        private enum ScaleMode
        {
            Uniform,
            Individual
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
        [BoxGroup("Position"), SerializeField] private CoordinateSpace _positionOffsetSpace;
        [BoxGroup("Position"), SerializeField] private float _spreadRadius = 0.5f;
        [BoxGroup("Position"), SerializeField] private AnimationCurve _spreadDistribution = new AnimationCurve(new Keyframe(0f, 1f, 0f, 0f), new Keyframe(1f, 0f, -2f, 2f));
        [BoxGroup("Rotation"), SerializeField] private RotationMode _rotationMode;
        [BoxGroup("Rotation"), ShowIf("ShowRandomRotation"), InlineProperty, SerializeField] private RandomRotationRange _randomRotationRange;
        [BoxGroup("Rotation"), SerializeField] private Vector3 _rotationOffset;
        [BoxGroup("Scale"), SerializeField] private ScaleMode _scaleMode = ScaleMode.Uniform;
        [BoxGroup("Scale"), InlineProperty, SerializeField, HideIf("IsUniformScale")] private RandomScaleRange _randomScaleRange;
        [BoxGroup("Scale"), InlineProperty, SerializeField, MinMaxSlider(0f, 10f, true), ShowIf("IsUniformScale")] private Vector2 _uniformRandomScaleRange = new Vector2(1f, 1f);
        [BoxGroup("Placement"), SerializeField, SceneObjectsOnly] private Transform _parent;
        [BoxGroup("Placement"), SerializeField] private LayerMask _layerMask = 1 << 0;
        [BoxGroup("Placement"), SerializeField] private bool _staticOnly = false;
        [BoxGroup("Placement"), SerializeField] private float _dragDistance = 2f;
        [BoxGroup("Placement"), SerializeField] private int _spawnCount = 1;
        [BoxGroup("Physics (Experimental)")] private bool _simulateSpawned;

        private bool IsUniformScale => _scaleMode == ScaleMode.Uniform;
        private bool ShowRandomRotation => _rotationMode == RotationMode.Random || _rotationMode == RotationMode.SurfaceNormal;
        private bool _controlDown;
        private bool _shiftDown;
        private Vector3 _lastSpawnPosition;
        private int _undoID;
        private bool _simulating;

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

            if (GetMousePosition(out Vector3 mousePosition, out Vector3 surfaceNormal, out Vector3 surfaceTangent))
            {
                Handles.color = Color.white;
                Handles.DrawWireDisc(mousePosition, surfaceNormal, _spreadRadius);
                Handles.Label(mousePosition + Vector3.back * _spreadRadius, _spreadRadius.ToString());
                int innerRadiusSteps = 50;
                for (int i = 0; i < innerRadiusSteps; i++)
                {
                    float progress = (float)i / innerRadiusSteps;
                    float value = _spreadDistribution.Evaluate(progress);
                    if (value <= 0.5f)
                    {
                        Handles.DrawWireDisc(mousePosition, surfaceNormal, progress * _spreadRadius);
                        break;
                    }
                }

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    Undo.IncrementCurrentGroup();
                    _undoID = Undo.GetCurrentGroup();
                    for (int i = 0; i < _spawnCount; i++)
                    {
                        Spawn(mousePosition, surfaceNormal, surfaceTangent);

                    }
                    Event.current.Use();
                }

                if (Event.current.type == EventType.MouseDrag && Event.current.button == 0 && Vector3.Distance(_lastSpawnPosition, mousePosition) > _dragDistance)
                {
                    for (int i = 0; i < _spawnCount; i++)
                    {
                        Spawn(mousePosition, surfaceNormal, surfaceTangent);
                    }
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
            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(position);
            Ray ray = HandleUtility.GUIPointToWorldRay(guiPoint);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMask))
            {
                Vector3 hitPosition = hit.point;
                Vector3 normal = hit.normal;
                Vector3 tangent = Vector3.Cross(normal, ray.direction);
                if (!_staticOnly || (_staticOnly && !GameObjectUtility.AreStaticEditorFlagsSet(hit.transform.gameObject, StaticEditorFlags.BatchingStatic)))
                {
                    Spawn(hitPosition, normal, tangent);
                }
            }
        }

        private void Spawn(Vector3 centerPosition, Vector3 normal, Vector3 tangent)
        {
            if (_prefabs == null || _prefabs.Count < 1) return;

            GameObject prefab = _prefabs[Random.Range(0, _prefabs.Count)];
            if (prefab == null) return;

            Undo.IncrementCurrentGroup();

            Vector3 forward = -Vector3.Cross(normal, tangent);
            Vector3 right = tangent;
            Vector3 up = normal;

            float distribution = _spreadDistribution.Evaluate(Random.value);
            Vector3 offset = _positionOffset;
            Vector3 spreadOffset = Vector3.ClampMagnitude(new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)), 1f) * _spreadRadius * distribution;
            spreadOffset = forward * spreadOffset.x + right * spreadOffset.z;
            if (_positionOffsetSpace == CoordinateSpace.Local)
            {
                offset = forward * _positionOffset.z + right * _positionOffset.x + up * _positionOffset.y;
            }
            Vector3 spawnPosition = centerPosition + spreadOffset;
            if (GetPosition(spawnPosition, out Vector3 pos, out Vector3 norm, out Vector3 tang))
            {
                spawnPosition = pos;
                normal = norm;
                tangent = tang;
            }

            Quaternion rotation = Quaternion.identity;
            float x = Random.Range(_randomRotationRange.X.x, _randomRotationRange.X.y);
            float y = Random.Range(_randomRotationRange.Y.x, _randomRotationRange.Y.y);
            float z = Random.Range(_randomRotationRange.Z.x, _randomRotationRange.Z.y);
            switch (_rotationMode)
            {
                case RotationMode.Random:
                    rotation = Quaternion.Euler(x, y, z);
                    break;
                case RotationMode.Identity:
                    rotation = Quaternion.identity;
                    break;
                case RotationMode.Prefab:
                    rotation = prefab.transform.rotation;
                    break;
                case RotationMode.DragDirection:
                    Vector3 dragDir = (centerPosition - _lastSpawnPosition).normalized;
                    rotation = Quaternion.LookRotation(dragDir);
                    break;
                case RotationMode.CameraFlattened:
                    Camera[] sceneCameras = SceneView.GetAllSceneCameras();
                    Vector3 cameraDir = sceneCameras[0].transform.forward;
                    cameraDir.y = 0f;
                    rotation = Quaternion.LookRotation(cameraDir.normalized);
                    break;
                case RotationMode.SurfaceNormal:
                    rotation = Quaternion.LookRotation(tangent, normal);
                    rotation *= Quaternion.Euler(x, y, z);
                    break;
            }
            rotation *= Quaternion.Euler(_rotationOffset);

            Vector3 scale = Vector3.one;
            switch (_scaleMode)
            {
                case ScaleMode.Uniform:
                    scale = Vector3.one * Random.Range(_uniformRandomScaleRange.x, _uniformRandomScaleRange.y);
                    break;
                case ScaleMode.Individual:
                    scale = new Vector3(Random.Range(_randomScaleRange.X.x, _randomScaleRange.X.y) * prefab.transform.localScale.x,
                                        Random.Range(_randomScaleRange.Y.x, _randomScaleRange.Y.y) * prefab.transform.localScale.y,
                                        Random.Range(_randomScaleRange.Z.x, _randomScaleRange.Z.y) * prefab.transform.localScale.z);
                    break;
            }

            GameObject instantiated = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instantiated.transform.position = spawnPosition + offset;
            instantiated.transform.rotation = rotation;
            instantiated.transform.localScale = scale;
            instantiated.transform.SetParent(_parent);

            Undo.RegisterCreatedObjectUndo(instantiated, "Place prefab");

            _lastSpawnPosition = centerPosition;
        }

        private bool GetMousePosition(out Vector3 position, out Vector3 normal, out Vector3 tangent)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMask))
            {
                position = hit.point;
                normal = hit.normal;
                tangent = Vector3.Cross(normal, ray.direction);
                if (_staticOnly && !GameObjectUtility.AreStaticEditorFlagsSet(hit.transform.gameObject, StaticEditorFlags.BatchingStatic)) return false;
                return true;
            }

            position = Vector3.zero;
            normal = Vector3.up;
            tangent = Vector3.right;
            return false;
        }

        private bool GetPosition(Vector3 worldPos, out Vector3 position, out Vector3 normal, out Vector3 tangent)
        {
            Vector2 guiPosition = HandleUtility.WorldToGUIPointWithDepth(worldPos);
            Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMask))
            {
                position = hit.point;
                normal = hit.normal;
                tangent = Vector3.Cross(normal, ray.direction);
                if (_staticOnly && !GameObjectUtility.AreStaticEditorFlagsSet(hit.transform.gameObject, StaticEditorFlags.BatchingStatic)) return false;
                return true;
            }

            position = Vector3.zero;
            normal = Vector3.up;
            tangent = Vector3.right;
            return false;
        }

#if UNITY_EDITOR

        private struct EditorPhysicsState
        {
            public Rigidbody Rigidbody;
            public bool IsKinematic;
        }

        private List<EditorPhysicsState> _editorPhysicsStates = new List<EditorPhysicsState>();

        [HideIf("_simulating")]
        [BoxGroup("Physics (Experimental)")]
        [Button("Simulate Target"), GUIColor(0.5f, 1f, 0.5f)]
        private void SimulateTarget()
        {
            List<Rigidbody> toSimulate = new List<Rigidbody>();
            foreach (GameObject gameObject in Selection.gameObjects)
            {
                Rigidbody[] childrenRB = gameObject.GetComponentsInChildren<Rigidbody>();
                toSimulate.AddRange(childrenRB);
            }
            List<Rigidbody> allRBs = FindObjectsOfType<Rigidbody>().ToList();
            foreach (Rigidbody rb in toSimulate)
            {
                allRBs.Remove(rb);
            }
            foreach (Rigidbody rigidbody in allRBs)
            {
                _editorPhysicsStates.Add(new EditorPhysicsState { Rigidbody = rigidbody, IsKinematic = rigidbody.isKinematic });
                rigidbody.isKinematic = true;
            }

            Physics.simulationMode = SimulationMode.Script;
            _simulating = true;
        }

        [ShowIf("_simulating")]
        [BoxGroup("Physics (Experimental)")]
        [Button("Stop Simulation"), GUIColor(1f, 0.5f, 0.5f)]
        private void StopSimulation()
        {
            foreach(EditorPhysicsState eps in _editorPhysicsStates)
            {
                if(eps.Rigidbody.isKinematic != eps.IsKinematic) eps.Rigidbody.isKinematic = eps.IsKinematic;
            }

            _editorPhysicsStates.Clear();
            _simulating = false;
            Physics.simulationMode = SimulationMode.FixedUpdate;
        }

        private void Update()
        {
            if (_simulating)
            {
                Physics.Simulate(Time.deltaTime);
            }
        }
#endif
    }
}

#endif