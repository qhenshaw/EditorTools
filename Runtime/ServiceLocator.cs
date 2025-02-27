using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EditorTools
{
    public class ServiceLocator : MonoBehaviour
    {
        [field: SerializeField] private bool _logChanges = true;
        [SerializeField] private List<Object> _registerOnAwake = new List<Object>();

        [ShowInInspector] private ServiceRegistry _localServices;
        public ServiceRegistry LocalServices
        {
            get
            {
                if (_localServices == null) _localServices = new ServiceRegistry(_logChanges, false, gameObject);
                return _localServices;
            }
        }

        private static ServiceLocator _globalLocator;
        private static ServiceRegistry _globalServices;
        public static ServiceRegistry GlobalServices
        {
            get
            {
                if (_globalLocator == null)
                {
                    GameObject go = new GameObject("Global Service Locator");
                    ServiceLocator sl = go.AddComponent<ServiceLocator>();
                    _globalLocator = sl;
                    _globalServices = new ServiceRegistry(true, true, go);
                }
                return _globalServices;
            }
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            _globalLocator = null;
            _globalServices = null;
        }

        private void Awake()
        {
            foreach (Object obj in _registerOnAwake)
            {
                LocalServices.Register(obj.GetType(), obj);
            }
        }
    }
}