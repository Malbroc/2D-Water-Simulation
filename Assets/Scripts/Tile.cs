/**
 * This script manage the behavior of a tile, given its current state and water level.
 * The current state of a tile is changed here (see OnMouseOver and stuff), but the
 * water level is updated by Grid.cs.
 * 
 * Author : Martin Genet
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
	// Tile's coordinates in the grid (see Grid.cs)
	private int _x, _y;
	// Properties to make those coordinates public
	public int x {
		get { return _x; }
		set { _x = value; }
	}
	public int y {
		get { return _y; }
		set { _y = value; }
	}

	// Whether the tile is occupated or not
	// Note : a block partially or fully filled with water is considered as empty,
	// because water doesn't really exist. I am still using an enum instead of a simple
	// bool because it's easier to modify if needed.
	public enum TileState {
		Empty,
		Brick
	}

	private Renderer _rend;
	// Default colors to be used
	private Color _none = new Color (1, 1, 1, 0.0f);
	private Color _trsp = new Color (1, 0, 0, 0.5f);
	private Color _brck = new Color (0, 0, 0, 1.0f);

	// Properties (accessed by Grid)
	// - Current state (see TileState)
	private TileState _currentState = TileState.Empty;
	public TileState CurrentState {
		get { return _currentState; }
		set { _currentState = value; }
	}
	// - Current water level (fraction of the tile that is filled with water)
	private float _currentWaterLevel = 0.0f;
	private bool _modified = false;
	public float CurrentWaterLevel {
		get { return _currentWaterLevel; }
		set { 
			_currentWaterLevel = value;
			_modified = true;
		}
	}

	// Water object associated to the tile
	public Water _water;

	// START & UPDATE
	// ------------------------------------------------------------------------------------------------------------

	// Use this for initialization
	void Start () {
		// Set the tile transparent
		_rend = GetComponent<Renderer>();
		_rend.material = new Material(Shader.Find("Transparent/Diffuse"));
		_rend.material.color = _none;
		// Get the water's reference
		_water = transform.GetChild(0).GetComponent<Water>();
	}
	
	// Update is called once per frame
	void Update () {
		// To avoid eventual issues with incorrect water level :
		if (CurrentWaterLevel < 0.0f) {
			CurrentWaterLevel = 0.0f;
		}
		if (CurrentWaterLevel > 1.0f) {
			CurrentWaterLevel = 1.0f;
		}
		// Update the water level
		if (_modified) {
			_water.ChangeLevel (CurrentWaterLevel);
			_modified = false;
		}
	}

	// MOUSE INTERACTION FUNCTIONS
	// ------------------------------------------------------------------------------------------------------------

	// When the mouse cursor is pointing the tile :
	void OnMouseEnter() {
		// Make the tile appear, slightly transparent
		_rend.material.color = _trsp;
		//Debug.Log (x);
		//Debug.Log (y);
	}

	// While the cursor is over the tile, it is possible to place a block
	void OnMouseOver() {
		switch (CurrentState) {
		// If the block is empty,
		case (TileState.Empty):
			if (Input.GetKey (KeyCode.Mouse1)) {
				// On right click, turn the tile to a brick
				_rend.material.color = _brck;
				CurrentWaterLevel = 0.0f;
				CurrentState = TileState.Brick;
			} else {
				if (Input.GetKey(KeyCode.Mouse0)) {
					// On left click, turn the tile to a full water block
					CurrentWaterLevel = 1.0f;
				}
			}
			break;
		// If there is a brick
		case (TileState.Brick):
			if (Input.GetKey (KeyCode.Mouse2)) {
				// On middle click, free the tile
				_rend.material.color = _none;
				CurrentState = TileState.Empty;
			}
			break;
		}
	}

	// When the mouse cursor is not pointing the tile anymore
	void OnMouseExit() {
		switch (CurrentState) {
		// Give the tile back its color
		case (TileState.Empty):
			_rend.material.color = _none;
			break;
		case (TileState.Brick):
			_rend.material.color = _brck;
			break;
		}
	}
}
