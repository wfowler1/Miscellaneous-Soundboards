using UnityEngine;
using System.Collections.Generic;

public class Mortal : MonoBehaviour {
	public bool dead = false;
	public List<Health> healths;
	bool broadcastDeath = false;
	public float invincible = .4f;
	
	public float total {
		get {
			float t = 0;
			foreach (Health h in healths) { t += h.value; }
			return t;
		}
	}
	
	public float capacity {
		get {
			float t = 0;
			foreach (Health h in healths) { t += h.max; }	
			return t;
		}
	}
	
	public float percentage { get { return total / capacity; } }
	
	
	//Constructor
	public Mortal() {
		dead = false;
		healths = new List<Health>();
		healths.Add(new Health());
	}
	
	public Health Get(string n) {
		foreach(Health h in healths) {
			if (h.name == n) { return h; }
		}
		return null;
	}
	
	public void Fill() {
		foreach (Health h in healths) { h.value = h.max; }
		dead = false;
	}
	
	
	public void Update() { Update(Time.deltaTime); }
	public void Update(float time) {
		if (invincible > 0) { invincible -= Time.deltaTime; }
		
		foreach (Health h in healths) { h.Update(time); }
		if (total < .01) { dead = true; }
		
		if (dead && !broadcastDeath) { 
			SayDie();
		} else if (!dead) { 
			//broadcastDeath = false; 
		
		}
		
	}
	
	public void Rez() {
		dead = false;
		broadcastDeath = false;
		foreach (Health h in healths) { h.Fill(); }
	}
	
	
	public float Hit(Attack a, AudioClip playIfKilled = null) { 
		if (invincible > 0) { return 0; }
		float remain = 0;
		
		foreach (string s in a.Keys) {
			if (dead) { return a[s]; }
			remain = Hit(s, a[s], playIfKilled);
			if (dead) {
				return remain; 
			}
		}
		return remain;
	}
	
	public void SayDie(AudioClip playIfKilled = null) {
		//Debug.Log(name + " is dead");
		broadcastDeath = true;
		transform.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
		if(playIfKilled != null) {
			SoundMaster.Play(playIfKilled, transform.position);
		}
		
	}
	
	public float Hit(float d, AudioClip playIfKilled = null) { return Hit("", d, playIfKilled); }
	public float Hit(string s, float d, AudioClip playIfKilled = null) {
		if (invincible > 0) { return 0; }
		if (dead) { return d; }
		Health h = FindHighestLayer();
		Health h2 = FindSecondHighestLayer();
		
		float remain = d;
		if (total < .01) { return d; }
		
		for(int i = 0; remain > 0 && !dead && i < 5; i++) {//Safety wall for 5 layers
			remain = h.Hit(s, d);
			if (h.armor > 0 && !h.protective) {
				remain = h2.Hit(s, remain);
			}
			
			h = FindHighestLayer();
			h2 = FindSecondHighestLayer();
			
			if (dead) { 
				if (!broadcastDeath) {
					SayDie(playIfKilled); 
				}
				return remain; 
			}
			
		}
		
		return remain;
	}
	
	public Health FindHighestLayer() {
		for (int i = healths.Count-1; i >= 0; i--) {
			if (healths[i].value > .01) {
				//Debug.Log(healths[i].name + " Is above zero");
				//Debug.Log(healths[i].cur);
				return healths[i];
			} else {
				//Debug.Log(healths[i].name + " Has been drained.");
			}
		}
		//Debug.Log("all healths have been drained");
		
		dead = true;
		return null;
	}
	
	public Health FindSecondHighestLayer() {
		bool foundOne = false;
		for (int i = healths.Count-1; i >= 0; i--) {
			if (healths[i].value > .01) {
				if (foundOne) {
					//Debug.Log("Found Second Highest Layer");
					return healths[i];
				} else {
					//Debug.Log("Found First Highest Layer");
					if (i == 0) { 
						//Debug.Log("But we are already on the base layer");
						return healths[i];
					}
					foundOne = true;
				}
			} else {
				//Debug.Log(healths[i].name + " Has been drained.");
			}
		}
		//Debug.Log("all healths have been drained");
		
		dead = true;
		return null;
	}
	
	
}