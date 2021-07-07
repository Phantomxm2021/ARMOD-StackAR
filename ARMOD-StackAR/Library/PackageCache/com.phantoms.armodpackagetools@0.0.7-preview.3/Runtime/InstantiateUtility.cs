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

        public static GameObject Instantiate(GameObject _prefab, string _uniqueName, Vector3 _position,
            Quaternion _quaternion)
        {
            var tmp_Instantiate = _prefab.Instantiate("", _position, _quaternion);
            tmp_Instantiate.name = tmp_Instantiate.name.Replace("(Clone)", "");
            //StoreInstantiatedObject(tmp_Instantiate);

            var tmp_Transform = tmp_Instantiate.GetComponentsInChildren<Transform>(true);
            for (int tmp_ChildIndex = 0; tmp_ChildIndex < tmp_Transform.Length; tmp_ChildIndex++)
            {
                StoreInstantiatedObject(tmp_Transform[tmp_ChildIndex]);
            }


            return tmp_Instantiate;
        }


        public static GameObject Instantiate(GameObject _prefab, string _uniqueName, Transform _parent)
        {
            var tmp_Instantiate = _prefab.Instantiate("", _parent);
            tmp_Instantiate.name = tmp_Instantiate.name.Replace("(Clone)", "");
            //StoreInstantiatedObject(tmp_Instantiate);
            var tmp_Transform = tmp_Instantiate.GetComponentsInChildren<Transform>(true);
            for (int tmp_ChildIndex = 0; tmp_ChildIndex < tmp_Transform.Length; tmp_ChildIndex++)
            {
                StoreInstantiatedObject(tmp_Transform[tmp_ChildIndex]);
            }


            return tmp_Instantiate;
        }


        public static void StoreInstantiatedObject<T>(T _object) where T : Object
        {
            if (STORE_POOL.ContainsKey(_object.name))
            {
                Debug.LogError($"{_object.name} is already!");
                _object.name += _object.GetHashCode();
//                return;
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

    public static class Test
    {
        public static int WordCount(this String str)
        {
            return str.Split(new char[] {' ', '.', '?'}, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static GameObject Instantiate(this GameObject _prefab, string _uniqueName, Vector3 _position,
            Quaternion _quaternion)
        {
            GameObject tmp_Go = Object.Instantiate(_prefab, _position, _quaternion);
//            for (int tmp_ChildIndex = 0; tmp_ChildIndex < tmp_Go.transform.childCount; tmp_ChildIndex++)
//            {
//                tmp_Go.transform.GetChild(tmp_ChildIndex).name += $"_{_uniqueName}";
//            }
//
//            tmp_Go.name += $"_{_uniqueName}";
            return tmp_Go;
        }

        public static GameObject Instantiate(this GameObject _prefab, string _uniqueName, Transform _parent)
        {
            GameObject tmp_Go = Object.Instantiate(_prefab, _parent);
//            for (int tmp_ChildIndex = 0; tmp_ChildIndex < tmp_Go.transform.childCount; tmp_ChildIndex++)
//            {
//                tmp_Go.transform.GetChild(tmp_ChildIndex).name += $"_{_uniqueName}";
//            }
//
//            tmp_Go.name += $"_{_uniqueName}";
            return tmp_Go;
        }
    }
}