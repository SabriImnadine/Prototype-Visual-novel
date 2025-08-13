using System.Collections;
using TMPro;
using UnityEngine;


public class Typewriter : MonoBehaviour
{
    public float charsPerSecond = 40f;
    public bool skipOnClick = true;

    TMP_Text _label;
    Coroutine _routine;
    bool _isRunning;

    void Awake(){ _label = GetComponent<TMP_Text>(); }

    public void SetTextInstant(string text)
    {
        if (_routine != null) StopCoroutine(_routine);
        _label.text = text;
        _isRunning = false;
    }

    public void Play(string text)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(TypeCo(text));
    }

    IEnumerator TypeCo(string text)
    {
        _isRunning = true;
        _label.text = string.Empty;
        float t = 0f;
        int i = 0;
        while (i < text.Length)
        {
            if (skipOnClick && Input.GetMouseButtonDown(0))
            {
                _label.text = text;
                break;
            }
            t += Time.deltaTime * charsPerSecond;
            int next = Mathf.Clamp(Mathf.FloorToInt(t), 0, text.Length);
            if (next != i)
            {
                _label.text = text.Substring(0, next);
                i = next;
            }
            yield return null;
        }
        _isRunning = false;
    }

    public bool IsRunning => _isRunning;
}