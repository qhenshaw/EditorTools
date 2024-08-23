using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace EditorTools
{
    public class AutoMonoBehaviour : MonoBehaviour
    {
        protected virtual void OnValidate()
        {
            AutoAssign();
        }

        [ContextMenu("Auto-Assign")]
        protected void AutoAssign()
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type attributeType = typeof(AutoAssignAttribute);
            var fields = GetType().GetFields(flags).Where(x => x.GetCustomAttributes(attributeType, true).Any());

            foreach (var field in fields)
            {
                bool isArray = field.FieldType.IsArray;
                bool isList = field.FieldType.IsGenericType;
                if (field.GetValue(this) != null) continue;
                Debug.Log($"Auto-Assigning field: {field}", this);
                Type type = field.FieldType;
                AutoAssignAttribute attribute = field.GetCustomAttribute(attributeType, true) as AutoAssignAttribute;
                AutoAssignAttribute.AutoAssignMode mode = attribute.Mode;

                if (isArray)
                {
                    Type arrayType = type.GetElementType();
                    Array array = FindAutoComponentArray(arrayType, mode);
                    field.SetValue(this, array);

                }
                else if (isList)
                {
                    Type listType = type.GetGenericArguments()[0];
                    Array array = FindAutoComponentArray(listType, mode);
                    var constructedListType = typeof(List<>).MakeGenericType(listType);
                    var list = (IList)Activator.CreateInstance(constructedListType);
                    for (int i = 0; i < array.Length; i++)
                    {
                        list.Add(array.GetValue(i));
                    }
                    field.SetValue(this, list);
                }
                else
                {
                    field.SetValue(this, FindAutoComponent(type, mode));
                }
            }
        }

        private Component FindAutoComponent(Type type, AutoAssignAttribute.AutoAssignMode mode)
        {
            Component result = null;
            switch (mode)
            {
                case AutoAssignAttribute.AutoAssignMode.Same:
                    result = gameObject.GetComponent(type);
                    break;
                case AutoAssignAttribute.AutoAssignMode.Child:
                    result = gameObject.GetComponentInChildren(type);
                    break;
                case AutoAssignAttribute.AutoAssignMode.Parent:
                    result = gameObject.GetComponentInParent(type);
                    break;
                default:
                    break;
            }

            return result;
        }

        private Array FindAutoComponentArray(Type type, AutoAssignAttribute.AutoAssignMode mode)
        {
            Array array = null;
            switch (mode)
            {
                case AutoAssignAttribute.AutoAssignMode.Same:
                    array = gameObject.GetComponents(type);
                    break;
                case AutoAssignAttribute.AutoAssignMode.Child:
                    array = gameObject.GetComponentsInChildren(type);
                    break;
                case AutoAssignAttribute.AutoAssignMode.Parent:
                    array = gameObject.GetComponentsInParent(type);
                    break;
                default:
                    break;
            }

            Array convertedArray = Array.CreateInstance(type, array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                convertedArray.SetValue(array.GetValue(i), i);
            }
            return convertedArray;
        }
    }
}