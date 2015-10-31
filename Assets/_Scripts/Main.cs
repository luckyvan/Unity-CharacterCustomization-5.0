using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {
	CharacterGenerator generator;
	GameObject character;
	bool usingLatestConfig;
	bool newCharacterRequested = true;
	bool firstCharacter = true;
	string nonLoopingAnimationToPlay;
	const int typeWidth = 80;
	const int buttonWideth = 20;
	const string prefName = "Character Customization Pref";
	// Use this for initialization
	IEnumerator Start () {
		while (!CharacterGenerator.ReadyToUse) {
			yield return 0;
		}
		if (PlayerPrefs.HasKey(prefName)) {
			generator = CharacterGenerator.CreateWithConfig (PlayerPrefs.GetString (prefName));
		}else{
			generator = CharacterGenerator.CreateWithRandomConfig ("Female");
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (generator == null) {
			return;
		}
		if (usingLatestConfig) {
			return;
		}
		if (!generator.ConfigReady) {
			return;
		}
		usingLatestConfig = true;
		if (newCharacterRequested) {
			Destroy (character);
			character = generator.Generate ();
			Animation anim = character.GetComponent<Animation>();
			anim.Play ("idle1");
			anim["idle1"].wrapMode = WrapMode.Loop;
			newCharacterRequested = false;
			if (!firstCharacter) {
				return;
			}
			firstCharacter = false;
			if (anim["walkin"] == null) {
				return;
			}
			anim["walkin"].layer = 1;
			anim["walkin"].weight = 1;
			anim.CrossFade ("walkin", .8f);
			character.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
		}else{
			character = generator.Generate (character);
			if (nonLoopingAnimationToPlay == null) {
				return;
			}
			Animation anim = character.GetComponent<Animation>();
			anim[nonLoopingAnimationToPlay].layer = 1;
			anim[nonLoopingAnimationToPlay].weight = 1;
			anim.CrossFade (nonLoopingAnimationToPlay, .8f);
			nonLoopingAnimationToPlay = null;
		}
	}

	void ChangeCharacter (bool next){
		generator.ChangeCharacter (next);
		usingLatestConfig = false;
		newCharacterRequested = true;

	}

	void ChangeElement (string category, bool next, string anim){
		generator.ChangeElement (category,next);
		usingLatestConfig = false;
		if (!character.GetComponent<Animation>().IsPlaying (anim)) {
			nonLoopingAnimationToPlay = anim;
		}
	}

	void AddCategory(string category, string displayName, string anim){
		GUILayout.BeginHorizontal ();
		if (GUILayout.Button ("<", GUILayout.Width (buttonWideth))) {
			ChangeElement (category, false, anim);
		}
		GUILayout.Box (displayName, GUILayout.Width (typeWidth));
		if (GUILayout.Button (">", GUILayout.Width (buttonWideth))){
			ChangeElement (category, true, anim);
		}
		GUILayout.EndHorizontal ();
	}

	void OnGUI(){
		if (generator == null) {
			return;
		}
		GUI.enabled = usingLatestConfig&&!character.GetComponent<Animation>().IsPlaying ("walkin");
		GUILayout.BeginArea (new Rect(10, 10, typeWidth + 2*buttonWideth + 8, 500));
		// change character
		GUILayout.BeginHorizontal ();
		if (GUILayout.Button ("<", GUILayout.Width (buttonWideth))) {
			ChangeCharacter (false);
		}
		GUILayout.Box ("Character", GUILayout.Width (typeWidth));
		if (GUILayout.Button (">", GUILayout.Width (buttonWideth))){
			ChangeCharacter (true);
		}
		GUILayout.EndHorizontal ();

		// category
		AddCategory ("face", "face", null);	
		AddCategory ("hair", "hair", null);	
		AddCategory ("eyes", "eyes", null);	
		AddCategory ("top", "top", null);	
		AddCategory ("pants", "pants", null);	
		AddCategory ("shoes", "shoes", null);	
		GUILayout.EndArea ();
	}
}
