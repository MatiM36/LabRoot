using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mati36.Vinyl
{
    public class VinylSourcesPool : MonoBehaviour
    {
        private Pool<VinylAudioSource> _internalPool;

        //const int DEFAULT_POOL_SIZE = 10;

        public void Initialize()
        {
            if (_internalPool == null)
            {
                var existingResources = Resources.FindObjectsOfTypeAll<VinylAudioSource>();
                for (int i = 0; i < existingResources.Length; i++)
                    Destroy(existingResources[i].gameObject);

                _internalPool = new Pool<VinylAudioSource>(VinylConfig.Current.poolDefaultSize, CreatePoolableSource, (src) => src.gameObject.SetActive(true), (src) => { src.Clear(); src.gameObject.SetActive(false); });
            }
            SceneManager.activeSceneChanged += OnSceneChange;
        }

        private void OnSceneChange(Scene prevScene, Scene newScene)
        {
            if (VinylConfig.Current.autoStopOnSceneChange)
                VinylManager.StopAll();
        }

        private VinylAudioSource CreatePoolableSource()
        {
            VinylAudioSource source = new GameObject("PooledAudioSource", typeof(VinylAudioSource)).GetComponent<VinylAudioSource>();
            source.transform.parent = transform;
            //source.gameObject.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            source.Initialize();
            source.e_OnSoundReturnToPool += ReturnSourceToPool;
            return source;
        }

        private void ReturnSourceToPool(VinylAudioSource source)
        {
            _internalPool.Return(source);
        }

        public VinylAudioSource GetSource()
        {
            return _internalPool.Get();
        }

        public void ApplyToActiveSources(Action<VinylAudioSource> actionToApply)
        {
            foreach (var src in _internalPool)
                actionToApply(src);
        }

        //private void OnDestroy()
        //{
        //    if(_internalPool != null)
        //    {
        //        foreach (var src in _internalPool)
        //            Destroy(src.gameObject);
        //        _internalPool = null;
        //    }
        //}
    }
}