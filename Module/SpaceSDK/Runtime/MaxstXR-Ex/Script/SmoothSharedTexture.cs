using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace MaxstXR.Extension
{
    [Serializable]
    public class SmoothSharedTexture
    {
        [field: SerializeField] public string TexturePath { get; private set; } = string.Empty;
        [field: SerializeField] public Texture2D Texture2d { get; private set; } = null;
        public int RefCount => Texture2d != null ? refCount : 0;
        public int? Index { get; private set; } = null;
        public PovController PovController { get; private set; } = null;

        public readonly UnityEvent<SmoothSharedTexture> OnInject = new UnityEvent<SmoothSharedTexture>();
        private int refCount = 0;
        private readonly SmoothTextureManager textureManager;

        public SmoothSharedTexture(string path, int? index, 
            PovController povController, SmoothTextureManager textureManager)
        {
            TexturePath = path;
            Index = index;
            PovController = povController;
            this.textureManager = textureManager;
        }

        ~SmoothSharedTexture()
        {
            if (Texture2d != null)
            {
                Texture.Destroy(Texture2d);
                textureManager.AllowToBeUnloaded(TexturePath);
            }

            //Texture.Destroy(Texture2d);
            //textureManager.AllowToBeUnloaded(TexturePath);
        }

        public SmoothSharedTexture Inject(Texture2D texture2d) 
        {
            Texture2d = texture2d;
            OnInject.Invoke(this);
            OnInject.RemoveAllListeners();
            return this;
        }

        public SmoothSharedTexture Retain()
        {
            if (Texture2d != null)
            {
                Interlocked.Increment(ref refCount);
                //Debug.Log($"SmoothSharedTexture, Retain refCount : {refCount}/{TexturePath}");

            }
            return this;
        }

        public void Release(bool Immediate = false)
        {
            if (Texture2d != null)
            {
                if (Interlocked.Decrement(ref refCount) < 1)
                {
                    if (Immediate || !Application.isPlaying)
                    {
                        Texture.DestroyImmediate(Texture2d);
                    }
                    else
                    {
                        Texture.Destroy(Texture2d);
                    }
                    
                    Texture2d = null;
                    textureManager.AllowToBeUnloaded(TexturePath);
                }
                //Debug.Log($"SmoothSharedTexture, Release refCount : {refCount}/{TexturePath}");
            }
        }

        public static void Assign(ref SmoothSharedTexture dst, SmoothSharedTexture src)
        {
            var temp = dst;
            dst = src?.Retain() ?? null;
            temp?.Release();
        }
    }
}
