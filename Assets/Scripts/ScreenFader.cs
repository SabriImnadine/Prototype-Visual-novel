using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.35f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;   
            canvasGroup.interactable = false;     
        }
    }

    void Start()
    {
        if (canvasGroup != null) StartCoroutine(Fade(0f, fadeDuration));
    }

    IEnumerator Fade(float target, float dur)
    {
        if (canvasGroup == null) yield break;

        
        bool block = target > 0.001f;
        canvasGroup.blocksRaycasts = block;     
        canvasGroup.interactable = block;       

        float start = canvasGroup.alpha;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / dur);
            yield return null;
        }
        canvasGroup.alpha = target;

       
        block = target > 0.001f;
        canvasGroup.blocksRaycasts = block;      
        canvasGroup.interactable = block;        
    }

    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(LoadCo(sceneName));
    }

    IEnumerator LoadCo(string sceneName)
    {
        yield return Fade(1f, fadeDuration);
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return Fade(0f, fadeDuration);
    }
}
