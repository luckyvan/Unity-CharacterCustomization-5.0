using UnityEngine;
using System.Collections;

public class Client : MonoBehaviour {
	IEnumerator Start () {
		string bundlePath = "file://" + Application.dataPath + "/MyData.bundle";
		WWW www = WWW.LoadFromCacheOrDownload (bundlePath, 1);
		yield return www;

		if (!string.IsNullOrEmpty (www.error)) {
			print (www.error);
			yield return null;
		}

		MyData data = www.assetBundle.mainAsset as MyData;
		if (data != null) {
			print (data.content[0]);
			print (data.content[1]);
			print (data.content[2]);
		}
		yield return null;
	}

}
