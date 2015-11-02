using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Object = UnityEngine.Object; // Not refer to C# Object again.

public class CreateAssetBundle{
	static string AssetPath = "Assets" + Path.DirectorySeparatorChar;
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
				CreatePartAssetBundles (fbx, name);
				createBundle = true;				
			}
		}

		if (!createBundle) {
			//?????
			EditorUtility.DisplayDialog ("Character Generator",
				"Can not generate AssetBundle. Please select \"Character\" folder or its subfolder in \"Project\" view to generate bundle", "OK");
			return;
		}

		CreateElementDataBaseBundle();

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
		Object tempPrefab = CreatePrefab (clone, "characterbase");
		// clean cloned ojbect, In Editor mode, can use DestroyImmediate only.
		Object.DestroyImmediate (clone);

		BuildPipeline.BuildAssetBundle (tempPrefab, null, AssetBundlePath+bundleName, BuildAssetBundleOptions.CollectDependencies);//, BuildAssetBundleOptions.CollectDependencies is always enabled in 5.0
		AssetDatabase.DeleteAsset (AssetDatabase.GetAssetPath (tempPrefab));//clean temp prefab file, why not use File.delete????s
	}

	static Object CreatePrefab (GameObject obj, string name){
		string prefabPath = "Assets/" + name + ".prefab";
		Object tempPrefab = PrefabUtility.CreateEmptyPrefab (prefabPath);
		tempPrefab = PrefabUtility.ReplacePrefab (obj, tempPrefab);
		return tempPrefab;
	}

	//CreatePartAssetBundle
	//Example: 
	//    bundle: female_face-1.assetbundle
	//        - a: GameObject
	//            name: PartAssetGameObjectName(renderobject),
	//            type: GameObject(as Prefab)
	//        - a: Materials
	//            name: material name
	//            type: material
	//        - a: Bones
	//            name: PartAssetBonesName(bonesname)
	//            type: StringHolder
	static void CreatePartAssetBundles (GameObject fbx, string name)
	{
		List<Material> materials = EditorHelpers.CollectAll<Material>(MeterialsPath(fbx));

		foreach (var skinnedMeshRender in fbx.GetComponentsInChildren<SkinnedMeshRenderer>(true)) { // get all children with SkinnedMeshRender, even they are disabled
			List<Object> assets = new List<Object>();

			//Prefab for children with skinnedMeshRender
			GameObject rendererClone = (GameObject)PrefabUtility.InstantiatePrefab (skinnedMeshRender.gameObject);
			//delete cloned parent object by side effect
			GameObject rendererCloneParent = rendererClone.transform.parent.gameObject;
			rendererClone.transform.parent = null; 
			Object.DestroyImmediate (rendererCloneParent);

			//create prefab for GameObject
			Object rendererPrefab = CreatePrefab (rendererClone, CharacterElement.PartAssetGameObjectName);
			Object.DestroyImmediate (rendererClone);

			assets.Add (rendererPrefab);

			//Materials
			foreach (var material in materials) {
				if (material.name.Contains (skinnedMeshRender.name.ToLower ())) {
					assets.Add (material);
				}
			}

			//Bones
			List<string> boneNames = new List<string>();
			foreach (var transform in skinnedMeshRender.bones) {//???
				boneNames.Add (transform.name);
			}
			string boneNameAssetPath = AssetPath + CharacterElement.AssetFileName(CharacterElement.PartAssetBonenames);
			StringHolder holder = ScriptableObject.CreateInstance <StringHolder>();
			holder.content = boneNames.ToArray ();
			AssetDatabase.CreateAsset (holder, boneNameAssetPath);
			
			// Create bone asset
			Object boneAsset = AssetDatabase.LoadAssetAtPath (boneNameAssetPath, typeof(StringHolder));
			assets.Add (boneAsset);

			//Create bundl by assets
			string bundlePath = AssetBundlePath + name + "_" + skinnedMeshRender.name.ToLower () + ".assetbundle";
			BuildPipeline.BuildAssetBundle (null, assets.ToArray (), bundlePath);

			//Cleanup
			AssetDatabase.DeleteAsset (AssetDatabase.GetAssetPath (rendererPrefab));
			AssetDatabase.DeleteAsset (boneNameAssetPath);
		}
	}

	static string MeterialsPath (GameObject fbx)
	{
		string root = AssetDatabase.GetAssetPath(fbx);
		return root.Substring (0, root.LastIndexOf ("/") + 1)
				+ "Per Texture Materials";
 	}

	//create map material name -> bundle
	//example: female_face-1 --> female_face-1.assetbundle
	static void CreateElementDataBaseBundle ()
	{
		List<CharacterElement> characterElements = new List<CharacterElement>();

		string[] assetBundles = Directory.GetFiles (AssetBundlePath);
		string[] materials = Directory.GetFiles ("Assets/CharacterCustomization/characters", "*.mat", SearchOption.AllDirectories); //search materials recursively
		foreach (var material in materials) {
			foreach (var bundle in assetBundles) {
				FileInfo bundleFI = new FileInfo(bundle);
				FileInfo materialFI = new FileInfo(material);
				string bundleName = bundleFI.Name.Replace (CharacterElement.BundlePostfix, "");

				if (!materialFI.Name.StartsWith (bundleName) ||
				    !material.Contains ("Per Texture Materials")) {
					continue;
				}

				string name = materialFI.Name.Replace (".mat", "");
				characterElements.Add (new CharacterElement(name, bundleName));
				break;
			}
		}

		CharacterElementHolder holder = ScriptableObject.CreateInstance <CharacterElementHolder>();
		holder.content = characterElements;
		string path = AssetPath + CharacterElement.AssetFileName (CharacterElement.DataBaseName);
		AssetDatabase.CreateAsset (holder, path);
		Object asset = AssetDatabase.LoadAssetAtPath (path, typeof(CharacterElementHolder));
		BuildPipeline.BuildAssetBundle (asset, null, AssetBundlePath + CharacterElement.BundleFileName (CharacterElement.DataBaseName));
	}
}
