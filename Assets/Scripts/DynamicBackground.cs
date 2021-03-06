﻿using UnityEngine;
using System.Collections;

public class DynamicBackground : MonoBehaviour {
	private Renderer bg;
	private GameObject player;
	private GameObject ball;
	private float acc;
	private float dir;

	// Use this for initialization
	void Start () {
		acc = 0.0f;
		dir = 1.0f;
		bg = GetComponent<Renderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		float wubDelay = GameManager.instance.GetWubDelay();
		float wubBeat = GameManager.instance.GetWubFrequency();
		bg.material.SetFloat ("_WubBeat", wubBeat );
		acc += dir * Time.deltaTime;
		if (wubDelay <= 0) {
			bg.material.SetFloat ("_WubTime", acc);
			if (dir > 0 && acc > wubBeat)
				dir = -1;
			if (dir < 0 && acc < 0)
				dir = 1;
		} else if ( acc >= wubDelay ) {
			wubDelay = 0;
			acc = 0;
		}
		if ( player == null ) player = GameObject.FindGameObjectWithTag( "Player" );
		if ( ball == null ) ball = GameObject.FindGameObjectWithTag( "Ball" );
		if ( player != null ) bg.material.SetVector("_PlayerPosition", Camera.main.WorldToScreenPoint( player.transform.position ) );
		if ( ball != null ) bg.material.SetVector("_BallPosition", Camera.main.WorldToScreenPoint( ball.transform.position ) );
	}
}
