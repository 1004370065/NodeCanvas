using UnityEngine;
using UnityEditor;
using System.Collections;
using NodeCanvas;
public class DemoEditor : NodeEditor {

	[MenuItem("Assets/Node/Demo/Open Editor")]
	public static void Init(){
		var window =  GetWindow<DemoEditor> ();
	}
	DemoScriptableObject _scriptableObject;
	DemoClipData currentNodeClip{
		get{ 
			if(_scriptableObject!=null)
				return _scriptableObject.CurrentClip; 
			return null;
		}
	}
	protected override void OnEnable ()
	{
		base.OnEnable ();
		SelectObj ();
	}
	void OnSelectionChange (){
		SelectObj ();
	}
	void SelectObj(){
		DemoScriptableObject script = Selection.activeObject as DemoScriptableObject;
		if (script != null) {
			_scriptableObject = script;
			_canvasOffset = currentNodeClip.CurrentGroup.CanvasPos;
			UpdateNodes ();
			_startNode = (Node) currentNodeClip.startNode;
			_currentGroupPath = currentNodeClip.currentGroupPath;
		}
		Repaint ();
	}
	void UpdateNodes(){
		_nodes.Clear ();
		for (int i = 0; i < currentNodeClip.ShowNodes.Count; i++) {
			_nodes .Add( currentNodeClip.ShowNodes[i]);
		}
		allNodes = currentNodeClip.nodes;
		int point =0 ;
		for (int i = 0; i < currentNodeClip.nodes.Length; i++) {
			point = 0;
			Node fromnode = currentNodeClip.nodes [i];
			for(int t = 0;t<currentNodeClip.nodes[i].transition.Length;t++){
				Node tonode = FindNodeWithHash (currentNodeClip.nodes [i].transition [t].toNodeHash);
				if (tonode != null) {

					tonode.position.position = fromnode.position.position + new Vector2 (200,t * 60+point);
					point += tonode.infos [0].line * 12;
				}
			}
		}
	}

	#region 
	protected override void ContextMenu_Canvas (ref GenericMenu menu)
	{
		menu.AddItem (new GUIContent ("Add Node"), false, delegate {
			AddNode(_scriptableObject.AddNode(_cachedMousePosition+_canvasOffset));
		});
		menu.AddItem (new GUIContent ("Add NodeGroup"), false, delegate {
			AddNode(_scriptableObject.AddNodeGroup(_cachedMousePosition+_canvasOffset));
		});
	}
	protected override void ContextMenu_Node (ref GenericMenu menu)
	{
		menu.AddItem (new GUIContent ("Dele Node"), false, delegate {
			_scriptableObject.RemoveNode((DemoNode)_selectNodes[0]);	
			UpdateNodes();
		});
		menu.AddItem (new GUIContent ("Make Transition"), false, delegate {
			_makeTranstion = true;	
		});
	}
	protected override void ContextMenu_NodeGroud (ref GenericMenu menu)
	{
		menu.AddItem (new GUIContent ("Dele NodeGroup"), false, delegate {
			_scriptableObject.RemoveNode((DemoNode)_selectNodes[0]);	
			UpdateNodes();
		});
	}
	#endregion
}
