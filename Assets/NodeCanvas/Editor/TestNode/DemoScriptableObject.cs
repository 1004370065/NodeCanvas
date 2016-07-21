using UnityEngine;
using UnityEditor;
using System.Collections;
using NodeCanvas;
public class DemoScriptableObject : NodeScriptableObject<DemoNode,DemoTransition,DemoClipData> {
	[MenuItem("Assets/Node/Demo/Create NodeClip")]
	public static void Execute()
	{
		NodeScriptableFactor.Execute<DemoScriptableObject> ();
	}

}
public class DemoNode:Node{
	
}
public class DemoTransition:NodeTransition{
	
}
public class DemoClipData:NodeClip<DemoNode,DemoTransition>{
	
}
