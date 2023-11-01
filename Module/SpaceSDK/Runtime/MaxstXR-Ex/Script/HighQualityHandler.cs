using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace MaxstXR.Extension
{
    public interface IHighQualityDelegate
    {
        void Load8kImages(Transition transition, UnityAction<Transition> complete);
    }

    public enum HighQualityState
    {
        Cancel = -1,
        Idle = 0,
        Download,
        Apply
    }

    public class HighQualityHandler
    {
        public Transition Transition { get; private set; } = null;
        public IHighQualityDelegate HighQualityDelegate { get; private set; } = null;

        public readonly bool[] assignStatus = new bool[9];
        public readonly SmoothSharedTexture[] sharedTextures = new SmoothSharedTexture[9];
        public CancellationTokenSource cancellation = null;

        public HighQualityHandler()
        {

        }

        ~HighQualityHandler()
        {
            
        }

        public void Config(Transition transition, IHighQualityDelegate highQualityDelegate)
        {
            Reset();
            Transition = transition;
            HighQualityDelegate = highQualityDelegate;
            cancellation = new CancellationTokenSource();
            HighQualityDelegate?.Load8kImages(Transition, OnComplete);
        }

        public void OnComplete(Transition transition)
        {
            if (Transition == transition)
            {
                cancellation = null;
            }
        }

        public void Assign(SmoothSharedTexture st)
        {
            var i = st.Index ?? 0;
            assignStatus[i] = true;
            SmoothSharedTexture.Assign(ref sharedTextures[i], st);
        }

        public void ToInject(SmoothIbrManager ibrManager)
        {
            for (var i = 1; i < assignStatus.Length; ++i)
            {
                if (sharedTextures[i] != null)
                {
                    ibrManager.UpdateHighResolutionMaterial(sharedTextures[i]);
                    assignStatus[i] = false;
                    SmoothSharedTexture.Assign(ref sharedTextures[i], null);
                }
            }
        }

        public void Reset()
        {
            if (cancellation != null)
            {
                if (!cancellation.IsCancellationRequested) cancellation.Cancel(true);
                cancellation.Dispose();
                cancellation = null;
            }
        }

        public void RequestCancel()
        {
            if (cancellation != null && !cancellation.IsCancellationRequested)
            {
                cancellation.Cancel(true);
            }
        }


        private void ClearTexture()
        {
            for (var i = 0; i < assignStatus.Length; ++i)
            {
                assignStatus[i] = false;
                SmoothSharedTexture.Assign(ref sharedTextures[i], null);
            }
        }
    }
}
