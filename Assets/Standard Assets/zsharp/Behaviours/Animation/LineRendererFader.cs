﻿using UnityEngine;
using System.Collections;

public class LineRendererFader : MonoBehaviour {
	public Color startColor;
	public Color endColor;
	
	public float time;
	public float timeout;
	
	private LineRenderer lineRenderer;
	
	
	void Start() {
		lineRenderer = GetComponent<LineRenderer>();
		if (lineRenderer == null) { Destroy(this); }
	}
	
	void Update() {
		timeout += Time.deltaTime;
		if (timeout >= time) { Destroy(gameObject); }
		
		float alpha = 1 - (timeout/time);
		Color c1 = startColor;
		Color c2 = endColor;
		
		c1.a = alpha;
		c2.a = alpha;
		
		lineRenderer.SetColors(c1, c2);
		
		
		
	}
	
	
	
}
