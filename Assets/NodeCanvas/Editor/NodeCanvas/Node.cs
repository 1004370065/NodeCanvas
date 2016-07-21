using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace NodeCanvas{

public enum SelectType{
	None,
	Node,
	NodeGroud,
	Transition,
}
public enum NodeType{
	Node,
	NodeGroup,
}
[System.Serializable]
public class Node{
	public int hash;
	[HideInInspector]
	public Rect position = new Rect(0,0,150,30);
	[HideInInspector]
	public NodeType type = NodeType.Node;
	[HideInInspector]
	public Vector2 CanvasPos;
//	[HideInInspector]
	public NodeTransition[] transition = new NodeTransition[0];
	[HideInInspector]
	public NodeInfo[] infos = new NodeInfo[0];
	public string name = "";
	public string groupPath = "Base";
		public Node(){
			infos = ArrayUtility.Add<NodeInfo>(infos,new NodeInfo ());
			position = new Rect (0, 0, 150, 30);
		}
	public Node(NodeType _type,string _name = "node",string _groupPath = "Base"){
		
		infos = ArrayUtility.Add<NodeInfo>(infos,new NodeInfo ());
		name = _name;
		groupPath = _groupPath;
		type = _type;
		position = new Rect (0, 0, 150, 30);
	}
	public void AddTransition(Node target){
		transition = ArrayUtility.Add<NodeTransition>(transition,new NodeTransition( this.hash, target.hash));
	}
	public void AddTransition(NodeTransition target){
		transition = ArrayUtility.Add<NodeTransition>(transition,target);
	}
	/// <summary>
	/// 移除State 的时候 顺便移除对应的Transition
	/// </summary>
	/// <param name="target">Target.</param>
	public void RemoveTransition(Node target){
		for (int i = 0; i < transition.Length; i++) {
			if (target.hash == transition [i].toNodeHash) {
				transition = ArrayUtility.RemoveAt<NodeTransition> (transition, i);
			}
		}
	}
	/// <summary>
	/// 移除指定Transition
	/// </summary>
	/// <param name="target">Target.</param>
	public void RemoveTransition(NodeTransition target){
		transition = ArrayUtility.Remove<NodeTransition>(transition,target);
	}
	public void SetName(string _name){
		name = _name;
	}
	public string GetPath(){
		return name == ""?groupPath:groupPath+"/"+name;
	}
	public string GetGroupPath(){
		return groupPath;
	}
}
public enum NodeSelectionMode{
	None,
	Pick,
	Rect,
	CannotRect,
}
[System.Serializable]
public class NodeTransition{
	public int fromNodeHash;
	public int toNodeHash;
	public NodeTransition(){
		
	}
	public NodeTransition(int _from,int _to){
		fromNodeHash = _from;
		toNodeHash = _to;
	}
}
[System.Serializable]
public class NodeInfo{
	public string text = "";
	public int line = 0;
}
public class DrawTransitionLine{
	public NodeTransition line;
	public Vector2 startPos;
	public Vector2 endPos;
	public DrawTransitionLine(NodeTransition _line,Vector2 _start,Vector2 _end){
		line = _line;
		startPos = _start;
		endPos = _end;
	}
}
}