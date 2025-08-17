using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VNManager : MonoBehaviour
{
    [Header("Data")] public VNGraph graph;

    [Header("UI")]
    public TMP_Text bodyText;
    public Image backgroundImage;
    public Transform buttonsContainer;
    public Button choiceButtonPrefab;
    public VNCharacters characters;

    [Header("Settings")]
    public string startNodeOverride;

    VNNode _current;
    Typewriter _typewriter;

   
    readonly List<string> _lines = new List<string>();
    int _lineIndex = 0;

   
    bool _showingChoices = false;

    SpeakerSide _lastSideMC = SpeakerSide.Left;
SpeakerSide _lastSideFriend = SpeakerSide.Center;
SpeakerSide _lastSideUncle = SpeakerSide.Right;
string _lastEmotionMC = "normal";
string _lastEmotionFriend = "normal";
string _lastEmotionUncle = "normal";

readonly Dictionary<string, SpeakerId> _nameToId = new Dictionary<string, SpeakerId>(StringComparer.OrdinalIgnoreCase)
{
    { "mc", SpeakerId.MC }, { "jij", SpeakerId.MC },
    { "friend", SpeakerId.Friend }, { "vriend", SpeakerId.Friend },
    { "uncle", SpeakerId.Uncle }, { "oom", SpeakerId.Uncle },
    { "narration", SpeakerId.None }
};

SpeakerSide GetLastSide(SpeakerId id) =>
    id == SpeakerId.MC ? _lastSideMC :
    id == SpeakerId.Friend ? _lastSideFriend : _lastSideUncle;

void SetLastSide(SpeakerId id, SpeakerSide side)
{
    if (id == SpeakerId.MC) _lastSideMC = side;
    else if (id == SpeakerId.Friend) _lastSideFriend = side;
    else if (id == SpeakerId.Uncle) _lastSideUncle = side;
}

string GetLastEmotion(SpeakerId id) =>
    id == SpeakerId.MC ? _lastEmotionMC :
    id == SpeakerId.Friend ? _lastEmotionFriend : _lastEmotionUncle;

void SetLastEmotion(SpeakerId id, string emo)
{
    if (id == SpeakerId.MC) _lastEmotionMC = emo;
    else if (id == SpeakerId.Friend) _lastEmotionFriend = emo;
    else if (id == SpeakerId.Uncle) _lastEmotionUncle = emo;
}


    void Start()
    {
        _typewriter = bodyText.GetComponent<Typewriter>();
         if (MusicController.Instance) MusicController.Instance.PlayVNMusic();
        string startId = string.IsNullOrEmpty(startNodeOverride) ? graph.startNodeId : startNodeOverride;
        Jump(startId);
    }

    void Update()
    {
        
        if (_showingChoices) return;

    
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (_lines.Count == 0) return;

           
            if (_typewriter != null && _typewriter.IsRunning)
            {
                _typewriter.SetTextInstant(_lines[_lineIndex]);
                return;
            }

           
            if (_lineIndex < _lines.Count - 1)
            {
                _lineIndex++;
                ShowCurrentLine();
            }
            else
            {
                 if (TryAutoActionAtNode()) return; 
                ShowChoices();
            }
        }
    }

    void ClearButtons()
    {
        for (int i = buttonsContainer.childCount - 1; i >= 0; i--)
            Destroy(buttonsContainer.GetChild(i).gameObject);
    }

    void SpawnChoice(VNChoice choice)
    {
        if (!choiceButtonPrefab) { Debug.LogError("Assign Choice Button Prefab on VNManager."); return; }

        var btn = Instantiate(choiceButtonPrefab, buttonsContainer);
        btn.transform.localScale = Vector3.one;
        btn.gameObject.SetActive(true);

        var label = btn.GetComponentInChildren<TMP_Text>(true);
        if (label) label.text = choice.text;

        btn.onClick.RemoveAllListeners();
        var captured = choice;
        btn.onClick.AddListener(() => OnChoice(captured));
     
    }

    void OnChoice(VNChoice choice)
    {
       
        _showingChoices = false;

        if (choice.targetNodeId.Contains("TransitionGameSnel", StringComparison.OrdinalIgnoreCase))
        {
            if (MusicController.Instance) MusicController.Instance.PlaySpeedMusic();
            ScreenFader.Instance?.LoadSceneWithFade("HillClimbSpeed"); return; }

        if (choice.targetNodeId.Contains("TransitionGameEco", StringComparison.OrdinalIgnoreCase))
        {
             if (MusicController.Instance) MusicController.Instance.PlayEcoMusic();
            ScreenFader.Instance?.LoadSceneWithFade("HillClimbEco"); return; }

        Jump(choice.targetNodeId);
    }

    public void Jump(string nodeId)
    {
        var node = graph.GetNode(nodeId);
        if (node == null) { Debug.LogError($"VN node not found: {nodeId}"); return; }
        _current = node;
        ApplyNode();
    }

    void ApplyNode()
    {
        
        _showingChoices = false;

       
        if (backgroundImage && _current.background)
            backgroundImage.sprite = _current.background;
    
        if (characters != null)
        characters.Apply(_current.speaker, _current.emotion, _current.side, _current.keepOthersVisible);

       
        _lines.Clear();
        var raw = _current.body.Replace("\r\n", "\n").Split('\n');
        foreach (var r in raw)
        {
            var s = r.Trim();
            if (!string.IsNullOrWhiteSpace(s)) _lines.Add(s);
        }
        if (_lines.Count == 0) _lines.Add(string.Empty);

        _lineIndex = 0;

        ShowCurrentLine();
        ClearButtons();
    }

    void ShowCurrentLine()
    {
        string raw = _lines[_lineIndex];
        string line = raw.Trim();

        // Commandes de mise en scène
        if (line.StartsWith("!Clear", StringComparison.OrdinalIgnoreCase))
        {
            characters?.HideAll();
            bodyText.text = "";
            return;
        }
        if (line.Equals("-Right", StringComparison.OrdinalIgnoreCase)) { characters?.HideSide(SpeakerSide.Right); bodyText.text = ""; return; }
        if (line.Equals("-Left", StringComparison.OrdinalIgnoreCase)) { characters?.HideSide(SpeakerSide.Left); bodyText.text = ""; return; }
        if (line.Equals("-Center", StringComparison.OrdinalIgnoreCase)) { characters?.HideSide(SpeakerSide.Center); bodyText.text = ""; return; }

        if (line.StartsWith("+"))
        {
            // +Name[emo](Side)
            var head = line.Substring(1).Trim();
            ParseHead(head, out var id, out var emo, out var side);
            if (id != SpeakerId.None)
            {
                if (side == SpeakerSide.None) side = GetLastSide(id);
                if (string.IsNullOrWhiteSpace(emo)) emo = GetLastEmotion(id);
                characters?.ShowPortrait(id, emo, side);
                SetLastSide(id, side);
                SetLastEmotion(id, emo);
            }
            bodyText.text = "";
            return;
        }

        // Dialogue : Name[emo](Side): texte
        int colon = line.IndexOf(':');
        if (colon > 0)
        {
            string head = line.Substring(0, colon).Trim();
            string tail = line.Substring(colon + 1).TrimStart();

            ParseHead(head, out var id, out var emo, out var side);

            if (id == SpeakerId.None) // Narration
            {
                characters?.HideAll();
                PlayText(tail);
                return;
            }

            if (side == SpeakerSide.None) side = GetLastSide(id);
            if (string.IsNullOrWhiteSpace(emo)) emo = GetLastEmotion(id);

            characters?.ShowPortrait(id, emo, side);
            characters?.FocusOn(side); // focus sur le parlant

            SetLastSide(id, side);
            SetLastEmotion(id, emo);

            PlayText(tail);
            return;
        }

        // Pas de tag -> narration
        characters?.HideAll();
        PlayText(line);
    }

