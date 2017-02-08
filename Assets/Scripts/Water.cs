/**
 * Given the information on tile's behavior (see Tile.cs), this script modifies the local scale and
 * position of the GameObject representing the water in the game.
 * 
 * Author : Martin Genet
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour {
	private Renderer _rend;
	private Tile _tile;

	// Whether the block is part of a simple flow or of a waterfall
	public enum WaterState{
		Flowing,
		Falling
	}
	// Default state is falling
	private WaterState _waterState = WaterState.Falling;
	public WaterState CurrentWaterState {
		get { return _waterState; }
		set { _waterState = value; }
	}

	// START & UPDATE
	// ------------------------------------------------------------------------------------------------------------

	// Use this for initialization
	void Start () {
		// Set the color to water blue
		_rend = GetComponent<Renderer>();
		_rend.material = new Material(Shader.Find("Transparent/Diffuse"));
		_rend.material.color = new Color (0, 0, 1, 0.6f);
		// Get the reference to the parent tile
		_tile = transform.parent.GetComponent<Tile>();
	}

	// Update is called once per frame
	void Update () {
		Tile left, right, up;
		up = Grid.Get (_tile.x, _tile.y + 1);
		left = Grid.Get (_tile.x - 1, _tile.y);
		right = Grid.Get (_tile.x + 1, _tile.y);

		// If the water is between two bricks (in a tube)
		if (left != null && right != null &&
			left.CurrentState == Tile.TileState.Brick && right.CurrentState == Tile.TileState.Brick){
			// If the level on top is higher or equal, water is falling
			if (_tile.CurrentWaterLevel <= up.CurrentWaterLevel) {
				// then update current state to falling
				if (CurrentWaterState != WaterState.Falling) {
					CurrentWaterState = WaterState.Falling;
				}
				// else, update to flowing (water is flowing down the tube)
			} else {
				if (CurrentWaterState != WaterState.Flowing) {
					CurrentWaterState = WaterState.Flowing;
				}
			}
		}
		// If there's no water left and right,
		else if ((left == null || left.CurrentWaterLevel == 0.0f) &&
		         (right == null || right.CurrentWaterLevel == 0.0f)) {
			// then update current state to falling
			if (CurrentWaterState != WaterState.Falling) {
				CurrentWaterState = WaterState.Falling;
			}
		}
		// Else, water is flowing
		else {
			if (CurrentWaterState != WaterState.Flowing) {
				CurrentWaterState = WaterState.Flowing;
			}
		}
	}

	// GAMEOBJECT MODIFICATION FUNCTION
	// ------------------------------------------------------------------------------------------------------------

	/**
	 * Change the level (local scale & position) of the water block, depending of the
	 * water state. This function is called by Tile.cs.
	 **/
	public void ChangeLevel(float level){
		// Here's a desperate attempt to have reallistic waterfalls
		switch(CurrentWaterState) {
		case(WaterState.Falling):
			// if it's a waterfall, scale it horizontally and not vertically
			transform.localScale = new Vector3 (
				level,
				1.0f,
				transform.localScale.z);
			// Re-initialize the local position
			transform.localPosition = new Vector3 (
				transform.localPosition.x,
				0.0f,
				transform.localPosition.z);
			break;

		case(WaterState.Flowing):
			// If the water is simply flowing, update its water level
			transform.localScale = new Vector3 (
				1.0f,
				level,
				transform.localScale.z);
			// Replace the water block to fit with the bottom of the tile
			transform.localPosition = new Vector3 (
				transform.localPosition.x,
				(level / 2) - 0.5f,
				transform.localPosition.z);
			break;
		}
	}
}
