using UnityEngine;

namespace EffectPortfolio.CrackProgress
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    public sealed class CrackProgressPlayer : MonoBehaviour
    {
        private static readonly int ProgressId = Shader.PropertyToID("_Progress");

        [SerializeField, Min(0.01f)] private float duration = 1.25f;
        [SerializeField] private AnimationCurve revealCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool useUnscaledTime;

        private Renderer targetRenderer;
        private MaterialPropertyBlock propertyBlock;
        private float elapsed;
        private bool playing;

        public float Progress
        {
            get
            {
                EnsureInitialized();
                targetRenderer.GetPropertyBlock(propertyBlock);
                return propertyBlock.GetFloat(ProgressId);
            }
            set
            {
                EnsureInitialized();
                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(ProgressId, Mathf.Clamp01(value));
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void Update()
        {
            if (!playing)
                return;

            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);
            Progress = revealCurve.Evaluate(normalizedTime);

            if (normalizedTime >= 1f)
                playing = false;
        }

        [ContextMenu("Play Reveal")]
        public void Play()
        {
            elapsed = 0f;
            playing = true;
            Progress = 0f;
        }

        [ContextMenu("Show Complete Crack")]
        public void Complete()
        {
            playing = false;
            Progress = 1f;
        }

        private void EnsureInitialized()
        {
            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();

            propertyBlock ??= new MaterialPropertyBlock();
        }
    }
}
