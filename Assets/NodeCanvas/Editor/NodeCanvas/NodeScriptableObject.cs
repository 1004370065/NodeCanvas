using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
namespace NodeCanvas{
	[System.Serializable]
	public class NodeScriptableObject<TNode,TTransition,TClip>: ScriptableObject where TNode:Node,new() where TTransition:NodeTransition,new() where TClip:NodeClip<TNode,TTransition>,new (){
		public TClip[] NodeClips = new TClip[0];
		public int currentClipid= 0;
		public TClip CurrentClip{
			get{
				if (currentClipid < 0 || currentClipid >= NodeClips.Length)
					return null;
				return NodeClips [currentClipid]; 
			}
		}
		public NodeScriptableObject(){
			AddNodeClip ();
		}
		public void RemoveNodeClip(){
			if (NodeClips.Length == 1)
				return;
			NodeClips = ArrayUtility.Remove (NodeClips,CurrentClip);
		}
		public void AddNodeClip(){
			NodeClips = ArrayUtility.Add<TClip> (NodeClips, new TClip ());
			NodeClips [NodeClips.Length - 1].ClipName = "Clip_" + NodeClips.Length;
		}
		public TNode[] nodes{
			get{ return CurrentClip.nodes; }
		}
		public string defaultNodePath{
			get{ return CurrentClip.defaultNodePath; }
		}
		public string currentGroupPath{
			get{return  CurrentClip.currentGroupPath; }
			set{ CurrentClip.currentGroupPath = value; }
		}
		public TNode startNode{
			get{
				return CurrentClip.startNode;
			}
		}

		public TNode CurrentGroup{
			get{
				if (CurrentClip == null)
					return null;
				return CurrentClip.CurrentGroup;
			}
			set{
				if (CurrentClip != null)
				CurrentClip.CurrentGroup = value;
			}
		}
		public List<TNode> ShowNodes{
			get{
				return CurrentClip.ShowNodes;
			}
		}
		public int StringToHash(string name){
			return CurrentClip.StringToHash(name);
		}

		public virtual void AddTransition (Node fromNode,Node toNode){
			CurrentClip.AddTransition (fromNode, toNode);
		}
		public virtual void DeleTransition(Node targetNode, NodeTransition targetTransition){
			CurrentClip.DeleTransition(targetNode,targetTransition);
		}

		public virtual TNode AddNodeGroup(Vector2 pos){
			return CurrentClip.AddNodeGroup (pos);
		}
		public virtual TNode AddNode(Vector2 pos){
			return CurrentClip.AddNode(pos);
		}
		public virtual void RemoveNode(TNode node){
			CurrentClip.RemoveNode (node);
		}
		public virtual void SetDefaultState(Node node){
			CurrentClip.SetDefaultState (node);
		}
		public TNode FindNodeWithHash(int hash){
			return CurrentClip.FindNodeWithHash(hash);
		}
		public TNode FindNodeWithPath(string path){
			return CurrentClip.FindNodeWithPath(path);
		}
		public TNode FindState(string path){
			return CurrentClip.FindState(path);
		}
	}
	public class NodeClip<TNode,TTransition>where TNode:Node,new() where TTransition:NodeTransition,new(){
		#region Member
		public string ClipName = "";
		[SerializeField]
		public TNode[] nodes = new TNode[0];
		public int nodecreateCount = 0;
		public string defaultNodePath;
		public string currentGroupPath = "Base";
		public NodeClip(){
			AddNodeGroup (Vector2.zero).name ="Base";
			AddNode (Vector2.zero).name ="Idle";
		}
		public TNode startNode{
			get{
				if ((defaultNodePath == null || defaultNodePath == "") && nodes.Length>0) {
					defaultNodePath = nodes [0].GetPath();
				}
				return FindState (defaultNodePath);
			}
		}

		#endregion
		public TNode CurrentGroup{
			get{
				for (int i = 0; i < nodes.Length; i++) {
					if (nodes [i].type == NodeType.NodeGroup && nodes [i].GetGroupPath() == currentGroupPath) {
						return nodes[i];
					}
				}
				return nodes [0];
			}
			set{ 
				if(value!=null && value.type == NodeType.NodeGroup)
					currentGroupPath = value.GetGroupPath ();
			}
		}
		public List<TNode> ShowNodes{
			get{
				List<TNode> states = new List<TNode> ();
				for (int i = 0; i < nodes.Length; i++) {
					if (nodes [i].groupPath == currentGroupPath || (nodes[i].GetPath() == currentGroupPath && nodes[i].type == NodeType.NodeGroup)) {
						states.Add (nodes[i]);
					}
				}
				return states;
			}
		}
		public int StringToHash(string name){
			return name.GetHashCode ();
		}

		public virtual void AddTransition (Node fromNode,Node toNode){
			TTransition tr = new TTransition ();
			tr.fromNodeHash = fromNode.hash;
			tr.toNodeHash = toNode.hash;
			fromNode.AddTransition (tr);
		}
		public virtual void DeleTransition(Node targetNode, NodeTransition targetTransition){
			targetNode.RemoveTransition (targetTransition);
		}

		public virtual TNode AddNodeGroup(Vector2 pos){
			return addNode(pos,NodeType.NodeGroup,"NodeGroup_"+nodecreateCount);
		}
		public virtual TNode AddNode(Vector2 pos){
			return addNode(pos,NodeType.Node,"Node_"+nodecreateCount);
		}
		public virtual void RemoveNode(TNode node){
			for (int i = 0; i < nodes.Length; i++) {
				for (int t = 0; t < nodes [i].transition.Length; t++) {
					if (nodes [i].transition [t].toNodeHash == node.hash || nodes [i].transition [t].fromNodeHash == node.hash) {
						nodes [i].transition = ArrayUtility.Remove (nodes [i].transition, nodes [i].transition [t]);
					}
				}
			}
			nodes = ArrayUtility.Remove(nodes, node);
		}
		public virtual void SetDefaultState(Node node){
			defaultNodePath = node.GetPath ();
		}
		public TNode FindNodeWithPath(string path){
			for (int i = 0; i < nodes.Length; i++) {
				if (nodes [i].GetPath() == path)
					return nodes [i];
			}
			return null;
		}
		public TNode FindNodeWithHash(int hash){
			for (int i = 0; i < nodes.Length; i++) {
				if (nodes [i].hash == hash)
					return nodes [i];
			}
			return null;
		}
		public TNode FindState(string path){
			for (int i = 0; i < nodes.Length; i++) {
				if (nodes [i].GetPath() == path)
					return nodes [i];
			}
			return null;
		}

		private TNode addNode(Vector2 pos,NodeType type,string name){
			TNode node = new TNode ();
			node.position.position = pos;
			node.type = type;
			node.name = name;
			nodes= ArrayUtility.Add<TNode> (nodes,node);
			nodecreateCount++;
			node.hash = StringToHash(nodecreateCount.ToString());
			node.groupPath = currentGroupPath;
			return node;
		}
	}
}