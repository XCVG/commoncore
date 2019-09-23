using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen fade hack script. Spawn it in and it'll fade
/// </summary>
public class ScreenFadeHackScript : MonoBehaviour
{

    [SerializeField]
    private Image FadeImage;
    [SerializeField]
    private Color StartColor = new Color(0, 0, 0, 0);
    [SerializeField]
    private Color EndColor = new Color(0, 0, 0, 1);
    [SerializeField]
    private float FadeTime = 5f;

    private float Elapsed;

    void Start()
    {
        FadeImage.color = StartColor;
    }

    
    void Update()
    {
        if (Elapsed > FadeTime)
            return;

        Elapsed += Time.deltaTime;

        float ratio = Elapsed / FadeTime;
        Color currentColor = Vector4.Lerp(StartColor, EndColor, ratio); //hey, they actually did implement implicit conversions!
        FadeImage.color = currentColor;
    }
}
