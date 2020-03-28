using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controller for save indicator effect objects
/// </summary>
public class SaveIndicatorScript : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(CoSaveIndicator());
    }

    private IEnumerator CoSaveIndicator()
    {
        yield return null;

        CanvasGroup cg = GetComponentInChildren<CanvasGroup>();
        const float fadeInTime = 0.5f;
        const float holdTime = 3f;
        const float fadeOutTime = 1f;
        const float deleteDelayTime = 0.1f;

        //fade in, hold, fade out
        for (float elapsed = 0; elapsed < fadeInTime; elapsed += Time.unscaledDeltaTime)
        {
            float ratio = elapsed / fadeInTime;
            cg.alpha = ratio;

            yield return null;
        }

        cg.alpha = 1;
        yield return new WaitForSecondsRealtime(holdTime);

        for (float elapsed = 0; elapsed < fadeOutTime; elapsed += Time.unscaledDeltaTime)
        {
            float ratio = elapsed / fadeInTime;
            cg.alpha = 1 - ratio;

            yield return null;
        }

        cg.alpha = 0;
        yield return new WaitForSecondsRealtime(deleteDelayTime);
        Destroy(gameObject);
    }
}
