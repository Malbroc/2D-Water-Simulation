/**
 * This is just a very simple menu interface.
 * 
 * Author : Martin Genet
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowTo : MonoBehaviour {
	public GameObject _inGameMenu;
	private bool _display = false;

	// Use this for initialization
	void Start () {
		//By default, the menu is not displayed
		_inGameMenu.SetActive (false);
		Time.timeScale = 1.0f;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.LeftControl) || Input.GetKeyDown (KeyCode.RightControl)) {
			if (_display) {
				Time.timeScale = 1.0f;
			} else {
				Time.timeScale = 0.0f;
			}
			_display = !_display;
			_inGameMenu.SetActive (_display);
		}
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}
	}
}
