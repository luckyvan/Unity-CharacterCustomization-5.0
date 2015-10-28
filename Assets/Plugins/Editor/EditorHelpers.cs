using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Object = UnityEngine.Object;
using System;

public class EditorHelpers {
	public static List<T> CollectAll<T>(string path) where T : Object 
	{
		List<T> list = new List<T>();
		string[] files = Directory.GetFiles (path);
		foreach (var file in files) {
			if (file.Contains (".meta")) { //ignore meta file
				continue; 
			}
			T asset = (T)AssetDatabase.LoadAssetAtPath(file, typeof(T));
			if (asset == null) {
				throw new Exception("Asset does not belong to " + typeof(T) + ":" + file);
			}
			list.Add (asset);
		}
		return list;
	}
}
