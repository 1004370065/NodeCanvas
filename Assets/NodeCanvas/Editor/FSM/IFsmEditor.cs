using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
namespace NodeCanvas{
public class IFsmEditor : NodeEditor {
	#region Member
	FSMScriptableObject	_scriptableObject; 
	ReorderableList _parametersList;
	Vector2 _parametersPos;

	#endregion
	#region Init
	[MenuItem("Assets/Node/FSM/Open Editor")]
	public static void Init(){
		var window =  GetWindow<IFsmEditor> ();
		Rect r = new Rect ();
		r.width =  Screen.currentResolution.width*0.5f;
		r.height = Screen.currentResolution.height*0.5f;
		r.center = new Vector2(Screen.currentResolution.width,Screen.currentResolution.height)* 0.5f;
		window.position = r;
	}
	protected override void OnEnable ()
	{
		base.OnEnable ();
		SelectObj ();
	}
	protected override void UpdateOffset ()
	{
		if(_scriptableObject!=null)
			_scriptableObject.CurrentGroup.CanvasPos = offset;
	}
	void OnSelectionChange (){
		SelectObj ();
	}
	void SelectObj(){
		FSMScriptableObject script = Selection.activeObject as FSMScriptableObject;
		if (script != null) {
			_scriptableObject = script;
			_parametersList = new ReorderableList(_scriptableObject.parameters,typeof(FSMParameters));
			_parametersList.displayAdd = false;
			_parametersList.headerHeight = 0;
				offset = _scriptableObject.CurrentGroup.CanvasPos;
			UpdateNodes ();
//			startNode = (Node) _scriptableObject.startNode;
				groupPaths = _scriptableObject.currentGroupPath;
		}
		Repaint ();
	}
	#endregion
	#region Panel
	protected override void DrawLeftPanel ()
	{
		#region +
		GUILayout.BeginHorizontal (EditorStyles.toolbar);
		GUILayout.FlexibleSpace ();    
			if (GUILayout.Button ( NodeStyles.toolbarPlus,NodeStyles.label)) {
			GenericMenu menu= new GenericMenu();
			menu.AddItem(new GUIContent("Float"),false,delegate() {
				_scriptableObject.AddParamets(FSMParametersType.Float);
			});
			menu.AddItem(new GUIContent("Int"),false,delegate() {
				_scriptableObject.AddParamets(FSMParametersType.Int);
			});
			menu.AddItem(new GUIContent("Bool"),false,delegate() {
				_scriptableObject.AddParamets(FSMParametersType.Bool);
			});
			menu.AddItem(new GUIContent("Trigger"),false,delegate() {
				_scriptableObject.AddParamets(FSMParametersType.Trigger);
			});
			menu.ShowAsContext();
		}
		GUILayout.EndHorizontal ();
		#endregion
		#region Parameter
		if(_parametersList != null){
			_parametersPos = EditorGUILayout.BeginScrollView(_parametersPos);
			_parametersList.drawElementCallback = (Rect r,int id,bool isactive,bool isfocused)=>{
				FSMParameters parameter = _scriptableObject.parameters[id];
				Rect r1 = r;
				r1.width -= 50;
				if(isactive){
					parameter.name = GUI.TextField(r1,parameter.name);	
				}
				else{
					GUI.Label(r1,parameter.name);	
				}
				Rect r2 = r;
				r2.x += r1.width;
				r2.width = 50;
				switch(parameter.parameterType){
				case FSMParametersType.Float:
					parameter.defaultValue = EditorGUI.FloatField(r2,parameter.defaultValue);
					break;
				case FSMParametersType.Int:
					parameter.defaultValue = (float)(EditorGUI.IntField(r2,(int)parameter.defaultValue));
					break;
				case FSMParametersType.Bool:
					parameter.defaultValue = (float)(EditorGUI.Toggle(r2,parameter.defaultValue == 1) == true?1:0);
					break;
				case FSMParametersType.Trigger:
						parameter.defaultValue = (float)(EditorGUI.Toggle(r2,parameter.defaultValue == 1,NodeStyles.toggleTrigger) == true?1:0);
					break;
				}
			};
			_parametersList.DoLayoutList();
			EditorGUILayout.EndScrollView();
		}
		#endregion
	
	}
	protected override void DrawRightPanel ()
	{
		switch(selectType){
		case SelectType.None:
			break;
		case SelectType.NodeGroud:
			break;
		case SelectType.Node:
			if(selectNodes[0] is FSMNode)
				ShowNode ((FSMNode)selectNodes [0]);
			break;
		case SelectType.Transition:
			ShowTransition (selectTransition);
			break;
		}

//		for(int i = 0;i < nodes.Count;i++){
//			GUILayout.Label (nodes [i].GetPath ());
//		}
//		for(int i = 0;i < allNodes.Length;i++){
//			GUILayout.Label (allNodes [i].GetPath ());
//		}
//		for(int i = 0;i<transitionLines.Count;i++){
//			GUILayout.Label (transitionLines [i].startPos + " " + transitionLines [i].endPos);
//		}
	}
	void ShowTransition(NodeTransition currentTransition){
		try{
		GUILayout.Label ("From:" + FindNodeWithHashByAll (currentTransition.fromNodeHash).GetPath ());
		GUILayout.Label ("To  :" + FindNodeWithHashByAll (currentTransition.toNodeHash).GetPath ());
		}catch{
		}
	}
	void ShowNode(FSMNode currentnode){
		currentnode.name = EditorGUILayout.TextField(new GUIContent("Name"),currentnode.name);
		currentnode.infos [0].text = EditorGUILayout.TextArea (currentnode.infos [0].text);
	}
	#endregion
	protected override void SelectGroup (string path)
	{
		_scriptableObject.currentGroupPath = path;
		groupPaths = path;
		UpdateNodes ();
	}
	protected override void SelectGroup (Node nodegroup)
	{
			_scriptableObject.currentGroupPath = ((FSMNode)nodegroup).GetPath();
			groupPaths = _scriptableObject.currentGroupPath;
		UpdateNodes ();
	}
	void UpdateNodes(){
		nodes.Clear ();
			for (int i = 0; i < _scriptableObject.ShowNodes.Count; i++) {
				nodes .Add( _scriptableObject.ShowNodes[i]);
		}
			allNodes = _scriptableObject.nodes;
	}
	#region ContexMenu
	protected override void AddTranstition (Node fromNode, Node toNode)
	{
			_scriptableObject.AddTransition (fromNode, toNode);
		UpdateNodes ();
	}
	protected override void ContextMenu_Canvas (ref GenericMenu menu)
	{
		menu.AddItem (new GUIContent ("Add State"), false, delegate {
			AddState();
		});
		menu.AddItem (new GUIContent ("Add StateMachine"), false, delegate {
			AddStateMachine();
		});
	}
	protected override void ContextMenu_Node (ref GenericMenu menu)
	{
		menu.AddItem (new GUIContent ("Make Transtion"), false, delegate {
			MakeTranstion();
		});	
		menu.AddItem (new GUIContent ("Set Default State"), false, delegate {
			SetDefaultState();
		});	
		menu.AddItem (new GUIContent ("Dele State"), false, delegate {
			DeleState();
		});

	}
	protected override void ContextMenu_Transition (ref GenericMenu menu)
	{
		menu.AddItem (new GUIContent ("Dele Transition"), false, delegate {
			DeleTransition();
		});
	}
	protected override void ContextMenu_NodeGroud (ref GenericMenu menu)
	{
		menu.AddItem (new GUIContent ("Dele StateMachine"), false, delegate {
			string path = selectNodes[0].GetPath();
			for(int i = 0;i < allNodes.Length;i++){
				if(allNodes[i].GetPath().Contains(path)){
					DeleState(allNodes[i]);		
				}
			}
		});
	}
	protected override void ContextMenu_TransitionGroup (ref GenericMenu menu, Node fromNode,Node toGroup)
	{
			for(int i = 0;i<_scriptableObject.nodes.Length;i++){
				string path = _scriptableObject.nodes[i].GetPath();
			if (path.Contains(toGroup.GetPath()) && path != toGroup.GetPath()) {
				path = toGroup.name+path.Replace (toGroup.GetPath(),"");
				menu.AddItem (new GUIContent (path), false,delegate(object userData) {
						AddTranstition(fromNode,_scriptableObject.FindNodeWithHash((int)userData));
					},_scriptableObject.nodes[i].hash);
			}
		}
	}
	void MakeTranstion(){
		makeTranstion = true;
	}
	void AddState(){
			AddNode(_scriptableObject.AddNode (mousePosition-offset));
	}
	void AddStateMachine(){
			AddNode(_scriptableObject.AddNodeGroup (mousePosition-offset));
	}
	void DeleState(Node target){
		nodes.Remove (target);	
			_scriptableObject.RemoveNode ((FSMNode)target);
			startNode = _scriptableObject.startNode;
	}
	void DeleState(){
		for (int i = 0; i < selectNodes.Count; i++) {
			if (nodes.Contains (selectNodes [i])) {
				DeleState (selectNodes [i]);
				return;
			}
		}
	}
	void DeleTransition(){
			_scriptableObject.DeleTransition (FindNodeWithHashByAll (selectTransition.fromNodeHash),selectTransition);
	}
	void SetDefaultState(){
		startNode = selectNodes [0];
			_scriptableObject.SetDefaultState (startNode);
	}
	#endregion
}
}