using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace EditorTools
{
    [RequireComponent(typeof(BoxCollider))]
    public class SceneLoadTrigger : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private string _triggerObjectTag = "Player";
        [SerializeField] private string _sceneToLoad;
        [SerializeField] private string _sceneToUnload;

        [Header("Events")]
        public UnityEvent OnStart;
        public UnityEvent<string> OnLoad;
        public UnityEvent<string> OnUnload;

        private void OnValidate()
        {
            BoxCollider collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(_triggerObjectTag)) return;

            OnStart.Invoke();

            bool nextLoaded = CheckIfSceneLoaded(_sceneToLoad);
            bool previousLoaded = CheckIfSceneLoaded(_sceneToUnload);

            if (!string.IsNullOrEmpty(_sceneToLoad) && !nextLoaded) StartCoroutine(LoadNext(_sceneToLoad));
            if (!string.IsNullOrEmpty(_sceneToUnload) && previousLoaded) StartCoroutine(UnloadPrevious(_sceneToUnload));
        }

        private IEnumerator LoadNext(string sceneName)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!load.isDone)
            {
                yield return null;
            }

            OnLoad.Invoke(sceneName);
        }

        private IEnumerator UnloadPrevious(string sceneName)
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(sceneName);
            while (!unload.isDone)
            {
                yield return null;
            }

            OnUnload.Invoke(sceneName);
        }

        private bool CheckIfSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name.Equals(sceneName)) return true;
            }

            return false;
        }
    }
}