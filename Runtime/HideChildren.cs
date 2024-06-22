using UnityEngine;

namespace EditorTools
{
    public class HideChildren : MonoBehaviour
    {
        [SerializeField] private bool _isHidden = true;
        [SerializeField] private HideFlags _hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

        private void OnValidate()
        {
            SetHideState();
        }

        private void SetHideState()
        {
            if (!Application.isEditor) return;
            HideFlags flags = _isHidden ? _hideFlags : HideFlags.None;
            SetChildrenHideFlags(gameObject, flags);
        }

        public static void SetChildrenHideFlags(GameObject gameObject, HideFlags hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector)
        {
            Transform[] tranforms = gameObject.GetComponentsInChildren<Transform>();
            for (int i = 0; i < tranforms.Length; i++)
            {
                if (tranforms[i].gameObject == gameObject) continue;
                tranforms[i].gameObject.hideFlags = hideFlags;
            }
        }
    }
}