void PlayText(string text)
{
    if (_typewriter != null) _typewriter.Play(text);
    else bodyText.text = text;
}

void ParseHead(string head, out SpeakerId id, out string emo, out SpeakerSide side)
{
    id = SpeakerId.None; emo = ""; side = SpeakerSide.None;

    // extraire le nom avant [ ou (
    string name = head;
    int b = head.IndexOf('[');
    int p = head.IndexOf('(');
    int cut = -1;
    if (b >= 0 && p >= 0) cut = Mathf.Min(b, p);
    else if (b >= 0) cut = b;
    else if (p >= 0) cut = p;
    if (cut >= 0) name = head.Substring(0, cut).Trim();

    _nameToId.TryGetValue(name, out id);

    // emotion entre [ ]
    if (b >= 0)
    {
        int b2 = head.IndexOf(']', b + 1);
        if (b2 > b) emo = head.Substring(b + 1, b2 - b - 1).Trim();
    }

    // side entre ( )
    if (p >= 0)
    {
        int p2 = head.IndexOf(')', p + 1);
        if (p2 > p)
        {
            var sideStr = head.Substring(p + 1, p2 - p - 1).Trim();
            Enum.TryParse(sideStr, true, out side);
        }
    }
}

  void LoadSceneSafe(string sceneName)
{
    
    if (MusicController.Instance)
    {
        if (sceneName == "HillClimbSpeed")      MusicController.Instance.PlaySpeedMusic();
        else if (sceneName == "HillClimbEco")   MusicController.Instance.PlayEcoMusic();
        else                                     MusicController.Instance.PlayVNMusic();
    }

    if (ScreenFader.Instance != null)
        ScreenFader.Instance.LoadSceneWithFade(sceneName);
    else
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
}
bool TryAutoActionAtNode()
{
   
    string key = (_current != null)
        ? (_current.nodeId ?? _current.name)
        : string.Empty;

    if (!string.IsNullOrEmpty(key))
    {
        if (key.IndexOf("TransitionGameSnel", StringComparison.OrdinalIgnoreCase) >= 0)
        {
                if (MusicController.Instance) MusicController.Instance.PlaySpeedMusic();
                LoadSceneSafe("HillClimbSpeed"); return true; }

        if (key.IndexOf("TransitionGameEco", StringComparison.OrdinalIgnoreCase) >= 0)
        {
                  if (MusicController.Instance) MusicController.Instance.PlayEcoMusic();
                LoadSceneSafe("HillClimbEco"); return true; }
    }

    
    if (_current.choices != null && _current.choices.Count == 1)
    {
        var only = _current.choices[0];

        if (only.targetNodeId.IndexOf("TransitionGameSnel", StringComparison.OrdinalIgnoreCase) >= 0)
        { LoadSceneSafe("HillClimbSpeed"); return true; }

        if (only.targetNodeId.IndexOf("TransitionGameEco", StringComparison.OrdinalIgnoreCase) >= 0)
        { LoadSceneSafe("HillClimbEco"); return true; }

        Jump(only.targetNodeId);
        return true;
    }

    return false;
}


   void ShowChoices()
{
    ClearButtons();

    if (_current == null || _current.choices == null || _current.choices.Count == 0)
        return;

   
    if (TryAutoActionAtNode())
        return;

   
    if (_current.choices.Count == 1)
    {
        var only = _current.choices[0];

        if (only.targetNodeId.Contains("TransitionGameSnel", StringComparison.OrdinalIgnoreCase))
        { ScreenFader.Instance?.LoadSceneWithFade("HillClimbSpeed"); _showingChoices = false; return; }

        if (only.targetNodeId.Contains("TransitionGameEco", StringComparison.OrdinalIgnoreCase))
        { ScreenFader.Instance?.LoadSceneWithFade("HillClimbEco"); _showingChoices = false; return; }

        _showingChoices = false;
        Jump(only.targetNodeId);
        return;
    }

    
    _showingChoices = true;
    foreach (var c in _current.choices)
        SpawnChoice(c);
}

}
