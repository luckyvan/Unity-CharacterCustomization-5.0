using UnityEngine;
using System.Collections.Generic;

public class CharacterGenerator {
	#region fields
	static WWW database;

	static Dictionary<string, //character name:Female/Male
		Dictionary<string, //part category:face, hair, etc
			List<CharacterElement>>> sortedElements;

	static List<string> availableCharacters = new List<string>();

	static Dictionary<string, //bundle name: female_characterbase.assetbundle
		WWW> characterBaseWWWs = new Dictionary<string, WWW>();

	static Dictionary<string, //bundle name: female_characterbase.assetbundle
			AssetBundleRequest> characterBaseRequests = new Dictionary<string, AssetBundleRequest>();

	string currentCharacter;
	Dictionary<string, CharacterElement> currentConfiguration = new Dictionary<string, CharacterElement>();

	float assetBundlesAlreadyDownloaded;
	#endregion 

	#region public properties
	public bool ConfigReady{
		get{
			if (!CurrentCharacterBase.isDone) {
				return false;
			}
			if (!characterBaseRequests.ContainsKey (currentCharacter)) {
				characterBaseRequests.Add (currentCharacter, CurrentCharacterBase.assetBundle.LoadAssetAsync ("characterbase", typeof(GameObject)));
			}
			if (!characterBaseRequests[currentCharacter].isDone) {
				return false;
			}
			foreach (var element in currentConfiguration.Values) {
				if (!element.IsLoaded) {
					return false;
				}
			}
			return true;
		}
	}

	public WWW CurrentCharacterBase{
		get{
			if (!characterBaseWWWs.ContainsKey (currentCharacter)) {
				characterBaseWWWs.Add (currentCharacter, new WWW(CharacterElement.AssetBundleBaseURL + currentCharacter + "_characterbase.assetbundle"));
			}
			return characterBaseWWWs[currentCharacter];
		}
	}

	public float CurrentConfigProgress{
		get{
			float toDownload = currentConfiguration.Count + 1 - assetBundlesAlreadyDownloaded;
			if (toDownload == 0) {//???float == 0
				return 1;
			}
			float progress = CurrentCharacterBase.progress;
			foreach (var element in currentConfiguration.Values) {
				progress += element.WWW.progress;
			}
			return (progress - assetBundlesAlreadyDownloaded)/toDownload;
 		}
	}
	#endregion
	#region public static properties
	public static bool ReadyToUse{
		get	{
			if (database == null) {
				database = new WWW(CharacterElement.AssetBundleURL(CharacterElement.DataBaseName));
			}
			if (sortedElements != null) {
				return true;
			}
			if (!database.isDone) {
				return false;
			}

			//configure sortedElement, availableCharacters
			CharacterElementHolder holder = database.assetBundle.mainAsset as CharacterElementHolder;
			if (holder == null) {
				return false;
			}

			sortedElements = new Dictionary<string, Dictionary<string, List<CharacterElement>>>();
			foreach (var characterElement in holder.content) {
				//name: female_face-1.assetbundle
				string[] array = characterElement.name.Split ('_');
				string character = array[0]; //female
				string category = array[1].Split ('-')[0].Replace(".assetbundle", ""); // face
				if (!availableCharacters.Contains (character)) {
					availableCharacters.Add (character);
				}
				if (!sortedElements.ContainsKey (character)) {
					sortedElements.Add (character, new Dictionary<string, List<CharacterElement>>());
				}
				if (!sortedElements[character].ContainsKey (category)) {
					sortedElements[character].Add (category, new List<CharacterElement>());
				}
				sortedElements[character][category].Add (characterElement);
			}
			return true;
		}
	}


	#endregion

	#region public static methods
	public static CharacterGenerator CreateWithRandomConfig(string character){
		CharacterGenerator generator = new CharacterGenerator();
		generator.PrepareRandomConfig (character);
		return generator;
	}

	public static CharacterGenerator CreateWithConfig (string config){
		CharacterGenerator generator = new CharacterGenerator();
		generator.PrepareConfig (config);
		return generator;
	}
	#endregion 

