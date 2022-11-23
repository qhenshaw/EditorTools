using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace EditorTools.Runtime
{
    [RequireComponent(typeof(LightProbeGroup))]
    [RequireComponent(typeof(LightProbeGroup))]
    public class LightProbeVolume : MonoBehaviour
    {
#if UNITY_EDITOR

        [System.Serializable]
        private class ProbePass
        {
            [Range(0.05f, 1f), Tooltip("How many probes will be placed in a cubic meter, high = more probes.")] public float Density = 0.25f;
            [Tooltip("Place the probe a fixed distance away from the surface hit.")] public bool UseOffset;
            [Tooltip("The distance the probe will be placed from the surface.")] public float OffsetDistance = 0.25f;
        }


        [SerializeField, Tooltip("Configuration for each probe pass through the environment.")]
        private ProbePass[] _passes =
        {
            new ProbePass() { Density = 0.5f, OffsetDistance = 0.25f, UseOffset = true },
            new ProbePass() {Density = 0.25f, OffsetDistance = 0f, UseOffset = false }
        };
        [SerializeField, Tooltip("Which layers will be tested.")] private LayerMask _collisionMask = 1;
        [SerializeField, Tooltip("The size and shape of test area.")] private Bounds _bounds = new Bounds(Vector3.zero, new Vector3(20f, 20f, 20f));

        private int _occlusionTestMinVisibility = 6;

        public Bounds Bounds { get => _bounds; set => _bounds = value; }
        public LightProbeGroup LPG => GetComponent<LightProbeGroup>();
        public Vector3[] Positions { get => LPG.probePositions; set => LPG.probePositions = value; }

        private Vector3[] _directions =
        {
            Vector3.down,
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            Vector3.down,
            new Vector3(1f, 1f, 1f),
            new Vector3(1f, 1f, -1f),
            new Vector3(-1f, 1f, -1f),
            new Vector3(-1f, 1f, 1f),
            new Vector3(1f, -1f, 1f),
            new Vector3(1f, -1f, -1f),
            new Vector3(-1f, -1f, -1f),
            new Vector3(-1f, -1f, 1f)
        };

        public void Recalculate()
        {
            if (_passes == null || _passes.Length == 0) return;

            Stopwatch timer = new Stopwatch();
            timer.Start();
            Debug.Log("Light Probe Volume recalculation start...");

            List<Vector3> positions = new List<Vector3>();

            for (int i = 0; i < _passes.Length; i++)
            {
                ProbePass pass = _passes[i];
                float density = pass.Density;
                float distance = 1f / density;
                Vector3 start = transform.position + Bounds.min + Vector3.one * (pass.Density * 0.5f + 0.05f);

                for (int x = 0; x <= Bounds.size.x * density; x++)
                {
                    for (int y = 0; y <= Bounds.size.y * density; y++)
                    {
                        for (int z = 0; z <= Bounds.size.z * density; z++)
                        {
                            Vector3 position = start + new Vector3(x / density, y / density, z / density);
                            if (!TestVisible(position, distance, _occlusionTestMinVisibility)) continue;
                            if (Physics.OverlapSphere(position, distance, _collisionMask).Length >= 1)
                            {
                                if (pass.UseOffset && GetSurfaceOffsetPosition(position, distance, pass.OffsetDistance, out Vector3 surfacePosition)) positions.Add(surfacePosition);
                                else positions.Add(position);
                            }
                        }
                    }
                }
            }

            // TODO: add surface optimizations

            Positions = positions.ToArray();

            timer.Stop();
            Debug.Log($"... completed in {timer.Elapsed.TotalMilliseconds}");
        }

        private bool TestVisible(Vector3 position, float distance, int minVisibility = 0)
        {
            int visibleCount = 0;
            foreach (Vector3 dir in _directions)
            {
                Vector3 start = position + dir * distance;
                if (!Physics.Linecast(start, position, _collisionMask)) visibleCount++;
            }

            return visibleCount > minVisibility;
        }

        private bool GetSurfaceOffsetPosition(Vector3 position, float distance, float offset, out Vector3 offsetPosition)
        {
            foreach (Vector3 dir in _directions)
            {
                if (Physics.Raycast(position, dir, out RaycastHit hit, distance, _collisionMask))
                {
                    offsetPosition = hit.point + hit.normal * offset;
                    return true;
                }
            }

            offsetPosition = position;
            return false;
        }

#endif
    }
}