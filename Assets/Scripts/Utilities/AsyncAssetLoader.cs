using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Utilities
{
    public class AsyncAssetLoader : MonoBehaviour
    {
        private static AsyncAssetLoader _instance;
        private Dictionary<string, AsyncOperationHandle> _loadedAssets = new();
        private Dictionary<string, Queue<Action<GameObject>>> _pendingCallbacks = new();

        public static AsyncAssetLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("AsyncAssetLoader");
                    _instance = obj.AddComponent<AsyncAssetLoader>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadAssetAsync<T>(string assetPath, Action<T> onComplete) where T : UnityEngine.Object
        {
            if (_loadedAssets.ContainsKey(assetPath))
            {
                if (_loadedAssets[assetPath].IsDone)
                {
                    onComplete?.Invoke(_loadedAssets[assetPath].Result as T);
                    return;
                }
            }

            StartCoroutine(LoadAssetCoroutine(assetPath, onComplete));
        }

        private IEnumerator LoadAssetCoroutine<T>(string assetPath, Action<T> onComplete) where T : UnityEngine.Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetPath);
            _loadedAssets[assetPath] = handle;

            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke(handle.Result);
            }
            else
            {
                Debug.LogError($"Failed to load asset: {assetPath}");
                onComplete?.Invoke(null);
            }
        }

        public void LoadPrefabAsync(string assetPath, Action<GameObject> onComplete)
        {
            LoadAssetAsync<GameObject>(assetPath, onComplete);
        }

        public void UnloadAsset(string assetPath)
        {
            if (_loadedAssets.ContainsKey(assetPath))
            {
                Addressables.Release(_loadedAssets[assetPath]);
                _loadedAssets.Remove(assetPath);
            }
        }

        public void UnloadAllAssets()
        {
            foreach (var handle in _loadedAssets.Values)
            {
                Addressables.Release(handle);
            }
            _loadedAssets.Clear();
        }

        private void OnDestroy()
        {
            UnloadAllAssets();
        }
    }
}