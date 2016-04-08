﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ClockTime : MonoBehaviour {

	public float timer;

	public string timestring;

	private bool canRunTime = false;
	
	// Update is called once per frame
	void Update () {
		if (canRunTime) {
			updateTimes ();
			updateTextComponent ();
		}
	}

	void updateTimes () {
		timer += Time.deltaTime;
		string minutes = Mathf.Floor(timer / 60).ToString("00");
		string seconds = Mathf.Floor(timer % 60).ToString("00");
		string fraction = Mathf.Floor((timer * 100) % 100).ToString("00");
		timestring = minutes + ":" + seconds + ":" + fraction;

	}

	void updateTextComponent () {
		GetComponent<Text> ().text = timestring;
	}

	public void startTime () {
		canRunTime = true;
	}

	public void stopTime () {
		canRunTime = false;
	}

}
