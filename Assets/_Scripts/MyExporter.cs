using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MyExporter : MonoBehaviour {
	[MenuItem("Assets/MyExporter")]
	static void MyExec(){
		// Init Scriptable Object
		MyData data = ScriptableObject.CreateInstance <MyData>();
		data.content = new List<Vector3>(){
			new Vector3(0, 1, 2),
			new Vector3(3, 4, 5),
			new Vector3(6, 7, 8)
		};

		// Create Asset by Scriptable Object
		string assetPath = "Assets/MyData.asset";
		AssetDatabase.CreateAsset (data, assetPath);

		// Create bundle by Asset
		Object asset = AssetDatabase.LoadAssetAtPath (assetPath, typeof(MyData));
		string bundlePath = "Assets/MyData.bundle";
		BuildPipeline.BuildAssetBundle (asset, null, bundlePath);

		//Clean
		AssetDatabase.DeleteAsset (assetPath);
	}
}
