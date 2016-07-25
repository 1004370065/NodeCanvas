using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace NodeCanvas{
	public class NodeEditor: EditorWindow {
		#region Member
		/// <summary>
		/// 本来想做为Node 大小缩放的,暂时没有开放
		/// </summary>
		protected const float _scale = 1;
		/// <summary>
		/// 画布尺寸
		/// </summary>
		/// <value>The max scale.</value>
		protected virtual int _maxCanvasScale{
			get{return 1000;}
		}
		/// <summary>
		/// 左侧面板宽度
		/// </summary>
		/// <value>The width of the left panel.</value>
		protected virtual float _leftPanelWidth{
			get{ return 200; }
		}
		/// <summary>
		/// 右侧面板宽度
		/// </summary>
		/// <value>The width of the right panel.</value>
		protected virtual float _rightPanelWidth{
			get{ return 200; }
		}
		/// <summary>
		/// 面板上绘制的白字
		/// </summary>
		/// <value>The name of the current canvas.</value>
		protected virtual string _currentCanvasName{
			get{ return "Base Canvas"; }
		}
		/// <summary>
		/// 缩略图高度
		/// </summary>
		protected virtual float _miniNodeAreHight {
			get{ return  200; }
		}
		/// <summary>
		/// 缩略图宽度
		/// </summary>
		protected virtual float _miniNodeAreWidth {
			get{ return 200; }
		}
		/// <summary>
		/// 缩略图中间点的大小
		/// </summary>
		protected int _miniNodePointScale = 5;
		/// <summary>
		/// 是否可以绘制，
		/// 如果有什么其他条件要判断的话
		/// </summary>
		/// <value><c>true</c> if this instance can draw; otherwise, <c>false</c>.</value>
		protected virtual bool CanDraw{
			get{ return true; }
		}
		/// <summary>
		/// 是否绘制顶上主菜单
		/// </summary>
		/// <value><c>true</c> if draw main tool; otherwise, <c>false</c>.</value>
		protected virtual bool _drawMainTool {
			get{ return true; }
		}
		/// <summary>
		/// 是否绘制左侧面板
		/// </summary>
		/// <value><c>true</c> if draw left panel; otherwise, <c>false</c>.</value>
		protected virtual bool _drawLeftPanel {
			get{ return true; }
		}
		/// <summary>
		/// 是否绘制中间面板
		/// </summary>
		/// <value><c>true</c> if draw middle panel; otherwise, <c>false</c>.</value>
		protected virtual bool _drawMiddlePanel {
			get{ return true; }
		}
		/// <summary>
		/// 是否绘制右侧面板
		/// </summary>
		/// <value><c>true</c> if draw right panel; otherwise, <c>false</c>.</value>
		protected virtual bool _drawRightPanel {
			get{ return true; }
		}
		/// <summary>
		/// 如果当选中的Node 希望链接到另一个Node 的时候 确保打开MakeTransitoin
		/// </summary>
		/// <value><c>true</c> if make transtion; otherwise, <c>false</c>.</value>
		protected virtual bool _makeTranstion {
			get{ return __makeTransiton; }
			set{ __makeTransiton = value; }
		}
		/// <summary>
		/// 能否拖动节点，
		/// 如果需要自定义节点布局，请关闭这个
		/// </summary>
		/// <value><c>true</c> if can drag node; otherwise, <c>false</c>.</value>
		protected virtual bool _canDragNode{
			get{ return true; }
		}
		/// <summary>
		/// 当前事件
		/// </summary>
		protected Event _currentEvent;
		/// <summary>
		/// 当前鼠标位置
		/// </summary>
		protected Vector2 _cachedMousePosition;
		/// <summary>
		/// 画布偏移位置
		/// </summary>
		/// <value>The offset.</value>
		protected Vector2 _canvasOffset{
			get{ return _offset; }
			set{ 
				_offset = value; 
				_offset.x = Mathf.Clamp (_offset.x,-_maxCanvasScale,0);
				_offset.y = Mathf.Clamp (_offset.y,-_maxCanvasScale,0);
			}
		}
		/// <summary>
		/// 当前画布下所有的节点
		/// 包含所有NodeGroup以及NodeGroup下的东西
		/// </summary>
		protected Node[] allNodes;
		/// <summary>
		/// 显示中的物体
		/// </summary>
		protected List<Node> _nodes = new List<Node>();
		/// <summary>
		/// 所有选择的Node
		/// </summary>
		protected List<Node> _selectNodes = new List<Node>();
		/// <summary>
		/// 当前选择的第一个Node
		/// </summary>
		/// <value>The select node.</value>
		protected Node _selectNode{
			get{
				return _selectNodes.Count > 0 ? _selectNodes [0] : null;
			}
		}
		/// <summary>
		/// 当前Group下的所有状态过渡线
		/// </summary>
		protected List<DrawTransitionLine> _transitionLines = new List<DrawTransitionLine>();
		/// <summary>
		/// 选择的状态过度线
		/// </summary>
		protected NodeTransition _selectTransition;
		/// <summary>
		/// 类似Animator的默认状态，颜色会改变
		/// </summary>
		protected Node _startNode = null;
		/// <summary>
		/// 当前GroupPath
		/// </summary>
		protected string _currentGroupPath = "";
		/// <summary>
		/// 当前选择的东西
		/// n没有选择,选Node,选NodeGroup,选过渡线
		/// </summary>
		/// <value>The type of the select.</value>
		protected SelectType _selectType{
			get{ 
				if (__selectType == SelectType.Node || __selectType == SelectType.NodeGroud) {
					if (_selectNodes.Count == 0)
						__selectType = SelectType.None;
				}
				return __selectType; 
			}
			set{ __selectType = value;
				Repaint ();
			}
		}

		float GridMinorSize = 12f;
		float GridMajorSize = 120f;
		bool _draging = false;
		bool __makeTransiton= false;
		Vector2 _offset;
		Vector2 selectionStartPosition;
		SelectType __selectType;
		NodeSelectionMode selectionMode;
		Rect scaledCanvasSize{
			get{
				return new Rect(0,0,Screen.width,Screen.height);
			}
		}
		Rect canvasRect{
			get{ return new Rect (0, 0, Screen.width - _leftPanelWidth - _rightPanelWidth, Screen.height); }
		}


		#endregion
		#region Draw
		[MenuItem("Assets/Node/Base NodeCanvas Window")]
		static void OnInit(){
			GetWindow<NodeEditor> ();
		}

		void OnGUI(){
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

		void OnDrawMainTool(){
			if (!_drawMainTool)
				return;
			GUILayout.BeginHorizontal (EditorStyles.toolbar);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(_leftPanelWidth));
			DrawMainTool ();
			EditorGUILayout.EndHorizontal();
			DrawMainTool2 ();
			GUILayout.EndHorizontal ();

		}

		void OnDrawLeftPanel(){
			if (!_drawLeftPanel)
				return;
			GUILayout.BeginArea (new Rect(0,EditorStyles.toolbar.fixedHeight,_leftPanelWidth,Screen.height-EditorStyles.toolbar.fixedHeight));
			DrawLeftPanel ();
			GUILayout.EndArea ();
		}

		void OnDrawMiniNode(){
			Rect minibox;

			GUI.Label (new Rect(_leftPanelWidth,20,140,50),_currentCanvasName,(GUIStyle)"LODLevelNotifyText");
			minibox = new Rect (Screen.width - _rightPanelWidth - _miniNodeAreWidth, 18, _miniNodeAreWidth, _miniNodeAreHight);
				try{
			GUILayout.BeginArea (minibox);

			GUI.backgroundColor = new Color (1, 1, 1, 0.2f);
			GUI.Box (new Rect (0, 0, _miniNodeAreWidth, _miniNodeAreHight), "");
			GUI.backgroundColor = Color.white;

			Rect seer = canvasRect;
			float pw =0 ;
			float ph =0 ;
			pw = _miniNodeAreWidth / (_maxCanvasScale + canvasRect.width);
			ph = _miniNodeAreWidth / (_maxCanvasScale + canvasRect.height);

			seer.width = seer.width*pw;
			seer.height = seer.height*ph;
			seer.x = _offset.x*-1 *pw;
			seer.y = _offset.y*-1 * ph;

			GUI.backgroundColor = Color.red;
			for (int i = 0; i < _nodes.Count; i++) {
				Vector2 pointpos = _nodes [i].position.position;
				Rect pointr = new Rect (pointpos.x * pw, pointpos.y * ph, _miniNodePointScale, _miniNodePointScale);
				GUI.Box (pointr, "");	
			}
			GUI.backgroundColor = Color.white;

			GUI.Box (seer, "",NodeStyles.ControlHighlight);
			GUILayout.EndArea ();
			}catch{
			}
		}
		void OnDrawMiddlePanelBegin(){
			float widthleft = _leftPanelWidth;
			float widthright = _rightPanelWidth;
			if (!_drawLeftPanel)
				widthleft = 0;
			if (!_drawRightPanel)
				widthright = 0;
			GUILayout.BeginArea (new Rect(_leftPanelWidth,EditorStyles.toolbar.fixedHeight,Screen.width - widthleft - widthright,Screen.height-EditorStyles.toolbar.fixedHeight));
			_currentEvent = Event.current;
			_cachedMousePosition = _currentEvent.mousePosition;
			if (_currentEvent.type == EventType.scrollWheel) {
				_canvasOffset =new Vector2(_canvasOffset.x,_canvasOffset.y+ _currentEvent.delta.y*-10);
				UpdateOffset ();
				Event.current.Use();
			}
			if (_currentEvent.type == EventType.MouseDrag && _currentEvent.button == 2) {
				_canvasOffset+=_currentEvent.delta;
				UpdateOffset ();
				Event.current.Use();
			}
			if (_currentEvent.type == EventType.MouseUp &&  _currentEvent.button == 1) {
				GenericMenu menu= new GenericMenu();
				switch (_selectType) {
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
			if (_currentEvent.type == EventType.Repaint){
				NodeStyles.canvasBackground.Draw(scaledCanvasSize, false, false, false, false);
				DrawGrid ();
			}
		}
		void OnDrawMiddlePanelEnd(){
			GUILayout.EndArea ();
		}
		void DrawGrid()
		{
			GL.PushMatrix();
			GL.Begin(1);
			this.DrawGridLines(scaledCanvasSize,GridMinorSize,_canvasOffset, NodeStyles.gridMinorColor);
			this.DrawGridLines(scaledCanvasSize,GridMajorSize,_canvasOffset, NodeStyles.gridMajorColor);
			GL.End();
			GL.PopMatrix();
		}
		void DrawGridLines(Rect rect,float gridSize,Vector2 _offset, Color gridColor)
		{
			_offset *= _scale;
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
		void DrawLine(Vector2 p1, Vector2 p2)
		{
			GL.Vertex(p1);
			GL.Vertex(p2);
		}
			
		void OnDrawRightPanel(){
			if (!_drawLeftPanel)
				return;
			try {
				GUILayout.BeginArea (new Rect (Screen.width - _rightPanelWidth, EditorStyles.toolbar.fixedHeight, _rightPanelWidth, Screen.height - EditorStyles.toolbar.fixedHeight));
				DrawRightPanel ();
				GUILayout.EndArea ();
			} catch {
			}
		}
			
		void SelectNode(){
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			if (!canvasRect.Contains (_cachedMousePosition)) {
				return;
			}
			switch (_currentEvent.rawType) {
			case EventType.MouseDown:
				if (_currentEvent.button == 2)
					return;
				_selectType = SelectType.None;
				if (_makeTranstion) {
					Node node = MouseOverNode ();
					if (node != null && node != _selectNodes [0]) {
						if (node.type == NodeType.Node) {
							AddTranstition (_selectNodes [0], node);
						} else {
							TransitionGroup (_selectNodes [0], node);
						}
					}
					_makeTranstion = false;
					_selectNodes.Clear ();

				} else {
					//Select Node
					GUIUtility.hotControl = controlID;
					selectionStartPosition = _cachedMousePosition;
					_selectTransition = null;
					Node node = MouseOverNode ();
					if (node != null) {
						if (_selectNodes.Count > 0 && node == _selectNodes [0] && _currentEvent.clickCount == 2 && node.type == NodeType.NodeGroup) {
							if (node.GetPath () != _currentGroupPath)
								//double
								SelectGroup (node);
							else {
								SelectGroupUp (node);
							}
						} else {
							_selectNodes.Clear ();
							_selectNodes.Add (node);
							GUIUtility.hotControl = 0;
							GUIUtility.keyboardControl = 0;
							if (node.type == NodeType.Node)
								_selectType = SelectType.Node;
							else
								_selectType = SelectType.NodeGroud;
						}
					} else {
						for (int i = 0; i < _transitionLines.Count; i++) {
							if (CheckPointInLine (_transitionLines[i].startPos,_transitionLines[i].endPos, _cachedMousePosition, 1)) {
								_selectTransition = _transitionLines[i].line;
								_selectType = SelectType.Transition;
							}
						}

					}
					selectionMode = NodeSelectionMode.Pick;
					//Select Trasnstion
					

				}
				break;
			case EventType.MouseUp:
				_draging = false;
				if(GUIUtility.hotControl == controlID){
					selectionMode = NodeSelectionMode.None;
					GUIUtility.hotControl = 0;
					_currentEvent.Use ();
				}
				break;
			case EventType.MouseDrag:
				_selectTransition = null;
				if (GUIUtility.hotControl == controlID && !EditorGUI.actionKey && !_currentEvent.shift && (selectionMode == NodeSelectionMode.Pick || selectionMode == NodeSelectionMode.Rect)) {
					selectionMode = NodeSelectionMode.Rect;	
					SelectNodesInRect (FromToRect (selectionStartPosition, _cachedMousePosition));
					_selectType = _selectNodes.Count>1?SelectType.NodeGroud:SelectType.Node; 
					_currentEvent.Use ();
				}
				break;
			case EventType.Repaint:
				if (GUIUtility.hotControl == controlID && selectionMode == NodeSelectionMode.Rect) {
					NodeStyles.selectionRect.Draw(FromToRect(selectionStartPosition, _cachedMousePosition), false, false, false, false);		
				}
				break;
			}
		}
		void DragNode(){
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			switch (_currentEvent.rawType) {
			case EventType.MouseDown:
				if (MouseOverNode () != null) {
					GUIUtility.hotControl = controlID;
					_currentEvent.Use ();
				}
				break;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == controlID)
				{
					GUIUtility.hotControl = 0;
					_currentEvent.Use();
				}
				break;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == controlID)
				{
					if (_canDragNode) {
						for (int i = 0; i < _selectNodes.Count; i++) {
							Node node = _selectNodes [i];
							node.position.position += _currentEvent.delta;
						}
					}
				}
				break;
			case EventType.Repaint:
				break;
			}
		}
		void SelectNodesInRect(Rect r){
			r.position -= _canvasOffset;
			for(int i=0;i< _nodes.Count;i++){
				Node node=_nodes[i];
				Rect rect = node.position;
				if ( rect.xMax < r.x || rect.x > r.xMax || rect.yMax < r.y || rect.y > r.yMax)
				{
					_selectNodes.Remove(node);
					continue;
				}
				if(!_selectNodes.Contains(node)){
					_selectNodes.Add(node);
				}
			}
		}
			
		void TransitionGroup(Node fromNode,Node toNode){
			GenericMenu menu= new GenericMenu();
			ContextMenu_TransitionGroup (ref menu,fromNode,toNode);
			menu.ShowAsContext ();
		}
		void OnDrawTransition(){
			_transitionLines.Clear ();
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
					Color color=group.Any(x=>x == _selectTransition)?Color.cyan:Color.white;

					if (toNode.groupPath == fromNode.groupPath) {
						DrawConnection (fromNode.position.center + _canvasOffset, toNode.position.center + _canvasOffset, color, arrowCount, _offset); 	
						_transitionLines.Add (new DrawTransitionLine (group.First (), fromNode.position.center + _canvasOffset, toNode.position.center + _canvasOffset));
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
								DrawConnection (fromNode.position.center + _canvasOffset, toNode.position.center + _canvasOffset, color, arrowCount, _offset); 	
								_transitionLines.Add (new DrawTransitionLine (group.First (), fromNode.position.center + _canvasOffset, toNode.position.center + _canvasOffset));
							}
						} else {
							fromNode = FindNodeWithPath (toNode.groupPath);
							DrawConnection (fromNode.position.center + _canvasOffset, toNode.position.center + _canvasOffset, color, arrowCount, _offset); 	
							_transitionLines.Add (new DrawTransitionLine (group.First (), fromNode.position.center + _canvasOffset, toNode.position.center + _canvasOffset));
						}
					}
				}
			}
			if(_makeTranstion){
				if (_selectNodes.Count == 0)
					_makeTranstion = false;
				else
					DrawConnection(_selectNodes[0].position.center+_canvasOffset,_cachedMousePosition,Color.white,1,false); 
			}
		}
		void DrawConnection (Vector3 start, Vector3 end,Color color, int arrows,bool offset)
		{
			if (_currentEvent.type != EventType.repaint) {
				return;
			}
			Handles.color = color;
			Handles.DrawBezier (start,end,start,end,color,NodeStyles.connectionTexture,3f);
			float dis = Vector2.Distance (end, start);
			Vector3 nor = (end - start).normalized;
			Vector3 cross = nor * (dis*0.2f)+start;
			Quaternion q = Quaternion.FromToRotation (Vector3.back, start - end);
			cross.z = -100;
			for (int i=0; i<arrows; i++) {
				cross = nor * (dis*(0.5f+i*20/dis))+start;
				cross.z = -100;
				Handles.ConeCap(i,cross,q,18);
			}
		}
		#endregion
		#region 所有右键上下文
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
		#region 重载
		protected virtual void UpdateOffset(){

		}
		protected virtual void OnEnable(){

		}	
		/// <summary>
		/// 绘制菜单栏1
		/// </summary>
		protected virtual void DrawMainTool(){
			GUILayout.Label ("xx");
		}
		/// <summary>
		/// 绘制菜单栏2 
		/// 默认是节点组路径图
		/// </summary>
		protected virtual void DrawMainTool2(){
			if (_currentGroupPath == "")
				_currentGroupPath = "Base";
			string[] groups = _currentGroupPath.Split('/');
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
		/// <summary>
		/// 绘制左侧菜单栏
		/// </summary>
		protected virtual void DrawLeftPanel(){
			GUILayout.Label ("xx");
		}
		/// <summary>
		/// 绘制中间菜单栏
		/// </summary>
		protected virtual void DrawMiddlePanel(){

		}
		/// <summary>
		/// 绘制右边菜单栏
		/// </summary>
		protected virtual void DrawRightPanel(){
			GUILayout.Label ("right:"+_canvasOffset);
			GUILayout.Label ("node:" + _nodes.Count);
			GUILayout.Label ("selectType:" + _selectType);
		}
		/// <summary>
		/// 准备绘制所有节点
		/// ONGui调用
		/// </summary>
		protected virtual void OnDrawNode (){
			for (int i = 0; i < _nodes.Count; i++) {
				DoNode(_nodes[i]);	
			}
			SelectNode ();
			DragNode ();
		}
		/// <summary>
		/// 开始绘制节点
		/// </summary>
		/// <param name="node">Node.</param>
		protected virtual void DoNode(Node node){
			Rect rect = node.position;
			Rect nowCanvasR = canvasRect;
			nowCanvasR.position = -_canvasOffset;
			if (rect.xMax < nowCanvasR.x || rect.x > nowCanvasR.xMax || rect.yMax < nowCanvasR.y - nowCanvasR.height || rect.y > nowCanvasR.yMax) {
				return;
			}

			string nodename = node.DrawNodeName;
			int nodecolor = 0;
			if (node.name == "Base" && node.groupPath == "Base") {
			} else {
				if (node == _startNode || node.GetPath () == _currentGroupPath) {
					nodecolor = 5;
				}
				if (node.GetPath () == _currentGroupPath) {
					nodecolor = 4;
					string[] namepath = node.GetPath ().Split ('/');
					nodename = "(Up) " + (namepath.Length > 1 ? namepath [namepath.Length - 2] : namepath [0]);
				}
				nodecolor = GetNodeColor (nodecolor, node);
				bool isGroupStyle;
				isGroupStyle = node.type == NodeType.NodeGroup;
				isGroupStyle = GetNodeStyle (isGroupStyle, node);
				GUIStyle style = NodeStyles.GetNodeStyle (nodecolor, _selectNodes.Contains (node), isGroupStyle);
				Rect pos = new Rect ((_canvasOffset.x + node.position.x) * _scale, (_canvasOffset.y + node.position.y) * _scale, node.position.width * _scale, node.position.height * _scale);
				GUI.Box (pos, nodename, style);	
				if (node.ShowNodeProgressBar) {
					Rect progress = pos;
					progress.height = 10;
					progress.y += 20;
					EditorGUI.ProgressBar (progress, node.nodeProgressBar, "");
				}
				Rect inforect = pos;
				float cacheY = inforect.y + node.position.height;
				for (int i = 0; i < node.infos.Length; i++) {
					if (node.infos [i].text != "") {
						float h;
						inforect.y = cacheY;
						h = node.infos [i].text.Split ('\n').Length * 16;
						h = h == 16 ? 20 : h;
						inforect.height = h;
						GUILayout.BeginArea (inforect);
						try {
							GUILayout.TextArea (node.infos [i].text);
						} catch {
						}

						GUILayout.EndArea ();

						cacheY += inforect.width;
					}
				}
			}
		}
		/// <summary>
		/// 双击状态组之后
		/// </summary>
		/// <param name="path">Path.</param>
		protected virtual void SelectGroup(string path){
		}
		/// <summary>
		/// 双击状态组之后
		/// </summary>
		/// <param name="path">Path.</param>
		protected virtual void SelectGroup(Node nodegroup){

		}
		/// <summary>
		/// 返回状态组
		/// </summary>
		/// <param name="path">Path.</param>
		protected virtual void SelectGroupUp (Node nodegroup){
			SelectGroup (nodegroup.groupPath);
		}
		/// <summary>
		/// 添加一条过渡线
		/// </summary>
		/// <param name="fromNode">From node.</param>
		/// <param name="toNode">To node.</param>
		protected virtual void AddTranstition(Node fromNode,Node toNode){
			fromNode.AddTransition (toNode);
		}
		/// <summary>
		/// 开始绘制一条过渡线
		/// </summary>
		/// <param name="transition">Transition.</param>
		protected virtual void DoTranstition(NodeTransition transition){
			Color color = transition == _selectTransition?Color.blue: Color.white;
			try{
				DrawConnection(FindNodeWithHashByAll(transition.fromNodeHash).position.center+_canvasOffset,FindNodeWithHashByAll(transition.toNodeHash).position.center+_canvasOffset,color,1,false); 
			}catch{
			}
		}

		/// <summary>
		/// 获取节点颜色
		/// </summary>
		/// <returns>The node color.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="node">Node.</param>
		protected virtual int GetNodeColor(int id,Node node){
			return id;
		}
		/// <summary>
		/// 获取节点图
		/// </summary>
		/// <returns><c>true</c>, if node style was gotten, <c>false</c> otherwise.</returns>
		/// <param name="value">If set to <c>true</c> value.</param>
		/// <param name="node">Node.</param>
		protected virtual bool GetNodeStyle(bool value,Node node){
			return value;
		}
		#endregion
		#region 一些常用方法
		protected Node FindNodeWithHashByAll(int id){
			for(int i = 0;i < allNodes.Length;i++){
				if (allNodes[i].hash == id)
					return allNodes[i];
			}
			return null;
		}
		protected Node FindNodeWithHash(int id){
			for(int i = 0;i < _nodes.Count;i++){
				if (_nodes [i].hash == id)
					return _nodes [i];
			}
			return null;
		}
		protected Node FindNodeWithPath(string path){
			for (int i = 0; i < _nodes.Count; i++) {
				if (_nodes [i].GetPath () == path)
					return _nodes [i];
			}
			return null;
		}
		/// <summary>
		/// 返回鼠标对应的节点
		/// </summary>
		/// <returns>The over node.</returns>
		protected Node MouseOverNode(){
			for (int i = 0; i < _nodes.Count; i++) {
				if (_nodes[i].position.Contains (_currentEvent.mousePosition - _canvasOffset)) {
					return _nodes [i];
				}
			}
			return null;
		}
		protected Rect FromToRect(Vector2 start, Vector2 end)
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
		/// <summary>
		/// 检查点是否在直线上
		/// </summary>
		/// <returns><c>true</c>, if point in line was checked, <c>false</c> otherwise.</returns>
		/// <param name="p1">P1.</param>
		/// <param name="p2">P2.</param>
		/// <param name="point">Point.</param>
		/// <param name="offset">Offset.</param>
		protected bool CheckPointInLine(Vector2 p1,Vector2 p2,Vector2 point,float offset){
			float d1 = Vector2.Distance (p1, point);
			float d2 = Vector2.Distance (p2, point);
			float d3 = Vector2.Distance (p1, p2) + offset;
			if (d1 + d2 < d3) {
				return true;
			}
			return false;
		}
		/// <summary>
		/// 添加一个节点，
		/// 如果不想刷新整个列表的话
		/// </summary>
		/// <returns>The node.</returns>
		/// <param name="node">Node.</param>
		protected Node AddNode (Node node){
			_nodes.Add(node);
			return node;
		}
		#endregion
	}
}