	#region public methods
	public GameObject Generate(GameObject root){ // genreate character SkinnedMeshRender from sub elements
		List<CombineInstance> combineInstances = new List<CombineInstance>(); //????
		List<Material> materials = new List<Material>();
		List<Transform> bones = new List<Transform>();

		Transform[] transforms = root.GetComponentsInChildren<Transform>();
		foreach (var element in currentConfiguration.Values) {
			SkinnedMeshRenderer renderer = element.GetSkinnedMeshRenderer();
			//materials
			materials.AddRange (renderer.materials);
			//sharedMesh from all sub elements
			for (int i = 0; i < renderer.sharedMesh.subMeshCount; i++) {
				CombineInstance ci = new CombineInstance();
				ci.mesh = renderer.sharedMesh;
				ci.subMeshIndex = i;
				combineInstances.Add (ci);
			}
			//bones
			foreach (var boneName in element.GetBoneNames()) {
				foreach (var transform in transforms) {
					if (transform.name != boneName) {
						continue;
					}else{
						bones.Add (transform);
						break;
					}
				}
			}
		}
		//configure Skinned Mesh Render of Root
		SkinnedMeshRenderer renderRoot = root.GetComponent<SkinnedMeshRenderer>();
		renderRoot.sharedMesh = new Mesh();
		renderRoot.sharedMesh.CombineMeshes (combineInstances.ToArray(), false, false); //?????
		renderRoot.materials = materials.ToArray ();
		renderRoot.bones = bones.ToArray ();
		return root;
	}

	public GameObject Generate(){ // generate character
		GameObject root = (GameObject) Object.Instantiate (characterBaseRequests[currentCharacter].asset);
		root.name = currentCharacter;
		return Generate (root);
	}

	public void PrepareRandomConfig (string character){//update currentConfiguration
		currentConfiguration.Clear ();

		currentCharacter = character.ToLower ();
		foreach (var category in sortedElements[character]) {
			currentConfiguration.Add (category.Key, category.Value[Random.Range (0, category.Value.Count)]);

			UpdateAssetBundlesAlreadyDownloaded();
		}
	}
	//???? where to change currentCharacter
	public void ChangeCharacter(bool next){
		string character = null;
		for (int i = 0; i < availableCharacters.Count; i++) {
			if (availableCharacters[i] != currentCharacter) {
				continue;
			}
			if (next) {
				character = availableCharacters[(i+1)%availableCharacters.Count];
			}else{
				character = availableCharacters[(i-1)%availableCharacters.Count];
			}
			break;
		}
		PrepareRandomConfig (character);
	}

	public void ChangeElement(string category, bool next){
		List<CharacterElement> availableElements = sortedElements[currentCharacter][category];
		CharacterElement element = null;
		int count = availableElements.Count;
		for (int i = 0; i < count; i++) {
			if (availableElements[i] != currentConfiguration[category]) {
				continue;
			}
			if (next) {
				element = availableElements[(i+1)%count];
			}else{
				element = availableElements[(i-1)%count];
			}
			break;
		}
		currentConfiguration[category] = element;
		UpdateAssetBundlesAlreadyDownloaded ();
	}

	public string GetConfig(){
		string s = currentCharacter;
		foreach (var category in currentConfiguration) {
			s += "|" + category.Key + "|" + category.Value.name;
		}
		return s;
	}

	public void PrepareConfig(string config){
		config = config.ToLower();
		string[] settings = config.Split ('|');
		currentCharacter = settings[0];
		currentConfiguration = new Dictionary<string, CharacterElement>();
		for (int i = 1; i < settings.Length;) {
			string category = settings[i++];
			string elementName = settings[i++];
			CharacterElement element = null;

			foreach (var elem in sortedElements[currentCharacter][category]) {
				if (elem.name != elementName) {
					continue;
				}
				element = elem;
				break;
			}

			if (element == null) {
				throw new System.Exception(" Can not find element:" + elementName);
			}

			currentConfiguration.Add (category, element);
		}
		UpdateAssetBundlesAlreadyDownloaded ();
	}
	#endregion

	#region helpers
	void UpdateAssetBundlesAlreadyDownloaded(){
		//assetBundlesAlreadyDownloaded =
	}

	#endregion
}
