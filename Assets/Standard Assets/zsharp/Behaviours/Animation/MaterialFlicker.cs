using UnityEngine;
using System.Collections;

public class MaterialFlicker : MonoBehaviour {
	public string targetChannel = "_Emission";
	public Color baseColor = Color.white;
	public float randAdd = 0.3f;
	public bool squared = false;
	
	public float flickerTime = 0.2f;
	public float time = 0.0f;
	
	void Start() {
		baseColor = renderer.material.GetColor(targetChannel);
		
	}
	
	void Update() {
		time += Time.deltaTime;
		if (time > flickerTime) {
			Flicker();
			time -= flickerTime;
		}
		
	}
	
	public void Flicker() {
		float rnd = 1 + randAdd * Random.value;
		if (squared) { rnd *= 1 + randAdd * Random.value; }
		Color c = baseColor;
		c *= rnd;
		c.a = baseColor.a;
		renderer.material.SetColor(targetChannel, c);
	}
	
	public void ResetColor() {
		renderer.material.SetColor(targetChannel, baseColor);
	}
	
}

