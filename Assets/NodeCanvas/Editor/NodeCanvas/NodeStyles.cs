using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace NodeCanvas{
public static  class NodeStyles {
		public const float StateWidth = 150f;
		public const float StateHeight = 30f;
		public const float StateMachineWidth = 150f;
		public const float StateMachineHeight = 45f;

		public static GUIStyle canvasBackground;
		public static GUIStyle selectionRect;
		public static GUIStyle elementBackground;
		public static GUIStyle breadcrumbLeft;
		public static GUIStyle breadcrumbMiddle;
		public static GUIStyle wrappedLabel;
		public static GUIStyle wrappedLabelLeft;
		public static GUIStyle variableHeader;
		public static GUIStyle label;
		public static GUIStyle centeredLabel;
		public static GUIStyle inspectorTitle;
		public static GUIStyle inspectorTitleText;
		public static GUIStyle stateLabelGizmo;
		public static GUIStyle instructionLabel;
		public static GUIStyle shortcutLabel;
		public static GUIStyle browserPopup;
		public static GUIStyle toggleTrigger;
		public static GUIStyle dele;
		public static GUIStyle ControlHighlight;
		

		public static Texture2D popupIcon; 
		public static Texture2D helpIcon;
		public static Texture2D errorIcon;
		public static Texture2D warnIcon;
		public static Texture2D infoIcon;
		public static Texture2D toolbarPlus;
		public static Texture2D toolbarMinus;
		public static Texture2D iCodeLogo;
		public static Texture2D connectionTexture;

		public static Color gridMinorColor;
		public static Color gridMajorColor;

		public static int fsmColor;
		public static int startNodeColor;
		public static int anyStateColor;
		public static int defaultNodeColor;


		static NodeStyles(){
			GUISkin skin = new GUISkin ();
			NodeStyles.nodeStyleCache = new Dictionary<string, GUIStyle> ();
			NodeStyles.gridMinorColor = EditorGUIUtility.isProSkin? new Color(0f, 0f, 0f, 0.18f):new Color(0f, 0f, 0f, 0.1f);
			NodeStyles.gridMajorColor = EditorGUIUtility.isProSkin? new Color(0f, 0f, 0f, 0.28f):new Color(0f, 0f, 0f, 0.15f);

			NodeStyles.popupIcon = EditorGUIUtility.FindTexture ("_popup");
			NodeStyles.helpIcon = EditorGUIUtility.FindTexture ("_help");
			NodeStyles.errorIcon = EditorGUIUtility.FindTexture ("d_console.erroricon.sml");
			NodeStyles.warnIcon = EditorGUIUtility.FindTexture ("console.warnicon");
			NodeStyles.infoIcon = EditorGUIUtility.FindTexture ("console.infoicon");
			NodeStyles.toolbarPlus = EditorGUIUtility.FindTexture ("Toolbar Plus");
			NodeStyles.toolbarMinus = EditorGUIUtility.FindTexture ("Toolbar Minus");

			NodeStyles.ControlHighlight = (GUIStyle)"U2D.createRect";
			NodeStyles.canvasBackground = "flow background";
			NodeStyles.selectionRect = "SelectionRect";
			NodeStyles.elementBackground = new GUIStyle ("PopupCurveSwatchBackground"){
				padding=new RectOffset()
			};
			NodeStyles.breadcrumbLeft = "GUIEditor.BreadcrumbLeft";
			NodeStyles.breadcrumbMiddle = "GUIEditor.BreadcrumbMid";
			NodeStyles.toggleTrigger = (GUIStyle)"Radio";
			NodeStyles.dele = (GUIStyle)"OL Minus";

			NodeStyles.wrappedLabel = new GUIStyle ("label"){
				fixedHeight=0,
				wordWrap=true
			};
			NodeStyles.wrappedLabelLeft = new GUIStyle ("label"){
				fixedHeight=0,
				wordWrap=true,
				alignment= TextAnchor.UpperLeft
			};
			NodeStyles.variableHeader = "flow overlay header lower left";
			NodeStyles.label="label";
			NodeStyles.inspectorTitle="IN Title";
			NodeStyles.inspectorTitleText = "IN TitleText";
			NodeStyles.iCodeLogo =  (Texture2D)Resources.Load( "ICodeLogo");
			NodeStyles.stateLabelGizmo=new GUIStyle("HelpBox"){

				alignment = TextAnchor.UpperCenter,
				fontSize=21
			};
			NodeStyles.centeredLabel=new GUIStyle("Label"){
				alignment = TextAnchor.UpperCenter,
			};
			NodeStyles.instructionLabel = new GUIStyle ("TL Selection H2"){
				padding = new RectOffset (3, 3, 3, 3),
				contentOffset=NodeStyles.wrappedLabel.contentOffset,
				alignment = TextAnchor.UpperLeft,
				fixedHeight=0,
				wordWrap=true
			};
			NodeStyles.shortcutLabel=new GUIStyle("ObjectPickerLargeStatus"){
				padding = new RectOffset (3, 3, 3, 3),
				alignment = TextAnchor.UpperLeft
			};
			NodeStyles.browserPopup = new GUIStyle ("label"){
				contentOffset= new Vector2(0,2)
			};

			NodeStyles.fsmColor = (int)NodeColor.Blue;
			NodeStyles.startNodeColor = (int)NodeColor.Orange;
			NodeStyles.anyStateColor = (int)NodeColor.Aqua;
			NodeStyles.defaultNodeColor = (int)NodeColor.Grey;
		}

		private static Dictionary<string, GUIStyle> nodeStyleCache;

		private static string[] styleCache =
		{
			"flow node 0",
			"flow node 1",
			"flow node 2",
			"flow node 3",
			"flow node 4",
			"flow node 5",
			"flow node 6"
		};

		private static string[] styleCacheHex =
		{
			"flow node hex 0",
			"flow node hex 1",
			"flow node hex 2",
			"flow node hex 3",
			"flow node hex 4",
			"flow node hex 5",
			"flow node hex 6"
		};
		
		public static GUIStyle GetNodeStyle(int color, bool on, bool hex)
		{
			return GetNodeStyle (hex?styleCacheHex[color]:styleCache[color], on,hex?8f:2f);
		}

		public static GUIStyle GetNodeStyle(string styleName, bool on, float offset)
		{
			string str = on?string.Concat(styleName," on"):  styleName;
			if (!NodeStyles.nodeStyleCache.ContainsKey(str))
			{
				GUIStyle style= new GUIStyle(str);
				style.contentOffset=new Vector2(0,style.contentOffset.y - offset);
				if(on){
					style.fontStyle=FontStyle.Bold;
				}
				nodeStyleCache[str] = style;
			}
			return nodeStyleCache[str];
		}
}
public enum NodeColor{
	Grey = 0,
	Blue = 1,
	Aqua = 2,
	Green = 3,
	Yellow = 4,
	Orange = 5,
	Red = 6
}
}