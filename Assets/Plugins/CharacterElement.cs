﻿using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class CharacterElement{

	public static string AssetPostfix = ".asset";
	public static string BundlePostfix = ".assetbundle";
	public static string DataBaseName = "CharacterElementDatabase";

	public static string PartAssetGameObjectName = "rendererobject";
	public static string PartAssetBonenames = "bonenames";
	public static string BundleFileName(string name){
		return name + BundlePostfix;
	}
	
	public static string AssetFileName(string name){
		return name + AssetPostfix;
	}

	public static string AssetBundleBaseURL {
		get{
			return "file://" + Application.dataPath + "/assetbundles/";
		}
	}

	public static string AssetBundleURL(string name){
		return AssetBundleBaseURL + BundleFileName(name);
	}

	public bool IsLoaded {
		get{
			if (!WWW.isDone) {
				return false;
			}
			if (gameObjectRequest == null) {
				gameObjectRequest = WWW.assetBundle.LoadAssetAsync (PartAssetGameObjectName, typeof(GameObject));
			}

			if (materialRequest == null) {
				materialRequest = WWW.assetBundle.LoadAssetAsync (name, typeof(Material));
			}

			if (boneNamesRequest == null) {
				boneNamesRequest = WWW.assetBundle.LoadAssetAsync (PartAssetBonenames, typeof(StringHolder));
			}

			return gameObjectRequest.isDone && materialRequest.isDone && boneNamesRequest.isDone;
		}
	}

	public WWW WWW{
		get{
			if (!wwws.ContainsKey (bundleName)) {
				wwws.Add(bundleName, new WWW(AssetBundleBaseURL + bundleName + ".assetbundle"));
			}
			return wwws[bundleName];
		}
	}

	public string name;
	public string bundleName;

	AssetBundleRequest gameObjectRequest;
	AssetBundleRequest materialRequest;
	AssetBundleRequest boneNamesRequest;

	static Dictionary<string, WWW> wwws = new Dictionary<string, WWW>();
	public CharacterElement(string name, string bundleName){
		this.name = name;
		this.bundleName = bundleName;
	}

	public SkinnedMeshRenderer GetSkinnedMeshRenderer ()
	{
		GameObject obj = (GameObject)UnityEngine.Object.Instantiate (gameObjectRequest.asset);
		obj.GetComponent<Renderer>().material = (Material)materialRequest.asset;
		return (SkinnedMeshRenderer)obj.GetComponent<Renderer>();
	}

	public string[] GetBoneNames ()
	{
		return ((StringHolder)boneNamesRequest.asset).content;
	}
}
