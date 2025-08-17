using UnityEngine;
using System.Collections;

public class MusicController : MonoBehaviour
{
    public static MusicController Instance { get; private set; }

    [Header("Wiring")]
    public AudioSource audioSource;

    [Header("Clips")]
    public AudioClip vnMusic;     
    public AudioClip ecoMusic;    
    public AudioClip speedMusic;  

    [Header("Fade")]
    public float fadeDuration = 0.5f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (audioSource) audioSource.loop = true;
        // Start with silence; VN will call PlayVNMusic() on load.
    }

    public void PlayVNMusic()    => CrossfadeTo(vnMusic);
    public void PlayEcoMusic()   => CrossfadeTo(ecoMusic);
    public void PlaySpeedMusic() => CrossfadeTo(speedMusic);

    public void StopMusic()      => StartCoroutine(FadeOutThenStop());

    void CrossfadeTo(AudioClip clip)
    {
        if (!audioSource || clip == null) return;
        if (audioSource.clip == clip && audioSource.isPlaying) return;
        StartCoroutine(FadeSwap(clip));
    }

    IEnumerator FadeSwap(AudioClip next)
    {
        float t = 0f;
        float startVol = audioSource.volume;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }
        audioSource.clip = next;
        audioSource.Play();

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0f, startVol, t / fadeDuration);
            yield return null;
        }
    }

    IEnumerator FadeOutThenStop()
    {
        float t = 0f;
        float startVol = audioSource.volume;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }
        audioSource.Stop();
    }
}
