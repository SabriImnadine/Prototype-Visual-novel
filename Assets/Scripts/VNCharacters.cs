using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum SpeakerId { None, MC, Friend, Uncle }
public enum SpeakerSide { None, Left, Center, Right }

public class VNCharacters : MonoBehaviour
{
    [Header("UI")]
    public Image left;
    public Image center;
    public Image right;

    [Header("Effet focus")]
    public float dimAlpha = 0.45f;


    readonly Dictionary<SpeakerId, string> prefixMap = new()
    {
        { SpeakerId.MC, "3_" },
        { SpeakerId.Friend, "4_" },
        { SpeakerId.Uncle, "5_" }
    };

    Dictionary<string, Sprite> cache;

    void Awake()
    {
        cache = Resources.LoadAll<Sprite>("Characters")
            .GroupBy(s => s.name.ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First());
        HideAll();
    }

    public void HideAll()
    {
        if (left) left.gameObject.SetActive(false);
        if (center) center.gameObject.SetActive(false);
        if (right) right.gameObject.SetActive(false);
    }

    public void Apply(SpeakerId id, string emotion, SpeakerSide side, bool keepOthersVisible = true)
    {

        var slots = new[] { left, center, right };
        foreach (var img in slots) if (img) img.preserveAspect = true;

        if (!keepOthersVisible)
        {

            HideAll();
        }


        if (id != SpeakerId.None && side != SpeakerSide.None)
        {
            var target = (side == SpeakerSide.Left) ? left : (side == SpeakerSide.Center ? center : right);
            if (target)
            {
                var prefix = prefixMap[id];
                var sp = FindSprite(prefix, emotion) ?? FindSprite(prefix, "normal");
                if (sp)
                {
                    target.sprite = sp;
                    target.color = Color.white;
                    target.gameObject.SetActive(true);
                }
            }
        }


        DimNonSpeaking(side);
    }

    void DimNonSpeaking(SpeakerSide speakingSide)
    {
        SetAlpha(left, (speakingSide == SpeakerSide.Left) ? 1f : dimAlpha);
        SetAlpha(center, (speakingSide == SpeakerSide.Center) ? 1f : dimAlpha);
        SetAlpha(right, (speakingSide == SpeakerSide.Right) ? 1f : dimAlpha);
    }

    void SetAlpha(Image img, float a)
    {
        if (!img || !img.gameObject.activeSelf) return;
        var c = img.color; c.a = a; img.color = c;
    }

    Sprite FindSprite(string prefix, string emotionRaw)
    {
        var e = string.IsNullOrWhiteSpace(emotionRaw) ? "normal" : emotionRaw.Trim().ToLowerInvariant();
        var keys = new[] { prefix + e, prefix + e + "1", prefix + e + "2" };

        foreach (var k in keys)
            if (cache.TryGetValue(k.ToLowerInvariant(), out var sp)) return sp;

        var hit = cache.FirstOrDefault(kv =>
            kv.Key.StartsWith(prefix.ToLowerInvariant()) && kv.Key.Contains(e));
        return hit.Value;
    }
    
    Image GetSlot(SpeakerSide side)
{
    return side == SpeakerSide.Left ? left :
           side == SpeakerSide.Center ? center :
           side == SpeakerSide.Right ? right : null;
}

public void ShowPortrait(SpeakerId id, string emotion, SpeakerSide side)
{
    if (id == SpeakerId.None || side == SpeakerSide.None) return;
    var slot = GetSlot(side);
    if (!slot) return;

    string prefix = id == SpeakerId.MC ? "3_" : id == SpeakerId.Friend ? "4_" : "5_";
    var sp = FindSprite(prefix, emotion) ?? FindSprite(prefix, "normal");
    if (!sp) return;

    slot.sprite = sp;
    slot.preserveAspect = true;
    slot.color = Color.white;
    slot.gameObject.SetActive(true);
}

public void HideSide(SpeakerSide side)
{
    var slot = GetSlot(side);
    if (slot) slot.gameObject.SetActive(false);
}

public void FocusOn(SpeakerSide speakingSide, float dim = -1f)
{
    if (dim < 0f) dim = dimAlpha;
    if (left && left.gameObject.activeSelf)   { var c = left.color;   c.a = (speakingSide == SpeakerSide.Left)   ? 1f : dim; left.color = c; }
    if (center && center.gameObject.activeSelf){ var c = center.color; c.a = (speakingSide == SpeakerSide.Center) ? 1f : dim; center.color = c; }
    if (right && right.gameObject.activeSelf)  { var c = right.color;  c.a = (speakingSide == SpeakerSide.Right)  ? 1f : dim; right.color = c; }
}

}
