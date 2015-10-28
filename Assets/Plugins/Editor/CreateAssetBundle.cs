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
			//deal with model files only

			if (AssetDatabase.GetAssetPath (obj).Contains ("FBX")) {
				Debug.Log (AssetDatabase.GetAssetPath (obj));

				GameObject fbx = (GameObject)obj;
				string name = fbx.name.ToLower ();
				CreateCharacterBaseAssetBundle (fbx, name);
				createBundle = true;				
			}
		}

		if (!createBundle) {
			//?????
			EditorUtility.DisplayDialog ("Character Generator",
				"Can not generate AssetBundle. Please select \"Character\" folder or its subfolder in \"Project\" view to generate bundle", "OK");
			return;
		}
	}

	static void CreateCharacterBaseAssetBundle (GameObject fbx, string name)
	{
		if (!Directory.Exists (AssetBundlePath)) {
			Directory.CreateDirectory (AssetBundlePath);
		}

		//delete bundle with the same name
		string bundleName = name + "_characterbase.assetbundle";
		string[] existingAssetBundles = Directory.GetFiles (AssetBundlePath);
		foreach (var bundle in existingAssetBundles) {
			if (bundle.EndsWith (bundleName)) {
				File.Delete (bundle);
			}
		}

		GameObject clone = (GameObject)Object.Instantiate (fbx);
		foreach (var animation in clone.GetComponentsInChildren<Animation>()) {
			animation.cullingType = AnimationCullingType.AlwaysAnimate;
		}
		foreach (var skinedMeshRender in clone.GetComponentsInChildren<SkinnedMeshRenderer>()) {
			Object.DestroyImmediate(skinedMeshRender.gameObject);
		}
		clone.AddComponent<SkinnedMeshRenderer>();

		//Prefab
		string prefabPath = "Assets/" + name + ".prefab";
		Object tempPrefab = PrefabUtility.CreateEmptyPrefab (prefabPath);
		tempPrefab = PrefabUtility.ReplacePrefab (clone, tempPrefab);
		// clean cloned ojbect, In Editor mode, can use DestroyImmediate only.
		Object.DestroyImmediate (clone);

		BuildPipeline.BuildAssetBundle (tempPrefab, null, AssetBundlePath+bundleName, BuildAssetBundleOptions.CollectDependencies);//, BuildAssetBundleOptions.CollectDependencies is always enabled in 5.0
		AssetDatabase.DeleteAsset (AssetDatabase.GetAssetPath (tempPrefab));//clean temp prefab file, why not use File.delete????s
	}
}
