using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VNChoice
{
    public string text;         
    public string targetNodeId; 
}

[CreateAssetMenu(fileName = "VNNode", menuName = "VN/Node", order = 0)]
public class VNNode : ScriptableObject
{
    [Header("Identity")]
    public string nodeId; 

    [Header("Content")]
    [TextArea(3, 12)] public string body; 
    public Sprite background;        

    [Header("Speaker")]
    public SpeakerId speaker = SpeakerId.None;   
    public string    emotion = "normal";        
    public SpeakerSide side = SpeakerSide.Left; 
    public bool keepOthersVisible = true;     

    [Header("Choices")]
    public List<VNChoice> choices = new List<VNChoice>();
}

[CreateAssetMenu(fileName = "VNGraph", menuName = "VN/Graph", order = 1)]
public class VNGraph : ScriptableObject
{
    public string startNodeId;
    public List<VNNode> nodes = new List<VNNode>();

    public VNNode GetNode(string id)
    {
        return nodes.Find(n => string.Equals(n.nodeId, id, StringComparison.Ordinal));
    }
}
