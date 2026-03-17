using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Tools
{
    public static class GenericObjectPool<T> where T : Component
    {
        private class PoolData
        {
            public ObjectPool<T> Pool;
            public Transform Root;
        }


        //one pool per prefab (key = gameobjectPrefab.GetInstanceID())
        private static readonly Dictionary<int, PoolData> _pools = new();

        //Map live instances to their owning pool to enable Generic Release()
        private static readonly Dictionary<T, ObjectPool<T>> _owners = new();

        public static T Get(
            T prefab,
            Transform parentOverride = null,
            int defaultCapacity = 16,
            int maxSize = 256,
            bool collectionCheck = true,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null
        )
        {
            var pd = GetOrCreatePool(prefab, defaultCapacity, maxSize, collectionCheck, onGet, onRelease, onDestroy,
                parentOverride);
            var obj = pd.Pool.Get();

            if (parentOverride && obj.transform.parent != parentOverride)
                obj.transform.SetParent(parentOverride, false);

            return obj;
        }

        public static void Release(T instance)
        {
            if (!instance) return;

            if (_owners.TryGetValue(instance, out var pool))
            {
                pool.Release(instance);
            }
            else
            {
                Debug.Log($"[{nameof(GenericObjectPool<T>)}] Instance not tracked; destroying.", instance);
                UnityEngine.Object.Destroy(instance.gameObject);
            }
        }

        public static void Prewarm(
            T prefab,
            int count,
            Transform parent = null,
            int defaultCapacity = 16,
            int maxSize = 256,
            bool collectionCheck = true,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null
        )
        {
            var pd = GetOrCreatePool(prefab, defaultCapacity, maxSize, collectionCheck, onGet, onRelease, onDestroy,
                parent);
            // Use ListPool to avoid GC
            var tmp = ListPool<T>.Get();
            try
            {
                tmp.Capacity = Mathf.Max(tmp.Capacity, count);

                for (int i = 0; i < count; i++)
                    tmp.Add(pd.Pool.Get()); // forces creation until we have 'count' actives

                for (int i = 0; i < tmp.Count; i++)
                    pd.Pool.Release(tmp[i]); // now we have 'count' inactives in the pool
            }
            finally
            {
                tmp.Clear();
                ListPool<T>.Release(tmp);
            }
        }

        public static void Clear(T prefab)
        {
            var key = prefab.GetInstanceID();
            if (_pools.TryGetValue(key, out var pd))
            {
                pd.Pool.Clear();
                if (pd.Root) UnityEngine.Object.DestroyImmediate(pd.Root.gameObject);
                _pools.Remove(key);
            }
        }

        // ----- internals -------
        private static PoolData GetOrCreatePool(
            T prefab,
            int defaultCapacity,
            int maxSize,
            bool collectionCheck,
            Action<T> onGet,
            Action<T> onRelease,
            Action<T> onDestroy,
            Transform parent = null
        )
        {
            if (!prefab) throw new ArgumentNullException(nameof(prefab));

            var key = prefab.GetInstanceID();

            if (_pools.TryGetValue(key, out var existing)) return existing;

            //Create a parent
            var rootGo = new GameObject($"Pool<{typeof(T).Name}>:{prefab.name}");
            rootGo.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var root = rootGo.transform;
            var poolsRoot = GameObject.Find("Pools")?.transform;

            if (!parent)
            {
                root.SetParent(poolsRoot, false);
            }
            else
            {
                root = parent;
            }

            ObjectPool<T> pool = null;

            pool = new ObjectPool<T>(
                createFunc: () =>
                {
                    var inst = UnityEngine.Object.Instantiate(prefab, root);
                    inst.gameObject.SetActive(false);
                    _owners[inst] = pool;
                    return inst;
                },
                actionOnGet: obj =>
                {
                    if (!obj.gameObject.activeSelf) obj.gameObject.SetActive(true);
                    onGet?.Invoke(obj);
                },
                actionOnRelease: obj =>
                {
                    onRelease?.Invoke(obj);
                    if (obj) obj.gameObject.SetActive(false);
                    if (obj && root && obj.transform.parent != root)
                        obj.transform.SetParent(root, false);
                },
                actionOnDestroy: obj =>
                {
                    onDestroy?.Invoke(obj);
                    if (obj) _owners.Remove(obj);
                    if (obj) UnityEngine.Object.Destroy(obj.gameObject);
                },
                collectionCheck: collectionCheck,
                defaultCapacity: Mathf.Max(1, defaultCapacity),
                maxSize: Mathf.Max(1, maxSize)
            );

            var pd = new PoolData { Pool = pool, Root = root };
            _pools[key] = pd;
            return pd;
        }
    }
}