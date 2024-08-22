using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace EditorTools
{
    public class IncrementalChildEnable : MonoBehaviour
    {
        [SerializeField] private bool _enableOnStart = true;
        [SerializeField] private int _simultaneousEnableCount = 1;

        private void Start()
        {
            if (!_enableOnStart) return;
            StartCoroutine(EnableChildrenIncremental());
        }

        private IEnumerator EnableChildrenIncremental()
        {
            int count = transform.childCount;
            for (int i = 0; i < count;)
            {
                for (int j = 0; j < _simultaneousEnableCount; j++)
                {
                    if (i >= count) yield break;
                    transform.GetChild(i++).gameObject.SetActive(true);
                }
                yield return null;
            }
        }

#if ODIN_INSPECTOR
        [Button("Enable Children")]
#endif
        [ContextMenu("Enable Children")]
        public void EnableChildren()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }

#if ODIN_INSPECTOR
        [Button("Disable Children")]
#endif
        [ContextMenu("Disable Children")]
        public void DisableChildren()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}