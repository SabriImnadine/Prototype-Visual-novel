#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEditor;
using System.IO;
using UnityEngine;
using System;  

public static class TwineHtmlImporter
{

    static readonly Regex PassageRx = new Regex(
        "<tw-passagedata[^>]*name=\"(?<name>[^\"]+)\"[^>]*>(?<body>[\\s\\S]*?)</tw-passagedata>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

   
    static readonly Regex LinkRx = new Regex(
        "\\[\\[(?<label>[^\\]]*?)(?:->|→)?\\s*(?<target>[^\\]]+?)\\]\\]",
        RegexOptions.Compiled);

    [MenuItem("Tools/VN/Import From Twine HTML")] 
    public static void ImportFromSelectedHtml()
    {
        var obj = Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            EditorUtility.DisplayDialog("VN Importer", "Select a Twine .html asset in the Project window first.", "OK");
            return;
        }

        string html = File.ReadAllText(path);
        var matches = PassageRx.Matches(html);
        if (matches.Count == 0)
        {
            EditorUtility.DisplayDialog("VN Importer", "No <tw-passagedata> passages found.", "OK");
            return;
        }


        string dir = Path.GetDirectoryName(path);
        string graphPath = Path.Combine(dir, Path.GetFileNameWithoutExtension(path) + "_Graph.asset");
        var graph = ScriptableObject.CreateInstance<VNGraph>();

        foreach (Match m in matches)
        {
            string name = m.Groups["name"].Value.Trim();
            string body = m.Groups["body"].Value.Trim();

          
            body = System.Net.WebUtility.HtmlDecode(body);
           
            body = body.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n");

            var node = ScriptableObject.CreateInstance<VNNode>();
            node.nodeId = name;

            
            var links = LinkRx.Matches(body);
            var cleanBody = LinkRx.Replace(body, "");
            node.body = cleanBody.Trim();

            foreach (Match lm in links)
            {
                string label = lm.Groups["label"].Value.Trim();
                string target = lm.Groups["target"].Value.Trim();
                if (string.IsNullOrEmpty(label)) label = target; 
                node.choices.Add(new VNChoice { text = label, targetNodeId = target });
            }

            
            string nodePath = Path.Combine(dir, $"VNNode_{San(name)}.asset");
            AssetDatabase.CreateAsset(node, nodePath);
            graph.nodes.Add(node);
        }

        
        graph.startNodeId = graph.nodes.Count > 0 ? graph.nodes[0].nodeId : ":START";

        AssetDatabase.CreateAsset(graph, graphPath);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("VN Importer", $"Imported {graph.nodes.Count} passages into a VNGraph asset:\n{graphPath}", "OK");
    }

    static string San(string s)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c.ToString(), "_");
        return s;
    }
}
#endif