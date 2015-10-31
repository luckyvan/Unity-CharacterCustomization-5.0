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
}
