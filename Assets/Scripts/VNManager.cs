using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class VNManager : MonoBehaviour
{
    [Header("Data")] public VNGraph graph;

    [Header("UI")]
    public TMP_Text bodyText;
    public Image backgroundImage;
    public Transform buttonsContainer;
    public Button choiceButtonPrefab;

    [Header("Settings")]
    public string startNodeOverride;

    VNNode _current;
    Typewriter _typewriter;

    // Gestion ligne-par-ligne
    readonly List<string> _lines = new List<string>();
    int _lineIndex = 0;

    void Start()
    {
        _typewriter = bodyText.GetComponent<Typewriter>();
        string startId = string.IsNullOrEmpty(startNodeOverride) ? graph.startNodeId : startNodeOverride;
        Jump(startId);
    }

   void Update()
{
    if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
    {
        if (_lines.Count == 0) return;

        // si l’effet tape encore → afficher la ligne instantanément
        if (_typewriter != null && _typewriter.IsRunning)
        {
            _typewriter.SetTextInstant(_lines[_lineIndex]);
            return;
        }

        // sinon passer à la ligne suivante; à la fin, on affiche les choix
        if (_lineIndex < _lines.Count - 1)
        {
            _lineIndex++;
            ShowCurrentLine();
        }
        else
        {
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
        var btn = Instantiate(choiceButtonPrefab, buttonsContainer);
        btn.gameObject.SetActive(true);
        btn.GetComponentInChildren<TMP_Text>().text = choice.text;
        btn.onClick.AddListener(() => OnChoice(choice));
    }

  void OnChoice(VNChoice choice)
{
    if (choice.targetNodeId.Contains("TransitionGameSnel", StringComparison.OrdinalIgnoreCase))
    { ScreenFader.Instance?.LoadSceneWithFade("HillClimbSpeed"); return; }

    if (choice.targetNodeId.Contains("TransitionGameEco", StringComparison.OrdinalIgnoreCase))
    { ScreenFader.Instance?.LoadSceneWithFade("HillClimbEco"); return; }

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
    
        if (backgroundImage && _current.background)
            backgroundImage.sprite = _current.background;

       
        _lines.Clear();
        var rawLines = _current.body.Replace("\r\n", "\n").Split('\n');
        foreach (var r in rawLines)
        {
            var s = r.Trim();
            if (!string.IsNullOrWhiteSpace(s)) _lines.Add(s);
        }
        if (_lines.Count == 0) _lines.Add(""); 

        _lineIndex = 0;

        
        ShowCurrentLine();
        ClearButtons();
    }

    void ShowCurrentLine()
    {
        var line = _lines[_lineIndex];
        if (_typewriter != null) _typewriter.Play(line);
        else bodyText.text = line;
    }

   void ShowChoices()
{
    ClearButtons();

 
    if (_current.choices.Count == 0) return;

    if (_current.choices.Count == 1)
    {
        var only = _current.choices[0];

       
        if (only.targetNodeId.Contains("TransitionGameSnel", StringComparison.OrdinalIgnoreCase))
        { ScreenFader.Instance?.LoadSceneWithFade("HillClimbSpeed"); return; }

        if (only.targetNodeId.Contains("TransitionGameEco", StringComparison.OrdinalIgnoreCase))
        { ScreenFader.Instance?.LoadSceneWithFade("HillClimbEco"); return; }

        Jump(only.targetNodeId);
        return;
    }

    
    foreach (var c in _current.choices)
        SpawnChoice(c);
}
}
