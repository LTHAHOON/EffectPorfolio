using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InvertFullScreenController : MonoBehaviour
{
    [SerializeField]
    private ScriptableRendererFeature _invertFullScreenFeature;

    private readonly int _alphaPropertyId = Shader.PropertyToID("_InvertFS_Alpha");
    private Coroutine _transitionCoroutine;
    private bool _isEnding;

    public void OnStartInvertFullScreen(float duration = 0)
    {
        StopTransition();
        _isEnding = false;
        Shader.SetGlobalFloat(_alphaPropertyId, 0f);
        _invertFullScreenFeature.SetActive(true);

        if(duration <= 0)
        {
            Shader.SetGlobalFloat(_alphaPropertyId, 1f);
            return;
        }
        _transitionCoroutine = StartCoroutine(IESetInvertFullScreen(true, duration));

    }

    public void OnEndInvertFullScreen(float duration = 0)
    {
        if (_isEnding)
            return;

        _isEnding = true;
        StopTransition();

        if(duration <= 0)
        {
            Shader.SetGlobalFloat(_alphaPropertyId, 0f);
            _invertFullScreenFeature.SetActive(false);
            return;
        }

        _invertFullScreenFeature.SetActive(true);
        _transitionCoroutine = StartCoroutine(IESetInvertFullScreen(false, duration));
    }

    public IEnumerator IESetInvertFullScreen(bool IsStart, float duration)
    {
        float curTime = 0;
        float targetAlpha = IsStart ? 1f : 0f;

        float startAlpha = IsStart ? 0f : 1f;
        while (curTime < duration)
        {
            float normalizedTime = Mathf.Clamp01(curTime / duration);
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            Shader.SetGlobalFloat(_alphaPropertyId, alpha);
            curTime += Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
        }

        Shader.SetGlobalFloat(_alphaPropertyId, targetAlpha);
        if (!IsStart)
        {
            _invertFullScreenFeature.SetActive(false);
        }

        _transitionCoroutine = null;
    }

    private void StopTransition()
    {
        if (_transitionCoroutine == null)
        {
            return;
        }

        StopCoroutine(_transitionCoroutine);
        _transitionCoroutine = null;
    }
}
