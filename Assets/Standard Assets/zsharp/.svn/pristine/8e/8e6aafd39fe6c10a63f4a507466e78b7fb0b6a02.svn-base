using UnityEngine;
using System.Collections;

public class ActivateBehaviourAfterDelay : DelayedAction {
	public string behaviour;
	
	public override void Action() {
		MonoBehaviour b = GetComponent(behaviour) as MonoBehaviour;
		if (b != null) { b.enabled = true; }
		Destroy(this);
	}
	
}
