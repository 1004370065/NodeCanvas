using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
namespace NodeCanvas{
public class NodeScriptableFactor {
		public static void Execute<T>()where T:ScriptableObject
	{
		T  data = ScriptableObject.CreateInstance<T>();//创建Test的一个实例
		string path =  EditorUtility.SaveFilePanel("打开一个界面",Application.dataPath,"NewNodeAssets","asset");
		path =  "Assets"+ path.Replace (Application.dataPath,"");
		path = Path.GetDirectoryName(path)+"/"+ Path.GetFileNameWithoutExtension (path);
		if (path == "/Assets")
			return;
		AssetDatabase.CreateAsset(data , path+".asset");
		AssetDatabase.Refresh();
	}
}
}