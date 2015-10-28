using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Object = UnityEngine.Object; // Not refer to C# Object again.

public class CreateAssetBundle{
	static string AssetBundlePath = "Assets" + Path.DirectorySeparatorChar + "AssetBundles" + Path.DirectorySeparatorChar;

	[MenuItem("Character Generator/Create AssetBundles")] // restart the Editor can see it
	static void Execute (){
		bool createBundle = false;
		//????
		foreach (var obj in Selection.GetFiltered (typeof(Object), SelectionMode.DeepAssets)) {
			if (!obj is GameObject) {
				continue;
			}
			if (obj.name.Contains ("@")) {
				continue;
			}
			if (!AssetDatabase.GetAssetPath (obj).Contains ("/characters/")) {
				continue;
			}
			Debug.Log (AssetDatabase.GetAssetPath (obj));
			createBundle = true;
		}

		if (!createBundle) {
			//?????
			EditorUtility.DisplayDialog ("Character Generator",
				"Can not generate AssetBundle. Please select \"Character\" folder or its subfolder in \"Project\" view to generate bundle", "OK");
			return;
		}
	}
}
