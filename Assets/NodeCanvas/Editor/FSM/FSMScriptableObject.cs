using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
namespace NodeCanvas{
	
	public enum FSMParametersType{
		Float,
		Int,
		Bool,
		Trigger,
	}
	[System.Serializable]
	public class FSMScriptableObject : NodeScriptableObject<FSMNode,FSMTransition,NodeClip<FSMNode,FSMTransition>> {
		[MenuItem("Assets/Node/FSM/Create FSMClip")]
		public static void Execute()
		{
			NodeScriptableFactor.Execute<FSMScriptableObject> ();
		}
		public string Name;
		public List<FSMParameters> parameters = new List<FSMParameters>();

		public void AddParamets(FSMParametersType _type){
			parameters.Add (new FSMParameters ("New "+_type.ToString()+" "+parameters.Count,_type));
		}
		public void RemoveParameters(int id){
			parameters.RemoveAt (id);
		}
	}
	[System.Serializable]
	public class FSMParameters{
		public string name;
		public float defaultValue;
		public FSMParametersType parameterType;
		public FSMParameters(string _name,FSMParametersType _parameterType){
			name = _name;
			parameterType = _parameterType;
		}
	}
	[System.Serializable]
	public class FSMNode:Node{
		public FSMNode():base(){}
		public FSMNode(NodeType _type,string _name = "node",string _groupPath = "Base"):base(_type,_name,_groupPath){
			
		}
	}
	[System.Serializable]
	public class FSMTransition:NodeTransition{
		public FSMTransition():base(){
		}
		public FSMTransition(int fromid,int toid):base(fromid,toid){
		}
	}

}
