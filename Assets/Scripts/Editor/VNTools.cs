#if UNITY_EDITOR
using System;   
using UnityEditor;
using UnityEngine;

public class VNTools : EditorWindow
{
    [MenuItem("Tools/VN/Auto-Assign Backgrounds From Resources")] 
    static void AutoAssignBG()
    {
        var graph = Selection.activeObject as VNGraph;
        if (!graph){ EditorUtility.DisplayDialog("VN", "Select a VNGraph asset.", "OK"); return; }
        foreach (var n in graph.nodes)
        {
            if (n.background == null)
            {
                var spr = Resources.Load<Sprite>("Backgrounds/" + n.nodeId);
                if (spr) { n.background = spr; EditorUtility.SetDirty(n); }
            }
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("VN", "Background assignment done (matching names).", "OK");
    }
}
#endif
