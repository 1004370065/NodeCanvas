using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace NodeCanvas{
public class NodeEditor: EditorWindow {
	#region Member
	protected const float scale = 1;
	protected const int maxScale = 1000;

	protected virtual float _widthLeft{
		get{return 200;}
	}
	protected virtual float _widthRight{
		get{return 200;}
	}
	protected const float _miniNodeAreHight = 200;
	protected const float _miniNodeAreWidth = 200;
	protected const int _miniNodePointScale = 5;

	protected bool drawMainTool = true;
	protected bool drawLeftPanel = true;
	protected bool drawMiddlePanel = true;
	protected bool drawRightPanel = true;
	protected bool makeTranstion = false;
	bool drag = false;
	protected Event currentEvent;
	protected Vector2 mousePosition;
	protected Vector2 _offset;
	protected Vector2 offset{
		get{ return _offset; }
		set{ 
			_offset = value; 
			_offset.x = Mathf.Clamp (_offset.x,-maxScale,0);
			_offset.y = Mathf.Clamp (_offset.y,-maxScale,0);
		}
	}
	protected Node[] allNodes;
	protected List<Node> nodes = new List<Node>();
	protected List<Node> selectNodes = new List<Node>();
	protected List<DrawTransitionLine> transitionLines = new List<DrawTransitionLine>();
	protected Node startNode = null;
	protected NodeTransition selectTransition;
	protected string groupPaths = "";
	SelectType _selectType;
	protected SelectType selectType{
		get{ 
			if (_selectType == SelectType.Node || _selectType == SelectType.NodeGroud) {
				if (selectNodes.Count == 0)
					_selectType = SelectType.None;
			}
			return _selectType; 
		}
		set{ _selectType = value;
			Repaint ();
		}
	}
	private  Material material;

	NodeSelectionMode selectionMode;
	float GridMinorSize = 12f;
	float GridMajorSize = 120f;
	Vector2 selectionStartPosition;

	Rect scaledCanvasSize{
		get{
			return new Rect(0,0,Screen.width,Screen.height);
		}
	}
	Rect canvasRect{
		get{ return new Rect (0, 0, Screen.width - _widthLeft - _widthRight, Screen.height); }
	}
	protected virtual void UpdateOffset(){
		
	}
	#endregion
	[MenuItem("Assets/Node/Base NodeCanvas Window")]
	static void OnInit(){
		GetWindow<NodeEditor> ();
	}
	protected virtual void OnEnable(){
//		nodes.Add (new INode ());
//		nodes.Add (new INode ());
//		nodes [1].position.position += new Vector2 (300, 300);
//		nodes [0].AddTransition(nodes [1]);
	}	
	#region Draw
		protected virtual bool CanDraw{
			get{ return true; }
		}
	protected virtual void OnGUI(){

			OnDrawMainTool ();
		if (CanDraw) {
			OnDrawLeftPanel ();
			OnDrawMiddlePanelBegin ();
			DrawMiddlePanel ();

			OnDrawTransition ();
			OnDrawNode ();

			OnDrawMiddlePanelEnd ();
			OnDrawMiniNode ();

			OnDrawRightPanel ();
		}
		Repaint ();
	}
	#region Panel
	#region MainTool

	void OnDrawMainTool(){
		if (!drawMainTool)
			return;
		GUILayout.BeginHorizontal (EditorStyles.toolbar);
		EditorGUILayout.BeginHorizontal(GUILayout.Width(_widthLeft));
		DrawMainTool ();
		EditorGUILayout.EndHorizontal();
		DrawMainTool2 ();
		GUILayout.EndHorizontal ();
	}
	protected virtual void DrawMainTool(){
		GUILayout.Label ("xx");
	}
	protected virtual void DrawMainTool2(){
		if (groupPaths == "")
			groupPaths = "Base";
		string[] groups = groupPaths.Split('/');
		string path = "";
		for (int i = 0; i < groups.Length; i++) {
			path += groups [i];
			GUIStyle style=i==0?NodeStyles.breadcrumbLeft:NodeStyles.breadcrumbMiddle;
			GUIContent content = new GUIContent (groups[i]);
			float width = style.CalcSize (content).x;
			width = Mathf.Clamp (width, 80f, width);
			style.normal.textColor = i == groups.Length - 1 ? Color.black : Color.grey;
			if (GUILayout.Button (content, style, GUILayout.Width (width))) {
				SelectGroup (path);
			}
			path+= "/";
			style.normal.textColor = Color.white;
		}

		GUILayout.FlexibleSpace ();
	}
	#endregion
	#region LeftPanel
	void OnDrawLeftPanel(){
		if (!drawLeftPanel)
			return;
		GUILayout.BeginArea (new Rect(0,EditorStyles.toolbar.fixedHeight,_widthLeft,Screen.height-EditorStyles.toolbar.fixedHeight));
		DrawLeftPanel ();
		GUILayout.EndArea ();
	}
	protected virtual void DrawLeftPanel(){
		GUILayout.Label ("xx");
	}
	#endregion

	#region MiddlePanel
	void OnDrawMiniNode(){
		Rect minibox;
		minibox = new Rect (Screen.width - _widthRight - _miniNodeAreWidth, 18, _miniNodeAreWidth, _miniNodeAreHight);
			try{
		GUILayout.BeginArea (minibox);

		GUI.backgroundColor = new Color (1, 1, 1, 0.2f);
		GUI.Box (new Rect (0, 0, _miniNodeAreWidth, _miniNodeAreHight), "");
		GUI.backgroundColor = Color.white;

		Rect seer = canvasRect;
		float pw =0 ;
		float ph =0 ;
		pw = _miniNodeAreWidth / (maxScale + canvasRect.width);
		ph = _miniNodeAreWidth / (maxScale + canvasRect.height);

		seer.width = seer.width*pw;
		seer.height = seer.height*ph;
		seer.x = _offset.x*-1 *pw;
		seer.y = _offset.y*-1 * ph;
		GUI.Box (seer, "",NodeStyles.ControlHighlight);

		GUI.backgroundColor = Color.red;
		for (int i = 0; i < nodes.Count; i++) {
			Vector2 pointpos = nodes [i].position.position;
			Rect pointr = new Rect (pointpos.x * pw, pointpos.y * ph, _miniNodePointScale, _miniNodePointScale);
			GUI.Box (pointr, "");	
		}
		GUI.backgroundColor = Color.white;
		GUILayout.EndArea ();
		}catch{
		}
	}
	void OnDrawMiddlePanelBegin(){
		float widthleft = _widthLeft;
		float widthright = _widthRight;
		if (!drawLeftPanel)
			widthleft = 0;
		if (!drawRightPanel)
			widthright = 0;
		GUILayout.BeginArea (new Rect(_widthLeft,EditorStyles.toolbar.fixedHeight,Screen.width - widthleft - widthright,Screen.height-EditorStyles.toolbar.fixedHeight));
		currentEvent = Event.current;
		mousePosition = currentEvent.mousePosition;
		if (currentEvent.type == EventType.scrollWheel) {
			offset =new Vector2(offset.x,offset.y+ currentEvent.delta.y*10);
			UpdateOffset ();
			Event.current.Use();
		}
		if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 2) {
			offset+=currentEvent.delta;
			UpdateOffset ();
			Event.current.Use();
		}
		if (currentEvent.type == EventType.MouseUp &&  currentEvent.button == 1) {
			GenericMenu menu= new GenericMenu();
			switch (selectType) {
			case SelectType.Node:
				ContextMenu_Node (ref menu);	
				break;
			case SelectType.NodeGroud:
				ContextMenu_NodeGroud (ref menu);	
				break;
			case SelectType.None:
				ContextMenu_Canvas (ref menu);	
				break;
			case SelectType.Transition:
				ContextMenu_Transition (ref menu);	
				break;
			}
				
			menu.ShowAsContext ();
		}
		if (currentEvent.type == EventType.Repaint){
			NodeStyles.canvasBackground.Draw(scaledCanvasSize, false, false, false, false);
			DrawGrid ();
		}
	}
	void OnDrawMiddlePanelEnd(){
		GUILayout.EndArea ();
	}
	private void DrawGrid()
	{
		GL.PushMatrix();
		GL.Begin(1);
		this.DrawGridLines(scaledCanvasSize,GridMinorSize,offset, NodeStyles.gridMinorColor);
		this.DrawGridLines(scaledCanvasSize,GridMajorSize,offset, NodeStyles.gridMajorColor);
		GL.End();
		GL.PopMatrix();
	}
	private void DrawGridLines(Rect rect,float gridSize,Vector2 _offset, Color gridColor)
	{
		_offset *= scale;
		GL.Color(gridColor);
		for (float i = rect.x+(_offset.x<0f?gridSize:0f) + _offset.x % gridSize ; i < rect.x + rect.width; i = i + gridSize)
		{
			DrawLine(new Vector2(i, rect.y), new Vector2(i, rect.y + rect.height));
		}
		for (float j = rect.y+(_offset.y<0f?gridSize:0f) + _offset.y % gridSize; j < rect.y + rect.height; j = j + gridSize)
		{
			DrawLine(new Vector2(rect.x, j), new Vector2(rect.x + rect.width, j));
		}
	}
	private void DrawLine(Vector2 p1, Vector2 p2)
	{
		GL.Vertex(p1);
		GL.Vertex(p2);
	}
	protected virtual void DrawMiddlePanel(){
		
	}

	#endregion
	#region RightPanel
	void OnDrawRightPanel(){
		if (!drawLeftPanel)
			return;
			try{
		GUILayout.BeginArea (new Rect(Screen.width - _widthRight,EditorStyles.toolbar.fixedHeight,_widthRight,Screen.height-EditorStyles.toolbar.fixedHeight));
		DrawRightPanel ();
		GUILayout.EndArea ();
			}catch{
			}
	}
	protected virtual void DrawRightPanel(){
		GUILayout.Label ("right:"+offset);
		GUILayout.Label ("node:" + nodes.Count);
		GUILayout.Label ("selectType:" + selectType);
	}
	#endregion
	#endregion
	#region Node
	protected virtual void OnDrawNode (){
		for (int i = 0; i < nodes.Count; i++) {
			DoNode(nodes[i]);	
		}
		SelectNode ();
		DragNode ();
	}
	protected virtual int GetNodeColor(int id,Node node){
		return id;
	}
	protected virtual bool GetNodeStyle(bool value,Node node){
		return value;
	}
	protected virtual void DoNode(Node node){
		string nodename = node.name;
		int nodecolor = 0;
			if (node.name == "Base" && node.groupPath == "Base") {
			} else {
				if (node == startNode || node.GetPath () == groupPaths) {
					nodecolor = 5;
				}
				if (node.GetPath () == groupPaths) {
					nodecolor = 4;
					string[] namepath = node.GetPath ().Split ('/');
					nodename = "(Up) " + (namepath.Length > 1 ? namepath [namepath.Length - 2] : namepath [0]);
				}
				nodecolor = GetNodeColor (nodecolor, node);
				bool isGroupStyle;
				isGroupStyle = node.type == NodeType.NodeGroup;
				isGroupStyle = GetNodeStyle (isGroupStyle, node);
				GUIStyle style = NodeStyles.GetNodeStyle (nodecolor, selectNodes.Contains (node), isGroupStyle);
				Rect pos = new Rect ((offset.x + node.position.x) * scale, (offset.y + node.position.y) * scale, node.position.width * scale, node.position.height * scale);
				GUI.Box (pos, nodename, style);	
				Rect inforect = pos;
				float cacheY = inforect.y + node.position.height;
				for (int i = 0; i < node.infos.Length; i++) {
//					
					if (node.infos [i].text != "") {
						float h;
						inforect.y = cacheY;
						h = node.infos [i].text.Split ('\n').Length * 16;
						h = h == 16 ? 20 : h;
						inforect.height = h;
						GUILayout.BeginArea (inforect);
						try{
						GUILayout.TextArea (node.infos [i].text);
						}catch{
						}
						
						GUILayout.EndArea ();
						
						cacheY += inforect.width;
					}
//					
				}
			}
	}
	void SelectNode(){
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		if (!canvasRect.Contains (mousePosition)) {
			return;
		}
		switch (currentEvent.rawType) {
		case EventType.MouseDown:
			if (currentEvent.button == 2)
				return;
			selectType = SelectType.None;
			if (makeTranstion) {
				Node node = MouseOverNode ();
				if (node != null && node != selectNodes [0]) {
					if (node.type == NodeType.Node) {
						AddTranstition (selectNodes [0], node);
					} else {
						TransitionGroup (selectNodes [0], node);
					}
				}
				makeTranstion = false;
				selectNodes.Clear ();

			} else {
				//Select Node
				GUIUtility.hotControl = controlID;
				selectionStartPosition = mousePosition;
				selectTransition = null;
				Node node = MouseOverNode ();
				if (node != null) {
					if (selectNodes.Count > 0 && node == selectNodes [0] && currentEvent.clickCount == 2 && node.type == NodeType.NodeGroup) {
						if (node.GetPath () != groupPaths)
							//double
							SelectGroup (node);
						else {
							SelectGroupUp (node);
						}
					} else {
						selectNodes.Clear ();
						selectNodes.Add (node);
						GUIUtility.hotControl = 0;
						GUIUtility.keyboardControl = 0;
						if (node.type == NodeType.Node)
							selectType = SelectType.Node;
						else
							selectType = SelectType.NodeGroud;
					}
				} else {
					for (int i = 0; i < transitionLines.Count; i++) {
						if (CheckPointInLine (transitionLines[i].startPos,transitionLines[i].endPos, mousePosition, 1)) {
							selectTransition = transitionLines[i].line;
							selectType = SelectType.Transition;
						}
					}
//					for(int i = 0;i < nodes.Count;i++){
//						for (int x = 0; x < nodes [i].transition.Length; x++) {
//							NodeTransition tr = nodes [i].transition [x];
//							Vector2 p1 = FindNodeWithHashByAll (tr.fromNode).position.center + offset;
//							Vector2 p2 = FindNodeWithHashByAll (tr.toNode).position.center + offset;
//
//
//							if (CheckPointInLine (p1,p2, mousePosition, 1)) {
//								selectTransition = tr;
//								selectType = SelectType.Transition;
//							}
//						}
//					}
				}
				selectionMode = NodeSelectionMode.Pick;
				//Select Trasnstion
				

			}
			break;
		case EventType.MouseUp:
			drag = false;
			if(GUIUtility.hotControl == controlID){
				selectionMode = NodeSelectionMode.None;
				GUIUtility.hotControl = 0;
				currentEvent.Use ();
			}
			break;
		case EventType.MouseDrag:
			selectTransition = null;
			if (GUIUtility.hotControl == controlID && !EditorGUI.actionKey && !currentEvent.shift && (selectionMode == NodeSelectionMode.Pick || selectionMode == NodeSelectionMode.Rect)) {
				selectionMode = NodeSelectionMode.Rect;	
				SelectNodesInRect (FromToRect (selectionStartPosition, mousePosition));
				selectType = selectNodes.Count>1?SelectType.NodeGroud:SelectType.Node; 
				currentEvent.Use ();
			}
			break;
		case EventType.Repaint:
			if (GUIUtility.hotControl == controlID && selectionMode == NodeSelectionMode.Rect) {
				NodeStyles.selectionRect.Draw(FromToRect(selectionStartPosition, mousePosition), false, false, false, false);		
			}
			break;
		}
	}
	protected virtual void SelectGroup(string path){
	}
	protected virtual void SelectGroup(Node nodegroup){
		
	}
	protected virtual void SelectGroupUp (Node nodegroup){
		SelectGroup (nodegroup.groupPath);
	}
	void DragNode(){
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		switch (currentEvent.rawType) {
		case EventType.MouseDown:
			if (MouseOverNode () != null) {
				GUIUtility.hotControl = controlID;
				currentEvent.Use ();
			}
			break;
		case EventType.MouseUp:
			if (GUIUtility.hotControl == controlID)
			{
				GUIUtility.hotControl = 0;
				currentEvent.Use();
			}
			break;
		case EventType.MouseDrag:
			if (GUIUtility.hotControl == controlID)
			{
				for(int i=0;i< selectNodes.Count;i++){
					Node node=selectNodes[i];
					node.position.position+= currentEvent.delta;
				}
			}
			break;
		case EventType.Repaint:
			break;
		}
	}
	Node MouseOverNode(){
		for (int i = 0; i < nodes.Count; i++) {
			if (nodes[i].position.Contains (currentEvent.mousePosition - offset)) {
				return nodes [i];
			}
		}
		return null;
	}
	void SelectNodesInRect(Rect r){
		r.position -= offset;
		for(int i=0;i< nodes.Count;i++){
			Node node=nodes[i];
			Rect rect = node.position;
			if ( rect.xMax < r.x || rect.x > r.xMax || rect.yMax < r.y || rect.y > r.yMax)
			{
				selectNodes.Remove(node);
				continue;
			}
			if(!selectNodes.Contains(node)){
				selectNodes.Add(node);
			}
		}
	}
	Rect FromToRect(Vector2 start, Vector2 end)
	{
		Rect rect = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);
		if (rect.width < 0f)
		{
			rect.x = rect.x + rect.width;
			rect.width = -rect.width;
		}
		if (rect.height < 0f)
		{
			rect.y = rect.y + rect.height;
			rect.height = -rect.height;
		}
		return rect;
	}
	bool CheckPointInLine(Vector2 p1,Vector2 p2,Vector2 point,float offset){
		float d1 = Vector2.Distance (p1, point);
		float d2 = Vector2.Distance (p2, point);
		float d3 = Vector2.Distance (p1, p2) + offset;
		if (d1 + d2 < d3) {
			return true;
		}
		return false;
	}
	#endregion
	#region Transitipn
	protected virtual void AddTranstition(Node fromNode,Node toNode){
		fromNode.AddTransition (toNode);
	}
	void TransitionGroup(Node fromNode,Node toNode){
		GenericMenu menu= new GenericMenu();
		ContextMenu_TransitionGroup (ref menu,fromNode,toNode);
		menu.ShowAsContext ();
	}
	void OnDrawTransition(){
		transitionLines.Clear ();
		if (allNodes == null)
			return;
		for (int i = 0; i < allNodes.Length; i++) {
			var groups = allNodes [i].transition.GroupBy(c=>c.toNodeHash).ToList();
			foreach(var group in groups){   
				int fromid = group.First ().fromNodeHash;
				int toid = group.First ().toNodeHash;
				Node toNode=FindNodeWithHashByAll(toid);
				Node fromNode=FindNodeWithHashByAll(fromid);

				if (FindNodeWithHash (toid) == null && FindNodeWithHash (fromid) == null) {
					continue;
				}
				if (toNode == null || fromNode == null) {
					continue;
				}
				int arrowCount=group.Count() > 1 ? 3:1;
//				bool offset=toNode.Transitions.Any(x=>x.ToNode == fromNode);
				bool _offset = false;
				Color color=group.Any(x=>x == selectTransition)?Color.cyan:Color.white;

				if (toNode.groupPath == fromNode.groupPath) {
					DrawConnection (fromNode.position.center + offset, toNode.position.center + offset, color, arrowCount, _offset); 	
					transitionLines.Add (new DrawTransitionLine (group.First (), fromNode.position.center + offset, toNode.position.center + offset));
				} else {
					if (FindNodeWithHash (group.First ().toNodeHash) == null) {
						string path = toNode.GetPath ();
						int count = 0;
						while (Path.GetDirectoryName (path) != fromNode.groupPath && path != "" && count < 10) {
							path = Path.GetDirectoryName (path);
							count++;
						}
						toNode = FindNodeWithPath (path);
						if (toNode != null) {
							DrawConnection (fromNode.position.center + offset, toNode.position.center + offset, color, arrowCount, _offset); 	
							transitionLines.Add (new DrawTransitionLine (group.First (), fromNode.position.center + offset, toNode.position.center + offset));
						}
					} else {
						fromNode = FindNodeWithPath (toNode.groupPath);
						DrawConnection (fromNode.position.center + offset, toNode.position.center + offset, color, arrowCount, _offset); 	
						transitionLines.Add (new DrawTransitionLine (group.First (), fromNode.position.center + offset, toNode.position.center + offset));
					}
				}
			}
		}
		if(makeTranstion){
			if (selectNodes.Count == 0)
				makeTranstion = false;
			else
				DrawConnection(selectNodes[0].position.center+offset,mousePosition,Color.white,1,false); 
		}
	}
	protected virtual void DoTranstition(NodeTransition transition){
		Color color = transition == selectTransition?Color.blue: Color.white;
		try{
			DrawConnection(FindNodeWithHashByAll(transition.fromNodeHash).position.center+offset,FindNodeWithHashByAll(transition.toNodeHash).position.center+offset,color,1,false); 
		}catch{
		}
	}
	void DrawConnection (Vector3 start, Vector3 end,Color color, int arrows,bool offset)
	{
		if (currentEvent.type != EventType.repaint) {
			return;
		}
		Handles.color = color;
		Handles.DrawBezier (start,end,start,end,color,NodeStyles.connectionTexture,3f);
		float dis = Vector2.Distance (end, start);
		Vector3 nor = (end - start).normalized;
		Vector3 cross = nor * (dis*0.2f)+start;
		Quaternion q = Quaternion.FromToRotation (Vector3.back, start - end);
		cross.z = -100;
//		Handles.Label (cross, arrows.ToString());
		for (int i=0; i<arrows; i++) {
			cross = nor * (dis*(0.5f+i*20/dis))+start;
			cross.z = -100;
			Handles.ConeCap(i,cross,q,18);
		}
	}

	protected Node FindNodeWithHashByAll(int id){
		for(int i = 0;i < allNodes.Length;i++){
			if (allNodes[i].hash == id)
				return allNodes[i];
		}
		return null;
	}
	protected Node FindNodeWithHash(int id){
		for(int i = 0;i < nodes.Count;i++){
			if (nodes [i].hash == id)
				return nodes [i];
		}
		return null;
	}
	protected Node FindNodeWithPath(string path){
		for (int i = 0; i < nodes.Count; i++) {
			if (nodes [i].GetPath () == path)
				return nodes [i];
		}
		return null;
	}
	#endregion
	#endregion
	#region ContextMenu
	protected virtual void ContextMenu_Canvas(ref GenericMenu menu){
		menu.AddItem (new GUIContent("点击画布"),false,delegate {
			Debug.Log("点击画布");
		});
	}
	protected virtual void ContextMenu_Node(ref GenericMenu menu){
		menu.AddItem (new GUIContent("点击节点"),false,delegate {
			Debug.Log("点击节点");
		});
	}
	protected virtual void ContextMenu_NodeGroud(ref GenericMenu menu){
		menu.AddItem (new GUIContent ("点击节点组"), false, delegate {
			Debug.Log ("点击节点组");
		});
	}
	protected virtual void ContextMenu_Transition(ref GenericMenu menu){
		menu.AddItem (new GUIContent("点击状态"),false,delegate {
			Debug.Log("点击状态");
		});
	}
	protected virtual void ContextMenu_TransitionGroup(ref GenericMenu menu,Node fromNode,Node toGroup){
		menu.AddItem (new GUIContent("连线到Group"),false,delegate {
			Debug.Log("连线到Group");
		});
	}
	#endregion
	protected Node AddNode (Node node){
		nodes.Add(node);
		return node;
	}
}

}