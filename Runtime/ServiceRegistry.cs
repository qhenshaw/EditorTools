using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EditorTools
{
    [Serializable]
    public class ServiceRegistry
    {
        private bool _logChanges;
        private string _logPrefix;
        private Object _owner;
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();
        [ShowInInspector] public IEnumerable<object> ServicesList => _services.Values;

        public ServiceRegistry(bool logChanges, bool isGlobal, Object owner)
        {
            _logChanges = logChanges;
            _logPrefix = isGlobal ? "Global" : "Local";
            _owner = owner;
            _services = new Dictionary<Type, object>();
        }

        public ServiceRegistry Register<T>(T service)
        {
            Type type = typeof(T);
            Register(type, service);
            return this;
        }

        public ServiceRegistry Register(Type type, object service)
        {
            if (_services.ContainsKey(type))
            {
                Debug.LogError($"{_owner.name}[{_logPrefix}]: Service already registered: {type.Name}", _owner);
                return this;
            }
            if (_logChanges) Debug.Log($"{_owner.name}[{_logPrefix}]: Service registered: {type.Name}", _owner);
            _services.Add(type, service);

            return this;
        }

        public ServiceRegistry UnRegister<T>(T service)
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
                return this;
            }
            else
            {
                Debug.Log($"{_owner.name}[{_logPrefix}]: Service wasn't registered: {type.Name}", _owner);
                return this;
            }
        }

        public T Get<T>() where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object service)) return service as T;
            Debug.LogError($"{_owner.name}[{_logPrefix}]: Service not registered: {type.Name}", _owner);
            return null;
        }

        public bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object serviceObject))
            {
                service = (T)serviceObject;
                return true;
            }
            Debug.LogError($"{_owner.name}[{_logPrefix}]: Service not registered: {type.Name}", _owner);
            service = null;
            return false;
        }
    }
}