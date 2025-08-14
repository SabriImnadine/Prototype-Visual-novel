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

    [Header("Settings")]
    public string startNodeOverride;

    VNNode _current;
    Typewriter _typewriter;

   
    readonly List<string> _lines = new List<string>();
    int _lineIndex = 0;

   
    bool _showingChoices = false;

    void Start()
    {
        _typewriter = bodyText.GetComponent<Typewriter>();
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
        
        _showingChoices = false;

       
        if (backgroundImage && _current.background)
            backgroundImage.sprite = _current.background;

       
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
        var line = _lines[_lineIndex];
        if (_typewriter != null) _typewriter.Play(line);
        else bodyText.text = line;
    }

    void ShowChoices()
    {
        ClearButtons();

        if (_current.choices.Count == 0) return;

        
        _showingChoices = true;


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

        foreach (var c in _current.choices)
            SpawnChoice(c);
    }
}
