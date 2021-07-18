using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.Phantoms.ARMODPackageTools.Runtime
{
    public static class InstantiateUtility
    {
        private static readonly IDictionary<string, Object> STORE_POOL = new Dictionary<string, Object>();

        [Obsolete("", true)]
        public static GameObject Instantiate(GameObject _prefab, string _uniqueName, Vector3 _position,
            Quaternion _quaternion)
        {
            var tmp_Instantiate = Object.Instantiate(_prefab);
            tmp_Instantiate.name = tmp_Instantiate.name.Replace("(Clone)", "");
            //StoreInstantiatedObject(tmp_Instantiate);

            var tmp_Transform = tmp_Instantiate.GetComponentsInChildren<Transform>(true);
            for (int tmp_ChildIndex = 0; tmp_ChildIndex < tmp_Transform.Length; tmp_ChildIndex++)
            {
                StoreInstantiatedObject(tmp_Transform[tmp_ChildIndex]);
            }

            return tmp_Instantiate;
        }

        [Obsolete("", true)]
        public static GameObject Instantiate(GameObject _prefab, string _uniqueName, Transform _parent)
        {
            var tmp_Instantiate = Object.Instantiate(_prefab, _parent);
            tmp_Instantiate.name = tmp_Instantiate.name.Replace("(Clone)", "");
            //StoreInstantiatedObject(tmp_Instantiate);
            var tmp_Transform = tmp_Instantiate.GetComponentsInChildren<Transform>(true);
            for (int tmp_ChildIndex = 0; tmp_ChildIndex < tmp_Transform.Length; tmp_ChildIndex++)
            {
                StoreInstantiatedObject(tmp_Transform[tmp_ChildIndex]);
            }


            return tmp_Instantiate;
        }


        public static void StoreGameObject(GameObject _gameObject)
        {
            var tmp_Transform = _gameObject.GetComponentsInChildren<Transform>(true);
            for (int tmp_ChildIndex = 0; tmp_ChildIndex < tmp_Transform.Length; tmp_ChildIndex++)
            {
                StoreInstantiatedObject(tmp_Transform[tmp_ChildIndex]);
            }
        }


        private static void StoreInstantiatedObject<T>(T _object) where T : Object
        {
            if (STORE_POOL.ContainsKey(_object.name))
            {
                Debug.LogError($"{_object.name} is already!");
                _object.name += _object.GetHashCode();
            }


            STORE_POOL.Add(_object.name, _object);
        }

        public static void CleanPool()
        {
            var tmp_ConvertedListPool = STORE_POOL.Values.ToList();
            foreach (var tmp_Element in tmp_ConvertedListPool)
            {
                if (null == tmp_Element)
                {
                    continue;
                }

                if (tmp_Element is Transform tmp_Transform)
                {
                    Object.Destroy(tmp_Transform.gameObject);
                }

                Object.Destroy(tmp_Element);
            }

            STORE_POOL.Clear();
        }


        public static Object FindByName(string _name)
        {
            return STORE_POOL.TryGetValue(_name, out Object tmp_Value) ? tmp_Value : null;
        }
    }
}