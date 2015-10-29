using UnityEngine;
using System.Collections.Generic;

public class CharacterElement{
	public string name;
	public string bundleName;

	static Dictionary<string, WWW> www = new Dictionary<string, WWW>();
	public CharacterElement(string name, string bundleName){
		this.name = name;
		this.bundleName = bundleName;
	}
}
